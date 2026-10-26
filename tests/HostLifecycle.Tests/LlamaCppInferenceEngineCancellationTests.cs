using System.Net;
using System.Net.Http;
using System.Text;
using WhiteP.Host.Domain;
using WhiteP.Host.Windows.Inference;
using WhiteP.Host.Windows.Requests;

internal static class LlamaCppInferenceEngineCancellationTests
{
    public static async Task RunAsync()
    {
        await CoordinatorCancellationReachesInferenceEngineAndHttpAsync().ConfigureAwait(false);
        await EngineCancelApiReachesHttpAsync().ConfigureAwait(false);
    }

    private static async Task CoordinatorCancellationReachesInferenceEngineAndHttpAsync()
    {
        var handler = new CancellationProbeHandler();
        await using var engine = CreateEngine(handler);
        await engine.LoadModelAsync("fixture-model").ConfigureAwait(false);

        var coordinator = new HostRequestCoordinator();
        var active = coordinator.ExecuteAsync(ValidAdmission(), cancellationToken =>
            engine.GenerateAsync("safe cancellation test prompt", 16, cancellationToken));

        await handler.GenerationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(coordinator.CancelActiveRequest(), "The coordinator must signal the active request.");
        await AssertCancelledAsync(active, "Coordinator cancellation must cancel inference generation.").ConfigureAwait(false);
        await handler.GenerationCancelled.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!coordinator.IsBusy, "Coordinator must release its active-request gate after network cancellation.");

        Console.WriteLine("PASS: coordinator cancellation flows through LlamaCppInferenceEngine to the active HTTP request.");
    }

    private static async Task EngineCancelApiReachesHttpAsync()
    {
        var handler = new CancellationProbeHandler();
        await using var engine = CreateEngine(handler);
        await engine.LoadModelAsync("fixture-model").ConfigureAwait(false);

        var active = engine.GenerateAsync("safe cancellation test prompt", 16);
        await handler.GenerationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await engine.CancelAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await AssertCancelledAsync(active, "IInferenceEngine.CancelAsync must cancel active generation.").ConfigureAwait(false);
        await handler.GenerationCancelled.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        Console.WriteLine("PASS: IInferenceEngine.CancelAsync cancels active generation and its HTTP request token.");
    }

    private static LlamaCppInferenceEngine CreateEngine(CancellationProbeHandler handler)
    {
        var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        return new LlamaCppInferenceEngine(new Uri("http://127.0.0.1:8080/"), "fixture-model", client);
    }

    private static async Task AssertCancelledAsync(Task<string> active, string message)
    {
        var cancellationObserved = false;
        try
        {
            _ = await active.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancellationObserved = true;
        }

        Assert(cancellationObserved, message);
    }

    private static HostRequestAdmissionContext ValidAdmission() =>
        new(
            true,
            HostState.Ready,
            "fixture-model",
            "fixture-model",
            HostCapacityState.Ready,
            (ulong)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds());

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CancellationProbeHandler : HttpMessageHandler
    {
        public TaskCompletionSource<bool> GenerationStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> GenerationCancelled { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (request.Method == HttpMethod.Get && path.EndsWith("/health", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/v1/models", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"data\":[{\"id\":\"fixture-model\"}]}",
                        Encoding.UTF8,
                        "application/json")
                };
            }

            if (request.Method == HttpMethod.Post && path.EndsWith("/v1/chat/completions", StringComparison.Ordinal))
            {
                GenerationStarted.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    GenerationCancelled.TrySetResult(true);
                    throw;
                }

                throw new InvalidOperationException("The cancellation probe must not complete generation successfully.");
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}

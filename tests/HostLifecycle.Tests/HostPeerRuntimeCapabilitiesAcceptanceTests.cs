using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using WhiteP.Host.Application;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;
using HostStatus = WhiteP.Host.Domain.HostStatus;

internal static class HostPeerRuntimeCapabilitiesAcceptanceTests
{
    public static async Task RunAsync()
    {
        await LlamaCppBackendReportsOnlyItsVerifiedActiveModelAndContextAsync().ConfigureAwait(false);
        await SessionReturnsFreshCapabilitiesOnlyForAReadyModelAsync().ConfigureAwait(false);
        await SessionAdmitsOnlyRequestsWithinTheActiveModelAndProductLimitsAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: peer runtime status and inference admission use only live model identity and limits, bounded by the product and protocol ceilings.");
    }

    private static async Task LlamaCppBackendReportsOnlyItsVerifiedActiveModelAndContextAsync()
    {
        var handler = new ModelPropertiesHandler(
            "{\"data\":[{\"id\":\"fixture-model\"}]}",
            "{\"default_generation_settings\":{\"n_ctx\":4096}}");
        using var http = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var backend = new LlamaCppServerBackend(
            new Uri("http://127.0.0.1:8080/"),
            "fixture-model",
            http,
            TimeSpan.FromSeconds(2));

        InferenceModelCapabilitySnapshot? capability =
            await backend.GetModelCapabilityAsync().ConfigureAwait(false);

        Assert(capability is not null, "The backend should report capabilities when its active model and /props context are valid.");
        Assert(capability!.ModelId == "fixture-model", "The capability must identify the configured model actually returned by /v1/models.");
        Assert(capability.MaxContextTokens == 4096, "The context ceiling must come from /props.default_generation_settings.n_ctx.");
        Assert(capability.MaxOutputTokens == 4096, "The output ceiling must not exceed the reported context window.");
        Assert(!capability.SupportsStreaming, "The current backend adapter does not implement streamed responses.");
        Assert(handler.RequestedPaths.SequenceEqual(new[] { "/v1/models", "/props" }),
            "Capability discovery must read only the model list and read-only properties endpoint.");

        var missingModelHandler = new ModelPropertiesHandler(
            "{\"data\":[{\"id\":\"another-model\"}]}",
            "{\"default_generation_settings\":{\"n_ctx\":4096}}");
        using var missingModelHttp = new HttpClient(missingModelHandler) { Timeout = Timeout.InfiniteTimeSpan };
        using var missingModelBackend = new LlamaCppServerBackend(
            new Uri("http://127.0.0.1:8080/"),
            "fixture-model",
            missingModelHttp,
            TimeSpan.FromSeconds(2));
        Assert(await missingModelBackend.GetModelCapabilityAsync().ConfigureAwait(false) is null,
            "A configured model that is absent from /v1/models must not be advertised.");

        var missingContextHandler = new ModelPropertiesHandler(
            "{\"data\":[{\"id\":\"fixture-model\"}]}",
            "{\"default_generation_settings\":{\"n_ctx\":0}}");
        using var missingContextHttp = new HttpClient(missingContextHandler) { Timeout = Timeout.InfiniteTimeSpan };
        using var missingContextBackend = new LlamaCppServerBackend(
            new Uri("http://127.0.0.1:8080/"),
            "fixture-model",
            missingContextHttp,
            TimeSpan.FromSeconds(2));
        Assert(await missingContextBackend.GetModelCapabilityAsync().ConfigureAwait(false) is null,
            "A missing/zero context window must not be advertised as a usable model capability.");
    }

    private static async Task SessionReturnsFreshCapabilitiesOnlyForAReadyModelAsync()
    {
        var backend = new CapabilityTestBackend(
            new InferenceModelCapabilitySnapshot("fixture-model", "Fixture model", 8192, 4096, false));
        using var session = new HostInferenceSession(
            backendFactory: (_, _) => backend,
            readinessEvaluator: _ => Task.FromResult(new HostReadinessResult(true, "Test readiness passed.")));

        HostStatus connected = await session.ConnectAsync(
            new Uri("http://127.0.0.1:8080/"),
            "fixture-model").ConfigureAwait(false);
        Assert(connected.State == HostState.Ready, "The fixture backend should pass its live health and readiness gate.");

        HostPeerRuntimeSnapshot snapshot =
            await session.GetFreshPeerRuntimeSnapshotAsync().ConfigureAwait(false);
        Assert(snapshot.Status.State == HostState.Ready, "A fresh healthy backend status should remain Ready.");
        Assert(snapshot.ModelCapability is not null, "A READY backend that implements the capability provider should return its model snapshot.");
        Assert(snapshot.ModelCapability!.MaxContextTokens == 8192, "The peer snapshot must carry the provider's verified context limit.");
        Assert(snapshot.ModelCapability.MaxOutputTokens == 4096, "The peer snapshot must preserve the provider's conservative output limit.");
        Assert(backend.CapabilityCalls == 1, "The capability provider must be queried as part of the fresh snapshot.");

        await session.DisconnectAsync().ConfigureAwait(false);
        HostPeerRuntimeSnapshot disconnected =
            await session.GetFreshPeerRuntimeSnapshotAsync().ConfigureAwait(false);
        Assert(disconnected.Status.State == HostState.Error, "A disconnected/unconfigured backend must not become Ready during a fresh probe.");
        Assert(disconnected.ModelCapability is null, "A disconnected backend must never advertise a stale model snapshot.");
    }

    private static async Task SessionAdmitsOnlyRequestsWithinTheActiveModelAndProductLimitsAsync()
    {
        var backend = new CapabilityTestBackend(
            new InferenceModelCapabilitySnapshot("fixture-model", "Fixture model", 8192, 2048, false));
        using var session = new HostInferenceSession(
            backendFactory: (_, _) => backend,
            readinessEvaluator: _ => Task.FromResult(new HostReadinessResult(true, "Test readiness passed.")));

        var connected = await session.ConnectAsync(
            new Uri("http://127.0.0.1:8080/"),
            "fixture-model").ConfigureAwait(false);
        Assert(connected.State == HostState.Ready, "The request admission test backend should become Ready.");

        string validResponse = await session.GenerateAsync(CreateRequest("fixture-model", 2048)).ConfigureAwait(false);
        Assert(validResponse == "capability test response", "A request at the exact live model output ceiling should succeed.");
        Assert(backend.GenerationCalls == 1 && backend.LastMaxOutputTokens == 2048,
            "The backend should receive exactly the admitted model output ceiling.");

        await AssertThrowsAsync<InvalidDataException>(
            () => session.GenerateAsync(CreateRequest("fixture-model", 2049)),
            "A request above the active model output limit must be rejected before inference.");
        await AssertThrowsAsync<InvalidDataException>(
            () => session.GenerateAsync(CreateRequest("another-model", 1024)),
            "A request naming another model must be rejected before inference.");
        await AssertThrowsAsync<InvalidDataException>(
            () => session.GenerateAsync(CreateRequest("fixture-model", 0)),
            "A zero output limit must be rejected before inference.");
        Assert(backend.GenerationCalls == 1,
            "Rejected model identity and output-limit requests must not invoke the backend.");

        var productBackend = new CapabilityTestBackend(
            new InferenceModelCapabilitySnapshot("fixture-model", "Fixture model", 32768, 32768, false));
        using var productSession = new HostInferenceSession(
            backendFactory: (_, _) => productBackend,
            readinessEvaluator: _ => Task.FromResult(new HostReadinessResult(true, "Test readiness passed.")));
        await productSession.ConnectAsync(new Uri("http://127.0.0.1:8080/"), "fixture-model").ConfigureAwait(false);
        await AssertThrowsAsync<InvalidDataException>(
            () => productSession.GenerateAsync(CreateRequest("fixture-model", 4097)),
            "The product output-token ceiling must apply even when the model advertises a larger capability.");
        Assert(productBackend.GenerationCalls == 0,
            "A request beyond the product ceiling must not reach the inference backend.");
    }

    private static InferenceRequest CreateRequest(string modelId, uint maxOutputTokens) =>
        new()
        {
            RequestId = Guid.NewGuid().ToString("N"),
            AttemptId = Guid.NewGuid().ToString("N"),
            RoundId = "round-test",
            ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
            ModelId = modelId,
            Prompt = "capability admission test",
            DeadlineUnixMs = (ulong)DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds(),
            MaxOutputTokens = maxOutputTokens
        };

    private static async Task AssertThrowsAsync<TException>(Func<Task> action, string message)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CapabilityTestBackend(InferenceModelCapabilitySnapshot capability) :
        IInferenceBackend,
        IInferenceModelCapabilityProvider
    {
        public int CapabilityCalls { get; private set; }
        public int GenerationCalls { get; private set; }
        public uint LastMaxOutputTokens { get; private set; }
        public bool IsAvailable => true;
        public string UnavailableReason => string.Empty;

        public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(true);
        }

        public Task<string> GenerateAsync(
            string prompt,
            uint maxOutputTokens,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GenerationCalls++;
            LastMaxOutputTokens = maxOutputTokens;
            return Task.FromResult("capability test response");
        }

        public Task<InferenceModelCapabilitySnapshot?> GetModelCapabilityAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CapabilityCalls++;
            return Task.FromResult<InferenceModelCapabilitySnapshot?>(capability);
        }
    }

    private sealed class ModelPropertiesHandler(string modelsJson, string propertiesJson) : HttpMessageHandler
    {
        public List<string> RequestedPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            RequestedPaths.Add(path);
            string? body = path switch
            {
                "/v1/models" => modelsJson,
                "/props" => propertiesJson,
                _ => null
            };
            return Task.FromResult(body is null
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
        }
    }
}

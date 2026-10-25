using WhiteP.Host.Application;
using WhiteP.Host.Domain;

internal static class HostInferenceSessionConcurrencyTests
{
    public static async Task RunAsync()
    {
        var activeBackend = new BlockingInferenceBackend();
        var connectedModels = new List<string>();
        var session = new HostInferenceSession(
            backend: new ImmediateInferenceBackend(),
            backendFactory: (_, model) =>
            {
                connectedModels.Add(model);
                if (string.Equals(model, "model-a", StringComparison.Ordinal))
                {
                    return activeBackend;
                }

                if (string.Equals(model, "model-b", StringComparison.Ordinal))
                {
                    if (!activeBackend.GenerationCompleted)
                    {
                        throw new InvalidOperationException("The active backend was replaced before inference finished.");
                    }

                    return new ImmediateInferenceBackend();
                }

                throw new InvalidOperationException("Unexpected model requested by the test.");
            },
            readinessEvaluator: _ => Task.FromResult(new HostReadinessResult(true, "Test readiness passed.")));

        Task<string>? generationTask = null;
        try
        {
            var endpoint = new Uri("http://127.0.0.1:8080/");
            var firstConnection = await session.ConnectAsync(endpoint, "model-a");
            Assert(firstConnection.State == HostState.Ready, "The initial backend must connect before inference.");

            generationTask = session.GenerateAsync("blocking test prompt", 32);
            await activeBackend.GenerationStarted.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(session.Status.State == HostState.Busy, "The session must remain Busy while inference is in flight.");

            var modelSwitchTask = session.ConnectAsync(endpoint, "model-b");
            Assert(!modelSwitchTask.IsCompleted, "A model switch must wait until in-flight inference completes.");
            Assert(connectedModels.SequenceEqual(new[] { "model-a" }),
                "The replacement backend must not be constructed while the first model is generating.");

            activeBackend.ReleaseGeneration();
            Assert(await generationTask.WaitAsync(TimeSpan.FromSeconds(5)) == "generation finished",
                "The in-flight inference must complete before the model switch proceeds.");

            var switched = await modelSwitchTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(switched.State == HostState.Ready, "The new model backend must be checked before becoming Ready.");
            Assert(activeBackend.GenerationCompleted, "The old backend must finish inference before replacement.");
            Assert(connectedModels.SequenceEqual(new[] { "model-a", "model-b" }),
                "The replacement backend must be constructed only after the active request finishes.");
        }
        finally
        {
            activeBackend.ReleaseGeneration();
            if (generationTask is not null)
            {
                try { await generationTask.WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (Exception) { /* The test assertion below reports the original failure. */ }
            }

            session.Dispose();
        }

        Console.WriteLine("PASS: reconnect/model switching is serialized until the in-flight inference completes.");
    }

    public static async Task DisconnectReleasesBackendAsync()
    {
        var activeBackend = new DisposableTrackingBackend();
        var session = new HostInferenceSession(backend: new ImmediateInferenceBackend(), backendFactory: (_, _) => activeBackend,
            readinessEvaluator: _ => Task.FromResult(new HostReadinessResult(true, "Test readiness passed.")));
        var connected = await session.ConnectAsync(new Uri("http://127.0.0.1:8080/"), "model-a");
        Assert(connected.State == HostState.Ready, "The test backend must connect before disconnection.");
        var disconnected = await session.DisconnectAsync();
        Assert(disconnected.State == HostState.Stopped, "Disconnect must leave the host stopped.");
        Assert(activeBackend.Disposed, "Disconnect must dispose the active backend resources.");
        Assert(!session.IsConnected, "A disconnected session must not advertise a connected backend.");
        session.Dispose();
        Console.WriteLine("PASS: disconnect disposes backend resources and leaves the host stopped.");
    }

    private sealed class DisposableTrackingBackend : IInferenceBackend, IDisposable
    {
        public bool IsAvailable => true;
        public string UnavailableReason => string.Empty;
        public bool Disposed { get; private set; }
        public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) => Task.FromResult("test output");
        public void Dispose() => Disposed = true;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class BlockingInferenceBackend : IInferenceBackend, IDisposable
    {
        private readonly TaskCompletionSource<bool> _generationStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _releaseGeneration =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsAvailable => true;
        public string UnavailableReason => string.Empty;
        public bool GenerationCompleted { get; private set; }
        public Task GenerationStarted => _generationStarted.Task;

        public async Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default)
        {
            _generationStarted.TrySetResult(true);
            try
            {
                await _releaseGeneration.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return "generation finished";
            }
            finally
            {
                GenerationCompleted = true;
            }
        }

        public void ReleaseGeneration() => _releaseGeneration.TrySetResult(true);
        public void Dispose() { }
    }

    private sealed class ImmediateInferenceBackend : IInferenceBackend, IDisposable
    {
        public bool IsAvailable => true;
        public string UnavailableReason => string.Empty;
        public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
            Task.FromResult("immediate test response");
        public void Dispose() { }
    }
}

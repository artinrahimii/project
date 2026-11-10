using System.IO;
using WhiteP.Host.Domain;
using HostStatus = WhiteP.Host.Domain.HostStatus;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Application;

public sealed class HostInferenceSession : IDisposable, IAsyncDisposable
{
    private readonly HostLifecycle _lifecycle;
    private IInferenceBackend _backend;
    private readonly Func<Uri, string, IInferenceBackend>? _backendFactory;
    private readonly Func<CancellationToken, Task<HostReadinessResult>>? _readinessEvaluator;
    // Serializes inference with connect/reconnect so a backend cannot be replaced mid-request.
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly CancellationTokenSource _shutdownCancellation = new();
    private readonly CancellationToken _shutdownToken;
    private readonly object _disposeSync = new();
    private Task? _disposeTask;
    private int _shutdownStarted;
    private bool _disposed;

    public HostInferenceSession(
        HostLifecycle? lifecycle = null,
        IInferenceBackend? backend = null,
        Func<Uri, string, IInferenceBackend>? backendFactory = null,
        Func<CancellationToken, Task<HostReadinessResult>>? readinessEvaluator = null)
    {
        _lifecycle = lifecycle ?? new HostLifecycle();
        _backend = backend ?? new UnconfiguredInferenceBackend();
        _backendFactory = backendFactory;
        _readinessEvaluator = readinessEvaluator;
        _shutdownToken = _shutdownCancellation.Token;
    }

    public HostStatus Status => _lifecycle.Status;

    public bool IsConnected => !_disposed && Volatile.Read(ref _shutdownStarted) == 0 && _lifecycle.Status.State == HostState.Ready;

    private void ThrowIfShutdownStarted()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _shutdownStarted) != 0 || _disposed, this);
    }

    private CancellationTokenSource CreateOperationCancellationSource(CancellationToken callerToken)
    {
        ThrowIfShutdownStarted();
        return CancellationTokenSource.CreateLinkedTokenSource(callerToken, _shutdownToken);
    }

    /// <summary>
    /// Returns a freshly checked local backend/readiness state for an authenticated peer status
    /// request. Busy or concurrently transitioning sessions are reported conservatively without
    /// waiting behind an active inference operation.
    /// </summary>
    public async Task<HostStatus> GetFreshStatusAsync(CancellationToken cancellationToken = default)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();

        HostStatus current = _lifecycle.Status;
        if (current.State == HostState.Busy)
        {
            operationToken.ThrowIfCancellationRequested();
            return current;
        }

        if (!await _operationGate.WaitAsync(0, operationToken).ConfigureAwait(false))
        {
            operationToken.ThrowIfCancellationRequested();
            current = _lifecycle.Status;
            return current.State is HostState.Busy or HostState.Starting
                ? current
                : new HostStatus(HostState.Starting, "A host state transition is in progress; readiness is not yet confirmed.");
        }

        try
        {
            ThrowIfShutdownStarted();
            if (_lifecycle.Status.State == HostState.Busy)
            {
                operationToken.ThrowIfCancellationRequested();
                return _lifecycle.Status;
            }

            return await CheckBackendCoreAsync(operationToken).ConfigureAwait(false);
        }
        finally
        {
            _operationGate.Release();
        }
    }

    /// <summary>
    /// Returns a fresh readiness check together with model limits read from the active backend.
    /// Missing capability data is represented as null; callers must not construct a selectable
    /// host from a READY state alone.
    /// </summary>
    public async Task<HostPeerRuntimeSnapshot> GetFreshPeerRuntimeSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();

        HostStatus current = _lifecycle.Status;
        if (current.State == HostState.Busy)
        {
            operationToken.ThrowIfCancellationRequested();
            return new HostPeerRuntimeSnapshot(current, null);
        }

        if (!await _operationGate.WaitAsync(0, operationToken).ConfigureAwait(false))
        {
            operationToken.ThrowIfCancellationRequested();
            current = _lifecycle.Status;
            var transitional = current.State is HostState.Busy or HostState.Starting
                ? current
                : new HostStatus(HostState.Starting, "A host state transition is in progress; readiness is not yet confirmed.");
            return new HostPeerRuntimeSnapshot(transitional, null);
        }

        try
        {
            ThrowIfShutdownStarted();
            if (_lifecycle.Status.State == HostState.Busy)
            {
                operationToken.ThrowIfCancellationRequested();
                return new HostPeerRuntimeSnapshot(_lifecycle.Status, null);
            }

            HostStatus status = await CheckBackendCoreAsync(operationToken).ConfigureAwait(false);
            if (status.State != HostState.Ready || _backend is not IInferenceModelCapabilityProvider provider)
            {
                return new HostPeerRuntimeSnapshot(status, null);
            }

            try
            {
                InferenceModelCapabilitySnapshot? capability =
                    await provider.GetModelCapabilityAsync(operationToken).ConfigureAwait(false);
                operationToken.ThrowIfCancellationRequested();
                if (capability is null ||
                    string.IsNullOrWhiteSpace(capability.ModelId) ||
                    capability.MaxContextTokens == 0 ||
                    capability.MaxOutputTokens == 0 ||
                    capability.MaxOutputTokens > capability.MaxContextTokens ||
                    capability.MaxOutputTokens > ProtocolMessageLimits.MaximumOutputTokens)
                {
                    return new HostPeerRuntimeSnapshot(status, null);
                }

                return new HostPeerRuntimeSnapshot(status, capability);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                // A backend that cannot prove its active model limits is not selectable.
                return new HostPeerRuntimeSnapshot(status, null);
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task<HostStatus> ConnectAsync(Uri serverBaseAddress, string model, CancellationToken cancellationToken = default)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();
        await _operationGate.WaitAsync(operationToken).ConfigureAwait(false);
        try
        {
            ThrowIfShutdownStarted();
            if (_lifecycle.Status.State == HostState.Busy)
            {
                throw new InvalidOperationException("Cannot reconnect while inference is running.");
            }

            if (_lifecycle.Status.State != HostState.Stopped)
            {
                _lifecycle.Transition(HostState.Stopped, "Reconnecting to local inference backend.");
            }

            _lifecycle.Transition(HostState.Starting, "Checking local inference backend.");
            if (_backendFactory is null)
            {
                return _lifecycle.Transition(HostState.Error, "No local inference backend factory is configured.");
            }

            IInferenceBackend replacement;
            try
            {
                replacement = _backendFactory(serverBaseAddress, model);
                if (replacement is null)
                {
                    throw new InvalidOperationException("Backend factory returned no backend.");
                }
            }
            catch (Exception)
            {
                return _lifecycle.Transition(HostState.Error, "Local inference connection failed.");
            }

            var previousBackend = _backend;
            _backend = new UnconfiguredInferenceBackend();
            try
            {
                if (previousBackend is IDisposable oldBackend)
                {
                    oldBackend.Dispose();
                }

                _backend = replacement;
            }
            catch (Exception)
            {
                if (replacement is IDisposable replacementBackend)
                {
                    try
                    {
                        replacementBackend.Dispose();
                    }
                    catch (Exception)
                    {
                        // Keep the session fail-closed if replacement cleanup also fails.
                    }
                }

                return _lifecycle.Transition(HostState.Error, "Local inference connection failed.");
            }

            return await CheckBackendCoreAsync(operationToken).ConfigureAwait(false);
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task<HostStatus> CheckBackendAsync(CancellationToken cancellationToken = default)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();
        await _operationGate.WaitAsync(operationToken).ConfigureAwait(false);
        try
        {
            ThrowIfShutdownStarted();
            return await CheckBackendCoreAsync(operationToken).ConfigureAwait(false);
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private async Task<HostStatus> CheckBackendCoreAsync(CancellationToken cancellationToken)
    {
        if (_lifecycle.Status.State == HostState.Busy)
        {
            throw new InvalidOperationException("Cannot check backend while inference is running.");
        }

        if (_lifecycle.Status.State != HostState.Stopped && _lifecycle.Status.State != HostState.Starting)
        {
            _lifecycle.Transition(HostState.Stopped, "Checking local inference backend.");
        }

        if (_lifecycle.Status.State == HostState.Stopped)
        {
            _lifecycle.Transition(HostState.Starting, "Checking local inference backend.");
        }

        try
        {
            if (!await _backend.CheckHealthAsync(cancellationToken).ConfigureAwait(false))
            {
                return _lifecycle.Transition(HostState.Error, _backend.UnavailableReason);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var readiness = await EvaluateReadinessAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (!readiness.IsReady)
            {
                return _lifecycle.Transition(HostState.Error, readiness.Message);
            }

            return _lifecycle.Transition(HostState.Ready, readiness.Message);
        }
        catch (OperationCanceledException)
        {
            _lifecycle.Transition(HostState.Error, "Local inference connection was cancelled.");
            throw;
        }
        catch (Exception)
        {
            return _lifecycle.Transition(HostState.Error, "Local inference connection failed.");
        }
    }

    public async Task<string> GenerateAsync(InferenceRequest request, CancellationToken cancellationToken = default)
    {
        ThrowIfShutdownStarted();
        ArgumentNullException.ThrowIfNull(request);

        var validation = ProtocolMessageValidator.ValidateInferenceRequest(
            request.ProtocolVersion,
            request.RequestId,
            request.AttemptId,
            request.ModelId,
            request.Prompt,
            request.DeadlineUnixMs,
            request.MaxOutputTokens,
            request.ContextCapsule.Length,
            request.CalculateSize());
        if (!validation.IsValid)
        {
            throw new InvalidDataException(validation.Message);
        }

        if (request.MaxOutputTokens == 0)
        {
            throw new InvalidDataException("Maximum output tokens must be positive.");
        }

        return await GenerateCoreAsync(
            request.Prompt,
            request.MaxOutputTokens,
            request.ModelId,
            requireLiveModelCapability: true,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
        GenerateCoreAsync(
            prompt,
            maxOutputTokens,
            expectedModelId: null,
            requireLiveModelCapability: false,
            cancellationToken: cancellationToken);

    private async Task<string> GenerateCoreAsync(
        string prompt,
        uint maxOutputTokens,
        string? expectedModelId,
        bool requireLiveModelCapability,
        CancellationToken cancellationToken)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();
        await _operationGate.WaitAsync(operationToken).ConfigureAwait(false);
        try
        {
            ThrowIfShutdownStarted();
            if (!IsConnected)
            {
                throw new InvalidOperationException("Connect to a healthy local inference backend first.");
            }

            var promptValidation = ProtocolMessageLimits.ValidatePromptText(prompt);
            if (!promptValidation.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(prompt), promptValidation.Message);
            }

            HostReadinessResult readiness;
            try
            {
                readiness = await EvaluateReadinessAsync(operationToken).ConfigureAwait(false);
                operationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _lifecycle.Transition(HostState.Error, "Host readiness check was cancelled. Host is not Ready.");
                throw;
            }
            catch (Exception)
            {
                _lifecycle.Transition(HostState.Error, "Host readiness check failed. Host is not Ready.");
                throw new InvalidOperationException("Host readiness check failed. Host is not Ready.");
            }

            if (!readiness.IsReady)
            {
                _lifecycle.Transition(HostState.Error, readiness.Message);
                throw new InvalidOperationException(readiness.Message);
            }

            var absoluteOutputCeiling = Math.Min(
                ProtocolMessageLimits.MaximumOutputTokens,
                ProtocolMessageLimits.MaximumProductOutputTokens);
            if (maxOutputTokens == 0 || maxOutputTokens > absoluteOutputCeiling)
            {
                throw new InvalidDataException("Maximum output tokens are outside the product or protocol limit.");
            }

            if (_backend is IInferenceModelCapabilityProvider capabilityProvider)
            {
                var capability = await capabilityProvider.GetModelCapabilityAsync(operationToken).ConfigureAwait(false);
                operationToken.ThrowIfCancellationRequested();

                if (capability is null ||
                    string.IsNullOrWhiteSpace(capability.ModelId) ||
                    capability.MaxContextTokens == 0 ||
                    capability.MaxOutputTokens == 0 ||
                    capability.MaxOutputTokens > capability.MaxContextTokens ||
                    capability.MaxOutputTokens > ProtocolMessageLimits.MaximumOutputTokens)
                {
                    throw new InvalidOperationException("The active backend cannot prove valid model limits; inference is not admitted.");
                }

                if (requireLiveModelCapability &&
                    !string.Equals(expectedModelId, capability.ModelId, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The request model does not match the active backend model.");
                }

                var allowedOutputTokens = Math.Min(
                    Math.Min(capability.MaxOutputTokens, capability.MaxContextTokens),
                    Math.Min(
                        (ulong)ProtocolMessageLimits.MaximumProductOutputTokens,
                        (ulong)ProtocolMessageLimits.MaximumOutputTokens));

                if (maxOutputTokens > allowedOutputTokens)
                {
                    throw new InvalidDataException("Maximum output tokens exceeds the active model or product limit.");
                }
            }
            else if (requireLiveModelCapability)
            {
                throw new InvalidOperationException("The active backend cannot prove its model identity and output limits.");
            }

            _lifecycle.Transition(HostState.Busy, "Generating locally; one request at a time.");
            try
            {
                var output = await _backend.GenerateAsync(prompt, maxOutputTokens, operationToken).ConfigureAwait(false);
                operationToken.ThrowIfCancellationRequested();
                var responseValidation = ProtocolMessageLimits.ValidateResponseText(output);
                if (!responseValidation.IsValid)
                {
                    throw new InvalidDataException(responseValidation.Message);
                }

                _lifecycle.Transition(HostState.Ready, "Local inference completed.");
                return output;
            }
            catch
            {
                _lifecycle.Transition(HostState.Error, "Local inference failed. Check the local backend and reconnect.");
                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private async Task<HostReadinessResult> EvaluateReadinessAsync(CancellationToken cancellationToken)
    {
        if (_readinessEvaluator is null)
        {
            return new HostReadinessResult(
                false,
                "Host readiness checks are not configured; capacity remains unknown and the host is not Ready.");
        }

        var result = await _readinessEvaluator(cancellationToken).ConfigureAwait(false);
        if (result is null || string.IsNullOrWhiteSpace(result.Message))
        {
            return new HostReadinessResult(
                false,
                "Host readiness check returned no verified result; the host is not Ready.");
        }

        return result;
    }

    public async Task<HostStatus> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        using var operationCancellation = CreateOperationCancellationSource(cancellationToken);
        var operationToken = operationCancellation.Token;
        ThrowIfShutdownStarted();
        await _operationGate.WaitAsync(operationToken).ConfigureAwait(false);
        try
        {
            ThrowIfShutdownStarted();
            if (_lifecycle.Status.State != HostState.Stopped)
                _lifecycle.Transition(HostState.Stopped, "Active model disconnected; backend client resources released.");
            if (_backend is IDisposable backend) backend.Dispose();
            _backend = new UnconfiguredInferenceBackend();
            return _lifecycle.Status;
        }
        finally { _operationGate.Release(); }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
    {
        TaskCompletionSource? startDisposal = null;
        Task disposalTask;
        lock (_disposeSync)
        {
            if (_disposeTask is null)
            {
                startDisposal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _disposeTask = startDisposal.Task;
            }

            disposalTask = _disposeTask;
        }

        // Start outside the lock after publishing the shared task. Cancellation callbacks
        // may re-enter DisposeAsync; they must observe the same task instead of starting a
        // second shutdown while the first cancellation is being dispatched.
        if (startDisposal is not null)
        {
            _ = CompleteDisposeAsync(startDisposal);
        }

        return new ValueTask(disposalTask);
    }

    private async Task CompleteDisposeAsync(TaskCompletionSource completion)
    {
        try
        {
            await DisposeCoreAsync().ConfigureAwait(false);
            completion.TrySetResult();
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
    }

    private async Task DisposeCoreAsync()
    {
        Interlocked.Exchange(ref _shutdownStarted, 1);
        try
        {
            _shutdownCancellation.Cancel();
        }
        catch (AggregateException)
        {
            // Cancellation callbacks must not prevent the host from draining and disposing its backend.
        }

        var acquired = false;
        try
        {
            await _operationGate.WaitAsync().ConfigureAwait(false);
            acquired = true;
            if (_disposed)
            {
                return;
            }

            if (_lifecycle.Status.State != HostState.Stopped)
            {
                _lifecycle.Transition(HostState.Stopped, "Host session shutdown completed.");
            }

            var previousBackend = _backend;
            _backend = new UnconfiguredInferenceBackend();
            _disposed = true;
            if (previousBackend is IDisposable backend)
            {
                backend.Dispose();
            }
        }
        finally
        {
            if (acquired)
            {
                _operationGate.Release();
            }

            _shutdownCancellation.Dispose();
        }
    }
}

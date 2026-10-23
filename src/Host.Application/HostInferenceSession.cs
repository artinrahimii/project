using System.IO;
using WhiteP.Host.Domain;
using HostStatus = WhiteP.Host.Domain.HostStatus;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Application;

public sealed class HostInferenceSession : IDisposable
{
    private readonly HostLifecycle _lifecycle;
    private IInferenceBackend _backend;
    private readonly Func<Uri, string, IInferenceBackend>? _backendFactory;
    private readonly Func<CancellationToken, Task<HostReadinessResult>>? _readinessEvaluator;
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
    }

    public HostStatus Status => _lifecycle.Status;

    public bool IsConnected => !_disposed && _lifecycle.Status.State == HostState.Ready;

    public async Task<HostStatus> ConnectAsync(Uri serverBaseAddress, string model, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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

        try
        {
            var replacement = _backendFactory(serverBaseAddress, model);
            if (_backend is IDisposable oldBackend)
            {
                oldBackend.Dispose();
            }

            _backend = replacement;
        }
        catch (Exception)
        {
            return _lifecycle.Transition(HostState.Error, "Local inference connection failed.");
        }

        return await CheckBackendAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<HostStatus> CheckBackendAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_lifecycle.Status.State == HostState.Busy)
        {
            throw new InvalidOperationException("Cannot check backend while inference is running.");
        }

        if (_lifecycle.Status.State != HostState.Stopped)
        {
            _lifecycle.Transition(HostState.Stopped, "Checking local inference backend.");
        }

        _lifecycle.Transition(HostState.Starting, "Checking local inference backend.");
        try
        {
            if (!await _backend.CheckHealthAsync(cancellationToken).ConfigureAwait(false))
            {
                return _lifecycle.Transition(HostState.Error, _backend.UnavailableReason);
            }

            var readiness = await EvaluateReadinessAsync(cancellationToken).ConfigureAwait(false);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var validation = ProtocolMessageValidator.ValidateInferenceRequest(
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

        return await GenerateAsync(request.Prompt, request.MaxOutputTokens, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
            readiness = await EvaluateReadinessAsync(cancellationToken).ConfigureAwait(false);
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

        _lifecycle.Transition(HostState.Busy, "Generating locally; one request at a time.");
        try
        {
            var output = await _backend.GenerateAsync(prompt, maxOutputTokens, cancellationToken).ConfigureAwait(false);
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // A closed, idle session must not continue to advertise a healthy backend.
        // An in-flight request remains Busy until its cancellation/error path settles.
        if (_lifecycle.Status.State != HostState.Stopped &&
            _lifecycle.Status.State != HostState.Busy)
        {
            _lifecycle.Transition(HostState.Stopped, "Host session disposed.");
        }

        _disposed = true;
        if (_backend is IDisposable backend)
        {
            backend.Dispose();
        }
    }
}

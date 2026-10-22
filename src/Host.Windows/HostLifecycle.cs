using System.IO;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows;

public enum HostState
{
    Stopped,
    Starting,
    Ready,
    Busy,
    Error
}

public sealed record HostStatus(HostState State, string Message);

public sealed class HostLifecycle
{
    private static readonly IReadOnlyDictionary<HostState, HostState[]> Allowed = new Dictionary<HostState, HostState[]>
    {
        [HostState.Stopped] = [HostState.Starting],
        [HostState.Starting] = [HostState.Ready, HostState.Error, HostState.Stopped],
        [HostState.Ready] = [HostState.Busy, HostState.Error, HostState.Stopped],
        [HostState.Busy] = [HostState.Ready, HostState.Error],
        [HostState.Error] = [HostState.Starting, HostState.Stopped]
    };

    public HostStatus Status { get; private set; } = new(HostState.Stopped, "Host stopped.");

    public HostStatus Transition(HostState next, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!Allowed[Status.State].Contains(next))
        {
            throw new InvalidOperationException($"Invalid host lifecycle transition: {Status.State} -> {next}.");
        }

        return Status = new HostStatus(next, message.Trim());
    }
}

public interface IInferenceBackend
{
    bool IsAvailable { get; }

    string UnavailableReason { get; }

    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(IsAvailable);

    Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new NotSupportedException("This backend does not support text generation."));
}

public sealed class HostStartup(HostLifecycle lifecycle, IInferenceBackend backend)
{
    public HostStatus Start()
    {
        lifecycle.Transition(HostState.Starting, "Checking local inference backend.");
        try
        {
            if (backend.IsAvailable)
            {
                return lifecycle.Transition(HostState.Ready, "Local inference backend is ready.");
            }

            var reason = backend.UnavailableReason;
            return lifecycle.Transition(
                HostState.Error,
                string.IsNullOrWhiteSpace(reason)
                    ? "Local inference backend is unavailable. Host is not ready."
                    : reason.Trim());
        }
        catch (Exception)
        {
            return lifecycle.Transition(HostState.Error, "Local inference backend check failed. Host is not ready.");
        }
    }
}

public sealed class UnconfiguredInferenceBackend : IInferenceBackend
{
    public bool IsAvailable => false;

    public string UnavailableReason => "Local inference backend is not configured. Enter a server URL and connect.";

    public Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

    public Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new InvalidOperationException(UnavailableReason));
}

public sealed class HostInferenceSession : IDisposable
{
    private readonly HostLifecycle _lifecycle;
    private IInferenceBackend _backend;
    private bool _disposed;

    public HostInferenceSession(HostLifecycle? lifecycle = null, IInferenceBackend? backend = null)
    {
        _lifecycle = lifecycle ?? new HostLifecycle();
        _backend = backend ?? new UnconfiguredInferenceBackend();
    }

    public HostStatus Status => _lifecycle.Status;

    public bool IsConnected => _lifecycle.Status.State == HostState.Ready;

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
        if (_backend is IDisposable oldBackend)
        {
            oldBackend.Dispose();
        }

        _backend = new LlamaCppServerBackend(serverBaseAddress, model);
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
            if (await _backend.CheckHealthAsync(cancellationToken).ConfigureAwait(false))
            {
                return _lifecycle.Transition(HostState.Ready, "Local inference backend is ready.");
            }

            return _lifecycle.Transition(HostState.Error, _backend.UnavailableReason);
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_backend is IDisposable backend)
        {
            backend.Dispose();
        }
    }
}

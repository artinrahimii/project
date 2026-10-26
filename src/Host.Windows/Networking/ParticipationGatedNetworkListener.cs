using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using WhiteP.Shared.Contracts.V1;

namespace WhiteP.Host.Windows.Networking;

public sealed class ParticipationGatedNetworkListener : IAsyncDisposable
{
    private static readonly TimeSpan FrameReadTimeout = TimeSpan.FromSeconds(5);
    private readonly Func<bool> _isParticipationActive;
    private readonly Func<InferenceRequest, CancellationToken, Task>? _onValidInferenceRequest;
    private readonly IPEndPoint _requestedEndpoint;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly ConcurrentDictionary<TcpClient, byte> _activeClients = new();
    private readonly ConcurrentDictionary<Task, byte> _connectionTasks = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _acceptCancellation;
    private Task? _acceptLoop;
    private bool _disposed;

    public ParticipationGatedNetworkListener(Func<bool> isParticipationActive, IPAddress bindAddress, int port, Func<InferenceRequest, CancellationToken, Task>? onValidInferenceRequest = null)
    {
        ArgumentNullException.ThrowIfNull(isParticipationActive);
        ArgumentNullException.ThrowIfNull(bindAddress);
        if (port is < 0 or > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(port));
        _isParticipationActive = isParticipationActive;
        _requestedEndpoint = new IPEndPoint(bindAddress, port);
        _onValidInferenceRequest = onValidInferenceRequest;
    }
    public bool IsListening => _listener is not null;
    public IPEndPoint? LocalEndpoint { get; private set; }
    internal int ActiveConnectionCount => _activeClients.Count;

    public async Task<NetworkListenerStartResult> StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_listener is not null) return NetworkListenerStartResult.AlreadyListening;
            if (!_isParticipationActive()) return NetworkListenerStartResult.ParticipationInactive;
            var listener = new TcpListener(_requestedEndpoint);
            var acceptCancellation = new CancellationTokenSource();
            try { listener.Start(); }
            catch { acceptCancellation.Dispose(); listener.Stop(); throw; }
            _listener = listener; _acceptCancellation = acceptCancellation;
            LocalEndpoint = (IPEndPoint)listener.LocalEndpoint;
            _acceptLoop = AcceptConnectionsAsync(listener, acceptCancellation.Token);
            return NetworkListenerStartResult.Started;
        }
        finally { _lifecycleLock.Release(); }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { await StopCoreAsync().ConfigureAwait(false); }
        finally { _lifecycleLock.Release(); }
    }
    private async Task StopCoreAsync()
    {
        var listener = _listener; var acceptCancellation = _acceptCancellation; var acceptLoop = _acceptLoop;
        _listener = null; _acceptCancellation = null; _acceptLoop = null; LocalEndpoint = null;
        acceptCancellation?.Cancel(); listener?.Stop();
        if (acceptLoop is not null)
        {
            try { await acceptLoop.ConfigureAwait(false); }
            catch (OperationCanceledException) when (acceptCancellation?.IsCancellationRequested == true) { }
            catch (SocketException) when (acceptCancellation?.IsCancellationRequested == true) { }
            catch (ObjectDisposedException) when (acceptCancellation?.IsCancellationRequested == true) { }
        }
        foreach (var client in _activeClients.Keys) client.Dispose();
        var tasks = _connectionTasks.Keys.ToArray();
        if (tasks.Length > 0) { try { await Task.WhenAll(tasks).ConfigureAwait(false); } catch { } }
        acceptCancellation?.Dispose();
    }
    private async Task AcceptConnectionsAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try { client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return; }
            catch (SocketException) when (cancellationToken.IsCancellationRequested) { return; }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested) { return; }
            if (cancellationToken.IsCancellationRequested) { client.Dispose(); return; }
            _activeClients.TryAdd(client, 0);
            var connectionTask = HandleClientAsync(client, cancellationToken);
            _connectionTasks.TryAdd(connectionTask, 0);
            _ = connectionTask.ContinueWith(static (completedTask, state) =>
            {
                var owner = (ParticipationGatedNetworkListener)state!;
                owner._connectionTasks.TryRemove(completedTask, out _);
            }, this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
    private async Task HandleClientAsync(TcpClient client, CancellationToken shutdownToken)
    {
        try
        {
            using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
            readTimeout.CancelAfter(FrameReadTimeout);
            var frame = await NetworkInferenceRequestFrameReader.ReadAsync(client.GetStream(), readTimeout.Token).ConfigureAwait(false);
            if (!frame.IsAccepted || frame.Request is null) return;
            if (_onValidInferenceRequest is not null) await _onValidInferenceRequest(frame.Request, shutdownToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }
        catch (Exception) { }
        finally { _activeClients.TryRemove(client, out _); client.Dispose(); }
    }
    public async ValueTask DisposeAsync()
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try { if (_disposed) return; _disposed = true; await StopCoreAsync().ConfigureAwait(false); }
        finally { _lifecycleLock.Release(); }
    }
}
public enum NetworkListenerStartResult { Started, AlreadyListening, ParticipationInactive }

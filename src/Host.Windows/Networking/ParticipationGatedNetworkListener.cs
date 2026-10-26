using System.Net;
using System.Net.Sockets;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Owns the TCP listening socket for optional inbound host participation.
/// Until the shared protocol handler is added, accepted connections are closed without reading
/// or logging payload bytes.
/// </summary>
public sealed class ParticipationGatedNetworkListener : IAsyncDisposable
{
    private readonly Func<bool> _isParticipationActive;
    private readonly IPEndPoint _requestedEndpoint;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private TcpListener? _listener;
    private CancellationTokenSource? _acceptCancellation;
    private Task? _acceptLoop;
    private bool _disposed;

    public ParticipationGatedNetworkListener(
        Func<bool> isParticipationActive,
        IPAddress bindAddress,
        int port)
    {
        ArgumentNullException.ThrowIfNull(isParticipationActive);
        ArgumentNullException.ThrowIfNull(bindAddress);
        if (port is < 0 or > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        _isParticipationActive = isParticipationActive;
        _requestedEndpoint = new IPEndPoint(bindAddress, port);
    }

    public bool IsListening => _listener is not null;

    public IPEndPoint? LocalEndpoint { get; private set; }

    public async Task<NetworkListenerStartResult> StartAsync(
        CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_listener is not null)
            {
                return NetworkListenerStartResult.AlreadyListening;
            }

            // Fail closed: do not bind a socket unless current participation is explicitly active.
            if (!_isParticipationActive())
            {
                return NetworkListenerStartResult.ParticipationInactive;
            }

            var listener = new TcpListener(_requestedEndpoint);
            var acceptCancellation = new CancellationTokenSource();
            try
            {
                listener.Start();
            }
            catch
            {
                acceptCancellation.Dispose();
                listener.Stop();
                throw;
            }

            _listener = listener;
            _acceptCancellation = acceptCancellation;
            LocalEndpoint = (IPEndPoint)listener.LocalEndpoint;
            _acceptLoop = AcceptAndCloseUnparsedConnectionsAsync(listener, acceptCancellation.Token);

            return NetworkListenerStartResult.Started;
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StopCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    private async Task StopCoreAsync()
    {
        var listener = _listener;
        var acceptCancellation = _acceptCancellation;
        var acceptLoop = _acceptLoop;

        _listener = null;
        _acceptCancellation = null;
        _acceptLoop = null;
        LocalEndpoint = null;

        if (acceptCancellation is not null)
        {
            acceptCancellation.Cancel();
        }

        listener?.Stop();

        if (acceptLoop is not null)
        {
            try
            {
                await acceptLoop.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (acceptCancellation?.IsCancellationRequested == true)
            {
                // Expected when StopAsync cancels a pending accept.
            }
            catch (SocketException) when (acceptCancellation?.IsCancellationRequested == true)
            {
                // TcpListener.Stop may surface as a socket exception on the accept loop.
            }
            catch (ObjectDisposedException) when (acceptCancellation?.IsCancellationRequested == true)
            {
                // Expected if the listener is disposed while AcceptTcpClientAsync is pending.
            }
        }

        acceptCancellation?.Dispose();
    }

    private static async Task AcceptAndCloseUnparsedConnectionsAsync(
        TcpListener listener,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // T0110 adds the protocol parser. Until then, reject the connection without inspecting payloads.
            client.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            await StopCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }
}

public enum NetworkListenerStartResult
{
    Started,
    AlreadyListening,
    ParticipationInactive
}

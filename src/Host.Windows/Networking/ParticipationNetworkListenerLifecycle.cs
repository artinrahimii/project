using System.Net;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Serializes participation activation/deactivation and ensures listener disposal completes
/// before the owner exits. The first transport milestone intentionally binds to loopback only.
/// </summary>
public sealed class ParticipationNetworkListenerLifecycle : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly ParticipationGatedNetworkListener _listener;
    private volatile bool _participationActive;
    private bool _disposed;

    public ParticipationNetworkListenerLifecycle(IPAddress bindAddress, int port)
    {
        _listener = new ParticipationGatedNetworkListener(
            () => _participationActive,
            bindAddress,
            port);
    }

    public bool IsParticipationActive => _participationActive;

    public bool IsListening => _listener.IsListening;

    public IPEndPoint? LocalEndpoint => _listener.LocalEndpoint;

    public async Task SetParticipationActiveAsync(
        bool active,
        CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!active)
            {
                _participationActive = false;
                // Once deactivation starts, complete the stop even if the initiating UI operation is cancelled.
                await _listener.StopAsync().ConfigureAwait(false);
                return;
            }

            _participationActive = true;
            try
            {
                var result = await _listener.StartAsync(cancellationToken).ConfigureAwait(false);
                if (result == NetworkListenerStartResult.ParticipationInactive)
                {
                    throw new InvalidOperationException(
                        "Network listener startup was refused because participation is inactive.");
                }
            }
            catch
            {
                _participationActive = false;
                await _listener.StopAsync().ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            _lifecycleLock.Release();
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

            _participationActive = false;
            _disposed = true;
            await _listener.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }
}

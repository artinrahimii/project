using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WhiteP.Shared.Contracts.Networking;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Publishes metadata-only peer announcements as renewable, expiring leases.
/// The owner must only advertise addresses that are actually reachable by peers.
/// </summary>
public sealed class PeerAnnouncementLeasePublisher : IAsyncDisposable
{
    public static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan DefaultRenewalInterval = TimeSpan.FromMinutes(1);

    private readonly PeerAnnouncementClient _client;
    private readonly PeerAnnouncement _template;
    private readonly TimeSpan _timeToLive;
    private readonly TimeSpan _renewalInterval;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private CancellationTokenSource? _renewalCancellation;
    private Task? _renewalTask;
    private string? _lastRenewalFailureType;
    private bool _disposed;

    public PeerAnnouncementLeasePublisher(
        PeerAnnouncementClient client,
        PeerAnnouncement template,
        TimeSpan? timeToLive = null,
        TimeSpan? renewalInterval = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(template);

        _client = client;
        _template = template;
        _timeToLive = timeToLive ?? DefaultTimeToLive;
        _renewalInterval = renewalInterval ?? DefaultRenewalInterval;

        if (_timeToLive <= TimeSpan.Zero || _timeToLive > PeerAnnouncement.MaximumLifetime)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeToLive),
                "Lease time to live must be positive and no longer than the announcement contract allows.");
        }

        if (_renewalInterval <= TimeSpan.Zero || _renewalInterval >= _timeToLive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(renewalInterval),
                "Renewal interval must be positive and shorter than the lease time to live.");
        }
    }

    public bool IsRunning => _renewalTask is { IsCompleted: false };

    /// <summary>Gets the type of the last transient renewal failure, without retaining message content.</summary>
    public string? LastRenewalFailureType => Volatile.Read(ref _lastRenewalFailureType);

    /// <summary>Announces immediately, then refreshes expiry at the configured interval.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_renewalTask is not null)
            {
                throw new InvalidOperationException("This announcement lease has already been started.");
            }

            await PublishFreshLeaseAsync(cancellationToken).ConfigureAwait(false);
            Volatile.Write(ref _lastRenewalFailureType, null);
            _renewalCancellation = new CancellationTokenSource();
            _renewalTask = RenewAsync(_renewalCancellation.Token);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Stops renewal. The remote directory entry is intentionally not deleted; it expires naturally.
    /// </summary>
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

    private async Task RenewAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                await Task.Delay(_renewalInterval, cancellationToken).ConfigureAwait(false);
                await PublishFreshLeaseAsync(cancellationToken).ConfigureAwait(false);
                Volatile.Write(ref _lastRenewalFailureType, null);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception) when (
                !cancellationToken.IsCancellationRequested &&
                (exception is HttpRequestException or IOException or InvalidOperationException or TimeoutException or OperationCanceledException))
            {
                // Keep the renewal loop alive through transient failures and avoid retaining request or response content.
                Volatile.Write(ref _lastRenewalFailureType, exception.GetType().Name);
            }
        }
    }

    private Task PublishFreshLeaseAsync(CancellationToken cancellationToken)
    {
        var refreshed = new PeerAnnouncement(
            _template.PeerId,
            _template.Addresses,
            _template.Capabilities,
            DateTimeOffset.UtcNow.Add(_timeToLive));
        return _client.AnnounceAsync(refreshed, cancellationToken);
    }

    private async Task StopCoreAsync()
    {
        CancellationTokenSource? cancellation = _renewalCancellation;
        Task? renewal = _renewalTask;
        _renewalCancellation = null;
        _renewalTask = null;

        if (cancellation is null)
        {
            return;
        }

        cancellation.Cancel();
        if (renewal is not null)
        {
            try
            {
                await renewal.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // Expected when the periodic delay or HTTP request is cancelled during shutdown.
            }
        }

        cancellation.Dispose();
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

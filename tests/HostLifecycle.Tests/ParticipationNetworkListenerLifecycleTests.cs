using System.Net;
using System.Net.Sockets;
using WhiteP.Host.Windows.Networking;

internal static class ParticipationNetworkListenerLifecycleTests
{
    public static async Task RunAsync()
    {
        await DeactivationStopsAndReleasesTheListenerAsync().ConfigureAwait(false);
        await DisposalStopsAnActiveListenerAsync().ConfigureAwait(false);

        Console.WriteLine(
            "PASS: disabling network participation and disposing the host lifecycle stop and release the listener.");
    }

    private static async Task DeactivationStopsAndReleasesTheListenerAsync()
    {
        await using var lifecycle = new ParticipationNetworkListenerLifecycle(IPAddress.Loopback, 0);
        Assert(!lifecycle.IsParticipationActive && !lifecycle.IsListening,
            "Participation must start disabled without a listening socket.");

        await lifecycle.SetParticipationActiveAsync(true).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var firstEndpoint = lifecycle.LocalEndpoint
            ?? throw new InvalidOperationException("An active listener must publish its bound endpoint.");
        Assert(lifecycle.IsParticipationActive && lifecycle.IsListening,
            "Activating participation must start the listener.");

        await lifecycle.SetParticipationActiveAsync(false).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!lifecycle.IsParticipationActive && !lifecycle.IsListening && lifecycle.LocalEndpoint is null,
            "Disabling participation must stop and clear the active listener.");

        using (var bindProbe = new TcpListener(firstEndpoint.Address, firstEndpoint.Port))
        {
            bindProbe.Start();
            bindProbe.Stop();
        }

        await lifecycle.SetParticipationActiveAsync(true).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(lifecycle.IsListening && lifecycle.IsParticipationActive,
            "Participation must be restartable after a complete stop.");

        await lifecycle.SetParticipationActiveAsync(false).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!lifecycle.IsListening, "A subsequent disable must stop the restarted listener.");
    }

    private static async Task DisposalStopsAnActiveListenerAsync()
    {
        var lifecycle = new ParticipationNetworkListenerLifecycle(IPAddress.Loopback, 0);
        await lifecycle.SetParticipationActiveAsync(true).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var endpoint = lifecycle.LocalEndpoint
            ?? throw new InvalidOperationException("An active listener must expose its endpoint.");

        await lifecycle.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!lifecycle.IsParticipationActive && !lifecycle.IsListening && lifecycle.LocalEndpoint is null,
            "Disposing the lifecycle must stop the listener and clear endpoint state.");

        using (var bindProbe = new TcpListener(endpoint.Address, endpoint.Port))
        {
            bindProbe.Start();
            bindProbe.Stop();
        }

        var rejectedAfterDispose = false;
        try
        {
            await lifecycle.SetParticipationActiveAsync(true).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            rejectedAfterDispose = true;
        }

        Assert(rejectedAfterDispose, "A disposed lifecycle must reject later activation.");
        await lifecycle.DisposeAsync().ConfigureAwait(false);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

using System.Net;
using System.Net.Sockets;
using WhiteP.Host.Windows.Networking;

internal static class HostNetworkListenerParticipationTests
{
    public static async Task RunAsync()
    {
        await InactiveParticipationDoesNotBindAsync().ConfigureAwait(false);
        await ActiveParticipationStartsAndAcceptsTcpAsync().ConfigureAwait(false);

        Console.WriteLine(
            "PASS: the host TCP listener fails closed while participation is inactive and binds only after activation.");
    }

    private static async Task InactiveParticipationDoesNotBindAsync()
    {
        var isParticipationActive = false;
        var participationChecks = 0;
        await using var listener = new ParticipationGatedNetworkListener(
            () =>
            {
                participationChecks++;
                return isParticipationActive;
            },
            IPAddress.Loopback,
            0);

        var inactiveResult = await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        Assert(inactiveResult == NetworkListenerStartResult.ParticipationInactive,
            "Inactive participation must prevent listener startup.");
        Assert(!listener.IsListening && listener.LocalEndpoint is null,
            "An inactive participation attempt must not bind a listening socket.");

        isParticipationActive = true;
        var activeResult = await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(activeResult == NetworkListenerStartResult.Started,
            "An explicit active participation state must allow listener startup.");
        Assert(listener.IsListening && listener.LocalEndpoint is { Port: > 0 },
            "Successful startup must expose the actual locally bound endpoint.");
        Assert(participationChecks == 2,
            "Participation must be rechecked for each startup attempt.");
    }

    private static async Task ActiveParticipationStartsAndAcceptsTcpAsync()
    {
        await using var listener = new ParticipationGatedNetworkListener(
            () => true,
            IPAddress.Loopback,
            0);

        var start = await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(start == NetworkListenerStartResult.Started, "Active participation must start the TCP listener.");

        var endpoint = listener.LocalEndpoint
            ?? throw new InvalidOperationException("A started listener must expose its local endpoint.");

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(endpoint.Address, endpoint.Port)
                .WaitAsync(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            await client.GetStream().WriteAsync(new byte[] { 0, 0, 0, 0 }.AsMemory()).ConfigureAwait(false);
            var buffer = new byte[1];
            var bytesRead = await client.GetStream().ReadAsync(buffer.AsMemory())
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            Assert(bytesRead == 0,
                "Before T0110 installs the Protobuf parser, the listener must close accepted connections without reading payloads.");
        }

        var repeatedStart = await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(repeatedStart == NetworkListenerStartResult.AlreadyListening,
            "Starting an already running listener must not create another listening socket.");

        await listener.StopAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!listener.IsListening && listener.LocalEndpoint is null,
            "StopAsync must release the listening endpoint.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

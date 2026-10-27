using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using WhiteP.Host.Windows.Networking;

internal static class PeerTransportContractAcceptanceTests
{
    public static async Task RunAsync()
    {
        VerifyInterfaceExposesTheRequiredSecureLifecycle();
        VerifyEndpointRequiresAddressAndExpectedIdentity();
        await VerifyDirectEncryptedPeerMessageExchangeAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: IPeerTransport contract plus direct libp2p TCP message exchange, peer identity pinning, bounded framing and idempotent disconnect.");
    }

    private static async Task VerifyDirectEncryptedPeerMessageExchangeAsync()
    {
        await using var client = new Libp2pPeerTransport(maximumMessageBytes: 1024);
        await using var server = new Libp2pPeerTransport(
            expectedRemotePeerIdentity: client.LocalPeerIdentity,
            maximumMessageBytes: 1024);

        int port = ReserveTcpPort();
        await server.StartListeningAsync(IPAddress.Loopback, port).WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        string serverAddress = server.ListenAddresses.Single();
        Assert(serverAddress.Contains("/p2p/", StringComparison.Ordinal),
            "The listen address must include the local PeerId for authenticated dialing.");

        await using (var mismatchedIdentityClient = new Libp2pPeerTransport(maximumMessageBytes: 1024))
        {
            bool rejected = false;
            try
            {
                await mismatchedIdentityClient.ConnectSecureAsync(
                    new PeerTransportEndpoint(serverAddress, client.LocalPeerIdentity),
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait(false);
            }
            catch (AuthenticationException)
            {
                rejected = true;
            }

            Assert(rejected, "A peer address whose /p2p identity disagrees with the pinned identity must be rejected before dialing.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await client.ConnectSecureAsync(
            new PeerTransportEndpoint(serverAddress, server.LocalPeerIdentity),
            timeout.Token).ConfigureAwait(false);

        Assert(client.IsConnected && server.IsConnected,
            "Both peers must report connected only after the encrypted protocol stream is active.");
        Assert(client.RemotePeerIdentity == server.LocalPeerIdentity,
            "The dialing peer must expose the authenticated server PeerId.");
        Assert(server.RemotePeerIdentity == client.LocalPeerIdentity,
            "The listening peer must expose the authenticated client PeerId.");

        byte[] clientPayload = [0x00, 0x01, 0xFF, 0x0A, 0x7F];
        await client.SendMessageAsync(clientPayload, timeout.Token).ConfigureAwait(false);
        byte[] receivedByServer = await server.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        Assert(clientPayload.SequenceEqual(receivedByServer),
            "Direct peer transfer must preserve arbitrary binary message bytes.");

        byte[] serverPayload = [0xAA, 0x00, 0xFE, 0x03];
        await server.SendMessageAsync(serverPayload, timeout.Token).ConfigureAwait(false);
        byte[] receivedByClient = await client.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        Assert(serverPayload.SequenceEqual(receivedByClient),
            "A response message must travel directly from the listener to the dialing peer.");

        bool oversizedRejected = false;
        try
        {
            await client.SendMessageAsync(new byte[1025], timeout.Token).ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            oversizedRejected = true;
        }

        Assert(oversizedRejected, "Messages larger than the configured application bound must be rejected before transmission.");
        await client.DisconnectAsync(timeout.Token).ConfigureAwait(false);
        await client.DisconnectAsync(timeout.Token).ConfigureAwait(false);
        Assert(!client.IsConnected, "Disconnect must clear the authenticated connection state.");
    }

    private static int ReserveTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static void VerifyInterfaceExposesTheRequiredSecureLifecycle()
    {
        Assert(typeof(IPeerTransport).IsInterface, "IPeerTransport must be an abstraction, not a concrete transport implementation.");
        Assert(typeof(IAsyncDisposable).IsAssignableFrom(typeof(IPeerTransport)), "IPeerTransport must support asynchronous resource disposal.");
        Assert(typeof(IPeerTransport).GetProperty(nameof(IPeerTransport.IsConnected))?.PropertyType == typeof(bool),
            "IsConnected must expose the authenticated connection state.");
        Assert(typeof(IPeerTransport).GetProperty(nameof(IPeerTransport.RemotePeerIdentity))?.PropertyType == typeof(string),
            "RemotePeerIdentity must expose the authenticated remote identity.");

        AssertMethod(nameof(IPeerTransport.ConnectSecureAsync), typeof(Task), typeof(PeerTransportEndpoint), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.SendMessageAsync), typeof(Task), typeof(ReadOnlyMemory<byte>), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.ReceiveMessageAsync), typeof(Task<byte[]>), typeof(CancellationToken));
        AssertMethod(nameof(IPeerTransport.DisconnectAsync), typeof(Task), typeof(CancellationToken));
    }

    private static void AssertMethod(string name, Type returnType, params Type[] parameterTypes)
    {
        var method = typeof(IPeerTransport).GetMethod(name, parameterTypes);
        Assert(method is not null, $"IPeerTransport is missing required method {name}.");
        Assert(method!.ReturnType == returnType, $"{name} has an unexpected return type.");
    }

    private static void VerifyEndpointRequiresAddressAndExpectedIdentity()
    {
        var endpoint = new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", "peer-ephemeral-01");
        Assert(endpoint.Address == "/ip4/192.0.2.15/tcp/43001", "Transport address must be preserved.");
        Assert(endpoint.ExpectedPeerIdentity == "peer-ephemeral-01", "Expected peer identity must be preserved.");

        ExpectArgumentException(() => new PeerTransportEndpoint("", "peer-ephemeral-01"),
            "A blank transport address must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint(" /ip4/192.0.2.15/tcp/43001", "peer-ephemeral-01"),
            "An address with surrounding whitespace must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", " "),
            "A missing expected peer identity must be rejected.");
        ExpectArgumentException(() => new PeerTransportEndpoint("/ip4/192.0.2.15/tcp/43001", "peer\nmalicious"),
            "An expected peer identity containing control characters must be rejected.");
    }

    private static void ExpectArgumentException(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

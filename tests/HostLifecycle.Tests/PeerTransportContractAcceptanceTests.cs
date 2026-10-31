using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Multiformats.Address;
using Nethermind.Libp2p;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Core.Exceptions;
using WhiteP.Host.Windows.Networking;

internal static class PeerTransportContractAcceptanceTests
{
    public static async Task RunAsync()
    {
        VerifyInterfaceExposesTheRequiredSecureLifecycle();
        VerifyEndpointRequiresAddressAndExpectedIdentity();
        await VerifyDirectEncryptedPeerMessageExchangeAsync().ConfigureAwait(false);
        await VerifySecureChannelRejectsUnexpectedPeerIdentityAsync().ConfigureAwait(false);
        await VerifyRejectsPlaintextOnlyPeerAsync().ConfigureAwait(false);
        await VerifyRejectsUnsupportedSecureChannelProtocolVersionAsync().ConfigureAwait(false);
        await VerifyQuicFallbackAfterUnreachableTcpAddressAsync().ConfigureAwait(false);
        await VerifyFallbackRejectsMixedPeerIdentitiesBeforeConnectingAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: IPeerTransport contract, direct libp2p TCP exchange, authenticated TCP-to-QUIC fallback, bounded framing and disconnect semantics.");
    }

    private static async Task VerifyDirectEncryptedPeerMessageExchangeAsync()
    {
        using var clientTrustFixture = new TestPeerTrustStoreFixture();
        using var serverTrustFixture = new TestPeerTrustStoreFixture();
        await using var client = new Libp2pPeerTransport(clientTrustFixture.Store, maximumMessageBytes: 1024);
        await using var server = new Libp2pPeerTransport(
            serverTrustFixture.Store,
            expectedRemotePeerIdentity: client.LocalPeerIdentity,
            maximumMessageBytes: 1024);
        serverTrustFixture.Store.TrustPeerAfterExplicitConfirmation(client.LocalPeerIdentity, explicitlyConfirmed: true);

        int port = ReserveTcpPort();
        await server.StartListeningAsync(IPAddress.Loopback, port).WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        string serverAddress = server.ListenAddresses.Single(address => address.Contains("/tcp/", StringComparison.Ordinal));
        Assert(serverAddress.Contains("/p2p/", StringComparison.Ordinal),
            "The listen address must include the local PeerId for authenticated dialing.");
        Assert(server.ListenAddresses.Any(address => address.Contains("/quic-v1", StringComparison.Ordinal)),
            "The listener must publish a QUIC/UDP alternative for the same authenticated peer.");

        bool unknownPeerRejected = false;
        try
        {
            await client.ConnectSecureAsync(
                new PeerTransportEndpoint(serverAddress, server.LocalPeerIdentity),
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait(false);
        }
        catch (AuthenticationException)
        {
            unknownPeerRejected = true;
        }
        Assert(unknownPeerRejected && !client.IsConnected,
            "A syntactically valid but untrusted PeerId must be rejected before the transport dials.");

        clientTrustFixture.Store.TrustPeerAfterExplicitConfirmation(server.LocalPeerIdentity, explicitlyConfirmed: true);

        using var mismatchedTrustFixture = new TestPeerTrustStoreFixture();
        await using (var mismatchedIdentityClient = new Libp2pPeerTransport(mismatchedTrustFixture.Store, maximumMessageBytes: 1024))
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

    private static async Task VerifySecureChannelRejectsUnexpectedPeerIdentityAsync()
    {
        using var clientTrustFixture = new TestPeerTrustStoreFixture();
        using var serverTrustFixture = new TestPeerTrustStoreFixture();

        var unexpectedIdentity = new Identity();
        string unexpectedPeerIdentity = new PeerId(unexpectedIdentity.PublicKey).ToString();

        await using var client = new Libp2pPeerTransport(clientTrustFixture.Store, maximumMessageBytes: 1024);
        await using var server = new Libp2pPeerTransport(
            serverTrustFixture.Store,
            expectedRemotePeerIdentity: client.LocalPeerIdentity,
            maximumMessageBytes: 1024);

        clientTrustFixture.Store.TrustPeerAfterExplicitConfirmation(
            unexpectedPeerIdentity,
            explicitlyConfirmed: true);
        serverTrustFixture.Store.TrustPeerAfterExplicitConfirmation(
            client.LocalPeerIdentity,
            explicitlyConfirmed: true);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        int port = ReserveTcpPort();
        await server.StartListeningAsync(IPAddress.Loopback, port, timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
            .ConfigureAwait(false);

        string actualServerAddress = server.ListenAddresses.Single(
            address => address.Contains("/tcp/", StringComparison.Ordinal));
        string addressWithUnexpectedPeerIdentity = ReplacePeerIdentity(actualServerAddress, unexpectedPeerIdentity);

        bool rejected = false;
        try
        {
            await client.ConnectSecureAsync(
                new PeerTransportEndpoint(addressWithUnexpectedPeerIdentity, unexpectedPeerIdentity),
                timeout.Token)
                .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is AuthenticationException or IOException or SocketException or PeerConnectionException or InvalidOperationException)
        {
            rejected = true;
        }

        Assert(rejected,
            "The libp2p secure-channel boundary must reject a remote identity that differs from the pinned PeerId.");
        Assert(!client.IsConnected && client.RemotePeerIdentity is null,
            "A failed secure-channel identity check must never publish a connected or authenticated remote peer.");

        Console.WriteLine(
            "PASS: libp2p secure-channel authentication rejects a remote identity that does not match the pinned PeerId.");
    }

    private static async Task VerifyRejectsPlaintextOnlyPeerAsync()
    {
        using var clientTrustFixture = new TestPeerTrustStoreFixture();
        using ServiceProvider serverServices = new ServiceCollection()
            .AddLogging()
            .AddLibp2p(builder => builder.WithPlaintextEnforced())
            .BuildServiceProvider();

        IPeerFactory serverFactory = serverServices.GetRequiredService<IPeerFactory>();
        await using ILocalPeer serverPeer = serverFactory.Create();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        int port = ReserveTcpPort();
        await serverPeer.StartListenAsync(
                [Multiaddress.Decode($"/ip4/127.0.0.1/tcp/{port}")],
                timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
            .ConfigureAwait(false);

        string serverPeerIdentity = new PeerId(serverPeer.Identity.PublicKey).ToString();
        Multiaddress tcpAddress = serverPeer.ListenAddresses.Single(
            address => address.ToString().Contains("/tcp/", StringComparison.Ordinal));
        string serverAddress = tcpAddress.GetPeerId() is null
            ? $"{tcpAddress}/p2p/{serverPeerIdentity}"
            : tcpAddress.ToString();

        clientTrustFixture.Store.TrustPeerAfterExplicitConfirmation(
            serverPeerIdentity,
            explicitlyConfirmed: true);

        await using var client = new Libp2pPeerTransport(
            clientTrustFixture.Store,
            maximumMessageBytes: 1024);

        bool rejected = false;
        try
        {
            await client.ConnectSecureAsync(
                    new PeerTransportEndpoint(serverAddress, serverPeerIdentity),
                    timeout.Token)
                .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is AuthenticationException
                or IOException
                or SocketException
                or InvalidOperationException
                or Libp2pException)
        {
            rejected = true;
        }

        Assert(rejected,
            "A secure libp2p dialer must reject a peer that offers plaintext only when no compatible secure channel can be negotiated.");
        Assert(!client.IsConnected && client.RemotePeerIdentity is null,
            "A failed secure-channel negotiation must never publish a connected or authenticated remote peer.");

        Console.WriteLine(
            "PASS: secure libp2p dialing rejects a plaintext-only peer and does not downgrade to an insecure connection.");
    }

    private static async Task VerifyRejectsUnsupportedSecureChannelProtocolVersionAsync()
    {
        using var clientTrustFixture = new TestPeerTrustStoreFixture();
        using ServiceProvider serverServices = new ServiceCollection()
            .AddLogging()
            .AddLibp2p<UnsupportedTlsVersionPeerFactoryBuilder>()
            .BuildServiceProvider();

        IPeerFactory serverFactory = serverServices.GetRequiredService<IPeerFactory>();
        await using ILocalPeer serverPeer = serverFactory.Create();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        int port = ReserveTcpPort();
        await serverPeer.StartListenAsync(
                [Multiaddress.Decode($"/ip4/127.0.0.1/tcp/{port}")],
                timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
            .ConfigureAwait(false);

        string serverPeerIdentity = new PeerId(serverPeer.Identity.PublicKey).ToString();
        Multiaddress tcpAddress = serverPeer.ListenAddresses.Single(
            address => address.ToString().Contains("/tcp/", StringComparison.Ordinal));
        string serverAddress = tcpAddress.GetPeerId() is null
            ? $"{tcpAddress}/p2p/{serverPeerIdentity}"
            : tcpAddress.ToString();

        clientTrustFixture.Store.TrustPeerAfterExplicitConfirmation(
            serverPeerIdentity,
            explicitlyConfirmed: true);

        await using var client = new Libp2pPeerTransport(
            clientTrustFixture.Store,
            maximumMessageBytes: 1024);

        bool rejected = false;
        try
        {
            await client.ConnectSecureAsync(
                    new PeerTransportEndpoint(serverAddress, serverPeerIdentity),
                    timeout.Token)
                .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is AuthenticationException
                or IOException
                or SocketException
                or PeerConnectionException
                or InvalidOperationException
                or Libp2pException)
        {
            rejected = true;
        }

        Assert(rejected,
            "A secure libp2p client must reject a peer that advertises only an unsupported secure-channel protocol ID/version.");
        Assert(!client.IsConnected && client.RemotePeerIdentity is null,
            "A failed secure-channel protocol negotiation must never publish a connected or authenticated remote peer.");

        Console.WriteLine(
            "PASS: secure libp2p negotiation rejects an unsupported secure-channel protocol ID/version without an insecure connection.");
    }

    private static async Task VerifyQuicFallbackAfterUnreachableTcpAddressAsync()
    {
        var fallbackClientIdentity = new Identity();
        string fallbackClientPeerId = new PeerId(fallbackClientIdentity.PublicKey).ToString();
        using var fallbackClientTrustFixture = new TestPeerTrustStoreFixture();
        using var fallbackServerTrustFixture = new TestPeerTrustStoreFixture();
        await using var server = new Libp2pPeerTransport(
            fallbackServerTrustFixture.Store,
            expectedRemotePeerIdentity: fallbackClientPeerId,
            maximumMessageBytes: 1024);
        fallbackServerTrustFixture.Store.TrustPeerAfterExplicitConfirmation(fallbackClientPeerId, explicitlyConfirmed: true);
        fallbackClientTrustFixture.Store.TrustPeerAfterExplicitConfirmation(server.LocalPeerIdentity, explicitlyConfirmed: true);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        int listenPort = ReserveTcpPort();
        await server.StartListeningAsync(IPAddress.Loopback, listenPort, timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token).ConfigureAwait(false);

        string tcpAddress = server.ListenAddresses.Single(address => address.Contains("/tcp/", StringComparison.Ordinal));
        string quicAddress = server.ListenAddresses.Single(address => address.Contains("/quic-v1", StringComparison.Ordinal));
        int closedPort = ReserveTcpPort();
        string unavailableTcpAddress = ReplaceAddressPort(tcpAddress, "tcp", closedPort);

        var candidates = new[]
        {
            new PeerTransportEndpoint(unavailableTcpAddress, server.LocalPeerIdentity),
            new PeerTransportEndpoint(quicAddress, server.LocalPeerIdentity)
        };

        await using PeerTransportConnectionResult connection = await PeerTransportFallbackConnector.ConnectAsync(
            candidates,
            server.LocalPeerIdentity,
            () => new Libp2pPeerTransport(fallbackClientTrustFixture.Store, localIdentity: fallbackClientIdentity, maximumMessageBytes: 1024),
            timeout.Token).WaitAsync(TimeSpan.FromSeconds(25), timeout.Token).ConfigureAwait(false);

        Assert(connection.FailedAttemptCount == 1,
            "Fallback should report one refused TCP candidate before selecting QUIC.");
        Assert(connection.SelectedEndpoint.Address == quicAddress,
            "The fallback must select the reachable QUIC address after the TCP candidate fails.");
        Assert(connection.Transport.IsConnected &&
               connection.Transport.RemotePeerIdentity == server.LocalPeerIdentity,
            "The dialing transport must authenticate the expected remote peer identity.");

        // The local dialer can finish protocol startup before the remote listener's OnConnected
        // callback publishes RemotePeerIdentity. Await that independent state signal with a bound.
        DateTimeOffset listenerIdentityDeadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (server.RemotePeerIdentity is null && DateTimeOffset.UtcNow < listenerIdentityDeadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token).ConfigureAwait(false);
        }

        Assert(server.RemotePeerIdentity == fallbackClientPeerId,
            "The listener must observe the expected authenticated peer identity before the test proceeds.");

        byte[] requestBytes = [0x10, 0x00, 0xFE, 0x44];
        await connection.Transport.SendMessageAsync(requestBytes, timeout.Token).ConfigureAwait(false);
        byte[] receivedRequest = await server.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        Assert(requestBytes.SequenceEqual(receivedRequest),
            "Binary request bytes must travel directly over the selected peer connection.");

        byte[] responseBytes = [0x20, 0xFD, 0x00, 0x99];
        await server.SendMessageAsync(responseBytes, timeout.Token).ConfigureAwait(false);
        byte[] receivedResponse = await connection.Transport.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        Assert(responseBytes.SequenceEqual(receivedResponse),
            "Binary response bytes must return directly over the selected peer connection.");

        await connection.Transport.DisconnectAsync(timeout.Token).ConfigureAwait(false);
        Assert(!connection.Transport.IsConnected,
            "The selected fallback connection must report disconnected after local teardown.");
    }

    private static async Task VerifyFallbackRejectsMixedPeerIdentitiesBeforeConnectingAsync()
    {
        using var expectedPeerTrustFixture = new TestPeerTrustStoreFixture();
        await using var expectedPeer = new Libp2pPeerTransport(expectedPeerTrustFixture.Store);
        int factoryCalls = 0;
        var candidates = new[]
        {
            new PeerTransportEndpoint("/ip4/127.0.0.1/tcp/41001", expectedPeer.LocalPeerIdentity),
            new PeerTransportEndpoint("/ip4/127.0.0.1/udp/41001/quic-v1", "different-peer-identity")
        };

        try
        {
            await PeerTransportFallbackConnector.ConnectAsync(
                candidates,
                expectedPeer.LocalPeerIdentity,
                () =>
                {
                    factoryCalls++;
                    return new Libp2pPeerTransport(expectedPeerTrustFixture.Store);
                }).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            Assert(factoryCalls == 0,
                "Mixed peer identities must be rejected before creating or dialing a transport.");
            return;
        }

        throw new InvalidOperationException("Fallback accepted a candidate list with mixed expected peer identities.");
    }

    private static string ReplacePeerIdentity(string multiaddress, string replacementPeerIdentity)
    {
        const string peerMarker = "/p2p/";
        int markerIndex = multiaddress.LastIndexOf(peerMarker, StringComparison.Ordinal);
        Assert(markerIndex >= 0,
            "The server multiaddress must include its authenticated /p2p identity segment.");
        return multiaddress[..(markerIndex + peerMarker.Length)] + replacementPeerIdentity;
    }

    private static string ReplaceAddressPort(string multiaddress, string protocol, int replacementPort)
    {
        string[] parts = multiaddress.Split('/');
        int protocolIndex = Array.IndexOf(parts, protocol);
        Assert(protocolIndex >= 0 && protocolIndex + 1 < parts.Length,
            $"The advertised multiaddress does not contain a valid /{protocol}/port segment.");
        parts[protocolIndex + 1] = replacementPort.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return string.Join("/", parts);
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
    private sealed class UnsupportedTlsVersionPeerFactoryBuilder(IServiceProvider? serviceProvider = null)
        : Libp2pPeerFactoryBuilder(serviceProvider)
    {
        protected override ProtocolRef[] Encryption() =>
            [new ProtocolRef(new UnsupportedTlsVersionProtocol())];
    }

    private sealed class UnsupportedTlsVersionProtocol : IProtocol
    {
        public string Id => "/tls/999.0.0";
    }

}

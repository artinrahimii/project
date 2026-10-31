using Google.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Libp2p.Core;
using WhiteP.Host.Windows.Networking;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

internal static class AndroidInteropPeerServer
{
    public static async Task RunAsync(string expectedAndroidPeerId, int port)
    {
        if (string.IsNullOrWhiteSpace(expectedAndroidPeerId))
            throw new ArgumentException("The Android PeerId is required.", nameof(expectedAndroidPeerId));
        if (port is < 1 or > IPEndPoint.MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port));

        using var trustFixture = new TestPeerTrustStoreFixture();
        trustFixture.Store.TrustPeerAfterExplicitConfirmation(
            expectedAndroidPeerId,
            explicitlyConfirmed: true);

        var localIdentity = new Identity();
        await using var server = new Libp2pPeerTransport(
            trustFixture.Store,
            expectedRemotePeerIdentity: expectedAndroidPeerId,
            localIdentity: localIdentity);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await server.StartListeningAsync(IPAddress.Loopback, port, timeout.Token).ConfigureAwait(false);

        string tcpAddress = server.ListenAddresses.Single(
            address => address.Contains("/tcp/", StringComparison.Ordinal));
        Console.WriteLine($"READY\t{server.LocalPeerIdentity}\t{tcpAddress}");
        Console.Out.Flush();

        while (!timeout.IsCancellationRequested && !server.IsConnected)
            await Task.Delay(TimeSpan.FromMilliseconds(25), timeout.Token).ConfigureAwait(false);

        byte[] requestWire = await server.ReceiveMessageAsync(timeout.Token).ConfigureAwait(false);
        var parsed = ProtocolMessageParser.ParseInferenceRequest(requestWire);
        if (!parsed.IsValid || parsed.Message is null)
            throw new InvalidDataException("The Android peer sent an invalid inference request.");

        InferenceRequest request = parsed.Message;
        var response = new InferenceResponse
        {
            RequestId = request.RequestId,
            AttemptId = request.AttemptId,
            EphemeralHostId = server.LocalPeerIdentity,
            Outcome = InferenceOutcome.Succeeded,
            OutputText = "dotnet-host-response",
            CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            ModelId = request.ModelId,
            ErrorMessage = string.Empty
        };
        PeerResponseCorrelationValidator.Validate(request, response, server.LocalPeerIdentity);

        await server.SendMessageAsync(response.ToByteArray(), timeout.Token).ConfigureAwait(false);
        Console.WriteLine($"PASS\t{request.RequestId}\t{request.AttemptId}");
        Console.Out.Flush();
    }
}

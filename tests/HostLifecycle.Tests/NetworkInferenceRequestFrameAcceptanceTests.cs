using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using WhiteP.Host.Windows.Networking;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

internal static class NetworkInferenceRequestFrameAcceptanceTests
{
    public static async Task RunAsync()
    {
        await ValidLengthPrefixedRequestReachesTheSharedParserAsync().ConfigureAwait(false);
        await InvalidFramesAreRejectedAsync().ConfigureAwait(false);
        await ParticipationInactiveAtDispatchDoesNotInvokeCallbackAsync().ConfigureAwait(false);
        await ShutdownCancelsAClientWaitingForItsFrameAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: bounded TCP frames reach the shared Protobuf parser; malformed, truncated, oversized and interrupted frames are rejected.");
    }
    private static async Task ValidLengthPrefixedRequestReachesTheSharedParserAsync()
    {
        var parsedRequest = new TaskCompletionSource<InferenceRequest>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var listener = new ParticipationGatedNetworkListener(() => true, IPAddress.Loopback, 0, (request, _) => { parsedRequest.TrySetResult(request); return Task.CompletedTask; });
        await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var endpoint = listener.LocalEndpoint ?? throw new InvalidOperationException("Listener endpoint missing.");
        using var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await client.GetStream().WriteAsync(EncodeFrame(ValidRequest().ToByteArray()).AsMemory()).ConfigureAwait(false);
        var received = await parsedRequest.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(received.RequestId == "network-request" && received.AttemptId == "network-attempt" && received.Prompt == "framed prompt", "Unexpected parsed request fields.");
        await AssertPeerClosedAsync(client).ConfigureAwait(false);
    }
    private static async Task InvalidFramesAreRejectedAsync()
    {
        await using var listener = new ParticipationGatedNetworkListener(() => true, IPAddress.Loopback, 0);
        await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var endpoint = listener.LocalEndpoint ?? throw new InvalidOperationException("Listener endpoint missing.");
        await SendAndExpectCloseAsync(endpoint, new byte[] { 0, 0, 0, 0 }).ConfigureAwait(false);
        var oversized = new byte[NetworkInferenceRequestFrameReader.LengthPrefixBytes];
        BinaryPrimitives.WriteUInt32BigEndian(oversized, (uint)(ProtocolMessageLimits.MaximumRequestMessageBytes + 1));
        await SendAndExpectCloseAsync(endpoint, oversized).ConfigureAwait(false);
        await SendAndExpectCloseAsync(endpoint, EncodeFrame(new byte[] { 0xff })).ConfigureAwait(false);
        using (var client = await ConnectAsync(endpoint).ConfigureAwait(false))
        {
            await client.GetStream().WriteAsync(new byte[] { 0, 0 }.AsMemory()).ConfigureAwait(false);
            client.Client.Shutdown(SocketShutdown.Send);
            await AssertPeerClosedAsync(client).ConfigureAwait(false);
        }
        using (var client = await ConnectAsync(endpoint).ConfigureAwait(false))
        {
            var header = new byte[NetworkInferenceRequestFrameReader.LengthPrefixBytes];
            BinaryPrimitives.WriteUInt32BigEndian(header, 10);
            await client.GetStream().WriteAsync(header.AsMemory()).ConfigureAwait(false);
            await client.GetStream().WriteAsync(new byte[] { 8, 1 }.AsMemory()).ConfigureAwait(false);
            client.Client.Shutdown(SocketShutdown.Send);
            await AssertPeerClosedAsync(client).ConfigureAwait(false);
        }
    }
    private static async Task ParticipationInactiveAtDispatchDoesNotInvokeCallbackAsync()
    {
        var participationActive = true;
        var callbackInvoked = false;
        await using var listener = new ParticipationGatedNetworkListener(
            () => participationActive,
            IPAddress.Loopback,
            0,
            (_, _) =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            });
        await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var endpoint = listener.LocalEndpoint ?? throw new InvalidOperationException("Listener endpoint missing.");

        // Simulate revocation while the listener is already bound.
        participationActive = false;
        using var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await client.GetStream().WriteAsync(EncodeFrame(ValidRequest().ToByteArray()).AsMemory()).ConfigureAwait(false);
        await AssertPeerClosedAsync(client).ConfigureAwait(false);
        Assert(!callbackInvoked, "The listener must recheck local participation before dispatching an accepted request.");
    }

    private static async Task ShutdownCancelsAClientWaitingForItsFrameAsync()
    {
        var listener = new ParticipationGatedNetworkListener(() => true, IPAddress.Loopback, 0);
        await listener.StartAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var endpoint = listener.LocalEndpoint ?? throw new InvalidOperationException("Listener endpoint missing.");
        using var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await WaitUntilAsync(() => listener.ActiveConnectionCount == 1, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await listener.StopAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(!listener.IsListening && listener.LocalEndpoint is null && listener.ActiveConnectionCount == 0, "Listener stop must cancel reads and release sockets.");
        await AssertPeerClosedAsync(client).ConfigureAwait(false);
        await listener.DisposeAsync().ConfigureAwait(false);
    }
    private static InferenceRequest ValidRequest() => new() { ProtocolVersion = ProtocolVersionPolicy.CurrentVersion, RequestId = "network-request", AttemptId = "network-attempt", ModelId = "fixture-model", Prompt = "framed prompt", DeadlineUnixMs = 4_102_444_800_000UL, MaxOutputTokens = 64 };
    private static byte[] EncodeFrame(byte[] payload)
    {
        var frame = new byte[NetworkInferenceRequestFrameReader.LengthPrefixBytes + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(frame.AsSpan(0, NetworkInferenceRequestFrameReader.LengthPrefixBytes), (uint)payload.Length);
        payload.CopyTo(frame, NetworkInferenceRequestFrameReader.LengthPrefixBytes);
        return frame;
    }
    private static async Task<TcpClient> ConnectAsync(IPEndPoint endpoint)
    {
        var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        return client;
    }
    private static async Task SendAndExpectCloseAsync(IPEndPoint endpoint, byte[] bytes)
    {
        using var client = await ConnectAsync(endpoint).ConfigureAwait(false);
        await client.GetStream().WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
        await AssertPeerClosedAsync(client).ConfigureAwait(false);
    }
    private static async Task AssertPeerClosedAsync(TcpClient client)
    {
        NetworkStream stream;
        try
        {
            stream = client.GetStream();
        }
        catch (InvalidOperationException)
        {
            // A reset/closed socket may cease to be connected before GetStream is requested.
            return;
        }

        try
        {
            var read = await stream.ReadAsync(new byte[1].AsMemory()).AsTask()
                .WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Assert(read == 0, "Server left a one-frame connection open.");
        }
        catch (IOException) { }
        catch (SocketException) { }
    }
    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition()) { if (DateTime.UtcNow >= deadline) throw new TimeoutException("Listener did not accept connection."); await Task.Delay(10).ConfigureAwait(false); }
    }
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

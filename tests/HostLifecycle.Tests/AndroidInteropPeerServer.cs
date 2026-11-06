using Google.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethermind.Libp2p.Core;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Networking;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;

internal static class AndroidInteropPeerServer
{
    public static async Task RunAsync(string expectedAndroidPeerId, int port, bool waitForCancellation = false, IPAddress? bindAddress = null, bool silentResponse = false)
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
            localIdentity: localIdentity,
            diagnosticLoggerProvider: new InteropDiagnosticLoggerProvider());

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        IPAddress listenerAddress = bindAddress ?? IPAddress.Loopback;
        if (bindAddress is not null)
        {
            if (bindAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                IPAddress.IsLoopback(bindAddress) ||
                IPAddress.Any.Equals(bindAddress))
                throw new ArgumentException("LAN acceptance requires one explicit, non-loopback IPv4 interface address.", nameof(bindAddress));

            bool isAssignedLocalAddress = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
                .Any(unicastAddress => unicastAddress.Address.Equals(bindAddress));
            if (!isAssignedLocalAddress)
                throw new ArgumentException("The requested LAN bind address is not assigned to this Windows host.", nameof(bindAddress));
        }

        await server.StartListeningAsync(listenerAddress, port, timeout.Token).ConfigureAwait(false);

        string tcpAddress = server.ListenAddresses.Single(
            address => address.Contains("/tcp/", StringComparison.Ordinal));
        Console.WriteLine($"READY\t{server.LocalPeerIdentity}\t{tcpAddress}");
        Console.Out.Flush();

        Console.WriteLine("WAITING_FOR_REMOTE_SESSION");
        Console.Out.Flush();
        while (!timeout.IsCancellationRequested && server.RemotePeerIdentity is null)
            await Task.Delay(TimeSpan.FromMilliseconds(25), timeout.Token).ConfigureAwait(false);
        if (server.RemotePeerIdentity is null)
            throw new TimeoutException("The Android peer did not establish an authenticated libp2p session.");
        Console.WriteLine("SESSION_AUTHENTICATED");
        Console.Out.Flush();

        while (!timeout.IsCancellationRequested && !server.IsConnected)
            await Task.Delay(TimeSpan.FromMilliseconds(25), timeout.Token).ConfigureAwait(false);
        if (!server.IsConnected)
            throw new TimeoutException("The Android peer did not negotiate the White-P application protocol.");
        Console.WriteLine("APPLICATION_PROTOCOL_READY");
        Console.Out.Flush();

        byte[] requestWire = await server.ReceiveMessageAsync(timeout.Token).ConfigureAwait(false);
        Console.WriteLine("RECEIVED");
        Console.Out.Flush();
        var parsed = ProtocolMessageParser.ParseInferenceRequest(requestWire);
        if (!parsed.IsValid || parsed.Message is null)
            throw new InvalidDataException("The Android peer sent an invalid inference request.");

        InferenceRequest request = parsed.Message;
        InferenceResponse response;
        if (waitForCancellation)
        {
            Console.WriteLine("CANCEL_WAITING");
            Console.Out.Flush();
            byte[] cancelWire = await server.ReceiveMessageAsync(timeout.Token).ConfigureAwait(false);
            try
            {
                var parsedCancel = ProtocolMessageParser.ParseCancelRequest(cancelWire);
                if (!parsedCancel.IsValid || parsedCancel.Message is null)
                    throw new InvalidDataException("The Android peer sent an invalid CancelRequest.");

                CancelRequest cancel = parsedCancel.Message!;
                if (!StringComparer.Ordinal.Equals(cancel.RequestId, request.RequestId) ||
                    !StringComparer.Ordinal.Equals(cancel.AttemptId, request.AttemptId) ||
                    !StringComparer.Ordinal.Equals(cancel.Reason, ProtocolMessageValidator.UserCancellationReason))
                    throw new InvalidDataException("The CancelRequest did not match the active Android request/attempt.");

                response = new InferenceResponse
                {
                    RequestId = request.RequestId,
                    AttemptId = request.AttemptId,
                    RoundId = request.RoundId,
                    EphemeralHostId = server.LocalPeerIdentity,
                    Outcome = InferenceOutcome.Cancelled,
                    OutputText = string.Empty,
                    CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                    ModelId = request.ModelId,
                    ErrorMessage = "The request was cancelled by the remote client."
                };
                Console.WriteLine("CANCEL_RECEIVED");
            }
            finally
            {
                Array.Clear(cancelWire, 0, cancelWire.Length);
            }
        }
        else if (silentResponse)
        {
            // Keep the authenticated peer connected but deliberately withhold its inference response.
            // The cross-runtime retry test stops these peers after the shared Android round window expires.
            Console.WriteLine("RECEIVED_WITHOUT_RESPONSE");
            Console.Out.Flush();
            await Task.Delay(Timeout.Infinite, timeout.Token).ConfigureAwait(false);
            return;
        }
        else
        {
            response = new InferenceResponse
            {
                RequestId = request.RequestId,
                AttemptId = request.AttemptId,
                RoundId = request.RoundId,
                EphemeralHostId = server.LocalPeerIdentity,
                Outcome = InferenceOutcome.Succeeded,
                OutputText = "dotnet-host-response",
                CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                ModelId = request.ModelId,
                ErrorMessage = string.Empty
            };
        }

        PeerResponseCorrelationValidator.Validate(request, response, server.LocalPeerIdentity);
        Console.WriteLine("VALIDATED");
        Console.Out.Flush();

        byte[] responseWire = response.ToByteArray();
        int responseFirstTag = responseWire.Length > 0 ? responseWire[0] : -1;
        Console.WriteLine($"SENT_META\t{responseWire.Length}\t{responseFirstTag}");
        Console.Out.Flush();
        try
        {
            await server.SendMessageAsync(responseWire, timeout.Token).ConfigureAwait(false);
        }
        finally
        {
            Array.Clear(responseWire, 0, responseWire.Length);
        }
        Console.WriteLine("SENT");
        Console.WriteLine("PASS");
        Console.Out.Flush();

        // Keep the one-shot fixture alive until the Android client has consumed the
        // response and closes its stream from awaitResponse's finally block.
        try
        {
            _ = await server.ReceiveMessageAsync(timeout.Token).ConfigureAwait(false);
            throw new InvalidDataException("The Android client sent an unexpected application message after the response.");
        }
        catch (IOException) when (!timeout.IsCancellationRequested)
        {
            // Expected acknowledgement-by-close after the client has completed response handling.
        }
    }
    private sealed class InteropDiagnosticLoggerProvider : ILoggerProvider
    {
        private int _written;

        public ILogger CreateLogger(string _) =>
            new InteropDiagnosticLogger(() => Interlocked.Increment(ref _written) <= 80);

        public void Dispose()
        {
        }

        private sealed class InteropDiagnosticLogger(Func<bool> allowWrite) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel) || !allowWrite())
                    return;

                Console.WriteLine($"P2P_DIAG\t{(int)logLevel}\t{eventId.Id}");
                Console.Out.Flush();
            }
        }
    }
}

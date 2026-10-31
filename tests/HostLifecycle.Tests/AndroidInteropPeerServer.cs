using Google.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
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
            localIdentity: localIdentity,
            diagnosticLoggerProvider: new InteropDiagnosticLoggerProvider());

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await server.StartListeningAsync(IPAddress.Loopback, port, timeout.Token).ConfigureAwait(false);

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
        var response = new InferenceResponse
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
        PeerResponseCorrelationValidator.Validate(request, response, server.LocalPeerIdentity);
        Console.WriteLine("VALIDATED");
        Console.Out.Flush();

        await server.SendMessageAsync(response.ToByteArray(), timeout.Token).ConfigureAwait(false);
        Console.WriteLine("SENT");
        Console.WriteLine("PASS");
        Console.Out.Flush();
    }
    private sealed class InteropDiagnosticLoggerProvider : ILoggerProvider
    {
        private int _written;

        public ILogger CreateLogger(string categoryName) =>
            new InteropDiagnosticLogger(categoryName, () => Interlocked.Increment(ref _written) <= 80);

        public void Dispose()
        {
        }

        private sealed class InteropDiagnosticLogger(string categoryName, Func<bool> allowWrite) : ILogger
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

                string template =
                    state is IEnumerable<KeyValuePair<string, object?>> values
                        ? values.FirstOrDefault(static item => item.Key == "{OriginalFormat}").Value?.ToString()
                            ?? eventId.Name
                            ?? "<no-template>"
                        : eventId.Name ?? "<no-template>";
                string safeCategory = categoryName.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
                string safeTemplate = template.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

                Console.WriteLine(
                    $"P2P_DIAG\t{safeCategory}\t{logLevel}\t{eventId.Id}\t{safeTemplate}\t{exception?.GetType().Name ?? "none"}");
                Console.Out.Flush();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Nethermind.Libp2p.Core;
using HostCapacityState = WhiteP.Host.Domain.HostCapacityState;
using HostState = WhiteP.Host.Domain.HostState;
using WhiteP.Host.Application;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Networking;
using WhiteP.Host.Windows.Requests;
using WhiteP.Rendezvous.Service;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.Networking;
using WhiteP.Shared.Contracts.V1;

internal static class PeerAnnouncementClientAcceptanceTests
{
    public static async Task RunAsync()
    {
        VerifyContractSurfaceIsMetadataOnly();
        NonLoopbackPlainHttpIsRejected();
        LeasePublisherConfigurationIsValidated();
        await RealAnnouncementAndCapabilityDiscoveryWorkOverLoopbackAsync().ConfigureAwait(false);
        await PeriodicLeaseRenewalRefreshesTtlAndStopsAsync().ConfigureAwait(false);
        await PeerHostLifecyclePublishesAndServesTrustedPeerAsync().ConfigureAwait(false);
        await SensitiveInferencePayloadFieldsAreRejectedByTheControlPlaneAsync().ConfigureAwait(false);
        await OversizedControlMessagesAreRejectedAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: metadata-only discovery, lease configuration validation, periodic TTL renewal/expiry, unknown-field rejection and control-message bounds.");
    }

    private static void VerifyContractSurfaceIsMetadataOnly()
    {
        string[] properties = typeof(PeerAnnouncement).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        Assert(properties.SequenceEqual(["Addresses", "Capabilities", "ExpiresAtUtc", "PeerId"]),
            "PeerAnnouncement must expose only peer ID, addresses, capabilities and expiry.");
        string[] methods = typeof(PeerAnnouncementClient).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        Assert(methods.SequenceEqual(["AnnounceAsync", "DiscoverByCapabilityAsync"]),
            "The rendezvous client must expose only metadata announcement and capability discovery operations.");
        ExpectArgumentException(() => PeerAnnouncement.ValidateCapabilityName(string.Concat("inference", (char)10, "request")),
            "Capability names containing control characters must be rejected.");
        string expectedPeer = CreatePeerId(), otherPeer = CreatePeerId();
        ExpectArgumentException(() => new PeerAnnouncement(expectedPeer,
            [$"/ip4/127.0.0.1/tcp/43001/p2p/{otherPeer}"], ["inference.v1"], DateTimeOffset.UtcNow.AddMinutes(5)),
            "An address carrying a different PeerId must be rejected.");
    }

    private static async Task RealAnnouncementAndCapabilityDiscoveryWorkOverLoopbackAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
            Assert(addresses is not null && addresses.Count == 1, "The rendezvous service did not bind one loopback address.");
            var serviceUri = new Uri(addresses!.Single(), UriKind.Absolute);
            using var httpClient = new HttpClient();
            var client = new PeerAnnouncementClient(httpClient, serviceUri);
            var identity = new Identity();
            string peerId = new PeerId(identity.PublicKey).ToString();
            var first = new PeerAnnouncement(peerId,
                [$"/ip4/127.0.0.1/tcp/43001/p2p/{peerId}", $"/ip4/127.0.0.1/udp/43001/quic-v1/p2p/{peerId}"],
                ["inference.v1", "backend.local"], DateTimeOffset.UtcNow.AddMinutes(10));

            await client.AnnounceAsync(first).ConfigureAwait(false);
            var found = await client.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
            Assert(found.Count == 1 && found[0].PeerId == peerId, "Capability discovery did not return the announced peer.");
            Assert(found[0].Addresses.Count == 2 &&
                found[0].Addresses.Any(a => a.Contains("/tcp/", StringComparison.Ordinal)) &&
                found[0].Addresses.Any(a => a.Contains("/quic-v1", StringComparison.Ordinal)),
                "Discovery did not preserve the announced direct TCP and QUIC addresses.");
            Assert(found[0].SupportsCapability("backend.local"), "Declared capability metadata was not preserved.");
            using var rawDiscoveryResponse = await httpClient.GetAsync(
                new Uri(serviceUri, "v1/peers?capability=inference.v1")).ConfigureAwait(false);
            Assert(rawDiscoveryResponse.StatusCode == HttpStatusCode.OK,
                "The metadata-only discovery endpoint must return the announced peer.");
            string rawDiscoveryJson = await rawDiscoveryResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var discoveryDocument = JsonDocument.Parse(rawDiscoveryJson);
            string[] discoveredFields = discoveryDocument.RootElement[0]
                .EnumerateObject()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            Assert(discoveredFields.SequenceEqual(["addresses", "capabilities", "expiresAtUtc", "peerId"]),
                "The introducer response must expose only peer ID, direct addresses, capabilities and expiry.");
            Assert(!rawDiscoveryJson.Contains("prompt", StringComparison.OrdinalIgnoreCase) &&
                   !rawDiscoveryJson.Contains("capsule", StringComparison.OrdinalIgnoreCase),
                "The introducer response must not contain inference prompt or context-capsule fields.");
            Assert((await client.DiscoverByCapabilityAsync("embeddings.v1").ConfigureAwait(false)).Count == 0,
                "Discovery returned a peer that did not announce the requested capability.");

            var replacement = new PeerAnnouncement(peerId, [$"/ip4/127.0.0.1/tcp/43002/p2p/{peerId}"],
                ["inference.v1"], DateTimeOffset.UtcNow.AddMinutes(5));
            await client.AnnounceAsync(replacement).ConfigureAwait(false);
            var updated = await client.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
            Assert(updated.Count == 1 && updated[0].Addresses.Single().Contains("/tcp/43002", StringComparison.Ordinal),
                "A new announcement should replace current metadata rather than retain history.");

            using var noGetInference = await httpClient.GetAsync(new Uri(serviceUri, "v1/inference")).ConfigureAwait(false);
            Assert(noGetInference.StatusCode == HttpStatusCode.NotFound, "The service must not expose a central inference endpoint.");
            using var noPostInference = await httpClient.PostAsync(new Uri(serviceUri, "v1/inference"),
                new StringContent("{\"prompt\":\"not accepted\"}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            Assert(noPostInference.StatusCode == HttpStatusCode.NotFound, "The service must not accept inference content on any route.");
        }
        finally { await app.StopAsync().ConfigureAwait(false); }
    }

    private static async Task PeriodicLeaseRenewalRefreshesTtlAndStopsAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var serviceUri = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single(), UriKind.Absolute);
            using var httpClient = new HttpClient();
            var client = new PeerAnnouncementClient(httpClient, serviceUri);
            string peerId = CreatePeerId();
            var template = new PeerAnnouncement(
                peerId,
                [$"/ip4/127.0.0.1/tcp/43011/p2p/{peerId}"],
                ["inference.v1"],
                DateTimeOffset.UtcNow.AddMinutes(1));

            await using var lease = new PeerAnnouncementLeasePublisher(
                client,
                template,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(200));
            await lease.StartAsync().ConfigureAwait(false);
            Assert(lease.IsRunning, "The lease publisher should start after its first announcement succeeds.");

            var first = await client.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
            Assert(first.Count == 1 && first[0].PeerId == peerId, "Starting the lease should publish the peer immediately.");
            DateTimeOffset firstExpiry = first[0].ExpiresAtUtc;

            await Task.Delay(TimeSpan.FromMilliseconds(450)).ConfigureAwait(false);
            var renewed = await client.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
            Assert(renewed.Count == 1 && renewed[0].ExpiresAtUtc > firstExpiry,
                "Periodic renewal should replace the current announcement with a later expiry.");
            Assert(lease.LastRenewalFailureType is null, "A successful renewal should leave no recorded transient failure.");

            await lease.StopAsync().ConfigureAwait(false);
            Assert(!lease.IsRunning, "Stopping the lease should cancel its periodic renewal loop.");
            await Task.Delay(TimeSpan.FromMilliseconds(2350)).ConfigureAwait(false);
            Assert((await client.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false)).Count == 0,
                "After renewal stops, the directory should stop returning the peer when its last TTL expires.");
        }
        finally
        {
            await app.StopAsync().ConfigureAwait(false);
        }
    }

    private static async Task PeerHostLifecyclePublishesAndServesTrustedPeerAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);

        var server = app.Services.GetRequiredService<IServer>();
        var serviceUri = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single(), UriKind.Absolute);
        using var directoryHttpClient = new HttpClient();
        var directoryClient = new PeerAnnouncementClient(directoryHttpClient, serviceUri);
        using var hostTrust = new TestPeerTrustStoreFixture();
        using var clientTrust = new TestPeerTrustStoreFixture();
        var hostIdentity = new Identity();
        var clientIdentity = new Identity();
        string hostPeerId = new PeerId(hostIdentity.PublicKey).ToString();
        string clientPeerId = new PeerId(clientIdentity.PublicKey).ToString();
        hostTrust.Store.TrustPeerAfterExplicitConfirmation(clientPeerId, explicitlyConfirmed: true);
        clientTrust.Store.TrustPeerAfterExplicitConfirmation(hostPeerId, explicitlyConfirmed: true);

        bool unverifiedAddressRejected = false;
        try
        {
            _ = new PeerAnnouncementHostLifecycle(
                hostTrust.Store,
                hostIdentity,
                clientPeerId,
                serviceUri,
                IPAddress.Loopback,
                IPAddress.Loopback,
                ReservePort(),
                advertisedAddressVerified: false,
                (bytes, _) => Task.FromResult(bytes.ToArray()),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(150));
        }
        catch (ArgumentException)
        {
            unverifiedAddressRejected = true;
        }
        Assert(unverifiedAddressRejected,
            "The host must refuse to publish an advertised address unless its reachability has been independently confirmed.");

        var requestHandlerCalls = 0;
        var backendHttpHandler = new CancellationProbeHttpHandler();
        using var backendHttpClient = new HttpClient(backendHttpHandler);
        var backendBaseUri = new Uri("http://127.0.0.1:43127/");
        var inferenceBackend = new LlamaCppServerBackend(
            backendBaseUri,
            "fixture-model",
            backendHttpClient,
            TimeSpan.FromSeconds(20));
        await using var inferenceSession = new HostInferenceSession(
            backendFactory: (_, _) => inferenceBackend,
            readinessEvaluator: _ => Task.FromResult(
                new HostReadinessResult(true, "The authenticated cancellation test backend is ready.")));
        var connectedStatus = await inferenceSession.ConnectAsync(backendBaseUri, "fixture-model").ConfigureAwait(false);
        Assert(connectedStatus.State == HostState.Ready,
            "The production llama.cpp backend adapter must pass health and readiness checks before peer hosting.");
        var requestCoordinator = new HostRequestCoordinator();

        await using var host = new PeerAnnouncementHostLifecycle(
            hostTrust.Store,
            hostIdentity,
            clientPeerId,
            serviceUri,
            IPAddress.Loopback,
            IPAddress.Loopback,
            ReservePort(),
            advertisedAddressVerified: true,
            async (bytes, cancellationToken) =>
            {
                var parsed = ProtocolMessageParser.ParseInferenceRequest(bytes);
                if (!parsed.IsValid || parsed.Message is null)
                    throw new InvalidDataException("The loopback test supplied an invalid inference request.");

                Interlocked.Increment(ref requestHandlerCalls);
                var requestMessage = parsed.Message;
                var runtimeSnapshot = await inferenceSession.GetFreshPeerRuntimeSnapshotAsync(cancellationToken).ConfigureAwait(false);
                var modelCapability = runtimeSnapshot.ModelCapability;
                var admission = new HostRequestAdmissionContext(
                    HasActiveParticipation: true,
                    HostState: runtimeSnapshot.Status.State,
                    RequestedModelId: requestMessage.ModelId,
                    ReadyModelId: modelCapability?.ModelId,
                    CapacityState: HostCapacityState.Ready,
                    RequestedMaxOutputTokens: requestMessage.MaxOutputTokens,
                    ReadyModelMaxOutputTokens: modelCapability?.MaxOutputTokens ?? 0UL,
                    ReadyModelMaxContextTokens: modelCapability?.MaxContextTokens ?? 0UL,
                    DeadlineUnixMs: requestMessage.DeadlineUnixMs);
                var outcome = await requestCoordinator.TryExecuteAsync(
                    admission,
                    token => inferenceSession.GenerateAsync(requestMessage, token),
                    cancellationToken).ConfigureAwait(false);
                if (outcome.Status != HostRequestStatus.Completed || outcome.Result is null)
                    throw new InvalidOperationException("Peer inference did not complete through the production request coordinator.");

                var responseMessage = new InferenceResponse
                {
                    RequestId = requestMessage.RequestId,
                    AttemptId = requestMessage.AttemptId,
                    RoundId = requestMessage.RoundId,
                    EphemeralHostId = hostPeerId,
                    Outcome = InferenceOutcome.Succeeded,
                    OutputText = outcome.Result,
                    CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                    ModelId = requestMessage.ModelId,
                    ErrorMessage = string.Empty
                };
                return responseMessage.ToByteArray();
            },
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(150),
            statusProvider: _ => Task.FromResult(new HostStatus
            {
                State = (HostRuntimeState)2,
                ActiveRequests = 0
            }));

        await host.StartAsync().WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert(host.IsActive, "The host lifecycle should become active only after listener startup and the initial rendezvous announcement succeed.");

        var announcements = await directoryClient.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
        var announcement = announcements.SingleOrDefault(peer => peer.PeerId == hostPeerId);
        Assert(announcement is not null, "Starting the host lifecycle must immediately publish its stable local PeerId.");
        Assert(announcement!.Addresses.Count == 2 &&
               announcement.Addresses.Any(address => address.Contains("/tcp/", StringComparison.Ordinal)) &&
               announcement.Addresses.Any(address => address.Contains("/quic-v1", StringComparison.Ordinal)),
            "The announcement must advertise direct TCP and QUIC addresses carrying the exact local PeerId.");
        Assert(announcement.ExpiresAtUtc > DateTimeOffset.UtcNow &&
               announcement.SupportsCapability("inference.v1"),
            "The first host announcement must expose a live expiry and the inference capability.");

        await using var client = new Libp2pPeerTransport(
            clientTrust.Store,
            localIdentity: clientIdentity,
            maximumMessageBytes: 1024);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var endpoint = new PeerTransportEndpoint(
            announcement.Addresses.Single(address => address.Contains("/tcp/", StringComparison.Ordinal)),
            hostPeerId);
        await client.ConnectSecureAsync(endpoint, timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(20), timeout.Token).ConfigureAwait(false);
        Assert(client.IsConnected && host.IsActive,
            "A separately instantiated trusted peer must establish an authenticated encrypted connection to the published host.");

        long statusProbeStartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] statusRequestWire = new HostStatusRequest { EphemeralHostId = hostPeerId }.ToByteArray();
        int statusRequestBytes = statusRequestWire.Length;
        int statusRequestFirstTag = statusRequestWire.Length > 0 ? statusRequestWire[0] : -1;
        try
        {
            await client.SendMessageAsync(statusRequestWire, timeout.Token).ConfigureAwait(false);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(statusRequestWire);
        }

        byte[] statusWire;
        try
        {
            statusWire = await client.ReceiveMessageAsync(timeout.Token)
                .WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Authenticated HostStatus round-trip failed. " +
                $"hostActive={host.IsActive}; " +
                $"hostFailureType={host.LastFailureType ?? "none"}; " +
                $"hostFailureStage={host.LastFailureStage ?? "none"}; " +
                $"hostFailureParseSummary={host.LastFailureParseSummary ?? "none"}; " +
                $"hostFailureMessageBytes={host.LastFailureMessageBytes}; " +
                $"hostFailureFirstTag={host.LastFailureFirstTag}; " +
                $"statusRequestBytes={statusRequestBytes}; " +
                $"statusRequestFirstTag={statusRequestFirstTag}; " +
                $"clientConnected={client.IsConnected}; " +
                $"failureType={exception.GetType().Name}.",
                exception);
        }
        var liveStatus = HostStatus.Parser.ParseFrom(statusWire);
        Assert(liveStatus.EphemeralHostId == hostPeerId &&
               liveStatus.State == (HostRuntimeState)2 &&
               liveStatus.ActiveRequests == 0,
            "The authenticated peer must return the current host identity, READY state and active-request count.");
        Assert(liveStatus.ObservedAtUnixMs >= (ulong)Math.Max(1L, statusProbeStartedAt) &&
               liveStatus.ObservedAtUnixMs <= (ulong)DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeMilliseconds(),
            "HostStatus must carry a fresh server-generated observation timestamp, not a cached announcement timestamp.");

        var requestMessage = new InferenceRequest
        {
            ProtocolVersion = ProtocolVersionPolicy.CurrentVersion,
            RequestId = SecureRequestIdGenerator.CreateRequestId(),
            AttemptId = "attempt-loopback-001",
            RoundId = "round-loopback-001",
            ModelId = "fixture-model",
            Prompt = "peer security acceptance request",
            DeadlineUnixMs = (ulong)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds(),
            MaxOutputTokens = 32
        };
        byte[] request = requestMessage.ToByteArray();
        await client.SendMessageAsync(request, timeout.Token).ConfigureAwait(false);
        byte[] responseWire = await client.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token).ConfigureAwait(false);
        var response = InferenceResponse.Parser.ParseFrom(responseWire);
        Assert(response.RequestId == requestMessage.RequestId &&
               response.AttemptId == requestMessage.AttemptId &&
               response.RoundId == requestMessage.RoundId &&
               response.EphemeralHostId == hostPeerId &&
               response.Outcome == InferenceOutcome.Succeeded &&
               response.OutputText == "loopback-inference-result",
            "The host must return a protocol-valid response correlated to the authenticated request. " +
            $"requestMatch={response.RequestId == requestMessage.RequestId}; " +
            $"attemptMatch={response.AttemptId == requestMessage.AttemptId}; " +
            $"roundMatch={response.RoundId == requestMessage.RoundId}; " +
            $"peerMatch={response.EphemeralHostId == hostPeerId}; " +
            $"outcome={response.Outcome}; " +
            $"outputMatch={response.OutputText == "loopback-inference-result"}; " +
            $"modelMatch={response.ModelId == requestMessage.ModelId}; " +
            $"completedAtWithinDeadline={response.CompletedAtUnixMs <= requestMessage.DeadlineUnixMs}.");

        await client.SendMessageAsync(request, timeout.Token).ConfigureAwait(false);
        byte[] replayWire = await client.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token).ConfigureAwait(false);
        var replayResponse = InferenceResponse.Parser.ParseFrom(replayWire);
        Assert(replayResponse.Outcome == InferenceOutcome.Rejected &&
               replayResponse.RequestId == requestMessage.RequestId &&
               replayResponse.AttemptId == requestMessage.AttemptId &&
               replayResponse.ErrorMessage == "A duplicate or untracked request was rejected.",
            "Replaying the same authenticated request/attempt tuple must be rejected without a second handler invocation.");
        Assert(Volatile.Read(ref requestHandlerCalls) == 1,
            "An exact replay must not reach the inference handler twice.");

        var cancellableRequest = requestMessage.Clone();
        cancellableRequest.AttemptId = "attempt-cancel-loopback-001";
        cancellableRequest.Prompt = "wait-for-cancel";
        await client.SendMessageAsync(cancellableRequest.ToByteArray(), timeout.Token).ConfigureAwait(false);
        await backendHttpHandler.GenerationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);

        var cancelRequest = new CancelRequest
        {
            RequestId = cancellableRequest.RequestId,
            AttemptId = cancellableRequest.AttemptId,
            Reason = ProtocolMessageValidator.UserCancellationReason
        };
        Assert(ProtocolMessageParser.ParseCancelRequest(cancelRequest.ToByteArray()).IsValid,
            "A bounded cancellation request with the exact active request/attempt identity must pass protocol validation.");
        await client.SendMessageAsync(cancelRequest.ToByteArray(), timeout.Token).ConfigureAwait(false);
        await backendHttpHandler.CancellationObserved.Task.WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        await backendHttpHandler.GenerationStopped.Task.WaitAsync(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);

        byte[] cancelledWire = await client.ReceiveMessageAsync(timeout.Token)
            .WaitAsync(TimeSpan.FromSeconds(10), timeout.Token).ConfigureAwait(false);
        var cancelledResponse = InferenceResponse.Parser.ParseFrom(cancelledWire);
        Assert(cancelledResponse.RequestId == cancellableRequest.RequestId &&
               cancelledResponse.AttemptId == cancellableRequest.AttemptId &&
               cancelledResponse.RoundId == cancellableRequest.RoundId &&
               cancelledResponse.EphemeralHostId == hostPeerId &&
               cancelledResponse.Outcome == InferenceOutcome.Cancelled &&
               cancelledResponse.OutputText.Length == 0 &&
               cancelledResponse.ErrorMessage == "The request was cancelled by the remote client.",
            "Remote cancellation must reach the llama.cpp HTTP request and return CANCELLED without a late success payload.");
        Assert(!requestCoordinator.IsBusy,
            "The host request coordinator must release its single-request gate after remote cancellation.");
        Assert(Volatile.Read(ref requestHandlerCalls) == 2,
            "The cancellation path must process the second request exactly once.");

        await client.DisconnectAsync(timeout.Token).ConfigureAwait(false);
        DateTimeOffset stoppedDeadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (host.IsActive && DateTimeOffset.UtcNow < stoppedDeadline)
            await Task.Delay(TimeSpan.FromMilliseconds(25), timeout.Token).ConfigureAwait(false);
        Assert(!host.IsActive,
            "A peer disconnect or failed message lifecycle must stop host availability and stop lease renewal.");

        await Task.Delay(TimeSpan.FromSeconds(2.2), timeout.Token).ConfigureAwait(false);
        var expired = await directoryClient.DiscoverByCapabilityAsync("inference.v1").ConfigureAwait(false);
        Assert(expired.All(peer => peer.PeerId != hostPeerId),
            "After the last renewable lease expires, the directory must no longer return the disconnected host.");
    }

    private static int ReservePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task SensitiveInferencePayloadFieldsAreRejectedByTheControlPlaneAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var serviceUri = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single(), UriKind.Absolute);
            using var httpClient = new HttpClient();
            string peerId = CreatePeerId();
            var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["peerId"] = peerId,
                ["addresses"] = new[] { $"/ip4/127.0.0.1/tcp/43003/p2p/{peerId}" },
                ["capabilities"] = new[] { "inference.v1" },
                ["expiresAtUtc"] = DateTimeOffset.UtcNow.AddMinutes(5)
            };

            foreach (string forbiddenField in new[] { "prompt", "contextCapsule", "capsule" })
            {
                var attemptedAnnouncement = new Dictionary<string, object>(metadata, StringComparer.Ordinal)
                {
                    [forbiddenField] = forbiddenField == "contextCapsule"
                        ? new { goal = "DO_NOT_STORE_OR_FORWARD" }
                        : "DO_NOT_STORE_OR_FORWARD"
                };
                string json = JsonSerializer.Serialize(attemptedAnnouncement);
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(serviceUri, "v1/peers/announcements"))
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                using HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                Assert(response.StatusCode == HttpStatusCode.BadRequest,
                    $"Rendezvous must reject an announcement containing the unrecognized inference field '{forbiddenField}'.");
            }

            using var discoveryResponse = await httpClient.GetAsync(
                new Uri(serviceUri, "v1/peers?capability=inference.v1")).ConfigureAwait(false);
            Assert(discoveryResponse.StatusCode == HttpStatusCode.OK,
                "Metadata discovery must remain available after rejected sensitive-field announcements.");
            string discoveryJson = await discoveryResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var discoveryDocument = JsonDocument.Parse(discoveryJson);
            Assert(discoveryDocument.RootElement.GetArrayLength() == 0,
                "Rejected prompt/capsule announcements must never enter the introducer's peer directory.");
        }
        finally
        {
            await app.StopAsync().ConfigureAwait(false);
        }
    }

    private static async Task OversizedControlMessagesAreRejectedAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var serviceUri = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single(), UriKind.Absolute);
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(serviceUri, "v1/peers/announcements"))
            {
                Content = new ByteArrayContent(new byte[PeerAnnouncementWireFormat.MaximumMessageBytes + 1])
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            using HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            Assert(response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
                "An over-limit control message must be rejected by the raw request-body size limit.");
        }
        finally { await app.StopAsync().ConfigureAwait(false); }
    }

    private static void LeasePublisherConfigurationIsValidated()
    {
        using var httpClient = new HttpClient();
        var client = new PeerAnnouncementClient(httpClient, new Uri("http://127.0.0.1:1234/"));
        string peerId = CreatePeerId();
        var template = new PeerAnnouncement(
            peerId,
            [$"/ip4/127.0.0.1/tcp/43011/p2p/{peerId}"],
            ["inference.v1"],
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert(PeerAnnouncementLeasePublisher.DefaultTimeToLive == TimeSpan.FromMinutes(5),
            "The default announcement lease should have a five-minute TTL.");
        Assert(PeerAnnouncementLeasePublisher.DefaultRenewalInterval == TimeSpan.FromMinutes(1),
            "The default renewal interval should be one minute.");
        Assert(PeerAnnouncementLeasePublisher.DefaultRenewalInterval < PeerAnnouncementLeasePublisher.DefaultTimeToLive,
            "The default renewal interval must be shorter than its TTL.");

        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(null!, template),
            "A null HTTP announcement client must be rejected.");
        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(client, null!),
            "A null announcement template must be rejected.");
        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(
                client, template, TimeSpan.Zero, TimeSpan.FromSeconds(1)),
            "A zero TTL must be rejected.");
        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(
                client, template, PeerAnnouncement.MaximumLifetime + TimeSpan.FromTicks(1), TimeSpan.FromHours(1)),
            "A TTL beyond the shared announcement contract must be rejected.");
        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(
                client, template, TimeSpan.FromSeconds(2), TimeSpan.Zero),
            "A zero renewal interval must be rejected.");
        ExpectArgumentException(() => _ = new PeerAnnouncementLeasePublisher(
                client, template, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)),
            "A renewal interval equal to the TTL must be rejected.");
    }

    private static void NonLoopbackPlainHttpIsRejected()
    {
        using var client = new HttpClient();
        ExpectArgumentException(() => _ = new PeerAnnouncementClient(client, new Uri("http://announcer.example.test")),
            "A non-loopback rendezvous endpoint must require HTTPS.");
    }

    private static void ExpectArgumentException(Action action, string message)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new InvalidOperationException(message);
    }

    private static string CreatePeerId()
    {
        var identity = new Identity();
        return new PeerId(identity.PublicKey).ToString();
    }

    private sealed class CancellationProbeHttpHandler : HttpMessageHandler
    {
        public TaskCompletionSource<bool> GenerationStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> CancellationObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> GenerationStopped { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (request.Method == HttpMethod.Get && path.EndsWith("/health", StringComparison.Ordinal))
                return new HttpResponseMessage(HttpStatusCode.OK);

            if (request.Method == HttpMethod.Get && path == "/v1/models")
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"data\":[{\"id\":\"fixture-model\"}]}",
                        Encoding.UTF8,
                        "application/json")
                };

            if (request.Method == HttpMethod.Get && path == "/props")
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"default_generation_settings\":{\"n_ctx\":8192}}",
                        Encoding.UTF8,
                        "application/json")
                };

            if (request.Method != HttpMethod.Post || path != "/v1/chat/completions")
                throw new InvalidOperationException("The test backend received an unexpected HTTP request.");

            string requestBody = await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var payload = JsonDocument.Parse(requestBody);
            string prompt = payload.RootElement
                .GetProperty("messages")[0]
                .GetProperty("content")
                .GetString() ?? string.Empty;

            if (prompt == "wait-for-cancel")
            {
                GenerationStarted.TrySetResult(true);
                using var registration = cancellationToken.Register(
                    () => CancellationObserved.TrySetResult(true));
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    GenerationStopped.TrySetResult(true);
                    throw;
                }
            }

            const string responseJson =
                "{\"choices\":[{\"message\":{\"content\":\"loopback-inference-result\"}}]}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

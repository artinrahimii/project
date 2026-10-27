using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Nethermind.Libp2p.Core;
using WhiteP.Host.Windows.Networking;
using WhiteP.Rendezvous.Service;
using WhiteP.Shared.Contracts.Networking;

internal static class PeerAnnouncementClientAcceptanceTests
{
    public static async Task RunAsync()
    {
        VerifyContractSurfaceIsMetadataOnly();
        NonLoopbackPlainHttpIsRejected();
        await RealAnnouncementAndCapabilityDiscoveryWorkOverLoopbackAsync().ConfigureAwait(false);
        await UnknownPromptFieldIsRejectedByTheControlPlaneAsync().ConfigureAwait(false);
        await OversizedControlMessagesAreRejectedAsync().ConfigureAwait(false);
        Console.WriteLine("PASS: metadata-only peer announcement and capability discovery; unknown content fields and oversized control messages are rejected.");
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

    private static async Task UnknownPromptFieldIsRejectedByTheControlPlaneAsync()
    {
        await using var app = PeerAnnouncementServiceHost.Create(Array.Empty<string>(), "http://127.0.0.1:0");
        await app.StartAsync().ConfigureAwait(false);
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var serviceUri = new Uri(server.Features.Get<IServerAddressesFeature>()!.Addresses.Single(), UriKind.Absolute);
            using var httpClient = new HttpClient();
            string peerId = CreatePeerId();
            string json = JsonSerializer.Serialize(new
            {
                peerId,
                addresses = new[] { $"/ip4/127.0.0.1/tcp/43003/p2p/{peerId}" },
                capabilities = new[] { "inference.v1" },
                expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                prompt = "DO_NOT_STORE_OR_FORWARD"
            });
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(serviceUri, "v1/peers/announcements"))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            using HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            Assert(response.StatusCode == HttpStatusCode.BadRequest,
                "An announcement with an unrecognized prompt field must be rejected before directory storage.");
        }
        finally { await app.StopAsync().ConfigureAwait(false); }
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

    private static void NonLoopbackPlainHttpIsRejected()
    {
        using var client = new HttpClient();
        ExpectArgumentException(() => _ = new PeerAnnouncementClient(client, new Uri("http://announcer.example.test")),
            "A non-loopback rendezvous endpoint must require HTTPS.");
    }

    private static void ExpectArgumentException(Action action, string message)
    {
        try { action(); return; }
        catch (ArgumentException) { }
        throw new InvalidOperationException(message);
    }

    private static string CreatePeerId()
    {
        var identity = new Identity();
        return new PeerId(identity.PublicKey).ToString();
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

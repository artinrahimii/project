using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WhiteP.Shared.Contracts.Networking;
namespace WhiteP.Host.Windows.Networking;

/// <summary>Client exposes only announcement and capability-discovery operations.</summary>
public sealed class PeerAnnouncementClient
{
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;
    public PeerAnnouncementClient(HttpClient httpClient, Uri serviceBaseUri)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(serviceBaseUri);
        if (!serviceBaseUri.IsAbsoluteUri ||
            (!string.Equals(serviceBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
             !(string.Equals(serviceBaseUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && serviceBaseUri.IsLoopback)) ||
            !string.IsNullOrEmpty(serviceBaseUri.Query) || !string.IsNullOrEmpty(serviceBaseUri.Fragment))
            throw new ArgumentException("Rendezvous requires HTTPS except for loopback tests.", nameof(serviceBaseUri));
        _httpClient = httpClient;
        string text = serviceBaseUri.AbsoluteUri;
        _baseUri = new Uri(text.EndsWith("/", StringComparison.Ordinal) ? text : text + "/", UriKind.Absolute);
    }

    public async Task AnnounceAsync(PeerAnnouncement announcement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUri, "v1/peers/announcements"))
        {
            Content = new ByteArrayContent(PeerAnnouncementWireFormat.SerializeAnnouncement(announcement))
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<PeerAnnouncement>> DiscoverByCapabilityAsync(string capability, CancellationToken cancellationToken = default)
    {
        PeerAnnouncement.ValidateCapabilityName(capability);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, $"v1/peers?capability={Uri.EscapeDataString(capability)}"));
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        byte[] body = await ReadBoundedAsync(response.Content, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<PeerAnnouncement> peers = PeerAnnouncementWireFormat.DeserializeDirectory(body);
        if (peers.Any(peer => !peer.SupportsCapability(capability))) throw new InvalidDataException("Directory returned a peer without the requested capability.");
        return peers;
    }

    private static async Task<byte[]> ReadBoundedAsync(HttpContent content, CancellationToken token)
    {
        if (content.Headers.ContentLength is > PeerAnnouncementWireFormat.MaximumMessageBytes)
            throw new InvalidDataException("Directory response exceeds the control-message limit.");
        await using Stream source = await content.ReadAsStreamAsync(token).ConfigureAwait(false);
        using var target = new MemoryStream();
        byte[] buffer = new byte[4096];
        while (true)
        {
            int count = await source.ReadAsync(buffer.AsMemory(), token).ConfigureAwait(false);
            if (count == 0) break;
            if (target.Length + count > PeerAnnouncementWireFormat.MaximumMessageBytes)
                throw new InvalidDataException("Directory response exceeds the control-message limit.");
            await target.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
        }
        return target.ToArray();
    }
}

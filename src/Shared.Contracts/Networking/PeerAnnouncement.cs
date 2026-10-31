using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace WhiteP.Shared.Contracts.Networking;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PeerAnnouncement
{
    public const int MaximumMessageBytes = 16 * 1024;
    public const int MaximumAddresses = 8;
    public const int MaximumCapabilities = 32;
    public const int MaximumAddressLength = 512;
    public static readonly TimeSpan MaximumLifetime = TimeSpan.FromHours(24);

    [JsonConstructor]
    public PeerAnnouncement(string peerId, IReadOnlyList<string> addresses, IReadOnlyList<string> capabilities, DateTimeOffset expiresAtUtc)
    {
        PeerId = ValidatePeerId(peerId);
        ArgumentNullException.ThrowIfNull(addresses);
        ArgumentNullException.ThrowIfNull(capabilities);
        string[] addressCopy = addresses.ToArray();
        string[] capabilityCopy = capabilities.ToArray();
        if (addressCopy.Length is < 1 or > MaximumAddresses) throw new ArgumentException("Invalid direct address count.", nameof(addresses));
        if (capabilityCopy.Length is < 1 or > MaximumCapabilities) throw new ArgumentException("Invalid capability count.", nameof(capabilities));
        foreach (string address in addressCopy) ValidateDirectAddress(address, PeerId);
        if (addressCopy.Distinct(StringComparer.Ordinal).Count() != addressCopy.Length) throw new ArgumentException("Duplicate addresses.", nameof(addresses));
        foreach (string capability in capabilityCopy) ValidateCapabilityName(capability);
        if (capabilityCopy.Distinct(StringComparer.Ordinal).Count() != capabilityCopy.Length) throw new ArgumentException("Duplicate capabilities.", nameof(capabilities));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expiry = expiresAtUtc.ToUniversalTime();
        if (expiry <= now || expiry > now.Add(MaximumLifetime)) throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "Expiry must be within the next 24 hours.");
        Addresses = Array.AsReadOnly(addressCopy);
        Capabilities = Array.AsReadOnly(capabilityCopy);
        ExpiresAtUtc = expiry;
    }

    public string PeerId { get; }
    public IReadOnlyList<string> Addresses { get; }
    public IReadOnlyList<string> Capabilities { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public bool SupportsCapability(string capability) => Capabilities.Contains(capability, StringComparer.Ordinal);

    public static bool IsValidCapabilityName(string? capability) =>
        !string.IsNullOrEmpty(capability) && capability.Length <= 64 &&
        IsAsciiAlphaNumeric(capability[0]) && capability.All(c => IsAsciiAlphaNumeric(c) || c is '.' or '_' or '-');

    public static void ValidateCapabilityName(string? capability, string parameterName = "capability")
    {
        if (!IsValidCapabilityName(capability)) throw new ArgumentException("Invalid capability token.", parameterName);
    }

    private static string ValidatePeerId(string peerId)
    {
        if (string.IsNullOrWhiteSpace(peerId) || peerId.Length is < 20 or > 128 || peerId.Any(c => !IsAsciiAlphaNumeric(c)))
            throw new ArgumentException("A normalized libp2p PeerId is required.", nameof(peerId));
        return peerId;
    }

    private static void ValidateDirectAddress(string? address, string peerId)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Length > MaximumAddressLength ||
            !address.StartsWith("/", StringComparison.Ordinal) || address.Any(char.IsWhiteSpace) || address.Any(char.IsControl) ||
            address.Contains("/p2p-circuit", StringComparison.Ordinal) || !address.EndsWith($"/p2p/{peerId}", StringComparison.Ordinal))
            throw new ArgumentException("Only normalized direct multiaddresses ending in this PeerId are accepted.", nameof(address));
        bool tcp = address.Contains("/tcp/", StringComparison.Ordinal);
        bool quic = address.Contains("/udp/", StringComparison.Ordinal) && address.Contains("/quic-v1", StringComparison.Ordinal);
        if (!tcp && !quic) throw new ArgumentException("Only direct TCP or QUIC addresses can be announced.", nameof(address));
    }

    private static bool IsAsciiAlphaNumeric(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';
}

public static class PeerAnnouncementWireFormat
{
    public const int MaximumMessageBytes = PeerAnnouncement.MaximumMessageBytes;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        MaxDepth = 8,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static byte[] SerializeAnnouncement(PeerAnnouncement announcement)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        return SerializeBounded(announcement);
    }

    public static byte[] SerializeDirectory(IReadOnlyList<PeerAnnouncement> announcements)
    {
        ArgumentNullException.ThrowIfNull(announcements);
        return SerializeBounded(announcements);
    }

    public static IReadOnlyList<PeerAnnouncement> DeserializeDirectory(ReadOnlySpan<byte> json)
    {
        EnsureWithinLimit(json.Length);
        try
        {
            PeerAnnouncement[] peers = JsonSerializer.Deserialize<PeerAnnouncement[]>(json, JsonOptions)
                ?? throw new InvalidDataException("Null peer directory response.");
            if (peers.Length > 256) throw new InvalidDataException("Too many announcements.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (PeerAnnouncement peer in peers)
                if (!ids.Add(peer.PeerId)) throw new InvalidDataException("Duplicate PeerId in directory response.");
            return Array.AsReadOnly(peers);
        }
        catch (JsonException exception) { throw new InvalidDataException("Invalid peer metadata response.", exception); }
    }

    private static byte[] SerializeBounded<T>(T value)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        EnsureWithinLimit(payload.Length);
        return payload;
    }

    private static void EnsureWithinLimit(int size)
    {
        if (size > MaximumMessageBytes) throw new InvalidDataException($"Control messages must not exceed {MaximumMessageBytes} bytes.");
    }
}

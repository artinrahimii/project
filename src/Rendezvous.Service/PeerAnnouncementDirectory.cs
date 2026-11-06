using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhiteP.Shared.Contracts.Networking;
namespace WhiteP.Rendezvous.Service;

/// <summary>Ephemeral metadata-only directory retaining the latest PeerId announcement until expiry.</summary>
public sealed class PeerAnnouncementDirectory
{
    private const int MaximumActivePeers = 2048;
    private readonly object _gate = new();
    private readonly Dictionary<string, PeerAnnouncement> _current = new(StringComparer.Ordinal);

    public void Announce(PeerAnnouncement announcement)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (announcement.ExpiresAtUtc <= now) throw new ArgumentException("Expired announcement.", nameof(announcement));
        lock (_gate)
        {
            RemoveExpired(now);
            if (!_current.ContainsKey(announcement.PeerId) && _current.Count >= MaximumActivePeers)
                throw new InvalidOperationException("Peer directory at capacity.");
            _current[announcement.PeerId] = announcement;
        }
    }

    public IReadOnlyList<PeerAnnouncement> DiscoverByCapability(string capability)
    {
        PeerAnnouncement.ValidateCapabilityName(capability);
        PeerAnnouncement[] candidates;
        lock (_gate)
        {
            RemoveExpired(DateTimeOffset.UtcNow);
            candidates = _current.Values.Where(p => p.SupportsCapability(capability)).OrderBy(p => p.PeerId, StringComparer.Ordinal).ToArray();
        }
        var result = new List<PeerAnnouncement>();
        foreach (PeerAnnouncement candidate in candidates)
        {
            result.Add(candidate);
            try { _ = PeerAnnouncementWireFormat.SerializeDirectory(result); }
            catch (InvalidDataException) { result.RemoveAt(result.Count - 1); break; }
        }
        return Array.AsReadOnly(result.ToArray());
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (string id in _current.Where(pair => pair.Value.ExpiresAtUtc <= now).Select(pair => pair.Key).ToArray())
            _current.Remove(id);
    }
}

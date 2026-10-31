using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteP.Shared.Security;

/// <summary>
/// Bounded, in-memory replay fence keyed by the authenticated peer and the exact request/attempt tuple.
/// Entries live until their request deadline; the cache never evicts a still-live key to make room.
/// </summary>
public sealed class AuthenticatedRequestReplayGuard
{
    public const int DefaultMaximumTrackedRequests = 8192;

    private readonly object _gate = new();
    private readonly Dictionary<ReplayKey, DateTimeOffset> _seen = new();
    private readonly int _maximumTrackedRequests;

    public AuthenticatedRequestReplayGuard(int maximumTrackedRequests = DefaultMaximumTrackedRequests)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTrackedRequests);
        _maximumTrackedRequests = maximumTrackedRequests;
    }

    public int TrackedRequestCount
    {
        get { lock (_gate) return _seen.Count; }
    }

    public AuthenticatedRequestReplayDecision TryRegister(
        string authenticatedPeerIdentity,
        string requestId,
        string attemptId,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset nowUtc)
    {
        ValidateToken(authenticatedPeerIdentity, nameof(authenticatedPeerIdentity), 512);
        ValidateToken(requestId, nameof(requestId), 128);
        ValidateToken(attemptId, nameof(attemptId), 128);
        if (expiresAtUtc <= nowUtc) return AuthenticatedRequestReplayDecision.Expired;

        var key = new ReplayKey(authenticatedPeerIdentity, requestId, attemptId);
        lock (_gate)
        {
            foreach (ReplayKey expired in _seen.Where(pair => pair.Value <= nowUtc).Select(pair => pair.Key).ToArray())
                _seen.Remove(expired);

            if (_seen.TryGetValue(key, out DateTimeOffset expiry) && expiry > nowUtc)
                return AuthenticatedRequestReplayDecision.Duplicate;
            if (_seen.Count >= _maximumTrackedRequests)
                return AuthenticatedRequestReplayDecision.CapacityExceeded;

            _seen[key] = expiresAtUtc;
            return AuthenticatedRequestReplayDecision.Accepted;
        }
    }

    /// <summary>Releases only a request reservation that never entered execution.</summary>
    public bool Release(string authenticatedPeerIdentity, string requestId, string attemptId)
    {
        ValidateToken(authenticatedPeerIdentity, nameof(authenticatedPeerIdentity), 512);
        ValidateToken(requestId, nameof(requestId), 128);
        ValidateToken(attemptId, nameof(attemptId), 128);
        lock (_gate) return _seen.Remove(new ReplayKey(authenticatedPeerIdentity, requestId, attemptId));
    }

    private static void ValidateToken(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Any(char.IsControl))
            throw new ArgumentException("A bounded, non-empty replay-key component is required.", parameterName);
    }

    private readonly record struct ReplayKey(string PeerIdentity, string RequestId, string AttemptId);
}

public enum AuthenticatedRequestReplayDecision
{
    Accepted = 0,
    Duplicate = 1,
    CapacityExceeded = 2,
    Expired = 3
}

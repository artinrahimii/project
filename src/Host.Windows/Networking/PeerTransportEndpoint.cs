using System;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Transport-neutral address information plus the peer identity the secure handshake must verify.
/// The address syntax is interpreted by the selected transport implementation.
/// </summary>
public sealed record PeerTransportEndpoint
{
    public PeerTransportEndpoint(string address, string expectedPeerIdentity)
    {
        if (string.IsNullOrWhiteSpace(address) ||
            !string.Equals(address, address.Trim(), StringComparison.Ordinal) ||
            ContainsControlCharacters(address))
        {
            throw new ArgumentException(
                "A non-empty transport address without surrounding whitespace or control characters is required.",
                nameof(address));
        }

        if (string.IsNullOrWhiteSpace(expectedPeerIdentity) ||
            !string.Equals(expectedPeerIdentity, expectedPeerIdentity.Trim(), StringComparison.Ordinal) ||
            ContainsControlCharacters(expectedPeerIdentity))
        {
            throw new ArgumentException(
                "A non-empty expected peer identity without surrounding whitespace or control characters is required.",
                nameof(expectedPeerIdentity));
        }

        Address = address;
        ExpectedPeerIdentity = expectedPeerIdentity;
    }

    public string Address { get; }

    public string ExpectedPeerIdentity { get; }

    private static bool ContainsControlCharacters(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                return true;
            }
        }

        return false;
    }
}

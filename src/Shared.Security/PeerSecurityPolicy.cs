using System;
using System.Linq;
using System.Security.Authentication;
using Nethermind.Libp2p.Core;

namespace WhiteP.Shared.Security;

/// <summary>
/// Applies the project's fail-closed identity input policy and delegates libp2p PeerId parsing
/// to the maintained library. Secure-channel cryptography remains the responsibility of the
/// libp2p Noise/TLS stack; this class implements no cryptographic algorithms.
/// </summary>
public static class PeerSecurityPolicy
{
    public static void ValidatePeerIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity) ||
            !string.Equals(identity, identity.Trim(), StringComparison.Ordinal) ||
            identity.Any(char.IsControl))
        {
            throw new AuthenticationException("A valid, normalized peer identity is required.");
        }

        try
        {
            _ = new PeerId(identity);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or IndexOutOfRangeException)
        {
            throw new AuthenticationException("The supplied peer identity is not a valid libp2p PeerId.", exception);
        }
    }
}

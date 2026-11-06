using System;
using System.Security.Authentication;
using Nethermind.Libp2p.Core;
using WhiteP.Shared.Security;

internal static class PeerSecurityPolicyAcceptanceTests
{
    public static void Run()
    {
        var identity = new Identity();
        string peerId = new PeerId(identity.PublicKey).ToString();

        PeerSecurityPolicy.ValidatePeerIdentity(peerId);
        ExpectAuthenticationException(() => PeerSecurityPolicy.ValidatePeerIdentity(null!));
        ExpectAuthenticationException(() => PeerSecurityPolicy.ValidatePeerIdentity(""));
        ExpectAuthenticationException(() => PeerSecurityPolicy.ValidatePeerIdentity(" " + peerId));
        ExpectAuthenticationException(() => PeerSecurityPolicy.ValidatePeerIdentity(peerId + "\n"));
        ExpectAuthenticationException(() => PeerSecurityPolicy.ValidatePeerIdentity("not-a-peer-id"));

        Console.WriteLine("PASS: shared peer identity policy rejects malformed input and delegates PeerId parsing to libp2p.");
    }

    private static void ExpectAuthenticationException(Action action)
    {
        try
        {
            action();
        }
        catch (AuthenticationException)
        {
            return;
        }

        throw new InvalidOperationException("Invalid peer identity must fail closed with AuthenticationException.");
    }
}

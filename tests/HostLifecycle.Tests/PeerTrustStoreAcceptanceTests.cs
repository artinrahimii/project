using System;
using System.IO;
using System.Security.Authentication;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Core.Dto;
using WhiteP.Shared.Security;

internal static class PeerTrustStoreAcceptanceTests
{
    public static void Run()
    {
        using var fixture = new TestPeerTrustStoreFixture();
        var firstIdentity = new Identity(keyType: KeyType.Ed25519);
        var secondIdentity = new Identity(keyType: KeyType.Ed25519);
        string firstPeerId = new PeerId(firstIdentity.PublicKey).ToString();
        string secondPeerId = new PeerId(secondIdentity.PublicKey).ToString();

        Assert(fixture.Store.GetStatus(firstPeerId) == PeerTrustStatus.Unknown,
            "A peer must be unknown until a user explicitly confirms its exact PeerId.");
        bool unknownRejected = false;
        try { fixture.Store.EnsureTrusted(firstPeerId); }
        catch (AuthenticationException) { unknownRejected = true; }
        Assert(unknownRejected, "The transport trust gate must reject an unknown peer.");

        bool unconfirmedRejected = false;
        try { fixture.Store.TrustPeerAfterExplicitConfirmation(firstPeerId, explicitlyConfirmed: false); }
        catch (AuthenticationException) { unconfirmedRejected = true; }
        Assert(unconfirmedRejected && !fixture.Store.IsTrusted(firstPeerId),
            "A rejected or absent user confirmation must not write a trust decision.");

        fixture.Store.TrustPeerAfterExplicitConfirmation(firstPeerId, explicitlyConfirmed: true);
        fixture.Store.EnsureTrusted(firstPeerId);
        Assert(fixture.Store.GetStatus(firstPeerId) == PeerTrustStatus.Trusted,
            "An explicitly approved canonical PeerId must be trusted.");

        var reloaded = new PeerTrustStore(fixture.FilePath, new TestOnlyPeerTrustProtector());
        reloaded.EnsureTrusted(firstPeerId);
        bool secondPeerRejected = false;
        try { reloaded.EnsureTrusted(secondPeerId); }
        catch (AuthenticationException) { secondPeerRejected = true; }
        Assert(secondPeerRejected && reloaded.GetStatus(secondPeerId) == PeerTrustStatus.Unknown,
            "A different PeerId must remain unknown rather than replacing a previously trusted identity.");

        fixture.Store.TrustPeerAfterExplicitConfirmation(secondPeerId, explicitlyConfirmed: true);
        Assert(fixture.Store.IsTrusted(firstPeerId) && fixture.Store.IsTrusted(secondPeerId),
            "Trusting another peer must preserve prior explicit trust decisions.");

        bool malformedIdentityRejected = false;
        try { fixture.Store.TrustPeerAfterExplicitConfirmation("not-a-peer-id", explicitlyConfirmed: true); }
        catch (AuthenticationException) { malformedIdentityRejected = true; }
        Assert(malformedIdentityRejected, "Malformed or non-canonical PeerIds must not enter the trust store.");

        using var corruptFixture = new TestPeerTrustStoreFixture();
        byte[] corruptBytes = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05];
        File.WriteAllBytes(corruptFixture.FilePath, corruptBytes);
        bool corruptRejected = false;
        try { _ = corruptFixture.Store.IsTrusted(firstPeerId); }
        catch (InvalidDataException) { corruptRejected = true; }
        Assert(corruptRejected, "A corrupt trust store must fail closed instead of resetting to an empty store.");
        bool corruptWriteRejected = false;
        try { corruptFixture.Store.TrustPeerAfterExplicitConfirmation(secondPeerId, explicitlyConfirmed: true); }
        catch (InvalidDataException) { corruptWriteRejected = true; }
        Assert(corruptWriteRejected && File.ReadAllBytes(corruptFixture.FilePath).AsSpan().SequenceEqual(corruptBytes),
            "Trust operations must not overwrite malformed persisted trust data.");

        Console.WriteLine("PASS: peer trust is explicit, persisted, canonical-PeerId-bound and fail-closed for unknown or corrupt identities.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

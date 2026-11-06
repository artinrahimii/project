using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Core.Dto;
using WhiteP.Host.Windows.Security;
using WhiteP.Shared.Security;

internal static class WindowsPeerTrustStoreAcceptanceTests
{
    public static void Run()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This acceptance test must execute on a Windows runner.");

        string directory = Path.Combine(Path.GetTempPath(), "whitep-dpapi-peer-trust-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "peer-trust.dpapi");
        byte[]? peerIdBytes = null;
        try
        {
            var identity = new Identity(keyType: KeyType.Ed25519);
            string peerId = new PeerId(identity.PublicKey).ToString();
            peerIdBytes = Encoding.UTF8.GetBytes(peerId);

            new PeerTrustStore(path, new WindowsDpapiSecretProtector())
                .TrustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed: true);
            byte[] persisted = File.ReadAllBytes(path);
            Assert(!ContainsSequence(persisted, peerIdBytes),
                "The persisted peer trust file must not contain a trusted PeerId in plaintext.");

            var reloaded = new PeerTrustStore(path, new WindowsDpapiSecretProtector());
            Assert(reloaded.IsTrusted(peerId),
                "A trust decision protected by current-user DPAPI must survive a new store instance.");

            byte[] tampered = persisted.ToArray();
            tampered[^1] ^= 0x40;
            File.WriteAllBytes(path, tampered);
            bool tamperingRejected = false;
            try
            {
                _ = new PeerTrustStore(path, new WindowsDpapiSecretProtector()).IsTrusted(peerId);
            }
            catch (Exception exception) when (exception is CryptographicException or InvalidDataException)
            {
                tamperingRejected = true;
            }
            Assert(tamperingRejected, "Tampered DPAPI peer-trust data must fail closed.");
            Assert(File.ReadAllBytes(path).SequenceEqual(tampered),
                "Tampered peer-trust data must not be silently reset or overwritten.");

            Console.WriteLine("PASS: Windows DPAPI protects peer trust decisions at rest, preserves them across store instances, and rejects tampering.");
        }
        finally
        {
            if (peerIdBytes is not null)
                CryptographicOperations.ZeroMemory(peerIdBytes);
            try { Directory.Delete(directory, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static bool ContainsSequence(byte[] buffer, byte[] candidate) =>
        candidate.Length > 0 && buffer.AsSpan().IndexOf(candidate) >= 0;

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

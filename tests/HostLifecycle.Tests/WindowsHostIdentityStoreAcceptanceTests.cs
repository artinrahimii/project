using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using System.Security.Cryptography;
using Nethermind.Libp2p.Core;
using WhiteP.Host.Windows.Security;

internal static class WindowsHostIdentityStoreAcceptanceTests
{
    public static void Run()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This acceptance test must execute on a Windows runner.");

        string root = Path.Combine(Path.GetTempPath(), "whitep-dpapi-host-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string path = Path.Combine(root, "host-identity.dpapi");
        byte[]? serializedPrivateKey = null;
        try
        {
            Identity first = new WindowsHostIdentityProvider(path).LoadOrCreate();
            Assert(first.PrivateKey is not null, "A persisted host identity must contain its private key.");
            string peerId = new PeerId(first.PublicKey).ToString();
            serializedPrivateKey = first.PrivateKey!.ToByteArray();
            byte[] stored = File.ReadAllBytes(path);

            Assert(!ContainsSequence(stored, serializedPrivateKey), "The persisted file must not contain the serialized private key in plaintext.");
            Identity second = new WindowsHostIdentityProvider(path).LoadOrCreate();
            Assert(new PeerId(second.PublicKey).ToString() == peerId, "The Windows DPAPI store must preserve host identity across provider instances.");

            byte[] tampered = stored.ToArray();
            tampered[^1] ^= 0x20;
            File.WriteAllBytes(path, tampered);
            bool tamperingRejected = false;
            try
            {
                _ = new WindowsHostIdentityProvider(path).LoadOrCreate();
            }
            catch (Exception exception) when (exception is CryptographicException or InvalidDataException)
            {
                tamperingRejected = true;
            }
            Assert(tamperingRejected, "Tampered DPAPI data must fail closed.");
            Assert(File.ReadAllBytes(path).SequenceEqual(tampered), "Tampered key data must not be silently replaced.");

            Console.WriteLine("PASS: Windows current-user DPAPI preserves the host PeerId, protects key bytes at rest and rejects tampered storage.");
        }
        finally
        {
            if (serializedPrivateKey is not null)
                CryptographicOperations.ZeroMemory(serializedPrivateKey);
            try { Directory.Delete(root, recursive: true); }
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

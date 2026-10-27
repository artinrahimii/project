using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Nethermind.Libp2p.Core;
using WhiteP.Shared.Security;

internal static class HostIdentityStoreAcceptanceTests
{
    public static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "whitep-host-identity-store-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string path = Path.Combine(root, "identity.test-envelope");
            Task<Identity>[] firstRun = Enumerable.Range(0, 6)
                .Select(_ => Task.Run(() => new HostIdentityStore(path, new TestOnlyPrefixProtector()).LoadOrCreate()))
                .ToArray();
            Task.WaitAll(firstRun);

            string[] identities = firstRun
                .Select(task => new PeerId(task.Result.PublicKey).ToString())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            Assert(identities.Length == 1, "Concurrent first-run callers must observe a single persisted PeerId.");

            Identity loaded = new HostIdentityStore(path, new TestOnlyPrefixProtector()).LoadOrCreate();
            Assert(new PeerId(loaded.PublicKey).ToString() == identities[0], "Reload must preserve the existing PeerId.");

            byte[] existingBytes = File.ReadAllBytes(path);
            bool protectorFailureRejected = false;
            try
            {
                _ = new HostIdentityStore(path, new ThrowingUnprotector()).LoadOrCreate();
            }
            catch (CryptographicException)
            {
                protectorFailureRejected = true;
            }
            Assert(protectorFailureRejected, "A failed unprotect operation must fail closed.");
            Assert(File.ReadAllBytes(path).SequenceEqual(existingBytes), "A failed unprotect must not overwrite an existing identity.");

            string corruptPath = Path.Combine(root, "corrupt-envelope");
            byte[] corrupt = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05];
            File.WriteAllBytes(corruptPath, corrupt);
            bool invalidEnvelopeRejected = false;
            try
            {
                _ = new HostIdentityStore(corruptPath, new TestOnlyPrefixProtector()).LoadOrCreate();
            }
            catch (InvalidDataException)
            {
                invalidEnvelopeRejected = true;
            }
            Assert(invalidEnvelopeRejected, "An invalid existing identity file must be rejected, not replaced with a new key.");
            Assert(File.ReadAllBytes(corruptPath).SequenceEqual(corrupt), "An invalid existing identity file must remain untouched.");

            Console.WriteLine("PASS: host identity store serializes concurrent creation, preserves PeerId on reload and fails closed on invalid or unreadable files.");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    // This reversible protector is test-only and does not provide confidentiality.
    // Real at-rest protection is exercised by WindowsHostIdentityStoreAcceptanceTests on Windows.
    private sealed class TestOnlyPrefixProtector : IUserSecretProtector
    {
        private const byte Prefix = 0xA7;
        public byte[] Protect(byte[] plaintext)
        {
            var output = new byte[plaintext.Length + 1];
            output[0] = Prefix;
            plaintext.CopyTo(output, 1);
            return output;
        }
        public byte[] Unprotect(byte[] protectedPayload)
        {
            if (protectedPayload.Length < 2 || protectedPayload[0] != Prefix)
                throw new CryptographicException("Test-only envelope is invalid.");
            return protectedPayload.AsSpan(1).ToArray();
        }
    }

    private sealed class ThrowingUnprotector : IUserSecretProtector
    {
        public byte[] Protect(byte[] plaintext) => throw new NotSupportedException("This test protector must never create a new key.");
        public byte[] Unprotect(byte[] protectedPayload) => throw new CryptographicException("Simulated unprotect failure.");
    }
}

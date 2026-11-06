using System;
using System.IO;
using System.Security.Cryptography;
using WhiteP.Shared.Security;

/// <summary>
/// Test-only fixture. Its reversible protector is not cryptographic and must never be used by production code.
/// </summary>
internal sealed class TestPeerTrustStoreFixture : IDisposable
{
    private readonly string _directory;
    public string FilePath { get; }
    public PeerTrustStore Store { get; }

    public TestPeerTrustStoreFixture()
    {
        _directory = Path.Combine(Path.GetTempPath(), "whitep-peer-trust-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        FilePath = Path.Combine(_directory, "peer-trust.test-envelope");
        Store = new PeerTrustStore(FilePath, new TestOnlyPeerTrustProtector());
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

internal sealed class TestOnlyPeerTrustProtector : IUserSecretProtector
{
    private const byte Prefix = 0x6D;

    public byte[] Protect(byte[] plaintext)
    {
        var protectedPayload = new byte[plaintext.Length + 1];
        protectedPayload[0] = Prefix;
        plaintext.CopyTo(protectedPayload, 1);
        return protectedPayload;
    }

    public byte[] Unprotect(byte[] protectedPayload)
    {
        if (protectedPayload.Length < 2 || protectedPayload[0] != Prefix)
            throw new CryptographicException("Invalid test-only peer trust envelope.");
        return protectedPayload.AsSpan(1).ToArray();
    }
}

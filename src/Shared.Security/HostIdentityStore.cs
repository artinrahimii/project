using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Core.Dto;

namespace WhiteP.Shared.Security;

/// <summary>
/// Platform-neutral persistent host-key store. The caller supplies an OS-backed secret
/// protector; encrypted envelope bytes are written atomically and plaintext key buffers
/// are cleared after use. This store does not implement cryptographic primitives.
/// </summary>
public sealed class HostIdentityStore
{
    private static readonly byte[] FileMagic = [0x57, 0x50, 0x48, 0x49]; // WPHI
    private const byte FormatVersion = 1;
    private const int MaximumPrivateKeyPayloadBytes = 16 * 1024;
    private const int MaximumEnvelopeBytes = 64 * 1024;

    private readonly string _filePath;
    private readonly IUserSecretProtector _protector;
    private readonly string _mutexName;
    private readonly object _gate = new();

    public HostIdentityStore(string filePath, IUserSecretProtector protector)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A host identity storage path is required.", nameof(filePath));
        ArgumentNullException.ThrowIfNull(protector);
        _filePath = Path.GetFullPath(filePath);
        _protector = protector;
        string mutexIdentity = OperatingSystem.IsWindows() ? _filePath.ToUpperInvariant() : _filePath;
        _mutexName = "WhiteP.HostIdentityStore." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(mutexIdentity)));
    }

    /// <summary>
    /// Loads the existing identity or creates one Ed25519 identity if no file exists.
    /// Existing unreadable or invalid data fails closed and is never silently replaced.
    /// </summary>
    public Identity LoadOrCreate()
    {
        // The in-process gate does not coordinate independent HostIdentityStore instances.
        // Serialize first-run creation across processes that use the same normalized path.
        using var mutex = new Mutex(initiallyOwned: false, name: _mutexName);
        bool ownsMutex = false;
        try
        {
            try
            {
                ownsMutex = mutex.WaitOne(TimeSpan.FromSeconds(30));
            }
            catch (AbandonedMutexException)
            {
                // An earlier owner exited unexpectedly; this thread now owns the mutex.
                ownsMutex = true;
            }

            if (!ownsMutex)
                throw new TimeoutException("Timed out waiting to access the persisted host identity.");

            lock (_gate)
            {
                if (File.Exists(_filePath))
                    return LoadExisting();

                string? directory = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(directory))
                    throw new InvalidOperationException("The host identity path must have a parent directory.");

                Directory.CreateDirectory(directory);
                if (File.Exists(_filePath))
                    return LoadExisting();

                Identity candidate = new(keyType: KeyType.Ed25519);
                PrivateKey privateKey = candidate.PrivateKey
                    ?? throw new InvalidOperationException("The libp2p identity generator did not produce a private key.");
                ValidatePrivateKey(privateKey);

                byte[] serializedPrivateKey = privateKey.ToByteArray();
                byte[] protectedPayload = Array.Empty<byte>();
                byte[] envelope = Array.Empty<byte>();
                try
                {
                    if (serializedPrivateKey.Length is 0 or > MaximumPrivateKeyPayloadBytes)
                        throw new InvalidDataException("Generated host identity payload is outside the permitted size.");

                    protectedPayload = _protector.Protect(serializedPrivateKey);
                    ValidateProtectedPayload(protectedPayload);
                    envelope = BuildEnvelope(protectedPayload);

                    if (WriteNewFileWithoutOverwrite(envelope))
                        return candidate;

                    // A non-cooperating writer may have won; reload rather than returning a different identity.
                    return LoadExisting();
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(serializedPrivateKey);
                    CryptographicOperations.ZeroMemory(protectedPayload);
                    CryptographicOperations.ZeroMemory(envelope);
                }
            }
        }
        finally
        {
            if (ownsMutex)
                mutex.ReleaseMutex();
        }
    }

    private Identity LoadExisting()
    {
        byte[] envelope = ReadBoundedEnvelope();
        byte[] protectedPayload = Array.Empty<byte>();
        byte[] serializedPrivateKey = Array.Empty<byte>();
        try
        {
            if (envelope.Length < FileMagic.Length + 2 ||
                !envelope.AsSpan(0, FileMagic.Length).SequenceEqual(FileMagic))
                throw new InvalidDataException("The stored host identity envelope is invalid.");

            if (envelope[FileMagic.Length] != FormatVersion)
                throw new InvalidDataException("The stored host identity version is not supported.");

            protectedPayload = envelope.AsSpan(FileMagic.Length + 1).ToArray();
            ValidateProtectedPayload(protectedPayload);
            serializedPrivateKey = _protector.Unprotect(protectedPayload);
            if (serializedPrivateKey.Length is 0 or > MaximumPrivateKeyPayloadBytes)
                throw new InvalidDataException("The unprotected host identity payload is outside the permitted size.");

            PrivateKey privateKey;
            try
            {
                privateKey = PrivateKey.Parser.ParseFrom(serializedPrivateKey);
            }
            catch (InvalidProtocolBufferException exception)
            {
                throw new InvalidDataException("The stored host identity payload is malformed.", exception);
            }

            ValidatePrivateKey(privateKey);
            try
            {
                Identity identity = new(privateKey);
                if (identity.PrivateKey is null)
                    throw new InvalidDataException("Stored host identity has no private key.");
                _ = new PeerId(identity.PublicKey);
                return identity;
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException or CryptographicException or InvalidOperationException or IndexOutOfRangeException)
            {
                throw new InvalidDataException("The stored host identity could not be validated.", exception);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(envelope);
            CryptographicOperations.ZeroMemory(protectedPayload);
            CryptographicOperations.ZeroMemory(serializedPrivateKey);
        }
    }

    private byte[] ReadBoundedEnvelope()
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < (FileMagic.Length + 2L) || stream.Length > MaximumEnvelopeBytes)
            throw new InvalidDataException("The stored host identity file has an invalid size.");

        var envelope = new byte[(int)stream.Length];
        stream.ReadExactly(envelope);
        if (stream.ReadByte() != -1)
        {
            CryptographicOperations.ZeroMemory(envelope);
            throw new InvalidDataException("The stored host identity file changed while being read.");
        }
        return envelope;
    }

    private bool WriteNewFileWithoutOverwrite(byte[] envelope)
    {
        string directory = Path.GetDirectoryName(_filePath)!;
        string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.WriteThrough))
            {
                stream.Write(envelope);
                stream.Flush(flushToDisk: true);
            }

            try
            {
                File.Move(temporaryPath, _filePath);
                return true;
            }
            catch (IOException) when (File.Exists(_filePath))
            {
                return false;
            }
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static byte[] BuildEnvelope(byte[] protectedPayload)
    {
        var envelope = new byte[FileMagic.Length + 1 + protectedPayload.Length];
        FileMagic.CopyTo(envelope, 0);
        envelope[FileMagic.Length] = FormatVersion;
        protectedPayload.CopyTo(envelope, FileMagic.Length + 1);
        return envelope;
    }

    private static void ValidateProtectedPayload(byte[] payload)
    {
        if (payload.Length == 0 || payload.Length > (MaximumEnvelopeBytes - FileMagic.Length - 1))
            throw new InvalidDataException("The protected host identity payload is outside the permitted size.");
    }

    private static void ValidatePrivateKey(PrivateKey privateKey)
    {
        if (privateKey.Type != KeyType.Ed25519 || privateKey.Data.Length is 0 or > MaximumPrivateKeyPayloadBytes)
            throw new InvalidDataException("Only a valid, non-empty Ed25519 host identity is accepted.");
    }
}

/// <summary>
/// A platform adapter must protect key bytes using an operating-system credential store.
/// Test-only protectors must never be used by a production host.
/// </summary>
public interface IUserSecretProtector
{
    byte[] Protect(byte[] plaintext);
    byte[] Unprotect(byte[] protectedPayload);
}

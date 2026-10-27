using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Nethermind.Libp2p.Core;

namespace WhiteP.Shared.Security;

/// <summary>
/// Persists explicitly approved libp2p PeerIds. Unknown peers are never trusted by default.
/// The caller must collect explicit user approval before calling TrustPeerAfterExplicitConfirmation.
/// The trust list is protected by the same OS-backed secret-protector boundary as host identity.
/// </summary>
public sealed class PeerTrustStore
{
    public const int MaximumTrustedPeers = 256;
    private const int SchemaVersion = 1;
    private const int MaximumDocumentBytes = 64 * 1024;
    private const int MaximumEnvelopeBytes = 128 * 1024;
    private static readonly byte[] FileMagic = [0x57, 0x50, 0x54, 0x53]; // WPTS
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private readonly string _filePath;
    private readonly IUserSecretProtector _protector;
    private readonly string _mutexName;
    private readonly object _gate = new();

    public PeerTrustStore(string filePath, IUserSecretProtector protector)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A peer trust storage path is required.", nameof(filePath));
        ArgumentNullException.ThrowIfNull(protector);
        _filePath = Path.GetFullPath(filePath);
        _protector = protector;
        string mutexIdentity = OperatingSystem.IsWindows() ? _filePath.ToUpperInvariant() : _filePath;
        _mutexName = "WhiteP.PeerTrustStore." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(mutexIdentity)));
    }

    public PeerTrustStatus GetStatus(string peerId)
    {
        string canonicalPeerId = ValidateCanonicalPeerId(peerId);
        return WithPathLock(() => LoadTrustedPeerIds().Contains(canonicalPeerId)
            ? PeerTrustStatus.Trusted
            : PeerTrustStatus.Unknown);
    }

    public bool IsTrusted(string peerId) => GetStatus(peerId) == PeerTrustStatus.Trusted;

    public void EnsureTrusted(string peerId)
    {
        string canonicalPeerId = ValidateCanonicalPeerId(peerId);
        if (GetStatus(canonicalPeerId) != PeerTrustStatus.Trusted)
            throw new AuthenticationException("The peer identity has not been explicitly trusted.");
    }

    /// <summary>
    /// Records a trust-on-first-use decision only after the caller has explicitly confirmed
    /// the exact PeerId shown to the user. A different PeerId is a new, untrusted identity.
    /// </summary>
    public void TrustPeerAfterExplicitConfirmation(string peerId, bool explicitlyConfirmed)
    {
        string canonicalPeerId = ValidateCanonicalPeerId(peerId);
        if (!explicitlyConfirmed)
            throw new AuthenticationException("Peer trust was not explicitly confirmed.");

        _ = WithPathLock(() =>
        {
            HashSet<string> peers = LoadTrustedPeerIds();
            if (peers.Contains(canonicalPeerId))
                return true;

            if (peers.Count >= MaximumTrustedPeers)
                throw new InvalidDataException("The peer trust store has reached its configured capacity.");

            peers.Add(canonicalPeerId);
            SaveTrustedPeerIds(peers);
            return true;
        });
    }

    private T WithPathLock<T>(Func<T> action)
    {
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
                ownsMutex = true;
            }

            if (!ownsMutex)
                throw new TimeoutException("Timed out waiting to access the peer trust store.");

            lock (_gate)
                return action();
        }
        finally
        {
            if (ownsMutex)
                mutex.ReleaseMutex();
        }
    }

    private HashSet<string> LoadTrustedPeerIds()
    {
        if (!File.Exists(_filePath))
            return new HashSet<string>(StringComparer.Ordinal);

        byte[] envelope = ReadBoundedEnvelope();
        byte[] protectedPayload = Array.Empty<byte>();
        byte[] plaintext = Array.Empty<byte>();
        try
        {
            if (envelope.Length < FileMagic.Length + 2 ||
                !envelope.AsSpan(0, FileMagic.Length).SequenceEqual(FileMagic))
                throw new InvalidDataException("The stored peer trust envelope is invalid.");

            if (envelope[FileMagic.Length] != SchemaVersion)
                throw new InvalidDataException("The stored peer trust schema version is unsupported.");

            protectedPayload = envelope.AsSpan(FileMagic.Length + 1).ToArray();
            if (protectedPayload.Length == 0)
                throw new InvalidDataException("The stored peer trust payload is empty.");

            plaintext = _protector.Unprotect(protectedPayload);
            if (plaintext.Length is 0 or > MaximumDocumentBytes)
                throw new InvalidDataException("The stored peer trust document is outside the permitted size.");

            TrustDocument? document;
            try
            {
                document = JsonSerializer.Deserialize<TrustDocument>(plaintext, JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The stored peer trust document is malformed.", exception);
            }

            if (document is null || document.SchemaVersion != SchemaVersion ||
                document.PeerIds is null || document.PeerIds.Length > MaximumTrustedPeers)
                throw new InvalidDataException("The stored peer trust document is invalid.");

            var trusted = new HashSet<string>(StringComparer.Ordinal);
            foreach (string? storedPeerId in document.PeerIds)
            {
                if (string.IsNullOrWhiteSpace(storedPeerId))
                    throw new InvalidDataException("The stored peer trust document contains an empty identity.");

                string canonical;
                try
                {
                    canonical = ValidateCanonicalPeerId(storedPeerId);
                }
                catch (AuthenticationException exception)
                {
                    throw new InvalidDataException("The stored peer trust document contains an invalid identity.", exception);
                }

                if (!string.Equals(canonical, storedPeerId, StringComparison.Ordinal) || !trusted.Add(canonical))
                    throw new InvalidDataException("The stored peer trust document contains a non-canonical or duplicate identity.");
            }

            return trusted;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(envelope);
            CryptographicOperations.ZeroMemory(protectedPayload);
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private byte[] ReadBoundedEnvelope()
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < FileMagic.Length + 2L || stream.Length > MaximumEnvelopeBytes)
            throw new InvalidDataException("The stored peer trust file has an invalid size.");

        var envelope = new byte[(int)stream.Length];
        stream.ReadExactly(envelope);
        if (stream.ReadByte() != -1)
        {
            CryptographicOperations.ZeroMemory(envelope);
            throw new InvalidDataException("The stored peer trust file changed while being read.");
        }

        return envelope;
    }

    private void SaveTrustedPeerIds(HashSet<string> peerIds)
    {
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(
            new TrustDocument
            {
                SchemaVersion = SchemaVersion,
                PeerIds = peerIds.OrderBy(id => id, StringComparer.Ordinal).ToArray()
            },
            JsonOptions);
        byte[] protectedPayload = Array.Empty<byte>();
        byte[] envelope = Array.Empty<byte>();
        try
        {
            if (plaintext.Length is 0 or > MaximumDocumentBytes)
                throw new InvalidDataException("The peer trust document is outside the permitted size.");

            protectedPayload = _protector.Protect(plaintext);
            if (protectedPayload.Length is 0 or > (MaximumEnvelopeBytes - FileMagic.Length - 1))
                throw new InvalidDataException("The protected peer trust payload is outside the permitted size.");

            envelope = new byte[FileMagic.Length + 1 + protectedPayload.Length];
            FileMagic.CopyTo(envelope, 0);
            envelope[FileMagic.Length] = SchemaVersion;
            protectedPayload.CopyTo(envelope, FileMagic.Length + 1);
            WriteAtomically(envelope);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
            CryptographicOperations.ZeroMemory(protectedPayload);
            CryptographicOperations.ZeroMemory(envelope);
        }
    }

    private void WriteAtomically(byte[] envelope)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrEmpty(directory))
            throw new InvalidOperationException("The peer trust storage path must have a parent directory.");
        Directory.CreateDirectory(directory);
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

            File.Move(temporaryPath, _filePath, overwrite: true);
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

    private static string ValidateCanonicalPeerId(string peerId)
    {
        PeerSecurityPolicy.ValidatePeerIdentity(peerId);
        string canonical = new PeerId(peerId).ToString();
        if (!string.Equals(canonical, peerId, StringComparison.Ordinal))
            throw new AuthenticationException("The peer identity is not in canonical libp2p form.");
        return canonical;
    }

    private sealed class TrustDocument
    {
        public int SchemaVersion { get; set; }
        public string[]? PeerIds { get; set; }
    }
}

public enum PeerTrustStatus
{
    Unknown,
    Trusted
}

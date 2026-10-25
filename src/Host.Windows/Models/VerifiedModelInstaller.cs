using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using WhiteP.Host.Windows.Models.Catalog;

namespace WhiteP.Host.Windows.Models;

/// <summary>Result of installing a downloaded model after SHA-256 verification.</summary>
public sealed record ModelInstallationResult(string InstalledPath, long SizeBytes, string Sha256);

/// <summary>
/// Downloads to a unique staging file in the target directory, hashes the completed file, and only
/// then promotes it to the final path. The caller supplies a downloader with a trusted host allowlist.
/// </summary>
public sealed class VerifiedModelInstaller
{
    private readonly ModelStreamingDownloader _downloader;

    public VerifiedModelInstaller(ModelStreamingDownloader downloader)
    {
        ArgumentNullException.ThrowIfNull(downloader);
        _downloader = downloader;
    }

    public async Task<ModelInstallationResult> DownloadAndInstallAsync(
        ModelCatalogEntry model,
        string destinationPath,
        IProgress<ModelDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ValidateDeclaredFormat(model.Format);
        var expectedHash = ParseExpectedSha256(model);

        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestinationPath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("The final model path must have a parent directory.", nameof(destinationPath));
        }

        Directory.CreateDirectory(directory);
        if (File.Exists(fullDestinationPath))
        {
            throw new IOException("The destination model already exists; refusing to overwrite it.");
        }

        // Keep staging and destination on the same filesystem so final promotion is a rename.
        var temporaryPath = Path.Combine(directory, $".whitep-download-{Guid.NewGuid():N}.tmp");
        try
        {
            var sizeBytes = await _downloader.DownloadAsync(model, temporaryPath, progress, cancellationToken)
                .ConfigureAwait(false);
            byte[] actualHash;
            await using (var input = new FileStream(
                temporaryPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                ModelStreamingDownloader.BufferSizeBytes,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                actualHash = await SHA256.HashDataAsync(input, cancellationToken).ConfigureAwait(false);
            }

            if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            {
                throw new InvalidDataException("The downloaded model SHA-256 does not match the catalog metadata.");
            }

            await ValidateModelFileFormatAsync(model.Format, temporaryPath, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullDestinationPath);
            return new ModelInstallationResult(
                fullDestinationPath,
                sizeBytes,
                Convert.ToHexString(actualHash).ToLowerInvariant());
        }
        catch
        {
            TryDelete(temporaryPath);
            throw;
        }
    }

    private static void ValidateDeclaredFormat(string format)
    {
        if (!string.Equals(format, "GGUF", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Only GGUF model files can be installed by this catalog schema version.");
        }
    }

    private static async Task ValidateModelFileFormatAsync(
        string format,
        string path,
        CancellationToken cancellationToken)
    {
        ValidateDeclaredFormat(format);
        var header = new byte[8];
        var bytesRead = 0;
        await using (var input = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            header.Length,
            FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            while (bytesRead < header.Length)
            {
                var count = await input.ReadAsync(
                    header.AsMemory(bytesRead, header.Length - bytesRead),
                    cancellationToken).ConfigureAwait(false);
                if (count == 0)
                {
                    break;
                }

                bytesRead += count;
            }
        }

        if (bytesRead < header.Length ||
            header[0] != (byte)'G' ||
            header[1] != (byte)'G' ||
            header[2] != (byte)'U' ||
            header[3] != (byte)'F')
        {
            throw new InvalidDataException("The downloaded file does not contain a complete GGUF header.");
        }

        var version = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4, 4));
        if (version is < 1 or > 3)
        {
            throw new InvalidDataException("The GGUF header version is not supported by this installer.");
        }
    }

    private static byte[] ParseExpectedSha256(ModelCatalogEntry model)
    {
        if (!string.Equals(model.ChecksumAlgorithm, "sha256", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Only catalog entries with a SHA-256 checksum can be installed.");
        }

        try
        {
            var hash = Convert.FromHexString(model.ChecksumValue);
            if (hash.Length != 32)
            {
                throw new InvalidDataException("The catalog SHA-256 checksum must contain exactly 32 bytes.");
            }

            return hash;
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("The catalog SHA-256 checksum is not valid hexadecimal.", exception);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Preserve the original download, checksum, cancellation, or promotion failure.
        }
        catch (UnauthorizedAccessException)
        {
            // Preserve the original download, checksum, cancellation, or promotion failure.
        }
    }
}

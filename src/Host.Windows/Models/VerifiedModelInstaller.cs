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
    private readonly Func<string, long> _availableFreeSpaceBytesProvider;

    public VerifiedModelInstaller(
        ModelStreamingDownloader downloader,
        Func<string, long>? availableFreeSpaceBytesProvider = null)
    {
        ArgumentNullException.ThrowIfNull(downloader);
        _downloader = downloader;
        _availableFreeSpaceBytesProvider = availableFreeSpaceBytesProvider ?? GetAvailableFreeSpaceBytes;
    }

    public Task<ModelInstallationResult> DownloadAndInstallAsync(
        ModelCatalogEntry model,
        string destinationPath,
        IProgress<ModelDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        DownloadAndFinalizeAsync(model, destinationPath, progress, cancellationToken, replaceExisting: false);

    /// <summary>
    /// Downloads and validates a replacement beside the installed file, then atomically swaps it.
    /// The prior file is not touched until the replacement passes size, SHA-256, and GGUF checks.
    /// </summary>
    public Task<ModelInstallationResult> DownloadAndUpdateAsync(
        ModelCatalogEntry model,
        string destinationPath,
        IProgress<ModelDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        DownloadAndFinalizeAsync(model, destinationPath, progress, cancellationToken, replaceExisting: true);

    private async Task<ModelInstallationResult> DownloadAndFinalizeAsync(
        ModelCatalogEntry model,
        string destinationPath,
        IProgress<ModelDownloadProgress>? progress,
        CancellationToken cancellationToken,
        bool replaceExisting)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ValidateDeclaredFormat(model.Format);
        if (model.SizeBytes <= 0)
        {
            throw new InvalidDataException("Model metadata must declare a positive file size.");
        }

        var expectedHash = ParseExpectedSha256(model);

        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestinationPath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("The final model path must have a parent directory.", nameof(destinationPath));
        }

        Directory.CreateDirectory(directory);
        var originalFileLength = 0L;
        var originalFileLastWriteUtc = DateTime.MinValue;
        if (replaceExisting)
        {
            if (!File.Exists(fullDestinationPath))
            {
                throw new FileNotFoundException("The installed model to update does not exist.", fullDestinationPath);
            }

            var attributes = File.GetAttributes(fullDestinationPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                throw new IOException("The update destination must be a regular model file, not a directory or link.");
            }

            var originalFile = new FileInfo(fullDestinationPath);
            originalFile.Refresh();
            originalFileLength = originalFile.Length;
            originalFileLastWriteUtc = originalFile.LastWriteTimeUtc;
        }
        else if (File.Exists(fullDestinationPath))
        {
            throw new IOException("The destination model already exists; refusing to overwrite it.");
        }

        var availableFreeSpaceBytes = _availableFreeSpaceBytesProvider(directory);
        if (availableFreeSpaceBytes < model.SizeBytes)
        {
            throw new IOException(
                $"Insufficient free disk space for the model staging file. Required at least {model.SizeBytes} bytes; available {availableFreeSpaceBytes} bytes. The installed model was not changed.");
        }

        // Keep staging, destination, and backup on the same filesystem for atomic promotion.
        var temporaryPath = Path.Combine(directory, $".whitep-download-{Guid.NewGuid():N}.tmp");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
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
            if (replaceExisting)
            {
                var currentFile = new FileInfo(fullDestinationPath);
                currentFile.Refresh();
                if (!currentFile.Exists ||
                    currentFile.Length != originalFileLength ||
                    currentFile.LastWriteTimeUtc != originalFileLastWriteUtc)
                {
                    throw new IOException("The installed model changed during update; no replacement was made.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                var backupPath = Path.Combine(directory, $".whitep-update-{Guid.NewGuid():N}.bak");
                File.Replace(temporaryPath, fullDestinationPath, backupPath, ignoreMetadataErrors: true);
                // Replacement is already committed atomically; backup cleanup must not turn that
                // successful operation into a misleading failure. If deletion is denied, preserve
                // the backup rather than risking the verified replacement or original data.
                TryDelete(backupPath);
            }
            else
            {
                File.Move(temporaryPath, fullDestinationPath);
            }

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

    private static long GetAvailableFreeSpaceBytes(string directory)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(directory));
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new IOException("The destination storage volume could not be identified.");
        }

        var drive = new DriveInfo(root);
        if (!drive.IsReady)
        {
            throw new IOException("The destination storage volume is not ready for a model download.");
        }

        var available = drive.AvailableFreeSpace;
        if (available < 0)
        {
            throw new IOException("The destination storage volume returned an invalid free-space measurement.");
        }

        return available;
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

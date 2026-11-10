using System.Security.Cryptography;

namespace WhiteP.Host.Windows.Models.Catalog;

/// <summary>
/// Confirms that the active backend model ID matches the catalog selection and that the installed
/// local file still matches the SHA-256 persisted by the catalog-validated installer.
/// </summary>
public static class InstalledCatalogModelVerifier
{
    public static async Task<bool> VerifyAsync(
        string? activeCatalogModelId,
        string? activeBackendModelId,
        string? installedModelPath,
        string? expectedSha256,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(activeCatalogModelId) ||
            !string.Equals(activeCatalogModelId, activeBackendModelId, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(installedModelPath) ||
            string.IsNullOrWhiteSpace(expectedSha256))
        {
            return false;
        }

        byte[] expectedHash;
        try { expectedHash = Convert.FromHexString(expectedSha256); }
        catch (FormatException) { return false; }
        if (expectedHash.Length != 32) return false;

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(installedModelPath);
            if (!File.Exists(fullPath)) return false;
            var attributes = File.GetAttributes(fullPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0) return false;
        }
        catch (Exception exception) when (
            exception is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }

        try
        {
            await using var stream = new FileStream(
                fullPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            if (stream.Length <= 0) return false;
            var actualHash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }
}

using System.IO;

namespace WhiteP.Host.Windows.Models.Catalog;

/// <summary>
/// Loads a validated catalog and keeps the last successfully validated bytes for offline use.
/// The cache is untrusted input and is always revalidated with the current allowlists.
/// </summary>
public sealed class OfflineModelCatalogStore
{
    public ModelCatalog Load(
        string catalogPath,
        string cachePath,
        IEnumerable<string> allowedSourceHosts,
        IEnumerable<string> supportedBackendIds,
        IEnumerable<string>? allowedLicenseHosts,
        out bool usedCache)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(cachePath);
        usedCache = false;
        var loader = new ModelCatalogFileLoader();

        try
        {
            var catalog = loader.Load(catalogPath, allowedSourceHosts, supportedBackendIds, allowedLicenseHosts);
            SaveValidatedCopy(catalogPath, cachePath);
            return catalog;
        }
        catch (Exception sourceError) when (IsCatalogLoadFailure(sourceError))
        {
            if (!File.Exists(cachePath))
                throw;

            try
            {
                var cached = loader.Load(cachePath, allowedSourceHosts, supportedBackendIds, allowedLicenseHosts);
                usedCache = true;
                return cached;
            }
            catch (Exception cacheError) when (IsCatalogLoadFailure(cacheError))
            {
                throw new InvalidDataException(
                    "The selected catalog could not be loaded and the cached catalog is missing or invalid.",
                    new AggregateException(sourceError, cacheError));
            }
        }
    }

    private static bool IsCatalogLoadFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;

    private static void SaveValidatedCopy(string sourcePath, string cachePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(cachePath))!;
        Directory.CreateDirectory(directory);
        var temporaryPath = cachePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.Copy(sourcePath, temporaryPath, overwrite: false);
            File.Move(temporaryPath, cachePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}

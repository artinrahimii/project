namespace WhiteP.Host.Windows.Models.Catalog;

/// <summary>The parsed metadata document from a versioned local-model catalog.</summary>
public sealed record ModelCatalog(int SchemaVersion, IReadOnlyList<ModelCatalogEntry> Models);

/// <summary>Metadata for one model. This record does not imply download, license approval, or installation.</summary>
public sealed record ModelCatalogEntry(
    string Id,
    string Name,
    string Format,
    long SizeBytes,
    string ChecksumAlgorithm,
    string ChecksumValue,
    IReadOnlyList<string> BackendIds,
    Uri SourceUrl,
    ModelLicenseMetadata License);

/// <summary>License evidence and review state for a model entry.</summary>
public sealed record ModelLicenseMetadata(string Status, string? Identifier, Uri? Url)
{
    public bool RequiresReview => Status is "review_required" or "unknown";

    /// <summary>Human-readable warning label for catalogs that lack complete license evidence.</summary>
    public string ReviewLabel => RequiresReview ? "REVIEW REQUIRED" : "Declared; not independently verified";
}

namespace WhiteP.Host.Windows.Models.Catalog;

// Metadata-only catalog types: declarations here do not independently establish legal permission.
public sealed record ModelCatalog(int SchemaVersion, IReadOnlyList<ModelCatalogEntry> Models);

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

/// <summary>Catalog license evidence, separate from the catalog's claimed usage classification.</summary>
public sealed record ModelLicenseMetadata(
    string Status,
    string? Identifier,
    Uri? Url,
    string Classification = "unknown")
{
    public bool HasDeclaredRestrictions => Classification == "restricted";

    public bool RequiresReview =>
        Status is "review_required" or "unknown" ||
        Classification == "unknown";

    /// <summary>A visible disclosure label; an open-source classification remains a catalog claim.</summary>
    public string ReviewLabel => Classification switch
    {
        "restricted" => "RESTRICTIONS REPORTED — NOT OPEN SOURCE",
        "open_source" when !RequiresReview => "Open-source claim declared; not independently verified",
        _ => "REVIEW REQUIRED — DO NOT ASSUME OPEN SOURCE"
    };

    /// <summary>Plain-language warning shown before model update and in selected-model details.</summary>
    public string UsageDisclaimer => Classification switch
    {
        "restricted" =>
            "The catalog reports usage restrictions. Do not present this model as open source. Review the linked terms before use, modification, or redistribution.",
        "open_source" when !RequiresReview =>
            "The catalog declares an open-source classification, but this app has not independently verified the legal terms or license conditions. Review the linked terms before use or redistribution.",
        _ =>
            "The license classification is missing, unverified, or requires review. Do not assume the model is open source or that use, modification, or redistribution is permitted."
    };
}

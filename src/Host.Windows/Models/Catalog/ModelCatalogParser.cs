using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WhiteP.Host.Windows.Models.Catalog;

/// <summary>
/// Strict, fail-closed parser for model catalog metadata. It performs no network access.
/// Source and license URL host allowlists and currently available backend IDs must be supplied
/// by trusted application configuration rather than by untrusted catalog JSON.
/// </summary>
public sealed class ModelCatalogParser
{
    public const int CurrentSchemaVersion = 1;
    public const int MaximumCatalogBytes = 2 * 1024 * 1024;
    public const int MaximumCatalogModels = 10_000;

    private static readonly Regex IdentifierPattern = new(
        "^[a-z0-9][a-z0-9._-]{0,127}$",
        RegexOptions.CultureInvariant);
    private static readonly Regex BackendIdentifierPattern = new(
        "^[a-z0-9][a-z0-9._-]{0,63}$",
        RegexOptions.CultureInvariant);
    private static readonly Regex Sha256Pattern = new(
        "^[A-Fa-f0-9]{64}$",
        RegexOptions.CultureInvariant);
    private static readonly HashSet<string> SchemaV1BackendIds = new(StringComparer.Ordinal)
    {
        "llama.cpp"
    };

    private readonly HashSet<string> _allowedSourceHosts;
    private readonly HashSet<string> _allowedLicenseHosts;
    private readonly HashSet<string> _supportedBackendIds;

    public ModelCatalogParser(
        IEnumerable<string> allowedSourceHosts,
        IEnumerable<string> supportedBackendIds,
        IEnumerable<string>? allowedLicenseHosts = null)
    {
        _allowedSourceHosts = NormalizeHostSet(allowedSourceHosts, nameof(allowedSourceHosts));
        _allowedLicenseHosts = allowedLicenseHosts is null
            ? new HashSet<string>(_allowedSourceHosts, StringComparer.OrdinalIgnoreCase)
            : NormalizeHostSet(allowedLicenseHosts, nameof(allowedLicenseHosts));

        ArgumentNullException.ThrowIfNull(supportedBackendIds);
        _supportedBackendIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var backendId in supportedBackendIds)
        {
            if (string.IsNullOrWhiteSpace(backendId) ||
                !string.Equals(backendId, backendId.Trim(), StringComparison.Ordinal) ||
                !BackendIdentifierPattern.IsMatch(backendId))
            {
                throw new ArgumentException("Supported backend IDs must be stable identifiers.", nameof(supportedBackendIds));
            }

            _supportedBackendIds.Add(backendId);
        }

        if (_supportedBackendIds.Count == 0)
        {
            throw new ArgumentException("At least one supported backend ID is required.", nameof(supportedBackendIds));
        }
    }

    public ModelCatalog Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw Invalid("Catalog input is empty.");
        }

        if (Encoding.UTF8.GetByteCount(json) > MaximumCatalogBytes)
        {
            throw Invalid("Catalog input exceeds the maximum allowed size.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 32
            });
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Catalog input is not valid JSON.", exception);
        }

        using (document)
        {
            var root = document.RootElement;
            EnsureObjectProperties(root, "$", "schemaVersion", "models");

            var schemaVersion = GetRequired(root, "schemaVersion", "$");
            if (schemaVersion.ValueKind != JsonValueKind.Number ||
                !schemaVersion.TryGetInt32(out var version) ||
                version != CurrentSchemaVersion)
            {
                throw Invalid("Unsupported catalog schemaVersion.");
            }

            var modelsElement = GetRequired(root, "models", "$");
            if (modelsElement.ValueKind != JsonValueKind.Array)
            {
                throw Invalid("Catalog models must be an array.");
            }

            if (modelsElement.GetArrayLength() > MaximumCatalogModels)
            {
                throw Invalid("Catalog contains too many model records.");
            }

            var models = new List<ModelCatalogEntry>();
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in modelsElement.EnumerateArray())
            {
                var entry = ParseModel(item);
                if (!identifiers.Add(entry.Id))
                {
                    throw Invalid("Catalog contains a duplicate model ID.");
                }

                models.Add(entry);
            }

            return new ModelCatalog(CurrentSchemaVersion, Array.AsReadOnly(models.ToArray()));
        }
    }

    private ModelCatalogEntry ParseModel(JsonElement element)
    {
        EnsureObjectProperties(
            element,
            "models[]",
            "id", "name", "format", "sizeBytes", "checksum", "backend", "sourceUrl", "license");

        var id = GetRequiredString(element, "id", "models[]", 128);
        if (!IdentifierPattern.IsMatch(id))
        {
            throw Invalid("A model ID has an invalid format.");
        }

        var name = GetRequiredString(element, "name", "models[]", 160).Trim();
        if (name.Length == 0)
        {
            throw Invalid("A model name must not be empty.");
        }

        var format = GetRequiredString(element, "format", "models[]", 32);
        if (!string.Equals(format, "GGUF", StringComparison.Ordinal))
        {
            throw Invalid("This catalog schema version supports only the GGUF format.");
        }

        var sizeElement = GetRequired(element, "sizeBytes", "models[]");
        if (sizeElement.ValueKind != JsonValueKind.Number ||
            !sizeElement.TryGetInt64(out var sizeBytes) ||
            sizeBytes < 1)
        {
            throw Invalid("Model sizeBytes must be a positive integer.");
        }

        var checksumElement = GetRequired(element, "checksum", "models[]");
        EnsureObjectProperties(checksumElement, "models[].checksum", "algorithm", "value");
        var algorithm = GetRequiredString(checksumElement, "algorithm", "models[].checksum", 16);
        if (!string.Equals(algorithm, "sha256", StringComparison.Ordinal))
        {
            throw Invalid("Only the sha256 checksum algorithm is supported.");
        }

        var checksumValue = GetRequiredString(checksumElement, "value", "models[].checksum", 64);
        if (!Sha256Pattern.IsMatch(checksumValue))
        {
            throw Invalid("The model checksum must contain exactly 64 hexadecimal digits.");
        }

        var backendElement = GetRequired(element, "backend", "models[]");
        if (backendElement.ValueKind != JsonValueKind.Array || backendElement.GetArrayLength() == 0)
        {
            throw Invalid("A model must declare at least one compatible backend.");
        }

        var backendIds = new List<string>();
        var uniqueBackendIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var backend in backendElement.EnumerateArray())
        {
            if (backend.ValueKind != JsonValueKind.String)
            {
                throw Invalid("Backend IDs must be strings.");
            }

            var backendId = backend.GetString()!;
            if (!uniqueBackendIds.Add(backendId))
            {
                throw Invalid("A model contains a duplicate backend ID.");
            }

            if (!SchemaV1BackendIds.Contains(backendId))
            {
                throw Invalid("The model declares a backend unsupported by catalog schema version 1.");
            }

            if (!_supportedBackendIds.Contains(backendId))
            {
                throw Invalid("The model requires a backend not supported by this host configuration.");
            }

            backendIds.Add(backendId);
        }

        var sourceText = GetRequiredString(element, "sourceUrl", "models[]", 2048);
        var sourceUrl = ParseAllowlistedHttpsUrl(sourceText, _allowedSourceHosts, "sourceUrl");
        var license = element.TryGetProperty("license", out var licenseElement)
            ? ParseLicense(licenseElement)
            : new ModelLicenseMetadata("review_required", null, null);

        return new ModelCatalogEntry(
            id,
            name,
            format,
            sizeBytes,
            "sha256",
            checksumValue.ToLowerInvariant(),
            Array.AsReadOnly(backendIds.ToArray()),
            sourceUrl,
            license);
    }

    private ModelLicenseMetadata ParseLicense(JsonElement element)
    {
        EnsureObjectProperties(element, "models[].license", "status", "identifier", "url");
        var status = GetRequiredString(element, "status", "models[].license", 32);
        if (status is not ("verified" or "review_required" or "unknown"))
        {
            throw Invalid("The model license status is not recognized.");
        }

        string? identifier = null;
        if (element.TryGetProperty("identifier", out var identifierElement))
        {
            if (identifierElement.ValueKind != JsonValueKind.String)
            {
                throw Invalid("License identifier must be a string.");
            }

            identifier = identifierElement.GetString();
            if (string.IsNullOrWhiteSpace(identifier) || identifier.Length > 128 ||
                !string.Equals(identifier, identifier.Trim(), StringComparison.Ordinal))
            {
                throw Invalid("License identifier is empty or malformed.");
            }
        }

        Uri? url = null;
        if (element.TryGetProperty("url", out var urlElement))
        {
            if (urlElement.ValueKind != JsonValueKind.String)
            {
                throw Invalid("License URL must be a string.");
            }

            var urlText = urlElement.GetString();
            if (string.IsNullOrWhiteSpace(urlText))
            {
                throw Invalid("License URL must not be empty when present.");
            }

            url = ParseAllowlistedHttpsUrl(urlText, _allowedLicenseHosts, "license.url");
        }

        if (status == "verified" && (identifier is null || url is null))
        {
            throw Invalid("A verified license requires both identifier and URL evidence.");
        }

        return new ModelLicenseMetadata(status, identifier, url);
    }

    private static Uri ParseAllowlistedHttpsUrl(
        string value,
        HashSet<string> allowlistedHosts,
        string propertyName)
    {
        if (value.Length > 2048 ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(char.IsControl) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            uri.HostNameType != UriHostNameType.Dns ||
            uri.IsLoopback ||
            uri.UserInfo.Length != 0 ||
            !uri.IsDefaultPort ||
            uri.Fragment.Length != 0)
        {
            throw Invalid($"{propertyName} must be an absolute HTTPS URL without credentials, a nonstandard port, or a fragment.");
        }

        var host = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        if (!allowlistedHosts.Contains(host))
        {
            throw Invalid($"{propertyName} host is not allowlisted.");
        }

        return uri;
    }

    private static HashSet<string> NormalizeHostSet(IEnumerable<string> hosts, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(hosts);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hostValue in hosts)
        {
            if (string.IsNullOrWhiteSpace(hostValue) ||
                !string.Equals(hostValue, hostValue.Trim(), StringComparison.Ordinal) ||
                hostValue.Contains("://", StringComparison.Ordinal) ||
                hostValue.Contains('/') ||
                hostValue.Contains('@') ||
                hostValue.Contains('?') ||
                hostValue.Contains('#') ||
                hostValue.Contains(':') ||
                !Uri.TryCreate($"https://{hostValue}", UriKind.Absolute, out var uri) ||
                uri.HostNameType != UriHostNameType.Dns ||
                !uri.IsDefaultPort ||
                uri.AbsolutePath != "/" ||
                uri.Query.Length != 0 ||
                uri.Fragment.Length != 0 ||
                uri.UserInfo.Length != 0)
            {
                throw new ArgumentException("Allowed hosts must be bare DNS host names without paths or ports.", parameterName);
            }

            var normalized = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
            if (normalized.Length == 0 ||
                normalized == "localhost" ||
                normalized.EndsWith(".localhost", StringComparison.Ordinal) ||
                normalized.EndsWith(".local", StringComparison.Ordinal))
            {
                throw new ArgumentException("Local or invalid hostnames cannot be added to the allowlist.", parameterName);
            }

            result.Add(normalized);
        }

        if (result.Count == 0)
        {
            throw new ArgumentException("At least one allowlisted host is required.", parameterName);
        }

        return result;
    }

    private static void EnsureObjectProperties(JsonElement element, string path, params string[] allowedProperties)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw Invalid($"{path} must be an object.");
        }

        var allowed = new HashSet<string>(allowedProperties, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!seen.Add(property.Name))
            {
                throw Invalid($"{path} contains a duplicate property.");
            }

            if (!allowed.Contains(property.Name))
            {
                throw Invalid($"{path} contains an unsupported property.");
            }
        }
    }

    private static JsonElement GetRequired(JsonElement parent, string name, string path)
    {
        if (!parent.TryGetProperty(name, out var value))
        {
            throw Invalid($"{path} is missing a required property.");
        }

        return value;
    }

    private static string GetRequiredString(JsonElement parent, string name, string path, int maximumLength)
    {
        var value = GetRequired(parent, name, path);
        if (value.ValueKind != JsonValueKind.String)
        {
            throw Invalid($"{path}.{name} must be a string.");
        }

        var text = value.GetString();
        if (string.IsNullOrWhiteSpace(text) || text.Length > maximumLength)
        {
            throw Invalid($"{path}.{name} is empty or exceeds its length limit.");
        }

        return text;
    }

    private static InvalidDataException Invalid(string message) =>
        new($"Invalid model catalog: {message}");
}

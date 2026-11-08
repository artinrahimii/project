using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhiteP.Host.Infrastructure;

/// <summary>
/// Validated settings for the local llama.cpp-compatible inference endpoint.
/// The persisted document is explicitly schema-versioned so future changes fail closed.
/// </summary>
public sealed record LocalHostSettings(string ServerUrl, string ModelId, int RequestTimeoutSeconds, string? EngineExecutablePath = null, string? ModelFilePath = null, string? ActiveCatalogModelId = null, bool PersistHostConversation = false, double RequiredModelMemoryGiB = 4d, double MinimumFreeDiskGiB = 1d)
{
    public const int CurrentSchemaVersion = 2;
    public static LocalHostSettings Defaults { get; } =
        new("http://127.0.0.1:8080/", "local-model", 300, RequiredModelMemoryGiB: 4d, MinimumFreeDiskGiB: 1d);

    public static LocalHostSettings Validate(LocalHostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (!Uri.TryCreate(settings.ServerUrl?.Trim(), UriKind.Absolute, out var endpoint) ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps) ||
            !endpoint.IsLoopback || endpoint.UserInfo.Length != 0 ||
            endpoint.Query.Length != 0 || endpoint.Fragment.Length != 0)
        {
            throw new ArgumentException("The local inference URL must be an absolute HTTP(S) loopback URL without credentials, query, or fragment.", nameof(settings));
        }

        var model = settings.ModelId?.Trim();
        if (string.IsNullOrWhiteSpace(model) || model.Length > 128 ||
            model.Any(char.IsControl))
        {
            throw new ArgumentException("Model ID must contain 1–128 non-control characters.", nameof(settings));
        }

        if (settings.RequestTimeoutSeconds is < 1 or > 3600)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Request timeout must be between 1 and 3600 seconds.");
        }

        if (!IsValidCapacityRequirement(settings.RequiredModelMemoryGiB))
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Required model memory must be finite, positive, and no greater than 1,048,576 GiB.");
        }

        if (!IsValidCapacityRequirement(settings.MinimumFreeDiskGiB))
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Minimum free system-drive space must be finite, positive, and no greater than 1,048,576 GiB.");
        }

        var activeCatalogModelId = NormalizeOptionalModelId(settings.ActiveCatalogModelId);
        return new LocalHostSettings(endpoint.ToString(), model, settings.RequestTimeoutSeconds,
            NormalizeOptionalPath(settings.EngineExecutablePath, nameof(settings.EngineExecutablePath)),
            NormalizeOptionalPath(settings.ModelFilePath, nameof(settings.ModelFilePath)), activeCatalogModelId, settings.PersistHostConversation,
            settings.RequiredModelMemoryGiB, settings.MinimumFreeDiskGiB);
    }

    private static bool IsValidCapacityRequirement(double requiredGiB) =>
        double.IsFinite(requiredGiB) && requiredGiB > 0 && requiredGiB <= 1_048_576d;

    private static string? NormalizeOptionalModelId(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId)) return null;
        var normalized = modelId.Trim();
        if (normalized.Length > 128 || normalized.Any(char.IsControl))
            throw new ArgumentException("Active catalog model ID must contain at most 128 non-control characters.", nameof(modelId));
        return normalized;
    }

    private static string? NormalizeOptionalPath(string? path, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try { return Path.GetFullPath(path.Trim()); }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        { throw new ArgumentException("A configured local engine path is invalid.", parameterName); }
    }

}

/// <summary>
/// Loads and atomically saves user-local settings. Corrupt or future-version files are
/// reported rather than silently overwritten.
/// </summary>
public sealed class LocalHostSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        WriteIndented = true
    };

    private readonly string _filePath;

    public LocalHostSettingsStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WhiteP",
            "settings.json");
    }

    public string FilePath => _filePath;

    public LocalHostSettings LoadOrDefault()
    {
        if (!File.Exists(_filePath))
        {
            return LocalHostSettings.Defaults;
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var document = JsonSerializer.Deserialize<SettingsDocument>(stream, JsonOptions)
                ?? throw new InvalidDataException("Local settings document is empty.");
            if (document.SchemaVersion != 1 && document.SchemaVersion != LocalHostSettings.CurrentSchemaVersion)
            {
                throw new InvalidDataException($"Unsupported local settings schema version: {document.SchemaVersion}.");
            }

            // Schema v1 predates owner-configured resource limits; migrate with existing UI defaults.
            var requiredModelMemoryGiB = document.SchemaVersion == 1 ? LocalHostSettings.Defaults.RequiredModelMemoryGiB : document.RequiredModelMemoryGiB;
            var minimumFreeDiskGiB = document.SchemaVersion == 1 ? LocalHostSettings.Defaults.MinimumFreeDiskGiB : document.MinimumFreeDiskGiB;

            return LocalHostSettings.Validate(new LocalHostSettings(
                document.ServerUrl ?? string.Empty,
                document.ModelId ?? string.Empty,
                document.RequestTimeoutSeconds,
                document.EngineExecutablePath,
                document.ModelFilePath,
                document.ActiveCatalogModelId,
                document.PersistHostConversation,
                requiredModelMemoryGiB,
                minimumFreeDiskGiB));
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Local settings JSON is invalid; the file was not overwritten.", exception);
        }
    }

    public LocalHostSettings Save(LocalHostSettings settings)
    {
        var validated = LocalHostSettings.Validate(settings);
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("Local settings path must include a directory.");
        Directory.CreateDirectory(directory);

        var document = new SettingsDocument
        {
            SchemaVersion = LocalHostSettings.CurrentSchemaVersion,
            ServerUrl = validated.ServerUrl,
            ModelId = validated.ModelId,
            RequestTimeoutSeconds = validated.RequestTimeoutSeconds,
            EngineExecutablePath = validated.EngineExecutablePath,
            ModelFilePath = validated.ModelFilePath,
            ActiveCatalogModelId = validated.ActiveCatalogModelId,
            PersistHostConversation = validated.PersistHostConversation,
            RequiredModelMemoryGiB = validated.RequiredModelMemoryGiB,
            MinimumFreeDiskGiB = validated.MinimumFreeDiskGiB
        };
        var temporaryPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, document, JsonOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, _filePath, overwrite: true);
            return validated;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private sealed class SettingsDocument
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("serverUrl")]
        public string? ServerUrl { get; init; }

        [JsonPropertyName("modelId")]
        public string? ModelId { get; init; }

        [JsonPropertyName("requestTimeoutSeconds")]
        public int RequestTimeoutSeconds { get; init; }

        [JsonPropertyName("engineExecutablePath")]
        public string? EngineExecutablePath { get; init; }

        [JsonPropertyName("modelFilePath")]
        public string? ModelFilePath { get; init; }

        [JsonPropertyName("activeCatalogModelId")]
        public string? ActiveCatalogModelId { get; init; }

        [JsonPropertyName("persistHostConversation")]
        public bool PersistHostConversation { get; init; }

        [JsonPropertyName("requiredModelMemoryGiB")]
        public double RequiredModelMemoryGiB { get; init; }

        [JsonPropertyName("minimumFreeDiskGiB")]
        public double MinimumFreeDiskGiB { get; init; }
    }
}

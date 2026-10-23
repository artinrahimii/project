using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhiteP.Host.Infrastructure;

/// <summary>
/// Validated settings for the local llama.cpp-compatible inference endpoint.
/// The persisted document is explicitly schema-versioned so future changes fail closed.
/// </summary>
public sealed record LocalHostSettings(string ServerUrl, string ModelId, int RequestTimeoutSeconds, string? EngineExecutablePath = null, string? ModelFilePath = null)
{
    public const int CurrentSchemaVersion = 1;
    public static LocalHostSettings Defaults { get; } =
        new("http://127.0.0.1:8080/", "local-model", 300);

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

        return new LocalHostSettings(endpoint.ToString(), model, settings.RequestTimeoutSeconds, NormalizeOptionalPath(settings.EngineExecutablePath, nameof(settings.EngineExecutablePath)), NormalizeOptionalPath(settings.ModelFilePath, nameof(settings.ModelFilePath)));
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
            if (document.SchemaVersion != LocalHostSettings.CurrentSchemaVersion)
            {
                throw new InvalidDataException($"Unsupported local settings schema version: {document.SchemaVersion}.");
            }

            return LocalHostSettings.Validate(new LocalHostSettings(
                document.ServerUrl ?? string.Empty,
                document.ModelId ?? string.Empty,
                document.RequestTimeoutSeconds,
                document.EngineExecutablePath,
                document.ModelFilePath));
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
            ModelFilePath = validated.ModelFilePath
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
    }
}

using System.Text.Json;
using WhiteP.Host.Infrastructure;

internal static class LocalHostSettingsPersistenceTests
{
    public static void Run()
    {
        var directory = Path.Combine(Path.GetTempPath(), "whitep-settings-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            PersistsSelectionAndPreservesServerModel(directory);
            LoadsLegacySettingsWithoutSelection(directory);
            RejectsInvalidSelectionIds(directory);
            RejectsInvalidCapacityRequirements(directory);
            RejectsSemanticallyInvalidPersistedSettings(directory);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
        Console.WriteLine("PASS: settings round-trip, schema-v1 migration, persistent RAM/disk requirements, semantic validation, invalid-selection rejection, and fail-closed persistence.");
    }
    private static void PersistsSelectionAndPreservesServerModel(string directory)
    {
        var path = Path.Combine(directory, "roundtrip.json");
        var saved = new LocalHostSettingsStore(path).Save(new LocalHostSettings(
            "http://127.0.0.1:8080/", "server-model", 120, null, null, "catalog-model-42",
            PersistHostConversation: true, RequiredModelMemoryGiB: 7.5, MinimumFreeDiskGiB: 12.25,
            ActiveCatalogModelSha256: new string('a', 64)));
        var loaded = new LocalHostSettingsStore(path).LoadOrDefault();
        Assert(loaded.ActiveCatalogModelId == "catalog-model-42", "Selected catalog model ID was not persisted.");
        Assert(loaded.ActiveCatalogModelSha256 == new string('a', 64), "The verified installed-model SHA-256 was not persisted.");
        Assert(loaded.ModelId == "server-model", "Catalog selection must not overwrite the server model ID.");
        Assert(loaded.RequestTimeoutSeconds == 120, "Catalog selection must preserve other settings.");
        Assert(saved.ActiveCatalogModelId == loaded.ActiveCatalogModelId, "Save and reload differ.");
        Assert(loaded.PersistHostConversation, "The explicit host-retention preference was not persisted.");
        Assert(loaded.RequiredModelMemoryGiB == 7.5, "The owner-configured model RAM requirement was not persisted.");
        Assert(loaded.MinimumFreeDiskGiB == 12.25, "The owner-configured minimum free disk requirement was not persisted.");
        using var savedDocument = JsonDocument.Parse(File.ReadAllText(path));
        Assert(savedDocument.RootElement.GetProperty("schemaVersion").GetInt32() == LocalHostSettings.CurrentSchemaVersion,
            "Saving settings must write the current schema version.");
    }
    private static void LoadsLegacySettingsWithoutSelection(string directory)
    {
        var path = Path.Combine(directory, "legacy.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new { schemaVersion = 1, serverUrl = "http://127.0.0.1:8080/", modelId = "legacy-server-model", requestTimeoutSeconds = 300, engineExecutablePath = (string?)null, modelFilePath = (string?)null, activeCatalogModelId = (string?)null }));
        var loaded = new LocalHostSettingsStore(path).LoadOrDefault();
        Assert(loaded.ActiveCatalogModelId is null, "Legacy settings should have no selected catalog model.");
        Assert(loaded.ActiveCatalogModelSha256 is null, "Legacy settings must not claim a checksum that was never verified.");
        Assert(loaded.ModelId == "legacy-server-model", "Legacy server model ID changed.");
        Assert(!loaded.PersistHostConversation, "Legacy settings must keep optional host history disabled by default.");
        Assert(loaded.RequiredModelMemoryGiB == 4d, "Schema v1 migration must use the documented 4 GiB RAM default.");
        Assert(loaded.MinimumFreeDiskGiB == 1d, "Schema v1 migration must use the documented 1 GiB disk default.");
        var migrated = new LocalHostSettingsStore(path).Save(loaded);
        Assert(migrated.RequiredModelMemoryGiB == 4d && migrated.MinimumFreeDiskGiB == 1d,
            "Migrated settings must retain resource defaults when written as schema v2.");
        using var migratedDocument = JsonDocument.Parse(File.ReadAllText(path));
        Assert(migratedDocument.RootElement.GetProperty("schemaVersion").GetInt32() == LocalHostSettings.CurrentSchemaVersion,
            "The next successful save must upgrade schema v1 to the current schema version.");
    }
    private static void RejectsInvalidSelectionIds(string directory)
    {
        var store = new LocalHostSettingsStore(Path.Combine(directory, "invalid.json"));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelId = new string('x', 129) }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelId = "bad\nmodel" }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelSha256 = new string('z', 64) }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelSha256 = new string('a', 64) }));
    }
    private static void RejectsInvalidCapacityRequirements(string directory)
    {
        var store = new LocalHostSettingsStore(Path.Combine(directory, "invalid-capacity.json"));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { RequiredModelMemoryGiB = 0 }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { RequiredModelMemoryGiB = double.NaN }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { MinimumFreeDiskGiB = double.PositiveInfinity }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { MinimumFreeDiskGiB = 1_048_577d }));
    }
    private static void RejectsSemanticallyInvalidPersistedSettings(string directory)
    {
        var path = Path.Combine(directory, "invalid-semantic.json");
        var json = JsonSerializer.Serialize(new
        {
            schemaVersion = LocalHostSettings.CurrentSchemaVersion,
            serverUrl = "https://example.com/",
            modelId = "model",
            requestTimeoutSeconds = 30,
            engineExecutablePath = (string?)null,
            modelFilePath = (string?)null,
            activeCatalogModelId = (string?)null,
            activeCatalogModelSha256 = (string?)null,
            persistHostConversation = false,
            requiredModelMemoryGiB = 4d,
            minimumFreeDiskGiB = 1d
        });
        File.WriteAllText(path, json);
        ExpectArgumentException(() => new LocalHostSettingsStore(path).LoadOrDefault());
        Assert(File.ReadAllText(path) == json, "Failed validation must not rewrite the user's invalid settings file.");

        var orphanChecksumPath = Path.Combine(directory, "orphan-checksum.json");
        var orphanChecksum = JsonSerializer.Serialize(new
        {
            schemaVersion = LocalHostSettings.CurrentSchemaVersion,
            serverUrl = "http://127.0.0.1:8080/",
            modelId = "model",
            requestTimeoutSeconds = 30,
            activeCatalogModelId = (string?)null,
            activeCatalogModelSha256 = new string('a', 64),
            persistHostConversation = false,
            requiredModelMemoryGiB = 4d,
            minimumFreeDiskGiB = 1d
        });
        File.WriteAllText(orphanChecksumPath, orphanChecksum);
        ExpectArgumentException(() => new LocalHostSettingsStore(orphanChecksumPath).LoadOrDefault());
        Assert(File.ReadAllText(orphanChecksumPath) == orphanChecksum, "Invalid checksum metadata must not be rewritten.");
    }
    private static void ExpectArgumentException(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new InvalidOperationException("Invalid persisted settings or active catalog model ID were accepted.");
    }
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

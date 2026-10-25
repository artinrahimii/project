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
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
        Console.WriteLine("PASS: active catalog selection persists, legacy settings remain readable, and invalid IDs fail closed.");
    }
    private static void PersistsSelectionAndPreservesServerModel(string directory)
    {
        var path = Path.Combine(directory, "roundtrip.json");
        var saved = new LocalHostSettingsStore(path).Save(new LocalHostSettings("http://127.0.0.1:8080/", "server-model", 120, null, null, "catalog-model-42"));
        var loaded = new LocalHostSettingsStore(path).LoadOrDefault();
        Assert(loaded.ActiveCatalogModelId == "catalog-model-42", "Selected catalog model ID was not persisted.");
        Assert(loaded.ModelId == "server-model", "Catalog selection must not overwrite the server model ID.");
        Assert(loaded.RequestTimeoutSeconds == 120, "Catalog selection must preserve other settings.");
        Assert(saved.ActiveCatalogModelId == loaded.ActiveCatalogModelId, "Save and reload differ.");
    }
    private static void LoadsLegacySettingsWithoutSelection(string directory)
    {
        var path = Path.Combine(directory, "legacy.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new { schemaVersion = LocalHostSettings.CurrentSchemaVersion, serverUrl = "http://127.0.0.1:8080/", modelId = "legacy-server-model", requestTimeoutSeconds = 300, engineExecutablePath = (string?)null, modelFilePath = (string?)null }));
        var loaded = new LocalHostSettingsStore(path).LoadOrDefault();
        Assert(loaded.ActiveCatalogModelId is null, "Legacy settings should have no selected catalog model.");
        Assert(loaded.ModelId == "legacy-server-model", "Legacy server model ID changed.");
    }
    private static void RejectsInvalidSelectionIds(string directory)
    {
        var store = new LocalHostSettingsStore(Path.Combine(directory, "invalid.json"));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelId = new string('x', 129) }));
        ExpectArgumentException(() => store.Save(LocalHostSettings.Defaults with { ActiveCatalogModelId = "bad\nmodel" }));
    }
    private static void ExpectArgumentException(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new InvalidOperationException("Invalid active catalog model ID was accepted.");
    }
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using WhiteP.Host.Windows.Inference;
using WhiteP.Host.Windows.Models.Catalog;

internal static class ModelCatalogParserAcceptanceTests
{
    private const string Checksum = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private static readonly ModelCatalogParser Parser = CreateParser();

    public static void Run()
    {
        ParsesValidCatalogAndNormalizesChecksum();
        LoadsCatalogFromFileUsingTheHostBackendRegistry();
        OfflineCatalogStoreCachesOnlyValidatedDataAndFallsBackWhenSourceIsMissing();
        OfflineCatalogStoreRejectsInvalidCache();
        LoadsBundledSampleCatalog();
        RejectsOversizedCatalogFile();
        MissingLicenseDefaultsToReviewRequired();
        RejectsMalformedJsonAndUnsupportedSchemaVersions();
        RejectsMissingAndUnknownMetadata();
        RejectsUnsafeOrUnapprovedSourceUrls();
        RejectsUnsafeLicenseUrlsAndUnverifiedLicenseClaims();
        RejectsUnsupportedBackendsAndFormats();
        RejectsBadChecksumsAndSizes();
        RejectsDuplicateIdsAndDuplicateJsonProperties();
        RejectsOversizedCatalogs();
        RejectsEmptyAllowlistsAndBackendRegistries();

        Console.WriteLine("PASS: model catalog parser validates versioned metadata, checksum, backend compatibility, URL allowlists, duplicate IDs, and license review state.");
    }

    private static ModelCatalogParser CreateParser() => new(
        allowedSourceHosts: new[] { "models.example.test" },
        supportedBackendIds: new[] { "llama.cpp" },
        allowedLicenseHosts: new[] { "licenses.example.test" });

    private static string Catalog(params JsonObject[] models) =>
        new JsonObject
        {
            ["schemaVersion"] = ModelCatalogParser.CurrentSchemaVersion,
            ["models"] = new JsonArray(models.Cast<JsonNode?>().ToArray())
        }.ToJsonString();

    private static JsonObject Model(
        string id = "tiny-test",
        string name = "Test metadata record",
        string format = "GGUF",
        long sizeBytes = 42,
        string checksumAlgorithm = "sha256",
        string checksumValue = Checksum,
        string backend = "llama.cpp",
        string sourceUrl = "https://models.example.test/model.gguf",
        bool includeLicense = true,
        JsonObject? license = null,
        bool includeChecksum = true,
        bool addUnknownProperty = false)
    {
        var model = new JsonObject
        {
            ["id"] = id,
            ["name"] = name,
            ["format"] = format,
            ["sizeBytes"] = sizeBytes,
            ["backend"] = new JsonArray(JsonValue.Create(backend)),
            ["sourceUrl"] = sourceUrl
        };
        if (includeChecksum)
        {
            model["checksum"] = new JsonObject
            {
                ["algorithm"] = checksumAlgorithm,
                ["value"] = checksumValue
            };
        }

        if (includeLicense)
        {
            model["license"] = (license ?? VerifiedLicense()).DeepClone();
        }

        if (addUnknownProperty)
        {
            model["untrusted"] = true;
        }

        return model;
    }

    private static JsonObject VerifiedLicense() => new()
    {
        ["status"] = "verified",
        ["identifier"] = "LicenseRef-TestFixture",
        ["url"] = "https://licenses.example.test/LICENSE"
    };

    private static void ExpectInvalid(string json, string reason)
    {
        try
        {
            Parser.Parse(json);
        }
        catch (InvalidDataException)
        {
            return;
        }

        throw new Exception($"Model catalog parser accepted invalid input: {reason}");
    }

    private static void OfflineCatalogStoreCachesOnlyValidatedDataAndFallsBackWhenSourceIsMissing()
    {
        var folder = Path.Combine(Path.GetTempPath(), "whitep-catalog-offline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var sourcePath = Path.Combine(folder, "catalog.json");
        var cachePath = Path.Combine(folder, "cache", "catalog.json");
        try
        {
            File.WriteAllText(sourcePath, Catalog(Model(id: "offline-record")), new UTF8Encoding(false));
            var store = new OfflineModelCatalogStore();
            var first = store.Load(sourcePath, cachePath, new[] { "models.example.test" },
                LocalInferenceBackendFactory.SupportedBackendIds, new[] { "licenses.example.test" }, out var usedCache);
            if (usedCache || first.Models.Single().Id != "offline-record" || !File.Exists(cachePath))
                throw new Exception("A valid online/local catalog must be loaded and cached without claiming fallback.");

            File.Delete(sourcePath);
            var offline = store.Load(sourcePath, cachePath, new[] { "models.example.test" },
                LocalInferenceBackendFactory.SupportedBackendIds, new[] { "licenses.example.test" }, out usedCache);
            if (!usedCache || offline.Models.Single().Id != "offline-record")
                throw new Exception("A missing source catalog must fall back to the last validated cached catalog.");
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    private static void OfflineCatalogStoreRejectsInvalidCache()
    {
        var folder = Path.Combine(Path.GetTempPath(), "whitep-catalog-offline-invalid-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var sourcePath = Path.Combine(folder, "missing.json");
        var cachePath = Path.Combine(folder, "cache.json");
        try
        {
            File.WriteAllText(cachePath, "{ invalid json", new UTF8Encoding(false));
            try
            {
                _ = new OfflineModelCatalogStore().Load(sourcePath, cachePath,
                    new[] { "models.example.test" }, LocalInferenceBackendFactory.SupportedBackendIds,
                    new[] { "licenses.example.test" }, out _);
            }
            catch (InvalidDataException) { return; }
            throw new Exception("Invalid cached catalog data must not be displayed as a valid offline catalog.");
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    private static void LoadsCatalogFromFileUsingTheHostBackendRegistry()
    {
        var folder = Path.Combine(Path.GetTempPath(), "whitep-catalog-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "catalog.json");
        try
        {
            File.WriteAllText(path, Catalog(Model(id: "from-file", name: "Catalog loaded from a real file")), new UTF8Encoding(false));
            var catalog = new ModelCatalogFileLoader().Load(
                path, new[] { "models.example.test" }, LocalInferenceBackendFactory.SupportedBackendIds,
                new[] { "licenses.example.test" });
            if (catalog.SchemaVersion != 1 || catalog.Models.Count != 1 ||
                catalog.Models[0].Id != "from-file" || catalog.Models[0].Name != "Catalog loaded from a real file")
                throw new Exception("The file loader did not return the real catalog records parsed from disk.");
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    private static void LoadsBundledSampleCatalog()
    {
        var path = Path.Combine("src", "Host.Windows", "Models", "Catalog", "sample-model-catalog.json");
        if (!File.Exists(path))
            throw new Exception("The checked-in sample model catalog is missing.");

        var catalog = new ModelCatalogFileLoader().Load(
            path,
            new[] { "huggingface.co" },
            LocalInferenceBackendFactory.SupportedBackendIds,
            new[] { "huggingface.co" });

        if (catalog.SchemaVersion != 1 || catalog.Models.Count != 1)
            throw new Exception("The sample model catalog has an unexpected schema version or model count.");

        var model = catalog.Models.Single();
        if (model.Id != "smollm2-135m-instruct-q4-k-m" ||
            model.Name != "SmolLM2-135M-Instruct Q4_K_M" ||
            model.Format != "GGUF" ||
            model.SizeBytes != 105454432L ||
            model.ChecksumAlgorithm != "sha256" ||
            model.ChecksumValue != "2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d" ||
            model.BackendIds.Single() != "llama.cpp" ||
            model.SourceUrl.AbsoluteUri != "https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/resolve/main/SmolLM2-135M-Instruct-Q4_K_M.gguf" ||
            model.License.Status != "verified" ||
            model.License.Identifier != "Apache-2.0" ||
            model.License.Url?.AbsoluteUri != "https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/blob/main/README.md" ||
            model.License.RequiresReview ||
            model.License.ReviewLabel != "Declared; not independently verified")
        {
            throw new Exception("The sample catalog metadata does not match the reviewed upstream source and license declaration.");
        }

        Console.WriteLine("PASS: sample model catalog file, upstream SHA-256/size, llama.cpp backend, and Apache-2.0 declaration validated.");
    }

    private static void RejectsOversizedCatalogFile()
    {
        var folder = Path.Combine(Path.GetTempPath(), "whitep-catalog-size-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "oversized.json");
        var rejected = false;
        try
        {
            File.WriteAllText(path, new string('x', ModelCatalogParser.MaximumCatalogBytes + 1), new UTF8Encoding(false));
            try
            {
                _ = new ModelCatalogFileLoader().Load(path, new[] { "models.example.test" },
                    LocalInferenceBackendFactory.SupportedBackendIds, new[] { "licenses.example.test" });
            }
            catch (InvalidDataException) { rejected = true; }
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
        if (!rejected) throw new Exception("The catalog file loader must reject files above the configured byte limit.");
    }

    private static void ParsesValidCatalogAndNormalizesChecksum()
    {
        var input = Catalog(Model(checksumValue: Checksum.ToUpperInvariant()));
        var parsed = Parser.Parse(input);
        if (parsed.SchemaVersion != 1 || parsed.Models.Count != 1)
        {
            throw new Exception("A valid catalog did not preserve its schema version and model count.");
        }

        var model = parsed.Models[0];
        if (model.Id != "tiny-test" ||
            model.Format != "GGUF" ||
            model.SizeBytes != 42 ||
            model.ChecksumAlgorithm != "sha256" ||
            model.ChecksumValue != Checksum ||
            model.BackendIds.Single() != "llama.cpp" ||
            model.SourceUrl.Host != "models.example.test" ||
            model.License.Status != "verified" ||
            model.License.RequiresReview ||
            model.License.ReviewLabel != "Declared; not independently verified")
        {
            throw new Exception("A valid catalog did not produce the expected normalized metadata.");
        }
    }

    private static void MissingLicenseDefaultsToReviewRequired()
    {
        var model = Parser.Parse(Catalog(Model(includeLicense: false))).Models.Single();
        if (model.License.Status != "review_required" ||
            !model.License.RequiresReview ||
            model.License.ReviewLabel != "REVIEW REQUIRED" ||
            model.License.Identifier is not null ||
            model.License.Url is not null)
        {
            throw new Exception("Missing license metadata must be surfaced as review_required.");
        }

        var explicitlyUnverified = Parser.Parse(Catalog(Model(license: new JsonObject { ["status"] = "unknown" }))).Models.Single();
        if (!explicitlyUnverified.License.RequiresReview || explicitlyUnverified.License.ReviewLabel != "REVIEW REQUIRED")
        {
            throw new Exception("An unknown license must require review.");
        }
    }

    private static void RejectsMalformedJsonAndUnsupportedSchemaVersions()
    {
        ExpectInvalid("{", "malformed JSON");
        ExpectInvalid(new JsonObject { ["schemaVersion"] = 2, ["models"] = new JsonArray() }.ToJsonString(), "unknown schema version");
        ExpectInvalid(new JsonObject { ["schemaVersion"] = 1 }.ToJsonString(), "missing models list");
        ExpectInvalid(new JsonObject { ["schemaVersion"] = 1, ["models"] = "not-an-array" }.ToJsonString(), "models is not an array");
    }

    private static void RejectsMissingAndUnknownMetadata()
    {
        ExpectInvalid(Catalog(Model(includeChecksum: false)), "missing checksum");
        ExpectInvalid(Catalog(Model(id: "UpperCase")), "ID casing not stable");
        ExpectInvalid(Catalog(Model(id: "with spaces")), "spaces in ID");
        ExpectInvalid(Catalog(Model(name: "   ")), "empty name");
        ExpectInvalid(Catalog(Model(addUnknownProperty: true)), "unknown metadata");
        var unknownRoot = new JsonObject { ["schemaVersion"] = 1, ["models"] = new JsonArray(), ["untrusted"] = true }.ToJsonString();
        ExpectInvalid(unknownRoot, "unknown root property");
    }

    private static void RejectsUnsafeOrUnapprovedSourceUrls()
    {
        ExpectInvalid(Catalog(Model(sourceUrl: "http://models.example.test/model.gguf")), "HTTP source URL");
        ExpectInvalid(Catalog(Model(sourceUrl: "https://elsewhere.example.test/model.gguf")), "non-allowlisted host");
        ExpectInvalid(Catalog(Model(sourceUrl: "https://user@models.example.test/model.gguf")), "URL credentials");
        ExpectInvalid(Catalog(Model(sourceUrl: "https://models.example.test:8443/model.gguf")), "nonstandard port");
        ExpectInvalid(Catalog(Model(sourceUrl: "https://127.0.0.1/model.gguf")), "IP literal URL");
        ExpectInvalid(Catalog(Model(sourceUrl: "https://models.example.test/model.gguf#fragment")), "URL fragment");
    }

    private static void RejectsUnsafeLicenseUrlsAndUnverifiedLicenseClaims()
    {
        ExpectInvalid(Catalog(Model(license: new JsonObject
        {
            ["status"] = "verified",
            ["identifier"] = "LicenseRef-Test",
            ["url"] = "https://elsewhere.example.test/LICENSE"
        })), "non-allowlisted license URL");

        ExpectInvalid(Catalog(Model(license: new JsonObject
        {
            ["status"] = "verified",
            ["identifier"] = "LicenseRef-Test"
        })), "verified license without evidence URL");

        ExpectInvalid(Catalog(Model(license: new JsonObject { ["status"] = "approved-by-uploader" })), "unknown license status");
        ExpectInvalid(Catalog(Model(license: new JsonObject { ["status"] = "unknown", ["untrusted"] = true })), "unknown license property");
    }

    private static void RejectsUnsupportedBackendsAndFormats()
    {
        ExpectInvalid(Catalog(Model(format: "ONNX")), "unsupported format");
        ExpectInvalid(Catalog(Model(backend: "remote-api")), "unsupported backend");
        var parserWithNoAvailableLlamaCpp = new ModelCatalogParser(
            new[] { "models.example.test" }, new[] { "other-backend" }, new[] { "licenses.example.test" });
        try
        {
            parserWithNoAvailableLlamaCpp.Parse(Catalog(Model()));
        }
        catch (InvalidDataException)
        {
            return;
        }

        throw new Exception("A model must not be accepted if its backend is unavailable in host configuration.");
    }

    private static void RejectsBadChecksumsAndSizes()
    {
        ExpectInvalid(Catalog(Model(checksumAlgorithm: "md5")), "non-SHA256 checksum algorithm");
        ExpectInvalid(Catalog(Model(checksumValue: "abc")), "short checksum");
        ExpectInvalid(Catalog(Model(checksumValue: new string('g', 64))), "non-hex checksum");
        ExpectInvalid(Catalog(Model(sizeBytes: 0)), "zero model size");
    }

    private static void RejectsDuplicateIdsAndDuplicateJsonProperties()
    {
        ExpectInvalid(Catalog(Model(id: "duplicate"), Model(id: "duplicate")), "duplicate IDs");
        ExpectInvalid("{\"schemaVersion\":1,\"schemaVersion\":1,\"models\":[]}", "duplicate root property");
        var duplicateModelProperty = "{\"schemaVersion\":1,\"models\":[{\"id\":\"x\",\"id\":\"y\"}]}";
        ExpectInvalid(duplicateModelProperty, "duplicate model property");
    }

    private static void RejectsOversizedCatalogs()
    {
        ExpectInvalid("x" + new string(' ', ModelCatalogParser.MaximumCatalogBytes), "oversized catalog");
    }

    private static void RejectsEmptyAllowlistsAndBackendRegistries()
    {
        try
        {
            _ = new ModelCatalogParser(Array.Empty<string>(), new[] { "llama.cpp" });
        }
        catch (ArgumentException)
        {
            goto CheckBackends;
        }

        throw new Exception("An empty source URL allowlist must be rejected.");

    CheckBackends:
        try
        {
            _ = new ModelCatalogParser(new[] { "models.example.test" }, Array.Empty<string>());
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new Exception("An empty supported-backend registry must be rejected.");
    }
}

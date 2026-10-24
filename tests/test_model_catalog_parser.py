import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG_DIR = ROOT / "src" / "Host.Windows" / "Models" / "Catalog"
PARSER = CATALOG_DIR / "ModelCatalogParser.cs"
MODELS = CATALOG_DIR / "ModelCatalogModels.cs"
SCHEMA = CATALOG_DIR / "model-catalog-v1.schema.json"
TEST_PROJECT = ROOT / "tests" / "HostLifecycle.Tests" / "HostLifecycle.Tests.csproj"
HARNESS = ROOT / "tests" / "HostLifecycle.Tests" / "Program.cs"
ACCEPTANCE_TEST = ROOT / "tests" / "HostLifecycle.Tests" / "ModelCatalogParserAcceptanceTests.cs"


class ModelCatalogParserContractTests(unittest.TestCase):
    def test_parser_and_metadata_records_exist_in_the_expected_project_path(self):
        self.assertTrue(PARSER.is_file())
        self.assertTrue(MODELS.is_file())
        self.assertTrue(SCHEMA.is_file())

    def test_parser_is_fail_closed_and_does_not_fetch_urls(self):
        source = PARSER.read_text(encoding="utf-8")
        for required in (
            "JsonDocument.Parse",
            "CurrentSchemaVersion",
            "MaximumCatalogBytes",
            "NormalizeHostSet",
            "ParseAllowlistedHttpsUrl",
            "SchemaV1BackendIds",
            "duplicate model ID",
            "review_required",
            "Encoding.UTF8.GetByteCount",
        ):
            with self.subTest(required=required):
                self.assertIn(required, source)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("WebRequest", source)
        self.assertNotIn("Process.Start", source)

    def test_parser_and_runtime_acceptance_tests_are_linked_into_the_host_harness(self):
        project = TEST_PROJECT.read_text(encoding="utf-8")
        harness = HARNESS.read_text(encoding="utf-8")
        self.assertIn("ModelCatalogParser.cs", project)
        self.assertIn("ModelCatalogModels.cs", project)
        self.assertIn("ModelCatalogParserAcceptanceTests.Run();", harness)
        self.assertTrue(ACCEPTANCE_TEST.is_file())

    def test_schema_and_parser_share_the_current_format_and_backend_limits(self):
        import json
        schema = json.loads(SCHEMA.read_text(encoding="utf-8"))
        model = schema["$defs"]["model"]["properties"]
        parser = PARSER.read_text(encoding="utf-8")
        self.assertEqual(model["format"]["enum"], ["GGUF"])
        self.assertEqual(model["backend"]["items"]["enum"], ["llama.cpp"])
        self.assertIn('"GGUF"', parser)
        self.assertIn('"llama.cpp"', parser)

    def test_url_allowlists_are_supplied_by_trusted_configuration_not_catalog_fields(self):
        source = PARSER.read_text(encoding="utf-8")
        constructor = source[source.index("public ModelCatalogParser("):source.index("public ModelCatalog Parse")]
        self.assertIn("allowedSourceHosts", constructor)
        self.assertIn("allowedLicenseHosts", constructor)
        self.assertIn("supportedBackendIds", constructor)


if __name__ == "__main__":
    unittest.main()

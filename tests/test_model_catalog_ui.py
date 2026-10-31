import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
XAML = ROOT / "src" / "Host.Windows" / "MainWindow.xaml"
CODE_BEHIND = ROOT / "src" / "Host.Windows" / "MainWindow.xaml.cs"
LOADER = ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "ModelCatalogFileLoader.cs"
FACTORY = ROOT / "src" / "Host.Windows" / "Inference" / "LocalInferenceBackendFactory.cs"
PROJECT = ROOT / "tests" / "HostLifecycle.Tests" / "HostLifecycle.Tests.csproj"
PARSER_TESTS = ROOT / "tests" / "HostLifecycle.Tests" / "ModelCatalogParserAcceptanceTests.cs"
SCHEMA = ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "model-catalog-v1.schema.json"
LICENSE_MODELS = ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "ModelCatalogModels.cs"

class ModelCatalogUiTests(unittest.TestCase):
    def test_ui_binds_parsed_catalog_records_not_static_rows(self):
        xaml = XAML.read_text(encoding="utf-8")
        code = CODE_BEHIND.read_text(encoding="utf-8")
        for required in ('x:Name="CatalogFilePath"', 'x:Name="CatalogTrustedSourceHosts"',
                         'x:Name="CatalogTrustedLicenseHosts"', 'x:Name="CatalogItems"',
                         'Click="LoadCatalogButton_Click"', 'DisplayMemberBinding="{Binding Name}"',
                         'DisplayMemberBinding="{Binding Id}"'):
            with self.subTest(required=required): self.assertIn(required, xaml)
        self.assertNotIn("<ListViewItem", xaml)
        self.assertIn("CatalogItems.ItemsSource = catalog.Models;", code)
        self.assertIn("new OfflineModelCatalogStore()", code)
        store = (ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "OfflineModelCatalogStore.cs").read_text(encoding="utf-8")
        self.assertIn("new ModelCatalogFileLoader()", store)
        self.assertIn("LocalInferenceBackendFactory.SupportedBackendIds", code)

    def test_selected_model_details_show_license_source_size_and_limitations(self):
        xaml = XAML.read_text(encoding="utf-8")
        code = CODE_BEHIND.read_text(encoding="utf-8")
        license_models = LICENSE_MODELS.read_text(encoding="utf-8")
        self.assertIn('x:Name="CatalogModelDetails"', xaml)
        self.assertIn('SelectionChanged="CatalogItems_SelectionChanged"', xaml)
        for field in ("model.SourceUrl", "model.SizeBytes", "model.License.Status",
                      "model.License.Classification", "model.License.ReviewLabel",
                      "model.License.Identifier", "model.License.Url", "model.ChecksumValue",
                      "model.BackendIds", "Usage limitations:", "model.License.UsageDisclaimer"):
            with self.subTest(field=field):
                self.assertIn(field, code)
        self.assertIn("License classification:", code)
        self.assertIn("License disclosure:", code)
        self.assertIn("License disclosure", xaml)
        self.assertIn("License.ReviewLabel", xaml)
        self.assertIn("REVIEW REQUIRED — DO NOT ASSUME OPEN SOURCE", license_models)
        self.assertIn("RESTRICTIONS REPORTED — NOT OPEN SOURCE", license_models)
        self.assertIn("Do not assume the model is open source", license_models)
        self.assertIn("has not independently verified the legal terms", license_models)
        self.assertIn("var licenseWarning = model.License.UsageDisclaimer;", code)

    def test_loader_is_bounded_strict_utf8_and_delegates_validation(self):
        source = LOADER.read_text(encoding="utf-8")
        for required in ("new FileStream", "ModelCatalogParser.MaximumCatalogBytes",
                         "throwOnInvalidBytes: true",
                         "new ModelCatalogParser(allowedSourceHosts, supportedBackendIds, allowedLicenseHosts).Parse(json)"):
            with self.subTest(required=required): self.assertIn(required, source)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("WebRequest", source)

    def test_host_supported_backends_match_schema(self):
        factory = FACTORY.read_text(encoding="utf-8")
        schema = json.loads(SCHEMA.read_text(encoding="utf-8"))
        self.assertIn("SupportedBackendIds", factory)
        self.assertIn('"llama.cpp"', factory)
        self.assertEqual(schema["$defs"]["model"]["properties"]["backend"]["items"]["enum"], ["llama.cpp"])

    def test_runtime_loader_acceptance_tests_are_linked_and_called(self):
        project = PROJECT.read_text(encoding="utf-8")
        harness = (ROOT / "tests" / "HostLifecycle.Tests" / "Program.cs").read_text(encoding="utf-8")
        tests = PARSER_TESTS.read_text(encoding="utf-8")
        self.assertIn("ModelCatalogFileLoader.cs", project)
        self.assertIn("LocalInferenceBackendFactory.cs", project)
        self.assertIn("ModelCatalogParserAcceptanceTests.Run();", harness)
        self.assertIn("LoadsCatalogFromFileUsingTheHostBackendRegistry", tests)
        self.assertIn("RejectsOversizedCatalogFile", tests)

if __name__ == "__main__":
    unittest.main()

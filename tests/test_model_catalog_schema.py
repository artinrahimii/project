import json
import re
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "model-catalog-v1.schema.json"
PROJECT_PATH = ROOT / "src" / "Host.Windows" / "WhiteP.Host.Windows.csproj"
REQUIREMENTS_PATH = ROOT / "docs" / "requirements.md"


class ModelCatalogSchemaTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
        cls.model = cls.schema["$defs"]["model"]
        cls.properties = cls.model["properties"]

    def test_schema_is_versioned_and_has_no_implicit_extra_properties(self):
        self.assertEqual(self.schema["$schema"], "https://json-schema.org/draft/2020-12/schema")
        self.assertEqual(self.schema["properties"]["schemaVersion"]["const"], 1)
        self.assertEqual(self.schema["required"], ["schemaVersion", "models"])
        self.assertFalse(self.schema["additionalProperties"])
        self.assertFalse(self.model["additionalProperties"])

    def test_model_contract_contains_required_metadata_fields(self):
        for field in ("id", "name", "format", "sizeBytes", "checksum", "backend", "sourceUrl", "license"):
            with self.subTest(field=field):
                self.assertIn(field, self.properties)
        self.assertEqual(
            set(self.model["required"]),
            {"id", "name", "format", "sizeBytes", "checksum", "backend", "sourceUrl"},
        )
        # Missing license metadata is intentionally representable so T0069 can surface review_required.
        self.assertNotIn("license", self.model["required"])
        self.assertEqual(self.properties["license"]["default"], {"status": "review_required"})

    def test_size_format_and_stable_identifier_constraints_are_explicit(self):
        self.assertEqual(self.properties["id"]["pattern"], "^[a-z0-9][a-z0-9._-]*$")
        self.assertEqual(self.properties["sizeBytes"]["type"], "integer")
        self.assertEqual(self.properties["sizeBytes"]["minimum"], 1)
        self.assertEqual(self.properties["format"]["enum"], ["GGUF"])

    def test_checksum_requires_sha256_and_64_hexadecimal_digits(self):
        checksum = self.properties["checksum"]
        self.assertEqual(set(checksum["required"]), {"algorithm", "value"})
        self.assertEqual(checksum["properties"]["algorithm"]["const"], "sha256")
        pattern = checksum["properties"]["value"]["pattern"]
        self.assertRegex("a" * 64, pattern)
        self.assertRegex("0" * 64, pattern)
        self.assertNotRegex("a" * 63, pattern)
        self.assertNotRegex("g" * 64, pattern)

    def test_only_the_currently_supported_llama_cpp_backend_is_allowed_in_v1(self):
        backend = self.properties["backend"]
        self.assertEqual(backend["minItems"], 1)
        self.assertTrue(backend["uniqueItems"])
        self.assertEqual(backend["items"]["enum"], ["llama.cpp"])

    def test_source_and_license_urls_are_https_only_at_schema_level(self):
        for prop in (self.properties["sourceUrl"], self.properties["license"]["properties"]["url"]):
            self.assertEqual(prop["format"], "uri")
            self.assertTrue(prop["pattern"].startswith("^https://"))
        # URL host allowlisting is deliberately a parser responsibility in T0069.
        self.assertIn("trusted host allowlist", self.properties["sourceUrl"]["description"])

    def test_verified_license_requires_identifier_and_url(self):
        license_schema = self.properties["license"]
        self.assertEqual(
            set(license_schema["properties"]["status"]["enum"]),
            {"verified", "review_required", "unknown"},
        )
        conditional = license_schema["allOf"][0]
        self.assertEqual(conditional["if"]["properties"]["status"]["const"], "verified")
        self.assertEqual(set(conditional["then"]["required"]), {"identifier", "url"})

    def test_schema_is_copied_with_the_windows_host_outputs(self):
        root = ET.parse(PROJECT_PATH).getroot()
        content_items = [
            item for item in root.findall("./ItemGroup/Content")
            if item.attrib.get("Include", "").replace("\\", "/") == "Models/Catalog/model-catalog-v1.schema.json"
        ]
        self.assertEqual(len(content_items), 1)
        self.assertEqual(content_items[0].attrib.get("CopyToOutputDirectory"), "PreserveNewest")
        self.assertEqual(content_items[0].attrib.get("CopyToPublishDirectory"), "PreserveNewest")

    def test_schema_tracks_the_requirement_without_claiming_a_bundled_catalog(self):
        requirements = REQUIREMENTS_PATH.read_text(encoding="utf-8")
        self.assertIn("Each listed model has a stable catalog identifier, format, size, checksum, backend compatibility, source, and license status.", requirements)
        self.assertIn("missing license information is labelled as needing review", requirements)
        self.assertIn("T0068–T0075", requirements)
        # T0068 defines the schema only; it must not silently invent a bundled model record.
        catalog_json = ROOT / "src" / "Host.Windows" / "Models" / "Catalog" / "model-catalog-v1.json"
        self.assertFalse(catalog_json.exists())


if __name__ == "__main__":
    unittest.main()

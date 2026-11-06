import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from tools.generate_model_license_notice import generate, validate_apache_license


def make_model(**overrides):
    model = {
        "id": "smollm2-135m-instruct-q4-k-m",
        "name": "SmolLM2-135M-Instruct Q4_K_M",
        "format": "GGUF",
        "sizeBytes": 105454432,
        "checksum": {
            "algorithm": "sha256",
            "value": "2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d",
        },
        "backend": ["llama.cpp"],
        "sourceUrl": "https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/resolve/main/SmolLM2-135M-Instruct-Q4_K_M.gguf",
        "license": {
            "status": "verified",
            "identifier": "Apache-2.0",
            "url": "https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/blob/main/README.md",
        },
    }
    model.update(overrides)
    return model


def write_inputs(root: Path, models=None, apache_text=None):
    catalog = root / "sample-model-catalog.json"
    catalog.write_text(
        json.dumps({"schemaVersion": 1, "models": models if models is not None else [make_model()]}),
        encoding="utf-8",
    )
    license_file = root / "Apache-2.0.txt"
    if apache_text is None:
        apache_text = (
            "Apache License\nVersion 2.0, January 2004\n"
            + ("This is a test fixture for license marker and byte-preservation checks. " * 32)
            + "\nEND OF TERMS AND CONDITIONS\n"
        )
    license_file.write_text(apache_text, encoding="utf-8")
    return catalog, license_file


class ModelLicenseNoticeTests(unittest.TestCase):
    def test_generates_notice_manifest_and_verbatim_license_file(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            catalog, license_file = write_inputs(root)
            output = root / "output"
            result = generate(catalog, license_file, output)
            notice = (output / "ModelLicenses.md").read_text(encoding="utf-8")
            manifest = json.loads((output / "model-licenses.json").read_text(encoding="utf-8"))
            copied = output / "files/WhiteP-Sample-Model-Apache-2.0.txt"
            self.assertEqual(result["model_count"], 1)
            self.assertEqual(manifest["models"][0]["license"]["identifier"], "Apache-2.0")
            self.assertEqual(manifest["models"][0]["license"]["classification"], "unknown")
            self.assertFalse(manifest["models"][0]["weights_included"])
            self.assertEqual(copied.read_bytes(), license_file.read_bytes())
            self.assertIn("SmolLM2-135M-Instruct Q4_K_M", notice)
            self.assertIn("2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d", notice)
            self.assertIn("model weights are not included", notice.lower())

    def test_catalog_hash_is_stable_across_lf_and_crlf_checkouts(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            catalog, license_file = write_inputs(root)
            raw = catalog.read_bytes()
            lf_bytes = raw.replace(b"\r\n", b"\n")
            catalog.write_bytes(lf_bytes)
            lf_result = generate(catalog, license_file, root / "output-lf")
            lf_manifest = json.loads((root / "output-lf/model-licenses.json").read_text(encoding="utf-8"))

            catalog.write_bytes(lf_bytes.replace(b"\n", b"\r\n"))
            crlf_result = generate(catalog, license_file, root / "output-crlf")
            crlf_manifest = json.loads((root / "output-crlf/model-licenses.json").read_text(encoding="utf-8"))

            self.assertEqual(lf_result["source_catalog_sha256"], crlf_result["source_catalog_sha256"])
            self.assertEqual(lf_manifest["source_catalog_sha256"], crlf_manifest["source_catalog_sha256"])
            self.assertEqual(
                lf_manifest["source_catalog_hash_policy"],
                crlf_manifest["source_catalog_hash_policy"],
            )

    def test_unverified_or_missing_model_license_fails_closed(self):
        for license_data in (None, {"status": "review_required"}):
            with self.subTest(license_data=license_data), tempfile.TemporaryDirectory() as temporary:
                root = Path(temporary)
                model = make_model()
                if license_data is None:
                    model.pop("license")
                else:
                    model["license"] = license_data
                catalog, license_file = write_inputs(root, [model])
                with self.assertRaisesRegex(ValueError, "license status is not verified"):
                    generate(catalog, license_file, root / "out")

    def test_non_apache_license_requires_a_separate_handler(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            model = make_model(license={"status": "verified", "identifier": "MIT", "url": "https://example.org/license"})
            catalog, license_file = write_inputs(root, [model])
            with self.assertRaisesRegex(ValueError, "unsupported license identifier"):
                generate(catalog, license_file, root / "out")

    def test_http_model_or_license_urls_are_rejected(self):
        for field in ("sourceUrl", "license"):
            with self.subTest(field=field), tempfile.TemporaryDirectory() as temporary:
                root = Path(temporary)
                model = make_model()
                if field == "sourceUrl":
                    model["sourceUrl"] = "http://example.org/model.gguf"
                else:
                    model["license"]["url"] = "http://example.org/model-card"
                catalog, license_file = write_inputs(root, [model])
                with self.assertRaisesRegex(ValueError, "HTTPS URL"):
                    generate(catalog, license_file, root / "out")

    def test_invalid_model_checksum_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            model = make_model(checksum={"algorithm": "sha256", "value": "bad"})
            catalog, license_file = write_inputs(root, [model])
            with self.assertRaisesRegex(ValueError, "invalid SHA-256 checksum"):
                generate(catalog, license_file, root / "out")

    def test_empty_catalog_and_duplicate_model_ids_are_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            catalog, license_file = write_inputs(root, [])
            with self.assertRaisesRegex(ValueError, "at least one model"):
                generate(catalog, license_file, root / "empty")
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            model = make_model()
            catalog, license_file = write_inputs(root, [model, dict(model)])
            with self.assertRaisesRegex(ValueError, "Duplicate model id"):
                generate(catalog, license_file, root / "duplicate")

    def test_apache_license_source_must_be_complete_utf8_with_expected_markers(self):
        with tempfile.TemporaryDirectory() as temporary:
            path = Path(temporary) / "Apache-2.0.txt"
            path.write_text("Apache License\nVersion 2.0\n" + ("x" * 1200), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "missing required markers"):
                validate_apache_license(path)


if __name__ == "__main__":
    unittest.main()

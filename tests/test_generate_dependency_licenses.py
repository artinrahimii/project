import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from tools.generate_dependency_licenses import generate


class LicenseInventoryTests(unittest.TestCase):
    def test_expression_and_license_file_are_listed_and_copied(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            packages = root / "nuget"
            mit = packages / "sample.mit" / "1.2.3"
            packaged = packages / "sample.file" / "4.5.6"
            mit.mkdir(parents=True)
            packaged.mkdir(parents=True)
            (mit / "sample.mit.nuspec").write_text(
                '<package><metadata><id>Sample.Mit</id><license type="expression">MIT</license></metadata></package>',
                encoding="utf-8")
            (packaged / "sample.file.nuspec").write_text(
                '<package><metadata><id>Sample.File</id><license type="file">LICENSE.txt</license></metadata></package>',
                encoding="utf-8")
            (packaged / "LICENSE.txt").write_text("license terms", encoding="utf-8")
            assets = root / "project.assets.json"
            assets.write_text(json.dumps({
                "packageFolders": {str(packages) + "/": {}},
                "libraries": {
                    "Sample.Mit/1.2.3": {"type": "package", "path": "sample.mit/1.2.3"},
                    "Sample.File/4.5.6": {"type": "package", "path": "sample.file/4.5.6"},
                    "WhiteP/0.1.0": {"type": "project"}
                }}), encoding="utf-8")
            output = root / "licenses"
            result = generate(assets, output)
            self.assertEqual(result["package_count"], 2)
            by_id = {item["id"]: item for item in result["packages"]}
            self.assertIn("MIT", by_id["Sample.Mit"]["license"])
            copied = output / by_id["Sample.File"]["license_files"][0]
            self.assertEqual(copied.read_text(encoding="utf-8"), "license terms")
            self.assertTrue((output / "ThirdPartyNotices.md").is_file())
            self.assertTrue((output / "licenses.json").is_file())

    def test_exact_version_override_copies_verified_upstream_license_and_hash(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            packages = root / "nuget"
            package = packages / "collections.pooled" / "1.0.82"
            package.mkdir(parents=True)
            (package / "collections.pooled.nuspec").write_text(
                "<package><metadata><id>Collections.Pooled</id></metadata></package>", encoding="utf-8")
            assets = root / "project.assets.json"
            assets.write_text(json.dumps({
                "packageFolders": {str(packages) + "/": {}},
                "libraries": {"Collections.Pooled/1.0.82": {
                    "type": "package", "path": "collections.pooled/1.0.82"
                }}
            }), encoding="utf-8")
            source_dir = root / "license-source"
            source_dir.mkdir()
            source = source_dir / "Collections.Pooled-1.0.82-LICENSE.txt"
            source.write_text(
                "The MIT License (MIT)\nCopyright (c) Joel Mueller\nAll rights reserved.\n",
                encoding="utf-8")
            overrides = root / "overrides.json"
            overrides.write_text(json.dumps({
                "schema_version": 1,
                "packages": {"Collections.Pooled/1.0.82": {
                    "spdx": "MIT",
                    "source_file": source.name,
                    "source_url_template": "https://raw.githubusercontent.com/jtmueller/Collections.Pooled/{revision}/LICENSE.txt",
                    "license_url": "https://github.com/jtmueller/Collections.Pooled/blob/master/LICENSE.txt",
                    "required_markers": ["The MIT License (MIT)", "Copyright (c) Joel Mueller"]
                }}
            }), encoding="utf-8")
            output = root / "out"
            result = generate(assets, output, overrides, source_dir, "a" * 40)
            package_result = result["packages"][0]
            self.assertIn("MIT", package_result["license"])
            self.assertEqual(package_result["license_source_revision"], "a" * 40)
            self.assertEqual(len(package_result["license_sha256"]), 64)
            copied = output / package_result["license_files"][0]
            self.assertEqual(copied.read_text(encoding="utf-8"), source.read_text(encoding="utf-8"))
            self.assertIn(package_result["license_sha256"], (output / "ThirdPartyNotices.md").read_text(encoding="utf-8"))

    def test_override_does_not_apply_to_a_different_package_version(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            packages = root / "nuget"
            package = packages / "collections.pooled" / "1.0.83"
            package.mkdir(parents=True)
            (package / "collections.pooled.nuspec").write_text(
                "<package><metadata><id>Collections.Pooled</id></metadata></package>", encoding="utf-8")
            assets = root / "project.assets.json"
            assets.write_text(json.dumps({
                "packageFolders": {str(packages) + "/": {}},
                "libraries": {"Collections.Pooled/1.0.83": {
                    "type": "package", "path": "collections.pooled/1.0.83"
                }}
            }), encoding="utf-8")
            source_dir = root / "license-source"
            source_dir.mkdir()
            overrides = root / "overrides.json"
            overrides.write_text(json.dumps({
                "schema_version": 1,
                "packages": {"Collections.Pooled/1.0.82": {
                    "spdx": "MIT",
                    "source_file": "Collections.Pooled-1.0.82-LICENSE.txt",
                    "source_url_template": "https://raw.githubusercontent.com/jtmueller/Collections.Pooled/{revision}/LICENSE.txt",
                    "license_url": "https://github.com/jtmueller/Collections.Pooled/blob/master/LICENSE.txt",
                    "required_markers": ["The MIT License (MIT)", "Copyright (c) Joel Mueller"]
                }}
            }), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "No license expression"):
                generate(assets, root / "out", overrides, source_dir, "a" * 40)

    def test_package_without_license_information_fails_closed(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            packages = root / "nuget" / "no-license" / "1.0.0"
            packages.mkdir(parents=True)
            (packages / "no-license.nuspec").write_text(
                "<package><metadata><id>No-License</id></metadata></package>", encoding="utf-8")
            assets = root / "project.assets.json"
            assets.write_text(json.dumps({
                "packageFolders": {str(root / "nuget") + "/": {}},
                "libraries": {"No-License/1.0.0": {"type": "package", "path": "no-license/1.0.0"}}
            }), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "No license expression"):
                generate(assets, root / "out")


if __name__ == "__main__":
    unittest.main()

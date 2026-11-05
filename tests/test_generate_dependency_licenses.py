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

    def test_package_without_license_information_fails_closed(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            package = root / "nuget" / "no-license" / "1.0.0"
            package.mkdir(parents=True)
            (package / "no-license.nuspec").write_text(
                "<package><metadata><id>No-License</id></metadata></package>",
                encoding="utf-8")
            assets = root / "project.assets.json"
            assets.write_text(json.dumps({
                "packageFolders": {str(root / "nuget") + "/": {}},
                "libraries": {"No-License/1.0.0": {"type": "package", "path": "no-license/1.0.0"}}
            }), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "No license expression"):
                generate(assets, root / "out")

if __name__ == "__main__":
    unittest.main()

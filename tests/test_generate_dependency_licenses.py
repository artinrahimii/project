import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from tools.generate_dependency_licenses import generate, MissingPackageLicenseError


def make_assets(root: Path, packages: dict[str, tuple[str, str]]) -> Path:
    package_root = root / "nuget"
    libraries = {}
    for package_id, (version, nuspec) in packages.items():
        directory = package_root / package_id.lower() / version.lower()
        directory.mkdir(parents=True)
        (directory / f"{package_id}.nuspec").write_text(nuspec, encoding="utf-8")
        libraries[f"{package_id}/{version}"] = {
            "type": "package",
            "path": f"{package_id.lower()}/{version.lower()}",
        }
    assets = root / "project.assets.json"
    assets.write_text(json.dumps({
        "packageFolders": {str(package_root) + "/": {}},
        "libraries": libraries,
    }), encoding="utf-8")
    return assets


def write_override(root: Path, packages: dict) -> Path:
    path = root / "overrides.json"
    path.write_text(json.dumps({"schema_version": 1, "packages": packages}), encoding="utf-8")
    return path


class LicenseInventoryTests(unittest.TestCase):
    def test_expression_and_license_file_are_listed_and_copied(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Sample.Mit": ("1.2.3", '<package><metadata><id>Sample.Mit</id><license type="expression">MIT</license></metadata></package>'),
                "Sample.File": ("4.5.6", '<package><metadata><id>Sample.File</id><license type="file">LICENSE.txt</license></metadata></package>'),
            })
            (root / "nuget/sample.file/4.5.6/LICENSE.txt").write_text("license terms", encoding="utf-8")
            output = root / "licenses"
            result = generate(assets, output)
            self.assertEqual(result["package_count"], 2)
            by_id = {item["id"]: item for item in result["packages"]}
            self.assertIn("MIT", by_id["Sample.Mit"]["license"])
            copied = output / by_id["Sample.File"]["license_files"][0]
            self.assertEqual(copied.read_text(encoding="utf-8"), "license terms")
            self.assertTrue((output / "ThirdPartyNotices.md").is_file())
            self.assertTrue((output / "licenses.json").is_file())

    def test_collections_pooled_exact_version_override_records_revision_and_hash(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Collections.Pooled": ("1.0.82", "<package><metadata><id>Collections.Pooled</id></metadata></package>")
            })
            source_dir = root / "sources"
            source_dir.mkdir()
            source = source_dir / "Collections.Pooled-1.0.82-LICENSE.txt"
            source.write_text("The MIT License (MIT)\nCopyright (c) Joel Mueller\nAll rights reserved.\n", encoding="utf-8")
            revision = "a" * 40
            (source_dir / "Collections.Pooled.source-revision.txt").write_text(revision, encoding="ascii")
            overrides = write_override(root, {"Collections.Pooled/1.0.82": {
                "spdx": "MIT",
                "source_file": source.name,
                "source_revision_file": "Collections.Pooled.source-revision.txt",
                "source_url_template": "https://raw.githubusercontent.com/jtmueller/Collections.Pooled/{revision}/LICENSE.txt",
                "license_url": "https://github.com/jtmueller/Collections.Pooled/blob/master/LICENSE.txt",
                "required_markers": ["The MIT License (MIT)", "Copyright (c) Joel Mueller"],
            }})
            output = root / "output"
            result = generate(assets, output, overrides, source_dir)
            item = result["packages"][0]
            self.assertIn("MIT", item["license"])
            self.assertEqual(item["license_source_revision"], revision)
            self.assertEqual(len(item["license_sha256"]), 64)
            self.assertEqual((output / item["license_files"][0]).read_bytes(), source.read_bytes())

    def test_common_logging_and_core_share_verified_apache_license_for_exact_versions(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Common.Logging": ("3.4.1", "<package><metadata><id>Common.Logging</id></metadata></package>"),
                "Common.Logging.Core": ("3.4.1", "<package><metadata><id>Common.Logging.Core</id></metadata></package>"),
            })
            source_dir = root / "sources"
            source_dir.mkdir()
            source = source_dir / "common-logging-license.txt"
            source.write_text(
                "Apache License\nVersion 2.0\nhttp://www.apache.org/licenses/LICENSE-2.0\n",
                encoding="utf-8",
            )
            revision = "b" * 40
            (source_dir / "Common.Logging.source-revision.txt").write_text(revision, encoding="ascii")
            override = {
                "spdx": "Apache-2.0",
                "source_file": source.name,
                "source_revision_file": "Common.Logging.source-revision.txt",
                "source_url_template": "https://raw.githubusercontent.com/net-commons/common-logging/{revision}/license.txt",
                "license_url": "https://github.com/net-commons/common-logging/blob/master/license.txt",
                "required_markers": ["Apache License", "Version 2.0", "http://www.apache.org/licenses/LICENSE-2.0"],
            }
            overrides = write_override(root, {
                "Common.Logging/3.4.1": override,
                "Common.Logging.Core/3.4.1": override,
            })
            output = root / "output"
            result = generate(assets, output, overrides, source_dir)
            self.assertEqual(result["package_count"], 2)
            self.assertEqual({x["id"] for x in result["packages"]}, {"Common.Logging", "Common.Logging.Core"})
            for item in result["packages"]:
                self.assertIn("Apache-2.0", item["license"])
                self.assertEqual(item["license_source_revision"], revision)
                self.assertEqual(len(item["license_sha256"]), 64)
                self.assertEqual((output / item["license_files"][0]).read_bytes(), source.read_bytes())

    def test_makaretu_dns_packages_use_exact_version_pinned_mit_sources(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Makaretu.Dns": ("2.0.1", "<package><metadata><id>Makaretu.Dns</id></metadata></package>"),
                "Makaretu.Dns.Multicast": (
                    "0.27.0",
                    "<package><metadata><id>Makaretu.Dns.Multicast</id></metadata></package>",
                ),
            })
            source_dir = root / "sources"
            source_dir.mkdir()
            license_text = (
                "MIT License\\n\\nCopyright (c) 2018 Richard Schneider\\n\\n"
                "Permission is hereby granted, free of charge, to any person obtaining a copy\\n"
            )
            specs = {
                "Makaretu.Dns/2.0.1": {
                    "source_file": "Makaretu.Dns-2.0.1-LICENSE.txt",
                    "revision_file": "Makaretu.Dns.source-revision.txt",
                    "repo": "net-dns",
                    "revision": "c" * 40,
                    "url": "https://github.com/richardschneider/net-dns/blob/701463d2091e6d98d4cc4490abb0e0ead8ae2985/LICENSE",
                },
                "Makaretu.Dns.Multicast/0.27.0": {
                    "source_file": "Makaretu.Dns.Multicast-0.27.0-LICENSE.txt",
                    "revision_file": "Makaretu.Dns.Multicast.source-revision.txt",
                    "repo": "net-mdns",
                    "revision": "d" * 40,
                    "url": "https://github.com/richardschneider/net-mdns/blob/b9f2f8158052568a19d09536179ceaf5cae9b23e/LICENSE",
                },
            }
            overrides = {}
            for identity, spec in specs.items():
                (source_dir / spec["source_file"]).write_text(license_text, encoding="utf-8")
                (source_dir / spec["revision_file"]).write_text(spec["revision"], encoding="ascii")
                overrides[identity] = {
                    "spdx": "MIT",
                    "source_file": spec["source_file"],
                    "source_revision_file": spec["revision_file"],
                    "source_url_template": (
                        f"https://raw.githubusercontent.com/richardschneider/{spec['repo']}/"
                        "{{revision}}/LICENSE"
                    ),
                    "license_url": spec["url"],
                    "required_markers": ["MIT License", "Copyright (c) 2018 Richard Schneider"],
                }

            output = root / "output"
            result = generate(assets, output, write_override(root, overrides), source_dir)
            self.assertEqual(result["package_count"], 2)
            for item in result["packages"]:
                identity = f"{item['id']}/{item['version']}"
                spec = specs[identity]
                self.assertIn("MIT", item["license"])
                self.assertEqual(item["license_source_revision"], spec["revision"])
                self.assertEqual(len(item["license_sha256"]), 64)
                self.assertEqual((output / item["license_files"][0]).read_text(encoding="utf-8"), license_text)

    def test_override_does_not_apply_to_a_different_package_version(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Collections.Pooled": ("1.0.83", "<package><metadata><id>Collections.Pooled</id></metadata></package>")
            })
            source_dir = root / "sources"
            source_dir.mkdir()
            (source_dir / "Collections.Pooled-1.0.82-LICENSE.txt").write_text(
                "The MIT License (MIT)\nCopyright (c) Joel Mueller\n", encoding="utf-8")
            (source_dir / "Collections.Pooled.source-revision.txt").write_text("a" * 40, encoding="ascii")
            overrides = write_override(root, {"Collections.Pooled/1.0.82": {
                "spdx": "MIT",
                "source_file": "Collections.Pooled-1.0.82-LICENSE.txt",
                "source_revision_file": "Collections.Pooled.source-revision.txt",
                "source_url_template": "https://raw.githubusercontent.com/jtmueller/Collections.Pooled/{revision}/LICENSE.txt",
                "license_url": "https://github.com/jtmueller/Collections.Pooled/blob/master/LICENSE.txt",
                "required_markers": ["The MIT License (MIT)", "Copyright (c) Joel Mueller"],
            }})
            with self.assertRaisesRegex(MissingPackageLicenseError, "No license expression"):
                generate(assets, root / "out", overrides, source_dir)

    def test_missing_package_licenses_are_aggregated_without_being_skipped(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "Common.Logging": ("3.4.1", "<package><metadata><id>Common.Logging</id></metadata></package>"),
                "Example.Unlicensed": ("2.0.0", "<package><metadata><id>Example.Unlicensed</id></metadata></package>"),
            })
            with self.assertRaises(MissingPackageLicenseError) as raised:
                generate(assets, root / "out")
            message = str(raised.exception)
            self.assertIn("Common.Logging 3.4.1", message)
            self.assertIn("Example.Unlicensed 2.0.0", message)
            self.assertIn("2 restored package(s)", message)

    def test_package_without_license_information_fails_closed(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            assets = make_assets(root, {
                "No-License": ("1.0.0", "<package><metadata><id>No-License</id></metadata></package>")
            })
            with self.assertRaisesRegex(MissingPackageLicenseError, "No license expression"):
                generate(assets, root / "out")


if __name__ == "__main__":
    unittest.main()

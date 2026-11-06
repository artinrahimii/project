"""Guard the actual repository structure used for shared contracts.

These checks protect the canonical schema location and ensure contract tests are
discoverable. Cross-language generation and wire compatibility are verified by
the dedicated protocol tests and later generation tasks.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


class ContractLayoutTests(unittest.TestCase):
    def test_platform_contract_and_documentation_paths_exist(self):
        paths = (
            "src/Host.Windows/WhiteP.Host.Windows.csproj",
            "src/Client.Android/app/build.gradle.kts",
            "src/Shared.Contracts/README.md",
            "protocol/whitep/v1/messages.proto",
            "tests/test_proto_contract.py",
            "tests/contracts/__init__.py",
            "tests/contracts/test_contract_layout.py",
            "docs/requirements.md",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)

    def test_canonical_schema_remains_the_only_source_proto(self):
        canonical = ROOT / "protocol/whitep/v1/messages.proto"
        self.assertTrue(canonical.is_file())
        schema = canonical.read_text(encoding="utf-8")
        self.assertIn("message InferenceRequest", schema)
        self.assertIn("message InferenceResponse", schema)
        self.assertFalse(list((ROOT / "src/Shared.Contracts").glob("*.proto")))
        self.assertIn("protocol/whitep/v1/messages.proto", (ROOT / "src/Shared.Contracts/README.md").read_text(encoding="utf-8"))

    def test_contract_package_is_separately_discoverable(self):
        package = ROOT / "tests/contracts"
        self.assertTrue((package / "__init__.py").is_file())
        self.assertTrue((package / "test_contract_layout.py").is_file())
        self.assertIn("canonical", (package / "test_contract_layout.py").read_text(encoding="utf-8").lower())


if __name__ == "__main__":
    unittest.main()

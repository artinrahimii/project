"""Keep local secret files ignored and sample settings free of credentials."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class LocalConfigSafetyTests(unittest.TestCase):
    def test_gitignore_excludes_local_environment_and_signing_files(self):
        ignore = (ROOT / ".gitignore").read_text(encoding="utf-8")
        for pattern in (".env", ".env.*", "!/.env.example", "secrets/", "*.local.json", "local.properties", "*.jks", "*.keystore"):
            with self.subTest(pattern=pattern):
                self.assertIn(pattern, ignore)

    def test_environment_example_is_allowlisted_and_non_secret(self):
        example_path = ROOT / ".env.example"
        self.assertTrue(example_path.is_file())
        ignore = (ROOT / ".gitignore").read_text(encoding="utf-8")
        self.assertIn("!/.env.example", ignore)
        contents = example_path.read_text(encoding="utf-8")
        values = {}
        for line in contents.splitlines():
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            key, separator, value = line.partition("=")
            self.assertTrue(separator, f"Malformed example setting: {line}")
            self.assertNotIn(key, values, f"Duplicate setting: {key}")
            values[key] = value
        self.assertEqual(set(values), {"WHITEP_LLAMA_SERVER_URL", "WHITEP_LLAMA_MODEL", "WHITEP_REQUEST_TIMEOUT_SECONDS"})
        self.assertEqual(values["WHITEP_LLAMA_SERVER_URL"], "http://127.0.0.1:8080/")
        self.assertEqual(values["WHITEP_LLAMA_MODEL"], "local-model")
        self.assertEqual(values["WHITEP_REQUEST_TIMEOUT_SECONDS"], "300")
        self.assertFalse(re.search(r"(?i)(api[_-]?key|secret|password|token)\s*=\s*[^\s#]{8,}", contents))

    def test_example_is_not_claimed_as_runtime_loaded(self):
        contents = (ROOT / ".env.example").read_text(encoding="utf-8")
        self.assertIn("not auto-loaded", contents)

    def test_wpf_startup_contains_semantically_invalid_or_unreadable_settings(self):
        source = (ROOT / "src" / "Host.Windows" / "MainWindow.xaml.cs").read_text(encoding="utf-8")
        start = source.index("var settingsLoadFailed = false;")
        end = source.index("ServerUrl.Text = _settings.ServerUrl;", start)
        load_path = source[start:end]
        self.assertIn("catch (Exception exception) when (", load_path)
        for exception_type in ("InvalidDataException", "ArgumentException", "IOException", "UnauthorizedAccessException"):
            with self.subTest(exception_type=exception_type):
                self.assertIn(exception_type, load_path)
        self.assertIn("settingsLoadFailed = true;", load_path)
        self.assertNotIn("exception.Message", load_path)
        self.assertNotIn("exception.ToString()", load_path)
        self.assertIn("local settings could not be loaded or validated", source)
        self.assertIn("check the settings file and permissions before reconnecting", source)


if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MAIN_WINDOW = ROOT / "src" / "Host.Windows" / "MainWindow.xaml.cs"
FACTORY = ROOT / "src" / "Host.Windows" / "Inference" / "LocalInferenceBackendFactory.cs"
SESSION = ROOT / "src" / "Host.Application" / "HostInferenceSession.cs"
CONTRACT = ROOT / "src" / "Host.Application" / "IInferenceBackend.cs"
LIFECYCLE_TEST = ROOT / "tests" / "HostLifecycle.Tests" / "Program.cs"


class InferenceBackendAbstractionTests(unittest.TestCase):
    def test_window_composes_backend_through_factory_not_concrete_adapter(self):
        window = MAIN_WINDOW.read_text(encoding="utf-8")
        self.assertIn("backendFactory: LocalInferenceBackendFactory.Create", window)
        self.assertNotIn("new LlamaCppServerBackend", window)

    def test_factory_exposes_the_application_contract(self):
        factory = FACTORY.read_text(encoding="utf-8")
        self.assertIn("public static IInferenceBackend Create(Uri serverAddress, string modelId)", factory)
        self.assertIn("return new LlamaCppServerBackend(serverAddress, modelId);", factory)
        self.assertIn("ArgumentException.ThrowIfNullOrWhiteSpace(modelId)", factory)

    def test_session_logic_depends_on_the_interface(self):
        session = SESSION.read_text(encoding="utf-8")
        contract = CONTRACT.read_text(encoding="utf-8")
        self.assertIn("private IInferenceBackend _backend", session)
        self.assertIn("Func<Uri, string, IInferenceBackend>? _backendFactory", session)
        self.assertIn("_backend.GenerateAsync(prompt, maxOutputTokens, cancellationToken)", session)
        self.assertIn("public interface IInferenceBackend", contract)
        self.assertNotIn("LlamaCppServerBackend", session)

    def test_runtime_alternate_backend_regression_is_present(self):
        lifecycle_test = LIFECYCLE_TEST.read_text(encoding="utf-8")
        self.assertIn("AlternateFactoryTestBackend", lifecycle_test)
        self.assertIn("alternateSession.ConnectAsync", lifecycle_test)
        self.assertIn("alternate backend response", lifecycle_test)


if __name__ == "__main__":
    unittest.main()

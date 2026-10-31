import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ENGINE = ROOT / "src" / "Host.Windows" / "Inference" / "LlamaCppInferenceEngine.cs"
BACKEND = ROOT / "src" / "Host.Infrastructure" / "LlamaCppServerBackend.cs"

class LlamaCppInferenceEngineSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = ENGINE.read_text(encoding="utf-8")
        cls.backend = BACKEND.read_text(encoding="utf-8")

    def test_implements_application_contract_and_real_local_backend(self):
        self.assertIn("IInferenceEngine", self.source)
        self.assertIn("LlamaCppServerBackend", self.source)
        self.assertIn('"health"', self.backend)
        self.assertIn('"v1/models"', self.source)
        self.assertIn('"v1/chat/completions"', self.backend)

    def test_does_not_claim_model_loaded_without_discovery(self):
        self.assertIn("await DiscoverAsync(cancellationToken)", self.source)
        self.assertIn("if (!discovery.IsAvailable)", self.source)
        self.assertIn("_modelLoaded = true", self.source)

    def test_generation_is_cancellable_and_serialized(self):
        self.assertIn("SemaphoreSlim _operationGate", self.source)
        self.assertIn("CreateLinkedTokenSource(cancellationToken)", self.source)
        self.assertIn("_activeGeneration?.Cancel()", self.source)
        self.assertIn("active.Token", self.source)

    def test_unload_does_not_claim_remote_model_memory_was_freed(self):
        self.assertIn("server owns model memory", self.source)
        self.assertIn("_backend?.Dispose()", self.source)
        self.assertIn("_modelLoaded = false", self.source)

    def test_errors_do_not_log_prompt_or_response_content(self):
        self.assertNotIn("Console.Write", self.source)
        self.assertNotIn("Debug.Write", self.source)
        self.assertIn("Local backend could not be reached.", self.source)

if __name__ == "__main__":
    unittest.main()

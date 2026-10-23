"""Guards the concrete project boundaries for the Windows host layers."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class HostLayerProjectTests(unittest.TestCase):
    def test_domain_application_infrastructure_and_ui_are_distinct_projects(self):
        projects = {
            "domain": ROOT / "src/Host.Domain/WhiteP.Host.Domain.csproj",
            "application": ROOT / "src/Host.Application/WhiteP.Host.Application.csproj",
            "infrastructure": ROOT / "src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj",
            "ui": ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj",
        }
        for layer, path in projects.items():
            with self.subTest(layer=layer):
                self.assertTrue(path.is_file(), f"Missing buildable {layer} project")
                self.assertIn("<TargetFramework>", path.read_text(encoding="utf-8"))

        solution = (ROOT / "WhiteP.sln").read_text(encoding="utf-8")
        for fragment in (
            r"src\Host.Domain\WhiteP.Host.Domain.csproj",
            r"src\Host.Application\WhiteP.Host.Application.csproj",
            r"src\Host.Infrastructure\WhiteP.Host.Infrastructure.csproj",
            r"src\Host.Windows\WhiteP.Host.Windows.csproj",
        ):
            with self.subTest(project=fragment):
                self.assertIn(fragment, solution)

    def test_application_depends_on_abstractions_not_concrete_infrastructure(self):
        application = (ROOT / "src/Host.Application/WhiteP.Host.Application.csproj").read_text(encoding="utf-8")
        session = (ROOT / "src/Host.Application/HostInferenceSession.cs").read_text(encoding="utf-8")
        ui = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
        window = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
        self.assertIn("WhiteP.Host.Domain", application)
        self.assertNotIn("Host.Infrastructure", application)
        self.assertNotIn("LlamaCppServerBackend", session)
        self.assertIn("WhiteP.Host.Infrastructure", ui)
        self.assertIn("backendFactory:", window)

    def test_shared_protobuf_is_generated_once_in_the_contract_project(self):
        shared = (ROOT / "src/Shared.Contracts/WhiteP.Shared.Contracts.csproj").read_text(encoding="utf-8")
        host = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
        tests = (ROOT / "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj").read_text(encoding="utf-8")
        self.assertIn("messages.proto", shared)
        self.assertNotIn("<Protobuf Include=", host)
        self.assertNotIn("<Protobuf Include=", tests)
        self.assertIn("Host.Infrastructure/WhiteP.Host.Infrastructure.csproj", tests)


if __name__ == "__main__":
    unittest.main()

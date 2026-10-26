"""Regression guards for loopback-only listener exposure and user-visible endpoint status."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LISTENER = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")


class NetworkListenerExposureTests(unittest.TestCase):
    def test_listener_rejects_non_loopback_bind_addresses_before_binding(self):
        self.assertIn("IPAddress.IsLoopback(bindAddress)", LISTENER)
        self.assertIn("The initial host listener is restricted to loopback addresses.", LISTENER)

    def test_wpf_host_uses_loopback_and_displays_actual_port_and_status(self):
        self.assertIn("new(IPAddress.Loopback, 0)", WINDOW)
        self.assertIn("Loopback-only listener active at {endpoint.Address}:{endpoint.Port}", WINDOW)
        self.assertIn("Network participation is disabled; no listener is open.", WINDOW)
        self.assertIn("if (active && _networkParticipationLifecycle.LocalEndpoint is { } endpoint)", WINDOW)


if __name__ == "__main__":
    unittest.main()

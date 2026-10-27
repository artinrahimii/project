"""Traceability checks for technology decisions and their repository evidence.

This test validates source/config/test links and intentionally does not mark
runtime-only acceptance gates as passed.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REGISTER = (ROOT / "docs/decisions/README.md").read_text(encoding="utf-8")
README = (ROOT / "README.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
ACCEPTANCE = (ROOT / "docs/acceptance.md").read_text(encoding="utf-8")
ADR1 = (ROOT / "docs/decisions/ADR-0001-platform-build-baselines.md").read_text(encoding="utf-8")
ADR2 = (ROOT / "docs/decisions/ADR-0002-direct-p2p-data-path.md").read_text(encoding="utf-8")
ADR3 = (ROOT / "docs/decisions/ADR-0003-local-inference-backend.md").read_text(encoding="utf-8")
ADR3_TCP = (ROOT / "docs/decisions/ADR-0003-tcp-request-framing.md").read_text(encoding="utf-8")
ADR5_TRANSPORT = (ROOT / "docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md").read_text(encoding="utf-8")


class DecisionTraceabilityTests(unittest.TestCase):
    def test_register_lists_each_decision_and_all_linked_paths_exist(self):
        paths = (
            "docs/decisions/ADR-0001-platform-build-baselines.md",
            "docs/decisions/ADR-0002-direct-p2p-data-path.md",
            "docs/decisions/ADR-0003-local-inference-backend.md",
            "docs/decisions/ADR-0003-tcp-request-framing.md",
            "docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md",
            "protocol/whitep/v1/messages.proto",
            "src/Shared.Contracts/ProtocolMessageLimits.cs",
            "src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs",
            "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs",
            "tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs",
            "tests/test_product_build_contract.py",
            "tests/test_readme_contract.py",
            "tests/test_reproducible_build_contract.py",
            "tests/test_p2p_data_path_decision.py",
            "tests/HostLifecycle.Tests/Program.cs",
            ".github/workflows/product-build.yml",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
        self.assertIn("docs/decisions/README.md", README)

    def test_platform_build_decision_links_to_real_platform_configuration(self):
        for path in (
            "global.json",
            "src/Host.Windows/WhiteP.Host.Windows.csproj",
            "src/Client.Android/app/build.gradle.kts",
            "tests/test_product_build_contract.py",
        ):
            self.assertTrue((ROOT / path).is_file(), path)
            self.assertIn(path, REGISTER)
        self.assertIn("<UseWPF>true</UseWPF>", (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8"))
        self.assertIn("compileSdk = 36", (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8"))

    def test_p2p_decision_is_traceable_without_claiming_transport_is_delivered(self):
        for phrase in ("REQ-010", "REQ-011", "AC-P2P-01", "tests/test_p2p_data_path_decision.py"):
            self.assertIn(phrase, ADR2 + REGISTER)
        self.assertIn("Direct peer-to-peer transfer is the default data path.", REQUIREMENTS)
        self.assertIn("transport not implemented", (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt").read_text(encoding="utf-8"))
        self.assertIn("AC-P2P-01", ACCEPTANCE)
        self.assertIn("NOT RUN", ACCEPTANCE)

    def test_local_backend_decision_maps_to_real_adapter_ui_and_controlled_tests(self):
        backend = (ROOT / "src/Host.Infrastructure/LlamaCppServerBackend.cs").read_text(encoding="utf-8")
        lifecycle = (ROOT / "src/Host.Application/HostInferenceSession.cs").read_text(encoding="utf-8")
        ui = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        self.assertIn("public sealed class LlamaCppServerBackend", backend)
        # The bounded adapter serializes explicitly so it can check UTF-8 body size before sending.
        self.assertIn('new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")', backend)
        self.assertIn("JsonSerializer.SerializeToUtf8Bytes", backend)
        self.assertIn("ProtocolMessageLimits.ValidateLocalRequestBodyBytes", backend)
        self.assertIn("HttpCompletionOption.ResponseHeadersRead", backend)
        self.assertIn("ReadLimitedBodyAsync", backend)
        self.assertNotIn('PostAsJsonAsync("v1/chat/completions"', backend)
        self.assertIn('GetAsync("health"', backend)
        self.assertIn("public sealed class HostInferenceSession", lifecycle)
        self.assertIn('x:Name="ServerUrl"', ui)
        self.assertIn("RecordingHandler", harness)
        self.assertIn("generated fixture output", harness)
        self.assertIn("tests/HostLifecycle.Tests/Program.cs", ADR3)
        self.assertIn("AC-LOCAL-01", ACCEPTANCE)
        self.assertIn("AC-LOCAL-02", ACCEPTANCE)

    def test_register_separates_decisions_from_runtime_acceptance(self):
        self.assertIn("docs/acceptance.md", REGISTER)
        self.assertIn("AC-P2P-01", REGISTER)
        self.assertIn("AC-LOCAL-01", REGISTER)
        self.assertIn("NOT RUN", REGISTER)
        self.assertIn("live inference against a supported model remains unverified", REGISTER.lower())
        self.assertIn("runtime acceptance", REGISTER.lower())



    def test_tcp_framing_decision_maps_to_production_parser_and_runnable_acceptance_test(self):
        reader = (ROOT / "src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs").read_text(encoding="utf-8")
        listener = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
        limits = (ROOT / "src/Shared.Contracts/ProtocolMessageLimits.cs").read_text(encoding="utf-8")
        csharp_tests = (ROOT / "tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("ADR-0003-tcp-request-framing.md", REGISTER)
        self.assertIn("protocol_version = 1.0.0", ADR3_TCP)
        self.assertIn("MaximumRequestMessageBytes", ADR3_TCP)
        self.assertIn("BinaryPrimitives.ReadUInt32BigEndian(prefix)", reader)
        self.assertIn("payloadLength > ProtocolMessageLimits.MaximumRequestMessageBytes", reader)
        self.assertIn("ProtocolMessageParser.ParseInferenceRequest(payload)", reader)
        self.assertIn("MaximumRequestMessageBytes = 16 * 1024", limits)
        self.assertIn("if (!IPAddress.IsLoopback(bindAddress))", listener)
        self.assertIn("if (!_isParticipationActive()) return;", listener)
        for case in (
            "ValidLengthPrefixedRequestReachesTheSharedParserAsync",
            "InvalidFramesAreRejectedAsync",
            "ShutdownCancelsAClientWaitingForItsFrameAsync",
        ):
            with self.subTest(case=case):
                self.assertIn(case, csharp_tests)
        self.assertIn("NetworkInferenceRequestFrameAcceptanceTests.RunAsync()", entrypoint)

    def test_authenticated_peer_transport_fallback_decision_maps_to_connector_and_runtime_tests(self):
        connector = (ROOT / "src/Host.Windows/Networking/PeerTransportFallbackConnector.cs").read_text(encoding="utf-8")
        runtime_tests = (ROOT / "tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("ADR-0005-authenticated-peer-transport-fallback.md", REGISTER)
        self.assertIn("PeerTransportFallbackConnector.ConnectAsync", ADR5_TRANSPORT)
        self.assertIn("src/Host.Windows/Networking/PeerTransportFallbackConnector.cs", ADR5_TRANSPORT)
        self.assertIn("tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs", ADR5_TRANSPORT)
        self.assertIn("candidate.ExpectedPeerIdentity", connector)
        self.assertIn("!string.Equals(transport.RemotePeerIdentity, expectedPeerIdentity, StringComparison.Ordinal)", connector)
        self.assertIn("await DisposeQuietlyAsync(transport).ConfigureAwait(false)", connector)
        self.assertIn("new PeerTransportAttemptFailure(candidate.Address, exception.GetType().Name)", connector)
        for case in (
            "VerifyQuicFallbackAfterUnreachableTcpAddressAsync",
            "VerifyFallbackRejectsMixedPeerIdentitiesBeforeConnectingAsync",
        ):
            with self.subTest(case=case):
                self.assertIn(case, runtime_tests)
        self.assertIn("PeerTransportContractAcceptanceTests.RunAsync()", entrypoint)
        self.assertIn("AC-P2P-01", ADR5_TRANSPORT)
        self.assertIn("NOT RUN", ACCEPTANCE)


if __name__ == "__main__":
    unittest.main()

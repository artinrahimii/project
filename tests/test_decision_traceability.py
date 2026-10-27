"""Traceability checks for technology decisions and their repository evidence.

This test validates source/config/test links and intentionally does not mark
runtime-only acceptance gates as passed.
"""
import re
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
ADR4_RELAY = (ROOT / "docs/decisions/ADR-0004-encrypted-relay-boundary.md").read_text(encoding="utf-8")
ADR5_TRANSPORT = (ROOT / "docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md").read_text(encoding="utf-8")
ADR6_LEASE = (ROOT / "docs/decisions/ADR-0006-peer-announcement-leases.md").read_text(encoding="utf-8")
ADR7_SINGLE_ACTIVE = (ROOT / "docs/decisions/ADR-0007-single-active-inference.md").read_text(encoding="utf-8")
ADR8_PROCESS_LAUNCH = (ROOT / "docs/decisions/ADR-0008-local-engine-process-ownership.md").read_text(encoding="utf-8")
ADR9_AGENT_TOOLS = (ROOT / "docs/decisions/ADR-0009-agent-tool-execution-boundary.md").read_text(encoding="utf-8")
ADR10_MODEL_INSTALLATION = (ROOT / "docs/decisions/ADR-0010-verified-model-installation.md").read_text(encoding="utf-8")


class DecisionTraceabilityTests(unittest.TestCase):
    def test_register_lists_each_decision_and_all_linked_paths_exist(self):
        paths = (
            "docs/decisions/ADR-0001-platform-build-baselines.md",
            "docs/decisions/ADR-0002-direct-p2p-data-path.md",
            "docs/decisions/ADR-0003-local-inference-backend.md",
            "docs/decisions/ADR-0003-tcp-request-framing.md",
            "docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md",
            "docs/decisions/ADR-0008-local-engine-process-ownership.md",
            "src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs",
            "src/Host.Windows/Inference/LocalInferenceBinaryVerifier.cs",
            "tests/HostLifecycle.Tests/LocalInferenceProcessLauncherTests.cs",
            "tests/test_local_inference_binary_verifier.py",
            "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj",
            ".github/workflows/real-inference.yml",
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


    def test_encrypted_relay_boundary_decision_maps_to_fail_closed_transport_and_privacy_tests(self):
        transport = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
        connector = (ROOT / "src/Host.Windows/Networking/PeerTransportFallbackConnector.cs").read_text(encoding="utf-8")
        privacy_tests = (ROOT / "tests/test_relay_privacy_boundary.py").read_text(encoding="utf-8")

        self.assertIn("ADR-0004-encrypted-relay-boundary.md", REGISTER)
        self.assertIn("Libp2pPeerTransport", ADR4_RELAY)
        self.assertIn("PeerTransportFallbackConnector", ADR4_RELAY)
        self.assertIn("tests/test_relay_privacy_boundary.py", REGISTER)
        self.assertIn("no relay data path is enabled", ADR4_RELAY.lower())
        self.assertNotIn(".WithRelay(", transport)
        self.assertIn("throw new PeerConnectionUnavailableException(failures);", connector)

        for case in (
            "test_first_release_relay_is_disabled_and_direct_exhaustion_fails_closed",
            "test_future_relay_must_forward_opaque_end_to_end_protected_bytes",
            "test_relay_metadata_visibility_is_explicit_and_not_promised_away",
        ):
            with self.subTest(case=case):
                self.assertIn(case, privacy_tests)

        # The end-to-end product gate is tracked by the direct-P2P decision, not ADR-0004.
        self.assertIn("AC-P2P-01", ADR2)
        self.assertIn("NOT RUN", ADR2)


    def test_peer_announcement_lease_decision_maps_to_expiry_directory_and_live_acceptance(self):
        paths = (
            "docs/decisions/ADR-0006-peer-announcement-leases.md",
            "src/Shared.Contracts/Networking/PeerAnnouncement.cs",
            "src/Host.Windows/Networking/PeerAnnouncementClient.cs",
            "src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs",
            "src/Rendezvous.Service/PeerAnnouncementDirectory.cs",
            "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR6_LEASE)

        contract = (ROOT / "src/Shared.Contracts/Networking/PeerAnnouncement.cs").read_text(encoding="utf-8")
        client = (ROOT / "src/Host.Windows/Networking/PeerAnnouncementClient.cs").read_text(encoding="utf-8")
        publisher = (ROOT / "src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs").read_text(encoding="utf-8")
        directory = (ROOT / "src/Rendezvous.Service/PeerAnnouncementDirectory.cs").read_text(encoding="utf-8")
        acceptance_tests = (ROOT / "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs").read_text(encoding="utf-8")

        self.assertIn("MaximumMessageBytes = 16 * 1024", contract)
        self.assertIn("MaximumLifetime = TimeSpan.FromHours(24)", contract)
        self.assertIn("Rendezvous requires HTTPS except for loopback tests.", client)
        self.assertIn("DefaultTimeToLive = TimeSpan.FromMinutes(5)", publisher)
        self.assertIn("DefaultRenewalInterval = TimeSpan.FromMinutes(1)", publisher)
        self.assertIn("DateTimeOffset.UtcNow.Add(_timeToLive)", publisher)
        self.assertIn("Task.Delay(_renewalInterval, cancellationToken)", publisher)
        self.assertIn("expires naturally", publisher.lower())
        self.assertIn("RemoveExpired(DateTimeOffset.UtcNow)", directory)
        self.assertIn("pair.Value.ExpiresAtUtc <= now", directory)
        self.assertIn("_current.Remove(id)", directory)
        self.assertIn("PeriodicLeaseRenewalRefreshesTtlAndStopsAsync", acceptance_tests)
        self.assertIn("ExpiresAtUtc > firstExpiry", acceptance_tests)
        self.assertIn("await lease.StopAsync()", acceptance_tests)
        self.assertIn("AC-P2P-01 remains `NOT RUN`", ADR6_LEASE)
        self.assertIn("T0121 remains `in_progress`", ADR6_LEASE)


    def test_single_active_inference_decision_maps_to_admission_coordinator_and_protocol_tests(self):
        paths = (
            "docs/decisions/ADR-0007-single-active-inference.md",
            "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs",
            "src/Host.Windows/Requests/HostRequestCoordinator.cs",
            "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs",
            "tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs",
            "tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs",
            "tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs",
            "tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR7_SINGLE_ACTIVE)

        self.assertIn("REQ-005", ADR7_SINGLE_ACTIVE)
        self.assertIn("REQ-015", ADR7_SINGLE_ACTIVE)
        self.assertIn("REQ-016", ADR7_SINGLE_ACTIVE)
        self.assertIn("AC-P2P-01", ADR7_SINGLE_ACTIVE)
        self.assertIn("NOT RUN", ADR7_SINGLE_ACTIVE)

        policy = (ROOT / "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs").read_text(encoding="utf-8")
        coordinator = (ROOT / "src/Host.Windows/Requests/HostRequestCoordinator.cs").read_text(encoding="utf-8")
        mapper = (ROOT / "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs").read_text(encoding="utf-8")
        busy = (ROOT / "tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs").read_text(encoding="utf-8")
        failure_tests = (ROOT / "tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs").read_text(encoding="utf-8")
        response_tests = (ROOT / "tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        for token in ("InactiveParticipation", "ModelNotReady", "ModelMismatch", "CapacityUnknown", "CapacityInsufficient", "DeadlineExpired"):
            with self.subTest(policy_token=token):
                self.assertIn(token, policy)
        self.assertIn("WaitAsync(0, cancellationToken)", coordinator)
        self.assertIn("activeCancellation.Token.ThrowIfCancellationRequested()", coordinator)
        self.assertIn("the late result was withheld", coordinator)
        self.assertIn("HostRequestCoordinatorBusyTests.RunAsync()", entrypoint)
        self.assertIn("HostRequestCoordinatorFailureAcceptanceTests.RunAsync()", entrypoint)
        self.assertIn("HostRequestAdmissionTests.RunAsync()", entrypoint)
        self.assertIn("HostRequestResponseDeliveryTests.RunAsync()", entrypoint)
        self.assertIn("HostBusy", mapper)
        self.assertIn("UnsupportedModel", mapper)
        self.assertIn("ProtocolErrorCode.ResourceLimit", mapper)
        self.assertIn("Concurrent requests must not queue", busy)
        self.assertIn("ExpiredDeadlineIsRejectedBeforeInference", failure_tests)
        self.assertIn("InternalFailureIsSanitized", response_tests)

    def test_local_engine_process_decision_maps_to_verified_shell_free_launch_and_ownership(self):
        paths = (
            "docs/decisions/ADR-0008-local-engine-process-ownership.md",
            "src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs",
            "src/Host.Windows/Inference/LocalInferenceBinaryVerifier.cs",
            "tests/HostLifecycle.Tests/LocalInferenceProcessLauncherTests.cs",
            "tests/test_local_inference_binary_verifier.py",
            "tests/HostLifecycle.Tests/Program.cs",
            "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj",
            ".github/workflows/real-inference.yml",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR8_PROCESS_LAUNCH)

        launcher = (ROOT / "src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs").read_text(encoding="utf-8")
        verifier = (ROOT / "src/Host.Windows/Inference/LocalInferenceBinaryVerifier.cs").read_text(encoding="utf-8")
        csharp_tests = (ROOT / "tests/HostLifecycle.Tests/LocalInferenceProcessLauncherTests.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        project = (ROOT / "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj").read_text(encoding="utf-8")
        workflow = (ROOT / ".github/workflows/real-inference.yml").read_text(encoding="utf-8")

        self.assertIn("WHITEP_LLAMA_SERVER_VERSION", launcher)
        self.assertIn("WHITEP_LLAMA_SERVER_SHA256", launcher)
        self.assertLess(
            verifier.index("CryptographicOperations.FixedTimeEquals(expectedHash, actualHash)"),
            verifier.index('startInfo.ArgumentList.Add("--version")'),
        )
        self.assertIn("The llama.cpp executable changed during version verification", verifier)
        self.assertIn("UseShellExecute = false", launcher)
        self.assertIn('info.ArgumentList.Add("--host")', launcher)
        self.assertIn('info.ArgumentList.Add("127.0.0.1")', launcher)
        self.assertIn('info.ArgumentList.Add("--no-webui")', launcher)
        self.assertIn("process.Kill(entireProcessTree: true)", launcher)
        self.assertIn("DefaultStopTimeout = TimeSpan.FromSeconds(5)", launcher)
        self.assertIn("info.ArgumentList.SequenceEqual(expected)", csharp_tests)
        self.assertIn("Non-loopback bind must be rejected.", csharp_tests)
        self.assertIn("LocalInferenceProcessLauncherTests.Run();", harness)
        self.assertIn("LocalInferenceBinaryVerifier.cs", project)
        self.assertIn('"WHITEP_LLAMA_SERVER_VERSION=11443"', workflow)
        self.assertIn('"WHITEP_LLAMA_SERVER_SHA256=$serverSha256"', workflow)
        self.assertIn("AC-P2P-01", ADR8_PROCESS_LAUNCH)
        self.assertIn("NOT RUN", ADR8_PROCESS_LAUNCH)


    def test_agent_tool_execution_decision_maps_to_fail_closed_bounded_worker(self):
        paths = (
            "docs/requirements.md",
            "src/Host.Windows/Chat/AgentToolCapabilities.cs",
            "src/Host.Windows/Tools/AgentToolRegistry.cs",
            "src/Host.Windows/Tools/AgentToolExecutionCoordinator.cs",
            "src/Host.Windows/Tools/AgentToolProcessExecutor.cs",
            "src/Host.Windows/Tools/AgentToolWorkerHost.cs",
            "src/Host.Windows/Tools/WpfAgentToolConfirmationService.cs",
            "tests/HostLifecycle.Tests/AgentToolExecutionAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/AgentToolProcessExecutorTests.cs",
            "tests/HostLifecycle.Tests/AgentToolPolicyRejectionAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/AgentToolCapabilityAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/AgentToolRegistryAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0009-agent-tool-execution-boundary.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR9_AGENT_TOOLS)

        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        registry = (ROOT / "src/Host.Windows/Tools/AgentToolRegistry.cs").read_text(encoding="utf-8")
        coordinator = (ROOT / "src/Host.Windows/Tools/AgentToolExecutionCoordinator.cs").read_text(encoding="utf-8")
        executor = (ROOT / "src/Host.Windows/Tools/AgentToolProcessExecutor.cs").read_text(encoding="utf-8")
        worker = (ROOT / "src/Host.Windows/Tools/AgentToolWorkerHost.cs").read_text(encoding="utf-8")
        capabilities = (ROOT / "src/Host.Windows/Chat/AgentToolCapabilities.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("REQ-017", requirements)
        self.assertIn("REQ-017", ADR9_AGENT_TOOLS)
        self.assertIn("GetModelAdvertisableDefinitionsAsync", registry)
        self.assertIn("AgentToolCapabilityPolicy.EvaluateAsync", coordinator)
        self.assertIn("RequiresUserConfirmation", coordinator)
        self.assertIn("UseShellExecute = false", executor)
        self.assertIn('info.ArgumentList.Add("--agent-tool-worker")', executor)
        self.assertIn("info.Environment.Clear()", executor)
        self.assertIn("process.Kill(entireProcessTree: true)", executor)
        self.assertIn("MaximumRequestBytes = 64 * 1024", worker)
        self.assertIn("MaximumResponseBytes = 64 * 1024", worker)
        self.assertIn("MaximumReportAge", capabilities)
        self.assertIn("same operating-system user", ADR9_AGENT_TOOLS)
        self.assertIn("AppContainer", ADR9_AGENT_TOOLS)
        self.assertIn("AgentToolExecutionAcceptanceTests.RunAsync();", harness)
        self.assertIn("AgentToolProcessExecutorTests.RunAsync();", harness)
        self.assertIn("AgentToolPolicyRejectionAcceptanceTests.RunAsync();", harness)
        self.assertIn("AgentToolCapabilityAcceptanceTests.RunAsync();", harness)
        self.assertIn("AgentToolRegistryAcceptanceTests.RunAsync();", harness)
        self.assertIn("ADR-0009-agent-tool-execution-boundary.md", REGISTER)


    def test_verified_model_install_decision_maps_to_streaming_integrity_and_atomic_activation(self):
        paths = (
            "docs/requirements.md",
            "src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json",
            "src/Host.Windows/Models/Catalog/ModelCatalogParser.cs",
            "src/Host.Windows/Models/Catalog/ModelCatalogFileLoader.cs",
            "src/Host.Windows/Models/Catalog/OfflineModelCatalogStore.cs",
            "src/Host.Windows/Models/ModelStreamingDownloader.cs",
            "src/Host.Windows/Models/VerifiedModelInstaller.cs",
            "tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/ModelStreamingDownloaderAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/IncompleteModelDownloadAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/ModelDownloadCancellationAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/UpdateModelSafetyAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj",
            "tests/test_model_catalog_schema.py",
            "tests/test_model_catalog_parser.py",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0010-verified-model-installation.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR10_MODEL_INSTALLATION)

        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        parser = (ROOT / "src/Host.Windows/Models/Catalog/ModelCatalogParser.cs").read_text(encoding="utf-8")
        downloader = (ROOT / "src/Host.Windows/Models/ModelStreamingDownloader.cs").read_text(encoding="utf-8")
        installer = (ROOT / "src/Host.Windows/Models/VerifiedModelInstaller.cs").read_text(encoding="utf-8")
        catalog_tests = (ROOT / "tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs").read_text(encoding="utf-8")
        download_tests = (ROOT / "tests/HostLifecycle.Tests/ModelStreamingDownloaderAcceptanceTests.cs").read_text(encoding="utf-8")
        install_tests = (ROOT / "tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs").read_text(encoding="utf-8")
        update_tests = (ROOT / "tests/HostLifecycle.Tests/UpdateModelSafetyAcceptanceTests.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("REQ-007", requirements)
        self.assertIn("REQ-008", requirements)
        self.assertIn("MaximumCatalogBytes = 2 * 1024 * 1024", parser)
        self.assertIn("MaximumCatalogModels = 10_000", parser)
        self.assertIn("ParseAllowlistedHttpsUrl", parser)
        self.assertIn("MaximumRedirects = 5", downloader)
        self.assertIn("Content-Length does not match validated catalog metadata", downloader)
        self.assertIn("The model response exceeded its validated size", downloader)
        self.assertIn("The model response ended before its validated size", downloader)
        self.assertIn("ValidateSourceUrl(currentUri)", downloader)
        self.assertIn("SHA256.HashDataAsync", installer)
        self.assertIn("CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)", installer)
        self.assertIn("ValidateModelFileFormatAsync", installer)
        self.assertIn("File.Replace(temporaryPath, fullDestinationPath, backupPath, ignoreMetadataErrors: true)", installer)
        self.assertIn("File.Move(temporaryPath, fullDestinationPath)", installer)
        self.assertIn("InstallsOnlyAfterSha256Matches();", install_tests)
        self.assertIn("RejectsChecksumMismatchAndDeletesStagingFile();", install_tests)
        self.assertIn("UpdatesExistingModelAtomicallyAfterValidation();", install_tests)
        self.assertIn("RejectsFailedUpdateAndPreservesPreviousModel();", install_tests)
        self.assertIn("RejectsUnapprovedSourceAndRedirectHosts();", download_tests)
        self.assertIn("RejectsTruncatedPayloadAndRemovesPartialFile();", download_tests)
        self.assertIn("OfflineCatalogStoreRejectsInvalidCache();", catalog_tests)
        self.assertIn("CancellationDuringUpdatePreservesExistingModel();", update_tests)
        self.assertIn("ModelCatalogParserAcceptanceTests.Run();", harness)
        self.assertIn("ModelStreamingDownloaderAcceptanceTests.Run();", harness)
        self.assertIn("VerifiedModelInstallerAcceptanceTests.Run();", harness)
        self.assertIn("IncompleteModelDownloadAcceptanceTests.Run();", harness)
        self.assertIn("ModelDownloadCancellationAcceptanceTests.Run();", harness)
        self.assertIn("UpdateModelSafetyAcceptanceTests.Run();", harness)
        self.assertIn("catalog metadata", ADR10_MODEL_INSTALLATION)
        self.assertIn("not independent proof of publisher identity", ADR10_MODEL_INSTALLATION)
        self.assertIn("ADR-0010-verified-model-installation.md", REGISTER)


    def test_every_registered_decision_links_to_existing_sources_and_verification(self):
        rows = [
            line for line in REGISTER.splitlines()
            if line.startswith("| [ADR-")
        ]
        self.assertTrue(rows, "The decision register must contain decision rows.")

        for row in rows:
            cells = [
                cell.strip()
                for cell in row.strip().strip("|").split("|")
            ]
            with self.subTest(decision_row=cells[0] if cells else row):
                self.assertEqual(5, len(cells), row)
                match = re.search(r"\]\(([^)]+\.md)\)", cells[0])
                self.assertIsNotNone(match, "Decision row must link its ADR document.")
                decision_doc = ROOT / "docs" / "decisions" / match.group(1)
                self.assertTrue(decision_doc.is_file(), str(decision_doc))

                for label, column_index in (
                    ("source/configuration", 2),
                    ("test/verification", 3),
                ):
                    linked_paths = [
                        reference
                        for reference in re.findall(r"`([^`]+)`", cells[column_index])
                        if "/" in reference
                        and not reference.startswith(("https://", "http://"))
                    ]
                    self.assertTrue(
                        linked_paths,
                        f"{decision_doc.name} must link {label} paths.",
                    )
                    for relative_path in linked_paths:
                        with self.subTest(decision=decision_doc.name, reference=relative_path):
                            self.assertTrue(
                                (ROOT / relative_path).exists(),
                                f"{decision_doc.name} references missing path {relative_path}",
                            )
                    if label == "test/verification":
                        self.assertTrue(
                            any(path.startswith(("tests/", ".github/workflows/")) for path in linked_paths),
                            f"{decision_doc.name} must link an executable test or workflow.",
                        )
                self.assertTrue(cells[4], f"{decision_doc.name} must state its evidence boundary.")


    def test_every_adr_document_is_registered(self):
        decision_dir = ROOT / "docs" / "decisions"
        decision_files = sorted(path.name for path in decision_dir.glob("ADR-*.md"))
        registered_files = []

        for line in REGISTER.splitlines():
            if not line.startswith("| [ADR-"):
                continue
            link_start = line.find("](")
            link_end = line.find(")", link_start + 2)
            self.assertGreaterEqual(
                link_start,
                0,
                f"Decision row has no ADR link: {line}",
            )
            self.assertGreater(
                link_end,
                link_start + 2,
                f"Decision row has a malformed ADR link: {line}",
            )
            registered_files.append(Path(line[link_start + 2:link_end]).name)

        self.assertTrue(decision_files, "At least one ADR document must exist.")
        self.assertEqual(
            len(registered_files),
            len(set(registered_files)),
            "Each ADR document should appear at most once in the decision register.",
        )
        self.assertEqual(
            decision_files,
            sorted(registered_files),
            "Every docs/decisions/ADR-*.md file must have exactly one row in the decision register.",
        )


if __name__ == "__main__":
    unittest.main()

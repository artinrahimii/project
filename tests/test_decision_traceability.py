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
ADR11_HOST_CAPACITY = (ROOT / "docs/decisions/ADR-0011-host-capacity-readiness.md").read_text(encoding="utf-8")
ADR12_LOCAL_SETTINGS = (ROOT / "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md").read_text(encoding="utf-8")
ADR13_TEMP_REQUEST_ID = (ROOT / "docs/decisions/ADR-0013-temporary-request-identifiers.md").read_text(encoding="utf-8")
ADR14_CROSS_PLATFORM_PROTOCOL = (ROOT / "docs/decisions/ADR-0014-versioned-cross-platform-protocol.md").read_text(encoding="utf-8")
ADR15_PARTICIPATION_LIFECYCLE = (ROOT / "docs/decisions/ADR-0015-fail-closed-participation-listener-lifecycle.md").read_text(encoding="utf-8")
ADR16_CRYPTO_TRANSPORT = (ROOT / "docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md").read_text(encoding="utf-8")
ADR17_HOST_IDENTITY = (ROOT / "docs/decisions/ADR-0017-windows-user-scoped-host-identity-storage.md").read_text(encoding="utf-8")
ADR18_EXPLICIT_PEER_TRUST = (ROOT / "docs/decisions/ADR-0018-explicit-peer-trust-and-identity-verification.md").read_text(encoding="utf-8")


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
        self.assertIn("not a detached signature or independent proof of publisher identity", ADR10_MODEL_INSTALLATION)
        self.assertIn("ADR-0010-verified-model-installation.md", REGISTER)


    def test_host_capacity_decision_maps_to_observed_measurements_and_fail_closed_readiness(self):
        paths = (
            "docs/requirements.md",
            "docs/acceptance.md",
            "src/Host.Domain/HostCapabilities.cs",
            "src/Host.Application/HostReadinessEvaluator.cs",
            "src/Host.Application/HostReadinessResult.cs",
            "src/Host.Windows/WindowsHostHardwareDiscovery.cs",
            "src/Host.Windows/MainWindow.xaml.cs",
            "src/Host.Windows/MainWindow.xaml",
            "tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            "tests/test_windows_hardware_discovery.py",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0011-host-capacity-readiness.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR11_HOST_CAPACITY)

        domain = (ROOT / "src/Host.Domain/HostCapabilities.cs").read_text(encoding="utf-8")
        evaluator = (ROOT / "src/Host.Application/HostReadinessEvaluator.cs").read_text(encoding="utf-8")
        discovery = (ROOT / "src/Host.Windows/WindowsHostHardwareDiscovery.cs").read_text(encoding="utf-8")
        window = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
        csharp_tests = (ROOT / "tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        hardware_tests = (ROOT / "tests/test_windows_hardware_discovery.py").read_text(encoding="utf-8")

        self.assertIn("REQ-006", REQUIREMENTS)
        self.assertIn("AC-HOST-01", ACCEPTANCE)
        self.assertIn("GlobalMemoryStatusEx", discovery)
        self.assertIn("memoryStatus.AvailablePhysical", discovery)
        self.assertIn("new DriveInfo(systemDriveRoot)", discovery)
        self.assertIn("HostCapacityState.Insufficient", domain)
        self.assertIn("HostCapacityState.Unknown", domain)
        self.assertIn("return HostCapacityState.Ready", domain)
        self.assertIn("CanRunInference => CapacityState == HostCapacityState.Ready", domain)
        self.assertIn("double.IsFinite(requiredGiB)", evaluator)
        self.assertIn("HostReadinessEvaluator.Evaluate", window)
        self.assertIn("capacityState == HostCapacityState.Ready", window)
        self.assertIn("GPU compute/VRAM capability is not verified", window)
        self.assertIn("HostCapacityAcceptanceTests.Run();", harness)
        self.assertIn("Low RAM with other checks unknown", csharp_tests)
        self.assertIn("Missing checks and measurements", csharp_tests)
        self.assertIn("test_capacity_is_unknown_until_measurements_and_readiness_checks_are_verified", hardware_tests)
        self.assertIn("ADR-0011-host-capacity-readiness.md", REGISTER)


    def test_local_settings_decision_maps_to_bounded_schema_versioned_fail_closed_persistence(self):
        paths = (
            "docs/requirements.md",
            "src/Host.Infrastructure/LocalHostSettingsStore.cs",
            "src/Host.Windows/MainWindow.xaml.cs",
            ".env.example",
            ".gitignore",
            "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            "tests/test_local_config_safety.py",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR12_LOCAL_SETTINGS)

        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        store = (ROOT / "src/Host.Infrastructure/LocalHostSettingsStore.cs").read_text(encoding="utf-8")
        window = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
        persistence_tests = (ROOT / "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        safety_tests = (ROOT / "tests/test_local_config_safety.py").read_text(encoding="utf-8")

        self.assertIn("REQ-014", requirements)
        self.assertIn("CurrentSchemaVersion = 1", store)
        self.assertIn("Environment.SpecialFolder.LocalApplicationData", store)
        self.assertIn("endpoint.IsLoopback", store)
        self.assertIn("endpoint.UserInfo.Length != 0", store)
        self.assertIn("RequestTimeoutSeconds is < 1 or > 3600", store)
        self.assertIn("FileMode.CreateNew", store)
        self.assertIn("stream.Flush(flushToDisk: true)", store)
        self.assertIn("File.Move(temporaryPath, _filePath, overwrite: true)", store)
        self.assertIn("settingsLoadFailed = true;", window)
        self.assertIn("LocalHostSettingsPersistenceTests.Run();", harness)
        self.assertIn("LoadsLegacySettingsWithoutSelection", persistence_tests)
        self.assertIn("Failed validation must not rewrite the user's invalid settings file.", persistence_tests)
        self.assertIn("test_wpf_startup_contains_semantically_invalid_or_unreadable_settings", safety_tests)
        self.assertIn("test_environment_example_is_allowlisted_and_non_secret", safety_tests)
        self.assertIn("not auto-loaded", (ROOT / ".env.example").read_text(encoding="utf-8"))
        self.assertIn("LocalHostSettingsStore.cs", ADR12_LOCAL_SETTINGS)
        self.assertIn("This is not an OS permission/ACL audit", ADR12_LOCAL_SETTINGS)
        self.assertIn("ADR-0012-local-settings-validation-and-persistence.md", REGISTER)



    def test_temporary_request_identity_decision_maps_to_crypto_ids_and_metadata_only_rendezvous(self):
        paths = (
            "docs/requirements.md",
            "protocol/whitep/v1/messages.proto",
            "src/Host.Windows/Requests/SecureRequestIdGenerator.cs",
            "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs",
            "src/Shared.Contracts/ProtocolMessageValidator.cs",
            "src/Shared.Contracts/Networking/PeerAnnouncement.cs",
            "src/Rendezvous.Service/PeerAnnouncementDirectory.cs",
            "tests/HostLifecycle.Tests/SecureRequestIdGeneratorTests.cs",
            "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0013-temporary-request-identifiers.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR13_TEMP_REQUEST_ID)

        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        protocol = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
        generator = (ROOT / "src/Host.Windows/Requests/SecureRequestIdGenerator.cs").read_text(encoding="utf-8")
        id_tests = (ROOT / "tests/HostLifecycle.Tests/SecureRequestIdGeneratorTests.cs").read_text(encoding="utf-8")
        validator = (ROOT / "src/Shared.Contracts/ProtocolMessageValidator.cs").read_text(encoding="utf-8")
        mapper = (ROOT / "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs").read_text(encoding="utf-8")
        announcement = (ROOT / "src/Shared.Contracts/Networking/PeerAnnouncement.cs").read_text(encoding="utf-8")
        directory = (ROOT / "src/Rendezvous.Service/PeerAnnouncementDirectory.cs").read_text(encoding="utf-8")
        rendezvous_tests = (ROOT / "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs").read_text(encoding="utf-8")
        harness = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("REQ-012", requirements)
        self.assertIn("request_id", protocol)
        self.assertIn("RandomByteCount = 16", generator)
        self.assertIn("RandomNumberGenerator.GetBytes(RandomByteCount)", generator)
        self.assertIn('Prefix = "req_"', generator)
        self.assertIn("sampleCount = 4096", id_tests)
        self.assertIn("identifiers.Add(requestId)", id_tests)
        self.assertIn("MaximumRequestIdentifierLength = 128", validator)
        self.assertIn("IsValidRequestIdentifier(requestId)", validator)
        self.assertIn("RequireIdentifier(requestId, nameof(requestId))", mapper)
        self.assertIn("PeerId = ValidatePeerId(peerId)", announcement)
        self.assertIn("Dictionary<string, PeerAnnouncement>", directory)
        self.assertNotIn("request_id", directory.lower())
        self.assertIn("PeerAnnouncement must expose only peer ID, addresses, capabilities and expiry.", rendezvous_tests)
        self.assertIn("SecureRequestIdGeneratorTests.Run();", harness)
        self.assertIn("collision probability", ADR13_TEMP_REQUEST_ID.lower())
        self.assertIn("AC-P2P-01", ADR13_TEMP_REQUEST_ID)



    def test_versioned_cross_platform_protocol_decision_maps_to_canonical_schema_and_wire_checks(self):
        paths = (
            "docs/requirements.md",
            "protocol/whitep/v1/messages.proto",
            "src/Shared.Contracts/WhiteP.Shared.Contracts.csproj",
            "src/Shared.Contracts/ProtocolVersionPolicy.cs",
            "src/Client.Android/app/build.gradle.kts",
            "tests/HostLifecycle.Tests/Program.cs",
            "src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt",
            "tests/contracts/protobuf-wire-fixtures.txt",
            "tests/test_proto_contract.py",
            "tests/contracts/test_contract_layout.py",
            ".github/workflows/product-build.yml",
        )
        self.assertTrue((ROOT / "docs/decisions/ADR-0014-versioned-cross-platform-protocol.md").is_file())
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR14_CROSS_PLATFORM_PROTOCOL)

        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        schema = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
        shared_project = (ROOT / "src/Shared.Contracts/WhiteP.Shared.Contracts.csproj").read_text(encoding="utf-8")
        android_build = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
        version_policy = (ROOT / "src/Shared.Contracts/ProtocolVersionPolicy.cs").read_text(encoding="utf-8")
        csharp_round_trip = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        kotlin_round_trip = (ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt").read_text(encoding="utf-8")
        fixtures = (ROOT / "tests/contracts/protobuf-wire-fixtures.txt").read_text(encoding="utf-8")
        proto_tests = (ROOT / "tests/test_proto_contract.py").read_text(encoding="utf-8")
        layout_tests = (ROOT / "tests/contracts/test_contract_layout.py").read_text(encoding="utf-8")
        workflow = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")

        self.assertIn("REQ-013", requirements)
        self.assertIn('package whitep.protocol.v1;', schema)
        self.assertIn('Include="../../protocol/whitep/v1/messages.proto"', shared_project)
        self.assertIn('ProtoRoot="../../protocol"', shared_project)
        self.assertIn('srcDir("../../../protocol")', android_build)
        self.assertIn('create("java") { option("lite") }', android_build)
        self.assertIn('create("kotlin") { option("lite") }', android_build)
        self.assertIn('implementation("com.google.protobuf:protobuf-kotlin-lite:4.36.2")', android_build)
        self.assertIn('public const string CurrentVersion = "1.0.0";', version_policy)
        self.assertIn('AssertProtoRoundTrip("InferenceRequest"', csharp_round_trip)
        self.assertIn('assertRoundTrip("InferenceRequest"', kotlin_round_trip)
        self.assertIn('findRepositoryFile("tests/contracts/protobuf-wire-fixtures.txt")', kotlin_round_trip)
        self.assertIn("FindRepositoryFile", csharp_round_trip)
        self.assertIn("InferenceRequest", fixtures)
        self.assertIn("Base64(wire bytes)", fixtures)
        self.assertIn("test_android_generates_java_and_kotlin_lite_from_the_same_repository_schema", proto_tests)
        self.assertIn("test_canonical_schema_remains_the_only_source_proto", layout_tests)
        self.assertIn("Verify generated Protobuf C# sources", workflow)
        self.assertIn("Verify generated Protobuf Java and Kotlin sources", workflow)
        self.assertIn("AC-P2P-01", ADR14_CROSS_PLATFORM_PROTOCOL)
        self.assertIn("NOT RUN", ADR14_CROSS_PLATFORM_PROTOCOL)



    def test_participation_listener_lifecycle_decision_maps_to_fail_closed_wpf_shutdown(self):
        paths = (
            "src/Host.Windows/MainWindow.xaml",
            "src/Host.Windows/MainWindow.xaml.cs",
            "src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs",
            "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs",
            "src/Host.Application/ParticipationActivationRecovery.cs",
            "tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs",
            "tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs",
            "tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs",
            "tests/test_network_listener_exposure.py",
            ".github/workflows/product-build.yml",
        )
        self.assertIn("ADR-0015-fail-closed-participation-listener-lifecycle.md", REGISTER)
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(relative_path, ADR15_PARTICIPATION_LIFECYCLE)

        window_xaml = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
        window = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
        lifecycle = (ROOT / "src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs").read_text(encoding="utf-8")
        listener = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
        lifecycle_tests = (ROOT / "tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs").read_text(encoding="utf-8")
        recovery_tests = (ROOT / "tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs").read_text(encoding="utf-8")
        exposure_tests = (ROOT / "tests/test_network_listener_exposure.py").read_text(encoding="utf-8")

        self.assertIn("new(IPAddress.Loopback, 0)", window)
        self.assertIn("await _networkParticipationLifecycle.SetParticipationActiveAsync(active)", window)
        self.assertIn("await _networkParticipationLifecycle.DisposeAsync()", window)
        self.assertIn("Inbound requests are closed until protocol handling is implemented.", window)
        self.assertIn("loopback-only", window_xaml.lower())
        self.assertIn("await _listener.StopAsync()", lifecycle)
        self.assertIn("_participationActive = false", lifecycle)
        self.assertIn("IPAddress.IsLoopback(bindAddress)", listener)
        self.assertIn("DeactivationStopsAndReleasesTheListenerAsync", lifecycle_tests)
        self.assertIn("DisposalStopsAnActiveListenerAsync", lifecycle_tests)
        self.assertIn("SuccessfulRollbackKeepsTheActivationMessageSafeAsync", recovery_tests)
        self.assertIn("FailedRollbackAddsSafeRecoveryGuidanceAsync", recovery_tests)
        self.assertIn("test_window_closing_waits_for_listener_disposal_before_closing_again", exposure_tests)
        self.assertIn("AC-P2P-01", ADR15_PARTICIPATION_LIFECYCLE)
        self.assertIn("NOT RUN", ADR15_PARTICIPATION_LIFECYCLE)


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


    def test_maintained_libp2p_security_decision_maps_to_real_source_and_acceptance(self):
        paths = (
            "docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md",
            "src/Host.Windows/Networking/Libp2pPeerTransport.cs",
            "src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs",
            "src/Shared.Security/PeerSecurityPolicy.cs",
            "src/Shared.Security/WhiteP.Shared.Security.csproj",
            "tests/HostLifecycle.Tests/PeerSecurityPolicyAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs",
            "tests/test_crypto_library_policy.py",
            "tests/HostLifecycle.Tests/Program.cs",
            ".github/workflows/product-build.yml",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR16_CRYPTO_TRANSPORT)

        policy = (ROOT / "src/Shared.Security/PeerSecurityPolicy.cs").read_text(encoding="utf-8")
        transport = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        self.assertIn("new PeerId(identity)", policy)
        self.assertIn("PeerSecurityPolicy.ValidatePeerIdentity(identity);", transport)
        self.assertIn("PeerSecurityPolicyAcceptanceTests.Run()", entrypoint)
        self.assertIn("PeerTransportContractAcceptanceTests.RunAsync()", entrypoint)
        for phrase in ("Nethermind.Libp2p", "Noise/TLS", "not an independent cryptographic audit", "AC-P2P-01", "NOT RUN"):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase.lower(), ADR16_CRYPTO_TRANSPORT.lower())

    def test_user_scoped_host_identity_decision_maps_to_protection_and_storage_tests(self):
        paths = (
            "docs/decisions/ADR-0017-windows-user-scoped-host-identity-storage.md",
            "src/Shared.Security/HostIdentityStore.cs",
            "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs",
            "src/Host.Windows/Security/WindowsHostIdentityProvider.cs",
            "tests/HostLifecycle.Tests/HostIdentityStoreAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/WindowsHostIdentityStoreAcceptanceTests.cs",
            "tests/test_host_identity_store.py",
            ".github/workflows/product-build.yml",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR17_HOST_IDENTITY)

        store = (ROOT / "src/Shared.Security/HostIdentityStore.cs").read_text(encoding="utf-8")
        protector = (ROOT / "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs").read_text(encoding="utf-8")
        windows_tests = (ROOT / "tests/HostLifecycle.Tests/WindowsHostIdentityStoreAcceptanceTests.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        self.assertIn("LoadOrCreate", store)
        self.assertIn("DataProtectionScope.CurrentUser", protector)
        self.assertIn("tampered", windows_tests.lower())
        self.assertIn("HostIdentityStoreAcceptanceTests.Run()", entrypoint)
        for phrase in ("DataProtectionScope.CurrentUser", "at-rest protection", "AC-P2P-01", "NOT RUN"):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase.lower(), ADR17_HOST_IDENTITY.lower())

    def test_explicit_peer_trust_decision_maps_to_persistent_store_transport_gate_and_acceptance(self):
        paths = (
            "docs/decisions/ADR-0018-explicit-peer-trust-and-identity-verification.md",
            "src/Shared.Security/PeerTrustStore.cs",
            "src/Host.Windows/Networking/Libp2pPeerTransport.cs",
            "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs",
            "tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/WindowsPeerTrustStoreAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs",
            "tests/HostLifecycle.Tests/Program.cs",
            ".github/workflows/product-build.yml",
            "tests/test_decision_traceability.py",
        )
        for relative_path in paths:
            with self.subTest(path=relative_path):
                self.assertTrue((ROOT / relative_path).is_file(), relative_path)
                self.assertIn(Path(relative_path).name, REGISTER)
                if relative_path != paths[0]:
                    self.assertIn(relative_path, ADR18_EXPLICIT_PEER_TRUST)

        store = (ROOT / "src/Shared.Security/PeerTrustStore.cs").read_text(encoding="utf-8")
        transport = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
        protector = (ROOT / "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs").read_text(encoding="utf-8")
        portable_tests = (ROOT / "tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs").read_text(encoding="utf-8")
        windows_tests = (ROOT / "tests/HostLifecycle.Tests/WindowsPeerTrustStoreAcceptanceTests.cs").read_text(encoding="utf-8")
        entrypoint = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")

        self.assertIn("TrustPeerAfterExplicitConfirmation", store)
        self.assertIn("if (!explicitlyConfirmed)", store)
        self.assertIn("ValidateCanonicalPeerId", store)
        self.assertIn("MaximumTrustedPeers", store)
        self.assertIn('throw new AuthenticationException("The peer identity has not been explicitly trusted.")', store)
        self.assertIn("DataProtectionScope.CurrentUser", protector)
        self.assertIn("_peerTrustStore.EnsureTrusted(_expectedRemotePeerIdentity);", transport)
        self.assertIn("_peerTrustStore.EnsureTrusted(endpoint.ExpectedPeerIdentity);", transport)
        self.assertIn("The authenticated libp2p peer identity did not match the expected identity.", transport)
        self.assertIn("explicitlyConfirmed: false", portable_tests)
        self.assertIn("corrupt trust store must fail closed", portable_tests.lower())
        self.assertIn("tampered dpapi peer-trust data must fail closed", windows_tests.lower())
        self.assertIn("PeerTrustStoreAcceptanceTests.Run()", entrypoint)
        self.assertIn("WindowsPeerTrustStoreAcceptanceTests.Run()", entrypoint)
        self.assertIn("PeerTransportContractAcceptanceTests.RunAsync()", entrypoint)

        for phrase in (
            "unknown libp2p peerids are not trusted automatically",
            "current-user dpapi",
            "does not yet present a peer-enrollment prompt",
            "full independent-node e2e remain separate",
        ):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, ADR18_EXPLICIT_PEER_TRUST.lower())

    def test_t0007_execution_report_matches_its_task_folder(self):
        report = (ROOT / "step three/T0007/execution-report.md").read_text(encoding="utf-8")
        self.assertTrue(
            report.startswith("# T0007 execution report"),
            "T0007's report must identify T0007 instead of another task.",
        )
        self.assertIn("T0007-A074", report)
        self.assertIn("**Status:** `in_progress`", report)
        self.assertNotIn("## T0121-A003", report)
        self.assertNotIn("## T0121-A004", report)
        self.assertNotIn("## T0121-A005", report)


if __name__ == "__main__":
    unittest.main()

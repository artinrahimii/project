import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SETTINGS = ROOT / "src/Client.Android/settings.gradle.kts"
APP_BUILD = ROOT / "src/Client.Android/app/build.gradle.kts"
IDENTITY = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt"
TRUST = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt"
TRANSPORT = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt"
TRANSPORT_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt"
RUNTIME = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt"
RESPONSE_CORRELATION = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/InferenceResponseCorrelationValidator.kt"


class AndroidLibp2pTransportContractTests(unittest.TestCase):
    def test_uses_pinned_libp2p_release_and_one_full_protobuf_runtime(self):
        settings = SETTINGS.read_text(encoding="utf-8")
        build = APP_BUILD.read_text(encoding="utf-8")
        self.assertIn("dl.cloudsmith.io/public/libp2p/jvm-libp2p/maven", settings)
        self.assertIn("io.libp2p:jvm-libp2p:1.3.7-RELEASE", build)
        self.assertIn("com.google.protobuf:protobuf-java:4.36.2", build)
        self.assertIn("com.google.protobuf:protobuf-kotlin:4.36.2", build)
        self.assertNotIn("protobuf-javalite", build)
        self.assertNotIn("protobuf-kotlin-lite", build)
        self.assertNotIn('option("lite")', build)
        self.assertIn('exclude(group = "io.netty", module = "netty-codec-native-quic")', build)
        self.assertIn('exclude(group = "io.netty", module = "netty-tcnative-boringssl-static")', build)

    def test_android_identity_is_persistent_and_keystore_protected(self):
        source = IDENTITY.read_text(encoding="utf-8")
        self.assertIn('"AndroidKeyStore"', source)
        self.assertIn('"AES/GCM/NoPadding"', source)
        self.assertIn("generateKeyPair(KeyType.ED25519)", source)
        self.assertIn("unmarshalPrivateKey(cleartext)", source)
        self.assertIn("fails closed", source)
        self.assertNotIn("MODE_WORLD_READABLE", source)

    def test_android_offers_yamux_for_dotnet_and_mplex_for_jvm_compatibility(self):
        source = TRANSPORT.read_text(encoding="utf-8")
        self.assertIn("import io.libp2p.core.mux.StreamMuxerProtocol", source)
        self.assertIn("muxers {", source)
        yamux = source.index("+StreamMuxerProtocol.getYamux()")
        mplex = source.index("+StreamMuxerProtocol.Mplex")
        self.assertLess(yamux, mplex)

    def test_remote_peer_must_be_explicitly_trusted_before_dial(self):
        trust = TRUST.read_text(encoding="utf-8")
        transport = TRANSPORT.read_text(encoding="utf-8")
        self.assertIn("trustPeerAfterExplicitConfirmation", trust)
        self.assertIn("explicitlyConfirmed", trust)
        self.assertIn("trustVerifier.isTrusted(ephemeralHostId)", transport)
        self.assertIn("PeerId.fromBase58(endpoint.peerId)", transport)

    def test_transport_uses_windows_protocol_and_checks_authenticated_correlation(self):
        source = TRANSPORT.read_text(encoding="utf-8")
        self.assertIn('"/white-p/peer-message/1.0.0"', source)
        self.assertIn("PeerMessageFrameCodec.encode(payload)", source)
        self.assertIn("stream.remotePeerId().toBase58()", source)
        correlation = RESPONSE_CORRELATION.read_text(encoding="utf-8")
        self.assertIn("InferenceResponseCorrelationValidator.matches(", source)
        self.assertIn("pending.roundId", source)
        self.assertIn("pending.deadlineUnixMs", source)
        for marker in (
            "response.requestId",
            "response.attemptId",
            "response.roundId",
            "response.ephemeralHostId",
            "response.modelId",
            "response.completedAtUnixMs",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, correlation)
        self.assertIn("awaitResponse", source)

    def test_real_tcp_noise_stream_loopback_acceptance_is_present(self):
        source = TRANSPORT_TEST.read_text(encoding="utf-8")
        self.assertIn("sendsAndReceivesCorrelatedResponseOverAuthenticatedNoiseTcpStream", source)
        self.assertIn("server.start()", source)
        self.assertIn("client.submit(serverPeerId, request)", source)
        self.assertIn("client.awaitResponse", source)
        self.assertIn("response.requestId", source)
        self.assertIn("response.attemptId", source)

    def test_application_runtime_composes_persistent_identity_explicit_trust_directory_and_real_dispatcher(self):
        source = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("AndroidPeerIdentityStore(applicationContext).loadOrCreate()", source)
        self.assertIn("AndroidPeerTrustStore(applicationContext)", source)
        self.assertIn("RendezvousPeerDirectoryClient(rendezvousServiceBaseUri)", source)
        self.assertIn("resolveEndpoint = directory::resolve", source)
        self.assertIn("trustVerifier = trustStore", source)
        self.assertIn("InferenceAttemptDispatcher(transport)", source)
        self.assertIn("trustPeerAfterExplicitConfirmation", source)
        self.assertIn("suspend fun dispatchRound(", source)
        self.assertIn("suspend fun awaitResponse(", source)
        self.assertIn("Callers must supply HostCandidate values", source)

    def test_native_dotnet_peer_interoperability_is_wired_into_android_ci(self):
        workflow = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
        program = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
        fixture = (ROOT / "tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs").read_text(encoding="utf-8")
        transport_test = TRANSPORT_TEST.read_text(encoding="utf-8")
        runtime = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("WHITEP_INTEROP_SERVER_DLL", workflow)
        self.assertIn("Build Windows .NET peer interoperability fixture", workflow)
        self.assertIn('"--interop-peer-server"', program)
        self.assertIn("new Libp2pPeerTransport(", fixture)
        self.assertIn("PeerResponseCorrelationValidator.Validate", fixture)
        for phase in (
            "WAITING_FOR_REMOTE_SESSION",
            "SESSION_AUTHENTICATED",
            "APPLICATION_PROTOCOL_READY",
            "RECEIVED",
            "VALIDATED",
            "SENT",
        ):
            self.assertIn(f'Console.WriteLine("{phase}")', fixture)
            self.assertIn(f'"{phase}"', transport_test)
        self.assertIn("ConcurrentLinkedQueue<String>()", transport_test)
        self.assertIn("CompletableFuture<String>()", transport_test)
        self.assertIn("readyLineFuture.get(", transport_test)
        self.assertIn("val reader = process.inputStream.bufferedReader()", transport_test)
        self.assertIn("outputReader.start()", transport_test)
        self.assertIn("line in allowedMarkers", transport_test)
        self.assertIn("val phaseOutput = ConcurrentLinkedQueue<String>()", transport_test)
        self.assertIn("val diagnosticOutput = ConcurrentLinkedQueue<String>()", transport_test)
        self.assertIn("phaseOutput.add(", transport_test)
        self.assertIn("diagnosticOutput.add(", transport_test)
        error_capture = transport_test.split("val phases =", 1)[1].split("val processStates =", 1)[0]
        phase_capture = error_capture.split("val diagnostics =", 1)[0]
        diagnostic_capture = error_capture.split("val diagnostics =", 1)[1]
        self.assertIn("phaseOutput", phase_capture)
        self.assertIn(".toList()", phase_capture)
        self.assertNotIn(".takeLast(", phase_capture)
        self.assertIn("diagnosticOutput", diagnostic_capture)
        self.assertIn(".takeLast(60)", diagnostic_capture)
        self.assertIn("diagnostics=$diagnostics", transport_test)
        self.assertNotIn("serverOutput", transport_test)
        self.assertIn('.joinToString(separator = " | ")', error_capture)
        self.assertIn("dispatchesRoundToThreeNativeDotNetLibp2pHosts", transport_test)
        self.assertIn("internal val lastSubmissionFailure: String?", runtime)
        self.assertIn("attemptStatusSummary", transport_test)

    def test_three_host_noise_integration_exercises_the_application_runtime(self):
        source = TRANSPORT_TEST.read_text(encoding="utf-8")
        self.assertIn("AndroidInferenceRequestRuntime(", source)
        self.assertIn("client.dispatchRound(", source)
        self.assertIn("client.awaitResponse(", source)
        self.assertIn("dispatchesOneSharedRequestWithDistinctAttemptsOverAuthenticatedPeerStreams", source)


    def test_dotnet_ready_record_uses_matching_tab_delimiters(self):
        source = TRANSPORT_TEST.read_text(encoding="utf-8")
        self.assertIn('line.startsWith("READY\\t")', source)
        self.assertNotIn('line.startsWith("READY\\\\t")', source)
        self.assertIn("readyLine.split('\\t')", source)



    def test_dotnet_interop_diagnostics_log_templates_without_structured_values(self):
        transport = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
        fixture = (ROOT / "tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs").read_text(encoding="utf-8")
        test_source = TRANSPORT_TEST.read_text(encoding="utf-8")
        self.assertIn("ILoggerProvider? diagnosticLoggerProvider", transport)
        self.assertIn("logging.AddProvider(diagnosticLoggerProvider)", transport)
        self.assertIn('values.FirstOrDefault(static item => item.Key == "{OriginalFormat}")', fixture)
        self.assertIn("exception?.GetType().Name", fixture)
        self.assertIn("() => Interlocked.Increment(ref _written) <= 80", fixture)
        self.assertIn("Func<bool> allowWrite", fixture)
        self.assertIn("!allowWrite()", fixture)
        self.assertNotIn("formatter(state, exception)", fixture)
        self.assertIn('line.startsWith("P2P_DIAG\\t")', test_source)

    def test_android_registers_standard_identify_for_dotnet_session_initialization(self):
        source = TRANSPORT.read_text(encoding="utf-8")
        self.assertIn("import io.libp2p.protocol.Identify", source)
        protocol_block = source.split("protocols {", 1)[1].split("}", 1)[0]
        self.assertIn("add(Identify())", protocol_block)
        self.assertIn("add(protocolBindingForHost())", protocol_block)
        self.assertLess(
            protocol_block.index("add(Identify())"),
            protocol_block.index("add(protocolBindingForHost())"),
        )

if __name__ == "__main__":
    unittest.main()

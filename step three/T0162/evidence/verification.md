# T0162 verification evidence — exact implementation candidate

- **Repository:** `artinrahimii/project`
- **Baseline branch/commit:** `main` / `f4848a6b0a5358ab76e2fa8a1257f18016c9e610` (`V 3.0.608`)
- **Task directory:** `step three/T0162/`
- **Incoming edge T0160 → T0162:** `contract_or_interface_dependency`, validated. Evidence: `InferenceRequestPayloadBuilder.kt`, `messages.proto`, `PeerAnnouncement.cs`, T0160 execution report, selector source.
- **Incoming edge T0161 → T0162:** `sequencing_only`, validated. Evidence: `DistributedRequest.kt`, T0161 execution report and selector source. The selector deliberately does not import or accept `DistributedRequest`.
- **Production output:** `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`.
- **JVM test output:** `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`.
- **Repository-level contract test:** `tests/test_android_host_candidate_selector_contract.py`.
- **Behavior covered:** maximum three; stable deterministic ordering; distinct peer identity; matching announcement/capability/status identity; exact protocol compatibility; READY state; free concurrency; requested model present in advertised and runtime models; max-output, prompt-byte, capsule-byte and serialized-request bounds; rejection of malformed or oversized requests.
- **Not claimed:** no network discovery, remote status query, peer connection, inference dispatch, retry loop, or real Android-to-Windows integration is implemented by this task.
- **Test status at candidate creation:** NOT RUN in this tool session. Android Gradle/KtLint/JVM/Protobuf checks and the Python graph/evidence gate must pass on the exact implementation commit before the task can be marked done.

# T0160 verification evidence

- **Historical initial revision:** V 3.0.596 (current `main` at the time this evidence section was first written; its CI results are retained below).
- **Canonical schema:** `protocol/whitep/v1/messages.proto`. The `InferenceRequest` message contains exactly `request_id`, `attempt_id`, `model_id`, `prompt`, `deadline_unix_ms`, `max_output_tokens`, `context_capsule`, and `protocol_version`. It has no history field or permanent conversation identifier.
- **Generated runtime:** `src/Client.Android/app/build.gradle.kts` configures Java and Kotlin Protobuf built-ins with the `lite` option. The generated message therefore does not support the full-runtime `descriptorForType` API; the prior reflection assertion was invalid and caused V 3.0.595 JVM test compilation to fail.
- **Production output:** `src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt`.
- **JVM test output:** `src/Client.Android/app/src/test/java/com/whitep/client/protocol/InferenceRequestPayloadBuilderTest.kt`.
- **Schema/API contract test:** `tests/test_android_inference_request_payload_builder_contract.py`. It asserts the exact Protobuf field/type set and the builder's explicit API parameters; it also checks that the JVM test still covers current-message, capsule, round-trip, and size/invalid-input cases.
- **V 3.0.595 CI results:** Step Three quality passed; Android lint and debug APK build passed; Android JVM test compilation failed on unresolved `descriptorForType`; generated Protobuf verification and APK upload were skipped. Windows WPF and HostLifecycle jobs passed. [Quality workflow 40956394262](https://github.com/artinrahimii/project/actions/runs/40956394262) · [Product workflow 40956394258](https://github.com/artinrahimii/project/actions/runs/40956394258).
- **V 3.0.596 CI:** Step Three quality ran 260 tests; 258 passed and two new Python contract tests failed due to double-escaped regex patterns. At A004 preparation, Android lint/APK and Windows WPF/HostLifecycle had passed, Android JVM tests were still running, and generated-source verification/APK upload had not run. [Quality workflow 41013853810](https://github.com/artinrahimii/project/actions/runs/41013853810) · [Product workflow 41013853757](https://github.com/artinrahimii/project/actions/runs/41013853757). A004 corrects the regex and V 3.0.597 exact-head CI is pending.
- **Scope boundary:** No live peer transport/send/receive is implemented by T0160; this evidence proves payload construction and schema boundary only.


## Final exact-head verification — V 3.0.598

- **Commit:** `b52946919122be39e9bcb0e7687901c329b40241` on `main`.
- **Android KtLint:** `gradle --no-daemon ktlintCheck` passed.
- **Android build:** `gradle --no-daemon assembleDebug` passed.
- **Android JVM tests:** `gradle --no-daemon testDebugUnitTest` passed, including `InferenceRequestPayloadBuilderTest` under `RobolectricTestRunner` with SDK 35. This validates the test's Android JSON codec use; it is not a live transport test.
- **Generated Protobuf:** Java and Kotlin generated-source checks passed.
- **Android artifact:** `whitep-android-debug-apk.zip`, 11,965,862 bytes, artifact 12959356073; upload succeeded.
- **Windows WPF:** formatting, Release build, generated Protobuf, startup smoke, process restrictions, policy rejection, backend/model capability checks, DPAPI host identity and peer-trust persistence checks passed.
- **Windows artifact:** `whitep-windows-host.zip`, 14,254,978 bytes, artifact 12959371027; upload succeeded.
- **HostLifecycle:** Unit/integration tests passed.
- **Step Three quality:** 260 Python tests passed; compileall, graph structure, and output/evidence checks passed. The validated graph contained 229 tasks and 417 edges.
- **Workflows:** [Product build 41014428574](https://github.com/artinrahimii/project/actions/runs/41014428574) and [quality gate 41014428583](https://github.com/artinrahimii/project/actions/runs/41014428583) both completed successfully.
- **Result and boundary:** T0160 meets its acceptance criteria for constructing and serializing only the selected current message plus the encoded context capsule, without full local history. Live peer message delivery is outside this task's verified implementation and is not claimed.

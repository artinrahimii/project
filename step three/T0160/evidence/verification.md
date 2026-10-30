# T0160 verification evidence

- **Revision under verification:** V 3.0.596 (current `main` commit; CI pending).
- **Canonical schema:** `protocol/whitep/v1/messages.proto`. The `InferenceRequest` message contains exactly `request_id`, `attempt_id`, `model_id`, `prompt`, `deadline_unix_ms`, `max_output_tokens`, `context_capsule`, and `protocol_version`. It has no history field or permanent conversation identifier.
- **Generated runtime:** `src/Client.Android/app/build.gradle.kts` configures Java and Kotlin Protobuf built-ins with the `lite` option. The generated message therefore does not support the full-runtime `descriptorForType` API; the prior reflection assertion was invalid and caused V 3.0.595 JVM test compilation to fail.
- **Production output:** `src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt`.
- **JVM test output:** `src/Client.Android/app/src/test/java/com/whitep/client/protocol/InferenceRequestPayloadBuilderTest.kt`.
- **Schema/API contract test:** `tests/test_android_inference_request_payload_builder_contract.py`. It asserts the exact Protobuf field/type set and the builder's explicit API parameters; it also checks that the JVM test still covers current-message, capsule, round-trip, and size/invalid-input cases.
- **V 3.0.595 CI results:** Step Three quality passed; Android lint and debug APK build passed; Android JVM test compilation failed on unresolved `descriptorForType`; generated Protobuf verification and APK upload were skipped. Windows WPF and HostLifecycle jobs passed. [Quality workflow 40956394262](https://github.com/artinrahimii/project/actions/runs/40956394262) · [Product workflow 40956394258](https://github.com/artinrahimii/project/actions/runs/40956394258).
- **V 3.0.596 CI:** Pending; no success is claimed for the new Python contract test, Android JVM suite, Protobuf verification, or full product workflow until their exact-head jobs finish.
- **Scope boundary:** No live peer transport/send/receive is implemented by T0160; this evidence proves payload construction and schema boundary only.

# T0160 execution report

- **Task:** Prove that the serialized inference-request payload contains the selected current message and context capsule, not the full local message history.
- **Base revision:** `6691befd976f87ecdca75767e1cda632168a8f57` on `main` (`V 3.0.593`).
- **Dependency review:** The incoming `T0159 → T0160` and `T0150 → T0160` edges were reviewed against the source and task scopes and remain `blocks`, classified as `sequencing_only`. The protocol's prompt/capsule fields and existing Protobuf round-trip test are independently available; T0160 does not consume a unique output artifact produced by either predecessor.
- **Implementation:** Added `InferenceRequestPayloadBuilder`, a production Kotlin component that builds the repository's actual generated `InferenceRequest` Protobuf message from one explicit `currentMessage`, a locally encoded `ConversationContextCapsule`, and temporary request metadata. It rejects blank IDs/model/prompt/version, nonpositive deadlines/token limits, invalid capsule bytes, prompts over 8,192 UTF-8 bytes, and serialized requests over 16 KiB. The builder has no history or permanent conversation-ID parameter.
- **Runtime tests:** Added four JVM tests to assert selected-message/capsule byte equality and Protobuf round-trip, absence of history/permanent-conversation fields, rejection at the prompt byte ceiling, invalid capsule rejection, and rejection over the serialized-request ceiling.
- **Transport limitation:** This builder creates the exact typed payload for a future direct peer transport; it does not send bytes over a socket and does not claim Android-to-Windows delivery. The existing request UI and ADR explicitly state the transport is not wired yet.
- **Local execution:** No local Android/Gradle runtime is available through the connected GitHub tools. Exact-head CI is the source of test results.
- **Status:** `in_progress`; V 3.0.594 Step Three quality passed, but Android KtLint failed on multiline formatting of two result data classes in `InferenceRequestPayloadBuilder.kt`; APK/JVM/Protobuf steps were skipped. A002 formats those declarations and uses `String(byteArray, UTF_8)` to inspect serialized bytes in the history-exclusion test. V 3.0.595 CI is pending.
- **Next action:** Inspect Android lint, APK build, JVM tests, Protobuf verification, host checks, and graph/evidence validation. Do not mark T0160 complete until its payload assertions pass.


## Exact-head V 3.0.594 test outcomes

- Step Three quality gate: PASS, 257 Python tests and graph/evidence validation, [workflow 40955832262](https://github.com/artinrahimii/project/actions/runs/40955832262).
- Android product build: FAIL at Kotlin lint. `InferenceRequestPayloadBuilder.kt:23–26` reported required line breaks inside two data-class parameter lists and spacing around the supertype. APK/JVM/Protobuf steps were skipped, [workflow 40955831999](https://github.com/artinrahimii/project/actions/runs/40955831999).
- HostLifecycle unit/integration: PASS. The Windows WPF job was still running when this failure was recorded, so no final result is claimed for that job yet.
- A002 fixes the KtLint layout and replaces the byte-array conversion call with the standard `String(bytes, Charsets.UTF_8)` constructor in the test. Exact-head V 3.0.595 CI is pending.

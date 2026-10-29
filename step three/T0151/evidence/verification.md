# T0151 evidence ledger

- **Model:** `src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsule.kt` adds schema version 1 and fields for goal, key facts, decisions, constraints and unresolved items.
- **Wire alignment:** `protocol/whitep/v1/messages.proto` specifies `bytes context_capsule = 7` with a 4,096-byte cap; `src/Shared.Contracts/ProtocolMessageLimits.cs` defines `MaximumContextCapsuleBytes = 4 * 1024`. The model codec enforces the same hard ceiling against UTF-8 JSON bytes.
- **Unit coverage:** `src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleTest.kt` covers round-trip/escaping, schema version, blank data, exactly 4,096 bytes, over-limit Unicode, malformed JSON, invalid UTF-8, missing/unknown fields and wrong field types.
- **Source-contract tests:** `tests/test_android_context_capsule_contract.py` checks the schema fields, byte cap and presence of runtime test coverage.
- **Dependency review:** T0150 → T0151 validated as `sequencing_only`; T0151 → T0152 as `contract_or_interface_dependency`. Rationale/evidence appended to graph edge history.
- **Not yet proven:** Kotlin lint/compile, Robolectric/JVM execution, APK build, Python suite, graph guard and exact-head CI.


## Exact-head correction history

- V 3.0.548 quality [40881923866](https://github.com/artinrahimii/project/actions/runs/40881923866) passed 234 Python tests and graph/evidence gates.
- V 3.0.548 product [40881924197](https://github.com/artinrahimii/project/actions/runs/40881924197) passed Windows WPF and HostLifecycle, but Android ktlint failed one test-helper formatting rule at `ConversationContextCapsuleTest.kt:147:57`; Android APK/JVM tests were skipped.
- A003 moves the simple helper expression onto the signature line. This fix is not accepted until new exact-head CI passes.


## Additional CI correction history

- V 3.0.549 quality run [40882506760](https://github.com/artinrahimii/project/actions/runs/40882506760) passed the Python/graph/evidence gates.
- V 3.0.549 product run [40882506722](https://github.com/artinrahimii/project/actions/runs/40882506722) failed only the Android main-source ktlint stage at `ConversationContextCapsule.kt:166`; APK/JVM stages were not reached. Windows WPF and HostLifecycle passed.
- A005 reformats this assignment and call only. Runtime correctness and capsule tests are not yet confirmed.


## V 3.0.550 exact-head runtime result and correction

- Quality run [40883740170](https://github.com/artinrahimii/project/actions/runs/40883740170) passed 234 Python tests and graph/evidence gates.
- Product run [40883740159](https://github.com/artinrahimii/project/actions/runs/40883740159): Android lint and debug APK assembly passed; Android JVM tests failed 6 of 36 methods in `ConversationContextCapsuleTest` with `java.lang.RuntimeException` at JSON-backed setup/codec lines. Windows WPF and HostLifecycle jobs passed.
- Full root-exception text was not included in the workflow's failure summary. The test source uses Android `org.json`; the class had no Robolectric runner despite Robolectric already being a project test dependency and being used by `ClientDatabaseTest`.
- A006 adds the existing Robolectric runner configured for SDK 35 plus a source-contract assertion. This fix remains unverified until the next exact-head CI result; T0151 must remain in progress meanwhile.


## Final acceptance — V 3.0.551

- Quality gate [40885453525](https://github.com/artinrahimii/project/actions/runs/40885453525): 235 Python tests passed; graph/evidence checks passed with 229 tasks, 417 edges, zero errors and one preserved T0001 warning.
- Product workflow [40885453676](https://github.com/artinrahimii/project/actions/runs/40885453676): Android lint, debug APK build, JVM unit tests, generated Protobuf verification, APK upload, Windows WPF and HostLifecycle jobs all passed.
- The capsule-specific JVM suite passed after the class was configured with the existing Robolectric runner at SDK 35. The test runner is also asserted by a Python source-contract regression test.
- Acceptance scope is limited to T0151's versioned domain model and strict UTF-8 JSON codec; conversation-scoped Room persistence and UI remain T0152/T0153.

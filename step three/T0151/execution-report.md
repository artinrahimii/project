# T0151 execution report

- **Task:** Define a versioned `ConversationContextCapsule` for goal, key facts, decisions, constraints and unresolved items.
- **Status:** `in_progress`; implementation and tests are committed with exact-head CI pending.
- **Baseline:** `main` at [`V 3.0.547`](https://github.com/artinrahimii/project/commit/796a612c4ba224eaf4750cf9c7f86f4169ba73f4).
- **Attempt:** `T0151-A001`.
- **Protocol boundary:** the canonical Protobuf contract contains opaque `bytes context_capsule = 7` with a 4,096-byte maximum. The Windows shared policy agrees on that hard ceiling. The Android model uses local UTF-8 JSON; no Protobuf wire fields were changed.

## Dependency review

- **T0150 → T0151:** validated as `sequencing_only`. The accepted conversation create/send/reopen workflow does not expose or consume a capsule API. The original `blocks` edge remains as conservative sequence.
- **T0151 → T0152:** validated as `contract_or_interface_dependency`. T0152 stores capsule data as a conversation-scoped Room record and depends on this model's versioned fields and serialization contract. The original `blocks` edge remains.
- Both reviews and evidence paths were appended to graph `edge_change_log`. Other outgoing edges remain unreviewed.

## Implementation

- Added `src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsule.kt` in the idiomatic Kotlin package `com.whitep.client.context`.
- The model defines `schemaVersion`, `goal`, `keyFacts`, `decisions`, `constraints`, and `unresolvedItems`.
- Added UTF-8 JSON encoding/decoding with a strict field allowlist, unsupported-version rejection, malformed-JSON and invalid-UTF-8 rejection, typed validation results, blank-value checks and a 4,096-byte hard ceiling measured on encoded bytes.
- No new dependency was added; the Android `org.json` API is used.
- Added `src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleTest.kt` with eight JVM/Robolectric tests and `tests/test_android_context_capsule_contract.py` with three source-contract tests.
- Persistence and UI are explicitly out of scope here (T0152/T0153). This code makes no P2P-delivery or at-rest-encryption claim.

## Verification status

No local test execution is claimed. Exact-head CI is pending for:
- eight `ConversationContextCapsuleTest` JVM/Robolectric tests;
- Python tests, syntax, graph structure and evidence-path guards;
- Android `ktlintCheck`, `assembleDebug` and `testDebugUnitTest`;
- other repository product checks.

Keep T0151 `in_progress` until the exact-head outcomes are inspected. Record and fix any lint/compiler/test failure before acceptance.


## Exact-head correction history

- **V 3.0.548 quality** [40881923866](https://github.com/artinrahimii/project/actions/runs/40881923866) passed 234 Python tests, Python syntax, graph structure and recorded-output/evidence validation. Graph guard reported 229 tasks/417 edges, zero errors and one preserved T0001 warning.
- **V 3.0.548 product** [40881924197](https://github.com/artinrahimii/project/actions/runs/40881924197): Windows WPF and HostLifecycle jobs passed. Android lint failed at `ConversationContextCapsuleTest.kt:147:57` because `JSONArray.toStringList()` had its single-expression body on a second line; APK assembly and JVM tests did not run.
- **T0151-A002** records that failure. **T0151-A003** corrects only the helper's line wrapping. No Kotlin behavior or model contract changed; exact-head CI is still pending.


## Exact-head correction history — V 3.0.549

- **Quality** [run 40882506760](https://github.com/artinrahimii/project/actions/runs/40882506760) passed 234 Python tests, syntax checks, graph structure and output/evidence validation (229 tasks/417 edges, zero errors; one preserved T0001 metadata warning).
- **Product** [run 40882506722](https://github.com/artinrahimii/project/actions/runs/40882506722): Windows WPF and HostLifecycle jobs passed. Android lint failed in the model source at `ConversationContextCapsule.kt:166`: the long `unresolvedItems` assignment needed multiline formatting. APK build and Android JVM tests were skipped.
- **T0151-A004** records that observed failure. **T0151-A005** wraps the property assignment, JSON-array argument, closing parenthesis and Elvis fallback across lines, without changing the model schema or decoding rules. Exact-head CI is pending.

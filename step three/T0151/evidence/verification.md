# T0151 evidence ledger

- **Model:** `src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsule.kt` adds schema version 1 and fields for goal, key facts, decisions, constraints and unresolved items.
- **Wire alignment:** `protocol/whitep/v1/messages.proto` specifies `bytes context_capsule = 7` with a 4,096-byte cap; `src/Shared.Contracts/ProtocolMessageLimits.cs` defines `MaximumContextCapsuleBytes = 4 * 1024`. The model codec enforces the same hard ceiling against UTF-8 JSON bytes.
- **Unit coverage:** `src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleTest.kt` covers round-trip/escaping, schema version, blank data, exactly 4,096 bytes, over-limit Unicode, malformed JSON, invalid UTF-8, missing/unknown fields and wrong field types.
- **Source-contract tests:** `tests/test_android_context_capsule_contract.py` checks the schema fields, byte cap and presence of runtime test coverage.
- **Dependency review:** T0150 → T0151 validated as `sequencing_only`; T0151 → T0152 as `contract_or_interface_dependency`. Rationale/evidence appended to graph edge history.
- **Not yet proven:** Kotlin lint/compile, Robolectric/JVM execution, APK build, Python suite, graph guard and exact-head CI.

# T0159 verification evidence

- Base commit: `b82208a9e613792e3facb52f92d77c7af1facad7` (`V 3.0.590`), branch `main`.
- Prerequisites: T0158 acceptance was verified on `374c7416f6568cd5f267aee71a1069654fbe9afa`; T0150 has existing exact-head Room/UI acceptance evidence. Both incoming edges to T0159 are validated and remain present.
- New runtime tests in `ConversationContextCapsulePersistenceTest.kt`:
  - Exactly 4,096 serialized bytes are accepted; 4,097 bytes are rejected as `TOO_LARGE`.
  - A rejected oversize update preserves the saved capsule and does not append a provenance row.
  - Malformed persisted JSON returns `MALFORMED_JSON`; invalid UTF-8 returns `INVALID_UTF8`; neither appears as a valid capsule.
  - A later valid save repairs the local row and appends a `USER_EDIT` provenance event.
- Extra ViewModel assertions prove edits append provenance, deletion cascades history rows, and unresolved conflicting suggestions cannot append a row before explicit decisions and a successful Save.
- Python source-contract coverage checks both new runtime test cases remain present.
- Exact-head V 3.0.591 CI: pending; no local Gradle result is claimed.


## Exact-head V 3.0.591 verification

- Step Three quality gate passed all 257 Python tests plus graph/evidence validation, [workflow 40954025969](https://github.com/artinrahimii/project/actions/runs/40954025969).
- Android KtLint reported one style error, `Needless blank line(s)`, at `ConversationContextCapsulePersistenceTest.kt:249`. APK/JVM/Protobuf steps were skipped, [workflow 40954025988](https://github.com/artinrahimii/project/actions/runs/40954025988).
- Windows WPF build/startup/host policy/security checks and HostLifecycle unit/integration tests passed, [workflow 40954025988](https://github.com/artinrahimii/project/actions/runs/40954025988).
- A002 removes that extra blank line only. V 3.0.592 validation is pending.

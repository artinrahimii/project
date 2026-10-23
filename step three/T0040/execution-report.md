# T0040 — execution report

## Dependency review
- `T0039 → T0040`: reviewed and classified as `contract_or_interface_dependency`. The new tests validate the byte ceilings and error categories introduced by `ProtocolMessageLimits`, so T0040 depends on that shared policy and its passing C# harness.
- `T0030 → T0040`: reviewed and classified as `sequencing_only`. T0030 supplies the quality-gate and product-build failure propagation but no message-validation code interface; its original `blocks` edge is retained as conservative verification order.
- Both edges have written rationales, file-path evidence and `validation_status=validated`. No unrelated `unreviewed` edge was changed.

## Attempt T0040-A001 — implementation in progress
- Added `src/Shared.Contracts/ProtocolMessageValidator.cs` for semantic checks of required request values/deadlines/tokens, host announcement identity/version/capability/model values, and response enum/required values/sizes.
- Added `src/Host.Windows/ProtocolMessageParser.cs`. It enforces request/control byte limits before Protobuf parsing, translates malformed wire bytes into the canonical InvalidMessage error, and then calls the shared validator. It does not claim to implement a network socket or P2P transport.
- Routed `HostInferenceSession.GenerateAsync(InferenceRequest)` through the semantic validator so incomplete and invalid values are rejected before the model backend is called.
- Added `tests/contracts/invalid-message-cases.json` containing invalid-tag, truncated-varint, and truncated-length-delimited fixtures; tests synthesize an over-limit input to avoid a large fixture blob.
- Extended the C# executable harness for invalid fields, malformed wire data, oversized raw bytes, incompatible/malformed versions, invalid response outcome values and oversized serialized responses. Added Python repository-contract checks to keep validator, parser and fixtures in trace.
- Exact-commit test/build results have not yet been observed. T0040 remains `in_progress` until the actual C# harness and full quality/product CI pass.


## Attempt T0040-A001 — failed verification retained
- Tested commit: `3cc3687c6dc7d68fe51656d383a42f418ef0d5cf` (`V 3.0.120`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39861144156) passed.
- [Windows WPF build and generated C# verification](https://github.com/artinrahimii/project/actions/runs/39861144217) passed; Android Kotlin lint also passed.
- The [C# lifecycle/contract test job](https://github.com/artinrahimii/project/actions/runs/39861144217) failed in the intended incomplete-`HostAnnouncement` rejection case: `NullReferenceException` in `ProtocolMessageParser.ParseHostAnnouncement` because missing capabilities were dereferenced before validation. This is a real acceptance failure and T0040 is not complete on A001.

## Attempt T0040-A002 — null-capabilities correction pending
- Added explicit rejection of a `HostAnnouncement` without `HostCapabilities` immediately after parsing.
- Expected error is `PROTOCOL_ERROR_CODE_INVALID_MESSAGE` without an exception. Exact-commit C# and product CI results are pending.


## Attempt T0040-A002 — successful exact-commit verification
- Tested fix commit: `55c49f1595aa4bc48533fb3bcd2df77fdeb27896` (`V 3.0.121`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39861360027): **success**.
- [Full product build workflow](https://github.com/artinrahimii/project/actions/runs/39861360067): **success**, all three jobs:
  - C# host lifecycle and message-validation harness passed. Its log confirms malformed, incomplete, oversized, incompatible-version and invalid-value messages are rejected, plus all 11 Protobuf round trips/shared fixtures and the size-boundary tests.
  - Windows WPF formatting, app build, generated C# source verification and artifact upload passed.
  - Android lint, debug APK build, Protobuf round-trip unit tests, generated Java/Kotlin checks and APK upload passed.
- The original A001 failure remains recorded: an announcement without `HostCapabilities` caused NullReferenceException. A002 now returns InvalidMessage instead; no crash.
- **Result:** T0040 is `done`.
- Scope boundary: the parser is an executable pre-parse validation boundary, not a P2P transport implementation; sockets, peer discovery and end-to-end peer inference are not claimed.


## Current path reconciliation after T0042
The original attempts retain their historical file paths as written at the time. Current implementation paths are now `src/Host.Infrastructure/ProtocolMessageParser.cs`, `src/Host.Application/HostInferenceSession.cs`, and `src/Host.Domain/HostLifecycle.cs`. The actual C# test harness references product assemblies through project references, and canonical Protobuf generation lives in Shared.Contracts. Exact post-refactor acceptance evidence is recorded in `step three/T0042/evidence/verification.md`.

# T0038 — Shared cross-language Protobuf wire fixtures

## Attempt T0038-A001 — implementation awaiting CI
- Base: `d210a646d705fe5d99273ecc03ec6c695a88762d` (`V 3.0.110`); prerequisites T0037 and T0030 are complete.
- Added one shared Base64 wire payload for each of the 11 messages in `protocol/whitep/v1/messages.proto`.
- C# and Android Kotlin tests parse the same fixture bytes using their generated contract parsers and assert byte-identical re-serialization.
- Python contract checks require exact schema-to-fixture coverage and parser branches in both languages.
- This attempt remains in progress until the actual test and build workflows pass.


## Attempt T0038-A002 — failed verification retained
- Tested exact commit: `20832c67a78a04841f8e6f2ef81969e099603791` (`V 3.0.112`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39851178922) passed, but this was not enough to accept T0038.
- [Product build](https://github.com/artinrahimii/project/actions/runs/39851178925) failed in C# while parsing the shared `HostCapabilities` fixture: `Google.Protobuf.InvalidProtocolBufferException: invalid tag (zero)`. The Windows build job itself passed.
- The Android job failed at ktlint in `ProtoRoundTripTest.kt`; APK building and Kotlin shared-fixture tests were skipped.
- No cross-language fixture acceptance is claimed for A002. The failures are preserved in this report and in the graph history.

## Attempt T0038-A003 — corrective implementation pending CI
- Regenerated all 11 wire fixtures using schema-consistent Protobuf field tags, lengths and varints, with field-number order preserved. The nested `ModelCapability` payload is now well-formed in `HostCapabilities`, `HostAnnouncement` and `HostStatus`.
- Corrected Kotlin imports, the blank line between test declarations, the multiline `when` expression and the trailing blank line identified by ktlint.
- A lightweight local wire-structure validation ran over the generated fixture bytes, rejecting invalid tags, invalid wire types, truncated varints and truncated fixed/length-delimited fields. This check is not a substitute for the actual C#/Kotlin parsers.
- Exact-commit CI is still pending. T0038 remains `in_progress` until C# and Kotlin both parse and reserialize all 11 shared fixtures and the product workflow succeeds.


## Attempt T0038-A003 — failed Kotlin formatting verification retained
- Tested commit: `17cd09a637aedf989bd6337b91cdf4552265c022` (`V 3.0.113`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39858286362) passed.
- [C# host lifecycle/shared-fixture job](https://github.com/artinrahimii/project/actions/runs/39858286367) passed, including the fixture parsing that failed on A002. The Windows WPF build and generated C# source checks also passed.
- The Android job failed only at ktlint: `ProtoRoundTripTest.kt:196:1 File must end with a newline (\\n)`. The APK, Kotlin fixture test and generated Java/Kotlin source assertions were skipped because lint is a gate.
- No T0038 acceptance is claimed for A003 because the Android suite did not run.

## Attempt T0038-A004 — final-newline correction pending CI
- Applied the exact reported correction: the Kotlin test source now ends with one newline.
- Re-running exact-commit CI is required to verify Android lint, APK compilation, Kotlin parsing of all 11 fixtures and source-generation assertions. T0038 remains `in_progress` until that evidence is available.


## Attempt T0038-A004 — exact-commit verification succeeded
- Tested implementation commit: `6d326feaf3ec593695db64e379fae9302738f717` (`V 3.0.114`).
- [Step Three quality gate run 39858575967](https://github.com/artinrahimii/project/actions/runs/39858575967): **success**.
- [White-P product build run 39858575986](https://github.com/artinrahimii/project/actions/runs/39858575986): **success**, all three jobs:
  - C# lifecycle/contract harness: passed; log printed `PASS: C# parsed all 11 shared cross-language wire fixtures.`
  - Android: ktlint passed, debug APK built, `testDebugUnitTest` passed, generated Protobuf Java/Kotlin checks passed, and the APK artifact was uploaded.
  - Windows WPF host formatting/build, generated C# checks and artifact upload passed.
- T0038 is now **done**. The prior A002 fixture parsing failure and A003 missing-newline lint failure remain recorded; neither history entry was overwritten.
- Scope boundary: this task verifies cross-language parsing/re-serialization of shared Protobuf wire bytes. It does not claim that live P2P transport or end-to-end host-to-phone inference exists.

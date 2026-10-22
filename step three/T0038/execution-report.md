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

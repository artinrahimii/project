# T0038 — Shared cross-language Protobuf wire fixtures

## Attempt T0038-A001 — implementation awaiting CI
- Base: `d210a646d705fe5d99273ecc03ec6c695a88762d` (`V 3.0.110`); prerequisites T0037 and T0030 are complete.
- Added one shared Base64 wire payload for each of the 11 messages in `protocol/whitep/v1/messages.proto`.
- C# and Android Kotlin tests parse the same fixture bytes using their generated contract parsers and assert byte-identical re-serialization.
- Python contract checks require exact schema-to-fixture coverage and parser branches in both languages.
- This attempt remains in progress until the actual test and build workflows pass.

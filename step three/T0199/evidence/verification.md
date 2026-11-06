# T0199 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- Quality Gate run 41886746371: Ran 403 Python tests; result OK.
- tests/test_proto_contract.py checks canonical schema fields, C# generation, Android Java/Kotlin generation, round-trip suites in both languages and the shared protobuf-wire-fixtures.txt.
- PASS: C# Protobuf serialize/deserialize round-trip for all 11 primary messages.
- PASS: C# parsed all 11 shared cross-language wire fixtures.
- Android ProtoRoundTripTest uses the same shared fixtures and testDebugUnitTest passed.
- Product Build verified generated Protobuf C# sources and generated Protobuf Java/Kotlin sources.

## Source/test paths

- `protocol/whitep/v1/messages.proto`
- `tests/test_proto_contract.py`
- `tests/contracts/protobuf-wire-fixtures.txt`
- `tests/HostLifecycle.Tests/Program.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt`
- `.github/workflows/quality.yml`
- `.github/workflows/product-build.yml`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886746371
- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137103
- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137271

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.

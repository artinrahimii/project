# T0037 — Execution report

## Dependency review
- `T0036 → T0037`: validated as `contract_or_interface_dependency`. Round-trip suites instantiate/parse compiler-generated message classes and require the C# and Kotlin bindings established by T0036. Original `blocks` edge retained.
- `T0030 → T0037`: validated as `sequencing_only`. T0030 is about CI verification and failure/artifact propagation, not a code interface used by the round-trip tests. Original `blocks` edge retained.

## Attempt T0037-A001 — implementation in progress
- Added a .NET test-project Protobuf generator reference to the single canonical `protocol/whitep/v1/messages.proto`, enabling the runnable C# harness to compile the actual generated message types.
- Added C# serialize/parse/re-serialize assertions for all 11 schema message types, covering scalar fields, enums, nested messages, repeated model capabilities, bytes, Unicode prompt text, and stable wire bytes.
- Added Android JUnit round-trip tests for the same 11 message types using the generated Kotlin DSL and Java Lite parsers.
- Added the JUnit dependency and a CI step to run `gradle --no-daemon testDebugUnitTest`.
- Added a source-contract test that dynamically extracts the schema's message names and checks that both executable suites cover each name.
- The exact-commit Python quality gate, .NET harness, and Android unit tests are pending; task remains in progress until CI confirms all branches pass.

## Verification
- `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release`: pending exact-commit CI.
- `gradle --no-daemon testDebugUnitTest`: pending exact-commit CI.
- `python -m unittest discover -s tests -v`: pending exact-commit CI.

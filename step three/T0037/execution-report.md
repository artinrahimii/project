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


## Attempt T0037-A001 — failed verification retained
- Tested commit: `04b1fcb9d0656c0995f92ec5b6cb613b81979047` (`V 3.0.107`).
- [Quality gate run 39844383014](https://github.com/artinrahimii/project/actions/runs/39844383014): **failure**. 79 Python tests ran with one failure because the schema-name regex was double-escaped and matched zero declarations.
- [Product build run 39844383044](https://github.com/artinrahimii/project/actions/runs/39844383044): C# failed with CS0104 because generated Protobuf `HostStatus` conflicted with `WhiteP.Host.Windows.HostStatus`; Android ktlint rejected multiline builder-lambda initializers, so APK and JUnit steps were skipped.
- No round-trip acceptance is claimed for A001.

## Attempt T0037-A002 — corrective implementation in progress
- Fixed schema enumeration to use the correct single regex escapes.
- Added a `ProtoHostStatus` alias to disambiguate the generated message type from the host state DTO.
- Reformatted all 11 Kotlin DSL builder initializers so multiline expressions begin on the required next line.
- Normalized the preserved successful T0036-A001 history record to a one-element array, recalculated count fields and retained the V 3.0.106 graph-guard failure in the T0036 report. No past attempt was removed.
- Exact-commit Python, graph guard, .NET round-trip, Android lint/APK and JUnit results remain pending.


## Attempt T0037-A002 — failed Android formatting check retained
- Tested commit `dd87b8e3edc56c40a7383dea340e47adc44e5b8d` (`V 3.0.108`).
- [Quality run 39844860940](https://github.com/artinrahimii/project/actions/runs/39844860940): **success**. All 79 Python tests, syntax, graph structure, and repository output/evidence validation passed. The graph guard reports only one pre-existing warning: the T0001 dependency metadata string is not a task ID.
- [Product run 39844860924](https://github.com/artinrahimii/project/actions/runs/39844860924): the .NET harness **passed**, including round-trip encode/decode/re-encode for all 11 primary Protobuf messages. Android ktlint failed because the DSL block contents and closing braces weren't indented at the new nesting level; APK and Kotlin unit-test steps were skipped.
- No cross-language acceptance is claimed for A002 because Kotlin JUnit did not run.

## Attempt T0037-A003 — Kotlin formatting correction in progress
- Indented each of the 11 generated Kotlin DSL builder bodies to the correct nesting level and aligned the closing brace with the builder invocation.
- Retains the successful Python/graph and C# evidence from A002; A002's Android formatting failure stays in the history.
- Awaiting exact-commit lint, Android APK compilation, Kotlin JUnit round-trip tests, generated-source assertions, and full product build.

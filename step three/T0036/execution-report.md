# T0036 — Execution report

## Dependency review
- `T0035 → T0036`: validated as `sequencing_only`. T0036 consumes the canonical `.proto` schema directly; the version policy helper from T0035 is a separate .NET type and is not consumed by code generation.
- `T0030 → T0036`: validated as `sequencing_only`. T0030 checks CI behavior/artifact failure propagation; these checks support verification but do not define a contract interface consumed by T0036. Both original `blocks` edges are preserved.

## Attempt T0036-A001 — implementation in progress
- Preserved the single source of truth at `protocol/whitep/v1/messages.proto`.
- Android Gradle now requests both Java Lite and Kotlin Protobuf builtins, and adds the matching `protobuf-kotlin-lite:4.36.2` runtime.
- Registered variant-scoped Kotlin generated-source directories for debug and release to ensure generated `.kt` files participate in Kotlin compilation.
- Added `ProtocolMessages.kt` which imports the generated `HostAnnouncement` type and `hostAnnouncement` Kotlin DSL, creates a message, and uses Protobuf serialization/parsing. This source is useful contract code and does not claim to implement transport.
- Added CI checks that fail unless the Windows build outputs generated Protobuf C# and Android build outputs both generated Java and Kotlin files.
- Extended source-contract tests to require the Kotlin generator/runtime, the generated Kotlin helper, and CI source-artifact checks.
- Official implementation references reviewed: [Kotlin Generated Code Guide](https://protobuf.dev/reference/kotlin/kotlin-generated/) specifies both Java and Kotlin compiler outputs; [Protobuf Gradle plugin README](https://github.com/google/protobuf-gradle-plugin) documents builtin configuration and output locations; [protobuf-kotlin-lite 4.36.2](https://central.sonatype.com/artifact/com.google.protobuf/protobuf-kotlin-lite) is available from Maven Central.
- The exact-commit quality gate and full product build are pending. Do not mark this task complete until CI confirms that generated Kotlin DSL sources exist and compile in the Android APK.

## Verification
- `python -m unittest discover -s tests -v`: pending exact-commit CI.
- `dotnet build WhiteP.sln --configuration Release` plus generated C# file check: pending exact-commit CI.
- `gradle --no-daemon assembleDebug`, generated Java/Kotlin source checks, lint, and artifact upload: pending exact-commit CI.


## Attempt T0036-A001 — exact-commit verification and completion
- Tested implementation commit: `0052b09a36b26a16286dece3b004eb6675f03807` (`V 3.0.105`).
- Step Three quality gate [run 39843477170](https://github.com/artinrahimii/project/actions/runs/39843477170): **success**. All 78 Python tests, Python syntax compilation, graph structure, and repository output/evidence validation passed.
- Product build [run 39843477175](https://github.com/artinrahimii/project/actions/runs/39843477175): **success**, with all three jobs completed:
  - Windows WPF formatting and build passed; the added check found a compiler-generated Protobuf C# source under the host `obj/Release` output; required artifact uploaded.
  - Android Kotlin lint passed; debug APK compiled the new `ProtocolMessages.kt` against generated Protobuf Kotlin DSL; the job verified that both generated Java and Kotlin source files existed; APK artifact uploaded.
  - .NET host lifecycle/contract harness passed.
- The Kotlin code generator and `protobuf-kotlin-lite:4.36.2` runtime are configured alongside Java Lite, all from the existing canonical `protocol/whitep/v1/messages.proto`. Generated code remains in build outputs, not hand-edited or checked in.
- Result: T0036 is complete for build-time C# and Kotlin binding generation and compilation. This does not claim live P2P transport, discovery, or end-to-end cross-language network tests; those remain separate work.


## Graph metadata correction (follow-up V 3.0.108)
- V 3.0.106 quality run [39844000042](https://github.com/artinrahimii/project/actions/runs/39844000042) failed graph guard because T0036 `execution_history` was serialized as a single object, so the validator saw zero successful entries.
- The original successful T0036-A001 record from `0052b09a36b26a16286dece3b004eb6675f03807` is preserved and wrapped as an array. No prior execution evidence was discarded.

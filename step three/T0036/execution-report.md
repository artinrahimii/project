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

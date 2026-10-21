# T0017 — execution report

## Attempt T0017-A001 — shared Protobuf wire contract

- Started: 2026-10-21T21:26:48.431Z.
- Base commit: `183b7767867ab3ff66f5ac8b91a291c1483db0fe` (`V 3.0.49`).
- Result: partial; source schema and build-time code generation are included in this attempt, but acceptance remains pending exact-commit Windows and Android builds and the quality gate.
- Dependency review:
  - `T0016 → T0017`: validated as `hard_dependency`. The Android Gradle module is the compile target for generated Java-lite classes; it is a real integration prerequisite.
  - `T0010 → T0017`: validated as `sequencing_only`. T0017 does not consume a build artifact unique to T0010. Its workflow remains a verification mechanism, and T0010 is now marked done based on its own recorded CI/artifact evidence.
- Implementation: added one versioned proto3 source at `protocol/whitep/v1/messages.proto` with capability/status, inference request/response, cancellation, and protocol-error messages. The Windows project and Android app are configured to generate C#/Java-lite classes at build time from this shared source; generated code is not hand-maintained.
- Changed files: `protocol/whitep/v1/messages.proto`, `src/Host.Windows/WhiteP.Host.Windows.csproj`, `src/Client.Android/build.gradle.kts`, `src/Client.Android/app/build.gradle.kts`, `tests/test_proto_contract.py`, `README.md`, `docs/acceptance.md`, `step three/T0017/execution-report.md`, `step three/project-graph.md`.
- Tests: no local test execution is available in this tool session. The new contract test is wired into the existing quality workflow; real generated-code compilation must be verified by the exact commit's Windows and Android product-build jobs. No pass is claimed yet.
- Scope limit: this schema and code-generation setup do not implement transport, direct P2P, real local inference, encryption, or no-persistent-history behavior.
- Follow-up: inspect exact-commit quality and product-build results, correct generation/compiler failures if any, and only mark T0017 done once the quality gate and both platform builds pass.

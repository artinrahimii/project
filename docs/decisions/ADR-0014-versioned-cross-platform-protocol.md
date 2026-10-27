# ADR-0014: Versioned cross-platform Protobuf wire contract

- **Status:** Accepted for the shared schema, code generation, wire fixtures and round-trip verification boundary.
- **Date:** 2026-10-27
- **Related task:** T0007 (technical-decision traceability).
- **Related requirement:** REQ-013 (versioned cross-platform protocol).
- **Scope:** The canonical Protobuf wire contract shared by the Windows host and Android client.

## Context

The Windows and Android projects need to interpret the same versioned wire messages. Maintaining parallel handwritten schemas or validating only one language binding could allow fields, enum values, or wire encodings to drift between the clients.

## Decision

1. **Keep one canonical source schema.** `protocol/whitep/v1/messages.proto` is the repository's sole source `.proto` schema. The C# contracts project compiles it from that path; Android's Protobuf Gradle plugin generates Java Lite and Kotlin Lite sources from the same repository protocol directory.
2. **Keep protocol compatibility explicit.** The current request version is `1.0.0`. `ProtocolVersionPolicy` evaluates supported versions and maps malformed or unsupported versions to the protocol's typed error codes. A version mismatch must not be silently treated as compatible.
3. **Verify wire behavior in both language bindings.** The C# and Kotlin round-trip suites serialize and parse every primary message and verify that re-serialization preserves the wire bytes. Shared fixtures cover the same message families in both languages, so the assertions compare canonical wire representations rather than only source-code names.
4. **Verify generated artifacts in product CI.** Windows CI checks generated C# Protobuf sources; Android CI checks generated Java and Kotlin Protobuf sources, runs the round-trip suite, and builds the APK. These generation checks protect the build inputs and generated output paths.
5. **Separate wire compatibility from network delivery.** The shared schema, version policy, fixtures and round-trip tests do not prove the Android app can establish a peer connection to the Windows host or route a request end to end. The existing TCP framing decision retains its host-loopback scope, and `AC-P2P-01` remains a separate runtime acceptance gate.

## Implementation and verification links

- Requirement: `docs/requirements.md` (REQ-013).
- Canonical schema: `protocol/whitep/v1/messages.proto`.
- Windows generation: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` and `src/Shared.Contracts/ProtocolVersionPolicy.cs`.
- Android generation: `src/Client.Android/app/build.gradle.kts`.
- C# round-trip coverage: `tests/HostLifecycle.Tests/Program.cs`.
- Kotlin round-trip coverage: `src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt`.
- Shared encoded-wire fixtures: `tests/contracts/protobuf-wire-fixtures.txt`.
- Contract/schema guards: `tests/test_proto_contract.py` and `tests/contracts/test_contract_layout.py`.
- Decision/source traceability: `docs/decisions/README.md` and `tests/test_decision_traceability.py`.
- Product CI generation and language test steps: `.github/workflows/product-build.yml`.

## Evidence boundary

The checks demonstrate schema shape, generated binding presence, typed round-trip preservation, and version-policy source wiring for the selected messages. They do not prove physical-device interoperability, a live Android-to-Windows route, cryptographic peer authentication, or successful end-to-end inference. `AC-P2P-01` remains **NOT RUN**.

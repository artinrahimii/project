# Shared contracts project boundary

This directory contains the buildable `WhiteP.Shared.Contracts.csproj` library targeting `net10.0`, included in the root `WhiteP.sln` alongside the Windows WPF host. It is the platform-neutral .NET contract boundary.

- The canonical wire schema remains `protocol/whitep/v1/messages.proto`. Do not copy the schema into this directory or hand-maintain a second version.
- The Windows host and Android client are separate runnable projects under `src/Host.Windows/` and `src/Client.Android/`.
- The library project, host project reference, and root solution are delivered by task T0022. Generated contract code and cross-platform code-generation wiring are owned by the corresponding generation task (T0036).
- This file documents the project boundary; it is not a substitute for generated classes or cross-language compatibility tests.

The existing schema and source contract checks remain under `protocol/whitep/v1/messages.proto` and `tests/test_proto_contract.py` until the graph's later generation/build tasks integrate the shared project.

## Protocol-version policy

`ProtocolVersionPolicy.CurrentVersion` defines the initial protocol release as `1.0.0`. It accepts only an exact canonical numeric `MAJOR.MINOR.PATCH` match. Missing or malformed values are rejected with the wire error name `PROTOCOL_ERROR_CODE_INVALID_MESSAGE`; a valid but different version is rejected with `PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION`.

Exact-match checking is intentionally conservative: this project has not yet defined minor-version negotiation or backward-compatibility guarantees. The policy is tested against the shared .NET library from the runnable contract/lifecycle harness. It is not evidence that a network handshake already calls the policy; live peer discovery and transport are later implementation tasks.

## Generated Kotlin bindings

Android builds generate both Java Lite message classes and Kotlin DSL APIs from `protocol/whitep/v1/messages.proto`. The Kotlin generated source roots are registered for the Android debug and release variants, and the Android workflow verifies the generated `.java` and `.kt` files after compilation. `ProtocolMessages.kt` imports the generated `HostAnnouncement` type and builder DSL so an APK build fails if Kotlin generation or source registration is broken.

Generated sources stay under the build directory; they are not hand-edited or checked into the repository. This code provides serialization helpers only. It does not implement network discovery or peer-to-peer transport.

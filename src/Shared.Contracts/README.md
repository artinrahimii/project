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

## Initial protocol size limits

`ProtocolMessageLimits` is the platform-neutral source of the first protocol ceilings:

- Serialized `InferenceRequest`: 16 KiB maximum.
- Prompt text: 8 KiB maximum measured after UTF-8 encoding.
- Context capsule: 4 KiB maximum.
- Generated response text: 1 MiB maximum measured after UTF-8 encoding.
- Serialized `InferenceResponse`: 2 MiB maximum.
- Local llama.cpp JSON request body: 64 KiB maximum; local JSON response body: 2 MiB maximum.

The host checks prompt and response text before returning a result, checks typed inference requests before calling the backend, and bounds the local HTTP response stream before deserialization. Size failures map to `PROTOCOL_ERROR_CODE_RESOURCE_LIMIT`; negative byte lengths map to `PROTOCOL_ERROR_CODE_INVALID_MESSAGE`. These checks do not mean a P2P frame parser exists yet: when that transport is implemented, it must check raw frame lengths before Protobuf parsing and apply the lower of the local and negotiated capsule limits.

## Typed-message validation and safe parsing

`ProtocolMessageValidator` checks required inference identifiers, prompt/model values, deadlines, output-token values, advertised host capabilities, exact protocol-version compatibility, response outcomes and shared byte ceilings. `HostInferenceSession.GenerateAsync(InferenceRequest)` runs this validator before calling the local inference backend.

`ProtocolMessageParser` is the Windows-side boundary for inbound Protobuf bytes: it rejects oversized inference-request and host-control messages before invoking generated Protobuf parsers, translates malformed wire bytes to `PROTOCOL_ERROR_CODE_INVALID_MESSAGE`, and rejects parsed messages whose required values or protocol version are invalid. `tests/contracts/invalid-message-cases.json` holds invalid-tag, truncated-varint and truncated-length-delimited regression fixtures.

This is a testable parser/validation boundary that a future peer transport can call; it does not create a socket, discover peers, or implement P2P transport.

## Generated Kotlin bindings

Android builds generate both Java Lite message classes and Kotlin DSL APIs from `protocol/whitep/v1/messages.proto`. The Kotlin generated source roots are registered for the Android debug and release variants, and the Android workflow verifies the generated `.java` and `.kt` files after compilation. `ProtocolMessages.kt` imports the generated `HostAnnouncement` type and builder DSL so an APK build fails if Kotlin generation or source registration is broken.

Generated sources stay under the build directory; they are not hand-edited or checked into the repository. This code provides serialization helpers only. It does not implement network discovery or peer-to-peer transport.

## Protobuf round-trip verification

The C# harness compiles the canonical `messages.proto` through `Grpc.Tools` and round-trips each of its 11 primary message types through the generated C# parser. Android unit tests do the same with the generated Kotlin DSL and Java Lite message parsers. Each test compares both the parsed message and the re-serialized bytes. The two suites run in CI; they verify per-language serialization integrity, not a live network connection or peer-to-peer transport.

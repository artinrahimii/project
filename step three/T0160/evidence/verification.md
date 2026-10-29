# T0160 verification evidence

- Base revision: `6691befd976f87ecdca75767e1cda632168a8f57` (`V 3.0.593`), branch `main`.
- Protocol schema: `protocol/whitep/v1/messages.proto`, `InferenceRequest` contains `request_id`, `attempt_id`, `model_id`, `prompt`, `deadline_unix_ms`, `max_output_tokens`, `context_capsule`, and `protocol_version`; it has no history field or permanent conversation identifier.
- New production output: `src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt`.
- New JVM coverage: `src/Client.Android/app/src/test/java/com/whitep/client/protocol/InferenceRequestPayloadBuilderTest.kt`.
- The positive test checks the selected prompt is unchanged, capsule bytes match the local codec exactly, Protobuf parse/serialize round-trip preserves the request, the schema has no history/permanent-conversation field, and serialized size stays at or below 16 KiB.
- Negative tests cover a prompt over 8,192 UTF-8 bytes, invalid capsule data, and a serialized request exceeding 16 KiB.
- Explicit limit: no live network transport or peer send/receive is implemented by this task; this is payload construction and protocol-boundary verification only.
- Exact-head V 3.0.594 CI: pending; no local Gradle result is claimed.

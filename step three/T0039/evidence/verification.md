# T0039 verification evidence

## Acceptance policy and selected ceilings

- Serialized inference request: 16,384 bytes.
- Prompt: 8,192 UTF-8 bytes.
- Context capsule: 4,096 bytes.
- Output tokens: 131,072 maximum (consistent with the existing local backend ceiling).
- Generated response text: 1,048,576 UTF-8 bytes.
- Serialized inference response: 2,097,152 bytes.
- Current local llama.cpp JSON request body: 65,536 bytes; response body: 2,097,152 bytes.

## Implementation evidence

Files changed in the current attempt:
- `src/Shared.Contracts/ProtocolMessageLimits.cs`
- `src/Host.Windows/HostLifecycle.cs`
- `src/Host.Windows/LlamaCppServerBackend.cs`
- `protocol/whitep/v1/messages.proto`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_protocol_limits.py`
- `docs/requirements.md`
- `src/Shared.Contracts/README.md`

The policy validates typed request/response lengths and the live local HTTP adapter enforces prompt, outgoing JSON body, response stream and generated text ceilings. There is no peer-to-peer frame parser yet; future transport work must apply the raw frame limit before parsing the Protobuf message and compare against negotiated capsule limits. Exact-commit test outcomes have not yet been observed and will be appended after CI.

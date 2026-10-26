# ADR-0003 — Initial TCP request framing for the host listener

- **Status:** Accepted for the initial Windows host listener boundary; interoperability with the future Android peer transport must be verified before claiming P2P compatibility.
- **Date:** 2026-10-27
- **Related task:** T0110
- **Contract source:** `protocol/whitep/v1/messages.proto` and `src/Shared.Contracts/ProtocolMessageLimits.cs`

## Decision

1. The current Windows listener accepts exactly one inference request frame per TCP connection.
2. The frame begins with a 4-byte unsigned big-endian payload length followed by exactly that many serialized Protobuf `InferenceRequest` bytes.
3. Zero length, truncated prefix/payload, malformed Protobuf, semantically invalid requests, and payload length above `ProtocolMessageLimits.MaximumRequestMessageBytes` are rejected and the connection is closed.
4. Raw length is checked before allocating the payload buffer. Incomplete frame reads have a short deadline and are cancelled when participation is disabled or the listener shuts down.
5. Payloads are not logged. A valid typed request reaches an optional in-process callback; with no callback, the connection closes after validation. This does not yet invoke inference or return a network response.
6. The later Android/peer transport task must implement this framing or record a reviewed contract revision before claiming interoperability.

## Verification boundary

HostLifecycle acceptance tests send framed Protobuf bytes over real loopback TCP, confirm the typed request reaches the shared parser, and check fail-closed handling for malformed/oversized/truncated frames and shutdown while a peer waits for a frame.

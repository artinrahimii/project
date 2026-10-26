# ADR-0003 — Initial TCP request framing for the host listener

- **Status:** Accepted for the initial Windows host listener boundary; interoperability with the future Android peer transport must be verified before claiming P2P compatibility.
- **Date:** 2026-10-27
- **Related task:** T0110
- **Contract source:** `protocol/whitep/v1/messages.proto` and `src/Shared.Contracts/ProtocolMessageLimits.cs`

## Decision

1. The current Windows listener accepts exactly one inference request frame per TCP connection.
2. The frame begins with a 4-byte unsigned big-endian payload length followed by exactly that many serialized Protobuf `InferenceRequest` bytes.
3. Zero length, truncated prefix/payload, malformed Protobuf, unsupported/malformed request protocol versions, unsafe request/attempt identifiers, missing/invalid/expired deadlines, invalid semantic fields, and payload length above `ProtocolMessageLimits.MaximumRequestMessageBytes` are rejected and the connection is closed.
4. Every `InferenceRequest` carries `protocol_version = 1.0.0` in field 8. The parser validates the supported version and uses the existing typed error codes for unsupported versions, malformed messages, and expired deadlines.
5. Request and attempt identifiers must be ASCII alphanumeric strings with optional `_`, `-`, `.`, or `:` after the first character, up to 128 characters. The absolute Unix-millisecond deadline must be valid and in the future at parsing time.
6. Raw length is checked before allocating the payload buffer. Incomplete frame reads have a short deadline and are cancelled when participation is disabled or the listener shuts down. Participation is rechecked after parsing and before calling the request handler, so a request is dropped if local participation was revoked during the read.
7. Payloads are not logged. A valid typed request reaches an optional in-process callback; with no callback, the connection closes after validation. This does not yet invoke inference or return a network response.
8. The participation check is a local host-side gate, not cryptographic peer authentication or proof of sender identity. The later Android/peer transport task must implement the same framing and compatible validation or record a reviewed contract revision before claiming interoperability.

## Verification boundary

HostLifecycle acceptance tests send framed Protobuf bytes over real loopback TCP, confirm the typed request reaches the shared parser, and check fail-closed handling for malformed/oversized/truncated frames and shutdown while a peer waits for a frame.


## Response failure contract mapping

`NetworkInferenceRequestResponseMapper` maps a busy or not-ready host to the `HostBusy` message, a requested/ready-model mismatch to `UnsupportedModel`, validation failures to `ProtocolError` with the specific invalid-message, unsupported-version, request-expired or resource-limit code, and unexpected processing failures to a sanitized `INTERNAL` error. The mapping returns typed Protobuf messages only; it does not claim that a response frame has been serialized or delivered over TCP. Inactive participation and unavailable model state fail closed as busy; insufficient/unknown capacity fails closed as a resource-limit error.

# T0110 verification evidence

## Exact source snapshot and graph validation

- Repository `artinrahimii/project`, branch `main`, baseline `2d697e89875db836953721945103295f77f76408` (`V 3.0.336`).
- Tree `df69c831b4fb9c38baec886a69a3e46ed8c9fcb9`, not truncated; 743 entries.
- Graph path `step three/project-graph.md`; Blob ID `355f2972c2e9ad3acd4c022644440d55feb17748` from the baseline tree.
- Original Git Blob API content: **1,718,063 UTF-8 bytes**, **1,596,320 Unicode characters**. Recomputed Git blob SHA-1 matches; the original fenced Markdown was hashed unchanged and the internal JSON was parsed separately. Graph count: 229 tasks, 426 edges, 22 sections.
- T0107/T0108/T0109 are complete in the baseline. Product build on V336 passed: https://github.com/artinrahimii/project/actions/runs/40358791619.

## Parser contract

- Shared wire schema: `protocol/whitep/v1/messages.proto`.
- Existing raw-byte validator/parser: `src/Host.Infrastructure/ProtocolMessageParser.cs`.
- New framing/reader: `src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs`.
- Four-byte unsigned big-endian payload length; one Protobuf InferenceRequest per TCP connection; max payload **16,384 bytes**, checked before allocation.
- Invalid/truncated/oversized frames are closed without logging payloads. A valid request reaches an optional callback; dispatch/inference response is not implemented by this task.
- Future peer transport must implement the same framing or record a reviewed contract change.

## Edge review

- `T0109 -> T0110`: `contract_or_interface_dependency / validated`; stop/dispose behavior is explicitly exercised while a peer waits for a frame.
- `T0107 -> T0110`: `sequencing_only / validated`; T0110 consumes no coordinator output.
- `T0110 -> T0111` remains unreviewed.

## Tests added; CI pending

`NetworkInferenceRequestFrameAcceptanceTests` uses real loopback TCP for valid Protobuf delivery, zero/oversized length, malformed protobuf, truncated header/body and stop while a client is waiting for a frame. The prior T0108 test now sends a zero-length frame and asserts fail-closed rejection. Exact-head HostLifecycle, Step Three quality gate and full product-build outcomes are pending at commit preparation.

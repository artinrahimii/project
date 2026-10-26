# T0110 execution report

- **Task:** Connect the TCP network input parser to the shared Protobuf contract.
- **Attempt:** `T0110-2026-10-27-01`.
- **Baseline:** `main` at `2d697e89875db836953721945103295f77f76408` (`V 3.0.336`).
- **Status:** In progress pending exact-head CI.
- **Graph integrity:** Graph Blob `355f2972c2e9ad3acd4c022644440d55feb17748` was retrieved from this exact baseline. Original Markdown: **1,718,063 UTF-8 bytes** and **1,596,320 Unicode characters**; recomputed Git Blob SHA-1 matches. Complete fenced JSON parses to 229 tasks / 426 edges.

## Dependency review

- `T0109 -> T0110`: `type=blocks` retained; `contract_or_interface_dependency / validated`. Stop/dispose must release active frame readers; tests stop a listener while a client waits for a frame.
- `T0107 -> T0110`: `type=blocks` retained; `sequencing_only / validated`. T0110 uses the existing shared parser, not a T0107-specific coordinator API.
- `T0110 -> T0111` remains `unreviewed`.

## Implementation

- Added `src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs`: four-byte unsigned big-endian length; zero/oversized lengths are rejected before allocation; declared bytes are read exactly and passed to `ProtocolMessageParser.ParseInferenceRequest`.
- Integrated the frame reader with `ParticipationGatedNetworkListener`. One frame per connection, a five-second read deadline, active-client/task tracking, and awaited shutdown are implemented. A valid typed request reaches an optional in-process callback; without a callback it closes after validation. No inference response is claimed.
- Updated the prior T0108 listener test and added live-loopback tests for valid Protobuf delivery, empty/oversized lengths, malformed wire, truncated prefix/payload, and stop during a pending read.
- Added `docs/decisions/ADR-0003-tcp-request-framing.md` documenting the provisional host framing and future Android interoperability requirement.

## Acceptance boundary

T0108/T0109 are temporarily `in_progress` for contract revalidation. Local .NET execution is not available. Exact-head HostLifecycle, Step Three quality gate and full Windows/Android product build are pending; do not mark tasks complete before those CI results pass.

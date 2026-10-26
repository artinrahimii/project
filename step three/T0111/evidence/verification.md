# T0111 verification evidence — pending exact-head CI

## Baseline and source integrity
- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `50e2d0605b1c0c41090f39c0673c0bebe373b382`; tree: `65e2777fdd4d6006d7d7128c3b98ccd9c83727d9`.
- Graph path: `step three/project-graph.md`; tree Blob: `0fc67b4190acfd21bbda9fe4ffb208c5a6b0f757`, size 1,737,836 bytes.
- Git Blob API used because `fetch_file` returned empty content. The complete original payload was parsed only after removing the Markdown code fence for JSON parsing. The Git object SHA-1 recomputed over all original UTF-8 bytes matched `0fc67b4190acfd21bbda9fe4ffb208c5a6b0f757`; the graph contains 229 tasks and 426 edges.

## Code and contract checks
- `protocol/whitep/v1/messages.proto`: adds request protocol version as field 8; fields 1–7 are unchanged.
- `src/Shared.Contracts/ProtocolMessageValidator.cs`: checks identifier alphabet/length, exact supported request version, deadline validity and expiration; expired requests use existing `REQUEST_EXPIRED`.
- `src/Host.Infrastructure/ProtocolMessageParser.cs`: validates typed request data before dispatch and supports a deterministic test clock.
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`: rechecks local participation after parse and before callback dispatch.
- `tests/HostLifecycle.Tests/Program.cs`: negative coverage for incompatible/malformed versions, unsafe request IDs, expired deadlines and existing byte/value limits.
- `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`: loopback TCP acceptance test verifies dispatch is suppressed if participation is revoked.
- `src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt`: request round-trip fixture populates the new version field.
- Static contract tests and framing ADR were updated.

## Dependency evidence
- `T0110 → T0111`: parser/frame reader and tests show a direct API/data-contract dependency; classification `contract_or_interface_dependency`, validated.
- `T0107 → T0111`: admission/coordinator acceptance is separate from transport parsing; classification `sequencing_only`, validated; `blocks` remains.
- No outgoing or unrelated unreviewed edge was changed.

## Verification status
- GitHub Actions results for the implementation commit: **pending**.
- File existence, static source assertions and successful commit creation are not treated as acceptance evidence.
- Peer identity/authentication is not implemented or claimed; only the host-side participation gate is rechecked before dispatch.

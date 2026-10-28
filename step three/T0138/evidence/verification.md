# T0138 — Verification evidence

## Exact base and graph integrity
- Branch: `main`; base commit: `dd0b9988c95901d1c391d2f6c362d3b4531747b4` (`V 3.0.481`); tree: `df76c18f7cae546aa45b21b5d1899154ffe4025c`.
- Graph path: `step three/project-graph.md`; Blob: `6555d850f9f74ff55cff4b22d59550b674ca1f67`.
- Original graph: **2,332,362 UTF-8 bytes**, **2,210,346 characters**; Git Blob SHA-1 matches the tree entry. Full fenced JSON parsed successfully to 229 tasks and 418 active edges.

## Source evidence for the endpoint boundary
- `protocol/whitep/v1/messages.proto` declares `InferenceRequest.prompt` and `InferenceRequest.context_capsule`.
- `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs` documents that encryption/authentication are provided by the libp2p secure transport, not by the framing protocol.
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs` parses and validates the inbound request before dispatching the wire message to the handler.
- `src/Host.Windows/MainWindow.xaml.cs` parses the request and passes the request object to `_session.GenerateAsync(request, token)`.
- These are source/schema facts; they do not prove end-to-end Android-to-Windows routing or no-storage behavior.

## Dependency-edge decision
- `T0137 → T0138` stays validated `sequencing_only`.
- `T0128 → T0138` was `unreviewed`. It is now validated as `sequencing_only` and removed from active dependencies because an accurate endpoint-readability disclosure does not depend on external-node reachability; the text explicitly maintains the independent-node gate as NOT RUN. The removal and evidence are recorded in `edge_change_log`.

## Current test status
- New source-contract test: pending exact-head CI.
- Python repository suite/graph_guard: pending exact-head CI.
- Product regression: pending exact-head CI.
- No claim of successful acceptance until the workflow results are inspected.

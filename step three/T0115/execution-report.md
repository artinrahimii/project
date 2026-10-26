# T0115 execution report — accepted

- **Task:** add acceptance tests for a disconnected connection, malicious request, repeated connections and shutdown.
- **Implementation commit:** `V 3.0.347` — [`cb3da47b85050450da7957bf46ff2bb5c842bb46`](https://github.com/artinrahimii/project/commit/cb3da47b85050450da7957bf46ff2bb5c842bb46).
- **Acceptance tests added** in `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`:
  - `DisconnectedClientDoesNotPreventSubsequentValidRequestAsync`: closes a loopback client before it sends a frame, waits for active connection cleanup, then verifies a later valid request is accepted.
  - `SemanticallyInvalidRequestIsRejectedAsync`: sends a length-framed Protobuf message with an unsafe request ID and verifies the callback is not invoked.
  - `SequentialConnectionsAreHandledIndependentlyAsync`: verifies two independent, sequential valid connections each reach the callback once and leave the listener active.
- **Existing coverage retained:** malformed/oversized payloads, truncated length prefix and payload, participation revoked during request handling, no prompt text in ordinary console output, and shutdown while a client is waiting for a frame.
- **Exact-SHA verification on V 3.0.347:**
  - HostLifecycle suite — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844412
  - Step Three quality/graph/evidence gate — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341972
  - Windows WPF Release build/startup smoke and host policy checks — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844535
  - Android lint, APK, Protobuf round-trip, generated-source checks and artifact upload — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844191
  - Real local llama.cpp/GGUF inference integration — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417342040
- **Dependencies:** `T0114 → T0115` and `T0107 → T0115` were reviewed as `sequencing_only`; the original `blocks` edge types were preserved with evidence and rationale.
- **Result:** the new tests and all existing lifecycle acceptance tests passed on the exact implementation SHA. `T0115` is marked `done`.
- **Next:** review `T0116` dependency edges and its transport-interface acceptance criteria before implementation.
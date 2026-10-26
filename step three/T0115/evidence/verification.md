# T0115 verification evidence — accepted

## Baseline and graph integrity
- Repository: `artinrahimii/project`, branch `main`.
- Implementation commit: `cb3da47b85050450da7957bf46ff2bb5c842bb46` (`V 3.0.347`).
- Graph path: `step three/project-graph.md`; Blob at the implementation SHA: `3c4f2d1c9407e3a520fe27365efa0e5381a5bb6e`, size 1,788,621 bytes. Full original UTF-8 content was read via Git Blob API, SHA-1 was recomputed over the original bytes in Git blob format and matched, and the complete fenced JSON parsed to 229 tasks and 426 edges.

## New runtime acceptance
- `DisconnectedClientDoesNotPreventSubsequentValidRequestAsync`: real TCP peer disconnects before sending a frame; listener cleans up the active client slot and serves a later valid frame.
- `SemanticallyInvalidRequestIsRejectedAsync`: a syntactically framed Protobuf request with unsafe request ID is closed before callback dispatch.
- `SequentialConnectionsAreHandledIndependentlyAsync`: two valid one-frame TCP connections are handled once each while the listener remains active.
- All were registered in the executable `NetworkInferenceRequestFrameAcceptanceTests.RunAsync`; a throw/failure would fail the HostLifecycle job.

## Exact-SHA CI results
- HostLifecycle runtime acceptance — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844412
- Step Three quality gate including graph and evidence checks — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341972
- Windows WPF Release build, startup smoke and policy checks — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844535
- Android lint/APK/Protobuf round-trip/generated-source/artifact checks — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417341967/job/121902844191
- Real local inference integration — **PASS**: https://github.com/artinrahimii/project/actions/runs/40417342040

## Contract boundary
These tests verify the current one-request-per-TCP-connection listener behavior. They do not claim peer authentication, encryption, or a completed TCP response delivery path.

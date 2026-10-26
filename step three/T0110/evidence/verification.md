# T0110 verification evidence

## Exact source snapshot and graph validation

- Baseline commit `f090bee81a85dbdc6cac93dc6d08b36dd7e422b0` (`V 3.0.337`); graph Blob `3167748c930cc9cbf1b722db525631ff8a12f21c`.
- Original graph Markdown: **1,728,402 UTF-8 bytes**, **1,606,772 Unicode characters**. Recomputed Git Blob SHA-1 matches the tree-derived ID; full fenced JSON parses to 229 tasks, 426 edges, 22 sections.

## Exact-head results on V 3.0.337

- Step Three quality gate: PASS — https://github.com/artinrahimii/project/actions/runs/40405428005.
- Windows Release build/startup smoke: PASS; default HostLifecycle harness failed afterwards — https://github.com/artinrahimii/project/actions/runs/40405428011.
- Android lint/APK/Protobuf tests: PASS — https://github.com/artinrahimii/project/actions/runs/40405428011.
- T0108 listener participation and T0109 lifecycle tests printed PASS in that HostLifecycle run.
- T0110 framing tests failed when `TcpClient.GetStream()` threw `InvalidOperationException` after the peer socket was already closed/reset. Corrective attempt 02 catches this only around GetStream acquisition; any nonzero read remains a failure.
- The real local inference workflow did not reach its opt-in acceptance because the shared HostLifecycle harness failed first — https://github.com/artinrahimii/project/actions/runs/40405427961.
- Attempt 02 CI is pending; the task remains in progress.

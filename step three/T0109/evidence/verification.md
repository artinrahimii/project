# T0109 verification evidence

## Exact baseline and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Baseline commit: `f8b08f814f28c604fa5f8141ff3b2bd3d137e86b` (`V 3.0.333`); tree `5d26b07d79935d7f6f1428d247851c2e77dc8e6d`.
- Graph path: `step three/project-graph.md`; tree-derived Blob ID `7af68b3ff2eb6794f2a4c2189f58b2f566adf963`.
- Full original graph Blob API content measured **1,709,019 UTF-8 bytes** and **1,587,287 Unicode characters**. Recomputed Git Blob SHA-1 matched. The complete fenced JSON parsed as 229 nodes, 426 edges and 22 sections.
- Baseline `V 3.0.332` product build passed: https://github.com/artinrahimii/project/actions/runs/40341667468.

## Dependency edges

- `T0108 -> T0109`: preserved `blocks`; classified `hard_dependency / validated` because T0109 consumes the concrete stop/dispose APIs created in T0108.
- `T0107 -> T0109`: preserved `blocks`; classified `sequencing_only / validated` because coordinator fault tests do not export an API/data artifact used by network listener lifecycle handling.
- Both edge changes have rationale, source/test evidence, and append-only change-log entries in the graph.

## Attempts and current corrective action

- **Attempt 1:** `T0109-2026-10-26-01`. New lifecycle test passed and the Step Three quality gate passed. Windows Release compile passed but WPF startup smoke failed: `App.xaml.cs` called `Shutdown(0)` immediately after `Window.Close()`, while T0109's `Closing` handler was still awaiting listener disposal. Failure log: https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685895265.
- Exact HostLifecycle output confirmed: `PASS: disabling network participation and disposing the host lifecycle stop and release the listener.` Job: https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685894262.
- **Attempt 2:** added `ExitAfterWindowCleanup` in `src/Host.Windows/App.xaml.cs`; it calls `Application.Shutdown(exitCode)` only after the window's actual `Closed` event. Other smoke-test nonzero exit paths use the same orderly cleanup.
- Corrective exact-head CI remains pending at this commit. T0109 remains `in_progress` until rerun gates pass.
- Scope remains loopback-only; accepted connections close before reading payloads. T0110 must implement bounded Protobuf parsing before request handling.

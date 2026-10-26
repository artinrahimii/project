# T0109 verification evidence

## Baseline graph integrity

- Repository: `artinrahimii/project`, branch `main`; baseline commit `3bc81b6feab2fa3d5b927c9b4ef13ea1f7f626cc` (`V 3.0.334`), tree `8855161bf34d529d3bcc8950be3765dbb17f4c8e`.
- Graph path: `step three/project-graph.md`; tree-derived Blob ID `c62c1ec5b2cd1530ff3d9836f27e9a9ce78002b6`.
- Original full Markdown Blob API content: **1,711,792 UTF-8 bytes**, **1,590,060 Unicode characters**. Recomputed Git Blob SHA-1 matched. The complete fenced JSON parsed to 229 tasks, 426 edges, and 22 sections.
- Product build and real inference were successful on the prior baseline `V 3.0.332`: https://github.com/artinrahimii/project/actions/runs/40341667468.

## Dependency edge validation

- `T0108 -> T0109`: existing type `blocks` preserved; classification `hard_dependency`, validation `validated`. T0109 directly uses the listener's `StopAsync/DisposeAsync` lifecycle introduced in T0108.
- `T0107 -> T0109`: existing type `blocks` preserved; classification `sequencing_only`, validation `validated`; coordinator fault tests expose no task-specific API/data used by listener lifecycle handling.
- Both edge changes have rationale and evidence links in the graph's append-only edge-change history.

## Test and corrective evidence

- Attempt 01 lifecycle test: PASS — https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685894262. Log records that disabling participation and disposing the host lifecycle stop/release the listener.
- Attempt 01 Windows smoke: FAIL with exit code `-532462766` — https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685895265.
- Attempt 02 quality gate: FAIL due to three static contract tests looking for old direct `Shutdown(0/1/3)` literals — https://github.com/artinrahimii/project/actions/runs/40356065948.
- Attempt 02 Windows smoke remained failed — https://github.com/artinrahimii/project/actions/runs/40356065945.
- Attempt 03 defers final close via `Dispatcher.BeginInvoke`, after the original close event unwinds; test files assert the new helper and `Closed` callback. Exact-head CI is pending.
- Scope boundary: listener remains localhost-only and closes incoming sockets without reading payloads. T0110 must add bounded Protobuf parsing before request processing.

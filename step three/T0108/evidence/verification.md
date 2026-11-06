# T0108 verification evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Implementation commit tested by CI: `42efe64069939558deb18414de1c233c739885fd` (`V 3.0.331`), parent `de7ffa55058dfab624a1787d18073db0cb98fa26`.
- Original graph snapshot: commit `de7ffa55058dfab624a1787d18073db0cb98fa26`, tree `5658d5e2a3cfbb7aa50771e0a9cf0538c688d406`, graph path `step three/project-graph.md`, Blob ID `b2bb05b1a85a957696007be4be4ca9eee1119c6b`.
- The original graph Blob API response included its Markdown fence and measured **1,696,227 UTF-8 bytes** / **1,574,318 Unicode characters**. A Git blob SHA-1 recomputed from the original full bytes exactly matched the tree-derived Blob ID. The complete fenced JSON parsed and contained 229 nodes, 426 edges, and 22 sections.
- Current graph Blob at the implementation commit: `2dafac3b81e8068c5503f3ceaa42339d71386d4c`; counts remained 229 tasks and 426 edges.

## Dependency review

- `T0107 -> T0108`: type `blocks` preserved; classification changed from `unreviewed` to `sequencing_only`; validation changed from `unreviewed` to `validated`.
- Evidence inspected: `src/Host.Windows/Requests/HostRequestCoordinator.cs`, `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`, and `step three/T0107/evidence/verification.md`.
- Rationale: T0107 provides coordinator fault-path acceptance coverage, while T0108 introduces a separately gated TCP listener and consumes no T0107-specific output/API. The existing edge is retained as conservative execution order.
- `T0108 -> T0109` and other outgoing edges remain unreviewed.

## Executable outputs and exact-head test evidence

- Production listener: `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`.
- Acceptance test: `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs`, registered in `tests/HostLifecycle.Tests/Program.cs` and linked by `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- **HostLifecycle acceptance: PASS** — [job](https://github.com/artinrahimii/project/actions/runs/40340804518/job/121634661524). The job log explicitly confirms inactive fail-closed startup and successful binding after activation.
- **Step Three quality gate: PASS** — [run](https://github.com/artinrahimii/project/actions/runs/40340804805).
- **Windows and Android product build: PASS** — [run](https://github.com/artinrahimii/project/actions/runs/40340804518), including WPF build/startup and Android APK/Protobuf tests.
- **Live local inference integration: PASS** — [run](https://github.com/artinrahimii/project/actions/runs/40340806731).
- All four runs were associated with implementation commit `42efe64069939558deb18414de1c233c739885fd`. The local environment lacked .NET SDK; test results are from the named CI jobs, not local execution.
- Scope boundary: the listener currently accepts and closes TCP connections without parsing them. T0109 owns app-level shutdown/disable integration; T0110 owns Protobuf parsing. No full P2P transport or encryption claim is made.


# T0108 evidence — T0110 contract revalidation

- Task-specific acceptance ran on V338 commit `86267255efa01fe899a26d58f5e5beec4f860c75`: [HostLifecycle job](`https://github.com/artinrahimii/project/actions/runs/40406054183/job/121863964765`). The existing T0108 test printed PASS after sending a zero-length framed message and confirming fail-closed rejection.
- The new T0110 framing acceptance also passed on the same job. The Step Three quality gate passed: [workflow](https://github.com/artinrahimii/project/actions/runs/40406054261). The complete Windows/Android product workflow passed: [workflow](https://github.com/artinrahimii/project/actions/runs/40406054183).
- This revalidation resolves the temporary contract-review status introduced when T0110 changed incoming connection handling. T0108 is restored to `done`.
- Scope remains loopback-only; transport interoperability, inference dispatch over peer transport, responses, and encryption are not proven by this task.

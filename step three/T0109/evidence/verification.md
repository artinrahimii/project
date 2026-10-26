# T0109 verification evidence

## Exact baseline and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Baseline commit: `908efce321623898ec3cb8023ec8ba3f7b165b2a` (`V 3.0.332`); tree `4488c48e38c9fc39a23e797fa46fce664dd666c2`.
- Graph path: `step three/project-graph.md`; tree-derived Blob ID `ffc687b5dca771e8d6b98a5a2d6d4e2f5acaabc9`.
- Full original Markdown from the Git Blob API measured **1,703,036 UTF-8 bytes** and **1,581,211 Unicode characters**. Recomputed Git Blob SHA-1 matched the tree entry. The complete fenced JSON parsed to 229 unique tasks, 426 edges, and 22 sections.
- `T0108` and `T0107` were `done` in the exact baseline and have exact-head CI evidence. Product build on `V 3.0.332` succeeded: https://github.com/artinrahimii/project/actions/runs/40341667468.

## Reviewed dependency edges

- `T0108 -> T0109`: `type=blocks` preserved; classification `unreviewed -> hard_dependency`; validation `unreviewed -> validated`. Evidence: `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`, its acceptance test, and T0108 report/evidence. The lifecycle controller calls the actual listener stop/dispose contract.
- `T0107 -> T0109`: `type=blocks` preserved; classification `unreviewed -> sequencing_only`; validation `unreviewed -> validated`. The coordinator fault tests expose no T0107-specific API/data consumed by listener lifecycle handling.

## Implementation outputs and pending CI

- Production lifecycle controller: `src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs`.
- WPF integration: `src/Host.Windows/MainWindow.xaml` and `MainWindow.xaml.cs`.
- New tests: `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`; registered in `tests/HostLifecycle.Tests/Program.cs` and linked from `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- New test cases check default inactive state, activation, deactivation, actual port reuse after stop, reactivation, disposal while active, endpoint clearing, and rejecting activation after disposal.
- Exact-head test/build results: **pending** at report creation. No test is claimed passed until CI actually finishes.
- Security/scope boundary: this milestone explicitly binds to loopback only and closes accepted TCP connections without parsing or logging payload data. T0110 must implement the bounded Protobuf parser before requests are processed.

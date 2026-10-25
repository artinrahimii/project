# T0084 — Execution report

- Base commit: `0a044b69e8623a7653ae9db8b58c3b54bdc000c8` (`V 3.0.253`), branch `main`.
- Graph source: `step three/project-graph.md`, blob `cd5a52ecf79b6a844bb30519391f1c82045c62a2`, fetched through Git Blob API; embedded JSON parsed successfully (1,259,795-character response). Independent Git-blob hash comparison was not performed.
- Implemented: explicit confirmation before disconnecting the active model; prevent removal while inference is Busy; serialize disconnect with session operations; dispose the backend client and replace it with an unconfigured backend; stop llama.cpp only when owned by this app. The UI explicitly states the model file is retained.
- Changed: `src/Host.Windows/MainWindow.xaml.cs`, `src/Host.Windows/MainWindow.xaml`, `src/Host.Application/HostInferenceSession.cs`, `tests/HostLifecycle.Tests/HostInferenceSessionConcurrencyTests.cs`, `tests/HostLifecycle.Tests/Program.cs` and this report.
- Added an acceptance test for backend disposal, Stopped lifecycle state, and disconnected status. **Tests were not executed**, so this task remains in progress and is not counted as complete.
- Edge review: `T0083 -> T0084`, `T0075 -> T0084`, and `T0084 -> T0085` remain `unreviewed`; no edge type changed.
- Limitation: the remote llama.cpp protocol has no portable unload endpoint. Client disposal cannot guarantee remote server memory release. If the app owns the server process, stopping it releases its resources; an externally managed server is outside this app's control.

# T0193 — Verification evidence

## Baseline integrity
- Branch: `main`; baseline commit `b1ec4cccc8d0b55782054e12737c334d62864cbc` (`V 3.0.898`).
- Graph Blob: `8ad42a93096ddc1eecaa94f12c6abdcd99472f83`.
- Original fenced UTF-8 payload: 3,519,955 bytes; 3,392,475 JavaScript string code units.
- Git Blob SHA-1 recomputed over `blob <length>\0<payload>`: exact match.
- Full JSON parse: 230 nodes / 422 edges.

## Implemented behavior
- Session shutdown uses a session-wide token linked to gated operations, closes admission, cancels, drains the operation gate and disposes the backend only after active work exits.
- A backend returning after cancellation cannot publish a late inference result.
- WPF close path cancels local work, drains peer service handlers, stops only the app-owned engine, awaits session disposal and tracked UI tasks before the final close.
- Concurrent DisposeAsync callers share the same task; the new backend fixture intentionally ignores cancellation until the test releases it, making unsafe early disposal observable.

## Files
- `src/Host.Application/HostInferenceSession.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/HostInferenceSessionConcurrencyTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_network_listener_exposure.py`
- `tests/test_wpf_entrypoint.py`

## Dependency review
- `T0192 → T0193`: `sequencing_only / validated`; original `type=blocks` retained.
- `T0188 → T0193`: `sequencing_only / validated`; original `type=blocks` retained.

## Test status
Exact-head GitHub Actions has not yet run for the T0193 implementation commit. No tests are marked passed here. Local .NET execution was unavailable because the runtime environment has no .NET SDK.


## T0193-A002 — follow-up source and dependency review

- Follow-up baseline: `main` `0ba42632794058dd478b96427f08857e1b75479b` (`V 3.0.899`).
- Graph path: `step three/project-graph.md`; tree-selected Blob: `b52dbd1453a0f312c37cd2fb21670ebb42f7d9b8`.
- Original fenced UTF-8 payload: **3,524,924 bytes**; text length: **3,397,446 Unicode code points**.
- Recomputed Git SHA-1 over `blob 3524924\\0<payload>`: exact match with the tree Blob ID. Full JSON parse: 230 task nodes / 422 edges, no duplicate task IDs and no unresolved edge endpoints.
- Re-reviewed `T0192 → T0193` and `T0188 → T0193`: both remain `sequencing_only / validated`; the original `type=blocks` edges and conservative execution order are preserved.
- Follow-up found an untracked async network-participation transition during WPF Closing. A002 tracks, cancels and awaits that operation; the Python guard covers ordering. The runtime and exact-head build result remains pending; no pass is claimed.


## T0193-A003 — source/test consistency and attempt trace
- A001 implementation commit: `0ba42632794058dd478b96427f08857e1b75479b` (`V 3.0.899`).
- A002 peer-participation lifecycle commit: `dfc6ed433dc35bf57b088ed2b571f273e4f4eb0a` (`V 3.0.900`).
- Graph snapshot used for this review: `main` commit `dfc6ed433dc35bf57b088ed2b571f273e4f4eb0a`, path `step three/project-graph.md`, Blob `3cba90b5dac0d6d61d55ca555c379258a13f36fc`, GitHub size `3526074` bytes. The recomputed Git Blob SHA-1 matched; full JSON parsed as 230 tasks and 422 edges.
- The new A003 source guard now expects `service.StartAsync(cancellationToken)` and verifies the single-transition check. This is a correction made before running CI, not a recorded failed test.
- Exact-head jobs for V 3.0.900 were queued; no automated results are claimed. T0193 remains `in_progress`.

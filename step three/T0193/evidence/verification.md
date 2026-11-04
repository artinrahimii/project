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


## T0193-A004 — source guard review

- Baseline: `main` `717caa832d9b16073f045eb27957b94ad3ae62ff` (`V 3.0.901`).
- The new WPF single-flight transition guard is written in C# as `if (_suppressParticipationToggleChanged || _windowClosing || _peerParticipationTask is not null)`. The Python test now checks that exact source text.
- This was a static source/test consistency correction before CI execution. The test was not run locally; no pass/fail result is claimed.


- A004 commit attribution: `tests/test_network_listener_exposure.py` and the corresponding report/evidence/graph updates are committed in `bbb744beb126d4f38c50dfa00014a92992b0d119` (`V 3.0.902`).


## T0193-A005 — exact-head evidence (V 3.0.903)

- Branch/head: `main` / `dad30f9594175c36635a60c4f5cd872ce0852332`; commit message `V 3.0.903`.
- Graph source: `step three/project-graph.md`, Blob `cd54b7b97fe1eb31b0278c8dd391f81d0c1f2631`, 3,528,709 original UTF-8 bytes. SHA-1 computed over the complete Git object payload matched the Blob ID; full fenced JSON parsed as 230 nodes and 422 edges.
- Quality run `41865536479`, job `126698664591`: failed; four Python source-contract tests listed in the execution report failed.
- Windows product run `41865536757`, job `126698664507`: failed at C# whitespace verification, with 20 diagnostics in the two WPF `finally` blocks. The Windows build did not start.
- HostLifecycle job `126698665014`: passed. Its log contains the explicit PASS line for `DisposeAsyncCancelsAndDrainsActiveGenerationAsync`, verifying cancellation, rejection of late results, operation draining, backend disposal ordering and closed admission on this head.
- Android product job `126698665117`: passed. It is additional product CI coverage, not a substitute for the failed Windows/quality gates.
- Rendezvous run `41865537064` was still in progress when its jobs were inspected; no conclusion is claimed here.

## T0193-A006 — correction verification boundary

- Source-contract updates: `tests/test_decision_traceability.py`, `tests/test_inference_backend_abstraction.py`, and `tests/test_wpf_entrypoint.py`.
- Formatting correction: `src/Host.Windows/MainWindow.xaml.cs`; fixed the exact two indentation regions and whitespace-only line identified by Windows CI.
- The corrected tests/formatting have **not yet been executed**. No pass is claimed. T0193 remains `in_progress` until the new commit's quality workflow, Windows WPF workflow and HostLifecycle runtime workflow are inspected.


## T0193-A007 — exact-head evidence (V 3.0.904)

- Quality run `41868807190`, job `126709921133`: failed exactly one source-contract test. The failing expectation is the old direct-field `DisposeAsync` call; the current implementation uses the local `peerService` reference and conditional shared-field cleanup.
- Product run `41868807248`, Windows WPF job `126709919875`: passed all steps, including `dotnet format whitespace`, Release build, generated Protobuf check, WPF startup smoke test, and Windows agent-tool/identity tests.
- Product run `41868807248`, HostLifecycle job `126709920653`: passed, including the executable session shutdown lifecycle tests.
- Rendezvous run `41868807217`, job `126709919477`: passed.
- Android job `126709920356`: JVM test step was still in progress at inspection; no final conclusion is recorded.

## T0193-A008 — verification boundary

- Updated `tests/test_decision_traceability.py` to assert the local peer-service variable, async disposal call, and identity-checked clearing of the shared service reference.
- The source guard has been statically compared with the WPF implementation. It has **not** been run yet; no pass is claimed.
- T0193 remains `in_progress` pending a passing Quality run and completion of all relevant product CI.


## T0193-A009 — correction verification boundary

- Corrected `tests/test_decision_traceability.py` line 712 from 16 leading spaces to 8, matching the neighboring class-method assertions.
- The V 3.0.905 Quality run `41869697038` failed during test module import, before the test body could run; this is recorded as an A008 attempt failure.
- No new test pass is claimed. Exact-head V 3.0.906 Quality and product CI are required.


## T0193-A010 — peer request admission shutdown race

- Baseline: `main` `ed131c3456461f2bc215db8da2300a6d915f33cc` (`V 3.0.906`); the previous exact-head quality gate passed 399 Python tests, Windows WPF build/format/startup checks passed, HostLifecycle shutdown acceptance passed, and Rendezvous container smoke test passed. Android was still running at inspection; no Android result is claimed.
- Source inspection found that `StopAsync` delayed lifetime cancellation until after `publisher.StopAsync`, while `ReceiveLoopAsync` never checked `IsActive` before registering a request handler.
- The revised implementation places request slot acquisition and handler registration under `_requestAdmissionSync`, with an `IsActive`/cancellation guard. `StopAsync` sets inactive under the same lock and cancels the receive-loop lifetime before awaiting publisher shutdown.
- The receive-loop finalizer continues to cancel active request tokens and await all registered handler tasks, preserving in-flight drain semantics.
- Added `test_peer_shutdown_closes_request_admission_before_cancelling_and_draining` to `tests/test_network_listener_exposure.py`; it asserts the ordering and synchronization contract.
- The new changes have not yet been exercised by CI. No success is claimed for A010 until exact-head workflow results are inspected.

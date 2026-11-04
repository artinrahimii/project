# T0193 — Stop host admission and safely drain active work during shutdown

- **Status:** `in_progress`; implementation commits `V 3.0.899` and `V 3.0.900` are online. A003 corrected a stale source-contract assertion; exact-head CI is still pending.
- **Baseline:** `main`, `b1ec4cccc8d0b55782054e12737c334d62864cbc` (`V 3.0.898`); graph Blob `8ad42a93096ddc1eecaa94f12c6abdcd99472f83` (3,519,955 UTF-8 bytes; Git Blob SHA-1 independently matched).
- **Defect:** `HostInferenceSession.Dispose()` previously disposed its backend without acquiring the semaphore used by inference. It could release a backend during active generation. WPF cancelled operation sources only from `MainWindow_Closed`, after the closing path.
- **Session fix:** `HostInferenceSession` implements `IAsyncDisposable`; all gated operations are linked to a session shutdown token. Shutdown closes admission immediately, publishes one shared/idempotent disposal task, signals cancellation, waits for the common operation gate, transitions to `Stopped`, and only then detaches and disposes the backend. Generation checks cancellation after the backend returns so a cancellation-ignoring backend cannot publish late output. Synchronous `Dispose()` remains compatible and delegates to the async drain.
- **WPF fix:** generation and model-update tasks are tracked. `Closing` marks the window as closing and disables it, cancels local generation/model update, awaits peer service/listener disposal (including active handler drain), stops only the engine process owned by this app, awaits session disposal and tracked UI tasks, then schedules the second `Close`. External backend processes are not killed. `Closed` now only disposes already-drained cancellation sources and the process launcher.
- **Race protection:** the peer-service reference remains available until its asynchronous disposal finishes. The shared disposal task is published before cancellation callbacks run, so concurrent/re-entrant callers cannot begin a second shutdown.
- **Runtime test:** `DisposeAsyncCancelsAndDrainsActiveGenerationAsync` uses a deliberately cancellation-ignoring backend. It verifies cancellation signalling, common disposal task identity for simultaneous callers, that disposal waits while generation remains active, rejection of a late response, backend disposal only after operation completion, final `Stopped` state, and rejection of new work.
- **Source-contract tests:** `tests/test_network_listener_exposure.py` validates the WPF shutdown order; `tests/test_wpf_entrypoint.py` validates async session cancellation/drain. The C# runtime test is registered in `tests/HostLifecycle.Tests/Program.cs`.
- **Dependencies:** both incoming edges (`T0192 → T0193`, `T0188 → T0193`) were reviewed as `sequencing_only / validated`; the original `type=blocks` and conservative order are retained. Neither predecessor exposes an executable API/schema consumed by this shutdown implementation.
- **Limits:** no secure-memory-erasure claim is made; external processes are not terminated. This environment has no local .NET SDK, so no local .NET success is claimed.

## Changed implementation and tests

- `src/Host.Application/HostInferenceSession.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/HostInferenceSessionConcurrencyTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_network_listener_exposure.py`
- `tests/test_wpf_entrypoint.py`


## T0193-A002 — peer participation transition drain

- **Result:** partial; an additional WPF lifecycle race was found during review of the A001 implementation.
- **Finding:** `NetworkParticipationToggle_Changed` was an untracked async-void event. Closing the window while activation/deactivation awaited service startup/disposal could let that operation outlive the close handler's snapshot of `_peerParticipationService`.
- **Correction:** the handler now delegates to a tracked `Task`, uses an operation-specific cancellation source for startup, prevents overlapping transitions, and clears/disposes its tracking fields in `finally`. `MainWindow_Closing` cancels and awaits this transition before disposing any remaining peer service. Deactivation still drains `DisposeAsync`; activation receives the cancellation token and disposes any partially started service on cancellation.
- **Regression guard:** `tests/test_network_listener_exposure.py` now verifies that close cancels and awaits the transition before the final peer-service disposal, and that service startup observes the transition token.
- **Exact-head CI:** the previous A001 head `0ba42632794058dd478b96427f08857e1b75479b` had quality, product, Rendezvous and real-inference workflows queued when this correction was made. No CI success is claimed for A002; the new commit must pass its own exact-head checks.
- **Local execution:** no local .NET/WPF runtime is available in this environment. The new Python source guard is pending the exact-head quality workflow.


## T0193-A003 — source-contract consistency correction

- **Baseline:** `main` commit `dfc6ed433dc35bf57b088ed2b571f273e4f4eb0a` (`V 3.0.900`).
- **Finding:** `tests/test_network_listener_exposure.py` still required `await service.StartAsync();`, while the tracked WPF activation path now correctly calls `await service.StartAsync(cancellationToken);`. Source inspection showed the stale expectation would reject the updated implementation.
- **Correction:** updated the assertion to the cancellable overload and added a source guard for preventing overlapping participation transitions. No production runtime code changed in A003.
- **Execution status:** this mismatch was found by comparing source and test files before CI; it is not recorded as an executed test failure. No test pass/fail is claimed for this attempt.
- **Prior attempt commits:** A001 `0ba42632794058dd478b96427f08857e1b75479b` (`V 3.0.899`); A002 `dfc6ed433dc35bf57b088ed2b571f273e4f4eb0a` (`V 3.0.900`).
- **Exact-head CI for V 3.0.900:** quality run `41864411115`, product run `41864410577`, and Rendezvous run `41864410847` were queued at the latest check. The new V 3.0.901 commit must pass its own exact-head quality and product jobs before T0193 can be completed.
- **Environment limit:** no local .NET SDK/WPF runtime is available; no local .NET tests were run.


## T0193-A004 — source guard token correction

- **Finding:** the A003 assertion searched C# source for Python tokens (`or` and `None`) instead of the actual C# condition (`||` and `null`). That assertion would fail when executed.
- **Correction:** changed the source-contract expectation to the exact C# expression. No production source changed in A004.
- **Execution status:** found by direct source/test comparison before CI started; no executed test failure or pass is claimed.
- **Baseline:** `main` `717caa832d9b16073f045eb27957b94ad3ae62ff` (`V 3.0.901`). Exact-head CI for V 3.0.901 was queued at inspection; A004 awaits its own exact-head CI.
- **Task state:** `in_progress`; no acceptance claim is made.


- A004 source-guard correction commit: `bbb744beb126d4f38c50dfa00014a92992b0d119` (`V 3.0.902`), verified on `main`.


## T0193-A005 — exact-head CI verification (V 3.0.903)

- **Baseline:** `main` commit `dad30f9594175c36635a60c4f5cd872ce0852332` (`V 3.0.903`); graph Blob `cd54b7b97fe1eb31b0278c8dd391f81d0c1f2631` was retrieved and its complete Git Blob SHA-1 verified.
- **Quality workflow:** run [41865536479](https://github.com/artinrahimii/project/actions/runs/41865536479), job `126698664591`, failed during the Python unit-test step. Four source-contract tests failed:
  - `test_participation_listener_lifecycle_decision_maps_to_fail_closed_wpf_shutdown` expected the old parameterless `StartAsync()`, while WPF now passes its tracked cancellation token.
  - `test_session_logic_depends_on_the_interface` expected the caller token, while the session correctly passes its linked `operationToken`.
  - `test_clear_session_suppresses_late_generation_errors` expected the older condition and omitted the additional `_windowClosing` guard.
  - `test_host_session_async_disposal_cancels_and_drains_before_backend_release` expected a nonexistent `previousBackend.Dispose()` call instead of the actual `IDisposable backend` pattern.
- **Windows workflow:** product run [41865536757](https://github.com/artinrahimii/project/actions/runs/41865536757), job `126698664507`, failed at `dotnet format whitespace WhiteP.sln --verify-no-changes`. The log identified 20 whitespace/indentation diagnostics in two `finally` blocks in `MainWindow.xaml.cs`; the Windows build and later checks were skipped.
- **Runtime evidence:** the separate HostLifecycle job `126698665014` in the same product run passed. Its log explicitly reports: `PASS: async host shutdown cancels active inference, rejects late results, drains work before backend disposal, and closes admission.` The Android build/test job `126698665117` also passed. These results do not make the failed quality/Windows gates pass.
- **Decision:** T0193 remains `in_progress`. No failed source-contract test or formatter run is re-labelled as successful. The implementation's dedicated shutdown runtime acceptance test did pass on this exact head; remaining source-contract/formatting gates need correction and a fresh exact-head run.


## T0193-A006 — correct stale source guards and WPF formatting

- Updated the decision-traceability assertion to require the cancellable `service.StartAsync(cancellationToken)` call used by the tracked peer-participation transition.
- Updated the backend abstraction assertion to require the linked `operationToken` passed to the backend, preserving the session-shutdown cancellation contract.
- Updated the clear-session assertion to require both `_clearSessionRequested` and `_windowClosing` to suppress late UI updates.
- Updated the disposal assertion to match the actual safe pattern: detach `previousBackend`, pattern-match it as `IDisposable backend`, then call `backend.Dispose()` after the operation gate has drained.
- Corrected the two WPF `finally` blocks' indentation and removed the whitespace-only line reported by the Windows formatter. No lifecycle behavior was intentionally changed by this formatting correction.
- **Verification status:** these edits were statically checked against the source and the four failed CI assertions. No local Python suite or .NET formatter/build was run in this environment. Fresh exact-head CI for the resulting commit is required; T0193 remains `in_progress`.
- **Changed files:** `src/Host.Windows/MainWindow.xaml.cs`, `tests/test_decision_traceability.py`, `tests/test_inference_backend_abstraction.py`, `tests/test_wpf_entrypoint.py`.


## T0193-A007 — exact-head CI verification (V 3.0.904)

- **Quality workflow:** run [41868807190](https://github.com/artinrahimii/project/actions/runs/41868807190), job `126709921133`, failed one Python source-contract test: `test_participation_listener_lifecycle_decision_maps_to_fail_closed_wpf_shutdown` expected `await _peerParticipationService.DisposeAsync();`, while the WPF shutdown path retains the service in local variable `peerService`, awaits `peerService.DisposeAsync()`, then clears the shared field only if it still references that service.
- The three other source-contract tests corrected in A006 no longer appear among the failures.
- **Windows WPF job:** product run [41868807248](https://github.com/artinrahimii/project/actions/runs/41868807248), job `126709919875`, passed formatting, Release build, Protobuf generation, WPF startup smoke test, and all Windows agent-tool / host identity checks.
- **HostLifecycle job:** product job `126709920653` passed; the runtime shutdown acceptance remains verified.
- **Rendezvous:** run [41868807217](https://github.com/artinrahimii/project/actions/runs/41868807217), job `126709919477`, passed.
- **Android:** job `126709920356` was still running its JVM test step when inspected; no final result is claimed.
- **Decision:** T0193 remains `in_progress` because the Quality gate still fails one source-contract assertion and Android CI was not yet complete.


## T0193-A008 — peer-service ownership assertion correction

- Replaced the obsolete direct-field disposal expectation with checks for the actual close-path ownership sequence: copy `_peerParticipationService` to `peerService`, await `peerService.DisposeAsync()`, and clear the shared reference only when it still refers to the same service.
- This preserves the existing production lifecycle behavior; this attempt changes only the source-contract test and execution evidence.
- **Verification status:** the new assertion has been checked against the WPF source from the exact V 3.0.904 head. It has not yet been executed. A fresh exact-head Quality run is required, and T0193 remains `in_progress`.


## T0193-A009 — repair Python module indentation after A008

- **Failure found on exact head:** Quality run [41869697038](https://github.com/artinrahimii/project/actions/runs/41869697038), job `126713109741`, could not import `test_decision_traceability`. The first new peer-service assertion in A008 had 16 leading spaces at class-method scope, causing an indentation error.
- **Correction:** changed that assertion to the same 8-space class-method indentation as adjacent assertions. The other A008 lines already had the correct indentation.
- **Impact:** test source only; no production code or dependency contracts changed.
- **Verification:** statically inspected the corrected lines. The corrected Python module has not yet been executed; no pass is claimed. T0193 remains `in_progress` pending fresh exact-head CI.


## T0193-A010 — close peer request admission before shutdown drain

- **Baseline:** online `main` commit `ed131c3456461f2bc215db8da2300a6d915f33cc` (`V 3.0.906`). The previous exact-head quality, Windows WPF, HostLifecycle, and Rendezvous jobs passed; Android was still running when inspected. Those baseline results do not validate this new correction.
- **Finding:** `PeerAnnouncementHostLifecycle.StopAsync()` set `_active = 0`, but awaited `publisher.StopAsync()` before cancelling the receive-loop lifetime. `ReceiveLoopAsync()` did not consult `IsActive` when registering an inference handler. A request arriving during publisher shutdown could therefore be admitted after shutdown had begun.
- **Correction:** added `_requestAdmissionSync` to serialize the admission decision/handler registration with the `_active = 0` transition. The receive loop now rejects a request when admission is closed or the lifetime token is cancelled. `StopAsync()` closes admission under the same lock and cancels the receive loop immediately, before awaiting publisher shutdown; the existing receive-loop finalizer still cancels and awaits all already-registered handlers.
- **Regression guard:** added a Python source-contract test asserting the lock/guard/registration order and that lifetime cancellation precedes the awaited publisher stop.
- **Dependency review:** T0192 → T0193 and T0188 → T0193 remain `sequencing_only / validated`; original `blocks` edges and conservative execution order are unchanged. No edge was reclassified in this attempt.
- **Verification boundary:** source changes and the guard were checked structurally before commit. No local C#/.NET runtime is available here; this new correction has not yet run in CI. T0193 remains `in_progress` pending exact-head Quality, Windows WPF, and HostLifecycle CI.

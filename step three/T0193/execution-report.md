# T0193 — Stop host admission and safely drain active work during shutdown

- **Status:** `in_progress`; implementation and regression tests are prepared for `V 3.0.899`; exact-head CI is pending.
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

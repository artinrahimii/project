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

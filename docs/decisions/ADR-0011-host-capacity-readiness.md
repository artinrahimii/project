# ADR-0011: Fail-closed host capacity and readiness assessment

- **Status:** Accepted for the existing implementation and executable verification paths.
- **Date:** 2026-10-27
- **Related requirement:** REQ-006 in `docs/requirements.md`.
- **Acceptance gate:** AC-HOST-01 in `docs/acceptance.md`.
- **Scope:** Windows host hardware discovery and the local-inference readiness/capacity decision.

## Context

The host must not report that inference can run merely because its UI is open, an OS/device field is present, or a measurement/check is missing. Readiness depends on observed host capacity and an available inference backend; display-adapter inventory alone does not establish GPU compute or VRAM capability.

## Decision

1. **Use real platform observations.** `WindowsHostHardwareDiscovery` obtains OS information, logical processor count, physical-memory measurements through the Windows API, system-drive capacity through `DriveInfo`, and display-adapter inventory through the Windows display API. Discovery rejects invalid or unavailable required measurements rather than replacing them with fabricated values. An unavailable CPU-name string is reported as unavailable.
2. **Keep capacity states distinct.** The platform-neutral `HostCapabilities` model returns `Insufficient` when a known required check fails; it returns `Unknown` when required checks or valid measurements are missing/inconsistent; only complete valid measurements with all required checks explicitly passing return `Ready`.
3. **Fail closed for model-specific resource checks.** `HostReadinessEvaluator` rejects non-finite, non-positive or unreasonable RAM/disk requirements, rejects unavailable system-drive capacity, and checks declared model RAM against currently available physical memory and the declared free-disk minimum against observed free space. The configured backend must independently pass its health check.
4. **Gate inference on the full state.** `CanRunInference` is true only when the capacity state is `Ready`. The Windows UI disables generation unless the application lifecycle is `Ready` and the measured capacity state is `Ready`.
5. **Disclose measurement limits.** The UI distinguishes insufficient capacity from unknown capacity. Display-adapter discovery is inventory only; the UI explicitly says GPU compute/VRAM capability is not verified unless a separate supported check proves it.

## Implementation and verification links

- Requirement and acceptance contract: `docs/requirements.md` (REQ-006); `docs/acceptance.md` (AC-HOST-01).
- Platform-neutral state and fail-closed rule: `src/Host.Domain/HostCapabilities.cs`.
- Declared RAM/disk readiness policy: `src/Host.Application/HostReadinessEvaluator.cs` and `src/Host.Application/HostReadinessResult.cs`.
- Observed Windows measurements: `src/Host.Windows/WindowsHostHardwareDiscovery.cs`.
- UI wiring and generation gate: `src/Host.Windows/MainWindow.xaml.cs` and `src/Host.Windows/MainWindow.xaml`.
- Executable C# acceptance coverage: `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs` and the readiness cases registered in `tests/HostLifecycle.Tests/Program.cs`.
- Platform-discovery/readiness source-contract checks: `tests/test_windows_hardware_discovery.py` and `tests/test_decision_traceability.py`.
- Product verification entrypoint: `.github/workflows/product-build.yml`.

## Evidence boundary

CI proves the current tests/build execute on the configured runners; it does not prove the capacity of a particular user's physical machine. Readiness checks cover declared RAM and system-drive requirements plus backend health; adapter enumeration does not prove GPU execution or VRAM availability. Actual local inference and other runtime acceptance gates remain governed by their own evidence in `docs/acceptance.md`.

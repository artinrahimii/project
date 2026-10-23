# T0054 execution report

## Dependency review
- `T0053 → T0054`: validated as `contract_or_interface_dependency`; the runtime guard follows the capacity-state semantics exercised by the T0053 executable acceptance tests.
- `T0048 → T0054`: validated as `contract_or_interface_dependency`; the implementation uses the HostInferenceSession lifecycle transition contract and must clear Ready on readiness failure.

## Attempt T0054-A001 — readiness gating
- Added `HostReadinessResult` in the application layer.
- `HostInferenceSession` now refuses to report Ready unless a readiness evaluator is configured and returns success after the backend health check.
- The session rechecks readiness before each inference; if the capacity check fails or throws, the session transitions to Error and refuses generation rather than retaining Ready.
- The WPF host supplies a readiness evaluator that compares the declared model-memory requirement entered in the UI with live Windows available physical RAM via the existing estimator. Missing/invalid requirement or unreadable hardware returns not-ready.
- The backend's existing `/health` check remains the model/backend readiness signal. Disk-space thresholds and exact configured model-ID matching are not implemented by this task and are not claimed.

## Added regression assertions
- A session with no readiness evaluator remains out of Ready.
- An unhealthy/loading backend remains out of Ready.
- A host that becomes capacity-incompatible between connection and inference transitions from Ready to Error and refuses to generate.

## Verification
Exact-commit GitHub Actions results are pending. This task remains in progress until the product build, lifecycle tests and graph quality gate have been inspected.

 
## Attempt T0054-A002 — enforce both RAM and disk capacity

- Added `HostReadinessEvaluator` in the application layer as a fail-closed policy for the declared model RAM requirement and minimum free system-drive space.
- The Windows UI now exposes a minimum free-disk requirement (GiB), and the host checks current Windows RAM and system-drive free space after backend health succeeds and before each inference.
- Added executable policy assertions for sufficient capacity, low RAM, low disk, invalid declarations, and unknown disk measurement.
- The exact-commit GitHub Actions result is pending; this attempt remains in progress until the C# tests and product/quality workflows pass.

## Verification update

The earlier verification note above records the state of attempt A001. Attempt A002 expands the runtime guard to cover both resources; no test success is claimed until CI completes on the implementation commit.

 
## Verification result for T0054-A002

- The exact implementation commit `8611918c4e7dcd8bff075a3c76ab8535ba5bd574` failed the host lifecycle workflow at the low-disk message assertion in `tests/HostLifecycle.Tests/Program.cs`.
- The readiness policy correctly returned not-ready when free disk was below the declared minimum, but the user-facing message said "free space on the Windows system drive" and did not contain the word "disk" expected by the assertion.
- This is recorded as a failed verification attempt. The result is not treated as a passing acceptance test. The message wording is corrected in attempt A003; the revised commit must pass the full product workflow before completion.

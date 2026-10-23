# T0054 verification evidence

Implementation under verification:
- `src/Host.Application/HostReadinessResult.cs`
- `src/Host.Application/HostInferenceSession.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/Program.cs`

The executable harness now checks:
1. Missing readiness evaluator cannot transition a healthy endpoint to Ready.
2. Unhealthy/model-loading backend cannot transition to Ready.
3. Capacity becoming insufficient after initial connection transitions the session to Error and prevents generation.

The WPF evaluator checks the user-declared RAM requirement against live available physical RAM. Exact-commit CI is pending; no test pass is claimed here until GitHub Actions completes.

 
## Additional readiness-policy coverage (T0054-A002)

Added executable assertions for:
1. Ready only when the declared RAM and minimum free system-drive requirements both pass.
2. Insufficient live RAM produces a not-ready result.
3. Insufficient live system-drive space produces a not-ready result.
4. Invalid or missing RAM/disk declarations fail closed.
5. Unknown disk capacity cannot produce Ready.

Runtime integration uses `WindowsHostHardwareDiscovery.Discover()` for current available physical RAM and system-drive free space. These assertions and the Windows/Android product builds await exact-commit GitHub Actions results before this task can be marked complete.

 
## Failed exact-commit verification (T0054-A002)

- Host lifecycle unit/integration job failed on commit `8611918c4e7dcd8bff075a3c76ab8535ba5bd574`: assertion "Insufficient live system-drive space must fail readiness with a disk-specific message." The result message expressed the low system-drive space condition but omitted the expected word "disk".
- The failure is retained in the execution history. Attempt A003 changes the user-facing text to "free disk space..." and reruns the product and quality workflows.


## Final exact-commit evidence — T0054-A003

Implementation reviewed: `e3027d8ce8764849f2152362293c7ee52eb9b96b` (`V 3.0.168`).

- [C# host lifecycle unit/integration tests — passed](https://github.com/artinrahimii/project/actions/runs/39956743018/job/120424349599). Logs include PASS for low-RAM, low-disk, invalid declarations and unknown disk measurements.
- [Windows WPF Release build and startup smoke — passed](https://github.com/artinrahimii/project/actions/runs/39956743018/job/120424349834). Build completed with zero errors; the smoke check showed required controls and clean shutdown.
- [Android debug APK and Protobuf round-trip checks — passed](https://github.com/artinrahimii/project/actions/runs/39956743018/job/120424350049).
- [Step Three graph/Python quality gate — passed](https://github.com/artinrahimii/project/actions/runs/39956741851/job/120424345873).
- Overall [product build workflow](https://github.com/artinrahimii/project/actions/runs/39956743018) and [quality workflow](https://github.com/artinrahimii/project/actions/runs/39956741851) both completed with success on this exact commit.

The prior A002 low-disk assertion failure was corrected in A003 and is preserved as historical failure evidence. T0054 now meets the implemented acceptance scope. Exact runtime inference with a real configured model and peer-to-peer transport were not exercised by these workflows and are not claimed.

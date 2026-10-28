# T0139 — Verification evidence

## Graph source validation
- Exact base: `main`, `V 3.0.486`, commit `b4224c06e9b09c108361965143d93d1eaf42b789`; tree `8049cc186a1c166477e1f493af05b81a360a0e34`.
- Graph Blob `0ec4af178841903ab343533bafc75bdc2ad9f4b1`; original content is **2,356,247 UTF-8 bytes** and **2,234,407 characters**. SHA-1 over the correct Git object header plus complete bytes matched the tree Blob. JSON parsed fully as 229 tasks / 417 active edges.
- Task remains `in_progress`; failed A001 and A002 attempts are retained.

## A001 — V 3.0.485
- [Quality job 123096849080](https://github.com/artinrahimii/project/actions/runs/40767883598/job/123096849080): three existing source-contract tests failed because explicit P2P-preview/non-delivery phrases were absent.
- [Android job 123096849669](https://github.com/artinrahimii/project/actions/runs/40767883644/job/123096849669): ktlint failed on style issues; APK and JUnit did not run.
- [HostLifecycle job 123096849547](https://github.com/artinrahimii/project/actions/runs/40767883644/job/123096849547): pass.

## A002 — V 3.0.486
- [Quality job 123098802251](https://github.com/artinrahimii/project/actions/runs/40768437264/job/123098802251): two new Android navigation source-contract assertions failed because test expectations used the old function name and older privacy wording.
- [Android job 123098803201](https://github.com/artinrahimii/project/actions/runs/40768437336/job/123098803201): ktlintCheck failed; only the step-level failure could be retrieved, not detailed logs. APK/JUnit were skipped.
- [HostLifecycle job 123098803429](https://github.com/artinrahimii/project/actions/runs/40768437336/job/123098803429): pass.

## A003 correction
- MainActivity and route tests now use the actual lowercase `whitePClientApp()` convention and the explicit statement that transport encryption does not hide content from the receiver.
- The enum is now a pure route enum; `ClientNavigation` owns start/top-level destinations, avoiding enum companion/separator formatting.
- Exact A003 CI result: **pending**.


## V 3.0.487 verification — failed

- [Step Three quality job 123100940796](https://github.com/artinrahimii/project/actions/runs/40769048874/job/123100940796): 199 tests ran; 1 failed source-contract assertion still expected `ClientDestination.startDestination`. Graph/syntax/evidence checks were skipped.
- [Android job 123100941472](https://github.com/artinrahimii/project/actions/runs/40769048985/job/123100941472): ktlint failed at `MainActivity.kt:266` (“A multiline expression should start on a new line”); APK/JUnit were skipped. [HostLifecycle job 123100941303](https://github.com/artinrahimii/project/actions/runs/40769048985/job/123100941303) passed.
- Exact recursive-tree inspection of V 3.0.487 confirmed the referenced `ClientNavigation.kt` was absent, despite `MainActivity.kt` referring to it. This would cause a compilation failure after style issues were corrected.

## Attempt T0139-A004

- Added the missing `ClientNavigation.kt` implementation and its import; aligned the source-contract test with the real route owner; fixed the observed multiline-expression style.
- Exact-head CI is pending. No test execution is claimed in the local tool session.

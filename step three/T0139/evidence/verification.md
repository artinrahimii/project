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


## V 3.0.488 CI result — ledger validation failure

- Step Three quality job [123103335543](https://github.com/artinrahimii/project/actions/runs/40769725447): 199 Python tests passed; graph_guard failed with `ATTEMPT_COUNT_MISMATCH` (`summary=4`, `history=5`) for T0139. Existing T0001 free-text dependency remained a warning.
- Android job [123103334973](https://github.com/artinrahimii/project/actions/runs/40769725408/job/123103334973): `ktlintCheck` passed; APK build/JUnit had not completed at status capture. HostLifecycle [123103335562](https://github.com/artinrahimii/project/actions/runs/40769725408/job/123103335562) passed. Windows job [123103335380](https://github.com/artinrahimii/project/actions/runs/40769725408/job/123103335380) was still running.
- Attempt A005 corrects the summary count to the actual append-only history length. T0139 remains in_progress pending new exact-head CI.


## Exact-head audit — V 3.0.489

- Step Three quality gate [40770058117](https://github.com/artinrahimii/project/actions/runs/40770058117): passed.
- Android product build [40770058274](https://github.com/artinrahimii/project/actions/runs/40770058274), job [123104516428](https://github.com/artinrahimii/project/actions/runs/40770058274): ktlint passed; debug APK failed at `:app:checkDebugAarMetadata`. The log specifies Navigation Compose 2.10.2 requires compile API 37 and AGP 9.1.0+; the project currently uses SDK 36 / AGP 8.13.0. Android JUnit, generated-source checks and APK upload were skipped. Host lifecycle passed; Windows build had not yet completed at inspection.
- The full graph Blob from V 3.0.489 was read via Git Blob API; UTF-8 byte size matched the tree's 2,371,630 bytes, and the Git SHA-1 was independently recalculated against the tree Blob before editing.

## Attempt T0139-A006 — pending validation

- Pinned Navigation Compose to `2.9.8` and updated the dependency contract assertion. This keeps the navigation implementation on a stable version line that predates the API 37 / AGP 9.1 requirement while leaving AGP, Gradle, compileSdk, targetSdk, and minSdk untouched.
- Exact-head CI must now pass graph guard, source tests, ktlint, APK build, Android JUnit, generated Protobuf validation, HostLifecycle, and Windows WPF checks before T0139 can become done.


## A007 — single navigation owner and Android build compatibility

- Parent commit: `9d9cfa67ef3c7e009a6d7e441080ffda38631e28` (`V 3.0.490`). Exact-tree Blob verification was run on every input file before modification; the graph parsed as 229 tasks / 417 edges and the original graph content round-tripped byte-for-byte before its task ledger was edited.
- The exact V 3.0.489 Android log states that `navigation-compose-android:2.10.2` requires AGP 9.1+, while the project uses AGP 8.13.0 / compileSdk 36; the log also reported AndroidX Lifecycle 2.11.0 requiring API 37 / newer AGP. V 3.0.490 pins Navigation Compose to 2.9.8, a stable release line documented in the [official AndroidX Navigation release notes](https://developer.android.com/jetpack/androidx/releases/navigation). V 3.0.490 CI: source/graph quality passed, HostLifecycle passed, Android lint passed; APK build and Windows product jobs were still running at the exact-tree capture, so those results are not marked passed here.
- Source review of the V 3.0.490 tree found a second `internal object ClientNavigation` declaration left in `ClientDestination.kt` alongside the dedicated `ClientNavigation.kt` file. The prior APK build had failed before Kotlin compilation, so no compiler result for the duplicate was available. A007 removes the duplicate and adds a regression test verifying the single canonical object.
- Local Python, Android and Windows commands were not run; local GitHub name resolution failed and Gradle/.NET SDKs are absent. Exact-head GitHub Actions is pending. Keep T0139 `in_progress` until APK, Android JUnit/generated source, quality and all required product jobs pass.

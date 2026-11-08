# شواهد CI دقیق commit V 3.1.34

- Commit: `40d9128be6d3bd0df3531d44b893946f24e82961`
- Branch: `main`
- Message: `V 3.1.34`
- Step Three quality: [Run 42456934121](https://github.com/artinrahimii/project/actions/runs/42456934121)
- Product build: [Run 42456934059](https://github.com/artinrahimii/project/actions/runs/42456934059)

| کنترل در لحظهٔ ثبت quality result | نتیجه |
|---|---|
| Python quality tests | FAIL — 464 tests, 5 stale source-contract subtest failures |
| Graph and output validation steps | SKIPPED after test failure |
| Android Kotlin lint | PASS |
| Android debug APK | In progress |
| Android JVM tests | Pending |
| Android Keystore instrumentation | Pending |
| Windows WPF build/installer | In progress |
| HostLifecycle suite/coverage | PASS |

The five failures are Python source-contract assertions not updated after Kotlin tests were converted to sequential single-host dispatch. The follow-up aligns generic-vs-product budget markers, escaped tab source literals, the renamed transport tests, and the current runtime variable. Product-build results for this SHA are tracked separately after its workflow completes.


## Product Build نهایی V 3.1.34 — Run 42456934059

| کنترل | نتیجه |
|---|---|
| Windows WPF build/publish/startup/security/installer | PASS |
| HostLifecycle suite and coverage | PASS |
| Android ktlint | PASS |
| Android debug APK | PASS |
| Android release variant with temporary CI key | PASS |
| Temporary release key cleanup | PASS |
| Android merged-manifest permission checks | PASS |
| Android JVM unit tests | FAIL before tests: `:app:compileDebugUnitTestKotlin`, unresolved `InferenceHostAttempt.roundId` at lines 429 and 1002 |
| Android Keystore instrumentation | SKIPPED after compile failure |
| Final debug APK artifact upload | SKIPPED after compile failure |

The JVM methods were not executed. The two references read a nonexistent `roundId` property from `InferenceHostAttempt`; the contract object contains only peer ID, attempt ID, and submission status. The follow-up now checks the actual request/response round IDs without widening the data class. No physical Android-to-Windows model inference or in-place upgrade was performed.

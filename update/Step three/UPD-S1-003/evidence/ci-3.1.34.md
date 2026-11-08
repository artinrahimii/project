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

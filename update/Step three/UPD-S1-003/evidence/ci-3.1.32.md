# شواهد CI دقیق commit V 3.1.32

- Commit: `10303ad6faddd7d4a50096d702d0e9f4a10e2b7d`
- Branch: `main`
- Message: `V 3.1.32`
- Product build run: [42455653927](https://github.com/artinrahimii/project/actions/runs/42455653927)
- Step Three quality gate: [42455653912](https://github.com/artinrahimii/project/actions/runs/42455653912)
- Rendezvous: [42455653892](https://github.com/artinrahimii/project/actions/runs/42455653892)

| کنترل | نتیجه |
|---|---|
| Step Three quality gate | PASS |
| Rendezvous build/smoke | PASS |
| Windows WPF build/publish/startup/installer | PASS |
| HostLifecycle unit/integration/coverage | PASS |
| Android ktlint | PASS |
| Android debug APK | PASS |
| Android release variant (temporary CI key) | PASS |
| Temporary release key cleanup | PASS |
| Merged-manifest minimum permissions | PASS |
| Android JVM unit tests | FAIL — 5 of 214 |
| Android Keystore instrumentation | SKIPPED after JVM failure |
| Android debug APK artifact upload | SKIPPED after JVM failure |

## Failed test analysis

JUnit HTML report artifact `13608070526` was downloaded and inspected. ZIP SHA-256:

`23142a5339df2e98e969b8bf6b6c2738a4bc3095a0803ddc372a2a474d9020cd`

Four transport tests attempted three peers in one `AndroidInferenceRequestRuntime.dispatchRound` and were correctly rejected with `TOO_MANY_HOSTS`. They are rewritten to dispatch exactly one selected peer; the timeout/next-round test waits for the first peer response window to expire before selecting a different peer.

The generic dispatcher test `enforcesThreeDistinctHostsPerRoundAndNineAttemptsAcrossTheRequest` compared its nine generic attempts to the first-release three-attempt constant. It now explicitly requests the generic nine-attempt `MultiHostCompatibilityLimits` budget.

No production runtime cardinality limit is relaxed. No real local model, physical Android handset, external Windows host, or in-place upgrade was exercised by this run.

# T0147 evidence ledger

- **Accepted code revision:** [`V 3.0.534`](https://github.com/artinrahimii/project/commit/a10aebc13e8a5caf7ce27552fd3c970f49c6c820).
- **Step Three quality:** [run 40868883509](https://github.com/artinrahimii/project/actions/runs/40868883509) passed 228 Python tests, syntax, and both graph/evidence guards (229 tasks / 417 edges; 0 errors; one existing T0001 free-text metadata warning preserved).
- **Product workflow:** [run 40868883480](https://github.com/artinrahimii/project/actions/runs/40868883480) passed all three jobs: Android client APK; Windows WPF host; HostLifecycle tests.
- Android: `ktlintCheck`, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin verification and APK upload passed.
- Windows: formatting, Release build, generated Protobuf C# verification, WPF startup/security checks and artifact upload passed.
- HostLifecycle unit/integration tests passed.
- Artifact sizes: `whitep-android-debug-apk` **11,874,698 bytes**; `whitep-windows-host` **14,254,957 bytes**.
- Storage/UI contract: the screen renders only Room-backed `RequestStateEntity` rows and provides an empty state; it neither invents requests nor claims delivery.
- Dependency review: T0146 → T0147 and T0138 → T0147 were validated as sequencing-only; original `blocks` edges retained.
- Failed attempts A001 and A002 remain in the append-only graph history; A003 is the successful attempt.
- **Not claimed:** lifecycle-transition engine, host delivery, response generation or independent-network P2P success.

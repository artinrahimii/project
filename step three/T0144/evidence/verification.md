# T0144 evidence ledger

- Implementation: [`V 3.0.517`](https://github.com/artinrahimii/project/commit/12b76f6f617c982b1353e8e6ec7cb7f5370bb6b3).
- Correction: [`V 3.0.518`](https://github.com/artinrahimii/project/commit/06b8dd63afa37902ea9b834e97abe80458f6375b).
- Formatting correction: [`V 3.0.519`](https://github.com/artinrahimii/project/commit/44c207018fb3c2232730722b59e3cd1249317bbd).
- Quality gate passes on V 3.0.519: 218 tests and graph guard; https://github.com/artinrahimii/project/actions/runs/40852236710.
- Host lifecycle integration passes on V 3.0.519: https://github.com/artinrahimii/project/actions/runs/40852236733.
- Android ktlint on V 3.0.519 fails at `MainActivity.kt:109:29` due to `arguments = listOf(` multiline-expression formatting. The APK build and Android unit tests were skipped.
- Attempt A004 records the narrow formatting fix; all exact-head CI outcomes remain pending.
- Do not mark T0144 complete until Android lint, APK build, and Android JVM tests pass on the same commit.

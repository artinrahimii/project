# T0144 evidence ledger

- Implementation baseline: [`V 3.0.517`](https://github.com/artinrahimii/project/commit/12b76f6f617c982b1353e8e6ec7cb7f5370bb6b3).
- Final tested implementation revision: [`V 3.0.522`](https://github.com/artinrahimii/project/commit/c5775c09b8ec8ddd973bbf4e9ec34d01a6e8efbf).
- Step Three quality gate: [run 40853623895 passed](https://github.com/artinrahimii/project/actions/runs/40853623895). The job log confirms 218 Python tests passed, syntax/graph/output-evidence checks passed, and graph guard reported 229 tasks / 417 edges.
- Android product build: [run 40853623906, Android job passed](https://github.com/artinrahimii/project/actions/runs/40853623906/job/123378982847). The exact job log confirms `ktlintCheck`, `assembleDebug`, `testDebugUnitTest`, generated Protobuf checks and APK upload all succeeded.
- HostLifecycle unit/integration tests: [passed](https://github.com/artinrahimii/project/actions/runs/40853623906/job/123378982515).
- Windows WPF Release build/startup/security/DPAPI checks: [passed](https://github.com/artinrahimii/project/actions/runs/40853623906/job/123378982833).
- A006 replaced Android-only `Uri.encode` with JVM-available UTF-8 path-segment encoding. Exact-head Android unit tests now pass; the earlier route test failure is preserved in the graph's append-only attempt history.
- **Accepted scope:** local conversation creation, rename, delete, lookup and navigation. Independent-network P2P reachability is not tested here; T0121 remains blocked.
- Local execution limitation: GitHub DNS resolution failed for `git ls-remote`; no local build/test success is claimed.

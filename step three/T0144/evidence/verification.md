# T0144 evidence ledger

- Implementation baseline: [`V 3.0.517`](https://github.com/artinrahimii/project/commit/12b76f6f617c982b1353e8e6ec7cb7f5370bb6b3).
- Formatting fixes: [`V 3.0.521`](https://github.com/artinrahimii/project/commit/40aae42f0021b331a976845bc0146ddef1c19b6b).
- Exact-head quality gate on V 3.0.521 passed: 218 Python tests and graph validation; https://github.com/artinrahimii/project/actions/runs/40852830273.
- Exact-head Android lint passed and `assembleDebug` produced the APK.
- Exact-head `testDebugUnitTest`: 16 tests ran, one failed in `ClientDestinationTest.conversationDetailRouteCanCarryAnEncodedConversationId`; the route helper called Android framework `Uri.encode` from plain JVM test code.
- Exact-head HostLifecycle tests and Windows WPF product checks passed.
- Attempt A006 replaces Android-only path encoding with a JVM-available encoder and expands tests for spaces, slash, and plus. Results pending.
- Do not mark T0144 complete until all Android unit tests pass on the same commit.

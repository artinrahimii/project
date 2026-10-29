# T0158 verification evidence

- Base commit: `e4293fb9f5fce5bbb3aa346383ef41cc1055cf90` (`V 3.0.579`), branch `main`.
- Graph source: `step three/project-graph.md`, Blob `8a5041a47a5870815090982dfe049b0dce40c105`; complete original UTF-8 content was byte-counted and its Git Blob SHA-1 recomputed before parsing; 229 nodes / 417 edges.
- Incoming `T0157 → T0158`: validated as `contract_or_interface_dependency` from the actual conflict-review/save contract. Its `blocks` edge remains intact.
- Incoming `T0150 → T0158`: validated as `contract_or_interface_dependency` from the local conversation key, Room relationship, conversation workflow and local-ID versus request-ID requirement. Its `blocks` edge remains intact.
- Outgoing `T0158 → T0159` remains unreviewed.
- Local history rows contain only source, changed field names, timestamp, schema version and byte count. A foreign key to the stored capsule supports cascade deletion; the capsule write and history insert run in the same Room transaction.
- The network `InferenceRequest` Protobuf is unchanged; it contains temporary request/attempt IDs and bounded capsule bytes but no permanent conversation ID or full history.
- The previous revision's Step Three quality gate, Android build/lint/JVM/Protobuf checks, Windows host build and HostLifecycle tests all passed on `V 3.0.579`. The new T0158 exact-head CI has not yet run at report creation.
- No local Android/Gradle/Python run is claimed.


## CI failure and follow-up

- A001 candidate: V 3.0.580, commit 9673db6f13e46c05b289dd2e096eeeb0e4a29eb7.
- Step Three quality workflow [40945024330](https://github.com/artinrahimii/project/actions/runs/40945024330) failed only because test_android_database_migration_contract.py asserted on the local name database from a method that defines test_source; 256/257 Python tests passed.
- A002 correction moves that assertion to the database-source test. No product test outcome is inferred from the Python gate.
- New exact-head quality and product workflows for V 3.0.581 are pending; T0158 remains in progress.


## Exact-head V 3.0.581 test outcomes

- Step Three quality: PASS, 257 tests, [workflow 40945445001](https://github.com/artinrahimii/project/actions/runs/40945445001).
- Android: FAIL at KtLint line 52 for a long `addMigrations` call; APK/JVM/migration steps were not reached, [workflow 40945444883](https://github.com/artinrahimii/project/actions/runs/40945444883).
- Windows WPF and HostLifecycle unit/integration: PASS, [product workflow 40945444883](https://github.com/artinrahimii/project/actions/runs/40945444883).
- A003 fixes the formatting; exact-head V 3.0.582 verification is pending.


## Exact-head V 3.0.582 test outcomes

- Step Three quality: PASS, 257 tests, [workflow 40945957102](https://github.com/artinrahimii/project/actions/runs/40945957102).
- Android: FAIL at KtLint line 91 of `ClientDatabaseMigrationTest.kt`; APK/JVM/migration tests not reached, [workflow 40945957438](https://github.com/artinrahimii/project/actions/runs/40945957438).
- Windows WPF build/startup smoke and HostLifecycle unit/integration: PASS, [workflow 40945957438](https://github.com/artinrahimii/project/actions/runs/40945957438).
- A004 fixes the migration test formatting and source-contract assertion; exact-head V 3.0.583 verification is pending.


## Exact-head V 3.0.583 test outcomes

- Step Three quality: PASS, 257 tests, [workflow 40946486989](https://github.com/artinrahimii/project/actions/runs/40946486989).
- Android: FAIL at KtLint for `Unexpected newline before '.'` in two test builder chains; APK/JVM/migration tests not reached, [workflow 40946486860](https://github.com/artinrahimii/project/actions/runs/40946486860).
- Windows WPF host checks and HostLifecycle unit/integration: PASS, [workflow 40946486860](https://github.com/artinrahimii/project/actions/runs/40946486860).
- A005 corrects chain formatting; exact-head V 3.0.584 results remain pending.


## Exact-head V 3.0.584 test outcomes

- Step Three quality: PASS, 257 tests, [workflow 40947302353](https://github.com/artinrahimii/project/actions/runs/40947302353).
- Android: FAIL at KtLint, `Expected newline before '.'` for `.build()` at `ConversationContextCapsuleViewModelTest.kt:56`; APK/JVM/migration tests not reached, [workflow 40947302434](https://github.com/artinrahimii/project/actions/runs/40947302434).
- HostLifecycle unit/integration: PASS. Windows WPF was completing additional policy checks when inspected.
- A006 corrects chain wrapping; V 3.0.585 exact-head checks are pending.

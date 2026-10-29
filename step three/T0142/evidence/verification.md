# T0142 verification evidence

## Scope

The schema migration and runtime migration test were introduced in V 3.0.507. Exact-head CI exposed two actionable issues before Android compilation/unit tests could run: a stale existing assertion for Room schema version 1, and ktlint formatting violations in the new migration test.

## Recorded evidence for V 3.0.507

- Step Three quality run 40842236474: failed; 211 Python tests ran, one failed on the obsolete version=1 assertion. Structural graph/evidence steps were skipped.
- Android product run 40842236492: failed at ktlintCheck in ClientDatabaseMigrationTest.kt. APK build and Android unit tests were skipped.
- Windows WPF job and host lifecycle job in that product run passed.

## Corrective retry A002

- tests/test_android_local_storage_boundary.py now asserts Room schema version 2 and registration of Migration(1, 2).
- ClientDatabaseMigrationTest.kt was reformatted at the two source locations reported by ktlint.
- Corrective revision and exact-head CI results are pending. No test success is claimed for this retry until results are fetched from GitHub Actions.

## Dependency review

- T0141 → T0142: validated as contract_or_interface_dependency because migration 1→2 updates the concrete version-1 Room schema produced by T0141.
- T0138 → T0142: validated as sequencing_only because the host-visible-content disclosure supplies no Kotlin migration API/schema consumed by this index migration.

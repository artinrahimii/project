# T0142 verification evidence — attempts A001–A003

## Exact revisions and results

- V 3.0.507 / run 40842236474: quality failed because tests/test_android_local_storage_boundary.py still asserted database version 1. The graph validation steps did not run after the test failure.
- V 3.0.507 / run 40842236492: Android ktlint failed; APK and Android unit tests were skipped. Windows WPF and host lifecycle jobs passed.
- V 3.0.508 / run 40843247078: Step Three quality gate passed (tests, Python syntax, graph structure, output/evidence validation).
- V 3.0.508 / run 40843247158: Android ktlint failed at ClientDatabaseMigrationTest.kt lines 86 and 91 and ClientDatabase.kt line 53. Android assembly and unit tests were skipped. Host lifecycle job passed.
- V 3.0.509 / attempt A003: method-chain wrapping and max-line-length corrections are committed; exact-head CI results are still pending.

## Acceptance details

The runtime migration test verifies schema version 2; preservation of legacy conversation, message, request-state status/error code, and draft; and the added composite index. Passing static contract tests alone is not sufficient to close the task.

## Dependency review

- T0141 → T0142: `contract_or_interface_dependency`, validated; schema v1 is the concrete upgrade source.
- T0138 → T0142: `sequencing_only`, validated; the privacy disclosure is context but not a migration API.

# T0027 — execution report

## Dependency review
- `T0026 → T0027`: validated as `sequencing_only`. The secret-ignore rules and sample environment template are not consumed by the build/test/package commands; the conservative sequence remains.
- `T0020 → T0027`: validated as `contract_or_interface_dependency`. README toolchain and command guidance is constrained by ADR-0001, `global.json`, the .NET solution, Android Gradle configuration, and current CI workflow entrypoints.
- Both edge directions and their `type: blocks` values are retained. The outgoing edge `T0027 → T0028` remains unreviewed because it is not needed to begin T0027.

## Attempt T0027-A001 — reproducible commands (partial; CI pending)
- Replaced the duplicated README build notes with explicit prerequisites and commands for .NET restore, formatting, build, lifecycle tests, Windows publish, Android lint/debug APK, Python tests, syntax validation, and graph validation.
- Documented actual limitations: no Gradle Wrapper is present; the host lifecycle test is currently run with `dotnet run`; the Windows publish command is not yet a step in the product workflow; and the Android output is a debug APK, not a signed release package.
- Added `tests/test_readme_build_commands.py` to check documented commands against `global.json`, Android Gradle configuration, and the current GitHub Actions workflows.
- No local execution environment is available through this GitHub editing path. The exact-commit CI runs are the verification source; T0027 remains `in_progress` until they pass.

## Next
Inspect the exact-commit quality gate and product build; only then mark T0027 done.


## Attempt T0027-A002 — restore README contract wording (partial; CI pending)
- Exact-commit quality gate run [39743623233](https://github.com/artinrahimii/project/actions/runs/39743623233) ran 72 tests and failed one pre-existing README contract assertion: `test_readme_contract.py` requires the exact limitation phrase `جایگزین build یا آزمون runtime محصول نیست`.
- The README already communicated the limitation in different words. Restored the explicit existing phrase rather than weakening or deleting the regression test; the new command documentation remains in place.
- T0027 remains `in_progress` until a new exact-commit quality gate and product build pass.


## Attempt T0027-A003 — exact-commit CI verification (succeeded)
- Verified commit: `7169cb200f3b28d12184247e2272388825690c8b` (`V 3.0.84`).
- Step Three quality gate [run 39743813514](https://github.com/artinrahimii/project/actions/runs/39743813514) completed successfully. The graph/Python quality job passed.
- White-P product build [run 39743813489](https://github.com/artinrahimii/project/actions/runs/39743813489) completed successfully. All three jobs passed: Windows WPF Release build, Android Kotlin lint + debug APK build, and host lifecycle unit/integration tests.
- This confirms T0027's README-command contract and the CI/build entrypoints on the exact revision; it does **not** prove runtime inference with a real model, P2P transfer, hardware detection, or release signing.
- Status: `done`, with both prior attempts retained in the graph history. This attempt only records the successful revalidation; it does not claim the README or workflow was originally written in this attempt.


## Post-commit tracker-validation correction — V 3.0.85
- GitHub Actions quality run [39744728550](https://github.com/artinrahimii/project/actions/runs/39744728550) failed only in `Validate recorded outputs and evidence`. The validator requires graph `evidence` entries to be repository file paths; the new T0028–T0030 records mistakenly included Actions URLs in those path-only arrays.
- The task-level build/test results above are based on the successful exact-commit runs. URLs remain in the report/test-result records, while graph evidence arrays are restricted to repository files.
- This correction is included in commit V 3.0.86; its exact-commit quality workflow must pass before this tracker correction is considered verified.

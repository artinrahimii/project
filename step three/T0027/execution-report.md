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

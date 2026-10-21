# T0010 — execution report

## Attempt T0010-A001 — clean-checkout build/test reproducibility

- Started: 2026-10-21T21:18:00Z.
- Base commit: `e1d21d7b80a43230695d639687082d0cf7656e88` (`V 3.0.46`).
- Result: partial; CI for the changed workflow and artifact steps is pending.
- Dependency review: inspected `T0009 → T0010`. T0010 consumes the actual platform project files, toolchain declarations and CI workflow—not a specific output produced by the T0009 central-endpoint guard. The edge remains present and is validated as `sequencing_only`; rationale/evidence are recorded in `step three/project-graph.md`.
- Build-system inspection: `global.json` specifies SDK baseline `10.0.100` with `latestFeature`; CI configures .NET 10, Temurin JDK 17, Gradle 8.13, Android platform 36/build-tools 36.0.0. Existing product workflow builds the WPF host, builds the debug APK, and runs the host lifecycle harness.
- Changes in this attempt: added explicit clean-checkout commands to README; added uploads for the Windows build directory and Android debug APK, both with seven-day retention and `if-no-files-found: error`; added `tests/test_reproducible_build_contract.py` to keep README, CI and artifact paths aligned; recorded acceptance criteria here and in `docs/acceptance.md`.
- Tests: not run locally in this tool session. The exact commit's quality and product-build workflows are pending; no passing result is claimed yet.
- Scope limit: successful compilation/artifacts do not prove the app launches on target hardware, performs real local inference, transfers requests directly P2P, encrypts payloads, or avoids persistent conversation storage.
- Follow-up: inspect the exact commit's quality gate and all product-build jobs, including both artifact-upload steps. Mark T0010 done only after those jobs pass.

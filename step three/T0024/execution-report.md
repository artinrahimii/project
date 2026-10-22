# T0024 — execution report

## Dependency review
- `T0023 → T0024` was reviewed against the Android Gradle project and plugin declarations; retained `type: blocks` and validated as `contract_or_interface_dependency`.
- `T0020 → T0024` was reviewed against ADR-0001; retained `type: blocks` and validated as `contract_or_interface_dependency` because the accepted toolchain baseline is a direct configuration input.
- Both edge reviews and evidence are recorded in `step three/project-graph.md`.

## Attempt T0024-A001 — toolchain version locks
- Audited the existing real configuration: `global.json` selects .NET SDK `10.0.100`; the product workflow selects JDK `17` and Gradle `8.13`; `src/Client.Android/build.gradle.kts` pins Android Gradle Plugin `8.13.0`, Kotlin Android/Compose `2.2.20`, and Protobuf Gradle plugin `0.10.0`.
- Added `test_android_gradle_plugin_versions_are_pinned` to `tests/test_product_build_contract.py` so the exact plugin, JDK, Gradle, and .NET SDK version strings are checked by the automated suite.
- This is configuration drift protection, not byte-for-byte toolchain reproducibility: `global.json` still allows `latestFeature` roll-forward and CI selects the .NET `10.0.x` feature band.
- **Pending:** exact-commit quality gate and product build. T0024 remains `in_progress` until CI verifies the new test.

## Next
Inspect exact-commit CI results. If both workflows pass, record evidence and mark T0024 done. Do not infer runtime or P2P readiness from build success.

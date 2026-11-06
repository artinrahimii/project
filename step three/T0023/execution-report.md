# T0023 — execution report

## Dependency review
- `T0022 → T0023` was reviewed against the actual project references and CI workflow. The edge remains `type: blocks` for conservative ordering but is classified `sequencing_only`: the Android Gradle app and .NET solution are separate build jobs and no direct project reference connects them.
- `T0020 → T0023` was reviewed against ADR-0001 and the checked-in Android build configuration. The edge remains `type: blocks` and is classified `contract_or_interface_dependency` because JDK/Gradle/Android SDK baselines are direct build inputs.
- Both edge reviews and evidence are recorded in `step three/project-graph.md`. No edge was removed.

## Attempt T0023-A001 — Android module verification
- Audited the existing Gradle root settings, Android application plugin configuration, `app` module, launcher manifest, and Kotlin `MainActivity`.
- Confirmed the project includes the `app` Gradle module, Android application plugin, Kotlin/Compose configuration, canonical protocol source directory, and launcher activity.
- The exact V 3.0.68 commit built `src/Client.Android/app/build/outputs/apk/debug/app-debug.apk` in GitHub Actions and uploaded the artifact successfully. See [product build run 39737717682](https://github.com/artinrahimii/project/actions/runs/39737717682).
- The exact commit also passed the graph/evidence quality gate [39737717773](https://github.com/artinrahimii/project/actions/runs/39737717773).
- No Android source file was changed during this verification attempt; the graph's stale expected-output paths were corrected to the actual Android module paths.
- **Status: done** for buildable Android Gradle module scope only. `MainActivity` explicitly states that request transport is not implemented. This is not evidence of working P2P, local inference, or physical-device runtime behavior.

## Attempt T0023-A002 — graph evidence validation failed
- Quality gate run [39738674844](https://github.com/artinrahimii/project/actions/runs/39738674844) for V 3.0.69 failed with `DONE_WITHOUT_TEST_FILE_EVIDENCE`: the node's evidence list did not reference a test source file. This was a graph-record validation failure; the Android APK build on V 3.0.68 had succeeded.

## Attempt T0023-A003 — evidence correction verified
- Added `tests/test_product_build_contract.py` and `tests/test_reproducible_build_contract.py` as file-based evidence and removed workflow-run descriptions from the node's path-only evidence list.
- Quality gate [39738884394](https://github.com/artinrahimii/project/actions/runs/39738884394) passed for exact V 3.0.70 commit [47b2ec3e9320e0231949e9df7d4cc6b86dff0729](https://github.com/artinrahimii/project/commit/47b2ec3e9320e0231949e9df7d4cc6b86dff0729). The graph guard now accepts the test-source evidence and repository-root output/evidence checks pass.
- **Status: done** for the buildable Android Gradle module task. The earlier graph-record validation failure remains in history; it is not concealed.

## Next
Review T0024's incoming edges before changing toolchain locks. Keep Android transport and product runtime acceptance unclaimed.

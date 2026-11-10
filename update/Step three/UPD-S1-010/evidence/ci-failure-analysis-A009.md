# A009 corrective source review — UPD-S1-010

## Exact baseline
- Repository: `artinrahimii/project`
- Branch: `main`
- Baseline: `2d3842e0e3d2b7cc92e07580491dc6cff54e9a1d` (`V 3.1.172`)
- A008 product workflow: <https://github.com/artinrahimii/project/actions/runs/42865909953>

## Finding
A post-edit source review found an error in the A008 import-block replacement: it duplicated `ClientDatabase` and omitted the now-required `InferenceRequestLifecycleState` import. This was caught before any A008 Android build result was treated as successful. The A008 workflow was still in progress when this evidence was written.

## Correction
- Restore the single `ClientDatabase` import.
- Add `InferenceRequestLifecycleState` in sorted order.
- Reformat each Flow chain to one chained call per line, starting the receiver on its own line. No request/runtime or database behavior changed.

## Validation
A009 exact-head workflow is required. Android lint/APK/JVM tests remain pending, and no physical Android-to-Windows inference or in-place upgrade was performed.

# T0087 — Execution report

## Scope and baseline

- Repository: `artinrahimii/project`, target branch `main`.
- Implementation commit: `45fb0ff2bc8f4fa257e8e31fe673ce11f70d7659` (`V 3.0.263`).
- Validation/test-correction commit: `d742b984891c95e80f072641872ec32b58d1cda8` (`V 3.0.264`).
- The baseline graph Blob `628fd950e7f45e61450daa9571859f5d55fcffbf` was retrieved from the exact target commit tree with 1,423,612 UTF-8 bytes; its calculated Git Blob SHA matched.
- Baseline graph count: 229 tasks; 85 `done`, 1 `in_progress`, 143 `not_started`.

## Dependency review

- `T0075 -> T0087`: `contract_or_interface_dependency`, validated with rationale and evidence. The task extends catalog license metadata/parser contract from T0075.
- `T0086 -> T0087`: `sequencing_only`, validated with rationale and evidence. Installer update-safety logic does not technically require license classification, so the original `type=blocks` edge remains unchanged but is not claimed as a hard dependency.
- Other graph edges were not reclassified in this attempt.

## Implementation

The catalog now distinguishes declared license evidence from a declared usage classification (`open_source`, `restricted`, `unknown`). Missing classification defaults to `unknown`; a license URL or legacy `status=verified` alone cannot imply open source. The parser requires declared license identifier/URL evidence and the catalog's verified status before accepting an `open_source` claim, but the UI still states that legal terms are not independently verified. Restricted entries are visibly labelled `RESTRICTIONS REPORTED — NOT OPEN SOURCE`; missing/unknown classification is labelled `REVIEW REQUIRED — DO NOT ASSUME OPEN SOURCE`. Warnings are shown in the catalog list, selected-model details and before model update.

## Changed files

- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs`
- `tests/test_model_catalog_ui.py`
- `step three/T0087/execution-report.md`
- `step three/project-graph.md`

## Validation evidence

- Step Three quality gate passed on validation commit `d742b984891c95e80f072641872ec32b58d1cda8`: https://github.com/artinrahimii/project/actions/runs/40178745808
- HostLifecycle unit/integration tests passed on that commit: https://github.com/artinrahimii/project/actions/runs/40178745804/job/121125036355
- Windows WPF Release build, formatting verification and startup smoke test passed: https://github.com/artinrahimii/project/actions/runs/40178745804/job/121125036232
- Real local llama.cpp/GGUF inference passed on implementation commit `45fb0ff2bc8f4fa257e8e31fe673ce11f70d7659`: https://github.com/artinrahimii/project/actions/runs/40178594183. The subsequent validation commit changed test expectations/report/graph only, not production code.
- The first quality-gate attempt failed because two Python assertions still expected the previous code-behind warning structure. That failure is preserved in `execution_history` and `failed_attempt_count=1`; the assertions were updated to check the new visible disclosure contract and the corrected gate passed.
- The overall product-build workflow also contains an Android APK job that was still running when checked. No Android source was changed, and Android build is outside the acceptance surface of this Windows model-catalog task; the relevant Windows and HostLifecycle jobs are individually successful.

## Outcome

T0087 is complete against its task acceptance criteria. The validation commit's quality gate, host lifecycle tests, Windows build/startup checks passed, and real local inference had passed on the production implementation commit. One failed validation attempt is retained in history.

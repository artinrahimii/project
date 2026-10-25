# T0087 — Execution report

## Scope and baseline

- Repository: `artinrahimii/project`, target branch `main`.
- Implementation commit: `45fb0ff2bc8f4fa257e8e31fe673ce11f70d7659` (`V 3.0.263`).
- Graph Blob SHA at the initial baseline: `628fd950e7f45e61450daa9571859f5d55fcffbf`; its exact UTF-8 byte count matched the tree entry.
- Baseline graph: 229 tasks; 85 `done`, 1 `in_progress`, 143 `not_started`.
- Incoming edges reviewed: T0075→T0087 is `contract_or_interface_dependency`; T0086→T0087 is `sequencing_only`. Both original `type=blocks` values remain unchanged, and the classifications/rationale/evidence are recorded in the graph.

## Implementation commit V 3.0.263

The product change separates catalog license evidence from a catalog-declared usage classification (`open_source`, `restricted`, `unknown`). Missing classification defaults to `unknown`; the parser only accepts `open_source` with declared identifier/URL evidence and the existing `verified` status, while the app still says the claim is not independently legally verified. Restricted models receive a visible `RESTRICTIONS REPORTED — NOT OPEN SOURCE` label and explicit caution in the catalog, selected-model details and update confirmation.

Changed production files:
- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs`

## CI result and correction

- The Step Three quality gate failed on run [40178594185](https://github.com/artinrahimii/project/actions/runs/40178594185), with two failures in `tests/test_model_catalog_ui.py`. Those assertions still expected `RequiresReview` and the old inline warning text in `MainWindow.xaml.cs`; the warning is now centralized in `ModelLicenseMetadata.UsageDisclaimer`.
- This failure is retained as attempt 1 in `execution_history` and `failed_attempt_count=1`.
- The Python UI checks are being updated to verify the model-level disclosure properties and their display in the UI, not weakened or removed.
- The real local-inference workflow on commit `45fb0ff2bc8f4fa257e8e31fe673ce11f70d7659` passed: [40178594183](https://github.com/artinrahimii/project/actions/runs/40178594183). The combined product build and C# tests on that commit were still running when this report was revised.
- Task status remains `in_progress`; no full acceptance claim is made until CI passes for the corrected code and test commit.

## Current retry

- Test correction: `tests/test_model_catalog_ui.py`.
- Follow-up commit message: `V 3.0.264`.
- The next attempt is registered as in progress; validation must be read from GitHub Actions before closure.

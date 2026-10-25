# T0087 — Execution report

## Scope and inspected baseline

- Repository: `artinrahimii/project`, target branch `main`.
- Inspected baseline commit: `fb8c6f9f995145e23321897a52a7f32fb0f687f5` (`V 3.0.262`).
- The graph was fetched by Blob SHA `628fd950e7f45e61450daa9571859f5d55fcffbf` from that commit. UTF-8 byte count: 1,423,612; this matched the Git tree entry.
- Baseline graph count: 229 tasks; 85 `done`, 1 `in_progress`, 143 `not_started`.
- T0075 and T0086 were marked done in the baseline. T0075 was validated as a catalog metadata/parser contract dependency. T0086 was reviewed as sequencing-only; its original `type=blocks` edge remains intact.

## Implementation

- Added optional catalog license `classification`: `open_source`, `restricted`, or `unknown`.
- Omitted classification is `unknown`; a license URL or legacy `status=verified` alone cannot make the UI classify a model as open source.
- The parser rejects an `open_source` classification without declared license identifier/URL evidence and catalog `verified` status. The interface still describes that classification as a catalog claim, not independent legal verification.
- The Windows UI renders restriction/unknown disclosures in the catalog list, selected-model details, and before a model update.
- Acceptance assertions cover restricted disclosure, unknown-by-default behavior, unsupported classifications, and open-source classification without evidence.

## Changed files

- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs`
- `step three/T0087/execution-report.md`
- `step three/project-graph.md`

## Validation state

- Baseline graph retrieval/size check: passed; retrieved bytes matched the tree's declared size.
- Local .NET build/test execution: unavailable via the connected GitHub API tool.
- Repository CI: pending on the implementation commit.
- The task remains `in_progress`; no test or CI success is claimed before the relevant job results are checked.

# T0069 verification evidence

## Dependency edges
- `T0068 -> T0069`: `type=blocks`, `dependency_classification=contract_or_interface_dependency`, `validation_status=validated`. The parser enforces the schema-v1 contract; evidence: `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`, `tests/test_model_catalog_schema.py`, `docs/requirements.md`.
- `T0067 -> T0069`: `type=blocks`, `dependency_classification=sequencing_only`, `validation_status=validated`. The parser has an injected set of supported backend IDs but does not consume a factory artifact; evidence: `LocalInferenceBackendFactory.cs`, `IInferenceBackend.cs`, `LlamaCppServerBackend.cs`.

## Implementation and test coverage
- `ModelCatalogParser.cs`: bounded JSON parsing, schema version 1, strict unknown/duplicate property rejection, stable model ID and unique ID enforcement, positive byte size, SHA-256 syntax, GGUF and current backend restrictions, HTTPS URLs, explicit DNS host allowlists, no custom ports/credentials/fragments/IP literals/loopback, verified-license evidence requirement, and `review_required` normalization for missing license.
- `ModelCatalogModels.cs`: typed metadata models; returned metadata is not evidence that a model is downloaded or legally redistributable.
- `ModelCatalogParserAcceptanceTests.cs`: runtime tests for valid input and rejection cases; linked and invoked by `HostLifecycle.Tests`.
- `tests/test_model_catalog_parser.py`: automated contract checks for parser files, schema parity, test harness wiring, and injected allowlist configuration.
- Exact-head CI is pending; no passing test is claimed at this attempt.


## Exact-head CI evidence — T0069-A002
- Quality gate passed: https://github.com/artinrahimii/project/actions/runs/40106803595
- Windows host build and Android APK jobs passed, but the lifecycle test job failed at compile time due to a malformed C# JSON string literal: https://github.com/artinrahimii/project/actions/runs/40106803542/job/120910070094
- T0069 remains partial until the corrected test suite passes on an exact commit.

## T0069-A003
- Corrected the duplicate-root-property JSON fixture's escaped string literal. Verification is pending the new commit's CI.


## Graph validation follow-up — T0069-A004
- The quality gate caught a mismatch between `execution_summary.failed_attempt_count` and `execution_history`. The A002 entry now records the real failed compile attempt, and A003 remains partial pending a passing CI run.

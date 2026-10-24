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

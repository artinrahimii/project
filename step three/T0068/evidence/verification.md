# T0068 verification evidence

- Reviewed graph edge `T0067 -> T0068`: retained `type=blocks`; set `dependency_classification=sequencing_only`, `validation_status=validated`; cited `LocalInferenceBackendFactory.cs`, `IInferenceBackend.cs`, `LlamaCppServerBackend.cs`, and REQ-007.
- Source schema: `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`; version marker `schemaVersion=1`; Draft 2020-12; fields are stable ID, name, GGUF format, positive sizeBytes, SHA-256 checksum, supported backend list, HTTPS source URL, and optional license review metadata.
- The current schema describes only `GGUF` and `llama.cpp`; it does not claim other engines are supported.
- The schema is declared as Windows host Content and copied to build/publish output.
- Regression source: `tests/test_model_catalog_schema.py`. CI result pending. No successful test is claimed at this attempt.
- This task creates a schema, not a model catalog dataset. Actual source allowlisting and parser behavior belong to T0069.

## T0068-A002 — exact-head verified evidence

- Quality gate: [run 40106114209](https://github.com/artinrahimii/project/actions/runs/40106114209), completed `success`; 136 Python tests passed, including schema/field/constraint/packaging tests; syntax, graph and output-evidence checks passed.
- Windows host build and lifecycle tests: [product run 40106114148](https://github.com/artinrahimii/project/actions/runs/40106114148), both jobs completed `success`.
- The JSON schema file is copied into build/publish outputs by the Windows project file. The schema documents metadata constraints only and does not assert a real model catalog is bundled.
- T0068 is complete for its stated schema/packaging scope; runtime parser behavior remains the next task T0069.

# T0068 execution report

- Attempt: T0068-A001
- Result: partial; exact-head CI pending
- Scope: produce a versioned model metadata schema covering stable ID, name, format, byte size, checksum, backend compatibility, source, and license metadata.
- Dependency review: reviewed `T0067 -> T0068` against `LocalInferenceBackendFactory`, `IInferenceBackend`, `LlamaCppServerBackend`, and REQ-007. The factory currently exposes `llama.cpp` through the application contract, but T0067 creates no catalog/schema artifact and the factory consumes none. The `blocks` edge is preserved and classified as validated `sequencing_only`; no edge was deleted or reordered.
- Implementation: added `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json` using JSON Schema Draft 2020-12. Version 1 is deliberately limited to the implemented GGUF/llama.cpp path. Checksum is an explicit `sha256` object with 64 hexadecimal digits; source and license links require HTTPS at schema level; verified licenses require an identifier and URL.
- Packaging: the Windows project copies the schema to build and publish output. It is a schema only; no sample model or unverified catalog row has been fabricated.
- License rule: the license field is optional so the future parser can label missing metadata as `review_required`; omission is not interpreted as permission. T0069 still needs to implement parser normalization, duplicate-ID checks, trusted source URL allowlisting, and actual runtime validation.
- Tests: added `tests/test_model_catalog_schema.py` for schema structure/constraints, requirements traceability, and project packaging. Exact-head CI is pending.
- Do not mark done until the quality gate and Windows build have passed.

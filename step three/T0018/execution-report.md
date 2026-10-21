# T0018 — execution report

## Attempt T0018-A001 — real local llama.cpp HTTP backend

- Status: implementation committed for CI validation; task remains in progress until exact-commit CI passes.
- Dependency review:
  - `T0017 → T0018`: validated as `contract_or_interface_dependency`. The backend uses the existing local inference boundary and must align with the shared request's model, prompt, and output-token fields; evidence: `protocol/whitep/v1/messages.proto`, `tests/test_proto_contract.py`, and the T0017 execution report.
  - `T0010 → T0018`: validated as `sequencing_only`. T0010's clean-checkout workflow is a quality gate, not a runtime artifact consumed by the backend; evidence: `README.md`, `.github/workflows/product-build.yml`, and the T0010 execution report.
- Implementation: added `LlamaCppServerBackend`, a real HTTP client for a separately running local llama.cpp server. It checks `GET /health`, sends prompts to `POST /v1/chat/completions`, parses generated output, enforces request deadlines/cancellation, bounds output tokens, serializes inference calls per backend instance, and does not log prompt/response content.
- Tests added: controlled HTTP-handler tests check the exact endpoints, request body, response parsing, health failure and invalid-input rejection. These are contract/integration-boundary tests; they do not substitute for a live model run.
- Runtime limitation: no llama.cpp server binary or model is bundled. Live inference and real-model acceptance remain unverified until run against an explicitly installed local server/model.
- Next: inspect exact-commit quality and Windows build/lifecycle-test CI. Fix any failure; only then update status to done.

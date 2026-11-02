# T0176 evidence

## Exact base and graph validation

- Repository/branch: `artinrahimii/project`, `main`.
- Base commit: `6cab4939766b88d42ddce10e05091c8e8c4d66dd` (`V 3.0.806`).
- Graph path: `step three/project-graph.md`.
- Base graph blob: `a8c2b9893a87a447ebbd4162f05da3429c5a4961`.
- Base graph size: 3,877,170 UTF-8 bytes; received content measured 3,877,170 UTF-8 bytes.
- Git object hash: SHA-1 over `blob 3877170\0<original bytes>` exactly matched `a8c2b9893a87a447ebbd4162f05da3429c5a4961`.
- Full fenced JSON parsed; round-trip serialization preserved the JSON payload; 230 nodes and 422 edges were present.
- Before this attempt: 174/230 tasks marked done; T0176 was not_started.

## Reviewed incoming edges

### T0175 → T0176

- Classification: `contract_or_interface_dependency`; validation: `validated`; edge type retained as `blocks`.
- Evidence: RequestViewModel aggregation/status/retry/cancellation code and tests, collector production boundary, T0175 execution report/evidence.

### T0160 → T0176

- Classification: `contract_or_interface_dependency`; validation: `validated`; edge type retained as `blocks`.
- Evidence: InferenceRequest payload builder, dispatcher, protobuf schema, authenticated transport's request/attempt-keyed pending map, response validator, validator JVM tests, T0160 report/evidence.

## Test coverage added or made explicit

- Zero responses: three queued attempts produce three explicit timeout failures.
- One response: one response plus two per-attempt timeout failures.
- Two responses: existing concurrent collector test verifies both outcomes and dispatch-order correlation.
- Three responses: explicit all-success collector test verifies dispatch order.
- Duplicate response: ViewModel refuses a second response for the same request/attempt and retains the first card.
- Late responses: correlation test contract now requires both completion-after-deadline and receipt-after-deadline JVM regressions.
- Timeout determinism: replaces the 20 ms wall-clock race with an explicit `TimeoutException`.
- Cancellation: existing real ViewModel cancellation regression remains part of the required JVM contract.

## CI status

Pending exact-head `V 3.0.807` GitHub Actions. No test execution or pass is claimed in this evidence revision.

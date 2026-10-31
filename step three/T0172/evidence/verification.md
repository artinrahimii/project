# T0172 — verification evidence

## Reviewed base and file integrity

- Branch: `main`
- Base commit: `7de8c6983e9e423d36143d5a65fedb156df72c63` (`V 3.0.728`)
- Graph path: `step three/project-graph.md`
- Tree Blob ID and computed Git Blob SHA-1: `7050e4c0d521e800697aef401539c690f0d76c8c` — matched.
- Original UTF-8 content: 3,474,333 bytes; text length: 3,350,646 Unicode characters.
- Parsed structure: 229 task nodes and 417 dependency edges.

## Dependency evidence

- T0171 produces cumulative peer-attempt history in `DistributedRequestProgress`; the response-correlation transport/validator do not read that history. `T0171 → T0172` is therefore conservative execution order (`sequencing_only`).
- T0160 creates the canonical `InferenceRequest.request_id` and absolute `deadline_unix_ms` through `InferenceRequestPayloadBuilder`. The transport uses the request/attempt pair for pending-response lookup, and the validator enforces request/attempt identity and deadline. `T0160 → T0172` is a `contract_or_interface_dependency`.
- Both original `type=blocks` values are retained. Only classification, rationale, evidence and validation state are updated.

## Acceptance behavior and regression coverage

- Existing production validator rejects mismatched request/attempt IDs, stale completion timestamps and local receipt after the absolute request deadline.
- The transport ignores a response whose request/attempt pair is absent from the pending map; the exact pending entry is removed when `awaitResponse` returns, fails or times out.
- New JVM test: a previous request's response is rejected even though its completion and local receive times are inside the newer request's wider deadline.
- New ViewModel test: the stale response cannot create a card for the new expected request.
- Python source-contract guard verifies request/attempt match, pending-map key/removal and the two regression tests.

## Execution boundary

- No production code was changed because T0165 already implemented and CI-verified the fail-closed response path.
- T0172 remains `in_progress` until exact-head CI executes the new Kotlin regressions and the Android/Python quality checks.

# T0172 — execution report

## Status

`in_progress` pending exact-head CI. The production late-response protection already existed and was verified under T0165; this task does not duplicate or weaken it. T0172 adds explicit task-specific regression coverage for a response from a prior request arriving during a new request's time window and hardens the Python source-contract gate.

## Dependency review

- `T0171 → T0172`: reclassified as `sequencing_only`. T0171's cumulative peer-attempt history and retry allowlist are not consumed by response correlation. The conservative graph order and original `type=blocks` value are retained.
- `T0160 → T0172`: reclassified as `contract_or_interface_dependency`. T0160 produces the canonical `InferenceRequest.request_id` and absolute `deadline_unix_ms`; T0172 relies on these request-scoped values to prevent late/stale response cross-correlation.
- Both classifications are grounded in the payload builder, transport, validator and existing T0165 evidence, not inferred from the `blocks` label.

## Existing production behavior confirmed

- `Libp2pInferenceRequestTransport` keys pending work by `(requestId, attemptId)`. A response without a matching pending pair is ignored.
- On receipt, the transport checks the authenticated peer and calls `InferenceResponseCorrelationValidator.matches` with expected request, attempt, round, peer, model and the original absolute deadline.
- The validator requires both remote `completedAtUnixMs` and local `receivedAtUnixMs` to be no later than the request deadline, and exact request/attempt identity.
- `awaitResponse` removes the exact pending entry in `finally` and cancels an unfinished deferred response. The ViewModel independently refuses a response whose request ID differs from the expected request ID.
- These production checks were previously delivered and CI-verified under T0165. No production code change was needed here.

## Changes in this task

- Added an Android validator test: a prior request's response has old request, attempt and round IDs, but its completion and receive times still fall within a newer request's deadline. The newer request must reject it.
- Added a ViewModel test: a stale response from the previous request must not create a response card when a new request ID is expected.
- Extended the Python source-contract suite to assert exact request/attempt correlation, pending-map lookup/removal and the new regression-test names.
- Actual test paths follow the repository's existing `src/Client.Android/app/src/test/...` layout; the graph's generic `src/Client.Android/Requests/ + tests/` path is descriptive, not an existing directory.

## Changed files

- `src/Client.Android/app/src/test/java/com/whitep/client/network/InferenceResponseCorrelationValidatorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`
- `tests/test_android_inference_response_correlation_contract.py`
- `step three/T0172/execution-report.md`
- `step three/T0172/evidence/verification.md`
- `step three/project-graph.md`

## Verification

- Exact `main` head and graph were inspected at `V 3.0.728`.
- The 3,474,333-byte graph Blob's computed Git SHA-1 matched the tree Blob ID; all 229 nodes and 417 edges parsed.
- Exact-head V 3.0.728 Step Three quality and product workflows succeeded before these regression changes.
- The added Kotlin JVM and Python tests are awaiting exact-head CI. T0172 will not be closed until the new regressions, Android KtLint/JVM checks and Step Three quality gate pass on the implementation commit.

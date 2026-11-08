# ADR-0022: One active host and sequential request retries

- **Status:** Accepted for the first-release Android request path; CI verification is required on the implementation commit.
- **Date:** 2026-11-08
- **Related update task:** `UPD-S1-003`.
- **Affected legacy tasks:** `T0125`, `T0126`, `T0161`–`T0177`, `T0203`, `T0204`, `T0224`, `T0225`.
- **Related proposal:** `update/Step one/2026-11-07-automatic-chat-and-host-routing.md`, section 8.2.
- **Approved requirement source:** `update/Step one/Output/V1.10.json`, `UPD-S1-003`.

## Context

The previous product path and historical tests described up to three simultaneous host submissions in each round and up to nine total host attempts. The updated product proposal recommends one selected processing host at a time, followed by a sequential fallback. This is a product-policy change, not a cosmetic UI adjustment. The existing Android runtime already rejects more than one selected host in its production dispatch path, but the request-domain validation and factory default still allowed a nine-attempt budget. Source-contract tests also continued to assert the old first-release nine-attempt policy, causing CI failure.

The generic selector and dispatcher are still reusable multi-host primitives. Their compatibility behavior is not the same as the product's first-release request policy and must not silently become the request policy.

## Decision

1. **Cardinality:** A first-release logical inference request selects and dispatches at most one host in each round. The prompt is never broadcast to all online peers. Candidate metadata can be enumerated, but the production selector returns only one fresh, authenticated, trusted, ready and model-compatible peer for that request attempt.
2. **Retry budget:** The first-release product request may use at most three sequential host attempts across at most three rounds, with exactly one selected host per round. `RequestViewModel` creates the first-release budget by default; `AndroidInferenceRequestRuntime` rejects dispatches containing more than one selected host and separately enforces the one-host production path. The shared `DistributedRequest` value object also supports the bounded generic compatibility envelope (`MultiHostCompatibilityLimits`: at most three hosts per round and nine attempts across at most three rounds) so the retained generic selector/dispatcher/transport tests remain valid. That generic allowance does not authorize multi-host dispatch by the first-release product runtime. The attempt history is retained in order; a previously attempted host is excluded from later product rounds.
3. **Retry initiation:** Retry remains explicit and user-triggered. There is no automatic retry loop. A retry preserves the logical `requestId`, creates a fresh `attemptId` and `roundId`, and consumes the original request's remaining round/host budget and original absolute deadline.
4. **Timeout and disconnect:** Each attempt uses the existing round timer bounded by the logical request deadline. The Android transport removes the matching pending attempt and closes its stream when the response wait terminates. On Windows, each accepted `(requestId, attemptId)` has its own cancellation token; disconnect and host lifecycle shutdown cancel and drain the corresponding handler. The next host is not dispatched concurrently with a still-active previous attempt.
5. **Late, duplicate or mismatched response:** The existing correlation validator continues to bind a response to its exact request, attempt, host, model and round. A response that arrives after its wait window cannot be reassigned to a later attempt; duplicate or mismatched response identifiers are rejected.
6. **Cancellation:** User cancellation sends the existing correlated `CancelRequest` for the exact currently queued attempt. Cancellation is terminal for the logical request; a cancelled request cannot be retried. If the request deadline or peer disconnect prevents a cancellation response, the result remains unconfirmed rather than being reported as a successful cancellation.
7. **Host-side capacity:** The Windows host keeps its existing non-queueing, single-active-inference admission gate. A busy host returns the existing typed Busy response and is not treated as ready for an additional concurrent inference.
8. **Compatibility boundary:** `MultiHostCompatibilityLimits`, `HostCandidateSelector.select` and the generic dispatcher/collector retain their three-host batching behavior for existing unit/compatibility coverage. They do not authorize the first-release product runtime to dispatch more than one host. Product runtime enforcement remains in `AndroidInferenceRequestRuntime` and is tested independently.
9. **Scope of historical tasks:** The original completion histories of three-host selector/collector/dispatcher primitives are preserved. Product-level requirements and acceptance scenarios that explicitly assume three concurrent first-release hosts, multiple responses in one round, or nine first-release attempts require re-review and migration to optional/future compatibility scope. General correlation, deadline, timeout, late-response, idempotency and trust safeguards remain required.

## Rejected alternatives

- **Three simultaneous first-release hosts:** rejected because it conflicts with the updated single-host product proposal and creates unnecessary prompt disclosure, duplicate inference load and ambiguous cancellation/response aggregation.
- **Automatic retry:** rejected because it can spend host resources without a new user action and complicates ownership of timeout, late response and cancellation.
- **Unlimited or same-host retry:** rejected because it defeats bounded resource policy and can repeat an inference against a host whose prior operation may have completed ambiguously.
- **Removing the generic multi-host code:** rejected because it is independently reusable, already implemented and covered by compatibility tests; the change is to the first-release product path, not a requirement to delete sound generic primitives.

## Implementation and verification mapping

- First-release limits and request-domain validation: `src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt`.
- Production one-host selection and dispatch gate: `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`.
- Explicit retry, preserved logical request identity, fresh attempt identifiers, exclusion of previously attempted peers, deadline and cancellation UI: `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`.
- Android response timeout/stream cleanup and correlated cancellation: `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`.
- Windows per-attempt cancellation, replay registration, non-queueing request admission and handler cleanup: `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs` and `src/Host.Windows/Requests/HostRequestCoordinator.cs`.
- Request-domain acceptance: `src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt`.
- Request UI sequential retry/cancel acceptance: `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`.
- Generic multi-host compatibility: `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` and `InferenceAttemptDispatcherTest.kt`.
- Cross-layer regression guards: `tests/test_android_distributed_request_contract.py`, `tests/test_android_host_candidate_selector_contract.py`, `tests/test_android_inference_attempt_budget_acceptance.py`, `tests/test_android_manual_retry_contract.py`, and `tests/test_request_retry_budget_boundary.py`.

## Acceptance and evidence boundary

This decision sets a deterministic first-release policy and maps implementation/test sources. It does **not** assert that a physical Android-to-Windows model inference has been performed, that a clean install or in-place upgrade has passed, or that the full product is complete. The exact implementation commit must pass repository Python quality checks, Android Kotlin formatting/build/unit tests, Windows build and host lifecycle tests. Physical device/host and real local-model acceptance remain separately gated by `UPD-S1-020` and `UPD-S1-021`.

# ADR-0007: Single-active-inference admission and safe result delivery

- **Status:** Accepted for the Windows host-side admission/coordinator contract already implemented in source. This decision does not certify end-to-end Android-to-Windows request routing.
- **Date:** 2026-10-27
- **Related task:** T0007 (trace technical decisions to production source, configuration, and executable tests); T0106-T0107 (request admission and failure-path acceptance).
- **Related requirements:** REQ-005 (at most one active inference), REQ-015 (deadlines, cancellation and failure states), and REQ-016 (busy/unsupported-model outcomes) in `docs/requirements.md`.

## Decision

1. Admit no more than one host inference operation at a time. The coordinator uses a non-queueing semaphore acquisition (`WaitAsync(0, cancellationToken)`) and reports a stable Busy outcome when another operation is active.
2. Validate active participation, Ready host/model state, exact requested-versus-ready model identity, verified capacity, and a nonzero, valid, unexpired absolute deadline before invoking inference. Unknown or insufficient capacity fails closed.
3. Snapshot the deadline at the coordinator boundary and re-check admission immediately before inference. Do not return a successful result when cancellation was signalled or when the request deadline expires before delivery.
4. Release the coordinator gate and active cancellation state after completion, cancellation, rejection, or inference failure so a later request can be handled safely.
5. Map host-side outcomes to stable protocol responses: Busy/not-ready to `HostBusy`, model mismatch to `UnsupportedModel`, capacity failures to `RESOURCE_LIMIT`, and malformed/expired requests to typed protocol errors. Responses must not echo internal exception details or untrusted request content.
6. Treat this decision as a host-side admission and response contract only. The decision and its tests do not establish Android-to-Windows application routing, reachability, NAT traversal, or the full P2P acceptance gate; `AC-P2P-01` remains **NOT RUN** until the required end-to-end evidence exists.

## Implementation and executable evidence

- Admission rules and typed rejection reasons: `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`.
- Single-active-operation gate, deadline snapshot/re-check, cancellation and cleanup: `src/Host.Windows/Requests/HostRequestCoordinator.cs`.
- Stable wire response mapping: `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`.
- Admission cases (inactive participation, readiness, model mismatch, unknown/insufficient capacity, invalid/expired deadlines): `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`.
- Busy/non-queueing and cancellation behavior under contention: `tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs`.
- Repeated active admission, expiry, cancellation, inference failure and gate recovery: `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`.
- Response-delivery idempotence and concurrency cases: `tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs`.
- Typed response contract and sanitization checks: `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`.
- Executable harness registration: `tests/HostLifecycle.Tests/Program.cs`.
- Requirement mapping: `docs/requirements.md`; decision register: `docs/decisions/README.md`.

## Evidence boundary

The source and test files above were inspected on exact online `main` commit `f97911baade1d6dfdd7b92ecb50eb0a578e7abea`. The current baseline records the suites as registered in the HostLifecycle executable harness and prior CI passes them; this new ADR-to-source/test traceability guard must still pass on its own exact commit. No application runtime behavior was changed in this increment. Broad T0007 task-wide traceability remains incomplete, and neither this record nor a successful static/source-contract test closes a product runtime acceptance gate.

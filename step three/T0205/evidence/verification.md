# T0205 verification evidence

## Incoming edge validation

1. `T0204 → T0205` — `sequencing_only`, validated. T0204 tests Android-side second-round failover; T0205 tests Windows host-side cancellation. No T0204-specific artifact/API is consumed. Evidence: T0204 report/evidence and the separate `PeerAnnouncementHostLifecycle.cs` server path.
2. `T0195 → T0205` — `contract_or_interface_dependency`, validated. T0195 verifies `HostRequestCoordinator` cancellation and gate-release contracts that this end-to-end cancellation flow reuses. Evidence: T0195 report/evidence, `HostRequestCoordinatorFailureAcceptanceTests.cs`, `HostRequestCoordinatorTests.cs`, and `HostRequestCoordinator.cs`.

Original edge `type: blocks` values and conservative order are unchanged.

## Runtime contract under test

The test composes production components: authenticated `PeerAnnouncementHostLifecycle`, `HostRequestCoordinator`, `HostInferenceSession`, and `LlamaCppServerBackend`. A test-only `HttpMessageHandler` supplies the local backend's health response and blocks the active generation HTTP request until its cancellation token is signalled.

Acceptance assertions:
- the authenticated host receives the inference request and forwards it through coordinator/session/backend;
- the fake HTTP backend confirms that `POST /v1/chat/completions` is active;
- a matching authenticated `CancelRequest` (same request ID and attempt ID) is accepted;
- cancellation reaches the HTTP handler and that generation operation exits due to its cancellation token;
- the peer receives a correctly correlated `CANCELLED` response with empty output, not a late success;
- the HostRequestCoordinator releases its single-request gate.

## Backend limitation

The test proves cancellation of the host's HTTP request operation. It does not prove the separate llama.cpp server process stops internal CPU/GPU work after the client request is aborted; this backend exposes no remote compute-cancellation contract.

## Execution state

Code and evidence are prepared in attempt `T0205-A001`. Runtime verification is pending. T0205 remains `in_progress`.

## First CI compile result

Product Build run `41915532966`, Host Lifecycle job `126874055063` failed before runtime execution with CS0104: `HostStatus` was ambiguous between the domain model and the Protobuf contract after a broad domain namespace import. The attempt is recorded as failed; no cancellation behavior pass is claimed. Attempt `T0205-A002` uses explicit type aliases for the two needed domain types.

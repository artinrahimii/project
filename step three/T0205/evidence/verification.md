# T0205 verification evidence

## Incoming edge validation

1. `T0204 → T0205` — `sequencing_only`, validated. Android-side round-two failover and Windows-side incoming cancellation are separate acceptance paths; T0205 consumes no T0204-specific output/API.
2. `T0195 → T0205` — `contract_or_interface_dependency`, validated. T0195 verifies the request coordinator's cancellation and operation-gate behavior used by this test.
The original edge `type: blocks` values and conservative execution order are unchanged.

## Acceptance coverage

The updated authenticated loopback scenario composes the production peer lifecycle, host request coordinator, host inference session and llama.cpp server backend adapter. Its test-only HTTP handler:
- returns a successful `/health` response;
- captures the active `POST /v1/chat/completions` operation;
- signals when the supplied cancellation token is cancelled;
- signals when the generation operation exits due to cancellation.

The acceptance test sends a matching remote `CancelRequest` and asserts:
- the HTTP generation operation observed cancellation and stopped at the handler boundary;
- the response is correlated to request ID, attempt ID, round ID and authenticated host identity;
- response outcome is `CANCELLED`, output text is empty and no late successful output is delivered;
- the coordinator's single-request gate is released.

## Exact-head evidence

Commit `a3e50e03f105e54ad01a0d97d04b9436ebcb6dbd` (`V 3.0.940`):
- Product Build run `41915992536`, Host Lifecycle job `126875674392`: succeeded. `PeerAnnouncementClientAcceptanceTests.RunAsync` completed, so the updated authenticated host lifecycle/cancellation assertions ran without failure.
- Step Three Quality Gate run `41915992559`, job `126875673317`: succeeded.

Attempt A001 is retained as a compile failure (CS0104 ambiguous `HostStatus`); A002 uses explicit domain type aliases and passed CI.

## Limitation

This test confirms cancellation of the host-side HTTP request and suppression of late response delivery. It does not establish that a separate remote llama.cpp server process stops its internal CPU/GPU computation after the HTTP client aborts.

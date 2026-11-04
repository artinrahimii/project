# T0204 verification evidence

## Incoming edge validation

1. `T0203 → T0204` — `contract_or_interface_dependency`, validated. The existing Android/.NET peer-process test and its fixture are reused and extended. Evidence: `step three/T0203/execution-report.md`, `step three/T0203/evidence/verification.md`, `Libp2pInferenceRequestTransportTest.kt`, `AndroidInteropPeerServer.cs`, and `AndroidInferenceRequestRuntime.kt`.
2. `T0195 → T0204` — `sequencing_only`, validated. The second-round scenario uses the separate request-round timer, response collector, candidate selector and authenticated transport; it does not consume T0195 output. Evidence: `step three/T0195/execution-report.md`, `step three/T0195/evidence/verification.md`, `InferenceRoundTimer.kt`, `InferenceRoundResponseCollector.kt`, and `HostCandidateSelector.kt`.

The original `blocks` edge values and the conservative execution order remain unchanged.

## Test design / expected evidence

- Three distinct .NET peer processes authenticate and receive the first-round inference request.
- Each first-round peer deliberately withholds its response while remaining connected; the Android runtime's shared round window expires.
- The response collector must return zero responses and three `RESPONSE_TIMED_OUT` failures.
- The first three processes are stopped. Three new .NET peers start with fresh PeerIds.
- `HostCandidateSelector.selectNextRound` must select those three new identities and exclude every first-round identity.
- Round two is dispatched through `AndroidInferenceRequestRuntime`; three successful responses must match request ID, model ID, host identity and unique attempt IDs.
- Dispatch progress must finish at two rounds and six total host attempts.

## Execution state

The test code and fixture mode have been added. Exact-head CI has not yet executed, so this file intentionally contains no passing test claim. T0204 is `in_progress`, not `done`.

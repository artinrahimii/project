# T0192 — Verification evidence

## Snapshot integrity
- Branch: `main`.
- Commit inspected: `d6b91bdbaef56806447505be7a66e12765677e1f` (`V 3.0.895`).
- Graph path: `step three/project-graph.md`.
- Tree-selected Blob: `476afdeca2cc22649c734fa5c76992212a877406`.
- Git Blob SHA-1 recomputed over the original fenced UTF-8 payload: exact match.
- Original payload: 3,507,300 UTF-8 bytes and 3,379,842 JavaScript string code units.
- Full JSON parse: 230 task nodes and 422 edges.

## Production paths inspected
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt`: centralized 3 rounds / 3 hosts per round / 9 total hosts and cumulative request progress.
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`: one submit call per host on the timed or untimed branch; pre-dispatch round/total budget checks; timeout records `TIMED_OUT`; no retry scheduler.
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt` and `MainActivity.kt`: retries only from explicit user action, same logical request ID, fresh attempt IDs, preserved counters, failed-host-only selection and terminal cancellation.
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs` and Android `RemoteRequestErrorParser.kt`: bounded Busy hint and typed error mapping; no hidden client-side retry loop.
- `src/Host.Windows/Requests/HostRequestResponseDeliveryGate.cs` and `tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs`: at-most-once response-path handoff; ambiguous send failures are not replayed.
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs` and `src/Host.Windows/Networking/PeerTransportFallbackConnector.cs`: maximum 8 unique direct addresses for connection fallback, distinct from inference request retries.

## Existing executable coverage
- Android JVM: `InferenceAttemptDispatcherTest` covers one submission when individual host submissions reject/throw, 3 rounds/9 unique hosts, round rejection, and an explicitly retried host consuming the remaining custom budget.
- Android request ViewModel tests cover user-triggered manual retry with the same request ID and fresh attempt IDs, success exclusion, and cancellation as terminal.
- HostLifecycle tests cover sequential and concurrent response-gate claims and prohibit retry after ambiguous response-path failure.
- Exact-head outcomes for the new Python guard and product jobs are pending at this revision. They will be recorded only after the corresponding Actions results are inspected.

## Dependency validation
- `T0191 → T0192`: `contract_or_interface_dependency / validated`; the absolute request deadline flows from Android payload creation to the host admission/coordinator boundary. Evidence also includes T0191's exact-head successful regression.
- `T0188 → T0192`: `sequencing_only / validated`; privacy constraints are respected, but no executable artifact from T0188 is consumed.
- Original `type=blocks` values and execution order are preserved.

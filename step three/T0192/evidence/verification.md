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
- Exact-head acceptance on `V 3.0.897`: the five Python guard tests and full 396-test Python suite passed; HostLifecycle, Windows WPF, Android JVM/Keystore instrumentation, APK/permissions/Protobuf, and Rendezvous jobs all passed. See A003 below.

## Dependency validation
- `T0191 → T0192`: `contract_or_interface_dependency / validated`; the absolute request deadline flows from Android payload creation to the host admission/coordinator boundary. Evidence also includes T0191's exact-head successful regression.
- `T0188 → T0192`: `sequencing_only / validated`; privacy constraints are respected, but no executable artifact from T0188 is consumed.
- Original `type=blocks` values and execution order are preserved.


## T0192-A002 — first exact-head CI observation
- Commit `fc53011424092440573a94886a5d404c87c7d064` (`V 3.0.896`) ran the complete Python suite: **396 passed**, including all five tests in `test_request_retry_budget_boundary.py`; `compileall` passed.
- The exact-head graph guard then failed with one metadata error, `DONE_WITHOUT_TEST_FILE_EVIDENCE: T0191`, because its existing HostLifecycle acceptance source was absent from T0191's graph `evidence` array. The source path existed in `actual_output_paths`, and the test itself was green. The unrelated T0001 unmapped dependency remained a warning.
- The Rendezvous workflow on this exact commit passed. Android and Windows product workflows were still running at the time of this inspection; they are not claimed here.
- This is recorded as **partial**, not success or an implementation failure. The evidence list is being corrected and all exact-head jobs will be checked again.


## T0192-A003 — exact-head CI results

- Tested revision: `main` `a1026fff0b1f9088de167106611f73b4a914e374` (`V 3.0.897`).
- Step Three quality gate: run `41858050167`, job `126671925979` — **success**; 396 Python tests, syntax and graph guard (0 errors / 1 preserved warning).
- HostLifecycle: product run `41858050329`, job `126671926824` — **success**.
- Windows WPF: product run `41858050329`, job `126671926849` — **success**.
- Android: product run `41858050329`, job `126671926817` — **success**, including APK, permission checks, JVM tests, Keystore instrumentation, Protobuf Java/Kotlin verification and artifact uploads.
- Rendezvous: run `41858050036`, job `126671924869` — **success**.
- T0192 accepted as **done**. The underlying runtime was already bounded; the new tests guard its separation of retry scopes and prevent reintroducing hidden retries.

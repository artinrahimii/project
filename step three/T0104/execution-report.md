# T0104 execution report

- **Task:** Deliver each request result to the response callback at most once.
- **Attempt:** `T0104-2026-10-26-01`
- **Base:** `main` at `3696e6179aede840fd69112ffaaabdb2ceb5be0f` (`V 3.0.320`).
- **Prerequisites:** T0097 and T0103 are marked done with recorded evidence; the current coordinator and cancellation test paths were inspected before the change.

## Dependency review

- Reviewed `T0103 -> T0104`. Kept `type=blocks` and classified the relation as `sequencing_only`: cancellation signalling/post-callback checks and the one-shot response callback gate are distinct outputs, and the gate does not consume a T0103 API/data artifact. The existing ordering is preserved.
- The edge evidence now includes the coordinator, coordinator acceptance tests, engine cancellation test, and the one-shot gate implementation/tests.
- The edge `T0104 -> T0105` remains `unreviewed`; it was not needed to implement or validate this task and must be reviewed before starting T0105.

## Implementation

- Added `HostRequestResponseDeliveryGate` in `src/Host.Windows/Requests/`. It requires a non-empty request ID and uses an atomic claim so only one call through the same request-scoped instance can invoke the response callback.
- Cancellation already requested before handoff throws before claiming the gate. Once a callback claims it, the claim is not reset if the callback throws: the side effect may have occurred before the exception was observed, so retry could duplicate a response.
- Added `HostRequestResponseDeliveryTests` and registered it in the real HostLifecycle test harness. The test project explicitly links the actual production gate source file.
- No protobuf fields or wire numbers changed.

## Scope limitation

This is an in-process, request-lifecycle response-callback guard. Callers must create one gate for a request and share that instance across every terminal response path. It does not deduplicate across separately-created gates or process restarts, and it does not prove exactly-once network delivery. The inspected repository still has no production P2P ingress/response-sender path to wire end-to-end; that limitation is not represented as completed transport work.

## Validation state

- Implementation and acceptance tests are committed with task state `in_progress` pending exact-head GitHub Actions.
- Test cases: sequential duplicate, 64-way concurrent race, callback failure with ambiguous side effects, pre-cancelled handoff, and invalid request ID.
- Local .NET execution is unavailable in the current tool environment; CI results, not an unrun local command, will determine completion.

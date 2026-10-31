# T0168 — round response collection evidence

## Incoming dependency validation

- `T0167 → T0168` is `sequencing_only`: the collector gathers correlated transport responses and has no runtime dependency on the T0167 Compose renderer. The original `blocks` edge is retained for conservative ordering.
- `T0160 → T0168` is `contract_or_interface_dependency`: request ID, per-host attempt ID and the absolute deadline flow from `DistributedRequest` through `InferenceAttemptDispatcher` and the payload builder into the pending-response correlation boundary.
- Both edges were validated in the `V 3.0.701` graph and retain their `type: blocks`.

## Production implementation

- `AndroidInferenceRequestRuntime.awaitRoundResponses` obtains the round window stored for each queued attempt and sends all attempts to `InferenceRoundResponseCollector`.
- The collector starts one coroutine per queued attempt, passes the remaining shared round duration to the runtime's existing `awaitResponse`, and waits for all outcomes with `awaitAll`.
- A successful response does not short-circuit the collection. Other queued attempts remain active until they respond or the common deadline expires.
- Peer/attempt failures and response timeouts are isolated per attempt. Caller cancellation is propagated.
- Existing transport-side authenticated PeerId, request ID, attempt ID, round ID, model ID and request-deadline validation remains the only path that returns an `InferenceResponse` to the collector.

## Regression coverage

- `keepsWaitingForOtherHostsAfterTheFirstValidResponseArrives`: verifies the collection stays active after one response until the other response is resolved.
- `oneHostTimeoutDoesNotCancelASecondHostsValidResponse`: verifies an attempt timeout does not cancel sibling response waiters.
- `preservesSubmissionFailuresWithoutWaitingForUnqueuedHosts`: verifies rejected/failed/timed-out submissions are represented without awaiting a response.
- `doesNotStartAnAttemptWhenItsSharedRoundWindowHasExpired`: verifies an expired window does not start another waiter.
- `callerCancellationIsNotMisreportedAsAnAttemptTimeout`: verifies explicit cancellation propagates.
- Python source-contract checks bind the runtime API to the collector and the JVM tests.

## Scope boundary

This task implements round-scoped response collection and its runtime integration. It does not claim a complete UI-triggered end-to-end inference journey or persistent response storage.

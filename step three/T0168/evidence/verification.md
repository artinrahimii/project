# T0168 — response collection verification

- Runtime API: `AndroidInferenceRequestRuntime.awaitRoundResponses`.
- Collector concurrently awaits all submitted attempts on their dispatch-created shared `InferenceRoundTimer.Window`.
- First valid response does not cancel other queued waits; per-host failures/timeouts are isolated; rejected/failed/timed-out submissions are not awaited; parent cancellation propagates.
- Authenticated peer identity and request/attempt/round/model/deadline correlation checks remain in the existing transport.
- Five JVM regressions cover first response continuation, sibling survival after timeout, non-queued outcomes, expired window and caller cancellation.
- V 3.0.707: all 308 Python tests and syntax passed, but graph guard failed because summary counters were 7/6 versus six history records/five failed. KtLint flagged the last long RESPONSE_FAILED call. Windows WPF and HostLifecycle passed.
- V 3.0.708 A007 aligns graph counters with preserved history and reformats that call. Exact-head CI pending.
- Local Gradle cannot be executed in this connected GitHub context; T0168 remains in_progress.

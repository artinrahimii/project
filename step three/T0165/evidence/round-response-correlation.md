# T0165-A001 — round-scoped response correlation

## Source contract review

- `protocol/whitep/v1/messages.proto`: both request and response now carry additive `round_id = 9`; the existing request deadline remains `deadline_unix_ms = 5`.
- Android dispatch generates one ephemeral round ID per invocation. Every host attempt in that dispatch receives the same round ID and its own attempt ID.
- The Android pending-attempt registry binds the expected peer ID, model ID, round ID and absolute deadline. Incoming responses are checked against all five correlation dimensions: request, attempt, round, authenticated host, and model. Completion timestamp and local receive time must both be within the request deadline.
- Windows echoes the request round ID for successful/error responses and replay rejections. The response correlation validator rejects a missing/mismatched round ID or a completion timestamp later than the request deadline.
- The request's round ID is ephemeral request-scoped metadata; it is not a conversation, user or account identifier.

## Test coverage

- Added Kotlin unit coverage for correct correlation, wrong round, completion after deadline and receipt after deadline.
- Updated Android authenticated transport tests, including three independent native .NET peers, to require round-ID echo and one shared round ID across the three attempts.
- Updated Windows security acceptance checks to reject forged round IDs and late response timestamps.
- Updated Python schema and payload-builder guards.

## Verification state

- Source/diff review: completed.
- Local Gradle, .NET and Python execution: not run in this connected session.
- Exact-head Step Three quality and product CI: pending after `V 3.0.689`.
- No task completion or test success is claimed in this evidence record yet.

## Rollout limitation

Older peer implementations do not populate `round_id`. The new Android response path fails closed for such responses. Deploy compatible Windows and Android builds together before expecting cross-version peer inference to succeed.


## A001/A002 exact-head quality correction

V 3.0.689 Step Three quality ran 300 tests; 299 passed and one stale static contract assertion failed because it expected the response host-identity check to remain inside the transport adapter. The implementation now delegates to the dedicated InferenceResponseCorrelationValidator. A002 updates the test to assert the call boundary and check each response field in the validator that owns the logic. This is not a production behavior change. Exact-head V 3.0.690 quality and product CI are pending; no completion is claimed.

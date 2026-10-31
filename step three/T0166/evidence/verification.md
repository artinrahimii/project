# T0166 — response-card verification evidence

## Source contract reviewed

- `InferenceResponseCorrelationValidator.matches` checks request ID, attempt ID, round ID, authenticated host ID, model ID, host completion time and local receive time.
- `Libp2pInferenceRequestTransport` completes the pending response only after the correlation validator passes.
- `InferenceAttemptDispatcher` allocates a fresh attempt ID per selected host while reusing the distributed request ID for the round.
- `InferenceRequestPayloadBuilder` serializes request/attempt/model/deadline and the Protobuf schema defines those fields.

## Implemented state and UI

- ViewModel storage is an in-memory `StateFlow<List<HostResponseCardState>>`.
- Response identity is `(requestId, attemptId)`; two attempts do not overwrite one another, including retries from the same host.
- The ViewModel checks the expected request, attempt, host and model before accepting a response returned by the correlation-validating transport.
- The Compose UI iterates the response list and creates one independent card per entry. Output text is not joined or merged.
- The state is ephemeral and does not persist response bodies.

## Automated checks

- JVM tests added for two independent host responses, duplicate attempt rejection, correlation mismatch rejection and clearing ephemeral result state.
- Python source-contract checks added for correlation checks, unique attempt identity, UI card iteration and test coverage.
- CI result: pending exact-head workflow for this change. No test is marked passed before the workflow reports success.

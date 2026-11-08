# ADR-0023: One complete inference response in the first release

- **Status:** Accepted for the first-release scope.
- **Date:** 2026-11-08
- **Related update task:** UPD-S1-004
- **Related legacy tasks:** T0032, T0107, T0110–T0112, T0147, T0161–T0177, T0200, T0202, T0223
- **Scope:** Decide whether the first release exposes incremental model output or one complete, terminal response.

## Decision

The first release uses **one complete response per inference attempt**. Token-by-token or chunk-by-chunk response streaming is deferred to a later version. The Android UI must not display partial model text or imply that the current peer protocol supports streamed output.

This is a scope decision, not a claim that every part of the first-release request path is complete. The separate Android-to-Windows physical-device acceptance gate remains outstanding.

## Evidence from the current implementation

1. The canonical Protobuf contract defines one `InferenceResponse` carrying the complete `output_text`, a terminal `InferenceOutcome`, completion time, and request/attempt/round correlation fields. It does not define response-chunk, delta, sequence-number, or end-of-stream messages.
2. `ModelCapability.supports_streaming` is retained as capability metadata for compatibility and future negotiation. It does not itself define a streamed response wire contract.
3. The active `LlamaCppServerBackend` reports `SupportsStreaming: false`. Its `ChatCompletionRequest` includes model, messages, and max-token limit, but no streaming request flag.
4. The Windows inference engine returns one `Task<string>` result and passes cancellation through the active generation token.
5. The Android transport waits for one `InferenceResponse` keyed by `requestId` and `attemptId`; before accepting it, the transport checks the authenticated peer, round, model, and deadline. The Android request runtime collects those terminal attempt responses before updating the conversation UI.
6. Cancellation remains an explicit `CancelRequest` associated with the request and attempt. Timeout, cancellation, rejection, and success remain distinct terminal outcomes.

## Required first-release behavior

- Keep the existing versioned Protobuf fields and do not introduce a partial-response message.
- Display normal request progress while discovery, connection, and inference are active.
- Append generated text to the conversation only after a correlated successful terminal response has been accepted.
- On failure, timeout, disconnect, or cancellation, keep the typed non-success status; never render an incomplete body as a successful answer.
- Keep the absolute request deadline and request/attempt/round correlation checks. A response arriving after the deadline or for another attempt must not be accepted.
- Keep cancellation attempt-specific; do not convert cancellation into a successful empty or partial response.

## Deferred streaming scope

A future streaming change requires a separately versioned and cross-platform contract defining at least:

- a unique request/attempt/round correlation on every chunk;
- monotonically ordered chunk sequence numbers and duplicate/out-of-order handling;
- explicit terminal completion and terminal error frames;
- deadline, cancellation, disconnect, retry, and late-chunk behavior;
- bounded frame and aggregate response sizes, backpressure, and safe buffer cleanup;
- equivalent Windows, Android, shared-contract, UI, and automated-test behavior.

Until those conditions are implemented and accepted, an advertised `supports_streaming` capability must not activate a streaming UI or transport path.

## Implementation impact

No Protobuf wire change is needed for this decision. The existing contract and backend behavior are preserved. A regression test records the one-shot boundary so a future change cannot silently introduce streaming without revisiting this decision and its acceptance criteria.

# T0039 — execution report

## Dependency review
- `T0038 → T0039`: reviewed and classified as `sequencing_only`. T0039 consumes the existing Protobuf schema and generated C# types, not the fixture corpus or parser tests created by T0038. The `blocks` edge is retained as conservative execution order.
- `T0030 → T0039`: reviewed and classified as `sequencing_only`. T0030 verifies CI failure propagation but creates no code or schema interface consumed by T0039. Its `blocks` edge is retained as conservative verification order.
- The relevant predecessor evidence is recorded in `step three/project-graph.md`; no unrelated unreviewed edges were changed.

## Attempt T0039-A001 — implementation in progress
- Added a platform-neutral policy in `src/Shared.Contracts/ProtocolMessageLimits.cs` with ceilings for serialized requests, UTF-8 prompt text, context capsules, output tokens, response text, serialized responses, and the current local HTTP adapter body sizes.
- Added schema comments documenting prompt, capsule and response limits without changing Protobuf field numbers or wire types.
- Added a typed `HostInferenceSession.GenerateAsync(InferenceRequest)` path that validates capsule length and serialized request size before calling inference. The string-based path checks prompt and generated response text.
- Changed the actual llama.cpp HTTP adapter to serialize/check request-body size before sending, use `ResponseHeadersRead`, stream at most the configured response-body limit, and reject oversized response text before returning it.
- Added executable C# checks for in-limit and over-limit requests/capsules/prompts/responses and HTTP bodies, plus Python contract guards to keep the policy/schema/docs/runtime trace aligned.
- Added numeric bounds to REQ-014 and the shared-contract boundary README.
- Exact-commit test results are still pending. T0039 remains `in_progress`; this report does not claim the new code passes until CI runs on the implementation commit.

## Tracker quality-gate correction
- `V 3.0.115` product CI passed for the implementation preceding it, but its graph quality workflow failed because CI URLs were entered in the graph's file-path-only `evidence` array. The URL references have been removed from that array and remain linked under task history and reports; repository paths are now used as evidence entries.

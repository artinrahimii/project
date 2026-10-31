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


## Attempt T0039-A001 — failed quality-gate verification retained
- Tested commit: `c0070b88f71e075ce2d03fd15fb88ce716fd1fee` (`V 3.0.116`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39859692462) failed one static traceability assertion in `tests/test_decision_traceability.py`: it required `PostAsJsonAsync("v1/chat/completions"`.
- That assertion was stale after the implementation deliberately switched to `JsonSerializer.SerializeToUtf8Bytes`, pre-send body-size validation, `ByteArrayContent`, `ResponseHeadersRead`, and `ReadLimitedBodyAsync` to enforce the documented body ceilings.
- [Product build](https://github.com/artinrahimii/project/actions/runs/39859692488) had a passing C# host/lifecycle job, including the new runtime bounds harness. Its Android and Windows jobs were still reported as in progress at the last status check. Overall product success is not claimed for A001.

## Attempt T0039-A002 — correction pending exact-commit CI
- Replaced the obsolete `PostAsJsonAsync` assertion with assertions for bounded serialization, request-body validation, `ResponseHeadersRead`, and the bounded response reader.
- T0039 remains `in_progress`. No acceptance is claimed until the full Step Three quality gate and product workflow succeed for the same exact commit.


## Attempt T0039-A002 — failed Windows build retained
- Tested commit: `dccfeca05c0fc49b02fbcaad88937544a614fab3` (`V 3.0.117`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39859947826) passed all 85 Python tests after the decision-traceability assertion was updated.
- [C# lifecycle/bounds test job](https://github.com/artinrahimii/project/actions/runs/39859947626) passed and printed: `PASS: protocol request, capsule, prompt, response, HTTP-body ceilings and pre-inference rejection.`
- The [Windows WPF product job](https://github.com/artinrahimii/project/actions/runs/39859947626) failed compilation at `HostLifecycle.cs` lines 178 and 205: `CS0246 InvalidDataException could not be found`. The whitespace-format check passed. The direct harness has broader implicit usings, so this app build caught the missing explicit namespace import.
- Android was still in progress at the last status check; no complete product build is claimed for A002.

## Attempt T0039-A003 — compiler correction pending
- Added explicit `using System.IO;` to `src/Host.Windows/HostLifecycle.cs`, covering the two reported `InvalidDataException` references.
- T0039 stays `in_progress` until the exact-commit Windows build and required tests pass.


## Attempt T0039-A003 — successful exact-commit verification
- Tested implementation commit: `64b34cfd1b4817449c90049cedbfea904b2ac1e1` (`V 3.0.118`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39860175394): **success**, all 85 Python tests passed, including the new limit-contract tests and corrected decision-traceability assertion.
- [Full White-P product build](https://github.com/artinrahimii/project/actions/runs/39860175367): **completed successfully**:
  - Host lifecycle/contract test job passed, printing `PASS: protocol request, capsule, prompt, response, HTTP-body ceilings and pre-inference rejection.`
  - Windows WPF C# formatting, application build, generated Protobuf source verification and artifact upload passed.
  - Android Kotlin lint, debug APK build, Protobuf round-trip unit tests, generated Java/Kotlin source checks and APK upload passed.
- The explicit `System.IO` import fixed the compiler error from A002. A001 and A002 failures remain in history and are not overwritten.
- **Result:** T0039 is `done` based on these testable code paths and exact-commit evidence. Boundary note: there is no P2P transport frame parser yet, so raw-frame rejection before Protobuf parsing remains necessary when that transport is implemented. The current local HTTP adapter is genuinely bounded.

# T0039 verification evidence

## Acceptance policy and selected ceilings

- Serialized inference request: 16,384 bytes.
- Prompt: 8,192 UTF-8 bytes.
- Context capsule: 4,096 bytes.
- Output tokens: 131,072 maximum (consistent with the existing local backend ceiling).
- Generated response text: 1,048,576 UTF-8 bytes.
- Serialized inference response: 2,097,152 bytes.
- Current local llama.cpp JSON request body: 65,536 bytes; response body: 2,097,152 bytes.

## Implementation evidence

Files changed in the current attempt:
- `src/Shared.Contracts/ProtocolMessageLimits.cs`
- `src/Host.Windows/HostLifecycle.cs`
- `src/Host.Windows/LlamaCppServerBackend.cs`
- `protocol/whitep/v1/messages.proto`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_protocol_limits.py`
- `docs/requirements.md`
- `src/Shared.Contracts/README.md`

The policy validates typed request/response lengths and the live local HTTP adapter enforces prompt, outgoing JSON body, response stream and generated text ceilings. There is no peer-to-peer frame parser yet; future transport work must apply the raw frame limit before parsing the Protobuf message and compare against negotiated capsule limits. Exact-commit test outcomes have not yet been observed and will be appended after CI.


## Attempt A001 result
- [Step Three quality gate run 39859692462](https://github.com/artinrahimii/project/actions/runs/39859692462) failed one old static assertion expecting `PostAsJsonAsync`, which is intentionally no longer used after the adapter gained pre-send request body bounds and bounded response streaming.
- [Product build run 39859692488](https://github.com/artinrahimii/project/actions/runs/39859692488): C# host lifecycle/contract test job passed. Android and Windows were in progress at the last check.
- A002 updates only the stale traceability test to match the actual bounded adapter; exact-commit workflow results are pending.


## Attempt A002 — test and build evidence
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39859947826): passed all 85 Python tests.
- [C# lifecycle/bounds test job](https://github.com/artinrahimii/project/actions/runs/39859947626): passed size-boundary tests for requests, capsules, prompts, responses and local HTTP bodies.
- [Windows WPF product job](https://github.com/artinrahimii/project/actions/runs/39859947626): failed compilation because `HostLifecycle.cs` did not explicitly import `System.IO` for `InvalidDataException`.
- A003 adds the missing import; exact-commit product build remains pending.


## Attempt A003 — successful exact-commit verification
- Code and host fix tested on `64b34cfd1b4817449c90049cedbfea904b2ac1e1` (`V 3.0.118`).
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39860175394): successful, 85 tests.
- [Full product workflow](https://github.com/artinrahimii/project/actions/runs/39860175367): completed successfully.
- Host integration test verified acceptance at the documented ceilings and rejection of oversized requests, capsules, prompts, outputs and HTTP bodies before inference/deserialization.
- Windows WPF build, generated C# contract verification and build artifact upload passed.
- Android lint, debug APK build, shared-wire Protobuf tests, generated Java/Kotlin source verification and APK artifact upload passed.
- T0039 is accepted. The two prior failed attempts remain in history.

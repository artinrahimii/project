# T0040 verification evidence

## Current implementation
- Shared semantic validator: `src/Shared.Contracts/ProtocolMessageValidator.cs`
- Pre-parse raw Protobuf byte checks: `src/Host.Windows/ProtocolMessageParser.cs`
- Runtime typed-request validation before generation: `src/Host.Windows/HostLifecycle.cs`
- Invalid-wire corpus: `tests/contracts/invalid-message-cases.json`
- C# execution tests: `tests/HostLifecycle.Tests/Program.cs`
- Python contract tests: `tests/test_message_validation.py`

## Acceptance cases added
- Valid inference request and host announcement are accepted.
- Missing request fields, zero deadline and zero output-token value are rejected as InvalidMessage.
- Malformed Protobuf zero tag, truncated varint and truncated length-delimited wire fixtures are rejected as InvalidMessage.
- Raw over-limit request and host-announcement bytes are rejected before generated Protobuf parsing with ResourceLimit.
- Valid-but-unsupported version `2.0.0` is rejected as UnsupportedVersion; malformed version text is rejected as InvalidMessage.
- Host identity/capability mismatch, concurrency above the single-request contract and invalid model-output capability are rejected.
- Unspecified response outcome and oversized serialized response are rejected.
- Exact-commit test/build results are pending; this document does not assert they have passed yet.


## Attempt A001 result
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/39861144156): passed.
- [Windows WPF build and generated C# source check](https://github.com/artinrahimii/project/actions/runs/39861144217): passed.
- [C# host harness](https://github.com/artinrahimii/project/actions/runs/39861144217): failed because the intentionally incomplete HostAnnouncement triggered a NullReferenceException before validation.
- [Android product job](https://github.com/artinrahimii/project/actions/runs/39861144217): lint passed; APK steps were pending when this failure was recorded.
- A002 adds a defensive missing-capabilities check. Completion is still pending.

# T0195 — verification evidence

## New low-disk tests

- Source: `tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs`.
- Tests use the existing injectable `availableFreeSpaceBytesProvider`; they do not fill the CI runner's disk or depend on machine-specific free space.
- Initial-install case: available bytes = declared model size − 1. Expected: insufficient-space IOException, zero HTTP requests, no final model and no staging file.
- Update case: available bytes = replacement size − 1. Expected: insufficient-space IOException, zero HTTP requests, unchanged previous-model bytes and no staging/backup file.
- Both tests are invoked by `VerifiedModelInstallerAcceptanceTests.Run()`, already called from `tests/HostLifecycle.Tests/Program.cs`.

## Existing failure-scenario tests

- Internet interruption: `tests/HostLifecycle.Tests/IncompleteModelDownloadAcceptanceTests.cs`.
- Host disconnect/no-send: `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`.
- Inference-server disconnect: `tests/HostLifecycle.Tests/Program.cs` fault-injection path and `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`.
- App close/restart: T0194 ViewModel and encrypted Room/SQLCipher instrumentation tests; exact prior Product run [41882320680](https://github.com/artinrahimii/project/actions/runs/41882320680) passed the Android emulator suite (9 tests, 0 skipped, 0 failed).

## Status

A001 adds the two low-space acceptance tests. Source inspection is not treated as a passing CI result. Exact-head Quality, HostLifecycle and Android CI must pass before T0195 can move to `done`.

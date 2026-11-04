# T0195 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- HostLifecycle job 126773137103 passed: dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release.
- PASS: low-disk model install and update fail before network access.
- PASS: interrupted model downloads remove partial staging files and never expose an incomplete final model.
- PASS: insufficient disk space, HTTP failure, invalid checksum, and cancelled updates preserve the existing model.
- PASS: HostRequestCoordinator cancellation/failure paths release the request gate; host lifecycle shutdown/recovery checks passed.
- Android JVM unit-test task passed in Product Build job 126773137271.
- Android connectedDebugAndroidTest finished nine tests; the Gradle task and Android job completed successfully.
- Product Build run 41886746018 succeeded on the exact head; Quality Gate, Rendezvous and Real local inference workflows also succeeded.

## Source/test paths

- `tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/IncompleteModelDownloadAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/ModelDownloadCancellationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137103
- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137271
- https://github.com/artinrahimii/project/actions/runs/41886746018
- https://github.com/artinrahimii/project/actions/runs/41886746371

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.

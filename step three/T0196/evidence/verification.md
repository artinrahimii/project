# T0196 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- Program.cs lifecycle transition checks exercised all 20 allowed/rejected state pairs and invalid blank messages; the containing HostLifecycle runner completed successfully.
- PASS: low-RAM, low-disk, unsupported-backend, and unknown-capacity acceptance tests.
- PASS: admission rejects inactive participation, unready/mismatched model, unknown/insufficient capacity and invalid/expired deadline before inference.
- PASS: busy admission fails fast, does not queue 32 requests, preserves cancellation and resumes after release.
- PASS: one coordinator admitted exactly one of 64 simultaneous requests; 63 returned Busy and maximum active inference was 1.
- HostLifecycle job 126773137103 succeeded on exact commit a074815979aff371650c5101c514989f42180968.

## Source/test paths

- `tests/HostLifecycle.Tests/Program.cs`
- `src/Host.Domain/HostLifecycle.cs`
- `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorConcurrencyTests.cs`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137103

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.

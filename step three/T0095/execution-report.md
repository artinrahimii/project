# T0095 — Execution report

## Baseline and dependency review

- Target: `artinrahimii/project`, branch `main`, baseline commit `1be0757b6854218bc2196698bd647cf12c47ba45` (`V 3.0.294`).
- The graph was read from `step three/project-graph.md` at this exact commit through the Git Blob API. The original file is 1,526,055 bytes; UTF-8 byte count and recomputed Git Blob SHA `7392e45c7978cdd601412c9011ce96e5d09e7856` both match the exact tree entry. The fenced JSON parsed completely: 229 unique task nodes and 426 edges.
- `T0094 -> T0095` is a validated contract/interface dependency: T0094 owns the approval-aware execution seam; T0095 must run only after confirmation and independently validate the worker request.
- `T0087 -> T0095` was reviewed against `step three/T0087/execution-report.md`, `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs`, and the actual T0095 inputs/outputs. T0087 supplies license classification/display; the process executor consumes no catalog license contract. The retained edge is therefore classified as `sequencing_only`, with `type=blocks` preserved as the conservative ordering constraint. The edge remains present.
- `T0095 -> T0096` remains `unreviewed`; it was not changed for convenience.

## Attempt T0095-2026-10-26-01 — implementation

- Added an injected `IAgentToolProcessExecutor` boundary. The coordinator no longer calls tool implementations inside the host process; the mandatory executor runs only after allowlist validation, argument validation, and any required positive user confirmation.
- Added a shell-free worker launcher. It requires a code-configured absolute existing executable, rejects command shells, passes only trusted fixed arguments on argv, sends tool data through redirected standard input, clears the inherited environment except a small runtime/OS allowlist, redirects all standard streams, and enforces a bounded request/response protocol (64 KiB each).
- Added a worker-side host that independently parses a strict single-request JSON object, resolves the tool through its own explicit registry, validates arguments again, suppresses exception details, and bounds response size. The production WPF worker mode uses an empty registry by default; no shell or system-command tool is registered.
- Added timeout/cancellation handling that kills the entire child-process tree and waits for process termination, plus safe worker failure mapping. On Windows the child must join a Job Object with the active-process limit set to one and kill-on-job-close enabled; inability to attach fails closed. This prevents the worker from launching additional child processes and ensures surviving descendants are terminated with the worker. It is **not** a full Windows AppContainer/restricted-token security sandbox; the worker retains the same OS user identity and filesystem/network permissions.
- Added executable tests for shell-free launch configuration, environment isolation, successful request/response round-trip, oversized-input rejection, shell-path rejection, and timeout/child termination. The Windows test path also exercises Job Object creation/assignment through the normal launch; exact Windows CI is required to verify the native restriction. The existing coordinator acceptance tests were updated to use the process executor and continue covering approval-before-execution and fail-closed behavior.
- Exact-commit validation is pending; the task remains in progress until quality and product CI are verified for the implementation commit.

## Changed files

- `src/Host.Windows/Tools/IAgentToolProcessExecutor.cs`
- `src/Host.Windows/Tools/AgentToolProcessExecutor.cs`
- `src/Host.Windows/Tools/AgentToolWorkerHost.cs`
- `src/Host.Windows/Tools/AgentToolExecutionCoordinator.cs`
- `src/Host.Windows/Tools/IAgentTool.cs`
- `src/Host.Windows/App.xaml.cs`
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/HostLifecycle.Tests/AgentToolExecutionAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/AgentToolProcessExecutorTests.cs`
- `.github/workflows/product-build.yml`
- `step three/T0095/execution-report.md`
- `step three/project-graph.md`

## Pending evidence and follow-up

- Run the exact-commit Step Three quality gate, HostLifecycle tests, Windows Release build/format/startup smoke, and Android build/tests.
- If worker tests or product build fail, preserve the failed run in the graph/history, repair the implementation, and rerun CI before marking T0095 done.
- The test worker uses a fixed test-only registry in the lifecycle harness. Production remains empty-by-default until a shared, explicit trusted tool registration is added; any future side-effecting tool must continue requiring parent approval and worker-side allowlisting.


## Exact-commit validation — attempt A002 (failed)

- Tested commit: `17e7f7ede0f3f2357fb95ae484466f1aa02f45da` (`V 3.0.295`). The Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40280156875.
- The HostLifecycle job did not reach test execution because compilation failed in `src/Host.Windows/Tools/AgentToolProcessExecutor.cs` with compiler error `CS1022: Type or namespace definition, or end-of-file expected` at line 393. The recorded job is https://github.com/artinrahimii/project/actions/runs/40280156905.
- Cause found by inspecting the exact committed file: the nested Windows Job Object helper had been inserted after the enclosing executor class's closing brace. No passing product-test claim is made for this commit.

## Corrective attempt A003 — source structure fix

- Moved `WindowsWorkerJob` inside `AgentToolProcessExecutor`, retaining the one-process Job Object limit and kill-on-close flags.
- The correction is committed separately as `V 3.0.296`; exact-commit quality, Linux HostLifecycle tests, Windows WPF build/startup (including native Job Object assignment), Android build, and local-inference workflow must be checked before T0095 can be marked done.


## Exact-commit validation — attempt A004 (failed)

- Tested commit: `44be76a3f02ea579789cae6bae942c44c2f7f0cd` (`V 3.0.296`). The Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40280367686.
- The Linux HostLifecycle job passed, including the new process-boundary tests and their explicit PASS line: https://github.com/artinrahimii/project/actions/runs/40280367697/job/121432103236.
- The Windows WPF Release build failed because `StreamReader` in `AgentToolProcessExecutor.cs` and `Stream` in `AgentToolWorkerHost.cs` were unresolved (`CS0246`). The failing workflow is https://github.com/artinrahimii/project/actions/runs/40280367697. The Linux lifecycle project had implicit `System.IO` imports, so it did not reveal this target-project compile difference.

## Corrective attempt A005 — explicit IO namespace imports

- Added explicit `using System.IO;` directives to both affected production files. This directly addresses the exact compiler errors; all other policy and worker behavior is unchanged.
- The correction is committed separately as `V 3.0.297`. The Windows WPF Release build, smoke test (including worker-mode startup), Linux process tests, Android build/tests, and quality gate must pass on that exact commit before T0095 can be marked done.


## Corrective attempt A006 — exercise native Windows process restrictions

- Added the `--test-agent-tool-process-executor` entrypoint so the focused process-boundary suite can run without executing unrelated lifecycle tests.
- Added a step to the `windows-latest` product workflow that executes this focused suite. On Windows the normal executor path must create and attach the worker to a Job Object with a one-process maximum and kill-on-job-close; assignment failures fail closed. This validates the native path instead of relying only on Linux tests and Windows compilation.
- The new targeted Windows test step is part of `V 3.0.298`; its exact-commit CI result is pending.


## Exact-commit validation — attempt A007 (failed)

- Tested commit: `9b2540a70fd874da1cd5bddb5a769f12163ef295` (`V 3.0.298`). The Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40280875882.
- Linux HostLifecycle had previously passed on the prior corrective commit; on this commit the targeted `windows-latest` test was reached after a successful Windows WPF Release build and startup smoke, but failed when `SetInformationJobObject` returned false. The failure was `AgentToolWorkerFailedException` in `WindowsWorkerJob.CreateIfSupported` at the job-limit configuration call: https://github.com/artinrahimii/project/actions/runs/40280875990/job/121433660749.
- The test did not establish that the Windows Job Object restriction works. No success is claimed for native worker confinement in this attempt.

## Corrective attempt A008 — native structure marshaling and diagnostic error code

- Changed `SetInformationJobObject` to accept an explicit native `IntPtr` to a separately allocated `JOBOBJECT_BASIC_LIMIT_INFORMATION` structure, populated via `Marshal.StructureToPtr` and freed deterministically. This matches the native API's `LPVOID` contract.
- If Windows refuses the requested limits, the exception now preserves the exact Win32 error code so the next native CI result is diagnostic rather than opaque. The executor continues to fail closed if the restrictions cannot be applied.
- This correction is committed as `V 3.0.299`; do not mark T0095 done until the focused Windows Job Object test and the whole product CI pass on that commit.



## Exact-commit validation — attempt A009 (failed on V 3.0.299)

- Tested commit: `47856531a90a63ff736fe9ed067504913c4cf5a8` (`V 3.0.299`). The Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40281428816.
- The HostLifecycle tests on Linux passed on the exact commit: https://github.com/artinrahimii/project/actions/runs/40281428741/job/121435372882. The Android APK/lint/Protobuf checks also passed in that workflow.
- The Windows WPF Release build, generated Protobuf check and startup smoke test passed; the dedicated Windows process-restriction test failed at `SetInformationJobObject` with Win32 error 87 (`ERROR_INVALID_PARAMETER`): https://github.com/artinrahimii/project/actions/runs/40281428741.
- Root cause found against the native API contract: the implementation set `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` while passing `JOBOBJECT_BASIC_LIMIT_INFORMATION`. Microsoft documents that this flag requires `JOBOBJECT_EXTENDED_LIMIT_INFORMATION`; successful C# compilation was not sufficient evidence of native configuration success. No claim is made that V 3.0.299 passed the Windows restriction test.

## Corrective attempt A010 — use the required extended Job Object structure

- Change: `WindowsWorkerJob.CreateIfSupported` now populates `JOBOBJECT_EXTENDED_LIMIT_INFORMATION` (including the nested basic limits and native `IO_COUNTERS`) and passes information class 9 (`JobObjectExtendedLimitInformation`) to `SetInformationJobObject`. The active-process limit of one and kill-on-job-close policy remain enabled; configuration/assignment still fails closed.
- API reference used to diagnose the mismatch: https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_basic_limit_information and https://learn.microsoft.com/en-us/windows/win32/api/jobapi2/nf-jobapi2-setinformationjobobject.
- Base graph read from commit `47856531a90a63ff736fe9ed067504913c4cf5a8`, path `step three/project-graph.md`, Blob `ce50976115b51c7584e48e3d8e4e51b3f993bc97`; the original UTF-8 size was 1,542,009 bytes and its recomputed Git SHA-1 matched the tree Blob. The complete fenced JSON parsed as 229 unique tasks and 426 edges.
- This commit records the repair as an implementation attempt only. Exact-commit Windows Job Object test, product workflow, Step Three quality gate and other applicable workflows remain pending; T0095 stays `in_progress` until the Windows native check and full product validation pass.

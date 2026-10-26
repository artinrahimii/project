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
- `step three/T0095/execution-report.md`
- `step three/project-graph.md`

## Pending evidence and follow-up

- Run the exact-commit Step Three quality gate, HostLifecycle tests, Windows Release build/format/startup smoke, and Android build/tests.
- If worker tests or product build fail, preserve the failed run in the graph/history, repair the implementation, and rerun CI before marking T0095 done.
- The test worker uses a fixed test-only registry in the lifecycle harness. Production remains empty-by-default until a shared, explicit trusted tool registration is added; any future side-effecting tool must continue requiring parent approval and worker-side allowlisting.

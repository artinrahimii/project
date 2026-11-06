# ADR-0009: Fail-closed, bounded agent-tool execution

- **Status:** Accepted for the current implementation and its executable regression suite; this decision does not claim OS-level sandbox isolation.
- **Date:** 2026-10-27
- **Related task:** T0007 (technical-decision traceability).
- **Related requirement:** REQ-017 in `docs/requirements.md`.
- **Scope:** Model-proposed tools executed by the Windows host.

## Context

The host supports a deliberately narrow agent-tool boundary. A model's proposed tool call is not, by itself, permission to execute a command. Backend capability evidence, an explicit host-owned allowlist, schema validation, and user approval for side-effecting operations govern dispatch.

## Decision

1. **Fail closed on capability evidence.** Tool advertisement and execution use `AgentToolCapabilityPolicy`. A missing, unsupported, stale, future-dated beyond allowed skew, or failed capability report does not enable tools.
2. **Allowlist and validate calls.** `AgentToolRegistry` owns the available tool definitions and validates names and JSON arguments. Unknown names and invalid/oversized payloads are rejected; arbitrary model-selected system commands are not enabled by default.
3. **Require approval for side effects.** `AgentToolExecutionCoordinator` asks the host confirmation service for a tool whose definition requires user confirmation. Decline, cancellation, or a failed confirmation does not dispatch the worker.
4. **Launch a designated worker without a shell.** `AgentToolProcessExecutor` requires an absolute existing executable path, rejects command-shell executable names, uses shell-free argument passing, and sends model-selected names and arguments through standard input instead of command-line arguments. It clears the inherited environment and copies only an explicit small allowlist of variables.
5. **Bound the worker protocol and lifetime.** Requests and responses are capped at 64 KiB, JSON nesting is limited, standard-error collection is bounded, and execution has an enforced timeout. Cancellation/timeout attempts to kill and reap the process tree. On Windows, a job object uses kill-on-close for descendants.
6. **Keep errors stable and avoid leaking worker details.** Malformed worker requests produce a stable rejection response; callers do not receive arbitrary worker exception text as a normal tool result.

## Implementation and verification links

- Requirement: `docs/requirements.md` (REQ-017).
- Capability gate: `src/Host.Windows/Chat/AgentToolCapabilities.cs`.
- Allowlist and argument validation: `src/Host.Windows/Tools/AgentToolRegistry.cs`.
- Policy/confirmation dispatch: `src/Host.Windows/Tools/AgentToolExecutionCoordinator.cs` and `src/Host.Windows/Tools/WpfAgentToolConfirmationService.cs`.
- Bounded child-process launch: `src/Host.Windows/Tools/AgentToolProcessExecutor.cs`.
- Worker-side protocol: `src/Host.Windows/Tools/AgentToolWorkerHost.cs`.
- Executable acceptance and regression checks: `tests/HostLifecycle.Tests/AgentToolExecutionAcceptanceTests.cs`, `tests/HostLifecycle.Tests/AgentToolProcessExecutorTests.cs`, `tests/HostLifecycle.Tests/AgentToolPolicyRejectionAcceptanceTests.cs`, `tests/HostLifecycle.Tests/AgentToolCapabilityAcceptanceTests.cs`, and `tests/HostLifecycle.Tests/AgentToolRegistryAcceptanceTests.cs`.
- Test registration and compilation: `tests/HostLifecycle.Tests/Program.cs` and `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- CI entrypoint: `.github/workflows/product-build.yml`.

## Security limits and evidence boundary

The worker runs as the same operating-system user as the host. This is a bounded process/protocol boundary, **not** an AppContainer, restricted-token sandbox, or a claim that the worker is isolated from all user permissions. Shell-free launch and process-tree cleanup do not prove that every tool is harmless. The current executable tests establish the specific allowlist, validation, confirmation, environment, size, timeout and failure contracts they exercise; they do not constitute a general adversarial security audit.

A passing build or source-contract test does not prove the full product P2P/runtime acceptance gates. `AC-P2P-01` remains outside the scope of this decision and must be verified against its own acceptance criteria.

# T0097 execution report

- **Task:** Enable agent capabilities only when the selected model and backend demonstrably support them.
- **Attempt:** T0097-2026-10-26-01
- **Base:** `main` at `4546363bedbc758d514fddbb0d933e4c89880d5d` (`V 3.0.303`).
- **Graph validation:** `step three/project-graph.md` Blob `4d5b78bccec56c93f0d9d07c3823d91ae46214e1`; full content was 1,562,437 UTF-8 bytes / 1,440,408 Unicode characters. The embedded JSON parsed and computed Git Blob SHA-1 matched the tree-referenced ID.
- **Result:** partial; status remains `in_progress` until exact-commit CI passes.

## Dependency review

- `T0096 -> T0097`: keep `type=blocks`, validated as `sequencing_only`; T0096 contributes regression evidence, not a production backend capability contract.
- `T0087 -> T0097`: keep `type=blocks`, validated as `sequencing_only`; license classification does not prove tool-calling support.
- `T0097 -> T0098...T0107`: all ten `blocks` relations are preserved and reviewed as `sequencing_only`. Those tasks output a separate `Host.Windows/Requests` request coordination/concurrency/deadline/cancellation path and do not consume the new capability-report contract. Conservative order is retained.

## Implementation

- Added `IAgentToolCapabilityProvider` with separate backend, selected-model and structured-protocol support states; `Unknown` is non-permissive.
- Added a centralized fail-closed policy requiring current evidence (maximum five minutes old), nonempty backend/model IDs, and all three support dimensions set to `Supported`. Missing providers, provider failures, unavailable backends, stale/unknown/unsupported reports deny capability.
- The current `LlamaCppServerBackend` is explicitly text-only: it sends user text and parses `choices[0].message.content`; it does not send tool definitions or parse structured `tool_calls`. It reports backend/model support unknown and protocol support unsupported, so the current adapter does not activate agent tools.
- Removed unguarded registry definition exposure and gated model advertisement. The execution coordinator independently checks capability before confirmation or worker launch.
- Added acceptance coverage for missing/unknown/unsupported/stale reports, provider failures, the actual adapter report, advertisement gating, and dispatch blocked before confirmation/worker launch. Wired it into HostLifecycle and a focused Windows CI step.
- No arbitrary-command tool was added, and license metadata is not used to infer runtime capability.

## Validation and limitations

- Source-contract and exact graph Blob checks were performed against the identified base commit.
- **Local .NET tests were not run:** `dotnet` is not installed in this environment. A direct local Git remote check failed because `github.com` could not be resolved; GitHub writes are made using the connected GitHub API.
- No application test is claimed as passed before exact-commit CI results are retrieved. Keep T0097 in progress unless those checks pass.

## Changed paths

- `src/Host.Application/IAgentToolCapabilityProvider.cs`
- `src/Host.Infrastructure/LlamaCppServerBackend.cs`
- `src/Host.Windows/Chat/AgentToolCapabilities.cs`
- `src/Host.Windows/Tools/AgentToolRegistry.cs`
- `src/Host.Windows/Tools/AgentToolExecutionCoordinator.cs`
- `tests/HostLifecycle.Tests/AgentToolCapabilityAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/AgentToolRegistryAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/AgentToolExecutionAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/AgentToolPolicyRejectionAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`
- `.github/workflows/product-build.yml`
- `step three/T0097/execution-report.md`
- `step three/T0097/evidence/verification.md`

## Follow-up

Inspect the exact commit's quality gate, HostLifecycle, Windows WPF and Android results. If they pass, append a distinct successful validation attempt, mark T0097 done with workflow evidence, and commit that status using the next non-repeating version. Preserve this partial attempt if any check fails.

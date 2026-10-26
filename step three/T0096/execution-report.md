# T0096 — execution report

## Attempt T0096-2026-10-26-01 — targeted negative-path coverage added; CI pending

- Baseline: `artinrahimii/project`, branch `main`, commit `d094e2ef2fe4018ad1f775f6f61cf1dd6cd6a37c` (`V 3.0.301`). Version history was re-read before selecting the next implementation commit, `V 3.0.302`.
- Graph source: `step three/project-graph.md` from the baseline commit, fetched using tree-derived Blob `57c82d1ff6d84b765bac914151db07a2145dd891`. Full UTF-8 size: 1552449 bytes; recomputed Git Blob SHA-1 matched; complete fenced JSON parsed to 229 unique tasks and 426 edges.
- Prerequisites:
  - `T0095 -> T0096` is validated as `contract_or_interface_dependency`: T0096 exercises the real isolated worker/executor protocol implemented by T0095.
  - `T0087 -> T0096` is validated as `sequencing_only`: license classification/disclosure does not feed tool-call rejection tests. The existing `blocks` relation was preserved.
  - `T0095` is done on exact-commit CI evidence from `V 3.0.300`; `T0087` is done on previously recorded exact-commit evidence.
- Added `tests/HostLifecycle.Tests/AgentToolPolicyRejectionAcceptanceTests.cs`. It drives the actual test worker process to prove unknown names and schema-invalid arguments are rejected by the worker-side allowlist/validator; checks malformed protocol input is rejected with only the generic `worker_rejected` response; and checks that a side-effecting request declined by the confirmation service launches no process, while an approved request dispatches once to the worker without executing tool code in the parent process.
- Wired the suite into the normal HostLifecycle harness and added a dedicated `windows-latest` product-workflow step so rejection/confirmation behavior is exercised on Windows as well as Linux.
- No production tool or arbitrary shell command was added. New test tools are test-only and perform no external side effects.
- Exact-commit execution is pending; T0096 remains `in_progress` until the quality gate, HostLifecycle tests and Windows product test step pass on this implementation.
- `T0096 -> T0097` remains unreviewed; this test-only change does not alter the downstream capability-gating contract.

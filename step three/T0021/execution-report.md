# T0021 — execution report

## Current tree audit
- `src/Host.Windows/`, `src/Client.Android/`, `tests/`, and `docs/` already contained tracked source/configuration files. They were preserved without duplication.
- `src/Shared.Contracts/` and `tests/contracts/` were absent from the current tracked tree.
- The canonical schema `protocol/whitep/v1/messages.proto` already exists and is referenced by the current protocol contract checks. It was preserved in place rather than copied or moved.
- Incoming edge `T0020 → T0021` was reviewed against the decision register, platform configuration, and local-backend ADR. The edge remains `type: blocks` and in the graph order; its classification is `contract_or_interface_dependency` with rationale/evidence recorded in `step three/project-graph.md`.

## Attempt T0021-A001 — create the missing structural boundaries
- Added `src/Shared.Contracts/README.md` to establish the future shared .NET contract project boundary without prematurely creating a project/solution (owned by T0022) or duplicating proto/generated source (owned by the schema/generation tasks).
- Added `tests/contracts/__init__.py` and `tests/contracts/test_contract_layout.py` so this directory is a discoverable test package and checks the canonical schema/path layout.
- Added a repository-layout section to README identifying the source, protocol, test, and documentation boundaries.
- This is a structural task. The canonical contract schema has not been moved; no generated models or project solution were invented here.
- Exact-commit test and graph validation results remain pending.

## Next
Run the exact-commit quality workflow. Mark T0021 done only after the nested contract tests, graph structure validation, and repository-root evidence validation pass. T0022 remains responsible for .NET solution/project files; T0036 remains responsible for build-time generation.

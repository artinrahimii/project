# T0020 — execution report

## Incoming dependency review
- Reviewed `T0019 → T0020`. The edge remains in execution order with `type: blocks`; its classification is now `contract_or_interface_dependency` because the direct-P2P/rendezvous architecture decision in ADR-0002 is an explicit input to the technology decision register.
- Reviewed `T0010 → T0020`. The edge remains in execution order with `type: blocks`; its classification is now `sequencing_only` because the task records traceability to existing build/test workflows but consumes no build artifact unique to T0010.
- Reasons and path-based evidence are recorded in the edge fields and change log in `step three/project-graph.md`. Neither edge was removed.

## Attempt T0020-A001 — technology decision traceability
- Added `docs/decisions/README.md` as a decision register mapping every current ADR to code/configuration, relevant tests/workflows and the runtime-gate boundary.
- Added `docs/decisions/ADR-0003-local-inference-backend.md` to capture the selected `IInferenceBackend` boundary and the first llama.cpp HTTP adapter. It links the actual session/backend/WPF source and controlled endpoint tests, and explicitly states that live model inference is not proven by the test handler.
- Updated README to link the decision register and ADR-0003.
- Added `tests/test_decision_traceability.py` to check that decision entries link to tracked source/config/test paths, and to prevent the documents from claiming that P2P or live inference runtime gates have passed.
- Task outputs are committed together with this report and the graph. Exact-commit test and graph validation results are pending.
- Scope is traceability/design documentation; no P2P source implementation, model bundling, or hardware discovery has been added.

## Next
Run the exact-commit quality gate. Mark T0020 done only if the new tests and repository-root graph guard pass. The decision register remains honest about runtime gates: `AC-LOCAL-01`, `AC-LOCAL-02`, and `AC-P2P-01` are not satisfied by documentation/source-contract tests.

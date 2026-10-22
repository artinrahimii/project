# T0019 — execution report

## Attempt T0019-A001 — direct P2P architecture decision
- **Status: in progress; exact-commit CI pending.**
- Reviewed incoming graph edges from T0018 and T0010 against the actual task inputs/outputs, REQ-010/REQ-011, README, the local backend implementation, and the current CI workflow. Both retain the existing `type: blocks` relationship but are classified as `sequencing_only`: the architecture decision consumes the requirements directly and does not consume a build artifact or code output specific to either predecessor. The safe execution order is retained.
- Added `docs/decisions/ADR-0002-direct-p2p-data-path.md`: direct P2P is the default inference data path; a rendezvous service is restricted to peer discovery/connection setup and may not relay inference prompts or responses; failure to establish direct transfer must not silently switch to server relay.
- Linked the ADR from README and the acceptance matrix. Updated stale acceptance statements so they reflect the real local llama.cpp HTTP adapter and Windows UI, while preserving the fact that live inference and P2P runtime acceptance remain unverified.
- Added `tests/test_p2p_data_path_decision.py` to check consistency between the architecture decision, REQ-010/REQ-011, README, and the release-acceptance matrix. This is a source-contract test, not a P2P runtime test.
- Changed files: `README.md`, `docs/acceptance.md`, `docs/decisions/ADR-0002-direct-p2p-data-path.md`, `tests/test_p2p_data_path_decision.py`, `step three/T0018/execution-report.md`, `step three/T0019/execution-report.md`, and `step three/project-graph.md`.
- Test command to be verified on the exact commit: `python -m unittest discover -s tests -v`; the code and evidence are committed in the current batch, but no success is recorded before the CI result is observed.
- **Limit:** no P2P transport, rendezvous implementation, packet inspection, or device-to-device test has been added by this architecture task. AC-P2P-01 remains NOT RUN.

## Next
Verify the exact-commit quality and product workflows. Only record this task as done after its decision-contract test and graph guard pass. Later P2P implementation and product runtime acceptance remain separate tasks.


## Attempt T0019-A002 — CI regression found in source-contract test
- The quality gate for commit [3428fe8425c7b3b0d6bf0ad2d79bf55501d82cb5](https://github.com/artinrahimii/project/commit/3428fe8425c7b3b0d6bf0ad2d79bf55501d82cb5) failed in run [39735575767](https://github.com/artinrahimii/project/actions/runs/39735575767). Of 54 tests, one new assertion failed because it expected the English word `transport` in a README section written in Persian. The architecture test did not fail on P2P policy; the assertion was inconsistent with the file's actual language.
- The product-build run [39735575690](https://github.com/artinrahimii/project/actions/runs/39735575690) was still in progress when inspected; no result is claimed here.
- Corrected the source-contract test to assert the actual Persian statement (“انتقال P2P … هنوز موجود نیست” and “این تصمیم هنوز پیاده‌سازی نیست”) rather than requiring English wording. This is a test-fixture correction; no transport implementation or P2P runtime evidence is implied.
- **Status remains in progress** pending the new exact-commit quality and product workflows.

## Next
Run the exact-commit quality gate and product build for the correction. Keep `AC-P2P-01` NOT RUN until direct multi-process/device transfer and payload-path inspection are implemented and verified.

# T0018 — execution report

## Attempt T0018-A001 — local llama.cpp backend
- Implemented a real HTTP client for `GET /health` and OpenAI-compatible `POST /v1/chat/completions`, bounded output tokens, deadlines/cancellation, and per-instance request serialization.
- Product-build run [39681349154](https://github.com/artinrahimii/project/actions/runs/39681349154) passed for the backend implementation commit.
- Review found that the backend class existed but the Windows UI still displayed a preview-only message and did not call it.

## Attempt T0018-A002 — connect the backend to the Windows host UI
- **Status: partial; exact-commit CI pending.**
- Added `HostInferenceSession` for connection/readiness, busy/ready/error transitions, and local generation.
- Replaced the preview-only WPF window with controls for local server URL, model ID, health connection, prompt entry, and generated output.
- Extended the backend interface with health and generation methods and added host-session lifecycle tests.
- Updated README to remove the stale statement that the UI was not connected to live inference.
- Dependency review remains unchanged: `T0017 → T0018` is a validated contract/interface dependency; `T0010 → T0018` is sequencing-only.
- **Runtime limitation:** llama.cpp and a model must be installed separately. CI verifies build and controlled tests, not a live inference session on a target machine.
- **Prior CI issue:** quality gate run [39681738413](https://github.com/artinrahimii/project/actions/runs/39681738413) failed because the previous graph entry marked T0018 done without a `succeeded` history result or test-source evidence. This attempt records T0018 as in progress until new exact-commit checks pass.

## Next
Run the quality gate and product build for this exact integration commit. If both pass, record a successful attempt and mark T0018 done; then review the T0018 → T0019 edge before starting T0019.


## Attempt T0018-A003 — repair CI regressions
- The exact-commit quality gate [39733805458](https://github.com/artinrahimii/project/actions/runs/39733805458) failed because `tests/test_claims_audit.py` still expected the old preview-only title and explanatory text after the UI became functional.
- The product-build run [39733805441](https://github.com/artinrahimii/project/actions/runs/39733805441) failed in lifecycle tests because the unconfigured-backend message no longer included the established phrase `not configured`.
- Updated the audit to verify the new endpoint/model fields, connect action, prompt control, and generation action; restored the explicit unconfigured-backend reason. These are compatibility fixes, not evidence of live inference success.
- **Status remains in progress** until a new exact-commit quality gate and product build pass.

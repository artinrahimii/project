# T0138 — Execution report

- **Status:** `in_progress`; documentation and source-contract test are committed pending exact-head CI.
- **Attempt:** `T0138-A001`.
- **Base:** `main` / `dd0b9988c95901d1c391d2f6c362d3b4531747b4` (`V 3.0.481`).
- **Dependency review:** `T0137 → T0138` is already validated `sequencing_only`. The `T0128 → T0138` edge was reviewed against the canonical request schema and host-side parsing/handler code, classified as `sequencing_only`, and removed from active prerequisites. T0128 itself remains `not_started`; this task does not claim independent-node delivery.
- **Disclosure:** README, REQ-019, acceptance policy and ADR-0019 explicitly state that the receiving host can read `InferenceRequest.prompt` and `context_capsule` after transport decryption; channel encryption is not secrecy against the endpoint.
- **Repeatable check:** Added `tests/test_host_content_visibility.py` to compare the disclosure with the canonical schema and host handler and ensure runtime P2P gates remain unpassed.
- **Runtime boundary:** `AC-P2P-01` and `AC-PRIV-01` remain **NOT RUN** pending actual independent-node path and storage/network inspection.
- **CI:** Pending on `V 3.0.482`; no test result is claimed as successful before workflow completion.

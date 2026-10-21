# T0008 — execution report

## Attempt T0008-A001 — repeatable capability/privacy-claim audit

- Started: 2026-10-22T04:34:00Z.
- Base commit: `aa1bc7e77bc5bbce80e9d7f987de75c1bdc71c36` (`V 3.0.42`).
- Result: partial; the source/documentation audit, UI wording changes, and regression test have been prepared in this commit. CI for this new revision is not yet verified.
- Dependency review: inspected `T0007 → T0008`. The broad T0007 rule does not produce a specific artifact consumed by T0008, while T0008 directly inspects the current UI, backend configuration, README, requirements, and acceptance matrix. The edge is retained and reclassified from `unreviewed` to `sequencing_only`, with rationale and evidence recorded in `step three/project-graph.md`. T0007 remains `in_progress`.
- Findings:
  - `UnconfiguredInferenceBackend.IsAvailable` returns false. The WPF host shell does not demonstrate real local inference.
  - The Android request action only changes local UI status; no request transport is implemented.
  - The prior Android and Windows labels could be read as delivered capabilities; they now identify both applications as previews and disclose missing transport/inference.
  - The acceptance matrix had stale statements claiming the platform build projects were absent. Those statements were corrected against the actual files and the passing CI results for V 3.0.42.
  - Current source/document review does not establish encrypted transport, no persistent storage, or that prompts/responses cannot leave the device. Those release gates remain NOT RUN.
- Files changed: `tests/test_claims_audit.py`, `docs/acceptance.md`, `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`, `src/Host.Windows/MainWindow.xaml`, this report, and `step three/project-graph.md`.
- Tests: not executed locally in this tool session; local checkout/runtime execution is unavailable. The new test is scheduled by the existing `.github/workflows/quality.yml` discovery command. New-commit CI result: pending; no passing result is claimed yet.
- Scope limit: source-level contract tests can ensure that current UI/docs disclose limitations. They do not substitute for product runtime, network, cryptographic, or storage tests.
- Follow-up: inspect both GitHub Actions workflows for the exact resulting commit. Record their real outcomes before deciding whether T0008 can be marked done; keep product runtime privacy, P2P, encryption, and no-storage gates unpassed until suitable evidence exists.

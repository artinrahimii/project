# T0009 — execution report

## Attempt T0009-A001 — source boundary guard

- Started: 2026-10-21T21:12:00Z.
- Base commit: `340608c9f0c286f3c30fd6263a33d62030c7eb8d` (`V 3.0.44`).
- Result: partial; the narrow source guard and acceptance record are prepared; CI for the new revision is pending.
- Source inventory: the tracked `src/` tree contains the Windows host shell and Android client only. No central server/API project or central prompt/response/history handler is present in this snapshot. The Android request UI states that requests are not sent because host transport is not implemented.
- Dependency review: the `T0008 → T0009` edge was reviewed and retained as `type: blocks`, with `dependency_classification: sequencing_only`. T0009 reads REQ-011 and the current source inventory directly; it does not consume a specific artifact produced by T0008. Reason and file evidence are recorded in the graph and edge change log.
- Implementation: `tests/test_central_inference_boundary.py` checks common POST/PUT/PATCH route-handler declarations for central inference/prompt/chat/completion/response/conversation/history paths in first-party source outside `src/Host.Windows/`. It deliberately allows local host endpoints and does not ban direct P2P routes.
- Scope limit: the source-level guard cannot show where runtime request bytes travel, whether a relay receives payload contents, or whether prompts are persisted. The app transport is not implemented; REQ-011 network-level acceptance remains NOT RUN.
- Tests: not executed locally in this tool session. The exact-commit quality gate is pending; no passing result is claimed.
- Files changed: `tests/test_central_inference_boundary.py`, `docs/acceptance.md`, this report, and `step three/project-graph.md`.
- Follow-up: inspect the exact commit's Step Three quality-gate run; fix any failure. Keep T0009 in progress until the source guard passes CI, and do not claim end-to-end no-central handling until network/storage evidence exists.

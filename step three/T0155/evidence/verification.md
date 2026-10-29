# T0155 verification evidence

## Inspected base
- Branch: `main`
- Commit: `76245201996af722cacc95ffd77c27d9e3c2942a` (V 3.0.564)
- Graph blob read from the exact base tree: `caed3f78999256c1d7be866e58e9747dff207231`; the complete contents parsed successfully and the blob ID matched the tree entry.
- Prerequisites: T0154 exact-head CI passed; T0150 is done.

## Implemented paths
- `src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsuleSummarizer.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleSummarizerTest.kt`
- `tests/test_android_context_capsule_summarizer_contract.py`

## Acceptance checks introduced
- Provider implementation is injected through an interface.
- Candidate output is encoded under the selected limit and decoded through the v1 codec before it can be accepted.
- Input messages are sorted by timestamp and ID; the default provider uses explicit field markers plus a narrow initial-goal fallback rather than copying the full transcript.
- Invalid size, invalid schema, provider failures, and oversize output are rejected.
- Runtime test commands have not yet run; exact-head GitHub Actions results are pending.


## Exact-head V 3.0.565 verification

- Step Three quality passed: [run 40909822726](https://github.com/artinrahimii/project/actions/runs/40909822726), including Python tests, syntax, graph structure, and evidence checks.
- Android product gate failed `ktlintMainSourceSetCheck` with seven formatting findings. APK/JVM steps were skipped: [run 40909822388](https://github.com/artinrahimii/project/actions/runs/40909822388).
- HostLifecycle passed in run 40909822388. Windows WPF was still in progress at this evidence update. A002 fixes the reported formatting only; the task remains unaccepted pending a successful exact-head product gate.

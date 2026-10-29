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


## Exact-head V 3.0.566 verification

- Step Three quality passed: [run 40910278299](https://github.com/artinrahimii/project/actions/runs/40910278299).
- Android ktlint failed four chain-continuation findings in the marked-field value expression. APK/JVM steps were skipped: [run 40910278198](https://github.com/artinrahimii/project/actions/runs/40910278198).
- HostLifecycle passed in run 40910278198. Windows WPF was still running when this record was updated. A003 replaces the chain with separate normalization statements; acceptance remains pending.


## Exact-head V 3.0.567 verification

- Step Three quality passed: [run 40910752479](https://github.com/artinrahimii/project/actions/runs/40910752479).
- Android lint and APK build passed, but JVM tests had 3 failures out of 51 in the new summarizer test class during codec use. The class did not include the Robolectric runner used by existing codec tests; protobuf verification and artifact upload were skipped. [Product run 40910752935](https://github.com/artinrahimii/project/actions/runs/40910752935).
- Windows WPF and HostLifecycle passed. A004 configures the existing Robolectric test harness and awaits verification.


## Final verification — V 3.0.568

- Step Three quality passed: [run 40911585770](https://github.com/artinrahimii/project/actions/runs/40911585770).
- Android Kotlin lint, debug APK, all 51 Android JVM tests, generated Protobuf validation and APK artifact upload passed in [product run 40911585732](https://github.com/artinrahimii/project/actions/runs/40911585732), Android job 123580675606.
- Windows WPF build passed (job 123580675629) and HostLifecycle passed (job 123580675047) in the same product run.
- T0155 acceptance was evaluated against exact-head CI; no local Gradle execution is claimed. The default provider remains extractive and local.

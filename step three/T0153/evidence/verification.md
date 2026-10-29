# T0153 evidence ledger

- **UI:** `ConversationDetailScreen.kt` renders saved context fields and create/edit/save/cancel/delete-confirmation controls. Its disclosure states the capsule stays on this device and is not sent automatically.
- **State and operations:** `ConversationContextCapsuleViewModel.kt` connects the Compose form to `RoomConversationContextCapsuleRepository` for per-conversation observation, save and delete. Save validation remains delegated to the v1 codec.
- **Runtime tests:** `ConversationContextCapsuleViewModelTest.kt` covers form create/edit/delete and persistence reads through Room, plus blank-goal validation without a write.
- **Source contract:** `tests/test_android_context_capsule_ui_contract.py` checks actual screen controls, state wiring and the runtime-test cases.
- **Dependency reviews:** T0152 → T0153 = `contract_or_interface_dependency`; T0150 → T0153 = `sequencing_only`. Both original `blocks` edges remain and review rationale/evidence are logged.
- **Not yet proven:** exact-head CI for this change. Keep T0153 `in_progress` until Android lint/build/JVM checks and graph/evidence guards pass.


## V 3.0.558 CI failure and A002 correction

- [Quality run 40898143390](https://github.com/artinrahimii/project/actions/runs/40898143390) passed 242 Python tests and graph/evidence guards (229 tasks / 417 edges, zero errors).
- [Product run 40898143330](https://github.com/artinrahimii/project/actions/runs/40898143330) failed Android ktlint on the long schema-version error string and delete-dialog `Text` formatting. APK/JVM stages did not run; HostLifecycle passed; Windows WPF was still running when first checked.
- A002 splits the long error string and formats the confirmation text as a multiline Compose call; behavior and repository interfaces are unchanged. T0153 remains in progress until exact-head CI passes.


## V 3.0.559 CI failure and A003 correction

- [Quality run 40898787440](https://github.com/artinrahimii/project/actions/runs/40898787440) passed 242 Python tests and graph/evidence checks (229 tasks / 417 edges, zero errors).
- [Product run 40898787349](https://github.com/artinrahimii/project/actions/runs/40898787349) passed main-source Kotlin lint and HostLifecycle, but test-source lint failed on import order and one unused import in the new ViewModel test. APK/JVM tasks were skipped.
- A003 sorts imports and removes the unused symbol. The task remains in progress until the next exact-head build and JVM suite pass.

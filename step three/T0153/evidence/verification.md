# T0153 evidence ledger

- **UI:** `ConversationDetailScreen.kt` renders saved context fields and create/edit/save/cancel/delete-confirmation controls. Its disclosure states the capsule stays on this device and is not sent automatically.
- **State and operations:** `ConversationContextCapsuleViewModel.kt` connects the Compose form to `RoomConversationContextCapsuleRepository` for per-conversation observation, save and delete. Save validation remains delegated to the v1 codec.
- **Runtime tests:** `ConversationContextCapsuleViewModelTest.kt` covers form create/edit/delete and persistence reads through Room, plus blank-goal validation without a write.
- **Source contract:** `tests/test_android_context_capsule_ui_contract.py` checks actual screen controls, state wiring and the runtime-test cases.
- **Dependency reviews:** T0152 → T0153 = `contract_or_interface_dependency`; T0150 → T0153 = `sequencing_only`. Both original `blocks` edges remain and review rationale/evidence are logged.
- **Not yet proven:** exact-head CI for this change. Keep T0153 `in_progress` until Android lint/build/JVM checks and graph/evidence guards pass.

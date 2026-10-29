# T0148 evidence ledger

- **Attempt:** `T0148-A001`; result `partial`; exact-head CI pending.
- **Saved-state implementation:** `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt` exposes a ViewModel draft StateFlow backed by SavedStateHandle and accepts owner-provided state through Factory CreationExtras.
- **UI consumer:** `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt` collects and updates the ViewModel draft; the send path and Room-backed message/request-status flows remain unchanged.
- **Regression tests:** `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationMessagesViewModelTest.kt` tests restoring a non-empty draft into a recreated ViewModel; `tests/test_android_conversation_detail_contract.py` checks saved-state factory and Compose wiring.
- **Dependency evidence:** T0147 → T0148 validated as a contract/interface dependency; T0138 → T0148 validated as sequencing-only. Rationale and evidence paths are stored in the graph edge-change log.
- **Not yet proven:** Kotlin compilation, ktlint, Robolectric tests, APK build, Python suite, graph guard and exact-head CI. No successful test is claimed before logs are observed.

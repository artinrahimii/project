# T0148 execution report

- **Task:** T0148 — Restore conversation UI state after rotation and process recreation using ViewModel and saved state.
- **State in V 3.0.537:** `in_progress`; implementation is prepared for the online `main` change, but acceptance is pending exact-head CI.
- **Attempt:** `T0148-A001`.
- **Start:** 2026-10-29T12:06:55.010Z
- **End of implementation attempt:** 2026-10-29T12:06:58.856Z
- **Baseline:** `main` at `824d36186bb77e9ab27152c916462344d05456dc` (`V 3.0.536`).
- **Scope:** Restore the unsent composer draft when Android recreates the activity/ViewModel owner. Messages and request statuses continue to be reloaded from their existing Room flows.

## Dependency review

- **T0147 → T0148:** reviewed and validated as `contract_or_interface_dependency`. `ConversationDetailScreen.kt` renders the persisted request-status rows delivered by T0147. The lifecycle-state change touches the same screen and must preserve those status cards and observations. This is a concrete screen/interface dependency, not inferred from the edge type.
- **T0138 → T0148:** reviewed and validated as `sequencing_only`. T0138's host-visibility/privacy work does not supply an API consumed by the local SavedStateHandle/Room code. The original `blocks` edge remains as conservative sequencing; its type alone is not treated as proof of technical dependence.
- Both edge reviews, rationale and evidence were appended to `edge_change_log`. Other unreviewed edges, including T0148 → T0149, remain unchanged.

## Implementation

- Replaced Compose's non-restorable `remember(conversationId)` draft with a ViewModel-owned `StateFlow` backed by `SavedStateHandle`.
- Overrode `ViewModelProvider.Factory.create(modelClass, extras)` and used `extras.createSavedStateHandle()`, so the navigation/activity saved-state owner supplies restoration data.
- Kept message validation, Room persistence, request-status observation and the `sendMessage(String)` signature unchanged.
- Added a Robolectric/JVM regression test that recreates the ViewModel with restored saved-state values and verifies that the unfinished draft reappears without being stored as a message. Added Python source-contract assertions for the factory and UI wiring.
- Durable message/request persistence across a normal close/reopen remains T0149's separate scope; this task does not claim SavedStateHandle replaces Room.

## Verification status

No local Kotlin/Gradle or Python test execution is claimed in this tool session. These exact-head GitHub Actions checks are awaiting inspection:
- `draftIsRestoredAfterViewModelRecreation`
- `tests/test_android_conversation_detail_contract.py`
- Android `ktlintCheck`, `assembleDebug` and `testDebugUnitTest`
- Step Three quality, graph-structure and repository-backed output/evidence gates

T0148 must remain `in_progress` until actual job results are inspected. CI failures must be recorded as separate attempts and corrected before closing the task.

# A010 compile-source precheck — UPD-S1-010

## Exact baseline
- Branch: `main`
- Baseline commit: `047fae2293a886fb06447384db6a91d31a5510e5` (`V 3.1.173`)
- Android product workflow: <https://github.com/artinrahimii/project/actions/runs/42866515191>

## Finding
The new `ConversationWorkflowAcceptanceTest.kt` constructs `ConversationEntity` instances in the conversation-scoped Room test and the database-reopen test. The entity is declared in package `com.whitep.client.data`; the test package is `com.whitep.client.ui.conversations`. The import was missing, so Kotlin compilation would fail once the job passed lint.

## Correction
Added the explicit import `com.whitep.client.data.ConversationEntity` in sorted order. No runtime code or behavior changed.

## Validation state
Exact-head A010 CI must prove Kotlin test-source compilation, Robolectric execution, Android build and remaining product jobs. Physical Android-to-Windows E2E and in-place upgrade remain unperformed.

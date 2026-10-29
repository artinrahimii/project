# T0150 evidence ledger

- **New integration test:** `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationWorkflowAcceptanceTest.kt` tests create → send → close/reopen file-backed Room → restore conversation/message/request-state data using production ViewModels and DAO implementations.
- **UI/repository source contract:** `tests/test_android_conversation_detail_contract.py` verifies the Compose list screen's create/open actions, detail screen's persisted message flow and send action, and the workflow test.
- **Incoming-edge review:** T0149 → T0150 is `contract_or_interface_dependency`; T0138 → T0150 is `sequencing_only`. Both original `blocks` edges remain; rationale/evidence is recorded in graph edge-change log.
- **Not yet proven:** Kotlin lint/compile, file-backed workflow JVM/Robolectric test, APK build, Python suite and graph/evidence guards. Exact-head CI pending.
- **Boundary:** the flow under test is local-only and does not imply Android-to-Windows message delivery or host secrecy.

# T0144 evidence ledger — implementation candidate

- Base commit: `03fa46995911896599ff478da0e27763963f72b7` (`V 3.0.516`).
- Implemented source paths:
  - `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationListViewModel.kt`
  - `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationListScreen.kt`
  - `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
  - `src/Client.Android/app/src/main/java/com/whitep/client/ui/navigation/ClientDestination.kt`
  - `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- Regression paths:
  - `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationListViewModelTest.kt`
  - `src/Client.Android/app/src/test/java/com/whitep/client/ui/navigation/ClientDestinationTest.kt`
  - `tests/test_android_conversation_list_contract.py`
  - `tests/test_android_conversation_management_contract.py`
- Graph: incoming T0143 and T0138 edges were individually reviewed and validated; rationale/evidence is preserved in `edge_change_log`.
- Test result: **pending**. This ledger does not claim that a build or test has passed. Verify GitHub Actions for the exact implementation commit before completion.

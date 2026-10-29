# T0148 evidence ledger

- Saved-state implementation: [`V 3.0.537`](https://github.com/artinrahimii/project/commit/dc1f49e2f1e083b1fef50258b64c49e150a53dd3).
- Formatting correction: [`V 3.0.538`](https://github.com/artinrahimii/project/commit/5a017e5e85e89df763d8bb2be37a25ceea77587f).
- V 3.0.537 quality [40870690034](https://github.com/artinrahimii/project/actions/runs/40870690034) passed; Android ktlint failed five factory-format findings in product [40870690119](https://github.com/artinrahimii/project/actions/runs/40870690119).
- V 3.0.538 quality [40872063933](https://github.com/artinrahimii/project/actions/runs/40872063933) passed; Android ktlint still found two whitespace issues in `ConversationMessagesViewModel.kt:58` in product [40872063919](https://github.com/artinrahimii/project/actions/runs/40872063919). APK/build tests were skipped.
- The one-parameter `create(modelClass)` signature is adjusted in V 3.0.539 while its call body remains multi-line; exact-head CI is pending.
- Regression coverage: `ConversationMessagesViewModelTest.draftIsRestoredAfterViewModelRecreation` and `tests/test_android_conversation_detail_contract.py`.
- **Not claimed:** durability across normal app close/reopen, host delivery or live request transitions.

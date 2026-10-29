# T0148 evidence ledger

- **Baseline:** [`V 3.0.536`](https://github.com/artinrahimii/project/commit/824d36186bb77e9ab27152c916462344d05456dc).
- **Initial implementation:** [`V 3.0.537`](https://github.com/artinrahimii/project/commit/dc1f49e2f1e083b1fef50258b64c49e150a53dd3).
- **Quality CI:** [run 40870690034](https://github.com/artinrahimii/project/actions/runs/40870690034) passed.
- **Product CI:** [run 40870690119](https://github.com/artinrahimii/project/actions/runs/40870690119): Windows WPF and HostLifecycle jobs passed; Android ktlint failed five format findings in `ConversationMessagesViewModel.kt` lines 57 and 60. Android build and JVM tests were skipped.
- **Saved-state path:** `ConversationMessagesViewModel.draft` is a StateFlow backed by `SavedStateHandle`; the Factory consumes `CreationExtras.createSavedStateHandle()`. The Compose screen uses this ViewModel state instead of local `remember` draft state.
- **Regression test:** `ConversationMessagesViewModelTest.draftIsRestoredAfterViewModelRecreation` creates a replacement SavedStateHandle and checks that draft content is restored without creating a message.
- **V 3.0.538** corrects the ktlint-rejected factory formatting; exact-head CI pending.
- **Not claimed:** durable close/reopen persistence of message/request rows (T0149 scope), network delivery, or live request transitions.

# T0143 verification evidence — attempts A001 and A002

## Attempt A001 — V 3.0.511

- Step Three quality gate [40845443702](https://github.com/artinrahimii/project/actions/runs/40845443702): passed.
- Android product workflow [40845443840](https://github.com/artinrahimii/project/actions/runs/40845443840): failed at ktlint. Exact findings: import ordering in MainActivity.kt line 3 and uppercase function name in ConversationListScreen.kt line 28. APK build and Android unit tests were skipped.
- Host lifecycle job passed but does not prove Android acceptance.

## Attempt A002

- Corrected import order and changed the screen composable and call site to `conversationListScreen()`.
- Exact-head CI is pending; this attempt has no passed test evidence yet.

## Runtime acceptance still required

The Android unit test must prove that the ViewModel observes the Room DAO, starts empty and emits inserted conversations in the DAO's persisted ordering. Step Three quality validation must pass on the same commit before T0143 is completed.

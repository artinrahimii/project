# T0143 verification evidence — attempts A001–A003

## A001 — V 3.0.511
- [Step Three quality gate 40845443702](https://github.com/artinrahimii/project/actions/runs/40845443702): passed.
- [Android product run 40845443840](https://github.com/artinrahimii/project/actions/runs/40845443840): failed ktlint on MainActivity import order and uppercase composable name. APK and unit tests skipped.

## A002 — V 3.0.512
- [Step Three quality gate 40845871893](https://github.com/artinrahimii/project/actions/runs/40845871893): passed.
- [Android product run 40845871873](https://github.com/artinrahimii/project/actions/runs/40845871873): failed ktlint for an unused `ConversationListScreen` import after the composable was renamed; APK and unit tests skipped.

## A003 — pending
- Import corrected to `com.whitep.client.ui.conversations.conversationListScreen`.
- No success is claimed until the exact-head Android workflow executes the APK build and `testDebugUnitTest`, and the Step Three graph/evidence quality gate passes.

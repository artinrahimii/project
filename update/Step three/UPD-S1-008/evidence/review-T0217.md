# T0217 — راهنمای نصب، اتصال به میزبان و استفاده از گفت‌وگو را برای اندروید بر اساس اپ واقعی بنویس.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-001, UPD-S1-021, UPD-S1-024.
- Decision: `re_review_required`.

## Review finding

There is no accurate Android user guide for the requested host-connected ordinary-chat flow. The current conversation UI explicitly says messages are local-only and host sending is unavailable; documenting it as a supported flow would be false.

## Source and evidence

- `README.md`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`
- `docs/acceptance.md`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Re-review after the related UPD-S1 implementation and required real-product acceptance. Preserve the baseline status until the legacy task's own acceptance criteria are satisfied.

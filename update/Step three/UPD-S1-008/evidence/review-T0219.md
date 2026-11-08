# T0219 — همهٔ قابلیت‌های رابط کاربری را به سرویس اجرایی واقعی وصل کن و کنترل‌های بدون عملکرد را حذف یا غیرفعال کن.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-024.
- Decision: `re_review_required`.

## Review finding

The ordinary ConversationDetailScreen send path calls the local-only ConversationMessagesViewModel, which writes user messages to Room and does not invoke the host runtime. RequestViewModel/runtime exist separately; RoomClientRequestRepository reports TRANSPORT_UNAVAILABLE. This legacy UI-to-service task is not complete.

## Source and evidence

- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/RoomClientRequestRepository.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`
- `tests/test_android_conversation_detail_contract.py`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Re-review after the related UPD-S1 implementation and required real-product acceptance. Preserve the baseline status until the legacy task's own acceptance criteria are satisfied.

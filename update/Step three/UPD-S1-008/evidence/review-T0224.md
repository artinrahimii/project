# T0224 — ثابت کن سه میزبان مستقل می‌توانند برای یک درخواست پاسخ جداگانه بدهند و پاسخ‌ها در اپ جدا نمایش داده می‌شوند.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-003, UPD-S1-015, UPD-S1-020.
- Decision: `re_review_required`.

## Review finding

The approved V1.10 first-release policy supersedes three active hosts per round: one selected host at a time, at most three sequential attempts, no broadcast. Compatibility constants/tests remain but production runtime enforces one active peer. Do not restore the old behavior without a new approved decision.

## Source and evidence

- `docs/decisions/ADR-0022-single-host-sequential-retry.md`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`
- `tests/test_android_inference_attempt_budget_acceptance.py`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Keep the approved single-host policy; do not restore three-host broadcast without a new accepted product decision.

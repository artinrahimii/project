# بازبینی محتوایی T0127

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 7082162e902e1c7e32718e0af687f8f62b7d92dc — V 3.1.85
- وضعیت پایه: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-014
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 7082162e902e1c7e32718e0af687f8f62b7d92dc

## یافته‌ها
Android tests cover directory expiry/cache, duplicate PeerIds, expired lease, unavailable or untrusted probes, Busy/non-Ready hosts, stale status, PeerId mismatch, model incompatibility and zero candidates. Dispatcher tests cover rejected attempts and bounded retry behavior. These are JVM, HTTP test-double and contract checks, not proof of external public-network behavior. UPD-S1-014 still changes metadata TTL, heartbeat and candidate eligibility.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt
- src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt
- tests/test_android_network_failure_acceptance.py

## وابستگی‌ها
- T0126 -> T0127: sequencing_only / validated
- T0115 -> T0127: sequencing_only / validated
- T0127 -> T0128: unreviewed / unreviewed; preserved
- هیچ وضعیت پایه، تاریخچه یا یال قدیمی تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179813 — موفق.
- Rendezvous V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179886 — موفق.
- HostLifecycle V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179878 — موفق؛ یک آزمون real-model با وضعیت SKIP دارد.
- Windows و Android product jobs در زمان این snapshot هنوز در حال اجرا بودند.
- Full Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ کدهای محصول بررسی‌شده بعد از آن تغییر نکرده‌اند.

## اقدام بعدی
Rerun expiry, duplicate, disconnect, zero-host and retry tests against the deployed service metadata, authenticated status heartbeat and final one-host selection after UPD-S1-014.

## مرز شواهد
این بازبینی اثبات آزمون public independent network، inference مدل واقعی یا in-place upgrade نیست. T0128 با وضعیت deferred_final باقی می‌ماند.

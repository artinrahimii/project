# بازبینی محتوایی T0121

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-014
- تصمیم reconciliation: re_review_required
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
PeerAnnouncementLeasePublisher sends an immediate metadata announcement, uses a five-minute default TTL and one-minute renewal interval, renews in a managed task and stops renewal on StopAsync so the directory naturally expires the final record. The shared announcement contract enforces the 24-hour maximum lifetime. PeerAnnouncementHostLifecycle wires the publisher into host opt-in/start/stop. HostLifecycle loopback integration verifies publication, renewal, expiry and stop; the Rendezvous container build/smoke also passed on V3.1.84. This does not prove public HTTPS deployment or reachability from an independent network.

## فایل‌های بررسی‌شده
- src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs
- src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs
- src/Host.Windows/Networking/PeerAnnouncementClient.cs
- src/Rendezvous.Service/PeerAnnouncementDirectory.cs
- src/Rendezvous.Service/PeerAnnouncementServiceHost.cs
- src/Rendezvous.Service/Dockerfile
- .github/workflows/rendezvous-container.yml
- docs/deployment/rendezvous-service.md
- tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs
- tests/rendezvous_container_smoke.py
- tests/test_decision_traceability.py

## وابستگی‌های مستقیم
- T0120 -> T0121: contract_or_interface_dependency / validated
- T0115 -> T0121: sequencing_only / validated
- T0121 -> T0122: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-014 combines leases with live heartbeat and selection. Revalidate TTL, renewal, opt-in, stop/expiry and directory eligibility using the final policy and deployed service.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

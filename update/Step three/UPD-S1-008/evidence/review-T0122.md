# بازبینی محتوایی T0122

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
The server removes expired entries under its directory lock before returning capability-discovery results. The Android client filters expiry during parsing, resolve and enumeration; it bounds cache expiry by the earliest peer lease and does not return stale entries as a fallback after refresh failure. Android JVM test excludes expired peers, and HostLifecycle lease-expiry coverage checks server-side removal. This is lease-expiry filtering, not a substitute for the live readiness heartbeat implemented by T0123.

## فایل‌های بررسی‌شده
- src/Rendezvous.Service/PeerAnnouncementDirectory.cs
- src/Rendezvous.Service/PeerAnnouncementServiceHost.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt
- src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt
- tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs
- tests/rendezvous_container_smoke.py

## وابستگی‌های مستقیم
- T0121 -> T0122: contract_or_interface_dependency / validated
- T0115 -> T0122: sequencing_only / validated
- T0122 -> T0123: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-014 changes eligibility by combining lease expiry, live status, trust and readiness. Revalidate stale-metadata rejection together with status freshness in the final candidate path.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

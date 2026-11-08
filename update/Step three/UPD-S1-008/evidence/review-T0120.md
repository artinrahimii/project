# بازبینی محتوایی T0120

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-002, UPD-S1-014, UPD-S1-016
- تصمیم reconciliation: unchanged
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
PeerAnnouncement exposes only PeerId, direct addresses, capabilities and expiry. It bounds message bytes, addresses and capabilities and rejects circuit relay. Rendezvous exposes announcement POST and capability-discovery GET only; its in-memory directory stores the latest metadata by PeerId and removes expired announcements. HostLifecycle tests reject unknown JSON fields and oversized control bodies. The Android directory client performs metadata GET without a request body or prompt/capsule fields. The metadata-only boundary remains correct and is retained unchanged.

## فایل‌های بررسی‌شده
- src/Shared.Contracts/Networking/PeerAnnouncement.cs
- src/Host.Windows/Networking/PeerAnnouncementClient.cs
- src/Rendezvous.Service/PeerAnnouncementServiceHost.cs
- src/Rendezvous.Service/PeerAnnouncementDirectory.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt
- docs/decisions/ADR-0004-encrypted-relay-boundary.md
- tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt
- tests/test_relay_privacy_boundary.py

## وابستگی‌های مستقیم
- T0119 -> T0120: contract_or_interface_dependency / validated
- T0115 -> T0120: sequencing_only / validated
- T0120 -> T0121: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
If UPD-S1-014 changes the allowed metadata schema, revalidate the Android, Windows and service consumers together; never add prompt, response or conversation history to the rendezvous contract.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

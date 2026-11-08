# بازبینی محتوایی T0123

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
HostInferenceSession.GetFreshStatusAsync returns a fresh backend-health/readiness snapshot and reports Busy or transitions conservatively without queuing behind active inference. The Windows peer lifecycle accepts HostStatusRequest only from an authenticated peer whose identity is checked and answers for its own PeerId with a server-generated observed_at timestamp. Android sends status probes over the authenticated libp2p peer stream, verifies the same PeerId and timestamp freshness, and does not use cached status as reachability proof. HostLifecycle and Android JVM acceptance coverage exist.

## فایل‌های بررسی‌شده
- src/Host.Application/HostInferenceSession.cs
- src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs
- src/Shared.Contracts/ProtocolMessageValidator.cs
- src/Host.Infrastructure/ProtocolMessageParser.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt
- tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt
- tests/HostLifecycle.Tests/Program.cs

## وابستگی‌های مستقیم
- T0122 -> T0123: contract_or_interface_dependency / validated
- T0115 -> T0123: sequencing_only / validated
- T0123 -> T0124: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-014 adds trust, capacity, model compatibility and ranking to the heartbeat result. Revalidate identity, freshness, READY state and no-cache semantics against the final trust policy.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

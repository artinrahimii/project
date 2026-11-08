# بازبینی محتوایی T0119

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
ADR-0004 explicitly leaves the relay data path disabled in the first release. Only direct TCP/QUIC candidates for the same authenticated peer are attempted; if they all fail, the product returns a clear PeerConnectionUnavailable outcome. A future relay must forward opaque bytes beneath endpoint-to-endpoint authentication and encryption; a relay may see peer IDs, addresses, ports, timing and sizes but must not parse, log or persist payloads. PeerAnnouncement rejects circuit-relay addresses, and the current libp2p stack does not enable WithRelay(). This is consistent with the approved UPD-S1-002/016 boundary, so no implementation change is required now.

## فایل‌های بررسی‌شده
- docs/decisions/ADR-0002-direct-p2p-data-path.md
- docs/decisions/ADR-0004-encrypted-relay-boundary.md
- src/Shared.Contracts/Networking/PeerAnnouncement.cs
- src/Host.Windows/Networking/PeerTransportFallbackConnector.cs
- src/Host.Windows/Networking/Libp2pPeerTransport.cs
- tests/test_relay_privacy_boundary.py
- tests/test_decision_traceability.py

## وابستگی‌های مستقیم
- T0118 -> T0119: sequencing_only / validated
- T0115 -> T0119: sequencing_only / validated
- T0119 -> T0120: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
Reopen this decision only if the approved product privacy policy changes. Until then keep relay disabled and preserve explicit failure when direct connectivity is unavailable.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

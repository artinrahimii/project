# بازبینی محتوایی T0118

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-002, UPD-S1-014, UPD-S1-016
- تصمیم reconciliation: re_review_required
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
PeerTransportFallbackConnector tries direct candidate addresses for one expected PeerId, requires ConnectSecureAsync and exact authenticated RemotePeerIdentity, disposes failed transports and records only safe exception-type metadata. Acceptance tests execute a refused TCP address followed by successful QUIC to the same peer and reject mixed identities before dialing. This is direct TCP-to-QUIC fallback; it does not prove NAT hole punching or connectivity where both direct paths are blocked, and it does not implement relay.

## فایل‌های بررسی‌شده
- src/Host.Windows/Networking/PeerTransportFallbackConnector.cs
- src/Host.Windows/Networking/Libp2pPeerTransport.cs
- src/Host.Windows/Networking/PeerTransportEndpoint.cs
- docs/decisions/ADR-0002-direct-p2p-data-path.md
- docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md
- docs/decisions/ADR-0004-encrypted-relay-boundary.md
- tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs
- tests/test_relay_privacy_boundary.py

## وابستگی‌های مستقیم
- T0117 -> T0118: contract_or_interface_dependency / validated
- T0115 -> T0118: sequencing_only / validated
- T0118 -> T0119: sequencing_only / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-002/016 finalize the direct transport and fallback policy. Reconfirm that exhaustion of direct candidates remains an explicit connection-unavailable result and never silently forwards inference data through rendezvous or relay.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

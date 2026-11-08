# بازبینی محتوایی T0117

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-014, UPD-S1-016
- تصمیم reconciliation: re_review_required
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
The Windows host uses a concrete Nethermind libp2p transport with pinned peer identity, trust-store enforcement, message bounds, inbound queue bounds and direct TCP/QUIC listen addresses. Android has a separate production Libp2pInferenceRequestTransport using its persistent identity, explicit trust store, authenticated PeerId and Protobuf streams. Android JVM tests cover status probes, cancellation, correlation and invalid peers. The exact V3.1.83 HostLifecycle log confirms direct libp2p exchange and authenticated fallback. Full Android JVM/Keystore validation passed on V3.1.82 when these product source files were unchanged; the V3.1.84 Android job was still in progress at the review snapshot.

## فایل‌های بررسی‌شده
- src/Host.Windows/Networking/Libp2pPeerTransport.cs
- src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs
- src/Host.Windows/Networking/IPeerTransport.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt
- src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt
- docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md
- docs/decisions/ADR-0021-android-libp2p-peer-transport.md
- tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt
- tests/test_android_libp2p_transport_contract.py

## وابستگی‌های مستقیم
- T0116 -> T0117: contract_or_interface_dependency / validated
- T0115 -> T0117: sequencing_only / validated
- T0117 -> T0118: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-014/016 must connect the final live discovery and trust decision to these transports. Revalidate Android/Windows app-level routing, independent-network behavior and no-relay failure handling before the update task is closed.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.

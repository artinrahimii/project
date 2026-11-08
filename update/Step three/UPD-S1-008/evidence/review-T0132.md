# بازبینی محتوایی T0132

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 7082162e902e1c7e32718e0af687f8f62b7d92dc — V 3.1.85
- وضعیت پایه: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-002, UPD-S1-007, UPD-S1-016, UPD-S1-022
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 7082162e902e1c7e32718e0af687f8f62b7d92dc

## یافته‌ها
The maintained libp2p transport negotiates an authenticated secure channel, pins the expected PeerId and disconnects when the observed remote identity differs. Windows acceptance tests explicitly reject plaintext-only peers and prevent downgrade to an insecure connection; Android uses the libp2p transport with identity and trust checks. Loopback and CI tests do not prove an independently deployed Android-to-Windows session over a public network.

## فایل‌های بررسی‌شده
- src/Host.Windows/Networking/Libp2pPeerTransport.cs
- src/Shared.Security/PeerSecurityPolicy.cs
- src/Shared.Security/PeerTrustStore.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt
- docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md
- tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs
- tests/HostLifecycle.Tests/PeerSecurityPolicyAcceptanceTests.cs
- tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt

## وابستگی‌ها
- T0131 -> T0132: contract_or_interface_dependency / validated
- T0132 -> T0133: contract_or_interface_dependency / validated
- T0132 -> T0230: contract_or_interface_dependency / validated
- هیچ وضعیت پایه، تاریخچه یا یال قدیمی تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179813 — موفق.
- Rendezvous V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179886 — موفق.
- HostLifecycle V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179878 — موفق؛ یک آزمون real-model با وضعیت SKIP دارد.
- Windows و Android product jobs در زمان این snapshot هنوز در حال اجرا بودند.
- Full Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ کدهای محصول بررسی‌شده بعد از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-016/022 must rerun secure-channel, pinning, no-downgrade and trust checks through the final app transport and installed host.

## مرز شواهد
این بازبینی اثبات آزمون public independent network، inference مدل واقعی یا in-place upgrade نیست. T0128 با وضعیت deferred_final باقی می‌ماند.

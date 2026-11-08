# بازبینی محتوایی T0128

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 7082162e902e1c7e32718e0af687f8f62b7d92dc — V 3.1.85
- وضعیت پایه: deferred_final؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-014
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 7082162e902e1c7e32718e0af687f8f62b7d92dc

## یافته‌ها
The legacy baseline is deferred_final. Its acceptance requires two real computers or independent network nodes, the public HTTPS Rendezvous service from T0230, authenticated exact PeerId routing, advertised transport addresses and lease expiry after publisher shutdown. The repository has a self-hosted Windows LAN workflow and loopback tests, but neither proves the required independent public-network test. No timestamped, redacted two-node public-network result was available in this review; the legacy deferred_final status is preserved.

## فایل‌های بررسی‌شده
- .github/workflows/android-windows-lan-acceptance.yml
- tests/HostLifecycle.Tests/Invoke-AndroidWindowsLanAcceptance.ps1
- docs/deployment/rendezvous-service.md
- src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs
- src/Host.Windows/Networking/Libp2pPeerTransport.cs
- tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs
- tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs

## وابستگی‌ها
- T0127 -> T0128: unreviewed / unreviewed; preserved
- T0115 -> T0128: unreviewed / unreviewed; preserved
- T0131 -> T0128: contract_or_interface_dependency / validated
- T0230 -> T0128: contract_or_interface_dependency / validated
- هیچ وضعیت پایه، تاریخچه یا یال قدیمی تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179813 — موفق.
- Rendezvous V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179886 — موفق.
- HostLifecycle V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179878 — موفق؛ یک آزمون real-model با وضعیت SKIP دارد.
- Windows و Android product jobs در زمان این snapshot هنوز در حال اجرا بودند.
- Full Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ کدهای محصول بررسی‌شده بعد از آن تغییر نکرده‌اند.

## اقدام بعدی
T0128 remains deferred_final. Complete it only after T0230 public deployment and an independent two-node test; do not substitute loopback, LAN-only workflow or CI.

## مرز شواهد
این بازبینی اثبات آزمون public independent network، inference مدل واقعی یا in-place upgrade نیست. T0128 با وضعیت deferred_final باقی می‌ماند.

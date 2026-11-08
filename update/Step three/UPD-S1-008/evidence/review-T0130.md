# بازبینی محتوایی T0130

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
Windows creates a libp2p Ed25519 identity, protects serialized private-key bytes with an OS protector, writes a bounded envelope, zeroes temporary buffers and fails closed on corrupt or unsupported persisted data. Windows DPAPI uses CurrentUser scope; WindowsHostIdentityProvider stores beneath LocalApplicationData and preserves PeerId across provider instances. Android stores Ed25519 private-key bytes in an authenticated envelope protected by a non-exportable Android Keystore key; tests cover stable PeerId and corrupt-record rejection. In-place upgrade identity preservation remains a separate unverified gate.

## فایل‌های بررسی‌شده
- src/Shared.Security/HostIdentityStore.cs
- src/Host.Windows/Security/WindowsDpapiSecretProtector.cs
- src/Host.Windows/Security/WindowsHostIdentityProvider.cs
- src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt
- src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt
- tests/HostLifecycle.Tests/HostIdentityStoreAcceptanceTests.cs
- tests/HostLifecycle.Tests/WindowsHostIdentityStoreAcceptanceTests.cs
- src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerIdentityStoreTest.kt

## وابستگی‌ها
- T0129 -> T0130: contract_or_interface_dependency / validated
- T0130 -> T0131: contract_or_interface_dependency / validated
- هیچ وضعیت پایه، تاریخچه یا یال قدیمی تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179813 — موفق.
- Rendezvous V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179886 — موفق.
- HostLifecycle V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179878 — موفق؛ یک آزمون real-model با وضعیت SKIP دارد.
- Windows و Android product jobs در زمان این snapshot هنوز در حال اجرا بودند.
- Full Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ کدهای محصول بررسی‌شده بعد از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-022 and the final upgrade path must verify Windows and Android host identities survive an in-place update without key rotation or reset.

## مرز شواهد
این بازبینی اثبات آزمون public independent network، inference مدل واقعی یا in-place upgrade نیست. T0128 با وضعیت deferred_final باقی می‌ماند.

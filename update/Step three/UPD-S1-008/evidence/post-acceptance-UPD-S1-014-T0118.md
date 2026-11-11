# بازبینی پس از پذیرش UPD-S1-014 — T0118

- عنوان وظیفهٔ قدیمی: اتصال جایگزین بدون عبور متن درخواست از سرور معرفی‌کننده
- update_task_ids: UPD-S1-002, UPD-S1-014, UPD-S1-016
- commit کد بررسی‌شده: f9f4cc8ad57964dedaca985ccdc679fee5eb7718 — V 3.1.191
- وضعیت پایهٔ legacy: done؛ بدون تغییر
- decision پیشنهادی پس از بازبینی: unchanged
- review_status: reviewed
- status_changed: false

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Windows/Networking/PeerTransportFallbackConnector.cs`
- `src/Host.Windows/Networking/Libp2pPeerTransport.cs`
- `docs/decisions/ADR-0002-direct-p2p-data-path.md`
- `docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md`
- `docs/decisions/ADR-0004-encrypted-relay-boundary.md`
- `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`
- `tests/test_relay_privacy_boundary.py`

## آزمون‌های exact-head
- Quality Gate run 42999958123: 496/496 Python tests passed; rendezvous smoke run 42999958177: non-root container health, announcement, discovery and invalid-input rejection PASS.

## نتیجه
مرز relay و fallback در این delta تغییر نکرده است. آزمون privacy contract و smoke سرویس Rendezvous روی کد دقیق جاری موفق است؛ directory فقط metadata می‌گیرد. استقرار عمومی HTTPS در این بازبینی ادعا نمی‌شود.

این تصمیم فقط پس از ثبت هم‌زمان در reconciliation ledger و نشانگر update_impact گراف اولیه معتبر خواهد شد. هیچ status قدیمی بر اساس affected_task_ids تغییر داده نشده است.

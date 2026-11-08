# بازبینی T0190 — نمایش وضعیت شبکه و inference بدون محتوای پیام

- مبنای main: `fc006d4c7f01de25c27346fc85670d1dc503e231`، `V 3.1.98`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-010، UPD-S1-017

## پیاده‌سازی بررسی‌شده

- Android `RequestStatusPresentation.kt` هشت lifecycle state پایدار را به label/detail قابل‌نمایش تبدیل می‌کند؛ وضعیت شبکه و inference از ساختار UI state ساخته می‌شوند، نه متن آزاد درخواست/پاسخ.
- صفحهٔ Conversation Detail از Room، وضعیت‌های همان conversation را observe و کارت‌های پایدار نمایش می‌دهد.
- بازیابی پس از restart، وضعیت‌های in-flight را به `EXPIRED` تغییر می‌دهد و محتوای prompt/capsule/response را در request-state row ذخیره نمی‌کند.
- Windows backend/network presenter وضعیت backend health/busy/error را از متن محتوای پیام جدا نگه می‌دارد.

## ناسازگاری و شکاف باز

- Conversation Detail هنوز متن ثابت **“Requests are not sent to a host in this preview.”** را نشان می‌دهد؛ بنابراین این صفحه هنوز مسیر درخواست گفت‌وگوی عادی به host را به‌عنوان قابلیت موجود معرفی نمی‌کند.
- Windows `HostStatusPresentation.FormatNetworkStatus` می‌گوید **“Direct peer-to-peer transport: not implemented in this build.”** درحالی‌که `MainWindow` سرویس `PeerAnnouncementHostLifecycle` و toggle مشارکت همتا را ایجاد/اجرا می‌کند. این دو بخش از UI باید هماهنگ شوند یا دامنهٔ هرکدام صریحاً تفکیک شود.
- وضعیت‌های Room موجود هستند، اما این شواهد اتصال واقعی ارسال/پاسخ مکالمه به inference end-to-end را ثابت نمی‌کنند.

## آزمون و شواهد

- `RequestStatusPresentationTest.kt`، `ClientDatabaseTest.kt`، `test_android_request_status_contract.py` و `test_wpf_entrypoint.py`
- Product Build دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند.

## تصمیم

وضعیت پایهٔ `done` حفظ شد، اما `re_review_required` ضروری است. اتصال prompt و response به گفت‌وگوی جاری و Room، اصلاح متن preview، و تفکیک network/host status در Windows از کارهای باز UPD-S1-010/017 هستند؛ E2E فیزیکی اجرا نشده است.

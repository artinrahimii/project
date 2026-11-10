# تأیید CI و پذیرش نهایی UPD-S1-013 — A002

## مبنای دقیق

- پیاده‌سازی: `991930f84f7a9af9fc839d4d4c7ee9b8a049758f` — `V 3.1.176`
- Verification window: `2026-11-10T19:53:57Z` تا `2026-11-10T19:58:40Z`
- وظیفه: `UPD-S1-013`
- وضعیت پس از این شواهد: `done`

## نتیجهٔ هر معیار

1. مشارکت غیرفعال و مدل non-ready در admission رد می‌شوند؛ ظرفیت نامعلوم/ناکافی fail-closed است.
2. coordinator دارای یک slot واقعی است؛ CI گزارش کرد ۱ از ۶۴ پذیرفته و ۶۳ مورد Busy بودند.
3. محدودیت RAM و فضای آزاد system drive از اندازه‌گیری واقعی Windows گرفته می‌شود. پشتیبانی AC/idle در نسخهٔ جاری احراز نشده است، پس ادعای enforcement وجود ندارد.
4. `CapacityInsufficient` و `CapacityUnknown` به `RESOURCE_LIMIT` نگاشت می‌شوند؛ آزمون mapper و admission در HostLifecycle اجرا شدند.

## GitHub Actions روی commit پیاده‌سازی

- Quality: [42889844427](https://github.com/artinrahimii/project/actions/runs/42889844427) — ۴۹۴ تست Python؛ graph guards موفق. گراف قدیمی با یک هشدار metadata و صفر خطا؛ گراف به‌روزرسانی صفر خطا/هشدار.
- Windows + HostLifecycle: [42889844413](https://github.com/artinrahimii/project/actions/runs/42889844413) — WPF restore/build/publish/startup smoke و installer acceptance موفق؛ HostLifecycle با ۶۸ PASS marker و ۸۶٫۱۵٪ line coverage موفق.
- Real inference: [42889844288](https://github.com/artinrahimii/project/actions/runs/42889844288) — local llama.cpp generation و cancellation موفق.
- Rendezvous: [42889844469](https://github.com/artinrahimii/project/actions/runs/42889844469) — container و HTTP smoke موفق.

## شواهد منبع و آزمون

- `src/Host.Windows/MainWindow.xaml.cs`: ارزیابی مجدد capacity در مرز درخواست.
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`: رد typed ظرفیت ناکافی پیش از inference.
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`: نگاشت به `ProtocolError RESOURCE_LIMIT`.
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`: Regression برای HostState.Error همراه ظرفیت Insufficient.
- `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`: پاسخ typed ظرفیت.
- `tests/test_host_resource_settings.py`: regression برای سیم‌کشی مسیر peer به اندازه‌گیری تازه.

## مرز نتیجه

این شواهد Android-to-Windows E2E روی دستگاه فیزیکی، clean install روی دستگاه هدف یا in-place upgrade را انجام‌شده نشان نمی‌دهند. این معیارها در وظایف بعدی باقی هستند.

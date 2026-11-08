# بازبینی محتوایی T0210 — اجرای WPF نصب‌شده از محیط تمیز

- مبنای بررسی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` — `V 3.1.106`
- وضعیت پایه: `done`؛ بدون تغییر.
- UPDهای مرتبط: `UPD-S1-021`, `UPD-S1-024`
- تصمیم: `re_review_required`؛ وضعیت بازبینی: `reviewed`.
- دلیل بازبینی مجدد آتی: UPD-S1-021 بسته و راهنمای نهایی را دوباره می‌سنجد و UPD-S1-024 هویت بصری Windows را تغییر می‌دهد.

## کد و آزمون بررسی‌شده
- `.github/workflows/product-build.yml` — Blob `64fd1fb9695ae0aeb593cdb5b1499a9786c16f2a`
- `tools/build_windows_installer.ps1` — Blob `39739f078942bdac5013d3b211b0a5db04abdfde`
- `src/Host.Windows/App.xaml.cs`
- `tests/test_wpf_entrypoint.py` — Blob `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`

## شواهد اجرا روی HEAD جاری
- Product Build: https://github.com/artinrahimii/project/actions/runs/42516118989
- Windows job: https://github.com/artinrahimii/project/actions/runs/42516118989/job/128839399180
- installer واقعی در مسیر موقت و یکتا نصب شد.
- executable نصب‌شده با `--smoke-test` اجرا شد؛ WPF MainWindow و کنترل‌های لازم بررسی و خروج تمیز تأیید شد.
- uninstall مسیر نصب و ثبت برنامه را پاک کرد.
- Artifact نصب‌کننده: 13631034913.

## محدودیت
این پذیرش روی Windows GitHub-hosted clean runner انجام شده است، نه روی رایانهٔ شخصی کاربر یا همهٔ نسخه‌های Windows. انتشار عمومی انجام نشده است.

## نتیجه
اجرای WPF نصب‌شده از بستهٔ واقعی با شواهد current-head تأیید شد. وضعیت پایهٔ `done` حفظ شد و به‌علت تغییرهای آیندهٔ بسته و visual identity بازبینی مجدد لازم است.

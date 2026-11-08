# بازبینی محتوایی T0209 — بستهٔ نصب واقعی Windows

- مبنای بررسی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` — `V 3.1.106`
- وضعیت پایه: `done`؛ بدون تغییر.
- UPDهای مرتبط: `UPD-S1-021`
- تصمیم: `re_review_required`؛ وضعیت بازبینی: `reviewed`.
- دلیل بازبینی مجدد آتی: بسته‌بندی و شواهد تحویل نهایی در UPD-S1-021 باید روی نسخهٔ نهایی دوباره کنترل شود.

## کد و آزمون بررسی‌شده
- `.github/workflows/product-build.yml` — Blob `64fd1fb9695ae0aeb593cdb5b1499a9786c16f2a`
- `tools/build_windows_installer.ps1` — Blob `39739f078942bdac5013d3b211b0a5db04abdfde`
- `tests/test_generate_dependency_licenses.py` — Blob `7acb668758872f07e2555152785a0a295815d7db`
- `tests/test_generate_model_license_notice.py` — Blob `1666204f1d643b7f5362e879e2b787df63b6005b`
- `tests/test_reproducible_build_contract.py` — Blob `6662d35bf99118658bd54bae4c5bfabe5ba79ff4`

## شواهد اجرا روی HEAD جاری
- Product Build: https://github.com/artinrahimii/project/actions/runs/42516118989
- Windows job: https://github.com/artinrahimii/project/actions/runs/42516118989/job/128839399180
- Windows clean restore/build/publish موفق شد؛ 66 فایل publish تولید شد.
- installer واقعی `WhiteP-Setup-0.1.0-win-x64.exe` با اندازهٔ 61,465,445 بایت تولید شد.
- فهرست مجوز 83 package و اطلاعیهٔ مجوز یک مدل نمونه تولید شد.
- نصب ایزوله، بررسی Publisher ثبت‌شده، smoke launch و uninstall موفق شدند.
- Artifact `whitep-windows-installer` با شناسهٔ 13631034913، اندازهٔ ZIP برابر 60,988,759 بایت و SHA-256 برابر `f6bbf53eda79c1b91bc2ad46e79c4734bad36c53d57f3d02a8fe00944f618396` بارگذاری شد.

## محدودیت
هشدار غیرمسدودکنندهٔ padding فضای انتهایی PE CompanyName در log ثبت شد؛ مقدار Publisher در رجیستری محل نصب ایزوله به‌طور مستقل درست تأیید شد. این artifact از GitHub Actions است، نه انتشار عمومی GitHub Release و نه نصب روی رایانهٔ کاربر.

## نتیجه
پذیرش تولید installer، نسخه/ناشر، فهرست مجوزها، نصب، اجرا و حذف با شواهد CI فعلی تأیید شد. وضعیت پایهٔ `done` حفظ شد؛ بازبینی نهایی پس از UPD-S1-021 لازم است.

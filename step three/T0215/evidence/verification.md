# شواهد و راستی‌آزمایی T0215

## ورودی‌های بررسی‌شده
- src/Host.Windows/Models/Catalog/sample-model-catalog.json: مدل SmolLM2-135M-Instruct Q4_K_M، checksum مورد انتظار SHA-256 و metadata مجوز Apache-2.0 را اعلام می‌کند.
- صفحهٔ مدل در Hugging Face همین مجوز را در metadata مدل اعلام می‌کند: https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/blob/main/README.md
- tools/generate_dependency_licenses.py فهرست مجوزهای NuGet را از restore assets تولید می‌کند؛ مسیر مجوز مدل نمونه را به‌طور مستقل پوشش نمی‌داد.
- tools/installer/WhiteP.iss همهٔ فایل‌های LicensesDir را به پوشهٔ licenses نصب اضافه می‌کند.
- .github/workflows/release-assets.yml همان پوشهٔ windows-installer را در WhiteP-Windows-Installer.zip قرار می‌دهد.

## قرارداد پیاده‌سازی
- ModelLicenses.md: اعلامیهٔ قابل‌خواندن برای انسان.
- model-licenses.json: manifest ساختاریافته با catalog SHA-256، مدل، URL مدل‌کارت، checksum مدل، وضعیت/شناسهٔ مجوز، مسیر و SHA-256 متن مجوز.
- files/WhiteP-Sample-Model-Apache-2.0.txt: متن مجوز رسمی، verbatim.
- مدل‌هایی با مجوز missing/unverified یا شناسه‌ای غیر از Apache-2.0 فعلاً fail closed می‌شوند تا handler مستقل و بازبینی‌شده اضافه شود.
- مدل classification غایب به unknown تبدیل می‌شود؛ وزن مدل در artifact نیست.

## وضعیت آزمون
آزمون‌های جدید در tests/test_generate_model_license_notice.py ایجاد شده‌اند. انتظار اجرای آن‌ها در Product Build و Quality Gate روی commit مربوط به A001 ثبت شده است؛ تا آن زمان نتیجه pending است.

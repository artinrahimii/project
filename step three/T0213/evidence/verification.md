# شواهد T0213 — تلاش A001

## مبنای فایل‌ها و buildها

- Android production signed release: https://github.com/artinrahimii/project/actions/runs/42114568183 — commit 179efa9fe07c3ba007885e9e3198eba620092617, success.
- Android signed APK artifact: whitep-android-release-signed, artifact ID 13455966209, ZIP size 18,596,315 bytes. unzip -t passed; extracted APK size 22,786,909 bytes.
- Product build: https://github.com/artinrahimii/project/actions/runs/42113989702 — same commit, success; Android, Windows WPF/installer and HostLifecycle jobs all passed.
- Windows installer artifact: whitep-windows-installer, artifact ID 13453864758, size 60,978,083 bytes. unzip -t passed; installer and license inventory were present.

## نامزد checksum محلی

فایل SHA256SUMS-candidate.txt در همین پوشه، checksumهای فایل‌های استخراج‌شده و بستهٔ نامزد این مبنا را ثبت می‌کند. دستور sha256sum --check برای هر سه مورد خروجی OK داد. این خروجی صرفاً کنترل محلی اولیه است؛ checksum رسمی release باید در اجرای White-P release assets از artifactهای دقیق همان commit دوباره تولید و تأیید شود.

## پیاده‌سازی release automation

- .github/workflows/release-assets.yml runهای موفق signing و product build را با SHA دقیق جاری انتخاب می‌کند، artifactهای APK و installer را دانلود می‌کند، فایل Windows را همراه با license notices بسته‌بندی می‌کند و provenance را ثبت می‌کند.
- workflow SHA256SUMS.txt می‌سازد و بلافاصله آن را با sha256sum --check اعتبارسنجی می‌کند.
- Artifact whitep-release-assets برای نگهداری خروجی workflow بارگذاری می‌شود.
- فقط تگ android-v* می‌تواند GitHub Release را ایجاد کند؛ Release به‌صورت draft باقی می‌ماند و workflow انتشار قبلی را تغییر نمی‌دهد.
- آزمون‌های قرارداد workflow در tests/test_release_assets_contract.py افزوده شدند.

## محدودیت/وضعیت

در زمان این تلاش، GitHub Release هنوز ایجاد نشده است؛ API اتصال‌شده امکان create tag/release را ارائه نمی‌کند. پس از commit باید signing workflow روی commit جدید اجرا شود، سپس تگ android-v0.1.0 push شود. T0213 هنوز تکمیل‌شده محسوب نمی‌شود.

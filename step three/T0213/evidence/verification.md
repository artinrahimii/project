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

## A002 — بازبینی و اصلاح expansion

- commit بررسی‌شده: https://github.com/artinrahimii/project/commit/9464a17076b13088624d060f674d617230c09508.
- Quality Gate اولیه: https://github.com/artinrahimii/project/actions/runs/42120944588 — success.
- بازبینی فایل خام پس از commit یک backslash اضافی پیش از placeholderهای محیطی در release-assets.yml پیدا کرد. چون workflow هنوز اجرا نشده بود، از انتشار ناقص جلوگیری شد.
- نسخهٔ اصلاحی placeholderهای Bash و GitHub Actions را بدون backslash نگه می‌دارد؛ تست قرارداد اکنون صریحاً این شرط را کنترل می‌کند.
- این بازبینی اصلاحی، T0213 را تکمیل نمی‌کند؛ tag-based run و draft Release هنوز باید ایجاد و بررسی شوند.


## A003 — شواهد بازبینی مستقل (2026-11-06)

### نسخهٔ منبع

- شاخه: `main`
- commit فعلی هنگام بررسی: `4bad2fbf1458369097bcfb137d5daec729bc24a3` — `V 3.0.985`
- گراف: `step three/project-graph.md`, Blob `db25484ca21f08c0cf4acacb72f0576061c07196`, اندازهٔ 3,856,190 بایت، طول متن 3,726,678 نویسه. JSON کامل parse شد و SHA محاسبه‌شده توسط GitHub برای محتوای کامل عیناً با Blob ID برابر بود.
- ساختار: 230 node، 422 edge، صفر شناسهٔ تکراری، صفر ارجاع مفقود.

### CI و Artifactهای پیش‌نیاز

- Android signing run: https://github.com/artinrahimii/project/actions/runs/42114568183 — success، SHA `179efa9fe07c3ba007885e9e3198eba620092617`.
- Android signed artifact: https://github.com/artinrahimii/project/actions/runs/42114568183/artifacts/13455966209 — ID `13455966209`, digest `sha256:935a5d97b7ee32f4637867fceca00cb70809518b5494aeb43f0d8f35a5787825`.
- Product Build run: https://github.com/artinrahimii/project/actions/runs/42113989702 — success، همان SHA قدیمی.
- Windows installer artifact: https://github.com/artinrahimii/project/actions/runs/42113989702/artifacts/13453864758 — ID `13453864758`, digest `sha256:5f117cfb85ce902440ab6d906a0aa74a43cbd6d5b18857d0af27e63b0a36051b`.
- هر دو آرشیو ZIP از GitHub دریافت شدند؛ digest فایل دانلودشده با digest API برابر و `unzip -t` برای هر دو موفق بود.

### نتایج کنترل‌های همین نوبت

- 8/8 کنترل ایستای قرارداد روی Blobهای خام workflow، مستندات و فایل آزمون موفق شد.
- APK استخراج‌شده: 22,786,909 بایت؛ فایل نصب‌کنندهٔ Windows: 61,460,809 بایت؛ فایل‌های مجوز و اعلان‌ها در Artifact وجود داشتند.
- بازپخش `SHA256SUMS-candidate.txt`: checksum APK برابر بود؛ checksum ZIP بازسازی‌شده و provenance بازسازی‌شده برابر نبودند. ZIP بازسازی‌شده بعد از extract مجدداً ساخته شد و ورودی‌های پوشه با timestamp جدید داشت؛ بنابراین این کنترل، تطبیق بایت‌به‌بایت با ZIP نامزد قبلی نیست. provenance اصلی نامزد نیز در Artifactها موجود نبود. از این manifest برای Release نهایی استفاده نشود.

### مانع ادامه

Artifactهای واقعیِ بررسی‌شده برای SHA `179efa9fe07c3ba007885e9e3198eba620092617` هستند، اما HEAD فعلی main برابر `4bad2fbf1458369097bcfb137d5daec729bc24a3` است. چون workflow انتشار فقط runهای موفق همان SHA را قبول می‌کند، این artifactها برای HEAD فعلی قابل استفاده نیستند. ابزار اتصال فعلی workflow_dispatch و ساخت تگ/Release را پشتیبانی نمی‌کند. `T0213` تکمیل نشده و در گراف blocked می‌ماند.


## Erratum A003 — اصلاح digest آرشیو Windows (2026-11-06)

در نسخهٔ قبلی این فایل، digest مربوط به Artifact `13453864758` به‌اشتباه با 68 نویسه ثبت شده بود. مقدار صحیح 64 نویسه‌ای که هم در metadata گیت‌هاب و هم در فایل دانلودشده با SHA-256 تأیید شد:

`sha256:5f117cfb85ce902440ab6d906a0aa74a43cbd6d5b18857d0af27e63b0a36051b`

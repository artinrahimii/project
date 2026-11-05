# گزارش اجرای T0213

## هدف

تولید checksum برای APK امضاشدهٔ تولیدی و نصب‌کنندهٔ ویندوز که از buildهای موفق همان commit آمده‌اند، همراه با provenance و انتشار قابل‌راستی‌آزمایی در GitHub Release.

## بررسی پیش‌نیازها

- T0212 اکنون با اجرای موفق workflow تولیدی روی commit 179efa9fe07c3ba007885e9e3198eba620092617 تکمیل و تأیید شد.
- T0208 پیش از این در گراف done بوده است.
- یال T0212 → T0213 از نوع blocks و طبقه‌بندی hard_dependency / validated است؛ بدون APK تولیدی همان commit، ساخت checksum قابل‌قبول نیست. پس از تأیید Artifact واقعی، اجرای T0213 آغاز شد. هیچ یال گراف تغییر نکرد.
- یال T0208 → T0213 از نوع blocks با طبقه‌بندی sequencing_only / validated حفظ شد.
- یال T0213 → T0214 همچنان unreviewed / unreviewed است و عمداً تغییر نکرد.

## خروجی و بررسی واقعی اولیه

- APK امضاشدهٔ تولیدی از Artifact 13455966209 دریافت شد؛ ZIP معتبر بود و APK غیرخالی استخراج شد.
- Artifact نصب‌کنندهٔ ویندوز از Product Build run 42113989702، Artifact 13453864758، دریافت و با unzip -t بررسی شد. بسته شامل فایل اجرایی نصب‌کننده و inventory/notice مجوزهای وابستگی‌هاست.
- برای این خروجی‌های دریافت‌شده، یک manifest نامزد checksum ساخته و با sha256sum --check بررسی شد. این manifest شواهد preflight است و جای checksum نهایی ساخته‌شده در اجرای workflow انتشار را نمی‌گیرد.
- Workflow مستقل .github/workflows/release-assets.yml اضافه شد. این workflow فقط runهای موفق Android signed release و White-P product build با head_sha دقیقاً برابر commit جاری را می‌پذیرد، Artifactهای آن‌ها را دانلود می‌کند، بستهٔ Windows را با license notices می‌سازد، provenance و SHA256SUMS.txt تولید می‌کند و خود manifest را دوباره اعتبارسنجی می‌کند.
- workflow خروجی‌ها را به‌عنوان Artifact با نام whitep-release-assets بارگذاری می‌کند. در اجرای دارای تگ android-v*، یک GitHub Release به‌صورت draft ساخته یا به‌روزرسانی می‌شود. اگر Release موجود منتشرشده باشد، workflow از تغییر آن خودداری می‌کند.
- مجوزهای actions: read و contents: write فقط در job انتشار تعریف شده‌اند. هیچ secret یا keystore در این workflow استفاده یا ثبت نمی‌شود.
- راهنمای docs/release-assets.md فایل‌ها، روش بررسی checksum و روش آغاز انتشار draft را توضیح می‌دهد.

## محدودیت باقی‌مانده

اتصال GitHub در این نشست امکان POST ساخت تگ یا GitHub Release را ندارد؛ بنابراین هنوز تگ android-v0.1.0 و Release واقعی ایجاد نشده‌اند. Workflow و راهنمای اجرا در مخزن آماده شده‌اند، اما باید پس از ثبت این commit، signed-release روی main جدید اجرا شود و سپس تگ android-v0.1.0 به همان commit push شود. اجرای تگ باید Artifact نهایی و draft Release را بسازد و آزمون تأیید دارایی‌های Release سبز شود.

## وضعیت

T0213 در وضعیت in_progress می‌ماند. تولید manifest نامزد و پیاده‌سازی workflow به‌تنهایی اثبات انتشار واقعی نیست. تکمیل فقط پس از مشاهدهٔ موفق run تگ‌محور، Artifact whitep-release-assets، draft GitHub Release و وجود همهٔ فایل‌های checksum و توزیع در آن ثبت خواهد شد.

## اقدام بعدی

پس از ثبت commit این نوبت روی main:

1. workflow امضای تولیدی را روی main جدید اجرا و موفقیت آن را بررسی کن.
2. پس از موفقیت Product Build و Android signed release برای همان SHA، تگ android-v0.1.0 را روی همان commit push کن.
3. اجرای White-P release assets را بررسی کن؛ باید APK، ZIP نصب‌کننده، SHA256SUMS.txt و BUILD-PROVENANCE.txt را در Release به‌صورت draft قرار دهد.

## A002 — اصلاح placeholderهای Workflow پس از بازبینی V 3.0.984

- اجرای Quality Gate برای commit 9464a17076b13088624d060f674d617230c09508 با موفقیت پایان یافت: https://github.com/artinrahimii/project/actions/runs/42120944588.
- با وجود موفقیت تست‌های اولیه، بازبینی مستقیم فایل ثبت‌شده نشان داد backslashهای باقی‌مانده پیش از placeholderهای GitHub Actions و متغیرهای Bash می‌توانستند مانع expansion واقعی شوند. این ایراد پیش از dispatch کردن Workflow انتشار شناسایی شد؛ هیچ Release یا Artifact انتشار ناقصی منتشر نشد.
- تمام escapeهای اضافی پیش از placeholderهای GitHub Actions و متغیرهای محیطی Bash از فایل Workflow حذف شدند. تست قرارداد نیز تقویت شد تا حضور backslash پیش از placeholderها را رد کند و وجود token expression و GITHUB_REPOSITORY را بررسی کند.
- نتیجهٔ این اصلاح هنوز به commit جداگانه نیاز دارد؛ T0213 در وضعیت in_progress باقی می‌ماند. پس از commit، Quality Gate باید دوباره موفق شود و workflow فقط پس از signed-release موفق روی همان SHA و push تگ اجرا شود.


## A003 — بازبینی مستقل Artifactها و توقف امن (2026-11-06)

### مبنای دقیق و پیش‌نیازها

- HEAD شاخهٔ `main` هنگام این بررسی: `4bad2fbf1458369097bcfb137d5daec729bc24a3` با پیام `V 3.0.985`.
- گراف از Blob `db25484ca21f08c0cf4acacb72f0576061c07196` در همان commit دریافت شد؛ اندازهٔ اصلی 3,856,190 بایت و طول متن 3,726,678 نویسه بود. محتوای Blob مجدداً با Git Blob API ساخته شد و SHA حاصل دقیقاً با Blob ID درخت commit برابر بود. JSON کامل parse شد: 230 وظیفه، 422 یال، بدون شناسهٔ وظیفهٔ تکراری و بدون ارجاع یال حل‌نشده.
- یال `T0212 → T0213` همچنان `blocks / hard_dependency / validated` است. یال `T0208 → T0213` همچنان `blocks / sequencing_only / validated` است. یال `T0213 → T0214` هنوز `unreviewed` است و تغییر داده نشد.
- اجرای تولیدی Android signed release با run شمارهٔ `42114568183` و Product Build با run شمارهٔ `42113989702` هر دو موفق بودند، اما هر دو روی SHA قدیمی‌تر `179efa9fe07c3ba007885e9e3198eba620092617` (نسخهٔ `V 3.0.983`) اجرا شده‌اند؛ نه روی HEAD فعلی `main`.

### کنترل Artifactهای واقعی

- Artifact امضاشدهٔ Android، شناسهٔ `13455966209`، با digest ثبت‌شدهٔ GitHub برابر شد: `935a5d97b7ee32f4637867fceca00cb70809518b5494aeb43f0d8f35a5787825`. آزمون `unzip -t` موفق بود و یک `app-release.apk` غیرخالی با اندازهٔ 22,786,909 بایت داشت.
- Artifact نصب‌کنندهٔ Windows، شناسهٔ `13453864758`، با digest ثبت‌شدهٔ GitHub برابر شد: `5f117cfb85ce902440ab6d906a0aa74a43cbd6d5b18857d0af27e63b0a36051b`. آزمون `unzip -t` موفق بود؛ فایل نصب‌کنندهٔ `WhiteP-Setup-0.1.0-win-x64.exe` با اندازهٔ 61,460,809 بایت و دو فایل `ThirdPartyNotices.md` و `licenses.json` موجود بودند.
- هشت کنترل ایستا روی محتوای خام Blobهای workflow، مستندات و آزمون قرارداد انجام شد و هر 8 کنترل موفق بود. این کنترل ایستا جایگزین اجرای Python unittest یا اجرای workflow در GitHub Actions نیست.

### بازبینی manifest نامزد قبلی

- در بازسازی محلی بسته از Artifactهای دانلودشده، checksum مربوط به APK با مقدار نامزد برابر شد؛ اما checksum مربوط به ZIP بازسازی‌شدهٔ Windows و فایل provenance بازسازی‌شده برابر نبودند. ZIP محلی پس از استخراج دوباره ساخته شد و شامل ورودی‌های پوشه با زمان‌های ایجاد جدید بود؛ بنابراین بایت‌های آن با ZIP نامزد قبلی یکسان نبودند. محتوای دقیق provenance نامزد قبلی نیز در شواهد موجود نبود و بازسازی متنی آن تطبیق نکرد.
- این نتیجه به‌عنوان کنترل ناموفقِ بازپخش manifest نامزد ثبت می‌شود؛ نه به‌عنوان شکست workflow انتشار که هنوز اجرا نشده است. فایل `SHA256SUMS-candidate.txt` نباید به‌عنوان manifest رسمی Release استفاده شود. workflow باید در اجرای نهایی، ZIP و provenance همان run را بسازد و manifest تازه را روی همان فایل‌ها با `sha256sum --check` تأیید کند.

### مانع تعیین‌کننده و وضعیت

- workflow انتشار در `.github/workflows/release-assets.yml` فقط runهای موفق signing و product build با `head_sha == GITHUB_SHA` را می‌پذیرد. Artifactهای بررسی‌شده متعلق به SHA قبلی‌اند و برای HEAD فعلی قابل استفاده نیستند.
- برای تکمیل باید signing workflow و Product Build روی SHA فعلی `main` موفق شوند؛ سپس تگ `android-v0.1.0` دقیقاً روی همان SHA قرار گیرد و workflow انتشار، Artifact نهایی و Release پیش‌نویس را بسازد و تأیید کند.
- اتصال GitHub موجود در این نشست ابزار اجرای `workflow_dispatch`، ایجاد تگ یا ساخت Release را ارائه نمی‌کند. بنابراین ادامهٔ امن از این نقطه ممکن نیست و `T0213` به‌صورت `blocked` ثبت می‌شود؛ هیچ Release یا manifest رسمی ادعا نمی‌شود.

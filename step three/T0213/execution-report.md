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

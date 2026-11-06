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


## A004 — بازبینی مجدد main و کنترل امکان ادامه (2026-11-06)

### مبنای دقیق و اعتبارسنجی گراف

- شاخهٔ هدف: `main`؛ commit مبنا: `1e728bec371863f10b62a77354d0fd42459a57b1` با پیام `V 3.0.987`؛ درخت: `e959fe9ed2d9edebd36e7d129c0c739ebd0530f2`.
- فایل `step three/project-graph.md` از Blob همان درخت با شناسهٔ `bf0897613e2efbdc4b4052cf4d87996c2269cb7f` دریافت شد. طول متن 3,732,443 نویسه و اندازهٔ UTF-8 برابر 3,861,954 بایت بود؛ اندازه با مقدار درخت Git تطبیق داشت و Git Blob SHA-1 محاسبه‌شده دقیقاً با Blob ID برابر شد.
- JSON کامل تجزیه شد: 230 وظیفهٔ یکتا، 422 یال، 230 شناسهٔ یکتای ترتیب اجرا، صفر ارجاع یال مفقود و صفر شناسهٔ تکراری. وضعیت‌ها: 210 `done`، سه `deferred_final`، یک `blocked` و 16 `not_started`.
- از 422 یال، 385 مورد `validated` و 37 مورد `unreviewed` هستند. هیچ یالی در این نوبت تغییر نکرد.

### بررسی وابستگی‌ها و صف اجرا

- پیش‌نیازهای T0213 یعنی T0212 و T0208 همچنان `done` هستند. یال T0212 → T0213 از نوع `blocks / hard_dependency / validated` و یال T0208 → T0213 از نوع `blocks / sequencing_only / validated` باقی مانده‌اند.
- یال خروجی T0213 → T0214 هنوز `unreviewed` است و عمداً بدون شواهد تغییر داده نشد.
- هیچ وظیفهٔ `not_started` با تمام پیش‌نیازهای `done` وجود ندارد؛ بنابراین وظیفهٔ قابل‌اجرای بعدی مستقل از T0213 پیدا نشد. T0213 همچنان سرِ صف و مسدودکنندهٔ وظایف بعدی بخش انتشار است.

### کنترل مسیر انتشار و مانع

- فایل‌های `.github/workflows/android-release.yml` و `.github/workflows/release-assets.yml` بازبینی شدند. امضای تولیدی و انتشار دارایی‌ها فقط با `workflow_dispatch` یا تگ `android-v*` آغاز می‌شوند؛ Product Build نیز روی push به `main` تعریف شده است.
- اتصال GitHub در این نشست ابزار اجرای `workflow_dispatch`، ساخت تگ یا ایجاد Release را ارائه نمی‌کند. فهرست GitHub Releases در درخواست این نوبت خالی برگشت. endpoint فهرست تگ‌ها از اتصال قابل دریافت نبود؛ بنابراین دربارهٔ نبود تگ ادعای قطعی نمی‌شود.
- artifactهای ثبت‌شده در شواهد پیشین متعلق به SHA قدیمی `179efa9fe07c3ba007885e9e3198eba620092617` هستند و نباید برای انتشار از HEAD فعلی استفاده شوند. در این نوبت هیچ build، آزمون Python/.NET/Gradle یا workflow انتشار اجرا نشد و هیچ artifact یا Release جدیدی تولید نشد.

### نتیجه

T0213 در وضعیت `blocked` باقی می‌ماند؛ تعداد تلاش‌ها از 3 به 4 افزایش یافت و تعداد شکست‌ها همان 1 باقی ماند. این نوبت صرفاً بازبینی وضعیت و ثبت شواهد اعتبارسنجی است و پیشرفت نرم‌افزاری یا تکمیل وظیفه محسوب نمی‌شود.

### اقدام لازم برای ادامه

1. workflow امضای تولیدی Android را از GitHub Actions روی HEAD فعلی `main` اجرا و موفقیت آن را تأیید کن.
2. موفقیت Product Build را نیز برای همان SHA بررسی کن.
3. تگ `android-v0.1.0` را دقیقاً روی همان SHA ایجاد کن تا workflow انتشار اجرا شود.
4. Artifact `whitep-release-assets`، فایل `SHA256SUMS.txt`، فایل provenance و دارایی‌های Release پیش‌نویس را بررسی کن؛ سپس یال T0213 → T0214 را پیش از شروع T0214 اعتبارسنجی کن.


## A005 — اصلاح ناسازگاری شمارندهٔ Graph Guard (2026-11-06)

- مبنا: شاخهٔ `main`، commit `c21328f0e0052d1bf47a613bc8ad48915bd8646f` با پیام `V 3.0.988`.
- اجرای Quality Gate شمارهٔ `42125796616` پس از اجرای 436 آزمون Python موفق و بررسی syntax موفق، در مرحلهٔ Graph Guard شکست خورد.
- خطای دقیق: `FAILED_ATTEMPT_COUNT_MISMATCH: T0213: summary=1, history=0`.
- قاعدهٔ واقعی `tools/graph_guard.py`، تلاش ناموفق را با `execution_history[].result == "failed"` می‌شمارد. چهار رکورد T0213 به‌ترتیب `partial`, `partial`, `blocked`, `blocked` هستند؛ بنابراین مقدار صحیح `failed_attempt_count` برابر صفر است.
- فقط شمارندهٔ خلاصه از 1 به 0 اصلاح شد. هیچ رکوردی از تاریخچه حذف یا بازنویسی نشد، تلاش تازه‌ای جعل نشد و وضعیت `blocked` وظیفه تغییر نکرد.
- این تغییر صرفاً اصلاح یکپارچگی دفتر ثبت و رفع خطای Graph Guard است؛ تکمیل T0213، اجرای workflow انتشار یا پیشرفت قابلیت محصول محسوب نمی‌شود.


## A006 — بازبینی اجرای CI روی V 3.0.989 و محدودیت اجرای انتشار (2026-11-06)

- مبنای دقیق: شاخهٔ `main`، commit `80530a9c5c096c809f98df51688cdaf1a1a9e88c` با پیام `V 3.0.989`.
- Quality Gate با run شمارهٔ `42126067192` موفق شد: 436 آزمون Python، بررسی syntax، اعتبارسنجی ساختار گراف و بررسی خروجی/شواهد همگی موفق شدند؛ یک هشدار تاریخی T0001 حفظ شد.
- Rendezvous container run شمارهٔ `42126067045` با موفقیت build و smoke test شد.
- Product Build شمارهٔ `42126067068` روی همین SHA در زمان آخرین بررسی هنوز `in_progress` بود. jobهای HostLifecycle و Windows WPF/installer موفق شدند؛ نصب‌کنندهٔ Windows با artifact ID `13460797526`، digest `sha256:2bd6a783bf1ce05569ed4e5a1730ee0df4fef8d5444cf5ee648fd457e6c0c302` بارگذاری شد. artifact میزبان Windows با شناسهٔ `13461581726` و گزارش coverage چرخهٔ عمر با شناسهٔ `13461565694` نیز موجود بودند.
- در Android، lint، ساخت Debug APK، ساخت Release با کلید موقت CI، بررسی apksigner برای همان کلید موقت، کنترل مجوزها و JVM unit tests موفق شدند؛ اما Android Keystore instrumentation همچنان `in_progress` بود و مراحل upload و coverage نهایی هنوز تکمیل نشده بودند. بنابراین Product Build را موفق/کامل اعلام نمی‌کنم.
- برای همین SHA هیچ اجرای Android signed release با نتیجهٔ موفق وجود نداشت. اجرای موفق قبلی `42114568183` متعلق به SHA قدیمی `179efa9fe07c3ba007885e9e3198eba620092617` است و جایگزین artifact تولیدی همان commit نمی‌شود.
- endpoint جست‌وجوی tag refs و فهرست Releases در این بررسی آرایهٔ خالی برگرداند. ابزارهای متصل، عملیات `workflow_dispatch`، ایجاد Git tag یا ایجاد Release را ارائه نمی‌کنند. workflowها را با تغییر ناامن trigger به push خودکار روی main دور نزدم.
- تلاش T0213-A005 با نتیجهٔ `blocked` ثبت شد؛ تعداد تلاش‌ها 5 و تعداد تلاش‌های ناموفق واقعی 0 است. وضعیت `blocked` باقی ماند و هیچ یالی تغییر نکرد.


## A007 — تکمیل بستهٔ انتشار و اصلاح کنترل Draft Release (2026-11-06)

### نتیجهٔ اجرای tag

- تگ `android-v0.1.0` در GitHub ثبت شد و به کامیت دقیق `cca243d28197df90900885a35a54a8ca8ae3751e` (پیام `V 3.0.990`) اشاره می‌کند؛ خود GitHub Git Tag API نیز annotated tag object را به همین commit resolve کرد.
- Android signed release دستی روی همان SHA با run شمارهٔ `42127523912` موفق شد؛ اثرانگشت SHA-256 گواهی تولیدی `3d3875fbb136d7522221fe11bf4439783ebb299e50b3f0474ae26ea83970ccda` با fingerprint محافظت‌شده تطبیق داده شد. Artifact امضاشده: `13460878301`.
- Android signed release تگ‌محور با run شمارهٔ `42253832986` نیز روی همان SHA موفق شد.
- Product Build run شمارهٔ `42127185955` روی همان SHA موفق شد؛ سه Job مربوط به Android، Windows و HostLifecycle همگی موفق بودند.

### بستهٔ انتشار و checksumها

- Release Assets run شمارهٔ `42253832983` APK امضاشده و Windows installer را از runهای موفق همان SHA دانلود کرد؛ ZIP نصب‌کننده همراه با license inventory بسته‌بندی شد.
- گام تولید checksum در همان run موفق شد: هر سه ورودی `WhiteP-Android-0.1.0.apk`، `WhiteP-Windows-Installer.zip` و `BUILD-PROVENANCE.txt` در `sha256sum --check SHA256SUMS.txt` برابر `OK` بودند.
- Artifact نهایی `whitep-release-assets` با شناسهٔ `13521278559` بارگذاری شد. ZIP آن مستقل دریافت و استخراج شد؛ هر چهار فایل مورد انتظار وجود داشتند و بررسی مجدد محلی SHA-256 برای هر سه ورودی موفق شد.
- provenance بسته: `source_commit=cca243d28197df90900885a35a54a8ca8ae3751e`, `android_signed_release_run_id=42127523912`, `product_build_run_id=42127185955`.
- GitHub Draft Release با شناسهٔ `436740935`، `tag_name=android-v0.1.0`، `draft=true` ایجاد شد و هر چهار دارایی APK، Windows ZIP، SHA256SUMS و BUILD-PROVENANCE در آن با state=uploaded قرار گرفتند.

### علت conclusion=Failure و اصلاح

- conclusion کلی Release Assets run برابر `failure` بود، اما فقط آخرین assertion شکست خورد. Workflow از `GET /releases/tags/{tag}` برای خواندن Draft استفاده کرده بود؛ طبق مستندات GitHub این endpoint فقط Releaseهای منتشرشده را برمی‌گرداند و برای Draft انتظار 404 دارد. GET فهرست Releaseها، Draft را با tag و چهار دارایی برگرداند؛ بنابراین خروجی انتشار و checksum سالم‌اند و خطا، نقص کنترل وضعیت Draft بود.
- URL مرورگر Draft با پیشوند `untagged-` مطابق رفتار GitHub برای Draft است و به‌تنهایی نشانهٔ جداشدن تگ نیست؛ Git Tag API و فیلد `tag_name` تطبیق صحیح را تأیید کردند.
- برای جلوگیری از تکرار مشکل، `.github/workflows/release-assets.yml` اصلاح شد تا شناسایی و تأیید Draft را از endpoint فهرست Releaseها با صفحه‌بندی انجام دهد؛ Draft موجود را پیدا کند، فقط Draft را به‌روزرسانی کند و همچنان از تغییر Release منتشرشده خودداری کند. تست رگرسیون در `tests/test_release_assets_contract.py` افزوده و یادداشت مستندات به‌روز شد.

### وضعیت در این commit

- خروجی مورد انتظار T0213 تولید و مستقل اعتبارسنجی شده است. هیچ artifact قدیمی از SHA دیگری استفاده نشد و هیچ تگی جابه‌جا یا حذف نشد.
- وضعیت وظیفه موقتاً `in_progress` می‌ماند تا Quality Gate پس از اصلاح کنترل Draft در commit این نوبت نیز تأیید شود؛ پس از موفقیت آن، وضعیت نهایی به `done` تغییر می‌کند.

## A008 — تأیید نهایی و تکمیل T0213 (2026-11-06)

### مبنای دقیق انتشار

- آخرین commit شاخهٔ `main` پیش از این ثبت: `98e808bb886ebd9586bee62f5f22ff73404d4d3d`، پیام `V 3.0.991`.
- تگ annotated `android-v0.1.0` به commit `cca243d28197df90900885a35a54a8ca8ae3751e` (پیام `V 3.0.990`) اشاره می‌کند. تگ جابه‌جا یا بازنویسی نشده است؛ artifactهای تولیدی و Product Build هر دو متعلق به همین SHA هستند.
- اجرای امضای دستی `42127523912` و اجرای امضای tag-triggered `42253832986` موفق شدند. اثرانگشت گواهی APK تولیدی `3d3875fbb136d7522221fe11bf4439783ebb299e50b3f0474ae26ea83970ccda` با fingerprint پین‌شدهٔ محیط محافظت‌شده تطبیق داده شد.
- Product Build `42127185955` روی همان SHA موفق شد؛ Jobهای Android، Windows و HostLifecycle همگی success بودند.

### Artifact نهایی و کنترل checksum مستقل

- Artifact نهایی `whitep-release-assets` با شناسهٔ `13521278559` از GitHub دریافت شد؛ اندازهٔ ZIP برابر 79,549,283 بایت و digest آن `sha256:7758e32252dd83e1a1a31f253c0cbb919419ba8cf6f00ff66eedb82522154c53` بود. SHA-256 محاسبه‌شدهٔ فایل دانلودشده دقیقاً با digest ثبت‌شده در GitHub برابر شد.
- آزمون `unzip -t` روی Artifact نهایی بدون خطا بود. چهار فایل مورد انتظار استخراج شدند:
  - `WhiteP-Android-0.1.0.apk` — 22,786,909 بایت
  - `WhiteP-Windows-Installer.zip` — 60,961,390 بایت
  - `SHA256SUMS.txt` — 273 بایت
  - `BUILD-PROVENANCE.txt` — 130 بایت
- فایل manifest رسمی در `step three/T0213/evidence/SHA256SUMS-final.txt` نیز عیناً ثبت شده است. اجرای مستقل `sha256sum --check SHA256SUMS.txt` برای هر سه ورودی APK، بستهٔ Windows و provenance نتیجهٔ `OK` داد.
- محتوای provenance دقیقاً این داده‌ها را ثبت می‌کند: `source_commit=cca243d28197df90900885a35a54a8ca8ae3751e`، `android_signed_release_run_id=42127523912` و `product_build_run_id=42127185955`.

### Draft Release و اصلاح کنترل Workflow

- GitHub Release با شناسهٔ `436740935`، مقدار `tag_name=android-v0.1.0` و `draft=true` در فهرست Releases تأیید شد. هر چهار دارایی مورد انتظار با state برابر `uploaded` وجود دارند.
- اجرای tag-triggered Release Assets با شمارهٔ `42253832983` در مجموع conclusion برابر failure داشت؛ با این حال مراحل انتخاب buildهای همان SHA، دانلود Artifactها، تولید و بررسی سه checksum و بارگذاری Artifact موفق شدند. فقط assertion پایانی، Draft را از endpoint مخصوص Release منتشرشده جست‌وجو کرد و 404 گرفت. این خطا به معنی فقدان Draft نبود؛ Release از فهرست Releases و شناسهٔ مستقیم تأیید شد.
- اصلاح این کنترل در `.github/workflows/release-assets.yml` و تست رگرسیون `test_draft_release_lookup_uses_list_endpoint` در commit `98e808bb886ebd9586bee62f5f22ff73404d4d3d` ثبت شدند.
- پس از این اصلاح، Quality Gate run `42255409787` روی commit `V 3.0.991` موفق شد: 437 آزمون Python، compileall، graph structure و output/evidence validation موفق؛ 0 خطا و یک هشدار تاریخی T0001 حفظ شد. Product Build run `42255409532` و Rendezvous run `42255409556` نیز روی همان commit موفق شدند.
- Draft عمداً منتشر عمومی نشده است؛ این وضعیت با سیاست نگه‌داشتن انتشار در حالت پیش‌نویس تا پذیرش نهایی بعدی سازگار است.

### نتیجه

معیارهای T0213 با artifact واقعی، manifest رسمی، کنترل checksum مستقل، provenance و Draft Release تأیید شدند. وضعیت وظیفه به `done` تغییر می‌کند. یال‌های ورودی T0212 و T0208 بدون تغییر باقی ماندند؛ یال خروجی `T0213 → T0214` همچنان `unreviewed` است و پیش از شروع T0214 باید جداگانه اعتبارسنجی شود.

## A009 — اصلاح مسیرهای خروجی برای Graph Guard (2026-11-06)

- Quality Gate run `42257992342` روی commit `05ef56da28cb7fc532c9e90dcac78cf92a1818ea` (V 3.0.992) در آزمون‌ها، syntax و graph structure موفق بود؛ فقط مرحلهٔ `Validate recorded outputs and evidence` شکست خورد.
- علت دقیق دو خطای `MISSING_OUTPUT_FILE` این بود که شناسه‌های GitHub Draft Release و Artifact نهایی در `actual_output_paths` به‌عنوان مسیر فایل محلی قرار گرفته بودند. این دو منبع آنلاین هستند، نه فایل‌های داخل repository.
- فقط دو مقدار `GitHub Draft Release ID 436740935` و `GitHub workflow artifact whitep-release-assets ID 13521278559` از `actual_output_paths` برداشته شدند. نشانی‌ها و شناسه‌های آن‌ها در `evidence` حفظ شدند و فایل واقعی `step three/T0213/evidence/SHA256SUMS-final.txt` همچنان در مسیر خروجی محلی باقی است.
- هیچ تگ، Release، Artifact، checksum، فایل محصول یا یال وابستگی تغییر نکرد. این اصلاح فقط تمایز بین فایل‌های محلی و شواهد آنلاین را با قرارداد Graph Guard هماهنگ می‌کند.
- پس از ثبت این اصلاح، Quality Gate جدید باید مرحلهٔ `Validate recorded outputs and evidence` را نیز با موفقیت بگذراند.

# گزارش اجرای T0215

## هدف
گنجاندن فهرست وابستگی‌ها، متن مجوزها و اعلامیهٔ مجوز مدل نمونه در بستهٔ نصب ویندوز و در بستهٔ ZIP انتشار.

## بررسی یال‌ها
- T0214 → T0215: نوع اصلی `blocks` حفظ شد؛ طبقه‌بندی `contract_or_interface_dependency` و `validation_status=validated` ثبت شد. T0215 قرارداد artifact ویندوز تولیدشده در T0214 (مسیر installer و پوشهٔ windows-installer/licenses) را مصرف می‌کند.
- T0208 → T0215: نوع اصلی `blocks` حفظ شد؛ طبقه‌بندی `sequencing_only` و `validation_status=validated` ثبت شد. T0215 گزارش‌های coverage و critical-test تولیدشده در T0208 را ورودی فنی خود نمی‌گیرد.
- دلیل و شواهد هر دو یال در edge_change_log ثبت شده‌اند؛ ترتیب محافظه‌کارانه تغییر نکرد.

## تلاش T0215-A001 — پیاده‌سازی و آزمون بسته‌بندی

- مولد fail-closed برای notice مدل نمونه و manifest JSON اضافه شد.
- متن Apache-2.0 از منبع رسمی دریافت شد و اندازه، UTF-8 و markerهای آن کنترل شدند.
- فقط license با status=verified و identifier=Apache-2.0 پذیرفته شد؛ classification غایب به unknown باقی ماند.
- وزن‌های مدل در installer یا بستهٔ انتشار قرار نگرفتند.
- Quality Gate run 42268763411 روی commit 78924a6da3eb4aaf96308a80041ce091b3deb250 موفق شد: 446 آزمون پایتون، compileall، بررسی ساختار گراف و شواهد؛ Graph Guard تعداد 230 task و 422 edge را با صفر خطا و یک هشدار ثبت‌شده گزارش کرد.
- Product Build run 42268763399 روی همان commit موفق شد؛ هر سه job Windows، Android و HostLifecycle موفق شدند. در Windows، آزمون مولد مجوز مدل و آزمون مجوزهای NuGet، restore/build/publish، ساخت و نصب آزمایشی installer، smoke launch، کنترل notice و upload artifact موفق شدند. Android instrumentation نیز روی emulator موفق شد.
- Rendezvous run 42268763422 نیز موفق شد.
- Artifact نصب‌کنندهٔ Windows با ID برابر 13528547357 و اندازهٔ 60985518 بایت دریافت و بازبینی شد. ZIP شامل 28 entry بود و integrity check خطایی گزارش نکرد. فایل‌های ModelLicenses.md، model-licenses.json، متن کامل Apache-2.0، ThirdPartyNotices.md و licenses.json در آن حاضر بودند. SHA-256 متن مجوز با مقدار manifest برابر بود: cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30.
- بازبینی دقیق‌تر نشان داد source_catalog_sha256 داخل artifact برابر 27455158559da383eb9c44ffaf9989c362f6e2353cf318df02622f7069a9def4 است؛ این مقدار با نسخهٔ CRLF فایل کاتالوگ منطبق بود، درحالی‌که Git Blob همان فایل در commit مبنا، 711 بایت UTF-8 با LF است و SHA-256 آن faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580 است. بنابراین هش catalog به line ending محیط checkout وابسته بود و cross-platform ثابت نبود. این ایراد در آزمون‌های اولیه دیده نشد.

## تلاش T0215-A002 — اصلاح پایداری هش catalog

- مولد اکنون CRLF را پیش از محاسبهٔ هش به LF تبدیل می‌کند و bare CR را رد می‌کند.
- manifest و notice سیاست canonicalization را صریح ثبت می‌کنند.
- آزمون regression تازه، برابری hash برای فایل LF و CRLF را کنترل می‌کند.
- این اصلاح هنوز روی CI اجرا نشده است؛ بنابراین موفقیت آزمون جدید یا نسخهٔ اصلاح‌شدهٔ Artifact را ادعا نمی‌کنم.

## محدودیت انتشار GitHub

پیش‌نویس Release موجود، ID 436740935 با tag برابر android-v0.1.0، قدیمی است. شناسهٔ ad90a80f98321602dc57a1d3c89113cefa86a0e1 شناسهٔ شیء annotated tag است، نه commit مقصد؛ با بازکردن tag object، commit مقصد `cca243d28197df90900885a35a54a8ca8ae3751e` با پیام `V 3.0.990` به دست می‌آید. بنابراین این Release به نسخهٔ فعلی main متصل نیست و artifactهای آن مجوز مدل جدید را ندارند. ابزار GitHub متصل‌شده در این نشست عملیات ساخت tag یا `workflow_dispatch` را ارائه نمی‌کند؛ بنابراین Release جدید یا به‌روزرسانی draft قدیمی انجام نشده است.

## وضعیت فعلی

T0215 در `in_progress` باقی می‌ماند. بستن وظیفه منوط به موفقیت CI برای اصلاح A002 و تولید/اعتبارسنجی بستهٔ Release متناظر با همان commit جدید است.


## A003 — راستی‌آزمایی اصلاح هش و Artifact روی main فعلی

**شاخه و commit بررسی‌شده:** `main` / `b93cd2088de0f19d6ee7b465ffab72afddc39f3f` (`V 3.0.998`)

### CI واقعی روی همین commit
- Quality Gate run [42277720293](https://github.com/artinrahimii/project/actions/runs/42277720293) با موفقیت پایان یافت: ۴۴۷ آزمون Python، compileall، کنترل گراف و کنترل خروجی‌ها/شواهد موفق شدند. Graph Guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را با صفر خطا و یک هشدار ثبت‌شده گزارش کرد؛ هشدار وابستگی متنی T0001 به شناسهٔ وظیفه نبود و به‌درستی حذف یا تبدیل نشد.
- Product Build run [42277720362](https://github.com/artinrahimii/project/actions/runs/42277720362) با موفقیت پایان یافت و jobهای Windows، Android و HostLifecycle همگی موفق شدند.
- در job ویندوز، ۸ آزمون مولد مجوز مدل و ۷ آزمون inventory مجوز وابستگی موفق شدند. آزمون تازهٔ `test_catalog_hash_is_stable_across_lf_and_crlf_checkouts` نیز موفق بود. Installer واقعاً build، نصب بی‌صدا، smoke-launch، بررسی وجود noticeها و uninstall شد.
- این workflow موفقیت ساخت و آزمون نسخه را نشان می‌دهد؛ جایگزین انتشار signed Android یا انتشار GitHub Release نیست.

### راستی‌آزمایی Artifact واقعی نصب‌کننده
- Artifact: [whitep-windows-installer — ID 13534875582](https://api.github.com/repos/artinrahimii/project/actions/artifacts/13534875582/zip)
- اندازهٔ دانلودشده: ۶۰٬۹۸۵٬۵۲۷ بایت؛ ۲۸ ورودی ZIP؛ آزمون integrity بدون خطا.
- پنج فایل مورد انتظار حاضر بودند: `ModelLicenses.md`، `model-licenses.json`، متن کامل Apache-2.0، `ThirdPartyNotices.md` و `licenses.json`.
- کاتالوگ نمونه در commit مبنا، فایل ۷۱۱ بایتی با Git Blob SHA برابر `f91c7cb68c151b216260fce14c2f30a8a027a06c` است. Git Blob SHA-1 محاسبه‌شده از بایت‌های اصلی دقیقاً با درخت همان commit تطبیق کرد.
- SHA-256 بایت‌های دقیق کاتالوگ LF برابر `faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580` شد؛ همین مقدار در فیلد `source_catalog_sha256` در manifest Artifact قرار دارد. بنابراین ایراد حساسیت به CRLF در A001 با اصلاح A002 رفع و به‌صورت واقعی تأیید شد.
- متن Apache-2.0 داخل ZIP برابر ۱۱٬۳۵۸ بایت و SHA-256 آن `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30` است؛ با هش manifest تطبیق دارد.

### مانع انتشار باقی‌مانده
- Release پیش‌نویس 436740935 هنوز از tag قدیمی `android-v0.1.0` استفاده می‌کند. annotated tag object آن `ad90a80f98321602dc57a1d3c89113cefa86a0e1` به commit `cca243d28197df90900885a35a54a8ca8ae3751e` با پیام `V 3.0.990` اشاره می‌کند، نه commit فعلی main.
- workflow `android-release.yml` به tag جدید `android-v*` یا اجرای دستی نیاز دارد و از محیط محافظت‌شدهٔ امضای تولید استفاده می‌کند. اتصال GitHub موجود در این نشست عملیات ساخت tag یا `workflow_dispatch` را ارائه نمی‌کند. هیچ tag یا Release موجودی تغییر نکرده است.

## وضعیت فعلی

- A001: جزئیات بسته‌بندی و ایراد اولیه ثبت شد؛ نتیجهٔ آن partial باقی می‌ماند.
- A002: **موفق**؛ اصلاح هش LF/CRLF در commit `80cf6082e365ce5c581a95d4ebd4dae15441e45b` ثبت شده و CI و Artifact جدید آن را تأیید کردند.
- A003: **partial**؛ راستی‌آزمایی CI و Artifact موفق است، اما GitHub Release تازهٔ signed روی commit فعلی هنوز وجود ندارد.
- بنابراین T0215 هنوز `in_progress` است و تکمیل‌شده اعلام نمی‌شود. تا رفع مانع Release، T0216 شروع نمی‌شود.


## راستی‌آزمایی تکمیلی پس از A003 — main / V 3.0.999

- commit بررسی‌شده: `4f6adb09e592fc8a14e41ffc7fa9aa9407fd40a7`؛ این commit فقط گراف و شواهد اجرایی را به‌روزرسانی کرده و کد محصول T0215 را تغییر نداده است.
- Quality Gate [42286282408](https://github.com/artinrahimii/project/actions/runs/42286282408)، Product Build [42286282513](https://github.com/artinrahimii/project/actions/runs/42286282513) و Rendezvous [42286282640](https://github.com/artinrahimii/project/actions/runs/42286282640) همگی روی همین commit با نتیجهٔ success پایان یافتند. در Product Build، jobهای Windows، Android و HostLifecycle و تمام گام‌های گزارش‌شده موفق بودند.
- Artifact تازهٔ Windows installer با شناسهٔ `13536989328` و اندازهٔ `60,985,523` بایت دانلود شد. ZIP دارای ۲۸ ورودی بود و `testzip()` هیچ خرابی گزارش نکرد. پنج فایل `ModelLicenses.md`، `model-licenses.json`، متن کامل Apache-2.0، `ThirdPartyNotices.md` و `licenses.json` در بسته حاضر بودند.
- SHA-256 کل ZIP برابر `c21d1692402c5ea23a5a5f626e111744a5ee9dfc6d90ee6beba3c31946e2d5ab` است. مقدار `source_catalog_sha256` در manifest برابر `faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580` و مقدار `license_text_sha256` برابر `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30` است؛ هر دو با شواهد منبع قبلی تطبیق دارند. وزن مدل در بسته وجود ندارد.
- این اجرا همچنان اثبات انتشار اندروید production-signed نیست. Product Build، APK دیباگ را artifact می‌کند و variant انتشار را با کلید موقت CI می‌سازد. workflow امضای تولیدی `android-release.yml` به secrets محیط محافظت‌شدهٔ `android-release` احتیاج دارد.
- Draft Release `436740935` هنوز به tag `android-v0.1.0` وصل است؛ tag object `ad90a80f98321602dc57a1d3c89113cefa86a0e1` به commit `cca243d28197df90900885a35a54a8ca8ae3751e` (`V 3.0.990`) اشاره می‌کند، نه main فعلی. هر دو workflow انتشار فقط از طریق push یک tag با الگوی `android-v*` یا `workflow_dispatch` اجرا می‌شوند. ابزارهای در دسترس این نشست tag creation و `workflow_dispatch` را پشتیبانی نمی‌کنند و هیچ tag یا Release قدیمی تغییر داده نشد.
- **نتیجه:** بسته‌بندی و CI ویندوز روی commit جاری تأیید شد، اما انتشار GitHub روی همان commit هنوز وجود ندارد؛ `T0215` همچنان `in_progress` است و `T0216` آغاز نمی‌شود.


## A004 — راستی‌آزمایی مجدد Release موجود (2026-11-07)

- آخرین commit main در شروع بررسی `8665c9bef63ac60f57250cca43ebadef2fb3ca44` با پیام `V 3.0.1000` بود.
- Draft Release `436740935` واقعاً چهار asset بارگذاری‌شده دارد: provenance، checksum، APK و ZIP نصب ویندوز. این موضوع، یادداشت قدیمی A003 مبنی بر نبود asset را اصلاح می‌کند.
- اما tag `android-v0.1.0` از طریق annotated tag object `ad90a80f98321602dc57a1d3c89113cefa86a0e1` به commit `cca243d28197df90900885a35a54a8ca8ae3751e` (`V 3.0.990`) اشاره دارد؛ نه main فعلی.
- در commit tagged، فایل `tools/generate_model_license_notice.py` موجود نیست و اسناد و اسکریپت installer/release نیز ادغام notice مجوز مدل را ندارند. پس assetهای آن Draft، خروجی انتشار پس از پیاده‌سازی T0215 نیستند. محتویات ZIP Draft مستقیماً دانلود نشد؛ این نتیجه بر پایهٔ commit دقیق source و workflowای است که روی همان tag اجرا می‌شود، نه ادعای بررسی مستقیم ZIP.
- ابزارهای فعلی این نشست امکان ساخت tag یا اجرای `workflow_dispatch` در محیط محافظت‌شدهٔ امضای Android را ارائه نمی‌کنند. هیچ tag، Release یا artifact قدیمی تغییر نکرد.
- **نتیجه:** T0215 همچنان `in_progress` است؛ وظیفهٔ بعدی T0216 شروع نمی‌شود. این A004 فقط راستی‌آزمایی و اصلاح ثبت شواهد بود و هیچ قابلیت نرم‌افزاری تازه‌ای را پیاده‌سازی نکرد.



## T0215-A005 — اجرای واقعی tag و رفع خطای ایجاد Draft Release (2026-11-07)

### مبنای اجرای این نوبت
- شاخهٔ بررسی‌شده: `main`
- commit مبنا: `97612f7e1b4eec759bf931563bffc1234e99782b` با پیام `V 3.0.1001`.
- تگ `android-v0.1.0-build.2` به همین commit اشاره می‌کند و با force یا جابه‌جایی تغییر داده نشد.
- پیش‌نیازهای گرافی T0215 یعنی T0214 و T0208 هر دو `done` هستند. یال‌های T0214 → T0215 و T0208 → T0215 معتبرسازی شده‌اند؛ یال T0215 → T0216 همچنان `unreviewed` باقی می‌ماند.

### اجرای واقعی روی همان commit
- Android signed release دستی، run [42295764549](https://github.com/artinrahimii/project/actions/runs/42295764549): `success` روی `97612f7e1b4eec759bf931563bffc1234e99782b` و شاخهٔ `main`. artifact امضاشدهٔ `whitep-android-release-signed` با ID `13541342893` موجود بود.
- Product Build، run [42292631561](https://github.com/artinrahimii/project/actions/runs/42292631561): `success` روی همان commit.
- پس از ارسال تگ `android-v0.1.0-build.2`، Android signed release خودکار نیز run [42297408066](https://github.com/artinrahimii/project/actions/runs/42297408066) را با نتیجهٔ `success` روی همان SHA گذراند؛ artifact امضاشده با ID `13542941760` بارگذاری شد.
- Release assets run [42297408035](https://github.com/artinrahimii/project/actions/runs/42297408035) همان SHA را دریافت کرد. مراحل checkout، یافتن buildهای هم‌کامیت، دریافت APK و Windows installer، کنترل پنج فایل مجوز و notice و ساخت/تأیید سه SHA-256 موفق شدند.
- artifact موقت `whitep-release-assets` با ID `13541643378` و اندازهٔ 79,556,087 بایت بارگذاری شد؛ این artifact شامل چهار فایل خروجی مورد انتظار است: APK، ZIP نصب ویندوز، checksum و provenance. لاگ مرحلهٔ checksum هر سه فایل را `OK` گزارش کرد.

### علت شکست و اصلاح
- تنها مرحلهٔ ناموفق Release assets، مرحلهٔ 7 با نام `Create or update the draft GitHub Release for an android-v tag` بود؛ job ID برابر `128166125176`.
- لاگ run 42297408035 خطای دقیق زیر را ثبت کرد: `the --slurp option is not supported with --jq or --template`.
- علت، ترکیب نامعتبر `gh api --paginate --slurp --jq` در تابع `find_release()` بود؛ بنابراین اجرای انتشار پیش از ایجاد Draft متوقف شد. این شکست مربوط به CLI lookup است، نه امضای APK، build ویندوز، فایل‌های مجوز یا checksum.
- در این نوبت تابع lookup اصلاح شد تا `gh api --paginate` پاسخ صفحه‌ها را به `jq -s` pipe کند و صفحه‌ها را با هم ادغام و tag موردنظر را انتخاب کند. آزمون قرارداد `test_draft_release_lookup_uses_list_endpoint` نیز به‌روزرسانی شد تا استفادهٔ مجدد از `--slurp` ممنوع باشد.
- این اصلاح و آزمون قرارداد هنوز در CI اجرا نشده‌اند؛ در نتیجه موفقیت fix را تا پایان Quality Gate و اجرای مجدد release workflow ادعا نمی‌کنم.

### وضعیت انتشار و ادامه
- تگ `android-v0.1.0-build.2` به commit پیش از اصلاح lookup اشاره می‌کند و نباید جابه‌جا یا بازنویسی شود.
- در زمان بررسی، تگ `android-v0.1.0-build.3` وجود نداشت. Draft قدیمی `436740935` همچنان به تگ `android-v0.1.0` و commit قدیمی `V 3.0.990` وصل است؛ آن را تغییر ندادم.
- پس از تأیید CI روی commit اصلاح‌شده، یک اجرای دستی Android signed release روی همان SHA جدید لازم است؛ سپس باید تگ تازهٔ `android-v0.1.0-build.3` روی همان SHA ساخته و push شود. push تگ هر دو workflow انتشار را فعال می‌کند. اتصال GitHub این نوبت امکان dispatch کردن workflow یا ساخت/push تگ را ندارد.
- **نتیجه:** T0215 همچنان `in_progress` است. T0216 آغاز نمی‌شود؛ یال خروجی آن همچنان بررسی‌نشده است.

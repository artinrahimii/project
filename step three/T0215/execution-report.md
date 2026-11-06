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

پیش‌نویس Release موجود، ID 436740935 با tag برابر android-v0.1.0، به commit قدیمی‌تر ad90a80f98321602dc57a1d3c89113cefa86a0e1 اشاره دارد و artifactهای آن مجوز مدل جدید را ندارند. workflow انتشار به build امضاشدهٔ اندروید و Product Build موفق روی یک SHA واحد و سپس tag جدید نیاز دارد. ابزار GitHub متصل‌شده در این نشست عملیات workflow_dispatch یا ساخت tag را ارائه نمی‌کند؛ بنابراین release جدید یا به‌روزرسانی draft قدیمی انجام نشده و انجام‌شده گزارش نمی‌شود.

## وضعیت فعلی

T0215 در `in_progress` باقی می‌ماند. بستن وظیفه منوط به موفقیت CI برای اصلاح A002 و تولید/اعتبارسنجی بستهٔ Release متناظر با همان commit جدید است.

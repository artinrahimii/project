# T0187 — آزمون اسکن log و فایل‌های موقت برای prompt، capsule و اسرار

- **وضعیت:** در حال اجرا؛ تست‌های runtime و source-contract افزوده شده‌اند، اما CI دقیق commit هنوز اجرا/تأیید نشده است.
- **مبنای بررسی:** مخزن `artinrahimii/project`، شاخه `main`، commit `e528d6d9e9daeb5a4c9540dfefe7d136770b90af`، نسخهٔ `V 3.0.876`.
- **پیش‌نیازها:** `T0186` و `T0177` هر دو `done` بودند و خروجی‌های واقعی و آزمون‌هایشان بررسی شدند.
- **بازبینی یال‌ها:**
  - `T0186 → T0187`: `sequencing_only` / `validated`; type اصلی `blocks` و ترتیب حفظ شد. T0186 رابط رضایت می‌سازد، اما T0187 از API/خروجی آن استفاده نمی‌کند.
  - `T0177 → T0187`: `sequencing_only` / `validated`; type اصلی `blocks` و ترتیب حفظ شد. آزمون‌های log و فایل موقت هیچ قرارداد selector/attempt-budget را مصرف نمی‌کنند.
  - یال خروجی `T0187 → T0188` بدون بررسی باقی ماند.
- **Windows temp/history runtime:** آزمون یک پوشهٔ موقت اختصاصی می‌سازد؛ ابتدا وجود canaryهای ساختگی prompt/capsule/secret را با scanner تأیید می‌کند، سپس `WindowsHostConversationStore` فایل stale را پاک می‌کند و نبود marker در همهٔ فایل‌های باقی‌مانده را بررسی می‌کند. تست DPAPI نیز تاریخچهٔ دارای همان markerها را ذخیره و reload می‌کند و نبود marker روشن در فایل‌های کنترل‌شده و نبود `*.tmp` پس از atomic move را می‌سنجد.
- **Windows logs:** آزمون EventSource payload عددی را همچنان بررسی می‌کند و خروجی message/payload را برای سه canary حساس جست‌وجو می‌کند.
- **Android database temp:** آزمون instrumentation مهاجرت Room/SQLCipher، markerهای ساختگی prompt/capsule/secret را در دادهٔ legacy قرار می‌دهد؛ پس از مهاجرت، فایل DB و sidecarهای WAL/SHM و فایل `.sqlcipher-migrating` در صورت وجود اسکن می‌شوند؛ فایل موقت مهاجرت نیز باید حذف شده باشد.
- **Android logs:** آزمون JVM خروجی formatter را با canaryهای حساس مقایسه می‌کند و حفظ schema عددی را می‌سنجد.
- **Static regression:** فایل Python جدید ترتیب Protect-before-temp و SQLCipher-export-before-rename را به کد اجرایی واقعی متصل می‌کند؛ آزمون موجود `test_network_payload_logging.py` همچنان log sinkهای شناخته‌شده را اسکن می‌کند.
- **محدودیت:** اسکن فقط روی فایل‌های برنامه و پوشه‌های آزمون کنترل‌شده است. pagefile/swap، crash dump، backup، screenshot، Event Viewer خارج از EventSource تعریف‌شده و logهای نرم‌افزار ثالث خارج از این تست‌اند.
- **CI:** هیچ نتیجهٔ pass برای commit جدید ادعا نمی‌شود؛ Windows HostLifecycle، Android KtLint/JVM/instrumentation و Python quality/graph guard باید روی exact-head اجرا شوند.
- **گام بعدی:** بر اساس نتیجهٔ واقعی CI اصلاح کن؛ تنها پس از پاس‌شدن همهٔ گیت‌های مرتبط، T0187 را done کن.

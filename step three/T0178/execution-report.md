# T0178 — پیش‌فرض عدم ذخیرهٔ پایدار متن درخواست و پاسخ در میزبان

- **شناسهٔ تلاش:** `T0178-A001`
- **commit پایه:** `08431f8eabb2a6010be6cd3f8cd149ec8928acbe` (`V 3.0.812`)، شاخهٔ `main`
- **وضعیت این تلاش:** `partial`؛ پیاده‌سازی آمادهٔ آزمون دقیق commit است، اما نتیجهٔ CI هنوز ثبت نشده است.
- **آغاز تلاش:** `2026-11-02T11:47:11.868Z`

## بررسی پیش‌نیاز و وابستگی

`T0177` در commit `8345a037602b9cdf6f562f98cde18f6be8954da6` تکمیل شده و گزارش آن، آزمون‌های Android و CI موفق را ثبت می‌کند. یال `T0177 → T0178` بررسی شد: آزمون‌های بودجهٔ تلاش و انتخاب میزبان، قرارداد یا خروجی‌ای تولید نمی‌کنند که سیاست نگهداری محتوای میزبان مصرف کند. طبقه‌بندی این یال به `sequencing_only` تغییر داده شد؛ نوع اصلی `blocks` و ترتیب محافظه‌کارانهٔ گراف حفظ شده‌اند. دلیل و شواهد در `edge_change_log` آمده‌اند.

## کار واقعی انجام‌شده

- سیاست immutable با دامنهٔ `ProcessSessionOnly` اضافه شد؛ مقدار پیش‌فرض صریحاً ذخیرهٔ پایدار متن درخواست و پاسخ را غیرفعال می‌کند.
- `ChatConversation` اکنون سیاست نگهداری را دریافت و آشکار می‌کند و همچنان پیام‌ها را فقط در مجموعهٔ حافظه‌ای نشست نگه می‌دارد.
- پنجرهٔ میزبان ویندوز سیاست پیش‌فرض را به‌صورت صریح به مکالمه تزریق می‌کند.
- آزمون HostLifecycle برای سیاست پیش‌فرض، استفاده از سیاست در مکالمه و خالی بودن نمونهٔ مستقل اضافه شد.
- آزمون منبع پایتون و `docs/security.md` اضافه شدند تا مرز ادعا روشن بماند و ذخیره‌سازی محلی Android با ذخیره‌سازی میزبان اشتباه نشود.

## محدودیت دامنه

این کار دربارهٔ ذخیره‌سازی پایدار تحت کنترل برنامهٔ میزبان است؛ صفرسازی حافظه، pagefile/swap، crash dump، ابزارهای خارجی و logهای ساختاریافته را تضمین نمی‌کند. logها در `T0179`، پاک‌سازی بافرها در `T0180` و پذیرش عملیاتی حریم خصوصی در `AC-PRIV-01` جداگانه بررسی می‌شوند. ذخیره‌سازی محلی مکالمه در Android طبق ADR-0020 رفتار جداگانه‌ای است.

## فایل‌های تغییر‌یافته

- `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`
- `src/Host.Application/Chat/ChatConversation.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `docs/security.md`
- `step three/T0178/execution-report.md`
- `step three/T0178/evidence/verification.md`
- `step three/project-graph.md`

## آزمون‌ها

در زمان ثبت این گزارش هیچ آزمونی از این commit اجراشده اعلام نمی‌شود. گردش‌های `White-P product build`، `Step Three quality gate` و `Rendezvous container build and smoke test` پس از ثبت روی `main` باید روی همین commit بررسی شوند. نتیجهٔ موفق فقط پس از مشاهدهٔ واقعی آن‌ها در تاریخچهٔ Actions ثبت خواهد شد.

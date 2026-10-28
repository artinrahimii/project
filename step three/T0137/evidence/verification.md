# T0137 — شواهد و اعتبارسنجی

## نسخه و دریافت گراف
- Branch: `main`
- Commit مبنا: `8e3e48dc035fc36a2c3d851d3ac6a6b8ca1d45c2` — `V 3.0.478`
- Tree: `81cb2e17837e46c1ef8bdafeb4330f768ca0081e`
- Graph Blob از همان tree: `0820ccb59d3c716bbc7a9eca7dcb9b4335da3d28`
- اندازهٔ Blob و بایت‌های UTF-8 دریافتی: **2,311,197 بایت**
- طول رشتهٔ حاصل از دریافت: **2,189,147 نویسه**
- Git Blob SHA-1 روی محتوای اصلی (شامل حصارهای Markdown): `0820ccb59d3c716bbc7a9eca7dcb9b4335da3d28` — **تطبیق شد**
- JSON کامل پس از برداشتن حصار Markdown تجزیه شد: **229 وظیفه / 419 یال**، بدون یال ارجاع‌داده‌شده به شناسهٔ ناموجود.

## پیاده‌سازی در تلاش T0137-A001
- `src/Shared.Security/AuthenticatedRequestReplayGuard.cs`: کلید replay از PeerId احرازشده، request ID و attempt ID تشکیل می‌شود؛ داده فقط در حافظه نگه داشته می‌شود؛ deadlineهای گذشته پاک می‌شوند؛ کلید زنده در ظرفیت پر حذف نمی‌شود و درخواست جدید fail-closed می‌شود.
- `src/Host.Windows/Networking/PeerResponseCorrelationValidator.cs`: پاسخ فقط وقتی قابل ارسال است که قرارداد پیام و request/attempt/model/host identity با درخواست فعال و هویت میزبان هم‌خوان باشد.
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`: درخواست ورودی قبل از handler تجزیه و اعتبارسنجی می‌شود؛ درخواست تکراری پاسخ رد ثابت می‌گیرد و handler دوباره اجرا نمی‌شود؛ پاسخ نامعتبر یا دارای هم‌بستگی نادرست ارسال نمی‌شود.
- `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs`: ورودی دستکاری‌شده/نامعتبر، کلید تکراری، تفکیک همتا/تلاش، ظرفیت/انقضا و پاسخ جعلی/نامنطبق را بررسی می‌کند.
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`: آزمون یکپارچهٔ loopback با InferenceRequest/InferenceResponse و ارسال دوبارهٔ همان request/attempt برای اثبات رد replay.
- `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`: آزمون‌های موجود هویت همتای ناشناخته/نامنطبق، plaintext-only peer و نسخهٔ secure-channel ناسازگار را اجرا می‌کند.

## وضعیت آزمون
در زمان ثبت این attempt هنوز نتیجهٔ workflowهای commit جدید دریافت نشده است. CI مبنای قبلی `V 3.0.478` موفق بوده، اما آن نتیجه شواهد موفقیت این تغییرات جدید نیست. لینک‌ها/شناسه‌های workflow جدید پس از بررسی exact-head به همین فایل اضافه می‌شوند.

## مرز ادعا و ریسک
- این مجموعه آزمون‌های محلی/loopback است؛ `T0128` و دسترسی از شبکهٔ مستقل همچنان `not_started` و NOT RUN هستند.
- replay cache متکی به حافظهٔ زندهٔ فرایند است؛ پس از restart پاک می‌شود. این تصمیم از ذخیرهٔ پایدار شناسه‌های درخواست جلوگیری می‌کند، اما replay بین restartها را به تنهایی دفع نمی‌کند.

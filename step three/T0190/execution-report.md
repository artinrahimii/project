# T0190 — گزارش وضعیت شبکه و inference بدون محتوای پیام

- **وضعیت:** در حال اجرا؛ نمایش وضعیت ساختاریافته در Windows و Android پیاده‌سازی شده و آزمون‌های رگرسیون افزوده شده‌اند؛ CI دقیق commit این تلاش هنوز اجرا نشده است.
- **مبنای بررسی:** مخزن `artinrahimii/project`، شاخهٔ `main`، commit `bcec43249466d88015c2807701f9ec873b7caa27`، نسخهٔ `V 3.0.890`.
- **گراف مبنا:** `step three/project-graph.md`، Blob `5e228afd442578fdbe4fea79e5c56cda62db219f`؛ ۲۳۰ گره و ۴۲۲ یال.
- **پیش‌نیازها:** T0189 و T0188 هر دو `done` هستند. یال‌های ورودی `T0189 → T0190` و `T0188 → T0190` از نوع اصلی `blocks` حفظ شده‌اند، اما پس از بررسی واقعی به `sequencing_only / validated` طبقه‌بندی شده‌اند. ترتیب محافظه‌کارانهٔ گراف تغییر نکرده است.
- **یافتهٔ Windows:** `HostStatusPresentation.FormatBackendStatus` متن آزاد `HostStatus.Message` را در پنل backend نمایش می‌داد؛ مسیر `BackendCapabilityStatus` نیز در وضعیت Error همان متن را الحاق می‌کرد. هر دو مسیر اکنون از state ساختاریافته و عبارت‌های ثابت استفاده می‌کنند.
- **یافتهٔ Android:** `MainActivity` متن‌های آزاد `networkStatusMessage` و `hostAvailabilityMessage` را مستقیماً در ناحیهٔ وضعیت رندر می‌کرد. اکنون وضعیت شبکه از تعداد peerهای کشف‌شده/مورداعتماد و پرچم‌های چرخهٔ کار، و وضعیت inference از `RequestRunStatus` و حالت لغو/تلاش مجدد تولید می‌شود. متن آزاد وضعیت دیگر در UI نمایش داده نمی‌شود.
- **محدودهٔ محتوا:** prompt و پاسخ مدل همچنان در ورودی prompt و کارت پاسخِ مستقل خود نمایش داده می‌شوند؛ اما هیچ‌کدام به متن وضعیت شبکه یا inference وارد نمی‌شوند. این تغییر، قابلیت نمایش پاسخ واقعی را حذف یا مخفی نمی‌کند.
- **آزمون‌های افزوده‌شده:** HostLifecycle assertions با محتوای حساس sentinel برای اطمینان از حذف `HostStatus.Message` از خروجی status؛ Android unit tests برای تبدیل وضعیت ساختاریافته و عدم افشای فیلدهای متنی آزاد؛ Python source-contract assertions برای جلوگیری از رندر مجدد فیلدهای آزاد در هر دو UI.
- **نتیجهٔ اجرای محلی:** checkout و SDKهای اجرایی در این محیط در دسترس نیستند؛ آزمون محلی ادعا نمی‌شود. اجرای exact-head در GitHub Actions لازم است.
- **گام بعدی:** CI دقیق commit را برای Python/graph gate، Windows HostLifecycle/WPF و Android KtLint/build/JVM/instrumentation بررسی کن. تا عبور همهٔ گیت‌های مرتبط، T0190 در وضعیت `in_progress` باقی می‌ماند.

## T0190-A001 — پیاده‌سازی اولیه

- تاریخ شروع/پایان این تلاش کدنویسی: `2026-11-04T13:19:04.026Z`.
- فایل‌های کد: `src/Host.Application/HostStatusPresentation.cs`، `src/Host.Windows/MainWindow.xaml.cs`، `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt` و `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestStatusPresentation.kt`.
- فایل‌های آزمون: `tests/HostLifecycle.Tests/Program.cs`، `tests/test_wpf_entrypoint.py`، `tests/test_android_request_status_contract.py` و `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestStatusPresentationTest.kt`.
- وضعیت: `partial` تا اجرای CI دقیق همین تغییرات.

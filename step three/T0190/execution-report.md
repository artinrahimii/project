# T0190 — گزارش وضعیت شبکه و inference بدون محتوای پیام

- **وضعیت:** تکمیل‌شده؛ وضعیت شبکه و inference در Windows و Android از state ساختاریافته نمایش داده می‌شود و متن آزاد پیام/استثنا وارد برچسب‌های وضعیت نمی‌شود. معیارهای پذیرش مرتبط در CI دقیق `V 3.0.893` موفق‌اند.
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


## بازبینی CI دقیق V 3.0.891 و اصلاح A002

- **Commit:** `7ec9f9009263f86c78c05e21b3ccf81569562b74`.
- **Step Three quality gate:** run `41837229188` موفق؛ automated tests، syntax، graph structure و recorded outputs/evidence validation همگی pass.
- **Rendezvous container:** run `41837229166` موفق؛ production image و non-root HTTP smoke test pass.
- **Windows WPF:** job `126599163618` موفق؛ C# format، build، Protobuf، WPF startup smoke، agent-tool restrictions/policy/capability و DPAPI/peer-trust checks pass.
- **Windows HostLifecycle:** job `126599162902` شکست خورد. assertion قدیمی `Contains("Ready", StringComparison.Ordinal)` با متن ثابت جدید `Backend — ready.` تطبیق نداشت؛ شکست در مقایسهٔ حساس به حروف بود، نه در نمایش پیام خصوصی یا نگاشت state. خط دقیق: `tests/HostLifecycle.Tests/Program.cs:1072`.
- **Android:** job `126599163603` در `ktlintTestSourceSetCheck` شکست خورد؛ تنها یافتهٔ log: یک خط خالی اضافی پیش از `}` در `RequestStatusPresentationTest.kt:71`. APK، JVM unit tests و instrumentation به علت lint اجرا نشدند.
- **Real local inference:** run `41837229151` باینری pin‌شدهٔ llama.cpp و فایل GGUF را با موفقیت download/verify کرد؛ اما اجرای acceptance واقعی در همان harness آزمون HostLifecycle به assertion قدیمی بالا برخورد کرد و پیش از تولید پاسخ مدل متوقف شد. بنابراین این run نه موفقیت inference است و نه شکست موتور inference.
- **A002:** assertion C# اکنون با `StringComparison.OrdinalIgnoreCase` عبارت `ready` را بررسی می‌کند؛ آزمون Kotlin نیز خط خالی اضافی حذف شده است. منطق وضعیت، متن ثابت، redaction، پروتکل یا کد runtime تغییر نکرده‌اند.
- **وضعیت:** `in_progress`؛ نتایج exact-head پس از A002 هنوز pending.


## بررسی CI دقیق V 3.0.892 و اصلاح A003

- **Commit:** `032213166c6212b6e3646a81d8785e67e07b1d8c` (`V 3.0.892`).
- **Rendezvous container:** run `41837994420` موفق.
- **Windows HostLifecycle:** job `126601806755` موفق؛ assertionهای جدید T0190 برای عدم نمایش sentinel در status و assertion اصلاح‌شدهٔ Ready پاس شدند.
- **Windows WPF:** job `126601807304` موفق؛ build، Protobuf، startup smoke، agent-tool policy/capability و DPAPI/peer-trust موفق.
- **Real local inference:** run `41837994496` / job `126601806330` موفق؛ باینری pin‌شدهٔ llama.cpp و GGUF اعتبارسنجی شدند، inference واقعی پاسخ برگرداند، لغو حین generation مشاهده شد و process تحت مالکیت برنامه تمیز متوقف شد.
- **Step Three quality gate:** run `41837994512` شکست خورد. ۳۹۱ آزمون Python و syntax check موفق بودند؛ graph guard فقط به‌دلیل اختلاف `failed_attempt_count=1` با صفر نتیجهٔ `failed` در execution_history خطا داد.
- **Android:** job `126601807116` در KtLint خط اصلی شکست خورد؛ یک خط خالی غیرضروری در `RequestStatusPresentation.kt:51`. APK/JVM/instrumentation در این job اجرا نشدند.
- **A003:** خط خالی presenter حذف شد. نتیجهٔ تلاش A001 به `failed` اصلاح شد تا با شکست واقعی exact-head آن سازگار باشد؛ شمارندهٔ شکست همچنان ۱ است. A002 به‌عنوان `partial` باقی می‌ماند.
- **وضعیت:** T0190 همچنان `in_progress` تا عبور گیت‌های exact-head بعد از A003.


## پذیرش نهایی T0190 — CI دقیق V 3.0.893

- **Commit:** `ba0f5dd57eb5b11dda9aabe3cd56430e939b561e` در شاخهٔ `main`.
- **Step Three quality gate:** run `41838724150` موفق؛ ۳۹۱ آزمون Python، syntax، graph guard، تطبیق execution_history با failed_attempt_count و بررسی مسیرهای شواهد/خروجی موفق.
- **Rendezvous container:** run `41838723576` موفق.
- **Windows HostLifecycle:** job `126604339681` موفق؛ assertion sentinel تضمین کرد متن آزاد `HostStatus.Message` در status شبکه/backend ظاهر نمی‌شود.
- **Windows WPF:** job `126604339078` موفق؛ C# format، build، Protobuf، startup smoke، agent-tool policy/capability و DPAPI/peer-trust checks پاس شدند.
- **Android:** job `126604339686` در زمان بررسی: KtLint، APK build، بررسی permission policy و Android JVM unit tests موفق بودند؛ JVM suite شامل `RequestStatusPresentationTest` است.
- **اجرای واقعی مدل:** run `41837994496` روی commit `V 3.0.892` موفق؛ llama.cpp محلی پاسخ واقعی تولید کرد، لغو حین generation مشاهده شد و process تحت مالکیت برنامه تمیز متوقف شد. commit `V 3.0.893` فقط whitespace در presenter Android و تاریخچه/شواهد را اصلاح کرد و کد runtime inference/status را تغییر نداد.
- **محدودیت کل workflow:** Android Keystore instrumentation در product build run `41838723715` در آخرین مشاهده هنوز `in_progress` بود و مراحل upload گزارش instrumentation، Protobuf generated verification و APK artifact upload هنوز pending بودند. این‌ها دروازه‌های عمومی محصول/ذخیره‌سازی‌اند، نه آزمون مستقیم T0190؛ بنابراین موفقیت کل product-build workflow اعلام نمی‌شود.
- **نتیجه:** کد وضعیت ساختاریافته و آزمون‌های پذیرش مرتبط Windows/Android پاس شده‌اند؛ T0190 بر اساس معیارهای خودش `done` است.

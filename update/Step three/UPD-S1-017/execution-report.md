# گزارش اجرای UPD-S1-017 — قرارداد مشترک پیام و چرخهٔ درخواست

## وضعیت این تلاش
- وضعیت: `in_progress`؛ کد و آزمون‌ها تغییر کرده‌اند، اما exact-commit CI هنوز اجرا/تأیید نشده است.
- Attempt: `UPD-S1-017-20261108114610`
- Repository/branch: `artinrahimii/project` / `main`
- Baseline: `66f21b572b2766433d1d9eed40e98bf7c8ebba73` — `V 3.1.48`
- زمان آغاز: `2026-11-08T11:46:10.532Z`
- Operation: `modify`؛ schema و runtimeهای موجود حفظ و فقط شکاف قرارداد/نگاشت خطا اصلاح شد.

## شکاف‌های احرازشده
1. `InferenceResponse` شناسه‌های request/attempt/round داشت اما error code typed نداشت؛ Android `FAILED` را به `INTERNAL` عمومی تبدیل می‌کرد و `HOST_BUSY` و `RESOURCE_LIMIT` از بین می‌رفتند.
2. پیام‌های جانبی `ProtocolError`, `HostBusy`, `UnsupportedModel` فقط به request ID متصل بودند؛ Android transport آن‌ها را با requestId به چند attempt pending اعمال می‌کرد.
3. parser سمت Android برای خطاهای جانبی attempt و round را الزام نمی‌کرد؛ این با retry ترتیبی و round جدید سازگار نبود.
4. timeout داخلی backend با deadline مطلق درخواست باید تفکیک می‌شد؛ هیچ پاسخ دیررس یا دارای completion timestamp ساختگی نباید ارسال شود.

## اصلاحات اجرایی
- Protobuf به‌صورت additive توسعه یافت: `InferenceResponse.error_code = 10`; فیلدهای `attempt_id` و `round_id` در شماره‌های جدید برای `ProtocolError`, `HostBusy`, `UnsupportedModel`.
- validator مشترک C# بازهٔ enum و سازگاری error_code با outcome terminal را بررسی می‌کند؛ پاسخ success/cancelled کد خطا نمی‌گیرد.
- Windows مسیر واقعی peer response را به کدهای typed برای Busy، ModelMismatch، ResourceLimit، InvalidMessage، backend timeout و Internal متصل کرد.
- اگر deadline منطقی تمام شده باشد، میزبان هیچ پاسخ دیررس نمی‌فرستد؛ lifecycle timeout بعد از deadline را suppress می‌کند تا timer اصلی Android وضعیت نهایی را ثبت کند.
- پیام‌های خطای جانبی ساخته‌شده توسط mapper، request/attempt/round را حفظ می‌کنند.
- Android parser فقط خطایی را می‌پذیرد که هر سه شناسهٔ nonblank داشته باشد؛ transport آن را فقط به همان AttemptKey، round، peer احرازشده، stream controller و model مرتبط می‌کند. خطای stale یا نامنطبق فقط unmatched می‌شود.
- RequestViewModel کد typed را در کارت همان attempt نمایش می‌دهد و کد ناسازگار با outcome را رد می‌کند.
- Streaming قطعه‌ای اضافه نشده است؛ پاسخ کامل terminal نسخهٔ نخست حفظ می‌شود.

## فایل‌های تغییر‌یافته
- `protocol/whitep/v1/messages.proto`
- `src/Shared.Contracts/ProtocolMessageValidator.cs`
- `src/Host.Windows/Networking/PeerResponseCorrelationValidator.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RemoteRequestErrorParser.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RemoteRequestErrorParserTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/RequestIdentityIndependenceAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_message_validation.py`
- `tests/test_request_error_codes.py`
- `tests/test_android_libp2p_transport_contract.py`
- `update/Step three/UPD-S1-017/execution-report.md`
- `update/Step three/UPD-S1-017/evidence/protocol-error-correlation.md`
- `update/Step two/project-graph.md`

## آزمون‌های لازم و وضعیت
- Windows HostLifecycle و validator مشترک: pending exact-commit CI.
- Android parser و RequestViewModel unit tests: pending exact-commit CI.
- Python protocol/error/transport contracts: pending exact-commit CI.
- quality.yml و product-build.yml: pending.
- آزمون فیزیکی Android-to-Windows، clean install و in-place upgrade در این وظیفه اجرا نشده‌اند.

## محدودیت
کد و آزمون‌ها به فایل‌های واقعی محصول و test suiteهای موجود افزوده شده‌اند؛ تا اجرای موفق exact-commit CI، هیچ معیار پذیرشی تکمیل‌شده اعلام نمی‌شود.


## نتایج exact V 3.1.49 — ثبت خطاها و اصلاح محدود

- Commit: `8a4d1b7d1d231ee9f86d970d9db4fa5092bffec2`.
- Quality [42477241092](https://github.com/artinrahimii/project/actions/runs/42477241092): **success**؛ ۴۷۹ آزمون Python پاس؛ graph guard گراف update با ۲۴/۸۰ صفر خطا و صفر هشدار؛ legacy graph با ۲۳۰/۴۲۲ صفر خطا و یک هشدار تاریخی T0001.
- HostLifecycle: **success**؛ build از clean outputs، documented acceptance suite، coverage و critical results.
- Windows WPF: **success**؛ clean restore، format، build/publish، Protobuf generated source، WPF startup smoke، agent-tool restrictions/policy، backend/model gate، DPAPI host identity/trust، installer و upload artifacts.
- Rendezvous [42477240994](https://github.com/artinrahimii/project/actions/runs/42477240994): **success**.
- Android [42477241098](https://github.com/artinrahimii/project/actions/runs/42477241098): **failed** در `ktlintTestSourceSetCheck`; APK، release، JVM test، Keystore instrumentation و generated Java/Kotlin verify همگی به‌علت ترتیب job **skipped** شدند. علت‌های دقیق: سه builder در RemoteRequestErrorParserTest، دو builder در RequestViewModelTest و ترتیب import در RequestViewModelTest.
- اصلاح سبک فقط در `RemoteRequestErrorParserTest.kt` و `RequestViewModelTest.kt` انجام شد؛ منطق محصول/پروتکل تغییر نکرد.
- Real local inference [42477241035](https://github.com/artinrahimii/project/actions/runs/42477241035): **failed before inference**؛ endpoint دانلود آرشیو pinned `llama.cpp b11443` صفحهٔ خطای GitHub با متن «We couldn't respond to your request in time» برگرداند. باینری دریافت/اعتبارسنجی نشد؛ checksum و GGUF/model inference اجرا نشدند.
- این دو شکست در دو دامنهٔ مستقل‌اند؛ lint اصلاح شده و download upstream باید در V3.1.50 دوباره بررسی شود.
- `UPD-S1-017` همچنان `in_progress` است.

## تلاش اصلاح Ktlint

- Attempt: `UPD-S1-017-20261108115240`.
- تغییرات: فقط دو test file در Android و گزارش/گراف؛ بدون تغییر runtime.
- آزمون‌های exact V3.1.50: pending.

## نتایج exact V 3.1.50 و اصلاح دوم lint

- Commit: `6b8417d8b607a5da122758d97bf9ccca110db0b5`.
- Quality [42477696370](https://github.com/artinrahimii/project/actions/runs/42477696370): **success**؛ ۴۷۹ آزمون Python؛ graph guard update ۲۴/۸۰، صفر خطا/هشدار؛ legacy ۲۳۰/۴۲۲، صفر خطا و یک هشدار تاریخی T0001.
- Rendezvous [42477696426](https://github.com/artinrahimii/project/actions/runs/42477696426): **success**.
- HostLifecycle: **success** با coverage.
- Android [42477696429](https://github.com/artinrahimii/project/actions/runs/42477696429): lint دوباره شکست خورد. خطاهای parser test قبلاً رفع شده بودند؛ باقی‌مانده‌ها فقط ترتیب import و دو زنجیرهٔ دسترسی به کد خطا در RequestViewModelTest بودند. APK/JVM/Keystore به علت fail شدن lint اجرا نشدند.
- اصلاح دوم: ترتیب importهای protobuf مرتب شد و دو assertion مربوط به کد خطای کارت‌ها به زنجیرهٔ چندخطی تبدیل شدند.
- Windows WPF در زمان آخرین بررسی هنوز در حال اجرا بود؛ نتیجهٔ موفقیت در این گزارش ادعا نمی‌شود.
- workflow مدل محلی در V3.1.49 پیش از دریافت باینری و checksum به‌دلیل خطای موقت دانلود GitHub متوقف شد؛ در V3.1.50 دوباره اجرا نشد.
- وضعیت UPD-S1-017 همچنان `in_progress` است تا exact V3.1.51 CI تکمیل شود.


## نتایج exact V 3.1.51 و اصلاح main-source ktlint

- Commit: `cd27ca0adbaf12068f14e4c942a0ac7f833a24f0`.
- Quality [42478015523](https://github.com/artinrahimii/project/actions/runs/42478015523): **success**؛ ۴۷۹ آزمون Python و graph guards موفق؛ گراف update ۲۴/۸۰ بدون خطا/هشدار؛ گراف legacy ۲۳۰/۴۲۲ بدون خطا و یک هشدار قدیمی T0001.
- Rendezvous [42478015406](https://github.com/artinrahimii/project/actions/runs/42478015406): **success**.
- HostLifecycle [42478015591]: **success** همراه coverage.
- Windows WPF [42478015591]: **success**؛ build/publish، generated Protobuf، WPF startup، policy/backend gate، DPAPI/peer trust و installer.
- Android [42478015591]: **failed** در ktlint main source. Test source lint در این نسخه پاس شد؛ خطاها به validator outcome در RequestViewModel منتقل شدند: assignment/when wrapping، چند expression چندخطی و سه شاخهٔ if/else بدون brace.
- اصلاح main-source در تلاش بعدی انجام شد و منطق allowed error codes همان مقدارهای قبلی را حفظ می‌کند.
- APK، release، JVM unit tests و Keystore instrumentation در این نسخه به دلیل lint اجرا نشدند.
- آزمون فیزیکی Android-to-Windows، clean install کاربر و in-place upgrade هنوز اجرا نشده‌اند.

## نتایج exact V 3.1.52 و اصلاح پایدار main-source

- Commit: `04ad9eb9eace198ee14ee3d739321938c0a69f41`.
- Quality [42478443061](https://github.com/artinrahimii/project/actions/runs/42478443061): **success**؛ ۴۷۹ Python tests؛ graph guard update ۲۴/۸۰ صفر خطا/هشدار؛ legacy ۲۳۰/۴۲۲ صفر خطا و یک هشدار تاریخی T0001.
- Rendezvous [42478442987](https://github.com/artinrahimii/project/actions/runs/42478442987): **success**.
- HostLifecycle [42478443044]: **success**.
- Windows WPF [42478443044]: clean build/publish، generated Protobuf، WPF startup و policy/DPAPI gates موفق بودند؛ installer step در آخرین بررسی هنوز در حال اجرا بود.
- Android [42478443044]: test source lint را پاس کرد اما main-source lint سه خطا در multiline setOf membership expressions در سه outcome branch داشت. APK/JVM/Keystore به‌علت fail شدن lint اجرا نشدند.
- اصلاح پنجم: مجموعه‌های مجاز FAILED/TIMED_OUT/REJECTED اکنون به private immutable companion properties منتقل شده‌اند؛ when branches فقط یک membership check تک‌خطی دارند و enum values تغییر نکرده‌اند.
- retry دوم workflow مدل محلی روی V3.1.49 نیز در همان مرحلهٔ دانلود آرشیو pinned llama.cpp شکست خورد؛ SHA-256 و inference همچنان اجرا نشده‌اند.
- UPD-S1-017 همچنان `in_progress` است تا exact V3.1.53 CI.


## نتیجهٔ نهایی UPD-S1-017 — تأیید exact V 3.1.53

- وضعیت: **done**؛ معیارهای قرارداد پیام، اعتبارسنجی typed outcome و correlation پاسخ/خطا در Android و Windows تأیید شدند.
- Commit کد و آزمون‌ها: `fe74186f1fbf71ed41e0806d8c3e14c43a3a64e2` — `V 3.1.53`.
- Quality [42478793396](https://github.com/artinrahimii/project/actions/runs/42478793396): **success**؛ ۴۷۹ Python tests؛ گراف update با ۲۴/۸۰ صفر خطا و صفر هشدار؛ گراف legacy با ۲۳۰/۴۲۲ صفر خطا و یک هشدار قدیمی T0001.
- Product [42478793456](https://github.com/artinrahimii/project/actions/runs/42478793456): هر سه job **success**.
  - Android lint، debug APK، release variant، minimum permissions، JVM unit tests، Keystore instrumentation و generated Protobuf verification همگی passed.
  - Android JUnit: **۲۲۷ تست، ۲۲۶ موفق، صفر شکست، یک skip**؛ ۳۴ فایل گزارش و دو HTML coverage report بارگذاری شدند.
  - Windows WPF: clean build، publish، Protobuf source verification، startup smoke، process/policy/backend/DPAPI checks و installer موفق؛ artifactها upload شدند.
  - HostLifecycle: acceptance و coverage موفق؛ line coverage برابر **۸۵٫۸۸٪ (۸٬۹۳۵ از ۱۰٬۴۰۴ خط)**، ۶۷ pass marker و یک skip marker.
- Rendezvous [42478793453](https://github.com/artinrahimii/project/actions/runs/42478793453): **success**.
- Artifactهای ثبت‌شده:
  - `whitep-android-debug-apk`: ۲۸٬۹۷۹٬۴۰۲ بایت.
  - `whitep-android-unit-test-reports`: ۹۱٬۳۶۰ بایت.
  - `whitep-android-keystore-instrumentation-reports`: ۹۹٬۶۶۰ بایت.
  - `whitep-android-coverage-and-critical-results`: ۵٬۳۷۱ بایت.
  - `whitep-windows-installer`: ۶۰٬۹۸۸٬۶۰۷ بایت.
  - `whitep-windows-host`: ۱۰۱٬۵۷۳٬۰۳۱ بایت.
  - `whitep-windows-publish`: ۱۰٬۰۲۳٬۲۱۴ بایت.
  - `whitep-host-lifecycle-coverage-and-critical-results`: ۱۱۱٬۸۶۱ بایت.
- محدودیت مستقل: retry آزمون `real-local-inference` به‌علت دانلود نشدن artifact پین‌شدهٔ llama.cpp پیش از checksum/inference شکست خورد؛ این آزمون، مدل محلی را اجرا نکرد.
- هیچ آزمون فیزیکی Android-to-Windows، پاسخ زندهٔ مدل در Room، clean install روی دستگاه هدف یا in-place upgrade در این وظیفه انجام نشده است؛ این gates مستقل باقی می‌مانند.
- نتیجهٔ نهایی این وظیفه بر مبنای تست‌های قرارداد، C#/Kotlin، buildهای clean و CI دقیق commit بالا ثبت شد.

# گزارش اجرای UPD-S1-003 — تعداد میزبان و سیاست retry

## وضعیت فعلی

- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit مبنای این تلاش: `38c63f441cd3991da105ba1edafc319c30dc7241` — `V 3.1.26`
- گرهٔ پیش‌نیاز UPD-S1-001: `done`
- وضعیت UPD-S1-003: `in_progress`؛ بررسی اولیه انجام شده، تصمیم نهایی و تغییر کد هنوز در این گزارش ثبت نشده است.
- شناسهٔ تلاش: `UPD-S1-003-20261107203548`
- زمان شروع: `2026-11-07T20:35:48.198Z`

## سؤال محصول

گزارش به‌روزرسانی در بخش 8.2، یک میزبان در هر درخواست و انتقال ترتیبی پس از پایان یا لغو ایمن تلاش قبلی را پیشنهاد می‌کند. در مقابل، نسخهٔ فعلی برنامه سه میزبان مستقل را برای هر دور انتخاب و هم‌زمان dispatch می‌کند. این اختلاف فقط تغییر UI نیست؛ الگوریتم، سقف تلاش، رفتار لغو و تست‌های محصول را تحت تأثیر قرار می‌دهد.

## ممیزی کد فعلی

- `FirstReleaseInferenceLimits` در `DistributedRequest.kt`: سه دور، حداکثر سه میزبان در هر دور و حداکثر نه میزبان برای یک درخواست.
- `DistributedRequest` به‌صورت پیش‌فرض برای سه دور، بودجهٔ نه میزبان ایجاد می‌کند.
- `HostCandidateSelector`: هویت‌های تکراری را رد می‌کند، PeerIdها را مرتب می‌کند، سازگاری مدل/capacity و آمادگی را بررسی می‌کند؛ `selectFirstRound` تا سقف سه میزبان برمی‌گرداند.
- `AuthenticatedHostDiscovery`: اعلان‌های Rendezvous را بر اساس PeerId مرتب می‌کند و تا رسیدن به تعداد درخواست‌شده، برای میزبان‌ها status probe احرازشده می‌فرستد. با `maxHosts=1` قادر است پس از اولین میزبان معتبر متوقف شود؛ نیازی به broadcast کردن prompt به همهٔ peers ندارد.
- `InferenceAttemptDispatcher`: برای فهرست میزبان‌های انتخاب‌شده، attempt مستقل می‌سازد و همه را dispatch می‌کند؛ سطح فعلی امکان ارسال هم‌زمان تا سه میزبان را دارد.
- `InferenceRoundResponseCollector`: attemptهای round را جمع‌آوری می‌کند و تا deadline مشترک پنجرهٔ پاسخ، نتیجه‌های مستقل را نگه می‌دارد.
- `InferenceRoundTimer`: timeout پیش‌فرض هر round برابر ۶۰ ثانیه است و deadline نهایی درخواست را نیز رعایت می‌کند.
- `RequestViewModel`: request deadline فعلی ۹۰ ثانیه است؛ ارسال اولیه تا سه میزبان را می‌خواهد؛ retry دستی و user-triggered است، attemptId و roundId تازه می‌سازد و requestId را حفظ می‌کند. برای retry، deadline را از نو تنظیم می‌کند و میزبان‌های موفق/لغوشده را مستثنا می‌کند.
- UI فعلی برای لغو، پیام correlated `CancelRequest` را برای تمام attemptهای queued ارسال می‌کند؛ وضعیت `CANCELLED` terminal است و retry نمی‌شود.
- transport Android لغو را با requestId و attemptId دقیق به stream احرازشده می‌فرستد. در میزبان Windows، CancellationToken به همان request/attempt پیوند دارد؛ در صورت disconnect stream، handler لغو و taskهای فعال drain می‌شوند. اگر لغو قبل از deadline پردازش شود، میزبان پاسخ `CANCELLED` correlated می‌فرستد؛ پس از deadline پاسخ لغو ممکن است ارسال نشود.
- پاسخ‌ها با requestId، attemptId، roundId، PeerId و deadline اعتبارسنجی می‌شوند. response دیررس یا تکراری نباید به تلاش یا درخواست دیگر نسبت داده شود.
- زمان‌بندی فعلی retry خودکار نیست؛ کاربر دکمهٔ retry را می‌زند. این رفتار باید در تصمیم جدید صریح بماند یا با دلیل محصولی تغییر کند.

## وظایف قدیمی متأثر (۲۳ شناسه)

### پیاده‌سازی سه‌میزبانهٔ موجود که باید حفظ شود یا به قابلیت آینده منتقل شود

- `T0125` و `T0126`: انتخاب سه PeerId متمایز و رفتار کمتر از سه میزبان.
- `T0162`: selection تا سه میزبان ready و سازگار.
- `T0166` و `T0167`: کارت‌های پاسخ مستقل و بدون merge خودکار.
- `T0168`: ادامهٔ دریافت پاسخ‌های مستقل تا پایان پنجرهٔ round.
- `T0169` و `T0170`: round بعدی در صورت نرسیدن پاسخ و سقف round/کل میزبان‌ها.
- `T0177`: بودجهٔ سه میزبان برای هر round.
- `T0203` و `T0204`: آزمون‌های سه فرایند میزبان و round دوم.

این قابلیت‌ها در گراف اولیه دارای سوابق `done` هستند. صرفاً به‌سبب تغییر سیاست نسخهٔ نخست نباید تاریخچه یا وضعیتشان پاک شود. تصمیم جدید باید روشن کند کدام بخش‌ها به compatibility/future scope منتقل می‌شوند و کدام معیارهای پذیرش نسخهٔ نخست تغییر می‌کنند.

### سازوکارهای عمومی که باید حفظ و با سیاست جدید سازگار شوند

- `T0161`: request lifecycle، deadline و budget.
- `T0163`: شناسه‌های مستقل attempt و correlation.
- `T0164`: timer تزریق‌پذیر و timeout.
- `T0165`: اعتبارسنجی correlation پاسخ.
- `T0171`: عدم retry روی همان PeerId مگر با سیاست صریح.
- `T0172`: رد پاسخ دیرهنگام.
- `T0173`: idempotency برای پاسخ تکراری.
- `T0174`: لغو correlated درخواست.
- `T0175`: وضعیت failed و retry دستی.
- `T0176`: آزمون صفر/یک/دو/سه پاسخ، timeout، late/duplicate و cancel؛ آزمون‌های چندمیزبانه می‌توانند برای compatibility باقی بمانند.

### وظایف برنامه‌ریزی‌شده ولی شروع‌نشده

- `T0224` و `T0225`: هنوز `not_started` و بدون execution history هستند. تکمیل آن‌ها در سیاست نسخهٔ نخست نباید جعل شود؛ در تصمیم باید صریحاً به‌عنوان کار آینده یا بازطراحی‌شده تعیین تکلیف شوند.

## تصمیم نهایی

**هنوز در این مرحله اتخاذ نشده است.** شواهد کد با پیشنهاد بخش 8.2 سازگارند، اما ابتدا باید سیاست نهایی cardinality، مجموع تلاش مجاز، deadline کلی، رفتار timeout/disconnect/cancel و طبقه‌بندی دقیق همهٔ ۲۳ شناسه مشخص و در ADR مستقل ثبت شود. کد محصول و آزمون‌ها پس از تثبیت این تصمیم اصلاح خواهند شد.

## شواهد این تلاش

- Update requirements: `update/Step one/Output/V1.10.json`, UPD-S1-003.
- Product proposal: `update/Step one/2026-11-07-automatic-chat-and-host-routing.md`, بخش 8.2.
- Android selector: `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`.
- Android discovery: `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`.
- Android dispatch/domain: `DistributedRequest.kt`, `InferenceAttemptDispatcher.kt`.
- Timer/collector: `InferenceRoundTimer.kt`, `InferenceRoundResponseCollector.kt`.
- UI retry/cancel: `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`.
- Android transport: `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`.
- Windows cancellation: `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`.
- Old graph source: `step three/project-graph.md`; 23 affected IDs were found and matched.

## آزمون‌ها

- در این تلاش فقط ممیزی منبع و تاریخچه انجام شد؛ آزمون جدید یا اجرای محصولی انجام نشده است.
- آزمون‌های baseline مربوط به V 3.1.24 روی commit مستقل خود قبلاً موفق بوده‌اند؛ این موفقیت‌ها جای آزمون تغییرات آتی UPD-S1-003 را نمی‌گیرند.


## تلاش دوم — تثبیت سیاست تک‌میزبانه و اصلاح ناسازگاری بودجه

- commit مبنا: `28594b1fd1c706069748e63fecc482f3634dd296` — `V 3.1.28`.
- تصمیم: `ADR-0022-single-host-sequential-retry.md`.
- سیاست: یک میزبان در هر round؛ سه round/سه تلاش متمایز برای کل درخواست؛ retry دستی؛ requestId ثابت؛ attemptId/roundId تازه؛ یک deadline مشترک؛ لغو terminal.
- اصلاح محصول: اعتبارسنجی دامنهٔ `DistributedRequest` و default factory اکنون فقط سقف first-release را می‌پذیرند. سقف generic سه‌میزبانه در `MultiHostCompatibilityLimits` و selector/dispatcher عمومی حفظ می‌شود.
- اصلاح آزمون: آزمون دامنه و UI به مسیر یک میزبان و fallback ترتیبی تغییر کردند؛ source-contract guardها میان policy محصول و compatibility تفکیک شدند.
- اصلاح قالب‌بندی: دو تخلف ktlint در `DistributedRequest.kt` و `AndroidInferenceRequestRuntime.kt` برطرف شدند.
- شواهد: `update/Step three/UPD-S1-003/evidence/decision-matrix.md` و `ci-baseline.md`.
- وضعیت: در انتظار CI commit اصلاحی؛ تا سبزشدن build و آزمون‌ها، گره `in_progress` باقی می‌ماند.
- آزمون دستگاه واقعی و Android-to-Windows با مدل محلی در این محیط اجرا نشده‌اند و به UPD-S1-020/021 تعلق دارند.


## تلاش سوم — اصلاح پنج خطای باقی‌ماندهٔ CI نسخهٔ V 3.1.29

- مبنای آنلاین: `89042d5feba3c3859d63a7527aac0c7b2669cb88` — `V 3.1.29`.
- Quality run `42417360215`: پنج شکست قراردادی، شامل selector compatibility، نام آزمون‌های ViewModel، markerهای retry/cancel و قالب ردیف ADR-0022.
- اصلاح: آزمون selector اکنون ثابت generic compatibility را بررسی می‌کند؛ guardهای UI/ retry/cancel با نام‌های واقعی آزمون تک‌میزبانه هم‌راستا شدند؛ ADR-0022 با پنج ستون و مسیرهای کامل مخزن ثبت شد.
- Rendezvous container build: موفق؛ HostLifecycle tests: موفق.
- Windows WPF build و Android APK هنگام آخرین بررسی هنوز در حال اجرا بودند.
- وضعیت: partial تا اجرای مجدد CI روی commit اصلاحی.


## اعتبارسنجی دقیق V 3.1.30 و علت شکست Android

- مبنای آنلاین: `cb6050e9b6e9f8fd60d2266efbf57af2534a16c0` — `V 3.1.30`.
- Step Three quality gate: **PASS**، run [42417548666](https://github.com/artinrahimii/project/actions/runs/42417548666).
- Rendezvous container build/smoke: **PASS**، run [42417548709](https://github.com/artinrahimii/project/actions/runs/42417548709).
- Windows WPF clean build، publish، startup smoke، identity/trust checks و installer acceptance: **PASS**، run [42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687).
- HostLifecycle unit/integration و coverage validation: **PASS**، run [42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687).
- Android debug APK، release APK، lint و merged-manifest permission checks: **PASS**؛ اما JVM tests: **FAIL**، ۲۱۳ آزمون اجرا و ۳۳ شکست، در همان run محصول.
- علت ریشه‌ای: `DistributedRequest` بودجهٔ کل را به سقف first-release یعنی ۳ محدود می‌کند؛ آزمون‌های سازگاری selector/dispatcher و transport به‌درستی بودجه‌های ۴، ۶ و ۹ را برای مسیر generic می‌سازند. شکست‌ها در constructor رخ داده‌اند، نه در transport واقعی.
- مرز محصول حفظ می‌شود: `RequestViewModel` حداکثر سه تلاش ترتیبی ایجاد می‌کند؛ `AndroidInferenceRequestRuntime` فقط یک peer را در هر round می‌پذیرد؛ `maxRounds` نیز حداکثر سه است. بنابراین اجازهٔ بودجهٔ generic در value object نباید مسیر محصول را چندمیزبانه کند.
- تصمیم اصلاحی: سقف عمومی `DistributedRequest` با `MultiHostCompatibilityLimits` هم‌راستا می‌شود؛ مقدار پیش‌فرض first-release و enforce کردن یک میزبان در هر round در runtime بدون تغییر می‌ماند.
- وضعیت UPD-S1-003: **in_progress**؛ تا قبولی آزمون‌های اصلاح‌شده و اعتبارسنجی مجدد معیارهای پذیرش، done نمی‌شود.


## اعتبارسنجی دقیق V 3.1.31 — شکست مرحلهٔ ktlint اندروید

- Commit آزموده‌شده: `719f2a4daf0b82c013c617a1a0a4778c625c474c` — `V 3.1.31`.
- Step Three quality gate: **PASS**، run [42455296797](https://github.com/artinrahimii/project/actions/runs/42455296797).
- Rendezvous container build/smoke: **PASS**، run [42455296891](https://github.com/artinrahimii/project/actions/runs/42455296891).
- Windows WPF build، publish، startup smoke و installer: **PASS**، run [42455296829](https://github.com/artinrahimii/project/actions/runs/42455296829).
- HostLifecycle unit/integration/coverage: **PASS**، run [42455296829](https://github.com/artinrahimii/project/actions/runs/42455296829).
- Android: **FAIL** در `:app:ktlintTestSourceSetCheck`؛ JVM tests و instrumentation پس از شکست gate اجرا نشدند.
- بررسی source در همان commit یک خط خالی تکراری میان declarationهای آزمون در `DistributedRequestTest.kt` نشان داد؛ این مورد اصلاح شد. همچنین توضیح ADR-0022 دربارهٔ سقف domain object با جداسازی سقف generic از محدودیت مسیر first-release همگام شد.
- این شکست به معنای شکست آزمون‌های JVM نیست؛ آن آزمون‌ها در این commit **اجرا نشدند**.
- وضعیت UPD-S1-003 همچنان `in_progress` است تا lint، JVM tests و سایر آزمون‌های لازم روی commit اصلاحی پاس شوند.


## اعتبارسنجی دقیق V 3.1.32 — پنج شکست در آزمون JVM اندروید

- Commit آزموده‌شده: `10303ad6faddd7d4a50096d702d0e9f4a10e2b7d` — `V 3.1.32`.
- Step Three quality gate: **PASS**، run [42455653912](https://github.com/artinrahimii/project/actions/runs/42455653912).
- Rendezvous build/smoke: **PASS**، run [42455653892](https://github.com/artinrahimii/project/actions/runs/42455653892).
- Windows build/publish/startup/installer و HostLifecycle suite/coverage: **PASS**، run [42455653927](https://github.com/artinrahimii/project/actions/runs/42455653927).
- Android lint، build دیباگ و انتشار و بررسی permissionها: **PASS**؛ اما JVM: **FAIL**، ۲۱۴ آزمون اجرا و ۵ شکست.
- چهار شکست transport به این علت بود که آزمون‌های قدیمی سه میزبان را در یک round به `AndroidInferenceRequestRuntime` می‌دادند؛ runtime به‌درستی `TOO_MANY_HOSTS` برگرداند. آزمون‌ها اکنون انتخاب یک میزبان از چند candidate و انتخاب میزبان بعدی فقط پس از پایان پنجرهٔ timeout را بررسی می‌کنند.
- شکست پنجم مربوط به آزمون generic dispatcher بود: بودجهٔ ۹ تلاش عمومی با سقف سه‌تلاشهٔ `FirstReleaseInferenceLimits` مقایسه شده بود. آزمون اکنون بودجهٔ generic نه‌تایی را صریحاً انتخاب می‌کند و با `MultiHostCompatibilityLimits` assert می‌کند.
- گزارش JUnit از artifact شمارهٔ `13608070526` دریافت و بررسی شد؛ SHA-256 فایل ZIP: `23142a5339df2e98e969b8bf6b6c2738a4bc3095a0803ddc372a2a474d9020cd`.
- Android Keystore instrumentation به‌دلیل شکست JVM اجرا نشد. وظیفهٔ UPD-S1-003 تا اجرای مجدد مجموعهٔ کامل آزمون‌ها `in_progress` باقی می‌ماند.


## اعتبارسنجی دقیق V 3.1.33 — شکست ktlint اندروید

- Commit آزموده‌شده: `b881d980419be1998e32eeb6a55d0d13d5a4cc3e` — `V 3.1.33`.
- Step Three quality gate: **PASS**، run [42456615858](https://github.com/artinrahimii/project/actions/runs/42456615858).
- Rendezvous build/smoke: **PASS**، run [42456615879](https://github.com/artinrahimii/project/actions/runs/42456615879).
- Windows WPF build و installer: **PASS**؛ HostLifecycle suite/coverage: **PASS**، run [42456615891](https://github.com/artinrahimii/project/actions/runs/42456615891).
- Android: **FAIL** در `:app:ktlintTestSourceSetCheck`. بنابراین APKهای debug/release، آزمون JVM و Keystore instrumentation روی این commit اجرا نشدند.
- گزارش ktlint به‌صورت artifact بارگذاری نشده بود. بررسی source دقیق commit دو خط ۱۲۱ نویسه‌ای را در فایل‌های تست Kotlin مشخص کرد؛ هر دو در اصلاح بعدی line-wrap شدند.
- پنج شکست JVM ثبت‌شده برای V 3.1.32 در V 3.1.33 دوباره اجرا نشدند، چون lint پیش از مرحلهٔ JVM متوقف شد.
- UPD-S1-003 همچنان `in_progress` است تا lint و آزمون‌های کامل Android روی commit اصلاحی پاس شوند.


## اعتبارسنجی دقیق V 3.1.34 — شکست قراردادهای Python

- Commit آزموده‌شده: `40d9128be6d3bd0df3531d44b893946f24e82961` — `V 3.1.34`.
- Step Three Python quality gate: **FAIL**، run [42456934121](https://github.com/artinrahimii/project/actions/runs/42456934121)، ۴۶۴ آزمون اجرا و ۵ شکست در assertionهای قراردادی.
- علت‌ها: marker قدیمی برای سقف generic ۹تایی؛ انتظار escape متنی `\\t` در حالی که تغییر قبلی tab واقعی وارد source کرده بود؛ نام آزمون چندمیزبانهٔ حذف‌شده؛ marker قدیمی `attemptStatusSummary`؛ و نام متغیر runtime قدیمی.
- این موارد نشان‌دهندهٔ شکست منطق محصول نیستند؛ آزمون‌های Python با بازطراحی آزمون‌های Kotlin همگام نشده بودند. محدودیت تولیدی یک میزبان در هر round تغییر نمی‌کند.
- در همان commit، Kotlin lint موفق شد و build دیباگ Android آغاز شد؛ نتیجهٔ باقی مراحل Product Build پس از تکمیل workflow جداگانه ثبت می‌شود.
- UPD-S1-003 تا پاس‌شدن مجدد Python quality، Android JVM و Keystore و سایر معیارها `in_progress` باقی می‌ماند.


## اعتبارسنجی تکمیلی V 3.1.34 — خطای کامپایل JVM اندروید

- Product build run: [42456934059](https://github.com/artinrahimii/project/actions/runs/42456934059).
- Windows WPF build/publish/startup/security/installer: **PASS**.
- HostLifecycle unit/integration/coverage: **PASS**.
- Android ktlint، debug/release APK و permission checks: **PASS**.
- Android JVM: **FAIL پیش از اجرای تست‌ها**؛ `:app:compileDebugUnitTestKotlin` در دو خط ۴۲۹ و ۱۰۰۲ از `Libp2pInferenceRequestTransportTest.kt` به `InferenceHostAttempt.roundId` ارجاع می‌داد، اما این value object چنین فیلدی ندارد.
- قرارداد واقعی `InferenceHostAttempt` شامل `ephemeralHostId`، `attemptId` و `submissionStatus` است؛ roundId در پیام Protobuf درخواست/پاسخ و pending transport correlation وجود دارد. تست‌ها به‌جای افزودن فیلد جدید به مدل، roundId پیام دریافتی را nonblank و roundId پاسخ را منطبق با درخواست دریافت‌شده بررسی می‌کنند.
- Keystore instrumentation و artifact نهایی APK به‌دلیل شکست کامپایل JVM اجرا/آپلود نشدند.
- UPD-S1-003 همچنان `in_progress` است.

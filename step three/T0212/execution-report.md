# گزارش اجرای T0212

## هدف

مدیریت امضای APK نسخهٔ Release به‌گونه‌ای که keystore و رمزهای واقعی خارج از مخزن بمانند، ساخت release بدون credentials به‌صورت fail-closed متوقف شود، و fingerprint گواهی واقعی پیش از انتشار artifact بررسی شود.

## بررسی اولیه و وابستگی‌ها

- T0208 و T0211 در گراف `done` هستند؛ Quality Gate فعلی commit V 3.0.969 نیز موفق شده است.
- یال T0211 → T0212: نوع اصلی `blocks` حفظ شد؛ با شواهد کد و workflow به `sequencing_only` با وضعیت `validated` طبقه‌بندی شد. T0212 variant مستقل release را از همان Gradle module می‌سازد و APK دیباگ یا artifact خروجی T0211 را به‌عنوان ورودی re-sign مصرف نمی‌کند.
- یال T0208 → T0212: نوع اصلی `blocks` حفظ شد؛ با شواهد artifactهای coverage و workflow به `sequencing_only` با وضعیت `validated` طبقه‌بندی شد. امضای release هیچ coverage artifact یا API اختصاصی T0208 را مصرف نمی‌کند.
- ترتیب گراف حفظ شد؛ هیچ یال دیگری تغییر نکرد.
- Gradle پیش از این فقط build type دیباگ را به‌طور صریح پیکربندی کرده بود؛ `signingConfig` برای release وجود نداشت. Product Build نیز فقط `assembleDebug` اجرا می‌کرد.
- `.gitignore` فایل‌های `.jks` و `.keystore` را نادیده می‌گرفت؛ الگوهای `.p12` و `.pfx` نیز افزوده شدند.

## پیاده‌سازی این تلاش

- Gradle اکنون چهار ورودی محیطی `WHITEP_ANDROID_RELEASE_*` را برای release می‌خواند و اگر هرکدام ناقص باشد، taskهای release/build/assemble/bundle را fail-closed متوقف می‌کند.
- مسیر keystore باید فایل واقعی و canonical خارج از ریشهٔ مخزن باشد؛ مسیر داخل checkout رد می‌شود.
- Product Build یک JKS تصادفی و موقت در `$RUNNER_TEMP` می‌سازد، `assembleRelease` واقعی را اجرا می‌کند، امضای APK را با `apksigner verify --print-certs` بررسی می‌کند و کلید موقت را در cleanup با `if: always()` حذف می‌کند. این کلید صرفاً smoke است و هویت امضای توزیع نیست.
- workflow مستقل `.github/workflows/android-release.yml` به Environment محافظت‌شدهٔ `android-release` نیاز دارد. چهار secret شامل keystore Base64، رمز store، alias و رمز key را می‌گیرد؛ سپس امضای APK و تطبیق SHA-256 fingerprint با Environment variable را کنترل می‌کند.
- مستندات `docs/android-release-signing.md` مراحل provisioning امن، محدودیت Base64، backup و workflow را توضیح می‌دهد.
- آزمون regression جدید، Gradle fail-closed، ممنوعیت keystore داخل checkout، smoke signing، production Environment، fingerprint pinning و ignore الگوهای فایل کلید را کنترل می‌کند.

## محدودیت واقعی این نوبت

مقادیر یا metadata مربوط به GitHub Environment Secrets از طریق اتصال GitHub در دسترس این اجرا نبود؛ بنابراین نه وجود secrets تولیدی و نه مقدار fingerprint آن‌ها تأیید نشده است. هیچ keystore واقعی تولید یا افشا نشد. این نوبت فقط زیرساخت و آزمون امضای موقت را پیاده می‌کند؛ workflow با کلید واقعی هنوز باید پس از provisioning توسط maintainer اجرا شود.

## وضعیت

`T0212` در وضعیت `in_progress` می‌ماند. `done` فقط پس از اجرای موفق workflow محافظت‌شده با keystore تولیدی و تطبیق fingerprint ثبت می‌شود.
## A002 — نتایج نخستین CI و شکست‌های مشخص

- Quality Gate run 42053031648 (commit V 3.0.970) تعداد ۴۲۹ آزمون را کشف کرد و فقط یک آزمون شکست خورد: `test_documentation_separates_smoke_signing_from_real_release_signing` عبارت دقیق `outside the repository` را در مستندات پیدا نکرد. پس از شکست unittest، Graph Guard در همان اجرا انجام نشد.
- Product Build run 42053032390: jobهای Windows WPF و HostLifecycle موفق شدند؛ job اندروید پس از `gradle clean` در `ktlintKotlinScriptCheck` متوقف شد.
- گزارش دقیق KtLint: ترتیب importها و شکستن عبارت‌های چندخطی در `app/build.gradle.kts`، خطوط ۱، ۲۲، ۳۱، ۴۳، ۴۷ و ۵۳ نیاز به اصلاح داشتند. بنابراین `assembleRelease` با کلید موقت و `apksigner` در این اجرا اصلاً اجرا نشدند.
- workflow تولیدی `android-release.yml` در push عادی main اجرا نشد و run مربوط به آن صفر job داشت؛ این workflow فقط با `workflow_dispatch` یا tagهای `android-v*` فعال می‌شود. این اجرا را نه موفق و نه آزمون شکست‌خوردهٔ امضای تولیدی حساب نمی‌کنیم.

## A003 — اصلاح قالب کد و محدودسازی عمر کلید موقت

- ترتیب import و ساختار multiline در Gradle Kotlin DSL مطابق گزارش واقعی KtLint اصلاح شد.
- جملهٔ مستندات به‌گونه‌ای اصلاح شد که مرز امنیتی خارج‌بودن keystore از checkout را دقیقاً بیان کند و با قرارداد تست یکسان باشد.
- ساخت کلید موقت، `assembleRelease`، بررسی `apksigner` و پاک‌سازی در یک shell step جمع شدند. متغیرهای signing فقط در همان step export می‌شوند و `trap ... EXIT` پوشهٔ کلید موقت را حتی در زمان failure حذف می‌کند؛ دیگر از GITHUB_ENV برای نگهداری رمزهای smoke در باقی job استفاده نمی‌شود.
- Quality Gate و Product Build نسخهٔ اصلاحی هنوز اجرا نشده‌اند. T0212 همچنان `in_progress` است؛ هیچ ادعایی دربارهٔ امضای production یا fingerprint واقعی ثبت نمی‌شود.
## A003 — نتیجهٔ Quality Gate پس از اصلاح KtLint

- Quality Gate run 42062003986 روی V 3.0.971، ۴۲۹ آزمون اجرا کرد. تست مستندات و پنج تست دیگر امضای release موفق شدند؛ یک تست مسیر canonical شکست خورد، چون عبارت `resolve("../..").canonicalFile.toPath()` را دقیقاً روی یک خط می‌خواست.
- پیاده‌سازی برای رعایت KtLint زنجیرهٔ `resolve`، `canonicalFile` و `toPath` را چندخطی کرده بود؛ بنابراین شکست، محدودیت تست منبع بود نه اثبات ضعف کنترل مسیر.
- Product Build run 42062004172: job HostLifecycle موفق شد؛ jobهای Windows و Android هنگام ثبت این گزارش هنوز در حال اجرا بودند. Android از مرحلهٔ KtLint عبور کرده و به buildهای debug/release رسیده بود.

## A004 — اصلاح تست مسیر canonical

- تست اکنون فقط وجود سه عملیات و ترتیب آن‌ها را در عبارت `repositoryRoot` بررسی می‌کند: ابتدا resolve مسیر ریشه، سپس canonicalFile و در آخر toPath. این تست به فرمت تک‌خطی وابسته نیست.
- کنترل runtime همچنان فایل keystore را canonical می‌کند و با `Path.startsWith(repositoryRoot)` قرارگیری داخل checkout را رد می‌کند.
- نتیجهٔ QG و Product Build commit اصلاحی هنوز pending است؛ T0212 در وضعیت `in_progress` باقی می‌ماند.
## A004 — تأیید Quality Gate در V 3.0.972

- Quality Gate run 42062471023 با موفقیت کامل شد: ۴۲۹ آزمون Python، compileall، graph guard و recorded-output/evidence validation پاس شدند.
- Graph Guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را بررسی کرد؛ صفر خطا و یک هشدار تاریخی T0001 باقی ماند.
- تست canonical path اکنون با بررسی ترتیب عملیات چندخطی موفق است.
- Product Build run 42062471014 هنگام ثبت A005 هنوز در حال اجرا بود؛ HostLifecycle موفق شده بود، اما نتیجهٔ نهایی Android/Windows هنوز قطعی نبود.

## A005 — اصلاح نهایی ترتیب importهای Gradle

- KtLint در Product Build run 42062004172 صریحاً اعلام کرد importهای `java`، `javax` و `kotlin` باید در انتهای ترتیب lexicographic قرار بگیرند.
- ترتیب importهای Gradle اکنون به شکل `com.google.protobuf.gradle.*`، سپس `org.gradle.api.GradleException` و در پایان `java.io.File` است؛ فاصلهٔ خالی بین importها وجود ندارد.
- Quality Gate نسخهٔ قبلی موفق است؛ Product Build تازه باید KtLint، build debug و `assembleRelease` با کلید موقت را تأیید کند.
- هیچ secret یا keystore تولیدی ساخته یا استفاده نشده است. T0212 تا تأیید smoke و اجرای workflow محافظت‌شده در وضعیت `in_progress` می‌ماند.


## A006 — پذیرش CI روی V 3.0.973

**commit بررسی‌شده:** `5904eaac1629faba61483dc504a6c548de49b22c` روی `main`.

### Quality Gate
- Step Three Quality Gate run [42063282020](https://github.com/artinrahimii/project/actions/runs/42063282020) موفق شد.
- ۴۲۹ آزمون Python اجرا و همگی موفق شدند؛ `compileall` نیز موفق بود.
- Graph Guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را بررسی کرد: صفر خطا و یک هشدار تاریخی T0001 دربارهٔ metadata غیرشناسه‌ای. هشدار بدون تغییر حفظ شد.

### Android
- Android job [127370445148](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445148) با موفقیت کامل شد.
- `gradle --no-daemon ktlintCheck` موفق شد.
- `gradle --no-daemon assembleDebug --no-build-cache` موفق شد.
- `gradle --no-daemon assembleRelease --no-build-cache` با یک JKS تصادفی و موقت در مسیر خارج checkout موفق شد.
- `apksigner verify --verbose --print-certs` امضا را معتبر تشخیص داد؛ APK با scheme v2 تأیید شد و digest گواهی SHA-256 با طول ۶۴ رقم hex گزارش شد. گواهی متعلق به smoke موقت CI است و fingerprint تولیدی محسوب نمی‌شود.
- cleanup کلید موقت موفق شد؛ کلید smoke برای توزیع یا به‌روزرسانی محصول استفاده نمی‌شود.
- بررسی حداقل مجوزهای APK موفق شد؛ مجوزهای بسته‌شده فقط INTERNET و permission خصوصی signature-protected مربوط به AndroidX بودند.
- آزمون‌های JVM از ۳۴ فایل JUnit، ۲۲۴ مورد را ثبت کردند: ۲۲۳ موفق، یک skip، صفر failure و صفر error. دو گزارش HTML پوشش با اندازهٔ ۱۱٬۶۵۱ و ۱۱٬۴۶۹ بایت اعتبارسنجی شدند.
- آزمون instrumentation با موفقیت اجرا شد؛ ۱۲ تست روی emulator خاتمه یافتند. آزمون `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan` به‌علت نبود ورودی peer/IP/port در workflow عادی skip شد؛ بنابراین این اجرا اثبات اتصال LAN واقعی نیست.
- APK دیباگ، گزارش instrumentation، گزارش JVM و artifact پوشش/نتایج بحرانی بارگذاری شدند.

### Windows و HostLifecycle
- Windows job [127370445471](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445471) با ۲۲/۲۲ مرحله موفق شد؛ buildهای .NET بدون warning/error، smoke راه‌اندازی WPF، بررسی‌های سیاست/هویت، پذیرش installer و بارگذاری artifactها موفق بودند.
- HostLifecycle job [127370445384](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445384) با ۱۳/۱۳ مرحله موفق شد. Cobertura معتبر: ۸٬۸۴۹ از ۱۰٬۳۲۵ خط (۸۵٫۷۰٪)، ۶۷ نشانگر PASS و یک SKIP. مورد skipشده، آزمون real-model opt-in است و اجرای مدل واقعی ادعا نمی‌شود.
- Product Build run [42063281545](https://github.com/artinrahimii/project/actions/runs/42063281545) نتیجهٔ نهایی `success` داشت؛ هر سه job ویندوز، اندروید و HostLifecycle موفق شدند.

### Artifactهای واقعی همین run
- Android instrumentation reports: `13434575334`، ۹۹٬۲۹۷ بایت.
- Android unit-test reports: `13433990937`، ۹۰٬۹۷۵ بایت.
- Android debug APK: `13433781042`، ۲۸٬۹۷۲٬۴۷۱ بایت.
- Windows installer: `13432905703`، ۶۰٬۹۷۷٬۷۳۹ بایت.
- Android coverage/critical results: `13432679732`، ۵٬۳۶۸ بایت.
- HostLifecycle coverage/critical results: `13432588457`، ۱۱۰٬۹۳۹ بایت.
- Windows host: `13431974161`، ۱۰۱٬۵۶۴٬۹۸۰ بایت.

### مرز پذیرش و وضعیت
این اجرا، مسیر **امضای موقت CI** را اثبات می‌کند، نه امضای تولیدی. workflow `.github/workflows/android-release.yml` در اجرای push مربوط به run `42063279308` هیچ job تولیدی اجرا نکرد؛ اجرای واقعی آن به trigger دستی یا tag اختصاصی نیاز دارد. از اتصال فعلی امکان مشاهده/تأیید secretهای GitHub Environment یا اجرای دستی workflow فراهم نبود. وجود یا نبود secretهای تولیدی را حدس نمی‌زنم و هیچ کلید خصوصی یا مقدار محرمانه‌ای دریافت یا ثبت نشده است.

بنابراین `T0212` همچنان `in_progress` باقی می‌ماند تا maintainer، Environment محافظت‌شدهٔ `android-release` را با keystore تولیدی و fingerprint ثابت پیکربندی کند و workflow تولیدی، APK نهایی را با تطبیق fingerprint امضا و artifact آن را منتشر کند. پس از آن می‌توان معیار checksum وظیفهٔ T0213 را بر فایل نهایی اعمال کرد.


## A006 — بازاعتبارسنجی تکمیلی پس از V 3.0.974

**commit بررسی‌شده:** `219c3e44025ea0c25d6c254a5379209286ca2d93` روی شاخهٔ `main`.

### Quality Gate
- [Quality Gate run 42065762873](https://github.com/artinrahimii/project/actions/runs/42065762873) موفق شد.
- ۴۲۹ آزمون Python موفق شدند؛ `compileall` موفق بود؛ Graph Guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را بررسی کرد.

### Product Build
- [Product Build run 42065762818](https://github.com/artinrahimii/project/actions/runs/42065762818) با نتیجهٔ نهایی `success` پایان یافت و هر سه job موفق شدند.
- Android job [127379261869](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379261869): ‏KtLint، `assembleDebug` و `assembleRelease` موفق شدند. Release smoke با JKS موقت CI ساخته شد و اجرای `apksigner verify` موفق بود. digest گواهی smoke برابر `b7fe6f8501d232d9550f194848c9ce1a32b337beada9e1f4fa72a4a6dedbbba2` بود؛ این fingerprint فقط متعلق به گواهی موقت است و fingerprint تولیدی نیست.
- آزمون JVM اندروید: ۲۲۴ تست از ۳۴ فایل؛ ۲۲۳ موفق، یک skip، صفر failure و صفر error.
- Windows job [127379262443](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379262443): آزمون startup رابط WPF، کنترل‌های ابزار/DPAPI و پذیرش نصب/حذف installer موفق شدند. یک هشدار غیرمانع دربارهٔ فاصله‌های انتهایی metadata فیلد PE با نام CompanyName ثبت شد؛ ثبت Publisher در نصب واقعی جداگانه موفق بود.
- HostLifecycle job [127379262455](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379262455): ‏۶۷ نشانگر PASS؛ پوشش Cobertura برابر ۸٬۸۴۸ از ۱۰٬۳۲۵ خط (۸۵٫۶۹٪)؛ یک آزمون real-model اختیاری به‌صراحت SKIP شد.

### Artifactهای همین اجرا
- [Android debug APK — 13435100872](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13435100872)، ۲۸٬۹۷۲٬۴۷۷ بایت.
- [Windows installer — 13434616602](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13434616602)، ۶۰٬۹۷۸٬۴۰۲ بایت.
- [Android coverage/critical results — 13434442890](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13434442890)، ۵٬۳۶۸ بایت.
- [Android instrumentation reports — 13434152921](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13434152921)، ۱۱۰٬۸۰۸ بایت.
- [HostLifecycle coverage/critical results — 13433522628](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13433522628)، ۱۱۰٬۹۱۸ بایت.
- [Android JVM reports — 13433208534](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13433208534)، ۹۱٬۰۹۲ بایت.
- [Windows host — 13432783766](https://github.com/artinrahimii/project/actions/runs/42065762818/artifacts/13432783766)، ۱۰۱٬۵۶۴٬۹۸۸ بایت.

### مرز پذیرش
این بازاعتبارسنجی موفقیت build و امضای smoke با کلید دورریختنی CI را ثابت می‌کند؛ **امضای تولیدی هنوز اجرا و تأیید نشده است**. هیچ artifact امضاشده با گواهی production یا تطبیق fingerprint ثابت در این run تولید نشد. بنابراین T0212 همچنان `in_progress` است و T0213 به‌دلیل وابستگی سخت به T0212 نباید آغاز شود.

## A007 — بررسی V 3.0.975 و مانع امضای تولیدی

**commit مبنا:** `fa69ec6ee58998801e22e4811e331f4dc2ac6f7a` (`V 3.0.975`)، شاخهٔ آنلاین `main`.

### آزمون‌های انجام‌شده
- [Step Three Quality Gate run 42068538634](https://github.com/artinrahimii/project/actions/runs/42068538634): job کامل شد؛ آزمون‌های خودکار، بررسی نحوی Python، اعتبارسنجی ساختار گراف و بررسی خروجی‌ها/شواهد همگی موفق شدند.
- [White-P Product Build run 42068534197](https://github.com/artinrahimii/project/actions/runs/42068534197): هر سه job مربوط به Android، Windows WPF و HostLifecycle با نتیجهٔ موفق پایان یافتند. در Android، `assembleDebug` و `assembleRelease` اجرا شدند؛ امضای Release فقط با JKS موقت CI انجام شد، `apksigner` آن را تأیید کرد و کلید موقت پاک شد. این آزمون، امضای تولیدی را اثبات نمی‌کند.
- [Android signed release run 42068530991](https://github.com/artinrahimii/project/actions/runs/42068530991): رویداد ثبت‌شده `push` بود و نتیجهٔ workflow برابر failure با **صفر job** بود. بنابراین هیچ مرحله‌ای از اعتبارسنجی secretها، ساخت با keystore تولیدی، تطبیق fingerprint یا بارگذاری APK تولیدی اجرا نشد. این نتیجه، شکست آزمون رمزنگاری نیست؛ آزمون امضای تولیدی اصلاً اجرا نشده است.

### کنترل trigger و دسترسی
فایل `.github/workflows/android-release.yml` در commit مبنا فقط `workflow_dispatch` و push روی tagهای `android-v*` را تعریف می‌کند؛ push عادی شاخهٔ `main` مسیر امضای تولیدی را اجرا نمی‌کند. اتصال GitHub در این نشست ابزار provision کردن secretهای Environment یا اجرای `workflow_dispatch` ارائه نمی‌دهد. از این محدودیت نمی‌توان نتیجه گرفت که secretهای Environment وجود ندارند یا وجود دارند. هیچ keystore، رمز یا fingerprint خصوصی دریافت، چاپ یا در مخزن ذخیره نشده است.

### تصمیم وضعیت
`T0212` از `in_progress` به `blocked` تغییر یافت؛ علت، نیاز به اقدام maintainer برای تأیید/پیکربندی Environment محافظت‌شدهٔ `android-release` و اجرای workflow تولیدی است. دو یال ورودی از `T0208` و `T0211` همچنان `sequencing_only / validated` و یال `T0212 → T0213` همچنان `hard_dependency / validated` باقی ماندند. هیچ یال یا قرارداد فنی تغییر نکرد.

`T0213` آغاز نشد، چون بدون APK تولیدی با fingerprint تأییدشده، checksum خروجی انتشار قابل‌قبول نیست. اقدام لازم: maintainer باید secretهای لازم و متغیر `ANDROID_RELEASE_CERT_SHA256` را در Environment محافظت‌شده آماده کند، workflow را با `workflow_dispatch` اجرا کند و artifact `whitep-android-release-signed` را پس از تطبیق fingerprint تحویل دهد.

## A008 — بازاعتبارسنجی V 3.0.977

**commit مبنا:** `441a058c2461b049a74faba414dbc0ba7aa3eb49` روی شاخهٔ آنلاین `main`.

### نتایج CI
- [Step Three Quality Gate run 42071006532](https://github.com/artinrahimii/project/actions/runs/42071006532): یک job با نتیجهٔ success؛ آزمون‌های خودکار، بررسی نحوی Python، اعتبارسنجی گراف و بررسی خروجی‌ها/شواهد موفق شدند.
- [White-P Product Build run 42071006454](https://github.com/artinrahimii/project/actions/runs/42071006454): هر سه job مربوط به Android، Windows WPF/installer و HostLifecycle با نتیجهٔ success پایان یافتند. امضای Release اندروید فقط با JKS موقت CI آزموده و بعد پاک‌سازی شد؛ این گواهی تولیدی نیست.
- [Rendezvous container run 42071006474](https://github.com/artinrahimii/project/actions/runs/42071006474): build و smoke test کانتینر موفق شد.
- [Android signed release run 42071003708](https://github.com/artinrahimii/project/actions/runs/42071003708): رویداد `push`، نتیجهٔ نهایی `failure` و تعداد jobها صفر بود. بنابراین هیچ step مربوط به دریافت Secretها، ساخت با keystore تولیدی، `apksigner`، تطبیق fingerprint یا بارگذاری APK تولیدی اجرا نشد.

### علت و مرز تشخیص
فایل `.github/workflows/android-release.yml` در commit مبنا triggerهای `workflow_dispatch` و push روی tagهای `android-v*` را اعلام می‌کند. run با event عادی push روی شاخهٔ main هیچ jobی اجرا نکرده است. این داده به‌تنهایی ثابت نمی‌کند که Secretهای Environment وجود ندارند یا وجود دارند و نیز اثبات شکست رمزنگاری امضا نیست. ابزارهای GitHub در این نشست امکان اجرای دستی workflow یا مدیریت Secretهای Environment محافظت‌شده را ارائه نمی‌کنند. هیچ keystore، رمز یا fingerprint خصوصی دریافت یا ثبت نشده است.

### تصمیم
`T0212` همچنان `blocked` باقی می‌ماند. یال‌های ورودی آن از `T0208` و `T0211` بدون تغییر `sequencing_only / validated` هستند و یال `T0212 → T0213` بدون تغییر `hard_dependency / validated` باقی ماند. `T0213` شروع نشد؛ checksum APK انتشار فقط پس از دریافت و تأیید artifact تولیدی `whitep-android-release-signed` قابل‌قبول است.

اقدام لازم: maintainer باید Environment محافظت‌شدهٔ `android-release` را تأیید/پیکربندی کند و workflow را از طریق `workflow_dispatch` یا tag انتشار مجاز اجرا کند. پذیرش تنها با تطبیق fingerprint SHA-256 گواهی تولیدی و بارگذاری artifact نهایی ممکن است.


## A009 — بازبینی مجدد روی آخرین main (V 3.0.979)

- مبنای دقیق: `2ae34d8b7d90b9becab3842074b9efc92c1ca2e0` با پیام `V 3.0.979`.
- Quality Gate run 42074239731: موفق؛ یک job روی همین commit تکمیل شد و کنترل ساختار گراف و شواهد خروجی موفق بود.
- Product Build run 42074239730: هر سه job موفق شدند؛ Android client (۳۲ مرحله)، Windows WPF/installer (۲۲ مرحله) و HostLifecycle (۱۳ مرحله). امضای release در این مسیر فقط با کلید موقت CI انجام می‌شود و هویت تولیدی نیست.
- Rendezvous container smoke run 42074239854: موفق.
- Android signed release run 42074236692: رویداد `push`، نتیجهٔ run برابر `failure` و تعداد job برابر صفر. فایل workflow فقط `workflow_dispatch` و tagهای `android-v*` را می‌پذیرد؛ بنابراین هیچ مرحلهٔ بررسی secret، امضای تولیدی، تطبیق fingerprint یا بارگذاری APK امضاشده اجرا نشد.
- اتصال GitHub در این اجرا عملیاتی برای dispatch دستی یا مدیریت secretهای Environment ارائه نمی‌کند. از روی این run نمی‌توان نبود secretها را نتیجه گرفت؛ فقط می‌توان گفت امضای تولیدی هنوز اجرا و تأیید نشده است.
- وضعیت جاری: `blocked`. یال سخت `T0212 → T0213` معتبر باقی می‌ماند و T0213 شروع نشده است. این بازبینی فقط شواهد CI و وضعیت مانع را به‌روز می‌کند و پیشرفت پیاده‌سازی محسوب نمی‌شود.


## A010 — بررسی CI روی main با نسخهٔ V 3.0.980

- commit مبنا: `d7feb491ff974214348909cd840265d28a174d23`.
- Quality Gate run 42076992391: موفق.
- Product Build run 42076992372: jobهای Windows WPF/installer و HostLifecycle موفق شدند. در job اندروید، KtLint، ساخت clean APK دیباگ، ساخت `assembleRelease` با کلید موقت، بررسی `apksigner`، کنترل permission و پاک‌سازی کلید موقت موفق بودند. در آخرین مشاهده، آزمون JVM/coverage اندروید و مراحل emulator هنوز در حال اجرا بودند؛ بنابراین نتیجهٔ نهایی Product Build در این گزارش موفق اعلام نمی‌شود.
- Rendezvous container run 42076992419: موفق.
- Android signed-release run 42076990441: رویداد `push`، نتیجهٔ failure و صفر job؛ هیچ مرحلهٔ امضای تولیدی اجرا نشد.
- درخواست فهرست Environmentها و Environment با نام `android-release` از طریق connector با خطای endpoint غیرمجاز رد شد. ابزار dispatch دستی و مدیریت secret نیز ارائه نشده است؛ در نتیجه وضعیت وجود Secretها و تنظیمات Environment قابل مشاهده نیست.
- وضعیت: `T0212=blocked`. وابستگی سخت `T0212 → T0213` تغییر نکرد و T0213 شروع نشد.

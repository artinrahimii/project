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

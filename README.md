# White-P — اجرای محلی مدل و گفت‌وگوی همتا‌به‌همتا

## هدف محصول

White-P با هدف ساخت دو برنامه طراحی می‌شود: یک میزبان ویندوز برای اجرای مدل‌های هوش مصنوعیِ قابل اجرا روی سخت‌افزار خود میزبان، و یک کلاینت اندروید برای فرستادن درخواست به میزبان‌های در دسترس. مسیر هدف، ارتباط مستقیم همتا‌به‌همتا (P2P) با شناسه/هش موقت برای هر درخواست است؛ سرویس مرکزی نباید متن درخواست، پاسخ تولیدشده یا تاریخچهٔ گفت‌وگو را دریافت یا نگهداری کند.

این README شرح هدف نسخهٔ نخست و وضعیت واقعی مخزن است؛ ادعای آماده‌بودن محصول نیست.

## اجزای هدف نسخهٔ نخست

- **میزبان ویندوز:** رابط WPF با C# و .NET 10؛ اسکلت رابط و وضعیت اولیهٔ چرخهٔ عمر موجود است، اما اتصال به موتور استنتاج محلی و کشف سخت‌افزار هنوز پیاده‌سازی نشده‌اند.
- **کلاینت اندروید:** Kotlin، Jetpack Compose و Gradle؛ صفحهٔ ورود درخواست و وضعیت اولیه موجود است، اما انتقال درخواست به میزبان هنوز پیاده‌سازی نشده است.
- **قراردادهای مشترک:** طرح نسخه‌دار Protobuf در `protocol/whitep/v1/messages.proto` ایجاد شده و کلاس‌های C#/Java هنگام build از همان منبع تولید می‌شوند؛ انتقال شبکه، اتصال میزبان/کلاینت و گردش واقعی درخواست هنوز پیاده‌سازی نشده‌اند.
- **موتور محلی:** اتصال به backend واقعی روی همان دستگاه میزبان، با کنترل خطا، لغو و health check هنوز پیاده‌سازی نشده است.
- **انتقال P2P:** تصمیم معماری نسخهٔ نخست در [ADR-0002 — Direct P2P data path](docs/decisions/ADR-0002-direct-p2p-data-path.md) این است که درخواست و پاسخ استنتاج مستقیماً بین کلاینت Android و میزبان Windows منتقل شوند؛ سرویس rendezvous فقط برای کشف همتا و هماهنگی اتصال است و نباید محتوای استنتاج را relay کند. این تصمیم هنوز پیاده‌سازی نیست؛ انتقال P2P، کشف همتا و آزمون شبکه هنوز موجود نیست.
- **آزمون و CI:** آزمون‌های Python برای قراردادهای مستندات و گراف، آزمون چرخهٔ عمر میزبان، و workflowهای کنترل کیفیت و ساخت وجود دارند.

این موارد محدودهٔ هدف هستند؛ وجودشان در این فهرست به معنی پیاده‌سازی کامل نیست.

## محدودهٔ نسخهٔ نخست

نسخهٔ نخست باید بتواند میزبان را اجرا کند، backend محلی واقعی را شناسایی کند، فقط یک inference هم‌زمان برای هر میزبان بپذیرد، درخواست را از کلاینت اندروید به میزبان برساند و پاسخ را بدون عبور متن درخواست از یک API مرکزی برگرداند. خطا، لغو، deadline، مدل ناسازگار و میزبان مشغول باید وضعیت‌های قابل‌تشخیص داشته باشند.

مواردی مانند بازارچهٔ مدل کامل، اجرای ابزارهای عامل با دسترسی گسترده، چند inference هم‌زمان روی یک میزبان و هرگونه ادعای امنیتی تأییدنشده خارج از محدودهٔ نسخهٔ نخست‌اند.

## وضعیت واقعی مخزن

در commit فعلی، پروژهٔ WPF در `src/Host.Windows/`، پروژهٔ Android Gradle در `src/Client.Android/` و آزمون چرخهٔ عمر میزبان در `tests/HostLifecycle.Tests/` وجود دارند. فایل `global.json`، تنظیمات Gradle و workflow ساخت در `.github/workflows/product-build.yml` نسخه‌های ابزار و ورودی‌های ساخت را مشخص می‌کنند. workflow محصول برای ساخت میزبان ویندوز، تولید debug APK اندروید و اجرای آزمون چرخهٔ عمر تنظیم شده است؛ نتیجهٔ هر اجرای مشخص باید از صفحهٔ همان run بررسی شود.

این پروژه‌ها هنوز در حال تکمیل‌اند. یک adapter واقعی HTTP برای backend محلی llama.cpp و رابط Windows برای اتصال، بررسی سلامت و ارسال prompt پیاده‌سازی شده‌اند؛ اما اجرای موفق با یک مدل واقعی روی دستگاه هدف هنوز تأیید نشده است. وجود build موفق به‌تنهایی اثبات نمی‌کند که inference روی مدل واقعی، انتقال P2P، رمزنگاری، تشخیص سخت‌افزار یا گردش کامل درخواست کار می‌کند. انتقال P2P، کشف همتا، تشخیص سخت‌افزار، رمزنگاری انتقال و آزمون end-to-end محصول هنوز تحویل نشده‌اند.

## تصمیم‌های نسخه و ابزار ساخت

- مبنای میزبان ویندوز: .NET SDK نسخهٔ `10.0.100` با `global.json` و هدف `net10.0-windows` همراه WPF.
- مبنای Android: JDK 17، Gradle 8.13 در CI، `compileSdk/targetSdk = 36` و `minSdk = 26`.
- دلیل و محدودیت این انتخاب‌ها در [ADR-0001 — Platform build baselines](docs/decisions/ADR-0001-platform-build-baselines.md) ثبت شده است.
- نسخه‌های فوق baseline فعلی build هستند، نه تضمین پشتیبانی همهٔ دستگاه‌ها یا آمادگی انتشار.

## اصول حریم خصوصی و امنیت

- متن درخواست و پاسخ نباید به سرویس مرکزی ارسال یا در log عادی ثبت شود.
- هش/شناسهٔ درخواست باید موقت و غیرقابل‌استفاده به‌عنوان شناسهٔ دائمی کاربر باشد.
- هر ادعای «محلی»، «P2P»، «رمزگذاری‌شده» یا «بدون ذخیره‌سازی» تنها پس از آزمون یا بررسی قابل‌تکرار قابل تأیید است.
- تا زمانی که آزمون‌های متناظر اجرا نشده‌اند، این موارد الزامات طراحی‌اند، نه ادعاهای اثبات‌شدهٔ محصول.

## توسعه و آزمون

با Python 3.11 یا جدیدتر، کنترل‌های فعلی مخزن را اجرا کن:

```bash
python -m unittest discover -s tests -v
python -m compileall -q tools tests
python tools/graph_guard.py "step three/project-graph.md" --repo-root .
```

ساخت میزبان ویندوز در runner ویندوز با .NET SDK 10 انجام می‌شود؛ ساخت APK اندروید در runner لینوکس با JDK 17، Gradle 8.13 و Android SDK platform/build-tools 36 انجام می‌شود. workflow متناظر: [product-build.yml](.github/workflows/product-build.yml). برای نتیجهٔ واقعی به اجرای موفق GitHub Actions و artifactهای همان اجرا مراجعه کن؛ اجرای آزمون‌های Python جایگزین build یا آزمون runtime محصول نیست.



### ساخت و آزمون از checkout تمیز

دستورهای زیر با workflowهای فعلی هماهنگ‌اند. برای میزبان ویندوز، .NET SDK باید با `global.json` سازگار باشد: نسخهٔ پایه `10.0.100` و سیاست `latestFeature`. برای اندروید، JDK 17، Gradle 8.13 و Android SDK platform `android-36`، build-tools `36.0.0` و platform-tools لازم است. محیط CI همین ورودی‌ها را از یک checkout تازه تنظیم می‌کند.

از ریشهٔ مخزن، ساخت میزبان ویندوز و اجرای آزمون چرخهٔ عمر:

```bash
dotnet build src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release
dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release
```

برای ساخت APK اندروید، ابتدا JDK 17، Gradle 8.13 و بسته‌های Android SDK یادشده را نصب و `ANDROID_HOME` را به SDK root تنظیم کن؛ سپس:

```bash
cd src/Client.Android
gradle --no-daemon assembleDebug
```

آزمون‌ها و کنترل‌های مخزن از ریشه:

```bash
python -m unittest discover -s tests -v
python -m compileall -q tools tests
python tools/graph_guard.py "step three/project-graph.md" --repo-root .
```

workflow محصول پس از build، خروجی میزبان ویندوز و فایل `app-debug.apk` را به‌عنوان artifact برای ۷ روز منتشر می‌کند و در صورت نبودن فایل خروجی شکست می‌خورد. موفقیت ساخت و وجود artifact، اجرای درست برنامه، inference محلی یا تبادل P2P را اثبات نمی‌کند.

## وضعیت توسعه

مرحلهٔ سوم به‌صورت وظیفه‌محور پیش می‌رود. هر قابلیت فقط با فایل واقعی، آزمون متناسب و شواهد قابل‌بررسی تکمیل اعلام می‌شود. برای وضعیت و وابستگی‌های جاری به `step three/project-graph.md` مراجعه کنید.


## Local inference backend (T0018)

The Windows host now includes an HTTP client for a separately installed llama.cpp server. Start llama.cpp with its OpenAI-compatible server endpoint and confirm `GET /health` succeeds; configure the backend with the server base URL (for example, `http://127.0.0.1:8080/`) and the model identifier expected by that server. The client calls `POST /v1/chat/completions`, supports cancellation/deadlines, rejects empty prompts and invalid output-token limits, and serializes requests per backend instance. It never writes prompt/response contents to application logs. The Windows host UI lets the user enter the local server URL and model ID, check backend health, submit a prompt, and view returned text. Start a compatible llama.cpp server separately (for example at `http://127.0.0.1:8080/`), then open the Windows host and select **Connect**. This code does not bundle llama.cpp or a model; a live-model run on the target machine remains a separate acceptance check.

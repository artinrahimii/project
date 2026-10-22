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

## ساختار مخزن

- `src/Host.Windows/`: میزبان ویندوز با رابط WPF و backend محلی.
- `src/Client.Android/`: کلاینت اندروید.
- `WhiteP.sln`: solution ریشه که پروژهٔ میزبان ویندوز و کتابخانهٔ مشترک .NET را با هم build می‌کند.
- `src/Shared.Contracts/`: کتابخانهٔ buildable برای قراردادهای مشترک .NET با هدف `net10.0`؛ فایل‌های تولیدشده از Protobuf در وظیفهٔ T0036 یکپارچه می‌شوند.
- `protocol/whitep/v1/messages.proto`: منبع یگانهٔ قرارداد wire؛ کپی جداگانهٔ فایل proto در پوشهٔ پروژهٔ مشترک ایجاد نشده است.
- `tests/`: آزمون‌های پایتون و چرخهٔ عمر میزبان.
- `tests/contracts/`: آزمون‌های کشف‌پذیر برای ساختار قراردادها و مسیر منبع canonical.
- `docs/`: نیازمندی‌ها، معیارهای پذیرش و تصمیم‌های فنی.

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
- فهرست تصمیم‌های فناوری و پیوند هر تصمیم به کد/آزمون متناظر در [دفتر تصمیم‌ها](docs/decisions/README.md) قرار دارد.
- تصمیم انتخاب backend استنتاج محلی و اتصال آن به رابط میزبان در [ADR-0003 — Local inference backend](docs/decisions/ADR-0003-local-inference-backend.md) ثبت شده است.
- نسخه‌های فوق baseline فعلی build هستند، نه تضمین پشتیبانی همهٔ دستگاه‌ها یا آمادگی انتشار.

## اصول حریم خصوصی و امنیت

- متن درخواست و پاسخ نباید به سرویس مرکزی ارسال یا در log عادی ثبت شود.
- هش/شناسهٔ درخواست باید موقت و غیرقابل‌استفاده به‌عنوان شناسهٔ دائمی کاربر باشد.
- هر ادعای «محلی»، «P2P»، «رمزگذاری‌شده» یا «بدون ذخیره‌سازی» تنها پس از آزمون یا بررسی قابل‌تکرار قابل تأیید است.
- تا زمانی که آزمون‌های متناظر اجرا نشده‌اند، این موارد الزامات طراحی‌اند، نه ادعاهای اثبات‌شدهٔ محصول.

## Restore، build، test و package

این دستورها از checkout تمیز قابل اجرا هستند؛ مسیرها نسبت به ریشهٔ مخزن‌اند مگر آن‌که دستور صریحاً وارد پوشهٔ Android شود. نسخه‌های ابزار با فایل‌های مخزن و workflowهای فعلی تطبیق داده شده‌اند.

### پیش‌نیازهای ابزار

- **Windows host:** Windows، .NET SDK نسخهٔ پایهٔ `10.0.100` با سیاست `latestFeature` در `global.json`. پروژهٔ میزبان WPF روی `.NET 10` ساخته می‌شود.
- **Android client:** JDK 17، Gradle 8.13، Android SDK platform 36، build-tools 36.0.0 و platform-tools؛ متغیرهای `ANDROID_HOME` و `ANDROID_SDK_ROOT` باید به مسیر SDK اشاره کنند.
- **Repository checks:** Python 3.13 برای تطبیق با workflow کنترل کیفیت.
- این مخزن در حال حاضر Gradle Wrapper (`gradlew`) ندارد؛ Gradle 8.13 باید جداگانه نصب یا از محیطی مشابه workflow CI فراهم شود.

### Restore، build و آزمون میزبان ویندوز

از ریشهٔ مخزن در یک ترمینال ویندوز:

```powershell
dotnet restore WhiteP.sln
dotnet format whitespace WhiteP.sln --verify-no-changes
dotnet build WhiteP.sln --configuration Release
dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release
```

آزمون چرخهٔ عمر یک برنامهٔ کنسولی آزمون است که با `dotnet run` اجرا می‌شود؛ در وضعیت فعلی آن را با `dotnet test` جایگزین نکن. برای تولید پوشهٔ انتشار میزبان win-x64:

```powershell
dotnet publish src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/windows-host
```

خروجی `publish` یک پوشهٔ برنامه برای سیستمی است که .NET Runtime سازگار دارد؛ این دستور به‌تنهایی نصب‌کنندهٔ MSI یا بستهٔ امضاشده تولید نمی‌کند. workflow فعلی میزبان را build و خروجی build را artifact می‌کند؛ اجرای دستور publish بالا فعلاً مرحله‌ای مستقل است و در workflow محصول اجرا نمی‌شود.

### Lint، build و package اندروید

در ریشهٔ مخزن، ابتدا ابزارها و بسته‌های Android SDK بالا را نصب کن؛ سپس:

```bash
cd src/Client.Android
gradle --no-daemon ktlintCheck
gradle --no-daemon assembleDebug
```

بستهٔ APK توسعه در `src/Client.Android/app/build/outputs/apk/debug/app-debug.apk` ساخته می‌شود. این debug APK جایگزین بستهٔ release امضاشده نیست. workflow محصول همین lint و ساخت APK را اجرا و artifact را برای ۷ روز نگه می‌دارد. در حال حاضر آزمون runtime اندروید روی دستگاه فیزیکی به‌وسیلهٔ این workflow اثبات نمی‌شود.

### آزمون‌های مخزن و اعتبارسنجی گراف

از ریشهٔ مخزن با Python 3.13:

```bash
python -m unittest discover -s tests -v
python -m compileall -q tools tests
python tools/graph_guard.py "step three/project-graph.md"
python tools/graph_guard.py "step three/project-graph.md" --repo-root .
```

اولین فرمان آزمون‌های خودکار را اجرا می‌کند؛ فرمان دوم نحو فایل‌های Python را بررسی می‌کند؛ دو فرمان آخر به‌ترتیب ساختار گراف و وجود مسیرهای خروجی/شواهد ثبت‌شده را اعتبارسنجی می‌کنند. این کنترل‌ها جایگزین build محصول یا آزمون inference واقعی نیستند.

### Workflowهای CI و محدودیت نتیجه

- [کنترل کیفیت و آزمون‌های Python](.github/workflows/quality.yml)
- [ساخت میزبان ویندوز، آزمون چرخهٔ عمر و APK اندروید](.github/workflows/product-build.yml)

workflow محصول build میزبان، قالب‌بندی C#، آزمون چرخهٔ عمر و lint/build اندروید را اجرا می‌کند. موفقیت CI و وجود artifact، اجرای برنامه روی دستگاه هدف، تولید پاسخ از مدل زنده، نصب‌کنندهٔ release، یا انتقال P2P را به‌خودی‌خود اثبات نمی‌کند.

## وضعیت توسعه

مرحلهٔ سوم به‌صورت وظیفه‌محور پیش می‌رود. هر قابلیت فقط با فایل واقعی، آزمون متناسب و شواهد قابل‌بررسی تکمیل اعلام می‌شود. برای وضعیت و وابستگی‌های جاری به `step three/project-graph.md` مراجعه کنید.


## Local inference backend (T0018)

The Windows host now includes an HTTP client for a separately installed llama.cpp server. Start llama.cpp with its OpenAI-compatible server endpoint and confirm `GET /health` succeeds; configure the backend with the server base URL (for example, `http://127.0.0.1:8080/`) and the model identifier expected by that server. The client calls `POST /v1/chat/completions`, supports cancellation/deadlines, rejects empty prompts and invalid output-token limits, and serializes requests per backend instance. It never writes prompt/response contents to application logs. The Windows host UI lets the user enter the local server URL and model ID, check backend health, submit a prompt, and view returned text. Start a compatible llama.cpp server separately (for example at `http://127.0.0.1:8080/`), then open the Windows host and select **Connect**. This code does not bundle llama.cpp or a model; a live-model run on the target machine remains a separate acceptance check.

# گزارش اجرای UPD-S1-023 — سیاست انتخاب خودکار مدل و سقف خروجی

## وضعیت
**تکمیل‌شده (done) پس از اعتبارسنجی دقیق A003 در commit `9927e29a772bb1382fadd62ee2a02178b54998f0`.** A001 به‌علت هفت خطای کامپایل شکست خورد. A002 آن‌ها را رفع کرد اما HostLifecycle در runtime به‌درستی backend آزمایشی فاقد capability را رد کرد؛ A003 fixture را به capability provider مجهز می‌کند. CI دقیق A003 در انتظار است.

## مبنای ممیزی
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit مبنا: `c65ec1ae2ffc9020672971dafdecbd3a4558d87e`
- پیام: `V 3.1.159`
- workflow محصول روی همین مبنا: run [42835719820](https://github.com/artinrahimii/project/actions/runs/42835719820) ناموفق بود و دو assertion آزمون Android باعث شکست JVM tests شده بودند.

## یافته‌های کد موجود
- سیاست Android در `AutomaticModelSelectionPolicy.kt` از قبل typed failure، بررسی metadata مدل، انتخاب deterministic و clamp خروجی را داشت.
- `AuthenticatedHostDiscovery` میزبان‌ها را بر اساس Peer ID به ترتیب پایدار مرتب می‌کند؛ آزمون موجود به‌اشتباه ترتیب ورودی را انتظار داشت.
- backend ویندوز از طریق `IInferenceModelCapabilityProvider` شناسهٔ مدل فعال و سقف‌های واقعی را می‌خواند؛ کاتالوگ نصب‌شده و SHA-256 فایل مدل نیز پیش از انتشار READY تأیید می‌شود.
- شکاف احرازشده در پذیرش درخواست ویندوز بود: سقف خروجی درخواست در admission و مرز نهایی inference با capability زندهٔ backend مقایسه نمی‌شد؛ overload مبتنی بر `InferenceRequest` هم شناسهٔ مدل درخواست را تا session حفظ نمی‌کرد.

## تغییرات اجرایی
1. سقف محصول مشترک `MaximumProductOutputTokens = 4096` افزوده شد؛ سقف پروتکل 131072 بدون تغییر باقی ماند.
2. `HostRequestAdmissionContext` اکنون max output درخواست و سقف context/output مدل فعال را حمل می‌کند.
3. سیاست admission در نبود capability، metadata ناسازگار، خروجی صفر یا عبور از سقف مدل/محصول/پروتکل، درخواست را fail-closed رد می‌کند.
4. handler ویندوز پیش از coordinator یک snapshot تازه از backend می‌گیرد؛ شناسهٔ مدل و سقف‌ها از snapshot واقعی می‌آیند، نه صرفاً مقدار TextBox.
5. `HostInferenceSession.GenerateAsync(InferenceRequest)` شناسهٔ مدل را حفظ می‌کند و زیر همان operation gate، capability واقعی را دوباره می‌خواند؛ مدل نامنطبق یا سقف بیش‌ازحد پیش از فراخوانی backend رد می‌شود.
6. سقف محصول و پروتکل در مسیر تولید محلی نیز کنترل می‌شود.
7. آزمون‌های C# برای سقف مدل، سقف محصول، مدل نامنطبق و خروجی صفر اضافه شدند.
8. آزمون discovery با ترتیب deterministic تولید اصلاح شد و fixture clamp با رابطهٔ معتبر `maxOutputTokens <= maxContextTokens` ساخته شد.

## فایل‌های تغییر‌یافته
- `src/Shared.Contracts/ProtocolMessageLimits.cs`
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`
- `src/Host.Application/HostInferenceSession.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AutomaticModelSelectionPolicyTest.kt`

## آزمون‌ها و خطاهای A001
- Quality gate دقیق V 3.1.160 موفق شد: [run 42848534130](https://github.com/artinrahimii/project/actions/runs/42848534130).
- ساخت/آزمون rendezvous موفق شد: [run 42848534328](https://github.com/artinrahimii/project/actions/runs/42848534328).
- HostLifecycle در مرحلهٔ کامپایل شکست خورد: یک ابهام HostStatus و شش سازندهٔ ناسازگار؛ [Product Build run 42848534275](https://github.com/artinrahimii/project/actions/runs/42848534275).
- آزمون inference واقعی اجرا نشد؛ پروژهٔ HostLifecycle پیش از رسیدن به inference build نشد؛ [run 42848534114](https://github.com/artinrahimii/project/actions/runs/42848534114).
- تلاش A002 شش محل سازنده را با سقف‌های آزمونی معتبر به‌روز کرد و alias صریح HostStatus افزود.
- **CI دقیق A002: در انتظار.**

## تلاش‌های ثبت‌شده
- A001: commit `a38ac25a08820c8d0095413eec44ea0c230d5f69`؛ نتیجهٔ failed؛ هفت خطای کامپایل C#.
- A002: commit `41bbccf74e49f4a87b5004761592e02aa78df14c`؛ کامپایل موفق شد، اما HostLifecycle در اجرای آزمون SessionTestBackend فاقد capability رد شد؛ نتیجهٔ failed.
- A003: backend fixture مشترک اکنون `IInferenceModelCapabilityProvider` را پیاده می‌کند و مدل fixture را با context/output معتبر گزارش می‌دهد؛ نتیجهٔ partial تا تأیید CI.
- run inference A002 پیش از اجرای مدل واقعی در آزمون QUIC/UDP alternative شکست خورد؛ نتیجهٔ inference واقعی هنوز اثبات نشده است.

## فایل اضافه‌شده در A003
- `tests/HostLifecycle.Tests/Program.cs`

## محدودیت‌های این تلاش
- آزمون فیزیکی Android به Windows و inference واقعی از مسیر P2P در این تلاش انجام نشده است.
- نصب تمیز و ارتقای درجا انجام نشده‌اند.
- تا سبزشدن CI و تأیید تمام معیارهای پذیرش، UPD-S1-023 تکمیل‌شده محسوب نمی‌شود.


## تلاش A004 — اعتبارسنجی دقیق روی V 3.1.162

- شناسهٔ تلاش: `UPD-S1-023-20261110-A004`
- بازهٔ CI: `2026-11-10T16:05:59Z` تا `2026-11-10T16:21:55Z`
- commit دقیق کد آزموده‌شده: `9927e29a772bb1382fadd62ee2a02178b54998f0` — `V 3.1.162`
- نتیجه: **succeeded**؛ این تلاش هیچ تغییر کدی تازه‌ای اعمال نکرد و فقط نتیجهٔ CI دقیق A003 را تثبیت کرد.
- تغییر A003 در `tests/HostLifecycle.Tests/Program.cs` باعث شد backend آزمایشی، هویت مدل و سقف‌های capability را صریحاً ارائه کند؛ رفتار fail-closed تولید دست‌نخورده باقی ماند.

### نتایج آزمون دقیق

1. Quality Gate: [run 42851148516](https://github.com/artinrahimii/project/actions/runs/42851148516) موفق؛ آزمون‌های Python، compileall و هر دو graph guard موفق.
2. Android/Windows Product Build: [run 42851148642](https://github.com/artinrahimii/project/actions/runs/42851148642) موفق؛ Android unit tests با **232 موفق، 1 ردشدهٔ مجاز، 0 شکست از 233**؛ instrumentation اندروید موفق؛ build/publish/startup و installer ویندوز موفق؛ HostLifecycle موفق.
3. Real local inference: [run 42851148119](https://github.com/artinrahimii/project/actions/runs/42851148119) موفق؛ مدل واقعی از مسیر llama.cpp روی runner ویندوز GitHub بارگذاری و inference انجام شد؛ لغو generation نیز مشاهده و تأیید شد.
4. Rendezvous container: [run 42851148040](https://github.com/artinrahimii/project/actions/runs/42851148040) موفق.

### نسخه و checksum مدل آزموده‌شده

- llama.cpp: نسخهٔ CPU build 11443؛ SHA-256 باینری `4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968`.
- آرشیو llama.cpp: SHA-256 `98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6`.
- مدل: `SmolLM2-135M-Instruct.Q4_K_M.gguf`، revision `a6b7826ed969c3926a98359e39912af137b464cb)، اندازه `105454144` بایت، SHA-256 `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684`.
- شناسهٔ runtime آزمون: `smollm2-ci`.

### جمع‌بندی پذیرش

تمام شش معیار UPD-S1-023 با ترکیب سورس، آزمون Android، آزمون admission میزبان، آزمون‌های catalog و اجرای inference واقعی تأیید شدند؛ وضعیت این وظیفه از `in_progress` به `done` تغییر یافت.

این نتیجه صرفاً دربارهٔ انتخاب مدل، سقف خروجی و inference واقعی backend روی runner ویندوز است. آزمون فیزیکی Android-to-installed-Windows P2P، نصب تمیز فیزیکی و ارتقای درجا انجام نشده‌اند و به‌عنوان معیارهای مستقل UPD-S1-020/021 باقی می‌مانند.

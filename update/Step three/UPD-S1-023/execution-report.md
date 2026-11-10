# گزارش اجرای UPD-S1-023 — سیاست انتخاب خودکار مدل و سقف خروجی

## وضعیت
**در حال اجرا (in_progress)**. کد و آزمون‌ها اصلاح شده‌اند، اما CI دقیق همین commit هنوز اجرا نشده و هیچ ادعای تکمیل نهایی وجود ندارد.

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

## آزمون‌ها
- آزمون‌های admission و session capability افزوده/گسترش داده شدند.
- دو assertion نامعتبر Android اصلاح شدند.
- **اجرای دقیق-head پس از تغییرات: در انتظار CI.**

## محدودیت‌های این تلاش
- آزمون فیزیکی Android به Windows و inference واقعی از مسیر P2P در این تلاش انجام نشده است.
- نصب تمیز و ارتقای درجا انجام نشده‌اند.
- تا سبزشدن CI و تأیید تمام معیارهای پذیرش، UPD-S1-023 تکمیل‌شده محسوب نمی‌شود.

# شواهد فنی UPD-S1-023 — مرز مدل و سقف خروجی

## منبع انتخاب خودکار Android
`src/Client.Android/app/src/main/java/com/whitep/client/requests/AutomaticModelSelectionPolicy.kt`:
- فقط candidateهای READY را بررسی می‌کند.
- مدل را از capability موجود در HostStatus زنده می‌گیرد؛ advertised-only کافی نیست.
- شناسه، نام و سقف context/output را اعتبارسنجی می‌کند.
- انتخاب deterministic است: context بزرگ‌تر، output headroom بزرگ‌تر، سپس model ID و host ID با ترتیب پایدار.
- سقف را به کمترین حد مدل، context، محصول 4096 و پروتکل 131072 محدود می‌کند.
- نبود گزینهٔ معتبر را به failure typed تبدیل می‌کند.

## منبع واقعی capability در Windows
- `src/Host.Infrastructure/LlamaCppServerBackend.cs` شناسهٔ مدل را با پاسخ `/v1/models` تطبیق می‌دهد و context را از `/props` می‌خواند.
- `src/Host.Windows/Models/Catalog/InstalledCatalogModelVerifier.cs` شناسهٔ catalog، backend، مسیر و SHA-256 فایل نصب‌شده را بررسی می‌کند.
- `MainWindow.xaml.cs` READY را تنها پس از تأیید مدل نصب‌شده و capability فعال منتشر می‌کند.

## شکاف اصلاح‌شده
در commit مبنا `c65ec1ae2ffc9020672971dafdecbd3a4558d87e`:
- admission context سقف خروجی/context مدل را حمل نمی‌کرد.
- handler ویندوز فقط status جاری و مقدار TextBox را بررسی می‌کرد.
- `GenerateAsync(InferenceRequest)` شناسهٔ مدل را پس از اعتبارسنجی پروتکل دور می‌انداخت.
این وضعیت امکان عبور سقف درخواستی بالاتر از capability واقعی را تا backend فراهم می‌کرد، هرچند سقف عمومی پروتکل همچنان اعمال می‌شد.

## دفاع چندلایهٔ افزوده‌شده
1. handler، snapshot تازهٔ backend را پیش از admission می‌گیرد.
2. admission مقادیر نامعتبر و سقف‌های بیش‌ازحد را fail-closed رد می‌کند.
3. session زیر operation gate مدل و capability را دوباره بررسی می‌کند.
4. سقف محصول .NET برابر 4096 تعریف شد تا با Android هم‌راستا باشد.

## آزمون‌های مرتبط
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AutomaticModelSelectionPolicyTest.kt`

run مبنا `42835719820` دو assertion نادرست داشت. اصلاح آن‌ها در این commit انجام شد؛ آزمون دقیق همین commit هنوز اجرا نشده است. آزمون فیزیکی یا Android-to-Windows در این سند ادعا نشده است.


## نتیجهٔ CI تلاش A001
- Quality gate exact-head: موفق، run 42848534130.
- Rendezvous container: موفق، run 42848534328.
- Product build: ناموفق، run 42848534275؛ ۷ compile errors در HostLifecycle.
- Real inference: ناموفق پیش از اجرای inference، run 42848534114.
- A002 شش سازندهٔ باقی‌مانده و نام مبهم HostStatus را اصلاح کرده است؛ این اصلاحات تا اجرای CI بعدی تأییدنشده‌اند.


## تلاش A002: failure در زمان اجرا
- HostLifecycle compile با اصلاح constructorها موفق شد.
- Harness به دلیل اینکه `SessionTestBackend` در `tests/HostLifecycle.Tests/Program.cs` capability زنده را گزارش نمی‌کرد، fail-closed رد شد.
- real-inference workflow به دلیل assertion مربوط به انتشار جایگزین QUIC/UDP برای همان peer متوقف شد؛ مدل محلی هنوز در آن run اجرا نشده است.

## تلاش A003
- `SessionTestBackend` در `Program.cs` اکنون `IInferenceModelCapabilityProvider` است.
- Model ID برابر `fixture-model` و context/output برابر 8192/4096 گزارش می‌شوند.
- CI دقیق این اصلاح هنوز اجرا نشده است.

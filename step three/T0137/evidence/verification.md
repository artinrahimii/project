# T0137 — شواهد و اعتبارسنجی

## نسخه و گراف مبنا
- Branch: `main`
- Commit پایهٔ اصلاح: `80fa3a12c7387a1bfb5c8e39e9f1463cfd58b425` — `V 3.0.479`
- Tree: `49f27fa8e56243f89f0f25f8837650ee04b9ce83`
- Graph Blob: `8057a52cebba439bd7b7046fc74a7d54596f99ab`
- اندازهٔ محتوای فایل: **2,323,501 بایت UTF-8**؛ طول رشته: **2,201,506 نویسه**.
- Git Blob SHA-1 روی بایت‌های اصلی و شامل حصار Markdown محاسبه شد و با `8057a52cebba439bd7b7046fc74a7d54596f99ab` تطبیق داشت؛ JSON کامل 229 وظیفه و 418 یال فعال را parse کرد.

## تلاش A001 — شکست کامپایل روی `V 3.0.479`
- HostLifecycle job `123011004076`: شکست پیش از assertions؛ نبود `using Google.Protobuf` باعث `CS1061` برای `InferenceRequest.ToByteArray()` در خطوط 22 و 27 آزمون تازه شد.
- فایل `PeerResponseCorrelationValidator.cs` در پروژهٔ HostLifecycle.Tests که فهرست `Compile Include` صریح دارد وارد نشده بود؛ نتیجه `CS0103` در آزمون و `PeerAnnouncementHostLifecycle` بود.
- [HostLifecycle failure log](https://github.com/artinrahimii/project/actions/runs/40743489739/job/123011004076)
- [Real local inference failure log](https://github.com/artinrahimii/project/actions/runs/40743490197/job/123011003755)
- Windows WPF build job شکست خورد؛ Android APK job `123011004204` موفق شد؛ quality gate job `123011003650` موفق شد. موفقیت‌های مستقل جایگزین تست‌های C# اجرا‌نشده نیست.

## تلاش A002 — اصلاح wiring پروژه
- `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs`: وارد کردن `Google.Protobuf` برای extensionهای Protobuf.
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`: لینک صریح `src/Host.Windows/Networking/PeerResponseCorrelationValidator.cs` تا فایل در کامپایل executable test runner حضور داشته باشد.
- نتیجهٔ CI روی `V 3.0.480`: در زمان ثبت، هنوز pending؛ PASS ادعا نمی‌شود.

## محدوده و ریسک
- Replay fence در حافظه نگه‌داری می‌شود و در restart حذف می‌شود؛ replay میان restartها با این cache به‌تنهایی دفع نمی‌شود.
- آزمون loopback جایگزین `T0128` و بررسی reachability مستقل نیست.

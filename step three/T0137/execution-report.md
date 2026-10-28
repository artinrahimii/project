# T0137 — گزارش اجرای وظیفه

- **وضعیت فعلی:** `in_progress`؛ تلاش نخست با خطای کامپایل شکست خورد؛ اصلاح منبع/پروژه در `T0137-A002` ثبت شد و CI دقیق نسخهٔ جدید هنوز pending است.
- **مبنای اصلاح:** `main` / `80fa3a12c7387a1bfb5c8e39e9f1463cfd58b425` (`V 3.0.479`); نسخهٔ اصلاحی `V 3.0.480`.
- **خطای دقیق مشاهده‌شده:** HostLifecycle job `123011004076` با `CS1061` در `PeerMessageSecurityAcceptanceTests.cs` (متد extension مربوط به `ToByteArray` به‌علت نبودن `using Google.Protobuf`) و `CS0103` برای `PeerResponseCorrelationValidator` به‌علت نبودن این فایل در فهرست Compile صریح پروژهٔ آزمون شکست خورد؛ assertions اجرا نشده‌اند. [لاگ](https://github.com/artinrahimii/project/actions/runs/40743489739/job/123011004076).
- **اصلاح A002:** افزودن `using Google.Protobuf;` به آزمون امنیتی و افزودن لینک صریح `PeerResponseCorrelationValidator.cs` در `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- **نتایج V 3.0.479:** Android APK موفق؛ Step Three quality gate موفق؛ HostLifecycle، Windows WPF و real local inference شکست خوردند. شکست‌ها به‌عنوان نتیجهٔ رفتار امنیتی تفسیر نشده‌اند، چون کامپایل پیش از اجرای assertions متوقف شد.
- **پیاده‌سازی:** کنترل replay در حافظه با کلید PeerId احرازشده + request_id + attempt_id؛ parser قبل از dispatch؛ اعتبارسنجی اندازه/قالب/هم‌بستگی پاسخ قبل از ارسال؛ آزمون‌های tampering/replay/forged response و loopback پروتکلی.
- **محدوده:** T0128 (آزمون end-to-end روی دو شبکهٔ مستقل) همچنان `not_started` و NOT RUN است؛ این تغییر آن را تکمیل نمی‌کند.
- **ادامه:** بررسی دقیق CI مربوط به `V 3.0.480`; فقط بعد از PASS واقعی HostLifecycle، build ویندوز، APK اندروید و real local inference می‌توان این وظیفه را تکمیل‌شده دانست.

# T0179 — لاگ ساختاریافته با allowlist فیلدهای مجاز

<badge>done</badge>

## خروجی نهایی

- پیاده‌سازی logging محدود به allowlist در میزبان Windows و کلاینت Android انجام شد.
- هر دو API ثبت رویداد فقط کدهای ثابت رویداد/نتیجه و مدت‌زمان غیرمنفی می‌پذیرند؛ سقف هر فرایند ۸۰ رکورد است.
- مسیرهای transport به logger متصل شدند و اطلاعات آزاد مانند prompt، capsule، کلید، شناسه‌های peer/request و متن exception به logger داده نمی‌شوند.
- fixture تشخیصی interoperability از متن template/category/exception پاک‌سازی شد.
- آزمون‌های Python، آزمون واحد Android و پذیرش EventSource در .NET وجود دارند.
- مستند امنیت دامنهٔ کنترل و محدودیت‌های خارج از برنامه را بیان می‌کند.

## سوابق اجرا

| تلاش | commit | نتیجه |
|---|---|---|
| A001 | `V 3.0.816` — `583dccc5decebccb78d6599c082e12e516bb80c9` | failed؛ Android ktlint |
| A002 | `V 3.0.817` — `4bfb38ab6238b3c4b8168352715704241c739b82` | failed؛ Android ktlint |
| A003 | `V 3.0.818` — `4b686da26a6d61e73adc8daa13d1d0ac742f9e11` | failed؛ Android ktlint |
| A004 | `V 3.0.819` — `998c84ec83e3b8706b20e664f7d8570e6cac14b5` | failed؛ Kotlin compile |
| A005 | `V 3.0.820` — `0b023a4201b28feb9748c17ec4698bb5528b7d52` | succeeded |

چهار تلاش نخست در تاریخچه حفظ شده‌اند؛ A005 پنجمین تلاش است و تمام معیارهای پذیرش را گذرانده است.

## شواهد CI دقیق A005

- [Step Three quality gate — موفق](https://github.com/artinrahimii/project/actions/runs/41448964242): ۳۳۳ آزمون Python، compileall و graph_guard؛ ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطای گراف و یک هشدار metadata قدیمی T0001.
- [White-P product build — موفق](https://github.com/artinrahimii/project/actions/runs/41448964240): هر سه job اندروید، Windows WPF و HostLifecycle موفق شدند.
- Android: `ktlintCheck`، کامپایل Kotlin، ساخت APK، `testDebugUnitTest` و بررسی فایل‌های تولیدشدهٔ Protobuf موفق شدند.
- artifact واقعی APK: `whitep-android-debug-apk`، شناسهٔ `13144066959`، اندازهٔ ۱۹٬۵۲۹٬۷۵۶ بایت.
- artifact گزارش آزمون Android: `whitep-android-unit-test-reports`، شناسهٔ `13143233051`، اندازهٔ ۵۱٬۰۳۴ بایت.
- fixture میزبان در HostLifecycle صریحاً تأیید کرد که EventSource فقط سه فیلد عددی مجاز را منتشر می‌کند.
- [Rendezvous container build/smoke — موفق](https://github.com/artinrahimii/project/actions/runs/41448964125).

## حدود تضمین امنیتی

این کنترل، logهای تحت کنترل logger برنامه را محدود می‌کند؛ نبود داده در OS logs، pagefile/swap، crash dump، فرایندهای خارجی یا logهای داخلی کتابخانه‌های ثالث را اثبات نمی‌کند.

## وابستگی بعدی

T0180 پس از T0179 در ترتیب اجرایی قرار دارد؛ اما دو یال ورودی `T0179 → T0180` و `T0177 → T0180` هنوز `unreviewed` هستند. پیش از شروع T0180 باید هر دو با بررسی قراردادها و کد واقعی اعتبارسنجی شوند.

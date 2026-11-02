# T0179 — لاگ ساختاریافته با allowlist فیلدهای مجاز

- **وضعیت:** `in_progress`؛ پیاده‌سازی .NET و Android انجام شده، اما build اندروید در A004 پس از عبور از lint با خطای نام enumها شکست خورد.
- **A001:** `V 3.0.816` — `583dccc5decebccb78d6599c082e12e516bb80c9`.
- **A002:** `V 3.0.817` — `4bfb38ab6238b3c4b8168352715704241c739b82`.
- **A003:** `V 3.0.818` — `4b686da26a6d61e73adc8daa13d1d0ac742f9e11`.
- **A004:** `V 3.0.819` — `998c84ec83e3b8706b20e664f7d8570e6cac14b5`.
- **A005:** اصلاح فراخوانی enumها با نام واقعی Kotlin و به‌روزرسانی آزمون منبع؛ commit در حال ثبت با پیام `V 3.0.820`.

## نتایج CI تا A004

- Step Three quality gate در A001 تا A004 موفق: هر بار ۳۳۳ آزمون Python و compileall؛ graph_guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را با صفر خطا و یک هشدار metadata قدیمی T0001 گزارش کرد.
- HostLifecycle/EventSource: موفق؛ آزمون EventSource محدودیت سه فیلد عددی را تأیید کرد.
- Windows WPF در A001، A003 و A004: format، build، Protobuf، smoke و کنترل‌های process/policy/capability/DPAPI identity موفق.
- Rendezvous container در A001 تا A004: build و smoke موفق.
- Real local inference integration در A001 موفق.
- Android ktlint در A004 موفق شد و از خطاهای قالب‌بندی قبلی عبور کرد.
- اما `compileDebugKotlin` در A004 شکست خورد: transport از نام‌های PascalCase مانند `InferenceSubmission` و `Started` استفاده کرده بود، در حالی که enumهای Kotlin با `INFERENCE_SUBMISSION`، `STARTED`، `REJECTED`، `SUCCEEDED` و `CANCELLED` تعریف شده‌اند. APK تولید نشد و JVM tests اجرا نشدند.

## اصلاح A005

- فراخوانی‌های transport به constantهای واقعی uppercase Kotlin تغییر کردند.
- آزمون `tests/test_network_payload_logging.py` نیز برای همین نام‌های واقعی و کد رویداد Android هماهنگ شد.
- schema لاگ، مقادیر عددی، رفتار و سقف ۸۰ رکورد تغییر نکرده است.
- CI مربوط به A005 هنوز اجرا نشده و نتیجه‌ای برای آن ادعا نمی‌شود.

## وابستگی‌ها و ادامه

- یال‌های `T0177 → T0179` و `T0178 → T0179` با حفظ نوع `blocks` به‌صورت `sequencing_only` اعتبارسنجی شده‌اند؛ هیچ یالی حذف و ترتیب گراف عوض نشده است.
- تا پاس‌شدن `ktlintCheck`، `compileDebugKotlin`، `assembleDebug` و `testDebugUnitTest` در CI دقیق commit، وضعیت T0179 `in_progress` می‌ماند.
- این کنترل به‌تنهایی logهای سیستم‌عامل، crash dump، فرایندهای خارجی یا کتابخانه‌های ثالث را پوشش کامل نمی‌دهد.

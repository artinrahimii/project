# T0179 — لاگ ساختاریافته با allowlist فیلدهای مجاز

- **وضعیت:** `in_progress`؛ مسیرهای .NET و Android پیاده‌سازی شده‌اند، اما lint در سه تلاش پیاپی یک ایراد قالب‌بندی متفاوت در هر مرحله یافته است.
- **A001:** `V 3.0.816` — `583dccc5decebccb78d6599c082e12e516bb80c9`.
- **A002:** `V 3.0.817` — `4bfb38ab6238b3c4b8168352715704241c739b82`.
- **A003:** `V 3.0.818` — `4b686da26a6d61e73adc8daa13d1d0ac742f9e11`.
- **A004:** اصلاح یک فاصلهٔ خالی قبل از تابع محلی؛ در حال ثبت با پیام `V 3.0.819` و منتظر CI.

## نتیجهٔ CI در تلاش‌ها

**A001 — V 3.0.816**
- Step Three quality gate: ۳۳۳ آزمون Python و compileall موفق؛ graph_guard با ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطا و یک هشدار metadata قدیمی T0001.
- HostLifecycle و آزمون EventSource: موفق؛ سه فیلد عددی مجاز تأیید شد.
- Windows WPF: format، build، Protobuf، smoke و policy/identity موفق.
- Rendezvous container و Real local inference integration: موفق.
- Android `ktlintCheck` شکست خورد: خط ۳۵ طول بیش از ۱۲۰ و خط ۴۳ قالب‌بندی expression؛ APK/JVM tests اجرا نشدند.

**A002 — V 3.0.817**
- Step Three quality gate: ۳۳۳ آزمون Python، compileall و graph_guard موفق.
- HostLifecycle و Rendezvous container: موفق.
- Android `ktlintCheck` شکست خورد؛ ایرادهای قبلی Kotlin logger رفع شده بودند، اما پارامترهای constructor دو enum در خطوط ۷ و ۱۲ باید چندخطی می‌شدند؛ APK/JVM tests اجرا نشدند.

**A003 — V 3.0.818**
- Step Three quality gate: ۳۳۳ آزمون Python و graph_guard موفق.
- HostLifecycle، Windows WPF (از جمله DPAPI identity/trust) و Rendezvous container: موفق.
- Android `ktlintCheck` این بار فقط یک ایراد در `Libp2pInferenceRequestTransport.kt:132:9` گزارش کرد: قبل از declaration تابع محلی `reject` یک خط خالی لازم است.
- به علت lint failure، assembleDebug و testDebugUnitTest اجرا نشدند.

## اصلاح A004

- یک خط خالی پیش از declaration تابع محلی `reject` اضافه شد.
- هیچ رفتار اجرایی یا قرارداد داده عوض نشده است؛ schema همچنان فقط `schema`، `event_code`، `outcome_code` و `duration_ms` است.
- نتیجهٔ ktlint، APK و JVM unit tests برای A004 هنوز در دسترس نیست.

## وابستگی‌ها و ادامه

- یال‌های `T0177 → T0179` و `T0178 → T0179` با حفظ نوع `blocks` به‌صورت `sequencing_only` اعتبارسنجی شده‌اند؛ ترتیب گراف و `depends_on` حفظ شده است.
- تا موفقیت `ktlintCheck`، `assembleDebug` و `testDebugUnitTest` در CI دقیق commit، T0179 در وضعیت `in_progress` می‌ماند.
- کنترل موجود رفتار logهای سیستم‌عامل، crash dump، فرایندهای خارجی و کتابخانه‌های ثالث را تضمین یا پوشش کامل نمی‌دهد.

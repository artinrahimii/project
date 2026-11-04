# T0186 — رضایت آگاهانه و هشدار حریم خصوصی پیش از اعتماد به همتا

- **وضعیت:** در حال اجرا؛ کد رابط و متن رضایت پیاده‌سازی شده، اما پذیرش نهایی CI هنوز کامل نیست.
- **نسخهٔ مبنا:** main در `V 3.0.874`، commit `ea0edc3db0addf1f46c38663119b5ab33d8c3fe5`.
- **پیش‌نیازها:** T0185 و T0177 تکمیل‌شده و بررسی شده‌اند.
- **یال‌ها:** دو یال ورودی به `sequencing_only` / `validated` طبقه‌بندی شدند؛ `blocks` و ترتیب اصلی حفظ شد.
- **خروجی Android:** هشدار peer-specific قبل از ثبت اعتماد، مشاهده‌پذیری prompt و context capsule پس از رمزگشایی انتقال و عدم ارسال خودکار هنگام ذخیرهٔ trust را توضیح می‌دهد.
- **خروجی Windows:** پیش از ساخت/شروع listener میزبان همتا، صاحب دستگاه باید هشدار با پیش‌فرض No را بپذیرد؛ رد آن toggle را خاموش نگه می‌دارد و سرویس را ایجاد نمی‌کند.
- **CI نسخهٔ V 3.0.873:** Windows WPF build و HostLifecycle tests موفق؛ container و real local llama.cpp integration موفق؛ Android KtLint و quality tests شکست داشتند.
- **CI نسخهٔ V 3.0.874:** Python test suite با ۳۷۱ آزمون موفق شد؛ compileall موفق شد؛ container build/smoke موفق شد. graph guard به‌علت اختلاف `failed_attempt_count` با تعداد attemptهایی که `result=failed` دارند شکست خورد. هر دو تلاش T0186 با `result=partial` ثبت شده‌اند، پس شمار درست failed_attempt_count صفر است.
- **اصلاح گراف A003:** failed_attempt_count به صفر اصلاح شد و خطای validator با شواهد ثبت شد. آزمون graph guard روی commit بعدی باید آن را تأیید کند.
- **در انتظار:** Android product build، WPF و HostLifecycle jobهای V 3.0.874 هنگام ثبت این گزارش هنوز در حال اجرا بودند.
- **محدودیت:** متن رضایت، تضمین رفتار میزبان/کلاینت مقابل یا حذف log و payload از کنترل سیستم‌عامل و نرم‌افزار ثالث نیست.
- **ادامه:** ابتدا exact-head graph guard و jobهای باقی‌ماندهٔ محصول را تأیید کن؛ فقط پس از موفقیت همهٔ معیارهای مرتبط، T0186 را done کن.

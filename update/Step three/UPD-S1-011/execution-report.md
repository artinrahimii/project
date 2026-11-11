# گزارش اجرای UPD-S1-011 — تنظیمات عادی Android و رضایت حریم خصوصی

- مبنا: `40cdea13743b35487ca1b6365cb881b181a5ede1` — `V 3.1.200`
- تاریخ: 2026-11-11T14:06:20.090Z
- وضعیت تلاش: `in_progress`
- عملیات: `modify`

## یافته‌ها

1. `ConversationDetailScreen` به `RequestViewModel.sendRequest` وصل است و پیش از اولین ارسال، رضایت/هشدار حریم خصوصی قابل ردشدن را نشان می‌دهد.
2. `RequestViewModel` با `resolveAutomaticModel` مدل و `maxOutputTokens` را از قابلیت تأییدشدهٔ میزبان انتخاب می‌کند.
3. اعتماد همتا با تأیید صریح ذخیره می‌شود؛ گفت‌وگوی عادی به‌جای PeerId از شمارهٔ میزبان استفاده می‌کند.
4. شکاف احرازشده، نیاز به واردکردن دستی URL سرویس Rendezvous در صفحهٔ فرعی بود. مخزن endpoint عمومی تولیدی مستقر و تأییدشده‌ای ندارد؛ URL حدسی اضافه نشد.

## تغییرات

- ورودی build-time با Gradle property `whitepRendezvousServiceBaseUrl` یا متغیر محیطی `WHITEP_RENDEZVOUS_SERVICE_BASE_URL`.
- اعتبارسنجی URI مطلق HTTPS، بدون credentials، query یا fragment؛ مقدار نامعتبر build را متوقف می‌کند.
- تزریق در `BuildConfig.RENDEZVOUS_SERVICE_BASE_URL`؛ استفاده به‌عنوان مقدار پیش‌فرض اپ، در صورت نبود override ذخیره‌شده.
- URL سفارشی فقط در صفحهٔ `Advanced host settings` باقی می‌ماند؛ پاک‌کردن override به مقدار بسته‌بندی‌شده برمی‌گردد.
- endpoint عمومی ساختگی، relay inference یا تغییر قرارداد/رمزنگاری اضافه نشده است.

## آزمون‌ها و محدودیت‌ها

- آزمون قراردادی جدید: `tests/test_update_s1_011_rendezvous_configuration.py`.
- کیفیت، lint، Android JVM، build و workflowها هنوز باید روی commit حاصل اجرا شوند؛ اجرای محلی در این محیط انجام نشده است.
- endpoint عمومی، دستگاه Android فیزیکی و میزبان Windows واقعی در این تلاش در دسترس نبودند.

## معیارهای پذیرش فعلی

- فیلدهای مدل/توکن/PeerId/IP/Port از مسیر عادی حذف شده‌اند؛ CI در انتظار.
- ورود دستی URL برای build پیکربندی‌شده حذف می‌شود؛ استقرار endpoint واقعی و build انتشار هنوز لازم است.
- انتخاب مدل deterministic و رد هویت نامعتبر از پیاده‌سازی موجود استفاده می‌کنند؛ CI exact-head در انتظار.
- رضایت پیش از درخواست اول و امکان لغو در UI موجود است؛ آزمون قراردادی اضافه شده، CI در انتظار.
- metadata هویتی فقط در صفحهٔ پیشرفتهٔ اعتماد/عیب‌یابی دیده می‌شود، نه چت معمولی.

## ادامه

تا استقرار و تعیین endpoint عمومی HTTPS و آزمون build منتشرشده با آن، این وظیفه تکمیل‌شده نیست. مقدار پیش‌فرض عمداً خالی است تا اتصال به سرویس ساختگی یا ناامن اتفاق نیفتد.


## بازاعتبارسنجی CI روی commit دقیق V 3.1.203

- آزمون قراردادی URL/HTTPS/تنظیمات پیشرفته و رضایت Android در quality gate دقیق commit `ea374677019b50b07677a60c1e6c0df3a970c89b` اجرا و موفق شد؛ quality run `43038776230` شامل ۵۰۲ آزمون و هر دو graph guard بود.
- Product build run `43038776527` در job اندروید، lint، JVM، debug/release build و Keystore instrumentation را موفق اجرا کرد؛ Windows WPF و HostLifecycle نیز موفق بودند.
- Rendezvous container workflow run `43038776377` build و smoke محلی را موفق کرد.
- این نتایج endpoint عمومی را مستقر یا قابل‌دسترسی اثبات نمی‌کنند. `docs/deployment/rendezvous-service.md` صریحاً دامنهٔ `https://rendezvous.example.net/` را placeholder می‌داند و استقرار TLS/ingress و آزمون دسترسی از شبکهٔ دیگر را لازم می‌شمارد. بنابراین UPD-S1-011 همچنان `in_progress` است تا endpoint واقعی مستقر و در build انتشار پیکربندی شود.


## تلاش A002 — مانع واقعی استقرار عمومی

- commit بررسی‌شده: `1b91af90fe42e4a33237bf4065d8c98dd25bcb42` (`V 3.1.204`).
- سند استقرار، `https://rendezvous.example.net/` را placeholder می‌داند؛ هیچ endpoint عمومی تأییدشده‌ای در پیکربندی محصول وجود ندارد.
- workflow کانتینر فقط build و smoke روی loopback انجام می‌دهد (run `43044517854` موفق)؛ این آزمون deploy عمومی یا دسترسی از شبکهٔ مستقل نیست.
- استقرار نیازمند host، DNS عمومی، TLS ingress/reverse proxy و آزمون peer واقعی روی شبکهٔ دیگر است. این منابع در محیط متصل فعلی فراهم نیستند.
- URL حدسی، default ساختگی یا relay inference اضافه نشد.
- وضعیت به `blocked` تغییر یافت تا زمان فراهم‌شدن endpoint و زیرساخت عمومی؛ جزئیات و شرط رفع مانع در `evidence/public-rendezvous-deployment-blocker-A002.md` ثبت شده است.


## تغییر برنامه‌ریزی — انتقال پذیرش عملیاتی به مرحلهٔ پایانی

- وضعیت فعلی این وظیفه در گراف از `blocked` به `deferred_final` تغییر کرده است؛ تلاش A002 و شواهد قبلی حذف نشده‌اند.
- این تغییر به معنی `done` نیست و درصد پیشرفت را افزایش نمی‌دهد.
- دامنهٔ باقی‌مانده فقط استقرار endpoint عمومی HTTPS واقعی، تنظیم build انتشار و آزمون کشف میزبان/اتصال همتا از شبکهٔ مستقل است.
- کار خودکار UPD-S1-019 می‌تواند بر اساس قراردادها و کد فعلی ادامه یابد؛ این اجازه، اثبات دسترسی عمومی یا تکمیل این وظیفه نیست.
- شرط شروع پذیرش نهایی: استقرار واقعی DNS/TLS، پیکربندی build انتشار و ثبت آزمون مستقل شبکه با PeerId واقعی و شواهد redacted.

# T0183-A002 — شواهد پذیرش exact-head

## snapshot و اعتبار فایل گراف

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit آزموده‌شده: `9cf5e9b858f975b605c952ead3c4010fa78a9d11` — `V 3.0.853`
- tree منبع: `3518804be7a98ddac216830980e181ab9512945c`
- Blob گراف: `b69a152faf7e1676a998cfd11a776997ef3b7c45`
- گراف از Git Blob API دریافت شد؛ اندازه ۳٬۳۵۱٬۱۵۹ بایت UTF-8، طول متن ۳٬۲۲۴٬۱۸۶ نویسه. SHA-1 با قالب Git `blob <length>\0<content>` دقیقاً با Blob در tree برابر شد و JSON کامل parse شد: ۲۳۰ node و ۴۲۲ edge.

## CI و آزمون

- Product run: https://github.com/artinrahimii/project/actions/runs/41678108221 — موفق؛ Windows WPF، Android و HostLifecycle همگی موفق.
- Android job `126064102869`: KtLint، `assembleDebug`، `testDebugUnitTest`، Protobuf Java/Kotlin و `connectedDebugAndroidTest` موفق.
- log instrumentation صریحاً `Starting 5 tests` و `Finished 5 tests` روی emulator API 36 را ثبت کرد.
- Instrumentation artifact `13252670975`؛ JVM report `13252631231`؛ APK `13252915718`.
- Step Three quality run: https://github.com/artinrahimii/project/actions/runs/41678108248 — موفق.
- Rendezvous container run: https://github.com/artinrahimii/project/actions/runs/41678108226 — موفق.

## خطاها و اصلاحات حفظ‌شده

- V848: هشت تخلف KtLint در main source.
- V849: دو خطای import در test source.
- V850: دو خطای قالب‌بندی Builder.
- V851: instrumentation به علت cwd اشتباه به Gradle نرسید؛ خطای مسیر build بود، نه شکست assert.
- V852: guard کیفیت هنوز فرمان قدیمی workflow را انتظار داشت؛ assertion با مسیر جدید همگام شد.
- V853: پنج سناریوی instrumentation واقعاً اجرا شدند؛ quality، product و container نیز موفق شدند.

## مرز ادعا

emulator صحت provider را در همان محیط تأیید می‌کند؛ TEE/StrongBox دستگاه فیزیکی را ثابت نمی‌کند. رمزگذاری پایگاه Room در T0184 مستقل است.

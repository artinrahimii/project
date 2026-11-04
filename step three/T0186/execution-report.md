# T0186 — رضایت آگاهانه و هشدار حریم خصوصی پیش از اعتماد به همتا

<badge>تکمیل‌شده</badge>

- **وضعیت:** تکمیل‌شده بر اساس معیارهای مستقیم پذیرش و شواهد CI روی `V 3.0.875`، commit `14d232be48af41a71dee4acf4bebc47467280895`.
- **پیش‌نیازها:** T0185 و T0177 تکمیل‌شده و بررسی شدند.
- **بازبینی یال‌ها:** T0185 → T0186 و T0177 → T0186 به `sequencing_only` / `validated` تغییر طبقه‌بندی یافتند؛ نوع `blocks` و ترتیب اصلی حفظ شد.
- **Android:** هشدار پیش از ذخیرهٔ اعتماد، PeerId دقیق را نشان می‌دهد؛ صریحاً افشا می‌کند که میزبان پس از رمزگشایی انتقال prompt و context capsule را می‌خواند؛ ذخیرهٔ trust به‌خودی‌خود payload ارسال نمی‌کند و ارسال فقط با اقدام جداگانهٔ کاربر است.
- **Windows:** پیش از ساخت یا شروع `PeerAnnouncementHostLifecycle`، رضایت صاحب دستگاه با پیش‌فرض No اخذ می‌شود. رد هشدار toggle را خاموش نگه می‌دارد و listener/lease شروع نمی‌شود؛ تأیید PeerId همچنان مستقل است.
- **CI موفق:**
  - Python quality suite: ۳۷۱ آزمون موفق، شامل تست‌های source-contract رضایت.
  - Python compileall: موفق.
  - Graph guard: ۲۳۰ وظیفه، ۴۲۲ یال، صفر خطا؛ یک هشدار قدیمی T0001 به‌عنوان metadata حفظ شد.
  - Windows: C# formatting، WPF build، generated Protobuf validation و startup smoke موفق.
  - HostLifecycle unit/integration tests: موفق.
  - Android: KtLint، ساخت debug APK، verifier مجوزهای APK ادغام‌شده و Android JVM unit tests موفق.
  - Rendezvous container build/smoke: موفق.
- **محدودیت مهم:** در زمان ثبت این وضعیت، کل product workflow هنوز یک Android Keystore instrumentation job جداگانه را اجرا می‌کرد؛ آن آزمون مربوط به کنترل‌های T0183/T0184 است، نه رضایت T0186. این گزارش به‌معنای تکمیل کل workflow یا کل محصول نیست.
- **فایل‌ها:** کد disclosure در Shared.Security، جریان Windows و Android، آزمون‌های Python و HostLifecycle، docs/security.md و شواهد این پوشه.
- **وظیفهٔ منطقی بعدی:** T0187 — آزمون جست‌وجوی log و فایل‌های موقت برای نبود prompt، capsule و اسرار. یال خروجی T0186 → T0187 هنوز `unreviewed` است و پیش از اجرای T0187 باید با شواهد بررسی شود.

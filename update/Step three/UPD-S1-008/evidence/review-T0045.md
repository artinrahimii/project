# بازبینی محتوایی T0045 — ذخیره‌سازی محلی نسخه‌دار و اعتبارسنجی

## خط مبنا و تصمیم
- commit منبع کد بررسی‌شده: `c933559b3be973f1cdf21e094b786c84742ca7ab` — `V 3.1.74`.
- وضعیت پایهٔ T0045 در گراف Step Three: `done`.
- نگاشت V1.10: `UPD-S1-012`.
- تصمیم: `re_review_required`؛ وضعیت پایهٔ `done` و تاریخچهٔ قبلی دست‌نخورده می‌مانند.

## پیاده‌سازی مشاهده‌شده
- `src/Host.Infrastructure/LocalHostSettingsStore.cs` یک store واقعی برای تنظیمات محلی کاربر است؛ schema فعلی ۲ است.
- بارگذاری schema قدیمی ۱ را پشتیبانی می‌کند و تنظیمات را با مقادیر پیش‌فرض مستند برای نیاز حافظه و فضای دیسک به schema فعلی مهاجرت می‌دهد.
- URL endpoint به HTTP(S) معتبر loopback محدود است؛ credential، query و fragment پذیرفته نمی‌شود. شناسهٔ مدل، timeout و مقادیر RAM/disk نیز اعتبارسنجی می‌شوند.
- فایل نامعتبر یا schema پشتیبانی‌نشده بی‌صدا بازنویسی نمی‌شود. ذخیره از فایل موقت هم‌مسیر، flush و جایگزینی اتمیک استفاده می‌کند.
- WPF در سازندهٔ `MainWindow` تنظیمات را بارگذاری می‌کند؛ خطای داده/دسترسی را به مسیر امن می‌برد و جزئیات exception یا مسیر محلی را نمایش نمی‌دهد.
- Blobهای بررسی‌شده در commit مبنا: `LocalHostSettingsStore.cs` = `5d83f1af85f4545b38ed5a61f6f4ffb53ccb4f12`؛ `MainWindow.xaml.cs` = `e2b2c3285f48fd8883b1781f5d98590eb6b77184`.

## آزمون‌ها و شواهد
- `tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs` پنج گروه آزمون اجرایی دارد: round-trip، مهاجرت schema v1، رد شناسهٔ انتخاب نامعتبر، رد نیاز ظرفیت نامعتبر، و رد تنظیمات persistشدهٔ نامعتبر بدون بازنویسی فایل.
- `tests/HostLifecycle.Tests/Program.cs` آزمون store را در harness اصلی اجرا می‌کند.
- Blob آزمون‌ها: فایل اختصاصی `LocalHostSettingsPersistenceTests.cs` = `6626a44b79af55e8142315317d3b93cbc6d73f79`؛ harness = `e63132c55538e289e5f49840943e35349e758052`.
- Product CI منطبق با همین Blobهای کد، run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793)، با نتیجهٔ کامل success برای Windows WPF، Android APK و HostLifecycle به پایان رسید. آزمون C# شامل persistence/migration در job میزبان اجرا می‌شود.
- Quality Gate دقیق HEAD مبنای reconciliation، run [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109)، موفق شد.
- Product run دقیق V3.1.74، [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134)، هنگام آخرین مشاهده هنوز `in_progress` بود و موفقیت آن در این سند ادعا نمی‌شود.

## وابستگی‌ها
- `T0044 → T0045`: `sequencing_only/validated`؛ store تنظیمات از API انتقال HostLifecycle استفاده نمی‌کند.
- `T0040 → T0045`: `sequencing_only/validated`؛ store schema و URL را مستقل از parser/validator پروتکل بررسی می‌کند.
- یال‌های اصلی و ترتیب محافظه‌کارانه حفظ شده‌اند.

## چرا بازبینی مجدد لازم است؟
`UPD-S1-012` هنوز `not_started` است و رفتار مشارکت اختیاری، رضایت مالک، خاموش/روشن شدن میزبان و توقف فوری پذیرش را به UI و تنظیمات میزبان متصل خواهد کرد. پس از آن باید پیش‌فرض opt-in، persistence و migration تنظیمات روی نسخهٔ جدید دوباره آزموده شوند؛ بنابراین تصمیم نهایی `unchanged` هنوز موجه نیست.

## خارج از محدودهٔ شواهد
این CI اثبات نصب فیزیکی میزبان، بارگذاری و اجرای یک مدل واقعی، ارتباط P2P با Android، clean install یا in-place upgrade نیست. هیچ snapshot ارتقای دستگاهی در این بازبینی تهیه نشده است.

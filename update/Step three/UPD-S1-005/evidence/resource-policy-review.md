# شواهد بازبینی و تأیید UPD-S1-005

مبنای اجرای دقیق: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` — `V 3.1.38`.

## رضایت و lifecycle

- `NetworkParticipationToggle` در startup خاموش است و فعال‌سازی نیازمند اقدام آگاهانهٔ مالک است.
- پیش از hosting، backend باید Ready باشد، ظرفیت معتبر باشد، privacy disclosure به کاربر نمایش داده شود (پاسخ پیش‌فرض No) و PeerId مورداعتماد صریحاً تأیید شود.
- `PeerAnnouncementHostLifecycle.StopAsync` مشارکت را غیرفعال و listener/transport را متوقف و منابع را آزاد می‌کند.
- آزمون‌های HostLifecycle حالت خاموش اولیه و توقف listener را بررسی کردند.

## ظرفیت قابل سنجش

- `WindowsHostHardwareDiscovery` RAM آزاد فیزیکی و فضای آزاد درایو سیستم را اندازه می‌گیرد.
- `HostReadinessEvaluator` مقادیر زنده را با محدودیت‌های پایدار مالک مقایسه می‌کند؛ دادهٔ Unknown یا ناکافی fail-closed است.
- `HostInferenceSession` پیش از inference readiness را دوباره بررسی می‌کند.
- `HostRequestAdmissionPolicy` مشارکت خاموش، میزبان non-ready، مدل ناسازگار، ظرفیت نامعلوم/ناکافی و deadline نامعتبر را رد می‌کند.
- `HostRequestCoordinator` با یک slot و admission غیرصفی، فقط یک inference را می‌پذیرد؛ آزمون رقابت ۶۴ درخواست، یک مورد پذیرفته و ۶۳ مورد Busy را ثبت کرد.
- GPU inventory-only باقی مانده است؛ سنجش/سقف CPU/GPU/VRAM/idle/AC در این نسخه ادعا نشده است.

## تنظیمات پایدار و migration

- `LocalHostSettingsStore` schema 2 را ذخیره می‌کند و حدهای RAM/disk را validate می‌کند.
- schema v1 با defaultهای ۴/۱ GiB سازگار است و در اولین save به schema 2 منتقل می‌شود.
- `LocalHostSettingsPersistenceTests` round-trip، حفظ سایر preferenceها، migration و رد مقدارهای صفر، NaN، Infinity و بزرگ‌تر از سقف را اجرا می‌کند.
- `tests/test_host_resource_settings.py` سیم‌کشی WPF، مقادیر persistشده و شروط fail-closed را بررسی می‌کند.

## CI و مدل واقعی محلی

- Quality gate: [42459356154](https://github.com/artinrahimii/project/actions/runs/42459356154) — موفق.
- Product build: [42459356153](https://github.com/artinrahimii/project/actions/runs/42459356153) — jobهای Android، Windows و HostLifecycle موفق.
- Real local inference: [42459356156](https://github.com/artinrahimii/project/actions/runs/42459356156) — اجرای llama.cpp واقعی و مدل GGUF checksumدار موفق؛ cancellation مشاهده شد و فرایند متعلق به آزمون به‌درستی متوقف شد.
- Rendezvous: [42459356182](https://github.com/artinrahimii/project/actions/runs/42459356182) — موفق.

## مرز شواهد

runner ویندوز CI و runner اندروید CI جایگزین گوشی فیزیکی و میزبان Windows هدف کاربر نیستند. Android-to-Windows واقعی، clean install محصول در محیط هدف و in-place upgrade در این تلاش اجرا نشده‌اند. هیچ نتیجه‌ای برای این آزمون‌های مستقل ثبت نمی‌شود.

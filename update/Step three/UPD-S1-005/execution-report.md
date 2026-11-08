# گزارش اجرای UPD-S1-005 — سیاست رضایت و ظرفیت میزبان

- مبنای اجرا: d027e72f3968ef7c8c826f889042eb67a8533fa4 (V 3.1.36)
- پیش‌نیاز UPD-S1-001: done
- وضعیت گره: in_progress؛ CI تغییرات جدید pending است.

## ممیزی
مشارکت شبکه در WPF به‌صورت پیش‌فرض خاموش است. فعال‌سازی نیازمند اقدام آگاهانه، backend آماده، capacity معتبر، privacy notice با پاسخ پیش‌فرض No و تأیید هویت PeerId مشخص است. lifecycle هنگام توقف admission را غیرفعال می‌کند، renewal را متوقف می‌کند، transport را می‌بندد و handlerها را drain می‌کند.
WindowsHostHardwareDiscovery مقدار RAM آزاد فیزیکی و فضای آزاد درایو سیستم را اندازه می‌گیرد. HostReadinessEvaluator آن‌ها را با آستانه‌های مثبت مقایسه می‌کند و Unknown/Insufficient را fail-closed نگه می‌دارد. HostInferenceSession پیش از هر inference readiness را دوباره بررسی می‌کند. HostRequestAdmissionPolicy مشارکت غیرفعال، میزبان غیر Ready، مدل ناسازگار، ظرفیت ناشناخته/ناکافی و deadline نامعتبر را رد می‌کند. HostRequestCoordinator با SemaphoreSlim(1,1) و WaitAsync(0) درخواست دوم را Busy می‌کند.
شکاف احرازشده: حد RAM و فضای آزاد دیسک در UI/حافظهٔ موقت بود و با restart ذخیره نمی‌شد.

## اصلاح
- LocalHostSettings و سند تنظیمات به schema version 2 ارتقا یافته‌اند.
- requiredModelMemoryGiB و minimumFreeDiskGiB پایدار ذخیره و با مقدار finite، مثبت و سقف 1,048,576 GiB validate می‌شوند.
- schema نسخهٔ 1 بدون از دست‌دادن URL، model، timeout، مسیرها و preference تاریخچه بارگذاری می‌شود؛ مقدارهای پیش‌فرض UI یعنی 4 GiB RAM و 1 GiB disk برای فیلدهای جدید اعمال می‌شوند و نخستین Save بعدی سند را به schema 2 ارتقا می‌دهد.
- WPF مقدارها را در startup بازیابی، در LostFocus ذخیره و پیش از اتصال، inference محلی و peer hosting دوباره validate/persist می‌کند.
- شکست ذخیره‌سازی یا مقدار نامعتبر، capacity را Unknown/Not Ready می‌کند و inference/admission fail-closed می‌ماند.
- آزمون C# round-trip، migration نسخهٔ 1، ارتقای schema و رد صفر/NaN/Infinity/بیش از سقف را پوشش می‌دهد؛ آزمون Python سیم‌کشی UI، persistence و admission را کنترل می‌کند.

## مرز سنجش
تنها RAM آزاد و فضای آزاد درایو سیستم قابل تنظیم‌اند. GPU صرفاً inventory آداپتور نمایش است؛ سنجش/سقف CPU/GPU/VRAM، حالت بیکار یا برق ادعا نمی‌شود.

## آزمون
- CI V 3.1.36 فقط baseline پیش از تغییر است.
- آزمون‌های جدید C# و Python، graph guards، Windows build/installer و HostLifecycle suite باید روی SHA این commit اجرا شوند.
- دستگاه Android فیزیکی، میزبان Windows فیزیکی، مدل checksumدار، Android-to-Windows E2E، clean install و in-place upgrade در این تلاش اجرا نشده‌اند.

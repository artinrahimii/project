# T0181-A001 — شواهد و بازبینی دقیق snapshot

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit مبنا: `cdd9282c4ee01abd6232a97dc7fd45565ecd38bb` (`V 3.0.837`)
- tree مبنا: `974580be02d75eedde3dd2a0c9cdf86eaf5b1316`
- Blob گراف مبنا: `03f4331d08f31c17f22404d0880e675bb0ee8757`؛ محتوای UTF-8 برابر 3,353,447 بایت بود و Git SHA-1 محاسبه‌شده با شناسهٔ Blob تطبیق داشت. JSON کامل در نسخهٔ جداشده از حصار Markdown parse شد.
- دریافت عادی فایل گراف محتوای خالی برگرداند؛ دریافت Blob با SHA استخراج‌شده از همان tree استفاده شد. دریافت عادی به‌عنوان محتوای کامل پذیرفته نشد.
- `T0177` و `T0180`، وضعیت `done` و شواهد موجودشان در snapshot مبنا بررسی شدند.
- دو یال ورودی T0181 بازبینی شدند. هر دو به `sequencing_only` تغییر طبقه‌بندی یافتند؛ `type=blocks` و ترتیب اجرایی حفظ شدند. دلیل و evidence در graph edge_change_log ثبت شد.
- قرارداد موجود: تاریخچهٔ میزبان تنها در حافظه بود؛ تنظیمات محلی پارامترهای backend و مدل را ذخیره می‌کردند. `WindowsDpapiSecretProtector` از `DataProtectionScope.CurrentUser` استفاده می‌کند.
- ADR-0019 مرز اعتماد میزبان را تشریح می‌کند؛ ADR-0020 ذخیره‌سازی Room اندروید را مستقل می‌داند و رمزگذاری در حالت سکون را به T0184 واگذار می‌کند. T0181 فقط مسیر میزبان Windows را تغییر می‌دهد.

## خروجی‌ها

- `src/Host.Windows/Chat/WindowsHostConversationStore.cs`: ذخیره‌سازی نسخه‌بندی‌شده، محدود و رمز‌شده با DPAPI، نوشتن اتمی و اعتبارسنجی پیام.
- `src/Host.Windows/MainWindow.xaml` و `MainWindow.xaml.cs`: گزینهٔ خاموش پیش‌فرض، تأیید مالک، بازیابی و ذخیره، خاموش‌سازی و حذف تاریخچه.
- سیاست `HostConversationRetentionPolicy`، `ChatConversation` و تنظیم `PersistHostConversation` افزوده/گسترش یافتند.
- آزمون‌های .NET/Python و smoke test WPF به‌روز شدند؛ `docs/security.md` دامنه و محدودیت DPAPI را بیان می‌کند.

## آزمون

- آزمون محلی .NET/Windows در این محیط اجرا نشد.
- آزمون‌های runtime و Python به CI دقیق commit سپرده شده‌اند؛ در این ثبت هیچ تستی موفق اعلام نشده است.
- تست DPAPI باید روی runner واقعی Windows اجرا شود؛ عبور از تست در سیستم غیر Windows معادل اجرای DPAPI نیست.

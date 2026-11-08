# بازبینی T0181 — رضایت صاحب میزبان برای نگهداری اختیاری

- مبنای شاخه main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd`، پیام `V 3.1.96`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی بررسی‌شده

- سیاست پیش‌فرض `ProcessSessionOnly` است و prompt/response را پایدار ذخیره نمی‌کند.
- فعال‌سازی تاریخچه در Windows یک رضایت جداگانه Yes/No با پیش‌فرض No می‌خواهد.
- هشدار UI دامنهٔ ذخیره و محدودیت backup، paging، crash dump و screenshot را اعلام می‌کند.
- تنظیمات فقط مقدار boolean رضایت را نگه می‌دارند؛ محتوای گفتگو در فایل جداگانهٔ محافظت‌شده با Windows DPAPI ذخیره می‌شود.
- خاموش‌کردن گزینه سیاست را به حالت پیش‌فرض بازمی‌گرداند و حذف فایل تاریخچه را امتحان می‌کند؛ شکست حذف نیز صریحاً گزارش می‌شود.

## آزمون و شواهد

- `ChatConversationAcceptanceTests.cs`: default-off، رضایت، ترتیب پیام، DPAPI round-trip با canary مصنوعی، حذف و فایل خراب را بررسی می‌کند.
- `LocalHostSettingsPersistenceTests.cs`: ذخیره و بارگذاری تنظیمات را بررسی می‌کند.
- `test_host_conversation_retention_policy.py` و `test_wpf_entrypoint.py`: UI و مرز ذخیره‌سازی را کنترل می‌کنند.
- CI دقیق کد محصول: Product Build run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق بودند.

وضعیت قدیمی `done` حفظ شد. بازبینی مجدد پس از UPD-S1-018 لازم است؛ آزمون دستگاه Windows واقعی در این بازبینی انجام نشد.

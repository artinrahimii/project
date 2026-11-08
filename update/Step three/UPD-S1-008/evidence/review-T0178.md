# بازبینی T0178 — عدم نگهداری پایدار محتوای میزبان به‌صورت پیش‌فرض

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` — `V 3.1.94`
- source code baseline: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-007، UPD-S1-018

## کد و رفتار

- `HostConversationRetentionPolicy.Default` از نوع `ProcessSessionOnly` است و `PersistsPromptAndResponse=false`.
- `ChatConversation` پیام‌ها را در فهرست حافظه نگه می‌دارد؛ API ذخیره‌سازی دیسک ندارد.
- ذخیرهٔ پایدار تاریخچه فقط با انتخاب صریح و تأیید جداگانهٔ مالک Windows فعال می‌شود؛ در آن حالت store از DPAPI برای حساب کاربر جاری استفاده می‌کند.
- تنظیمات پایدار فقط boolean رضایت را نگه می‌دارند، نه متن prompt/response.
- خاموش‌کردن retention تنظیم را false می‌کند و فایل محافظت‌شده را برای حذف ارسال می‌کند؛ پیام UI اگر حذف فایل شکست بخورد صریحاً محدودیت را اعلام می‌کند.

## آزمون و شواهد

- `tests/test_host_conversation_retention_policy.py`: default-off، consent، in-memory conversation، DPAPI، حذف فایل، opt-out و پاک‌سازی فایل‌های موقت قدیمی را کنترل می‌کند.
- `ChatConversationAcceptanceTests.cs`: ترتیب پیام‌ها، default retention، owner-consented DPAPI round-trip با canaryهای مصنوعی، حذف فایل و رد فایل خراب را بررسی می‌کند.
- Product Build دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد و HostLifecycle integration testها را اجرا کرد.
- Quality Gate دقیق source: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- اجرای محلی در این محیط انجام نشد.

## مرز و تصمیم

این وظیفه دربارهٔ retention در میزبان Windows است و به‌تنهایی نشان نمی‌دهد که prompt در logs/صفحه‌های واسط یا در crash dumps، paging یا backup وجود ندارد؛ خود UI نیز این استثناها را به صاحب میزبان هشدار می‌دهد. وضعیت پایه `done` حفظ شد و `re_review_required` برای هماهنگی با سیاست امنیتی و آزمون‌های UPD-S1-018 ثبت شد.

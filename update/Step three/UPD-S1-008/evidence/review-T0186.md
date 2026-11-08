# بازبینی T0186 — افشای حریم خصوصی قبل از اعتماد/مشارکت همتا

- مبنای main: `fc006d4c7f01de25c27346fc85670d1dc503e231`، پیام `V 3.1.98`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-011، UPD-S1-018، UPD-S1-022

## پیاده‌سازی بررسی‌شده

- متن مشترک Windows می‌گوید host بعد از رمزگشایی، prompt و capsule را می‌بیند و رمزگذاری انتقال محتوا را از میزبان مقصد پنهان نمی‌کند.
- Windows پیش از ساخت `PeerAnnouncementHostLifecycle` پنجرهٔ هشدار و رضایت Yes/No نمایش می‌دهد؛ پاسخ No مشارکت را خاموش نگه می‌دارد.
- Android پیش از ذخیره trust، PeerId را همراه هشدار قابل‌مشاهده‌بودن prompt/capsule نشان می‌دهد؛ ذخیره trust به‌تنهایی درخواست ارسال نمی‌کند و ارسال با اقدام مجزای کاربر انجام می‌شود.

## آزمون و شواهد

- `tests/test_peer_inference_privacy_disclosure.py`: ترتیب نمایش هشدار نسبت به ذخیره trust و ساخت سرویس Windows را کنترل می‌کند.
- `tests/HostLifecycle.Tests/Program.cs`: متن هشدار shared و مرز محتوای payload را کنترل می‌کند.
- `tests/test_claims_audit.py`: افشای visibility میزبان و عدم ادعای محرمانگی نسبت به endpoint را بررسی می‌کند.
- Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Quality Gate جدید روی main [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) نیز موفق بود.

## تصمیم

وضعیت پایه `done` حفظ شد؛ `re_review_required` تا پس از UPD-S1-018 و آزمون واقعی trust/request flow باقی می‌ماند. این آزمون‌ها شبکهٔ فیزیکی یا عدم نگهداری محتوا توسط اپراتور مقصد را اثبات نمی‌کنند.

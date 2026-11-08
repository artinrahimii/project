# بازبینی T0185 — حداقل مجوزهای Android

- مبنای شاخه main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd`، پیام `V 3.1.96`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `unchanged`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی بررسی‌شده

- source manifest فقط `android.permission.INTERNET` را درخواست می‌کند و Android backup خاموش است.
- برنامه API درخواست runtime permission را فراخوانی نمی‌کند.
- verifier مجوزهای APK بسته‌بندی‌شده را با AAPT2 بررسی می‌کند؛ علاوه بر INTERNET فقط مجوز signature خصوصی AndroidX برای dynamic receiver مجاز است.
- storage app-private، Keystore و SQLCipher به مجوز عمومی storage نیاز ندارند.

## آزمون و شواهد

- `test_android_minimum_permissions.py`: manifest، خروجی AAPT، permission اضافهٔ dependency، مجوزهای غیرمجاز و نبود درخواست runtime را بررسی می‌کند.
- Product Build، verifier را روی APK واقعی buildشده اجرا می‌کند: run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567).
- Quality Gate run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.

وضعیت پایه `done` و تصمیم `unchanged` حفظ شد؛ در source و workflow شواهدی برای نیاز به مجوز جدید دیده نشد. verifier باید در هر تغییر بعدی در transport/dependency دوباره اجرا شود.

# بازبینی T0204 — round دوم پس از سکوت سه میزبان

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916`، `V 3.1.104`
- اجرای تاریخی: `28d43ec6c5cf42a26926a5a50238d2f7a1ce044b`، `V 3.0.937`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- UPDهای مرتبط: UPD-S1-003، UPD-S1-015، UPD-S1-019، UPD-S1-020

## پیاده‌سازی و آزمون تاریخی

- در source تاریخی، تست `dispatchesSecondRoundToThreeNewPeersAfterFirstThreeRemainSilent` سه peer اول را بی‌پاسخ می‌گذاشت، timeout مشترک را بررسی می‌کرد و سپس سه peer تازه را برای round دوم انتخاب می‌کرد.
- آزمون native Android/.NET interop در Product Build run [41911308402](https://github.com/artinrahimii/project/actions/runs/41911308402)، job Android 126859276431، موفق بود. Quality Gate run 41911308225 نیز موفق بود.
- گزارش تاریخی سه دور/شش میزبان را با identity و attempt correlation آزمایش می‌کند؛ پاسخ‌ها fixture-based هستند و شبکه loopback است.

## کد فعلی و مغایرت

- در transport test فعلی، تست سه peer silent + سه peer دوم دیگر وجود ندارد؛ تست native .NET فعلی یک peer متفاوت را پس از timeout peer اول انتخاب می‌کند.
- production first-release اکنون یک میزبان در هر round و سه تلاش ترتیبی را enforce می‌کند؛ generic selector/dispatcher هنوز compatibility چندمیزبانه دارد.
- بنابراین اجرای تاریخی T0204 با سیاست current first-release یکسان نیست و باید بعد از UPD-S1-003/015 و انتخاب نهایی policy دوباره اجرا شود.

وضعیت پایه `done` حفظ شد؛ `re_review_required` ثبت شد. این شواهد LAN فیزیکی یا inference واقعی سه میزبان نیست.

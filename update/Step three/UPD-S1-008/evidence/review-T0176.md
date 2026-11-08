# بازبینی T0176 — آزمون تعداد پاسخ‌ها، timeout، پاسخ دیرهنگام و لغو

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` — `V 3.1.94`
- source code baseline: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ source محصول در commitهای مستنداتی بعدی تغییر نکرده است.
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017، UPD-S1-019

## آزمون‌های واقعی موجود

- `InferenceRoundResponseCollectorTest.kt`: انتظار برای پاسخ‌های sibling پس از اولین پاسخ معتبر، timeout یک میزبان بدون لغو پاسخ دیگر، submission failure، round window منقضی، cancellation واقعی caller، صفر پاسخ، یک پاسخ از سه و سه پاسخ مرتب را بررسی می‌کند.
- `RequestViewModelTest.kt`: پاسخ تکراری، پاسخ نامرتبط با request/attempt/host/model، پاسخ request قبلی، کارت‌های جدا و لغو attempt را آزمایش می‌کند.
- `InferenceResponseCorrelationValidatorTest.kt`: پاسخ قبلی، round mismatch و completion/receive دیرهنگام را رد می‌کند.
- contract testها پیوند source به مجموعهٔ تست را کنترل می‌کنند.

## شکاف مشاهده‌شده

در فهرست تست‌های فعلی، مورد مستقل و صریحی برای «دقیقاً دو پاسخ معتبر» پیدا نشد؛ تست one-of-three و three-responses وجود دارد. تست‌های صفر/یک/سه، پاسخ تکراری، پاسخ دیرهنگام، timeout و cancellation وجود دارند. بنابراین عبارت معیار «صفر، یک، دو و سه پاسخ» هنوز به‌طور کامل اثبات نشده است.

## شواهد اجرای CI

- Product Build دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد و Android JVM test suite را اجرا کرد.
- Quality Gate دقیق source: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- اجرای محلی مستقل در این محیط ممکن نشد.

## تصمیم

وضعیت پایهٔ `done` حفظ شد، اما تصمیم `re_review_required` ثبت می‌شود تا تست صریح دو پاسخ اضافه/اجرا شود و با تغییرات UPD-S1-015/019 مجموعهٔ آزمون نهایی دوباره اجرا شود. این بازبینی هیچ آزمون واقعی Windows/Android یا مدل محلی را جایگزین نمی‌کند.

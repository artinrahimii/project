# بازبینی T0197 — capsule، اندازهٔ payload و correlation پاسخ

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b`، `V 3.1.100`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- Update مرتبط طبق reconciliation: UPD-S1-019

## پیاده‌سازی و آزمون بررسی‌شده

- `ConversationContextCapsule` از نسخهٔ schema ثابت، انتخاب محدود پیام‌های زمینه و serialization صریح استفاده می‌کند؛ کل تاریخچهٔ Room به‌طور خودکار ارسال نمی‌شود.
- builder پیش از transport طول متن و حجم byteهای UTF-8 را بررسی می‌کند؛ capsule سقف ۴ KiB و کل پیام سقف پروتکل را رعایت می‌کند؛ خطای serialization یا metadata نامعتبر fail-closed است.
- request و response به request/attempt/round/peer/model متصل‌اند؛ پاسخ نامرتبط به‌جای ثبت در context فعلی رد می‌شود.
- آزمون‌ها Unicode/UTF-8، مرز طول، JSON نامعتبر، metadata، request mismatch و response correlation را پوشش می‌دهند.

## آزمون و شواهد

- `ConversationContextCapsuleTest.kt`
- `InferenceRequestPayloadBuilderTest.kt`
- `InferenceResponseCorrelationValidatorTest.kt`
- `InferenceRoundResponseCollectorTest.kt`
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) شامل Android JVM tests موفق بود؛ Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) نیز موفق بود.

وضعیت پایه `done` حفظ شد؛ تغییرات قرارداد capsule، payload و پاسخ در UPD-S1-019 باید با محصول نهایی دوباره اعتبارسنجی شوند. این تست‌ها جایگزین E2E Android-to-Windows و ذخیرهٔ پاسخ در همان Room conversation نیستند.

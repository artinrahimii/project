# T0135 — گزارش اجرای وظیفه

- **وضعیت نهایی:** `done`.
- **Commit حاوی اصلاح و آزمون پذیرفته‌شده:** [V 3.0.475 — `0f2e7efcd21d5a1d44f116054021a8adcd84ddf1`](https://github.com/artinrahimii/project/commit/0f2e7efcd21d5a1d44f116054021a8adcd84ddf1).
- **تلاش‌ها:** ۲؛ نخستین تلاش با خطای کامپایل شکست خورد، اصلاح شد و تلاش دوم از CI گذشت.

## پیاده‌سازی

`tests/HostLifecycle.Tests/RequestIdentityIndependenceAcceptanceTests.cs` به test runner موجود متصل شد. آزمون قالب شناسهٔ opaque با ۱۲۸ بیت تصادفی، ۵۱۲ تولید نمونه، نبود فیلدهای پایدار `user_id`/`account_id`/`device_id`/`conversation_id`/`persistent_conversation_id` در پیام‌های منتخب Protobuf، اعتبارسنجی درخواست serialize/parse شده و حفظ request ID در پاسخ خطای typed را می‌سنجد.

کد مولد شناسه و قرارداد wire تغییر نکردند؛ پیاده‌سازی موجود با regression test اجرایی حفاظت شد. `ChatConversation` فقط پیام و شمارهٔ ترتیبی session-local نگه می‌دارد و شناسهٔ پایدار گفت‌وگو ایجاد نمی‌کند.

## بازبینی وابستگی‌ها

- `T0134 → T0135` به `sequencing_only` اعتبارسنجی و در گراف فعال حفظ شد.
- `T0128 → T0135` با شواهد و دلیل ثبت‌شده از یال‌های فعال حذف شد؛ آزمون دو رایانه/شبکهٔ مستقل API یا قرارداد لازم برای جداسازی شناسه تولید نمی‌کند. این اقدام اجرای `T0128` را اثبات نمی‌کند؛ آن همچنان `not_started` است.

## نتایج آزمون

- تلاش اول در `V 3.0.474`: شکست کامپایل `CS1061` به دلیل واردنشدن namespace مربوط به extension `Google.Protobuf` برای `ToByteArray()`؛ سابقه حفظ شده است.
- تلاش دوم در `V 3.0.475`: پس از افزودن `using Google.Protobuf;`، HostLifecycle موفق شد و لاگ صریحاً ثبت کرد: `PASS: request IDs remain opaque and independent of stable user/conversation identifiers; protocol contracts exclude stable IDs and preserve request correlation.`
- Step Three quality gate، Windows WPF build، Android APK/Protobuf، و real local inference integration روی همان commit نیز موفق شدند.

## وظیفهٔ منطقی بعدی

`T0136` باقی است؛ یال‌های `T0135 → T0136` و `T0128 → T0136` هنوز بررسی‌نشده‌اند و باید قبل از اجرای آن تحلیل شوند. `T0128` اجرا‌نشده باقی می‌ماند.

# T0135 — شواهد و اعتبارسنجی

## نسخهٔ منبع

- Branch: `main`
- Base commit: `598b34674a15ded12c0c38979eed0d3a09a8189c` (`V 3.0.473`)
- Graph path: `step three/project-graph.md`
- Graph tree Blob: `b470ac4cbba0da9df673b3974167317934c59e5f`
- روش دریافت: Git Trees API در درخت `62095b19d4f50e1a6ebf8d2083f421b4b9dcfd5c` و سپس Git Blob API برای Blob بالا.
- اندازهٔ اعلام‌شده/دریافت‌شدهٔ گراف: **2,291,548 بایت UTF-8**؛ طول رشتهٔ دریافتی **2,169,396 نویسه**.
- Git Blob SHA-1 باز‌محاسبه‌شده از بایت‌های اصلی شامل حصارهای Markdown: `b470ac4cbba0da9df673b3974167317934c59e5f`؛ تطبیق با درخت: موفق.
- JSON محصور در Markdown به‌طور کامل parse شد: 229 گره، 421 یال فعال، وضعیت گره‌ها: 126 `done`، 1 `blocked`، 102 `not_started`. تعداد یال‌های `unreviewed`: 198.

## شواهد کد/قرارداد

- `src/Host.Windows/Requests/SecureRequestIdGenerator.cs` — مولد ۱۲۸ بیتی تصادفی و بدون پارامترهای هویتی.
- `protocol/whitep/v1/messages.proto` — پیام inference از `request_id` و `attempt_id` مستقل استفاده می‌کند؛ شناسهٔ پایدار کاربر/گفت‌وگو در پیام‌های منتخب تعریف نشده است.
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs` — خطاهای typed، `request_id` را برای هم‌بستگی حفظ می‌کنند.
- `src/Host.Application/Chat/ChatConversation.cs` و `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs` — گفت‌وگوی فعلی درون‌نشستی است؛ شناسهٔ پایدار گفت‌وگو ساخته یا ذخیره نمی‌شود.
- تصمیم مرتبط: `docs/decisions/ADR-0013-temporary-request-identifiers.md`.

## شاهد CI در نسخهٔ مبنا

- Step Three quality gate: [run 40729198789](https://github.com/artinrahimii/project/actions/runs/40729198789) — success، commit دقیق `598b34674a15ded12c0c38979eed0d3a09a8189c`.
- HostLifecycle / Windows / Android: [product run 40729198759](https://github.com/artinrahimii/project/actions/runs/40729198759) — همهٔ سه job موفق روی همان commit.
- در HostLifecycle log مبنا: آزمون secure libp2p peer identity، رد peer بدون encryption، رد protocol version نامعتبر، و آزمون فعلی generator شناسه با 4096 نمونه همگی PASS شدند. این‌ها فقط baseline هستند؛ آزمون تازهٔ T0135 را جایگزین نمی‌کنند.

## وضعیت آزمون این commit

- `RequestIdentityIndependenceAcceptanceTests.Run`: **در انتظار CI**.
- اجرای محلی .NET ادعا نمی‌شود.
- وضعیت T0135 در این revision به همین دلیل `in_progress` است.

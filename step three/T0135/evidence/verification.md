# T0135 — شواهد و اعتبارسنجی

## گراف و نسخهٔ دقیق بررسی‌شده

- بررسی قبل از اصلاح: `main` روی `V 3.0.474`، commit `d6cd1fced96dd85626c0e16b044b5a111f8890a3`.
- Tree SHA: `da9040116d69e07392903c7f1a6f06c2e08e0f56`.
- مسیر graph: `step three/project-graph.md`; Blob `2286f25800dc8613d4f3abd3679ed48b498f8366`.
- روش دریافت: Tree API از tree مختص commit و سپس Git Blob API.
- اندازهٔ Blob اعلام‌شده و UTF-8 محاسبه‌شده: **2,298,059 بایت**؛ طول رشتهٔ دریافتی: **2,175,961 نویسه**.
- Git Blob SHA-1 محاسبه‌شده از محتوای کامل اصلی شامل حصارهای Markdown: `2286f25800dc8613d4f3abd3679ed48b498f8366`; تطبیق با Blob همان tree: موفق.
- JSON کامل parse شد: 229 گره، 420 یال فعال؛ T0135 در `in_progress` بود و سابقهٔ تغییر یال‌ها در گراف موجود بود.

## شواهد کد

- `src/Host.Windows/Requests/SecureRequestIdGenerator.cs`: مولد ID تصادفی ۱۲۸ بیتی بدون پارامتر هویتی.
- `protocol/whitep/v1/messages.proto`: قرارداد wire، request و attempt ID را جدا نگه می‌دارد.
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`: حفظ request ID در پاسخ‌های خطادار.
- `docs/decisions/ADR-0013-temporary-request-identifiers.md`: تصمیم معماری شناسه‌های موقت.
- آزمون اختصاصی `tests/HostLifecycle.Tests/RequestIdentityIndependenceAcceptanceTests.cs` به `tests/HostLifecycle.Tests/Program.cs` متصل است.

## نتایج CI

- Step Three quality gate روی exact commit V 3.0.474: **PASS** — [run 40733088130](https://github.com/artinrahimii/project/actions/runs/40733088130).
- HostLifecycle روی exact commit V 3.0.474: **FAIL در کامپایل** پیش از اجرای آزمون‌ها؛ `CS1061` برای extension `ToByteArray`، به دلیل نبود `using Google.Protobuf;` — [job 122974324111](https://github.com/artinrahimii/project/actions/runs/40733088328/job/122974324111).
- رفع خطا در تغییر جاری: اضافه‌شدن `using Google.Protobuf;`; منتظر اجرای CI نسخهٔ بعدی هستیم.
- هیچ ادعایی دربارهٔ اجرای موفق آزمون تازه یا اجرای محلی وجود ندارد.

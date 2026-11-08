# بازبینی محتوایی T0061 — نگاشت inference به API محلی و دریافت پاسخ واقعی

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `bc9882a8ad1fcd29b6a2e42e31509a76e926f2f6` — `V 3.1.78`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد.
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Infrastructure/LlamaCppServerBackend.cs` — `8112e58e9a952fa41415acee70bc8ca3baf7e654`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`
- `.github/workflows/real-inference.yml` — `6c3a6aa183c24c5ad3493d7c89f8bd291a4bddc4`

## یافته‌های فنی
- LlamaCppServerBackend درخواست را به POST /v1/chat/completions با model ID تنظیم‌شده، prompt و max_tokens تبدیل می‌کند؛ پاسخ choices[0].message.content را استخراج، validate و برمی‌گرداند. پیش از inference health check اجرا می‌شود و model capability از /v1/models و /props خوانده می‌شود.
- آزمون opt-in واقعی process را از طریق launcher اجرا می‌کند، loopback و health را کنترل و پاسخ غیرخالی را از backend واقعی می‌گیرد؛ پاسخ fixture را صریحاً رد می‌کند.
- workflow باینری llama.cpp b11443 را با archive SHA ثابت اعتبارسنجی و GGUF را به revision مشخص pin کرده؛ تاریخچهٔ exact run 40077575618 پاسخ واقعی مدل را PASS کرده است.

## علت بازبینی مجدد
- UPD-S1-023 هنوز انتخاب deterministic مدل از catalog نصب‌شده و capability واقعی را به مسیر عادی وصل نکرده است؛ T0061 باید پس از اجرای سیاست مدل دوباره بررسی شود. این شواهد اثبات Android-to-P2P نیستند.

## شواهد و محدودیت CI
- Exact V3.1.78 Quality: [run 42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) passed.
- Exact V3.1.78 Rendezvous: [run 42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) passed.
- Product build [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) was still in progress during this review.
- Historical live local-model evidence: run 40077575618 (T0061), 40078502151 (T0062), and 40092245411 (T0063/T0064) on older exact implementation commits; these are not V3.1.78 results.
- T0065 is a source-level provider boundary check, not packet capture or runtime egress proof.
- No physical Android-to-Windows E2E or install/upgrade test was performed in this review.

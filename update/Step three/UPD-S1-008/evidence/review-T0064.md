# بازبینی محتوایی T0064 — آزمون opt-in با مدل کوچک دارای مجوز

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `bc9882a8ad1fcd29b6a2e42e31509a76e926f2f6` — `V 3.1.78`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد.
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `.github/workflows/real-inference.yml` — `6c3a6aa183c24c5ad3493d7c89f8bd291a4bddc4`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`
- `step three/T0064/execution-report.md` — `199ad2994bd1d38f3c21a6f07baf589c7eac5a12`

## یافته‌های فنی
- workflow llama.cpp CPU binary را با archive SHA ثابت دانلود و مدل GGUF را به revision a6b7826ed969c3926a98359e39912af137b464cb pin می‌کند؛ size و GGUF magic را بررسی و WHITEP_RUN_REAL_INFERENCE_TEST=1 را فعال می‌کند.
- مدل pin شده در شواهد قبلی Apache-2.0 ثبت شده؛ workflow exact run 40092245411 پاسخ واقعی، cancellation فعال و توقف process را PASS کرده است.
- Product Build عادی الزاماً مسیر live model را اجرا نمی‌کند؛ یک SKIP در test harness موفقیت inference واقعی نیست.

## علت بازبینی مجدد
- UPD-S1-023 باید مدل pin شده، license/checksum و metadata را با catalog محصول سازگار کند؛ پس از آن real-model workflow روی مسیر انتخاب‌شدهٔ جدید باید اجرا شود.

## شواهد و محدودیت CI
- Exact V3.1.78 Quality: [run 42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) passed.
- Exact V3.1.78 Rendezvous: [run 42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) passed.
- Product build [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) was still in progress during this review.
- Historical live local-model evidence: run 40077575618 (T0061), 40078502151 (T0062), and 40092245411 (T0063/T0064) on older exact implementation commits; these are not V3.1.78 results.
- T0065 is a source-level provider boundary check, not packet capture or runtime egress proof.
- No physical Android-to-Windows E2E or install/upgrade test was performed in this review.

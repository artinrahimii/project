# بازبینی محتوایی T0068 — schema نسخه‌دار metadata مدل

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json` — `57a8beac9317c8767c4409d9ce16dd9ccde8c3ac`
- `src/Host.Windows/WhiteP.Host.Windows.csproj` — `tree hash not captured`
- `tests/test_model_catalog_schema.py` — `c45b47249b86dd88b2e0acf176213db4a3a0c42b`
- `src/Host.Windows/Inference/LocalInferenceBackendFactory.cs` — `41208258c6afb87f0586da7697f277d13ade960a`

## یافته‌ها
- Schema v1 id/name/GGUF format/size/checksum SHA-256/backend/source URL/license metadata را تعریف می‌کند؛ اضافات را رد و backend را به llama.cpp محدود می‌سازد.
- license metadata اختیاری است اما missing به review_required و classification unknown منتهی می‌شود؛ schema تصریح می‌کند metadata قانونی اجازهٔ redistribution را ثابت نمی‌کند.
- tests schema required fields، hash regex، GGUF/backend limits، HTTPS fields و copy-to-output/publish را بررسی می‌کنند؛ runs 40106114209 و 40106114148 موفق‌اند.

## علت بازبینی مجدد
- UPD-S1-023 نیاز به deterministic selection و max-output policy دارد؛ schema v1 به‌تنهایی capability/token-window/backend runtime را نمایش نمی‌دهد. توسعهٔ schema مستلزم بازاعتبارسنجی parser و مصرف‌کنندگان است.

## شواهد و محدودیت CI
- Exact V3.1.79 Quality: [run 42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) passed.
- Exact V3.1.79 Rendezvous: [run 42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) passed.
- Product build [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) was still in progress during this review.
- Historical task-specific tests/builds are cited with their exact earlier runs in the legacy execution reports; they are not current V3.1.79 results.
- No physical Android-to-Windows E2E or installation/upgrade test was performed in this review.

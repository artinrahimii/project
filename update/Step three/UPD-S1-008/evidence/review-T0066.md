# بازبینی محتوایی T0066 — اعتبارسنجی نسخه و checksum باینری موتور

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Inference/LocalInferenceBinaryVerifier.cs` — `7a376bb4a6423ddbc9135b40325497ad35b2aef3`
- `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs` — `10ba72549cb8cd94e5d116813ce0554fdcfc13a1`
- `tests/test_local_inference_binary_verifier.py` — `9b1e842e0dfedc4f454987ca645ea928e7757122`
- `.github/workflows/real-inference.yml` — `6c3a6aa183c24c5ad3493d7c89f8bd291a4bddc4`

## یافته‌ها
- Verifier قبل از اجرای version probe، SHA-256 را با FixedTimeEquals مقایسه می‌کند؛ نام فایل llama-server.exe را محدود، --version را با ProcessStartInfo.ArgumentList و UseShellExecute=false اجرا، timeout را enforce و پس از probe hash را دوباره بررسی می‌کند.
- LocalInferenceProcessLauncher پیش از Process.Start verifier را فراخوانی می‌کند و env version/SHA مورد اعتماد را لازم می‌داند؛ regression tests ترتیب hash-before-probe، recheck، shell-free/timeout و verify-before-start را assert می‌کنند.
- workflow باینری رسمی llama.cpp b11443 را با SHA archive ثابت بررسی و SHA executable را به launcher می‌دهد؛ historical real inference run 40104554823 و product run 40104554847 موفق ثبت شده‌اند.

## علت بازبینی مجدد
- UPD-S1-023 باید executable pin و مدل catalog را به یک سیاست انتخاب backend/model وصل کند؛ مسیر انتخاب و منبع مقادیر مورد اعتماد باید پس از این کار دوباره بررسی شود.

## شواهد و محدودیت CI
- Exact V3.1.79 Quality: [run 42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) passed.
- Exact V3.1.79 Rendezvous: [run 42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) passed.
- Product build [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) was still in progress during this review.
- Historical task-specific tests/builds are cited with their exact earlier runs in the legacy execution reports; they are not current V3.1.79 results.
- No physical Android-to-Windows E2E or installation/upgrade test was performed in this review.

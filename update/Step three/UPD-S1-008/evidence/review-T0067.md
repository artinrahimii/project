# بازبینی محتوایی T0067 — قرار دادن backend پشت interface مشترک

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Inference/LocalInferenceBackendFactory.cs` — `41208258c6afb87f0586da7697f277d13ade960a`
- `src/Host.Application/IInferenceBackend.cs` — `e4840e3b7f5070adc6f536a357f3d2ccde286e3c`
- `src/Host.Application/HostInferenceSession.cs` — `2ccb69e748c5b40e89252925d1da80c1e259fab5`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/test_inference_backend_abstraction.py` — `c10fcd4ec3dc10bef26d9f56065d1de85a342b57`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`

## یافته‌ها
- MainWindow از backendFactory: LocalInferenceBackendFactory.Create استفاده می‌کند؛ HostInferenceSession فقط IInferenceBackend را نگه می‌دارد و GenerateAsync را از interface صدا می‌زند، نه از concrete LlamaCppServerBackend.
- Factory شناسهٔ پشتیبانی‌شدهٔ llama.cpp را ارائه و instance LlamaCppServerBackend را به عنوان IInferenceBackend برمی‌گرداند؛ harness یک alternate backend را بدون تغییر منطق conversation/network اجرا کرده و PASS ثبت کرده است.
- تست Python جداسازی window/session/factory و تست C# اجرای alternate backend را بررسی می‌کنند؛ runs 40105391421، 40105391441 و real inference 40105391434 موفق ثبت شده‌اند.

## علت بازبینی مجدد
- UPD-S1-023 انتخاب بین backendهای واقعاً پشتیبانی‌شده و capability واقعی را اضافه می‌کند؛ factory فعلی فقط llama.cpp را expose می‌کند. قرارداد و آزمون alternate backend باید با resolver جدید دوباره اعتبارسنجی شود.

## شواهد و محدودیت CI
- Exact V3.1.79 Quality: [run 42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) passed.
- Exact V3.1.79 Rendezvous: [run 42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) passed.
- Product build [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) was still in progress during this review.
- Historical task-specific tests/builds are cited with their exact earlier runs in the legacy execution reports; they are not current V3.1.79 results.
- No physical Android-to-Windows E2E or installation/upgrade test was performed in this review.

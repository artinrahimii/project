# بازبینی محتوایی T0060 — health check واقعی backend پیش از Ready

## خط مبنا و تصمیم
- شاخه: `main`
- commit کد بررسی‌شده: `a19595c475752832fdd3701e109dc0e1a088721f` — `V 3.1.77`
- وضعیت پایه: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌های بررسی‌شده و Blob SHA
- `src/Host.Infrastructure/LlamaCppServerBackend.cs` — `8112e58e9a952fa41415acee70bc8ca3baf7e654`
- `src/Host.Application/HostInferenceSession.cs` — `2ccb69e748c5b40e89252925d1da80c1e259fab5`
- `src/Host.Windows/Inference/LocalInferenceBackendFactory.cs`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`

## یافته‌های کد و مصرف‌کنندگان
- LlamaCppServerBackend.CheckHealthAsync درخواست GET /health واقعی می‌فرستد و خطا/timeout را با پیام امن و IsAvailable=false ثبت می‌کند.
- HostInferenceSession.CheckBackendCoreAsync ابتدا CheckHealthAsync را اجرا می‌کند، سپس Readiness را ارزیابی و فقط پس از موفقیت هر دو HostState.Ready را تنظیم می‌کند.
- همین backend مدل فعال را از /v1/models و context را از /props می‌خواند؛ Program test صریحاً شکست health قبل از capacity readiness را PASS می‌کند.

## محدودیت و trigger بازبینی مجدد
- UPD-S1-023 باید health/model capability را با catalog معتبر، شناسهٔ دقیق و سقف خروجی انتخاب‌شده یکپارچه کند؛ health endpoint به‌تنهایی انتخاب مدل و آمادگی کل درخواست را اثبات نمی‌کند.

## CI و مرز شواهد
- Quality V3.1.77: [run 42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق.
- Product Build V3.1.77: [run 42502195216](https://github.com/artinrahimii/project/actions/runs/42502195216) هنگام ثبت بررسی هنوز در حال اجرا بود.
- آزمون‌های قبلی همان وظیفه در تاریخچه حفظ شده‌اند؛ CI Product جدید هنوز نتیجهٔ نهایی نداشت.
- دستگاه فیزیکی، inference زنده در همین attempt، Android-to-Windows E2E و upgrade فیزیکی اجرا نشده‌اند.

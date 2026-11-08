# بازبینی محتوایی T0057 — قرارداد IInferenceEngine

## خط مبنا و تصمیم
- شاخه: `main`
- commit کد بررسی‌شده: `a19595c475752832fdd3701e109dc0e1a088721f` — `V 3.1.77`
- وضعیت پایه: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌های بررسی‌شده و Blob SHA
- `src/Host.Application/Inference/IInferenceEngine.cs` — `83822ca937bc21c00274ea9a53d3821c12063b5e`
- `tests/HostLifecycle.Tests/InferenceEngineContractTests.cs` — `77d67c08e3458c5924890c30b07f2bb3bc77d963`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`
- `src/Host.Windows/Inference/LocalInferenceBackendFactory.cs`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`

## یافته‌های کد و مصرف‌کنندگان
- IInferenceEngine قرارداد Discover/Load/Generate با cancellation، Cancel، Unload و IAsyncDisposable را تعریف می‌کند؛ آزمون contract امضاها و DiscoveryResult را با reflection کنترل می‌کند.
- در composition واقعی MainWindow، HostInferenceSession با LocalInferenceBackendFactory.Create ساخته می‌شود؛ factory، LlamaCppServerBackend را از طریق IInferenceBackend بازمی‌گرداند. MainWindow ارجاعی به IInferenceEngine یا LlamaCppInferenceEngine ندارد.
- بنابراین قرارداد و adapter وجود دارند، اما اتصال آن‌ها به مسیر اصلی پنجرهٔ میزبان احراز نشد؛ آزمون فعلی عمدتاً آزمون قرارداد API است، نه اثبات integration مسیر فعال.

## محدودیت و trigger بازبینی مجدد
- UPD-S1-023 باید مالکیت قرارداد مدل/backend را تعیین کند و مسیر فعال را بدون ایجاد دو پیاده‌سازی موازی یکپارچه یا adapter بلااستفاده را مستنداً کنار بگذارد. وضعیت پایهٔ legacy حفظ می‌شود اما بازبینی مجدد ضروری است.

## CI و مرز شواهد
- Quality V3.1.77: [run 42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق.
- Product Build V3.1.77: [run 42502195216](https://github.com/artinrahimii/project/actions/runs/42502195216) هنگام ثبت بررسی هنوز در حال اجرا بود.
- آزمون‌های قبلی همان وظیفه در تاریخچه حفظ شده‌اند؛ CI Product جدید هنوز نتیجهٔ نهایی نداشت.
- دستگاه فیزیکی، inference زنده در همین attempt، Android-to-Windows E2E و upgrade فیزیکی اجرا نشده‌اند.

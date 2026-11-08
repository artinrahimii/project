# بازبینی محتوایی T0059 — اجرای امن فرایند موتور محلی

## خط مبنا و تصمیم
- شاخه: `main`
- commit کد بررسی‌شده: `a19595c475752832fdd3701e109dc0e1a088721f` — `V 3.1.77`
- وضعیت پایه: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌های بررسی‌شده و Blob SHA
- `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs` — `10ba72549cb8cd94e5d116813ce0554fdcfc13a1`
- `tests/HostLifecycle.Tests/LocalInferenceProcessLauncherTests.cs` — `28d17851f0c8cc885d84a59ee8641f13fbdb4ea0`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`

## یافته‌های کد و مصرف‌کنندگان
- Launcher در MainWindow استفاده می‌شود؛ پیش از start نسخه و SHA-256 executable را با مقادیر مورد اعتماد verifier کنترل می‌کند، executable و GGUF را validate می‌کند و فقط loopback/port/alias مجاز را می‌پذیرد.
- ProcessStartInfo از UseShellExecute=false و ArgumentList جداگانه استفاده می‌کند؛ Stop/Dispose کل process tree را متوقف می‌کند.
- تست process launcher مسیر دارای فاصله و shell metacharacter، رد non-loopback، alias command-like و executable نامعتبر را بررسی می‌کند؛ این آزمون argument construction است و به‌تنهایی اجرای مدل واقعی نیست.

## محدودیت و trigger بازبینی مجدد
- منبع و سیاست catalog برای executable/model pinning و اتصال checksum با مدل انتخابی باید در UPD-S1-023 یکپارچه شوند. اجرای real local inference از تست fixture اثبات نمی‌شود.

## CI و مرز شواهد
- Quality V3.1.77: [run 42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق.
- Product Build V3.1.77: [run 42502195216](https://github.com/artinrahimii/project/actions/runs/42502195216) هنگام ثبت بررسی هنوز در حال اجرا بود.
- آزمون‌های قبلی همان وظیفه در تاریخچه حفظ شده‌اند؛ CI Product جدید هنوز نتیجهٔ نهایی نداشت.
- دستگاه فیزیکی، inference زنده در همین attempt، Android-to-Windows E2E و upgrade فیزیکی اجرا نشده‌اند.

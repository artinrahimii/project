# بازبینی محتوایی T0054 — خارج‌کردن میزبان از Ready در حالت نامناسب

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Application/HostReadinessEvaluator.cs` — Blob SHA: `049e6be160eb712602e4a56cf22b52ade916b302`
- `src/Host.Application/HostInferenceSession.cs` — Blob SHA: `2ccb69e748c5b40e89252925d1da80c1e259fab5`
- `src/Host.Windows/MainWindow.xaml.cs` — Blob SHA: `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs` — Blob SHA: `04540b9be9a1aa8f89446bb35941931bf7180f59`
- `tests/HostLifecycle.Tests/Program.cs` — Blob SHA: `e63132c55538e289e5f49840943e35349e758052`
- `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs` — Blob SHA: `fd4635da2b59af198533dbc5438c241067685f6e`

## یافته‌های کد و آزمون
- HostReadinessEvaluator ورودی RAM/disk را finite و مثبت اعتبارسنجی و نیازمندی را با RAM آزاد و فضای درایو سیستم مقایسه می‌کند؛ Unknown یا ناکافی fail-closed است.
- HostRequestAdmissionPolicy مشارکت خاموش، host/model غیر Ready، mismatch مدل، ظرفیت Unknown/Insufficient و deadline نامعتبر/منقضی را پیش از admission رد می‌کند.
- HostInferenceSession پیش از inference readiness/backend را ارزیابی می‌کند؛ HostLifecycle CI جاری مسیرهای ظرفیت/admission را PASS ثبت کرده است.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- UPD-S1-013 هنوز قیود نهایی ظرفیت و اعمال آن‌ها را کامل نکرده است؛ admission/readiness باید پس از آن بازآزمایی شود.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.

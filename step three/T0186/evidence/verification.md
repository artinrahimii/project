# T0186 — شواهد بازبینی وابستگی و اجرای رابط

## Snapshot و دریافت
- مخزن/شاخه: artinrahimii/project / main.
- commit مبنا: 6b507bba948743fe7da5cf3d4d4f9925601f24a7؛ پیام V 3.0.872.
- tree مبنا: 170b56f7a4dfe920a553b93748a08b0b7d5d30e7؛ tree کامل GitHub با truncated=false.
- فایل گراف از Blob 594db5476a2444a55724080b79a5ba8c734e2b45 دریافت شد؛ ۳٬۴۲۲٬۳۵۵ بایت UTF-8، SHA-1 Git Blob منطبق و JSON کامل قابل parse بود؛ ۲۳۰ وظیفه و ۴۲۲ یال استخراج شد.
- T0185 و T0177 هر دو در گراف done بودند؛ اجرای product/quality/container نسخهٔ V 3.0.872 نیز با موفقیت پایان یافته بود.

## شواهد پیش‌نیازها
- Android permission Manifest و verifier: فقط INTERNET در source manifest؛ verifier APK از exact product run قبلی برای AAPT2 استفاده می‌کند.
- T0177: گزارش و آزمون‌های InferenceAttemptDispatcher و HostCandidateSelector بررسی شد. T0177 تغییر API موردنیاز برای نمایش modal T0186 ایجاد نمی‌کند.
- جریان Android فعلی: MainActivity.kt پیش از ذخیرهٔ peer trust یک AlertDialog دارد؛ RequestViewModel.trustPeerAfterExplicitConfirmation فقط trust را ذخیره می‌کند و sendRequest مسیر جداگانهٔ ارسال است.
- جریان Windows فعلی: NetworkParticipationToggle_Changed پیش از ساخت PeerAnnouncementHostLifecycle فقط PeerId را تأیید می‌کرد؛ هشدار مستقلی دربارهٔ مشاهده‌پذیری prompt/capsule نداشت.

## یال‌های بررسی‌شده
- T0185 → T0186: blocks حفظ شد؛ طبقه‌بندی sequencing_only و وضعیت validated. شواهد: AndroidManifest.xml، verify_android_permissions.py، test_android_minimum_permissions.py و T0185/evidence/verification.md.
- T0177 → T0186: blocks حفظ شد؛ طبقه‌بندی sequencing_only و وضعیت validated. شواهد: RequestViewModel.kt، HostCandidateSelector.kt، InferenceAttemptDispatcher.kt و T0177/execution-report.md.
- یال بعدی T0186 → T0187 بررسی‌نشده باقی می‌ماند.

## اعتبارسنجی تغییرات همین commit
- کد و تست‌های Android، Windows، Shared.Security و Python به tree جدید افزوده شده‌اند.
- نتایج اجرای آزمون‌ها در لحظهٔ این snapshot: هنوز pending؛ هیچ موردی passed فرض نمی‌شود.
- پس از push باید exact-head CI شامل KtLint، ساخت/آزمون Android، WPF build/startup، HostLifecycle و Python quality بررسی شود.

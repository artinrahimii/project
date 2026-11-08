# بازبینی T0207 — clean build روی runner تازهٔ CI

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916`، `V 3.1.104`
- پذیرش تاریخی: Product Build run `41925547942` روی commit `5f45fc2a72b9ad382d5823c0568686aeb07ef3c`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- UPDهای مرتبط: UPD-S1-019، UPD-S1-021

## شواهد تاریخی

- Windows runner: WPF solution restore/clean/build/publish، installer و artifactهای host موفق بودند.
- Android runner: .NET peer fixture build، Gradle clean، KtLint، debug APK، JVM tests، Keystore instrumentation، Protobuf Java/Kotlin verification و APK/report upload موفق بودند.
- HostLifecycle job و Step Three Quality Gate نیز در گزارش تاریخی موفق ثبت شده‌اند.

## تغییر از زمان پذیرش تاریخی

مقایسهٔ commit پذیرش قدیمی تا main جاری 161 commit و 292 فایل تغییر را نشان می‌دهد. هر دو فایل زیر تغییر کرده‌اند:

- `.github/workflows/product-build.yml`
- `src/Client.Android/app/build.gradle.kts`

بنابراین pass تاریخی برای workflow/Gradle فعلی کافی نیست.

## اجرای جدید روی main جاری

Product Build run [42515602200](https://github.com/artinrahimii/project/actions/runs/42515602200) روی commit `168a1758094e88f3d1a9a2d86fde31136cdac916` در زمان بازبینی هنوز `in_progress` بود:
- Windows: clean outputs و build از خروجی پاک موفق؛ installer/publish artifact هنوز در حال تکمیل.
- HostLifecycle: build و acceptance، coverage و upload artifact با موفقیت تمام شده‌اند.
- Android: fixture .NET، clean Android outputs موفق؛ debug APK و مراحل بعدی هنوز در حال اجرا بودند.

وضعیت پایه `done` حفظ شد، اما تا تکمیل کامل run جاری و تأیید همهٔ سه job، `re_review_required` باقی می‌ماند. هیچ build محلی در این محیط اجرا نشد.

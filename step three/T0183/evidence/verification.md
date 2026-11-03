# T0183-A002 تا A005 — شواهد و اصلاح‌های KtLint

## snapshot و پیاده‌سازی
- آخرین commit: `ed8cca851bae251b8309ced5c2d8f96c22e996bc` (`V 3.0.850`).
- helper Android Keystore و migration identity/trust در A001 ایجاد شده‌اند؛ پنج instrumentation سناریو برای provider واقعی Android در workflow محصول متصل شده‌اند.
- یال‌های ورودی T0183 از قبل sequencing_only/validated هستند؛ یال خروجی به T0184 از نوع blocks و contract_or_interface_dependency/validated است.

## تلاش‌ها
- `V 3.0.848`, run `41673696991`: ۸ تخلف KtLint در `AndroidKeyStoreProtectedBlobProtector.kt` (اعلان‌ها، constructor، expression body و زنجیرهٔ Builder).
- `V 3.0.849`, run `41675040018`: main source اصلاح شد و KtLint به test source رسید؛ دو فایل JVM به‌خاطر ترتیب import و blank line خطا داشتند.
- `V 3.0.850`, run `41675745738`: Android job `126055750531` در main source دوباره متوقف شد؛ runner دو پیام مشخص ثبت کرد:
  - `AndroidKeyStoreProtectedBlobProtector.kt:100:36 Expected newline before '.'`
  - `AndroidKeyStoreProtectedBlobProtector.kt:104:21 Unexpected newline before '.'`
  اصلاح A005، `KeyGenParameterSpec.Builder` را به دو خط تقسیم و اولین `.setBlockModes` را به خط بسته‌شدن سازنده متصل می‌کند.
- Step Three quality gate run `41675744351` موفق است.
- KtLint و build APK و آزمون‌های JVM/instrumentation در run 850 موفق نیستند؛ به‌علت KtLint اجرا نشدند.

## وضعیت
A004 ترتیب importهای test source را اصلاح کرد، اما run 850 در main source پیش از رسیدن به test source متوقف شد؛ بنابراین اصلاح importها هنوز در CI تأیید نشده است. A005 در انتظار CI جدید است.

# شواهد و راستی‌آزمایی T0214

## مبنای بررسی

- شاخه: `main`
- commit پیش از این تلاش: `2d38b503ce9b894d35e9286c78e77b481f27ef15` (`V 3.0.993`)
- README و Product Build workflow همان commit از GitHub Blob API دریافت و بررسی شدند.

## اعتبارسنجی یال‌ها

### T0213 → T0214

- طبقه‌بندی: `sequencing_only`
- نوع اصلی: `blocks` حفظ شد.
- دلیل: دستورهای T0214 از checkout و toolchain مشخص‌شده در README اجرا می‌شوند؛ هیچ خروجی خاص T0213، از جمله APK امضاشده، Windows release ZIP، checksum، provenance یا Draft Release، ورودی این فرمان‌ها نیست.
- شواهد: `README.md`، `.github/workflows/product-build.yml`، `.github/workflows/release-assets.yml` و شواهد T0213.

### T0208 → T0214

- طبقه‌بندی: `sequencing_only`
- نوع اصلی: `blocks` حفظ شد.
- دلیل: T0208 گزارش‌های coverage/critical-test را به‌عنوان Artifact منتشر می‌کند؛ T0214 آزمون‌ها و buildهای خودش را از checkout جدید اجرا می‌کند و Artifactهای قبلی را مصرف نمی‌کند.
- شواهد: گزارش و verification مربوط به T0208، README و Product Build workflow.

## تغییرات اجرایی برای clone تمیز

- Windows job: اجرای صریح restore، clean، formatting، build، publish و بررسی وجود executable واقعی در `artifacts/windows-host/`.
- Windows publish output: بارگذاری Artifact مستقل `whitep-windows-publish`.
- HostLifecycle job: اجرای عین دستور مستندشدهٔ `dotnet run` پس از build پاک؛ جمع‌آوری coverage همچنان حفظ می‌شود.
- Android job و Quality Gate موجود همچنان فرمان‌های Gradle، Python، compileall و Graph Guard را از checkout تازه اجرا می‌کنند.

## آزمون‌ها

- آزمون‌های محلی در این محیط اجرا نشده‌اند.
- نتیجهٔ قطعی آزمون‌های source-contract و build باید از GitHub Actions روی commit همین تغییر استخراج شود.
- وضعیت acceptance تا بررسی Quality Gate، Product Build و محتوای Artifact برابر `pending` است.

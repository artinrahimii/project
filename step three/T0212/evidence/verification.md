# شواهد اجرای T0212

## بررسی مبنا

- Commit مبنا: `7ed118099b5ca594364839971f8d547e48c8c2ba` (`V 3.0.969`).
- Quality Gate همین مبنا: https://github.com/artinrahimii/project/actions/runs/42049765605 — ۴۲۳ آزمون Python موفق، compileall موفق، graph guard با ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطا و یک هشدار تاریخی T0001.
- T0211 و T0208 در گراف `done` بودند؛ هر دو ورودی T0212 پیش از تغییر، `unreviewed` بودند.

## طبقه‌بندی یال‌ها

- T0211 → T0212: `blocks` حفظ شد؛ `sequencing_only` / `validated`. شواهد: `src/Client.Android/app/build.gradle.kts`، `.github/workflows/product-build.yml` و گزارش T0211. workflow فعلی APK دیباگ را تولید می‌کند؛ signing جدید یک assembleRelease مستقل از همان source module دارد و APK دیباگ را ورودی نمی‌گیرد.
- T0208 → T0212: `blocks` حفظ شد؛ `sequencing_only` / `validated`. شواهد: `.github/workflows/product-build.yml`، `tests/validate_coverage_artifacts.py` و گزارش/evidence T0208. وظیفهٔ امضا به coverage artifact متکی نیست.

## آزمون‌های مورد انتظار برای commit پیاده‌سازی

- Quality Gate: تمام آزمون‌های Python، compileall، graph validation و recorded-output/evidence validation.
- Product Build: clean `assembleDebug` و clean `assembleRelease` با کلید موقت؛ `apksigner verify`; بررسی fingerprint دارای ۶۴ رقم hex؛ حذف keystore موقت حتی هنگام شکست.
- Production signed-release workflow: هنوز اجرا نشده؛ منتظر چهار GitHub Environment secret و متغیر fingerprint است.

## مرز پذیرش

وجود signing smoke key در CI به معنی ایجاد یا تأیید کلید تولیدی نیست. هیچ مقدار secret در گزارش یا مخزن ثبت نشده است. وضعیت `T0212` تا اجرای workflow محافظت‌شده با certificate واقعی، `in_progress` باقی می‌ماند.

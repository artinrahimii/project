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
## A002 — شواهد CI روی V 3.0.970

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42053031648 — ۴۲۹ آزمون کشف شد؛ یک failure در تست عبارت مستندات `outside the repository`.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42053032390 — Windows WPF و HostLifecycle موفق؛ Android job به علت KtLint ناموفق شد.
- Android job: https://github.com/artinrahimii/project/actions/runs/42053032390/job/127333902913 — `gradle clean` موفق؛ `ktlintKotlinScriptCheck` به علت import ordering و multiline formatting در خطوط ۱، ۲۲، ۳۱، ۴۳، ۴۷ و ۵۳ شکست خورد. امضای smoke اجرا نشد.
- Production workflow run: https://github.com/artinrahimii/project/actions/runs/42053028222 — صفر job؛ push شاخه، workflow دستی/tag-only را وارد job تولیدی نکرد.

## A003 — اصلاحات منتظر آزمون

- Gradle با قالب گزارش‌شدهٔ KtLint بازآرایی شد؛ رفتار task gating، canonical path validation و secret inputs تغییر معنایی نکرد.
- Product signing smoke به یک step واحد با متغیرهای local-to-step و `trap` پاک‌سازی تبدیل شد.
- مستندات مرز خارج checkout را روشن می‌کنند و با تست قراردادی هم‌خوان شده‌اند.
- نتیجهٔ CI اصلاحات در این commit هنوز pending است؛ production signing هنوز به Environment secrets و متغیر fingerprint نیاز دارد.
## A003 — Quality Gate run 42062003986

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42062003986 — ۴۲۹ آزمون؛ یک failure در تست canonical-path source assertion.
- پنج تست دیگر release signing، از جمله مستندات، fail-closed، disposable key cleanup marker، protected workflow/fingerprint و gitignore، موفق شدند.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42062004172 — HostLifecycle موفق؛ در زمان این گزارش Windows و Android همچنان in progress بودند.
- Android job پس از اصلاحات از KtLint عبور کرده بود و وارد مراحل build APK و signing smoke شده بود؛ نتیجهٔ نهایی آن هنوز تأیید نشده است.

## A004 — اصلاح assertion بدون وابستگی به قالب‌بندی

- تست مسیر repositoryRoot اکنون ترتیب resolve → canonicalFile → toPath را بررسی می‌کند و دیگر عبارت را به‌صورت یک خط ثابت طلب نمی‌کند.
- اجرای دوبارهٔ Quality Gate و پایان Product Build لازم است؛ هیچ نتیجهٔ قبولی پیشاپیش ادعا نمی‌شود.

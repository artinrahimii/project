# بازبینی T0208 — coverage و critical-test artifacts در CI

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916`، `V 3.1.104`
- پذیرش تاریخی: Product Build run `41930272240` و Quality/Host runs مرتبط
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- UPDهای مرتبط: UPD-S1-019، UPD-S1-021

## پیاده‌سازی فعلی بررسی‌شده

- `tests/validate_coverage_artifacts.py` Cobertura line counters، HTML report کامل، JUnit totals و critical-log PASS markers را parse و اعتبارسنجی می‌کند.
- آزمون‌های validator، صفر executable lines، HTML ناقص، شمارش passed/skipped/failure و نبود pass marker را رد می‌کنند.
- Product workflow گزارش‌های Android unit/instrumentation/coverage و HostLifecycle coverage/critical logs را upload می‌کند؛ failure/error در JUnit نباید به success خلاصه شود.

## شواهد تاریخی منتشرشده

در run تاریخی 41930272240، artifactهای واقعی زیر ثبت شده‌اند:
- Android: 34 JUnit files، مجموع 224 مورد: 223 passed، یک skipped، صفر failure/error؛ دو HTML coverage report معتبر.
- Windows HostLifecycle: Cobertura شامل 8,849 از 10,325 خط (85.70%) و 67 PASS marker، یک SKIP.
- Android APK، unit-test reports، instrumentation reports، coverage summary و HostLifecycle coverage/critical-results در artifactهای مجزا بارگذاری شده‌اند.
- skipها شامل Android physical LAN و real-model opt-in بوده‌اند؛ به‌عنوان pass ثبت نشده‌اند.

## اجرای جدید روی main جاری

Product Build run [42515602200](https://github.com/artinrahimii/project/actions/runs/42515602200) در زمان بازبینی هنوز `in_progress` بود:
- HostLifecycle artifact `whitep-host-lifecycle-coverage-and-critical-results` با اندازهٔ 111,852 بایت موجود و منقضی‌نشده بود.
- Windows job هنوز به پایان installer/artifact upload نرسیده بود.
- Android coverage validation و upload هنوز در صف اجرا بودند.

وضعیت پایه `done` حفظ شد؛ اما به‌دلیل تغییر workflow/Gradle نسبت به اجرای تاریخی و ناتمام‌بودن run جاری، `re_review_required` ثبت می‌شود. artifactهای تاریخی جایگزین artifactهای exact-current-head نیستند.

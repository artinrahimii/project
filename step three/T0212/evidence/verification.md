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
## A004 — Quality Gate موفق در V 3.0.972

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42062471023 — ۴۲۹ آزمون موفق؛ compileall و graph checks موفق؛ ۲۳۰ وظیفه/۴۲۲ یال، صفر خطای گراف، یک هشدار تاریخی T0001.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42062471014 — هنگام گزارش A005 هنوز در حال اجرا بود؛ HostLifecycle موفق شده بود.

## A005 — اصلاح import order

- لاگ Product Build قبلی: https://github.com/artinrahimii/project/actions/runs/42062004172 — KtLint اعلام کرد importهای java/javax/kotlin باید در انتهای لیست مرتب‌شده باشند.
- importها در Gradle به ترتیب com، org، java بازچینی شدند؛ نتیجهٔ KtLint این اصلاح هنوز pending است.
- build امضاشدهٔ موقت و apksigner باید در Product Build جدید تأیید شوند؛ امضای production هنوز اجرا نشده است.


## A006 — شواهد نهایی CI روی V 3.0.973

- Quality Gate [42063282020](https://github.com/artinrahimii/project/actions/runs/42063282020): ۴۲۹ آزمون موفق، compileall موفق، graph validation با ۲۳۰ وظیفه/۴۲۲ یال، صفر خطا و یک هشدار تاریخی T0001.
- Product Build [42063281545](https://github.com/artinrahimii/project/actions/runs/42063281545): نتیجهٔ کلی success.
- Android job [127370445148](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445148): KtLint، assembleDebug، assembleRelease با keystore موقت، apksigner verify، cleanup و حداقل مجوزها موفق؛ JVM: ۲۲۴ تست، ۲۲۳ passed، یک skipped، صفر failure/error؛ instrumentation: ۱۲ تست خاتمه‌یافته، LAN integration به‌صورت صریح skipped.
- Windows job [127370445471](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445471): ۲۲/۲۲ مرحله موفق، installer و host artifacts بارگذاری شدند.
- HostLifecycle job [127370445384](https://github.com/artinrahimii/project/actions/runs/42063281545/job/127370445384): ۱۳/۱۳ مرحله موفق؛ Cobertura با ۸۵٫۷۰٪ پوشش خطی، ۶۷ PASS و یک SKIP مربوط به real-model opt-in.
- Artifactهای run: [instrumentation 13434575334](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13434575334)، [JVM 13433990937](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13433990937)، [debug APK 13433781042](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13433781042)، [Windows installer 13432905703](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13432905703)، [Android coverage 13432679732](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13432679732)، [HostLifecycle coverage 13432588457](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13432588457) و [Windows host 13431974161](https://github.com/artinrahimii/project/actions/runs/42063281545/artifacts/13431974161).

### محدودیت باقی‌مانده
artifact مربوط به release APK تولیدی در این run ساخته/بارگذاری نشد؛ APK امضاشدهٔ این run فقط smoke با کلید موقت بود. workflow تولیدی در push job نداشت و secret/fingerprint واقعی از طریق اتصال فعلی قابل تأیید یا dispatch نبود. T0212 به‌درستی in_progress باقی می‌ماند.


## A006 — بازاعتبارسنجی تکمیلی روی V 3.0.974

- Quality Gate [42065762873](https://github.com/artinrahimii/project/actions/runs/42065762873): موفق؛ ۴۲۹ آزمون Python، compileall و Graph Guard (۲۳۰ وظیفه/۴۲۲ یال) موفق.
- Product Build [42065762818](https://github.com/artinrahimii/project/actions/runs/42065762818): هر سه job اندروید، ویندوز و HostLifecycle موفق شدند.
- Android job [127379261869](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379261869): KtLint، assembleDebug، assembleRelease با JKS موقت و apksigner verification موفق؛ digest گواهی `b7fe6f8501d232d9550f194848c9ce1a32b337beada9e1f4fa72a4a6dedbbba2` فقط fingerprint کلید موقت smoke است. آزمون JVM: ۲۲۳ passed، یک skipped، صفر failure/error.
- Windows job [127379262443](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379262443): startup smoke و installer acceptance موفق؛ هشدار غیرمانع metadata در PE گزارش شد.
- HostLifecycle job [127379262455](https://github.com/artinrahimii/project/actions/runs/42065762818/job/127379262455): ۸٬۸۴۸/۱۰٬۳۲۵ خط (۸۵٫۶۹٪)، ۶۷ PASS و یک opt-in SKIP.
- Artifactهای اندروید/ویندوز و گزارش‌های آزمون بارگذاری شدند؛ فهرست شناسه‌ها در execution-report.md ثبت شده است.
- این اجرا، امضای تولیدی را انجام نداد و APK تولیدی fingerprint-matched منتشر نکرد. T0212 در `in_progress` باقی می‌ماند؛ T0213 هنوز به‌علت hard dependency شروع نشده است.

## A007 — بازاعتبارسنجی سرخط V 3.0.975

- Commit: `fa69ec6ee58998801e22e4811e331f4dc2ac6f7a`، پیام `V 3.0.975`.
- Quality Gate: run `42068538634` با همهٔ مراحل موفق.
- Product Build: run `42068534197` با هر سه job موفق؛ Android Release فقط با کلید موقت CI امضا و با `apksigner` بررسی شد. این گواهی برای توزیع یا به‌روزرسانی محصول نیست.
- Production signing: run `42068530991` از نوع push با صفر job پایان یافت؛ بنابراین هیچ بررسی secret یا تطبیق fingerprint تولیدی انجام نشد.
- workflow تولیدی فقط با `workflow_dispatch` یا tagهای `android-v*` اجرا می‌شود. ابزار متصل فعلی نه قابلیت dispatch و نه مدیریت secretهای Environment را دارد؛ وضعیت وجود secretها نامعلوم باقی می‌ماند.
- تصمیم: T0212 مسدود است تا maintainer Environment محافظت‌شده را پیکربندی/تأیید و workflow را اجرا کند. T0213 به‌دلیل hard dependency آغاز نشده است. هیچ کلید، رمز یا دادهٔ محرمانه‌ای وارد شواهد نشده است.
- شواهد: [Quality Gate](https://github.com/artinrahimii/project/actions/runs/42068538634)، [Product Build](https://github.com/artinrahimii/project/actions/runs/42068534197)، [Android signed release workflow](https://github.com/artinrahimii/project/actions/runs/42068530991).

## A008 — آزمون‌های V 3.0.977

- commit مبنا: `441a058c2461b049a74faba414dbc0ba7aa3eb49` (`V 3.0.977`).
- Quality Gate run `42071006532`: موفق.
- Product Build run `42071006454`: هر سه job Android، Windows WPF/installer و HostLifecycle موفق؛ Android امضای Release فقط با کلید دورریختنی CI را آزمود.
- Rendezvous container run `42071006474`: موفق.
- Android signed release run `42071003708`: event از نوع push، نتیجهٔ failure و صفر job؛ هیچ بررسی یا امضای production انجام نشد.
- workflow امضای تولیدی فقط `workflow_dispatch` و tagهای `android-v*` را تعریف می‌کند. ابزار متصل، dispatch یا مدیریت Environment Secret را پشتیبانی نمی‌کند و وضعیت وجود Secretها نامعلوم است.
- تصمیم: T0212 مسدود باقی ماند؛ T0213 به‌دلیل hard dependency آغاز نشد. هیچ یال یا قرارداد تغییر نکرد و هیچ دادهٔ محرمانه‌ای ثبت نشد.

# گزارش اجرای UPD-S1-006 — هم‌ترازی نیازمندی‌ها و معیارهای پذیرش

## وضعیت این تلاش
- وضعیت: `in_progress` تا پایان quality gate روی commit دقیق این تغییر.
- تلاش: `UPD-S1-006-20261108111009`
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit مبنا: `ed3ebe09f8555f247a23a7abc67d7234a06e45d2` — `V 3.1.44`
- زمان آغاز: `2026-11-08T11:10:09.175Z`
- نوع تغییر: `modify`؛ اصلاح اسناد و قراردادهای قابل‌آزمون، بدون ادعای پیاده‌سازی قابلیت‌های UI یا تغییر runtime.

## تصمیم‌های مصوبی که به نیازمندی‌ها منتقل شدند
1. Direct authenticated P2P مسیر دادهٔ inference است؛ rendezvous فقط metadata دارد و fallback محتوایی خاموش و fail-closed است (ADR-0002، UPD-S1-002).
2. runtime نسخهٔ نخست در هر round فقط یک میزبان را dispatch می‌کند؛ حداکثر سه تلاش ترتیبی در سه round، retry صریح کاربر، deadline مشترک و cancellation terminal (ADR-0022، UPD-S1-003).
3. پاسخ نسخهٔ نخست یک پاسخ کامل terminal است؛ streaming قطعه‌ای deferred است و از capability flag فعال نمی‌شود (ADR-0023، UPD-S1-004).
4. رضایت میزبان و محدودیت‌های واقعی منابع، readiness و ظرفیت، و سقف یک inference فعال حفظ می‌شوند (ADR-0012 و خروجی UPD-S1-005).
5. تجربهٔ عادی Android به‌عنوان مسیر نهایی گفت‌وگو تعریف شده است: lifecycle محلی و Room حفظ می‌شود و پاسخ پذیرفته‌شده باید در همان conversation ذخیره شود. این الزام به‌عنوان شکاف اجرایی با معیار مستقل ثبت شده است.
6. هویت بصری مشترک Windows/Android، آیکون P و پنج توکن دقیق به معیارهای قابل‌آزمون و بصری تبدیل شدند؛ سند به‌تنهایی پیاده‌سازی نیست.

## فایل‌های تغییر‌یافته
- `docs/requirements.md`
- `docs/acceptance.md`
- `step one/V1.0.md`
- `tests/test_acceptance_contract.py`
- `tests/test_update_step_one_alignment.py`
- `update/Step three/UPD-S1-006/execution-report.md`
- `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`
- `update/Step two/project-graph.md`

## کنترل حفظ گراف اولیه
- فهرست checkboxهای `Txxxx` در `step one/V1.0.md` دارای ۲۲۹ شناسهٔ یکتا است.
- آزمون جدید اثرانگشت فهرست مرتب‌شده را با مقدار `75c65d66c98bd5bd295617fde5098d7ccdff9059` تثبیت می‌کند.
- الحاقیه فقط تصمیم‌ها و پیوندهای traceability را اضافه می‌کند؛ هیچ وظیفهٔ قدیمی حذف یا شماره‌گذاری مجدد نشده و وضعیت checkboxها تغییر نکرده است.

## آزمون‌ها
- `python -m unittest discover -s tests -p "test_update_step_one_alignment.py" -v` — pending اجرای CI روی commit دقیق.
- `python -m unittest discover -s tests -p "test_requirements_contract.py" -v` — pending.
- `python -m unittest discover -s tests -p "test_acceptance_contract.py" -v` — pending.
- `python -m unittest discover -s tests -v` — pending.
- graph_guard برای هر دو گراف، با و بدون `--repo-root .` — pending.
- Windows/Android product build روی commit دقیق — pending.

## محدودیت‌ها و عدم ادعا
- UI گفت‌وگوی عادی Android در خط مبنای ممیزی به inference runtime متصل نبود؛ این گزارش آن را پیاده‌سازی‌شده اعلام نمی‌کند.
- آیکون، palette، screenshots و contrast بررسی‌شدهٔ UI در این وظیفه تولید یا تأیید نشده‌اند؛ این‌ها خروجی اجرایی UPD-S1-024 هستند.
- آزمون فیزیکی Android-to-Windows، clean install و in-place upgrade در این تلاش اجرا نشده‌اند.
- وضعیت نهایی این وظیفه فقط پس از موفقیت آزمون‌های دقیق commit و بررسی کامل پوشش تصمیم‌ها قابل تغییر به `done` است.


## اعتبارسنجی دقیق commit V 3.1.45 — شکست قرارداد متنی و اصلاح

- Commit: `fc36774e3349558650e764476b70a721e4ed63a1` — `V 3.1.45`.
- Step Three quality run: [42474633661](https://github.com/artinrahimii/project/actions/runs/42474633661) — **FAIL**؛ ۴۷۹ آزمون اجرا شد و یک آزمون ناموفق بود.
- آزمون ناموفق: `test_claims_audit.ClaimsAuditTests.test_acceptance_document_matches_existing_platform_projects`.
- علت دقیق: آزمون، وجود عبارت تاریخی `does not establish real local inference` را در `docs/acceptance.md` الزام می‌کرد. بازنویسی معرفی معیار پذیرش این عبارت را حذف کرد، در حالی که معنای محدودیت همچنان معتبر است.
- اصلاح: عبارت به‌صورت دقیق و محدود بازگردانده شد: آزمون hosted local-inference، اجرای مدل واقعی روی میزبان Windows نصب‌شدهٔ کاربر یا مسیر گفت‌وگوی فیزیکی Android-to-Windows را اثبات نمی‌کند. هیچ معیار محصولی حذف یا آسان نشده است.
- build اندروید، build ویندوز/HostLifecycle و rendezvous در زمان ثبت این گزارش هنوز نتیجهٔ نهایی نداشتند؛ وضعیت آن‌ها جداگانه بررسی می‌شود.
- این اصلاح فقط خطای قرارداد متنی را هدف می‌گیرد. `UPD-S1-006` تا موفقیت کامل quality gate روی commit اصلاحی همچنان `in_progress` است.

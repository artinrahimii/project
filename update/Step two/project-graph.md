# گراف وابستگی و دفتر اجرای به‌روزرسانی White-P — v1.0

## هدف

این فایل گراف اجرایی ۲۴ وظیفهٔ نسخهٔ نهایی نیازمندی‌های به‌روزرسانی `update/Step one/Output/V1.10.json` است. ساختار آن از گراف `step two/v2.1.md` الگو می‌گیرد، اما دامنه، شناسه‌ها، وابستگی‌ها و خروجی‌ها مختص به‌روزرسانی محصول موجود هستند؛ هدف، تکمیل عملی کل محصول Android + Windows + اجزای مشترک است، نه بازسازی پروژه از ابتدا.

## مبنای مخزن و منبع وظایف

- شاخه: `main`
- کامیت مبنا: `8fa9aa812aeb49086a6d55e92ba17586534135a4` — `V 3.1.11`
- منبع وظایف: `update/Step one/Output/V1.10.json` — نسخهٔ `V1.10` — ۲۴ وظیفه
- سند اصلی پروژه برای نگاشت شناسه‌های قدیمی: `step one/V1.0.md` (۲۲۹ شناسهٔ Txxxx).
- فایل مرجع گراف: `update/Step two/project-graph.md`.

## معنی یال‌ها و وضعیت‌ها

- یال `A → B` یعنی A پیش‌نیاز B است و باید پیش از تکمیل B انجام و تأیید شود.
- وضعیت‌ها: `not_started`، `in_progress`، `done` و `blocked`.
- `done` فقط با فایل واقعی، آزمون موفق، معیار پذیرش پاس‌شده و شواهد commit/CI/device مجاز است.
- `expected_output_paths` مسیر هدف است؛ `actual_output_paths` تا زمان بررسی وجود فایل واقعی خالی می‌ماند.
- هر تلاش، حتی شکست‌خورده یا ناقص، باید در `execution_history` افزوده شود؛ سابقه پاک یا جایگزین نمی‌شود.

## تفاوت این گراف با گراف ساخت نسخهٔ اولیه

گراف `step two/v2.1.md` از ۲۲۹ وظیفهٔ ساخت اولیه ساخته شده است. این گراف فقط ۲۴ وظیفهٔ بازنگری V1.10 را دارد؛ هر گره علاوه بر شناسهٔ جدید `UPD-S1-xxx`، `change_operation` و `affected_task_ids` را برای پیوند به وظایف قدیمی حفظ می‌کند. شناسه‌های Txxxx در این گراف گره مستقل نیستند.

## سیاست وابستگی و ترتیب اجرا

پیش‌نیازها فقط از آرایهٔ `prerequisites` فایل V1.10 استخراج شده‌اند؛ هیچ یال ترتیبی ساختگی اضافه نشده است. همهٔ یال‌ها در شروع `unreviewed` هستند و پیش از اجرای گسترده باید با کد، قراردادها و آزمون‌ها طبقه‌بندی شوند. `execution_order` یک ترتیب توپولوژیک امن است و `execution_waves` گروه‌های بالقوهٔ موازی را نشان می‌دهد؛ اجرای موازی فقط وقتی مجاز است که مالکیت فایل/قرارداد مشترک و ظرفیت بازبینی روشن باشد.

## بخش‌های کاری

- **1. ممیزی خط مبنا و تصمیم‌های پایه** — UPD-S1-001 تا UPD-S1-005؛ 5 وظیفه. تثبیت وضعیت واقعی کد و تصمیم‌های محصول/معماری قبل از تغییر اجرایی.
- **2. تثبیت نیازمندی‌ها و نقشهٔ اثر تغییر** — UPD-S1-006 تا UPD-S1-008؛ 3 وظیفه. اعمال تصمیم‌های تأییدشده به سند مرجع و نگاشت اثر آن‌ها به گراف و اجرای پروژه.
- **3. تکمیل تجربهٔ چت و رابط Android** — UPD-S1-009 تا UPD-S1-011؛ 3 وظیفه. استفاده از UI و Room موجود و تکمیل اتصال واقعی درخواست/پاسخ، رضایت و نمایش خطاها.
- **4. تکمیل میزبان Windows و کنترل منابع** — UPD-S1-012 تا UPD-S1-013؛ 2 وظیفه. پیاده‌سازی مشارکت اختیاری، readiness و محدودیت منابع روی میزبان واقعی Windows.
- **5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی** — UPD-S1-014 تا UPD-S1-018؛ 5 وظیفه. اتصال مؤلفه‌های موجود Android/Windows با قرارداد نسخه‌دار، انتخاب و retry ایمن، انتقال و حفاظت داده.
- **6. آزمون یکپارچه و تحویل نصب‌پذیر** — UPD-S1-019 تا UPD-S1-021؛ 3 وظیفه. اثبات رفتار با آزمون‌های خودکار و دستگاه واقعی؛ سپس بسته‌بندی و تحویل دو برنامه.
- **7. قابلیت‌های مشترک اعتماد، مدل و هویت بصری** — UPD-S1-022 تا UPD-S1-024؛ 3 وظیفه. تکمیل قواعد مشترکی که مصرف‌کنندگان Android و Windows به آن وابسته‌اند؛ این وظایف در execution_order پیش از مصرف‌کنندگان اجرا می‌شوند، حتی اگر در فایل منبع بعدتر آمده باشند.

## شرط نتیجهٔ عملی

این گراف زمانی به نتیجهٔ محصولی رسیده محسوب می‌شود که:

1. کلاینت Android روی دستگاه واقعی نصب و اجرا شود، گفت‌وگوی موجود را باز کند، پیام عادی را بدون ورود شناسهٔ مدل/IP/Port/PeerId بفرستد و پاسخ واقعی میزبان را در همان گفتگو نشان دهد.
2. میزبان Windows روی دستگاه واقعی اجرا شود، مشارکت آن با رضایت و به‌صورت پیش‌فرض خاموش کنترل شود، مدل و backend واقعی را آماده تشخیص دهد و درخواست واقعی را پردازش کند.
3. مسیر واقعی Android-to-Windows، امنیت/اعتماد، خطا، لغو و retry آزموده شوند؛ rendezvous فقط metadata ببیند و محتوای inference را دریافت نکند.
4. آیکون سکهٔ P و پنج توکن رنگی در هر دو برنامه واقعاً اعمال شوند و کنتراست و تصاویر در اندازه‌های هدف بررسی شود.
5. build، آزمون‌های خودکار، آزمون دستگاه واقعی و بسته‌های نصب‌پذیر هر دو پلتفرم با شواهد ثبت شوند. سند، mock، stub یا CI سبز به‌تنهایی کافی نیست.

## قالب ویرایش

دادهٔ کامل در بلوک JSON زیر، قابل پردازش ماشینی و سازگار با `tools/graph_guard.py` است. هنگام به‌روزرسانی، ساختار JSON را حفظ کن و فقط وضعیت‌ها/تلاش‌ها/یال‌های دارای شاهد را به‌روزرسانی کن.

## دادهٔ کامل گراف v1.0

```json
{
  "schema_version": "1.0",
  "document": "update-step-two-v1.0",
  "title": "گراف وابستگی و دفتر اجرای به‌روزرسانی White-P بر پایهٔ V1.10",
  "created_at": "2026-11-07",
  "source": {
    "path": "update/Step one/Output/V1.10.json",
    "revision": "V1.10",
    "source_commit_sha": "8fa9aa812aeb49086a6d55e92ba17586534135a4",
    "source_blob_sha": "5cea3ec095d0c00e32ed3e067796ff698aedd982",
    "source_status": "approved_and_closed",
    "task_count": 24,
    "parent_product_revision": "main"
  },
  "base_repository": {
    "repository": "artinrahimii/project",
    "branch": "main",
    "base_commit_sha": "8fa9aa812aeb49086a6d55e92ba17586534135a4",
    "base_commit_message": "V 3.1.11",
    "purpose": "Update graph for the current integrated Windows host, Android client and shared components; not a plan to recreate the initial product."
  },
  "commit_intent": "V 3.1.12",
  "status_legend": {
    "not_started": "انجام‌نشده؛ هیچ تلاش اجرایی ثبت نشده است.",
    "in_progress": "در حال اجرا؛ دست‌کم یک تلاش آغاز شده اما معیارهای پذیرش کامل نشده‌اند.",
    "done": "انجام‌شده؛ فقط با کد/خروجی واقعی، آزمون موفق، actual_output_paths معتبر و شواهد قابل بازبینی.",
    "blocked": "مسدود؛ علت، پیش‌نیاز/دسترسی مفقود و شرط رفع مانع باید ثبت شود."
  },
  "graph_policy": {
    "direction": "A -> B یعنی A پیش‌نیاز B است و باید پیش از تکمیل B تأیید شود.",
    "source_of_truth": "شرح، ورودی، خروجی، پیش‌نیاز و معیار پذیرش هر وظیفه از V1.10 کپی شده است. در صورت اختلاف، V1.10 سند مالک وظیفه است و این گراف باید با ثبت تغییر به‌روز شود.",
    "dependency_rule": "Edges are generated only from explicit prerequisites in V1.10. They start as unreviewed; before broad execution classify each as hard_dependency, contract_or_interface_dependency, or sequencing_only using repository evidence and tests. Do not invent serial edges or remove an explicit prerequisite merely to increase parallelism.",
    "completion_rule": "ساخت گزارش، تصمیم یا سند به‌تنهایی تکمیل محصول نیست. برای گره‌های پیاده‌سازی، کد واقعی Android/Windows/shared، آزمون متناظر و شواهد پذیرش لازم است. build موفق به‌تنهایی اثبات inference زنده یا Android-to-Windows end-to-end نیست.",
    "status_truth": "تمام 24 گره در زمان ایجاد این گراف not_started هستند. وجود کد قبلی در مخزن به معنی تکمیل وظیفهٔ به‌روزرسانی نیست؛ ابتدا باید gap ممیزی و سپس تغییر موردنیاز و پذیرش همان دامنه اثبات شود.",
    "actual_paths_rule": "expected_output_paths فقط مسیر هدف است. actual_output_paths تا زمان وجود و بررسی فایل واقعی در commit مشخص، خالی می‌ماند.",
    "execution_tracking": "Every attempt is append-only. Record changed files, commit SHA or explicit no-commit, exact test commands/results, failures, blockers and contract revalidation. Never replace earlier attempts.",
    "source_traceability": "Each node preserves source_task_id, change_operation and affected_task_ids from V1.10. References to legacy Txxxx IDs are traceability metadata, not nodes in this update graph.",
    "preserve_existing_work": "Audit and reuse existing Room storage, Android conversation UI, host/backend, protocol, libp2p, trust and test infrastructure. Do not recreate a component merely because it is mentioned in an update task; only close verified gaps.",
    "security_and_privacy": "Do not expose prompt/response/history to rendezvous or normal logs. The destination inference host can read the prompt and context it processes; the Android user must be shown the trust/privacy consent required by V1.10.",
    "no_mock_completion": "Mocks may support isolated tests only. A mocked response, hard-coded response, sample-only code or stub cannot satisfy a real product acceptance gate."
  },
  "sections": [
    {
      "id": "S1",
      "name": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "description": "تثبیت وضعیت واقعی کد و تصمیم‌های محصول/معماری قبل از تغییر اجرایی.",
      "first_task": "UPD-S1-001",
      "last_task": "UPD-S1-005",
      "task_ids": [
        "UPD-S1-001",
        "UPD-S1-002",
        "UPD-S1-003",
        "UPD-S1-004",
        "UPD-S1-005"
      ],
      "task_count": 5,
      "depends_on_section": null,
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S2",
      "name": "2. تثبیت نیازمندی‌ها و نقشهٔ اثر تغییر",
      "description": "اعمال تصمیم‌های تأییدشده به سند مرجع و نگاشت اثر آن‌ها به گراف و اجرای پروژه.",
      "first_task": "UPD-S1-006",
      "last_task": "UPD-S1-008",
      "task_ids": [
        "UPD-S1-006",
        "UPD-S1-007",
        "UPD-S1-008"
      ],
      "task_count": 3,
      "depends_on_section": "S1",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S3",
      "name": "3. تکمیل تجربهٔ چت و رابط Android",
      "description": "استفاده از UI و Room موجود و تکمیل اتصال واقعی درخواست/پاسخ، رضایت و نمایش خطاها.",
      "first_task": "UPD-S1-009",
      "last_task": "UPD-S1-011",
      "task_ids": [
        "UPD-S1-009",
        "UPD-S1-010",
        "UPD-S1-011"
      ],
      "task_count": 3,
      "depends_on_section": "S2",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S4",
      "name": "4. تکمیل میزبان Windows و کنترل منابع",
      "description": "پیاده‌سازی مشارکت اختیاری، readiness و محدودیت منابع روی میزبان واقعی Windows.",
      "first_task": "UPD-S1-012",
      "last_task": "UPD-S1-013",
      "task_ids": [
        "UPD-S1-012",
        "UPD-S1-013"
      ],
      "task_count": 2,
      "depends_on_section": "S2",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S5",
      "name": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "description": "اتصال مؤلفه‌های موجود Android/Windows با قرارداد نسخه‌دار، انتخاب و retry ایمن، انتقال و حفاظت داده.",
      "first_task": "UPD-S1-014",
      "last_task": "UPD-S1-018",
      "task_ids": [
        "UPD-S1-014",
        "UPD-S1-015",
        "UPD-S1-016",
        "UPD-S1-017",
        "UPD-S1-018"
      ],
      "task_count": 5,
      "depends_on_section": "S2",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S6",
      "name": "6. آزمون یکپارچه و تحویل نصب‌پذیر",
      "description": "اثبات رفتار با آزمون‌های خودکار و دستگاه واقعی؛ سپس بسته‌بندی و تحویل دو برنامه.",
      "first_task": "UPD-S1-019",
      "last_task": "UPD-S1-021",
      "task_ids": [
        "UPD-S1-019",
        "UPD-S1-020",
        "UPD-S1-021"
      ],
      "task_count": 3,
      "depends_on_section": "S3–S5 و S7",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    },
    {
      "id": "S7",
      "name": "7. قابلیت‌های مشترک اعتماد، مدل و هویت بصری",
      "description": "تکمیل قواعد مشترکی که مصرف‌کنندگان Android و Windows به آن وابسته‌اند؛ این وظایف در execution_order پیش از مصرف‌کنندگان اجرا می‌شوند، حتی اگر در فایل منبع بعدتر آمده باشند.",
      "first_task": "UPD-S1-022",
      "last_task": "UPD-S1-024",
      "task_ids": [
        "UPD-S1-022",
        "UPD-S1-023",
        "UPD-S1-024"
      ],
      "task_count": 3,
      "depends_on_section": "S1–S2",
      "note": "وابستگی واقعی در edges و prerequisites هر گره مشخص است؛ این grouping به‌تنهایی یال فنی اضافه نمی‌کند."
    }
  ],
  "nodes": [
    {
      "id": "UPD-S1-001",
      "title": "بررسی خط مبنای واقعی برنامه‌ها",
      "section": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "source_task_id": "UPD-S1-001",
      "source_change_operation": "preserve",
      "affected_task_ids": [
        "T0015",
        "T0016",
        "T0041",
        "T0042",
        "T0139",
        "T0216",
        "T0217"
      ],
      "description": "ساختار و قابلیت‌های موجود اپلیکیشن اندروید، میزبان ویندوز، قراردادهای مشترک و مسیر ارتباطی را در آخرین کامیت بررسی کن. این وظیفه فقط وضعیت موجود را ثبت می‌کند و مجاز به فرض وجود قابلیت‌های صرفاً مستندشده نیست.",
      "depends_on": [],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-001",
        "کامیت مبنای main با SHA برابر 6f70586b4b7ff69c463f7e5b02293090c63e78ec",
        "کد و فایل‌های build پروژه",
        "step one/V1.0.md",
        "گزارش update/Step one/2026-11-07-automatic-chat-and-host-routing.md"
      ],
      "outputs": [
        "گزارش موجودی قابلیت‌های واقعی در Android و Windows",
        "نقشهٔ مسیر فعلی پیام، کشف میزبان و inference",
        "دستورهای build/test موجود و نتیجهٔ اجرای واقعی یا دلیل اجرا نشدن آن‌ها",
        "فهرست فاصله‌های کد با نیازمندی پیشنهادی"
      ],
      "implementation_focus": "ممیزی واقعی و بدون فرض وضعیت فعلی؛ خروجی این گره نقشهٔ مسیر و شواهد خط مبناست.",
      "start_condition": "نسخهٔ مبنای main و فایل V1.10 باید با SHA ثبت‌شده یکسان باشند؛ ممیزی باید با کد و فایل‌های واقعی مخزن انجام شود.",
      "acceptance": "1. مسیر واقعی پروژه‌های Android، Windows و Shared از روی مخزن مشخص شده باشد.\n2. جریان فعلی درخواست از UI تا میزبان و بازگشت پاسخ با ارجاع به فایل‌ها ثبت شده باشد.\n3. هر build یا test فقط در صورت اجرای واقعی دارای فرمان و نتیجه باشد.\n4. موارد ناموجود یا اثبات‌نشده صریحاً از قابلیت‌های پیاده‌سازی‌شده جدا شده باشند.",
      "acceptance_criteria": [
        "مسیر واقعی پروژه‌های Android، Windows و Shared از روی مخزن مشخص شده باشد.",
        "جریان فعلی درخواست از UI تا میزبان و بازگشت پاسخ با ارجاع به فایل‌ها ثبت شده باشد.",
        "هر build یا test فقط در صورت اجرای واقعی دارای فرمان و نتیجه باشد.",
        "موارد ناموجود یا اثبات‌نشده صریحاً از قابلیت‌های پیاده‌سازی‌شده جدا شده باشند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=preserve; affected_task_ids=T0015, T0016, T0041, T0042, T0139, T0216, T0217. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-002",
      "title": "تصمیم‌گیری دربارهٔ مسیر انتقال و مرز اعتماد",
      "section": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "source_task_id": "UPD-S1-002",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0019",
        "T0118",
        "T0119",
        "T0120",
        "T0129",
        "T0130",
        "T0131",
        "T0132",
        "T0133",
        "T0134",
        "T0206",
        "T0226",
        "T0227"
      ],
      "description": "مسیر ارتباطی نسخهٔ هدف را بین اتصال مستقیم P2P، Relay رمزگذاری‌شده یا مسیر مدیریت‌شدهٔ دیگر تعیین کن. تصمیم باید روشن کند هماهنگ‌کننده چه فراداده‌ای می‌بیند و آیا هر جزء واسط می‌تواند متن روشن درخواست یا پاسخ را بخواند.",
      "depends_on": [
        "UPD-S1-001"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-002",
        "گزارش بازنگری، به‌ویژه بخش‌های 5 و 8.1",
        "T0009 و T0019",
        "T0118 تا T0120",
        "T0206 و T0227",
        "وضعیت واقعی transport در کامیت مبنا",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001"
      ],
      "outputs": [
        "سند تصمیم معماری انتقال",
        "تصمیم صریح دربارهٔ مسیر نسخهٔ اولیه و مسیر جایگزین",
        "تعریف داده‌های مجاز برای هماهنگ‌کننده و Relay",
        "فهرست گزینه‌های ردشده و دلیل رد آن‌ها"
      ],
      "implementation_focus": "خروجی باید در فایل‌های اجرایی واقعی و آزمون‌های متناظر منعکس شود؛ گزارش به‌تنهایی کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. یک مسیر اصلی برای نسخهٔ هدف و یک سیاست روشن برای مسیر جایگزین انتخاب شده باشد.\n2. تصمیم به‌صراحت تضمین کند سرویس واسط متن روشن را دریافت یا ذخیره نمی‌کند، مگر آنکه تغییر سیاست حریم خصوصی جداگانه و صریحاً تصویب شده باشد.\n3. فرادادهٔ قابل مشاهده برای هر جزء و محدودیت نگهداری آن ثبت شده باشد.\n4. تصمیم با T0009، T0019، T0118 تا T0120، T0206 و T0227 تطبیق داده شده باشد.",
      "acceptance_criteria": [
        "یک مسیر اصلی برای نسخهٔ هدف و یک سیاست روشن برای مسیر جایگزین انتخاب شده باشد.",
        "تصمیم به‌صراحت تضمین کند سرویس واسط متن روشن را دریافت یا ذخیره نمی‌کند، مگر آنکه تغییر سیاست حریم خصوصی جداگانه و صریحاً تصویب شده باشد.",
        "فرادادهٔ قابل مشاهده برای هر جزء و محدودیت نگهداری آن ثبت شده باشد.",
        "تصمیم با T0009، T0019، T0118 تا T0120، T0206 و T0227 تطبیق داده شده باشد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "docs/decisions/",
        "update/Step three/UPD-S1-002/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0019, T0118, T0119, T0120, T0129, T0130, T0131, T0132, T0133, T0134, T0206, T0226, T0227. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-003",
      "title": "تصمیم‌گیری دربارهٔ تعداد میزبان و سیاست تلاش مجدد",
      "section": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "source_task_id": "UPD-S1-003",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0125",
        "T0126",
        "T0161",
        "T0162",
        "T0163",
        "T0164",
        "T0165",
        "T0166",
        "T0167",
        "T0168",
        "T0169",
        "T0170",
        "T0171",
        "T0172",
        "T0173",
        "T0174",
        "T0175",
        "T0176",
        "T0177",
        "T0203",
        "T0204",
        "T0224",
        "T0225"
      ],
      "description": "رفتار محصول را برای انتخاب یک میزبان با retry ترتیبی یا حفظ مدل سه میزبان و پاسخ‌های مستقل تعیین کن. در صورت انتخاب مدل یک‌میزبانه، زمان پایان تلاش، لغو امن درخواست قبلی و شرایط انتقال به میزبان بعدی را تعریف کن.",
      "depends_on": [
        "UPD-S1-001"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-003",
        "گزارش بازنگری، به‌ویژه بخش 8.2",
        "T0125 و T0126",
        "T0161 تا T0177",
        "T0203 و T0224",
        "نتایج ممیزی ظرفیت و transport",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001"
      ],
      "outputs": [
        "تصمیم محصول دربارهٔ cardinality میزبان",
        "سیاست زمان‌بندی، پایان تلاش و retry",
        "فهرست وظایف موجودی که باید بازنویسی، حفظ یا به قابلیت آینده منتقل شوند"
      ],
      "implementation_focus": "خروجی باید در فایل‌های اجرایی واقعی و آزمون‌های متناظر منعکس شود؛ گزارش به‌تنهایی کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. تعداد میزبان فعال برای هر درخواست به‌صورت صریح تعیین شده باشد.\n2. برای timeout، قطع اتصال، پاسخ دیرهنگام، لغو و انتقال ترتیبی رفتار قطعی تعریف شده باشد.\n3. تکلیف تمام وظایف سه‌میزبانه و چندپاسخی مرتبط با شناسهٔ مشخص ثبت شده باشد.\n4. رفتار انتخاب هم‌زمان چند میزبان با retry ترتیبی اشتباه گرفته نشده باشد.",
      "acceptance_criteria": [
        "تعداد میزبان فعال برای هر درخواست به‌صورت صریح تعیین شده باشد.",
        "برای timeout، قطع اتصال، پاسخ دیرهنگام، لغو و انتقال ترتیبی رفتار قطعی تعریف شده باشد.",
        "تکلیف تمام وظایف سه‌میزبانه و چندپاسخی مرتبط با شناسهٔ مشخص ثبت شده باشد.",
        "رفتار انتخاب هم‌زمان چند میزبان با retry ترتیبی اشتباه گرفته نشده باشد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "docs/decisions/",
        "update/Step three/UPD-S1-003/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0125, T0126, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0203, T0204, T0224, T0225. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-004",
      "title": "تعیین محدودهٔ Streaming در نسخهٔ هدف",
      "section": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "source_task_id": "UPD-S1-004",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0032",
        "T0107",
        "T0110",
        "T0111",
        "T0112",
        "T0147",
        "T0161",
        "T0162",
        "T0163",
        "T0164",
        "T0165",
        "T0166",
        "T0167",
        "T0168",
        "T0169",
        "T0170",
        "T0171",
        "T0172",
        "T0173",
        "T0174",
        "T0175",
        "T0176",
        "T0177",
        "T0200",
        "T0202",
        "T0223"
      ],
      "description": "تصمیم بگیر نمایش تدریجی پاسخ از نسخهٔ اولیه الزامی است یا به نسخهٔ بعد منتقل می‌شود. در هر دو حالت، اثر تصمیم بر قرارداد پیام، رابط کاربر، timeout و لغو را مشخص کن.",
      "depends_on": [
        "UPD-S1-001",
        "UPD-S1-002"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-004",
        "بخش 5.4 و 7 گزارش بازنگری",
        "کد فعلی inference و transport",
        "قراردادهای مشترک موجود",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001, UPD-S1-002"
      ],
      "outputs": [
        "تصمیم ثبت‌شده دربارهٔ زمان عرضهٔ Streaming",
        "معیارهای پذیرش پاسخ تدریجی یا پاسخ یک‌باره",
        "فهرست تغییرات لازم در قرارداد و UI"
      ],
      "implementation_focus": "خروجی باید در فایل‌های اجرایی واقعی و آزمون‌های متناظر منعکس شود؛ گزارش به‌تنهایی کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001, UPD-S1-002 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. نسخهٔ هدف برای Streaming یا پاسخ یک‌باره مشخص شده باشد.\n2. اگر Streaming انتخاب شد، ترتیب قطعه‌ها، پایان، خطا، timeout و لغو تعریف شده باشند.\n3. اگر به نسخهٔ بعد منتقل شد، رفتار قابل‌قبول نسخهٔ اولیه و محدودهٔ تعویق ثبت شده باشد.",
      "acceptance_criteria": [
        "نسخهٔ هدف برای Streaming یا پاسخ یک‌باره مشخص شده باشد.",
        "اگر Streaming انتخاب شد، ترتیب قطعه‌ها، پایان، خطا، timeout و لغو تعریف شده باشند.",
        "اگر به نسخهٔ بعد منتقل شد، رفتار قابل‌قبول نسخهٔ اولیه و محدودهٔ تعویق ثبت شده باشد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "docs/decisions/",
        "protocol/whitep/v1/messages.proto",
        "src/Shared.Contracts/",
        "src/Client.Android/app/src/main/java/com/whitep/client/protocol/",
        "update/Step three/UPD-S1-004/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0032, T0107, T0110, T0111, T0112, T0147, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0200, T0202, T0223. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-005",
      "title": "تعریف رضایت و سیاست قابل‌اندازه‌گیری منابع میزبان",
      "section": "1. ممیزی خط مبنا و تصمیم‌های پایه",
      "source_task_id": "UPD-S1-005",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0049",
        "T0050",
        "T0051",
        "T0052",
        "T0053",
        "T0054",
        "T0055",
        "T0056",
        "T0098",
        "T0099",
        "T0100",
        "T0101",
        "T0102",
        "T0103",
        "T0104",
        "T0105",
        "T0106",
        "T0107",
        "T0196"
      ],
      "description": "قواعد مشارکت اختیاری صاحب رایانه و محدودیت‌های منابع را به تنظیمات و شروط قابل سنجش تبدیل کن؛ از جمله وضعیت بیکار بودن، اتصال برق، سقف CPU/GPU/RAM یا سایر منابع قابل پشتیبانی، و سقف inference هم‌زمان.",
      "depends_on": [
        "UPD-S1-001"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-005",
        "بخش 3 گزارش بازنگری",
        "کد واقعی میزبان و backend مدل",
        "T0098 تا T0107 و T0196",
        "محدودیت‌های فنی قابل اندازه‌گیری در سیستم عامل و موتور مدل",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001"
      ],
      "outputs": [
        "سیاست رضایت پیش‌فرض خاموش",
        "مشخصات تنظیمات منابع و شرایط مجاز مشارکت",
        "قواعد readiness و توقف فوری",
        "فهرست معیارهای قابل آزمون برای رعایت سقف منابع"
      ],
      "implementation_focus": "خروجی باید در فایل‌های اجرایی واقعی و آزمون‌های متناظر منعکس شود؛ گزارش به‌تنهایی کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. مشارکت به‌صورت پیش‌فرض خاموش باشد و فقط با اقدام آگاهانه روشن شود.\n2. تنظیمات فقط شامل محدودیت‌هایی باشند که واقعاً قابل اندازه‌گیری و اعمال‌اند.\n3. شرایط عدم پذیرش درخواست و توقف مشارکت مشخص باشند.\n4. سقف یک inference هم‌زمان حفظ شود مگر اینکه تصمیم مستند و آزمون ظرفیت آن را تغییر دهد.",
      "acceptance_criteria": [
        "مشارکت به‌صورت پیش‌فرض خاموش باشد و فقط با اقدام آگاهانه روشن شود.",
        "تنظیمات فقط شامل محدودیت‌هایی باشند که واقعاً قابل اندازه‌گیری و اعمال‌اند.",
        "شرایط عدم پذیرش درخواست و توقف مشارکت مشخص باشند.",
        "سقف یک inference هم‌زمان حفظ شود مگر اینکه تصمیم مستند و آزمون ظرفیت آن را تغییر دهد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Host.Domain/",
        "src/Host.Application/",
        "src/Host.Windows/",
        "tests/HostLifecycle.Tests/",
        "update/Step three/UPD-S1-005/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0049, T0050, T0051, T0052, T0053, T0054, T0055, T0056, T0098, T0099, T0100, T0101, T0102, T0103, T0104, T0105, T0106, T0107, T0196. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-006",
      "title": "به‌روزرسانی نیازمندی‌ها و معیارهای پذیرش Step One",
      "section": "2. تثبیت نیازمندی‌ها و نقشهٔ اثر تغییر",
      "source_task_id": "UPD-S1-006",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0012",
        "T0013",
        "T0020"
      ],
      "description": "پس از تصویب تصمیم‌های معماری و محصول، سند مرجع Step One و اسناد نیازمندی/پذیرش موجود را با تجربهٔ چت اندروید، مشارکت اختیاری ویندوز و تخصیص خودکار میزبان هماهنگ کن. شناسه‌های وظایف موجود را تا وقتی ماهیت وظیفه ثابت است حفظ کن. نیازمندی هویت بصری مشترک Windows و Android، آیکون سکهٔ سفید با P بزرگ و پالت ثابت پنج‌رنگ تعریف‌شده در visual_identity نیز باید در requirements و acceptance به معیارهای قابل‌آزمون تبدیل شوند.",
      "depends_on": [
        "UPD-S1-002",
        "UPD-S1-003",
        "UPD-S1-004",
        "UPD-S1-005"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-006",
        "step one/V1.0.md",
        "گزارش بازنگری محصول",
        "خروجی UPD-S1-002 تا UPD-S1-005",
        "ممیزی UPD-S1-001",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-002, UPD-S1-003, UPD-S1-004, UPD-S1-005"
      ],
      "outputs": [
        "نسخهٔ بازنگری‌شدهٔ نیازمندی‌های محصول",
        "معیارهای پذیرش قابل بررسی برای Android، Windows، coordinator و transport",
        "سوابق تغییر برای وظایف T شناسه‌دار متأثر",
        "نیازمندی‌ها و معیارهای پذیرش قابل‌آزمون برای آیکون مشترک P و سیستم طراحی پنج‌رنگ در Windows و Android"
      ],
      "implementation_focus": "به‌روزرسانی سند مرجع فقط برای تصمیم‌های مصوب؛ تغییر سند جای اجرای کد را نمی‌گیرد.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-002, UPD-S1-003, UPD-S1-004, UPD-S1-005 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. هر تصمیم تصویب‌شده در سند مرجع و معیار پذیرش منعکس شده باشد.\n2. تعارض‌های شناسه‌دار با T0009، T0019، T0125 تا T0126، T0162 تا T0177 و وظایف امنیتی تعیین تکلیف شده باشند.\n3. شناسهٔ وظایف بدون تغییر ماهوی حفظ شده و حذف یا جایگزینی هر وظیفه دلیل داشته باشد.\n4. هیچ قابلیت بدون شاهد اجرایی به‌عنوان پیاده‌سازی‌شده یا done ثبت نشده باشد.\n5. هویت بصری، کدهای پنج رنگ، نقش هر توکن و قواعد یکسان استفاده در Windows و Android در نیازمندی‌ها و معیارهای پذیرش ثبت شده باشند؛ این ثبت به‌تنهایی ادعای پیاده‌سازی محسوب نشود.\n6. آستانه‌های کنتراست متن و اجزای ضروری رابط و محدودیت استفاده از muted_teal و antique_gold در سطوح روشن در سند نیازمندی/پذیرش ثبت شده باشند.",
      "acceptance_criteria": [
        "هر تصمیم تصویب‌شده در سند مرجع و معیار پذیرش منعکس شده باشد.",
        "تعارض‌های شناسه‌دار با T0009، T0019، T0125 تا T0126، T0162 تا T0177 و وظایف امنیتی تعیین تکلیف شده باشند.",
        "شناسهٔ وظایف بدون تغییر ماهوی حفظ شده و حذف یا جایگزینی هر وظیفه دلیل داشته باشد.",
        "هیچ قابلیت بدون شاهد اجرایی به‌عنوان پیاده‌سازی‌شده یا done ثبت نشده باشد.",
        "هویت بصری، کدهای پنج رنگ، نقش هر توکن و قواعد یکسان استفاده در Windows و Android در نیازمندی‌ها و معیارهای پذیرش ثبت شده باشند؛ این ثبت به‌تنهایی ادعای پیاده‌سازی محسوب نشود.",
        "آستانه‌های کنتراست متن و اجزای ضروری رابط و محدودیت استفاده از muted_teal و antique_gold در سطوح روشن در سند نیازمندی/پذیرش ثبت شده باشند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "step one/V1.0.md",
        "docs/acceptance.md",
        "update/Step three/UPD-S1-006/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0012, T0013, T0020. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-007",
      "title": "بازنگری مدل امنیت و حریم خصوصی",
      "section": "2. تثبیت نیازمندی‌ها و نقشهٔ اثر تغییر",
      "source_task_id": "UPD-S1-007",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0129",
        "T0130",
        "T0131",
        "T0132",
        "T0133",
        "T0134",
        "T0135",
        "T0136",
        "T0137",
        "T0138",
        "T0178",
        "T0179",
        "T0180",
        "T0181",
        "T0182",
        "T0183",
        "T0184",
        "T0185",
        "T0186",
        "T0187",
        "T0188",
        "T0206",
        "T0226",
        "T0227"
      ],
      "description": "مرز اعتماد و جریان داده را برای درخواست‌کننده، میزبان پردازشگر، هماهنگ‌کننده و Relay مستند کن. روشن کن که میزبان پردازشگر می‌تواند متن درخواست و زمینهٔ ارسالی را بخواند و این واقعیت چگونه پیش از استفاده افشا می‌شود.",
      "depends_on": [
        "UPD-S1-002",
        "UPD-S1-005"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-007",
        "بخش 6 گزارش بازنگری",
        "T0129 تا T0138",
        "T0178 تا T0188",
        "تصمیم معماری UPD-S1-002",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-002, UPD-S1-005"
      ],
      "outputs": [
        "مدل تهدید و مرز اعتماد به‌روز",
        "سیاست ثبت log و نگهداری داده",
        "تصمیم مدیریت کلید و هویت همتا",
        "متن دقیق هشدار حریم خصوصی برای درخواست‌کننده"
      ],
      "implementation_focus": "خروجی باید در فایل‌های اجرایی واقعی و آزمون‌های متناظر منعکس شود؛ گزارش به‌تنهایی کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-002, UPD-S1-005 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. نقشهٔ جریان داده مشخص کند چه نقشی به متن روشن دسترسی دارد.\n2. هشدار قابل فهم دربارهٔ قابلیت مشاهدهٔ متن توسط میزبان پیش از اولین درخواست نمایش داده شود.\n3. سیاست عدم ثبت متن درخواست/پاسخ در log عادی و نگهداری موقت مستند باشد.\n4. کنترل‌های احراز هویت، تمامیت، replay و نگهداری کلید به آزمون‌های مشخص متصل باشند.",
      "acceptance_criteria": [
        "نقشهٔ جریان داده مشخص کند چه نقشی به متن روشن دسترسی دارد.",
        "هشدار قابل فهم دربارهٔ قابلیت مشاهدهٔ متن توسط میزبان پیش از اولین درخواست نمایش داده شود.",
        "سیاست عدم ثبت متن درخواست/پاسخ در log عادی و نگهداری موقت مستند باشد.",
        "کنترل‌های احراز هویت، تمامیت، replay و نگهداری کلید به آزمون‌های مشخص متصل باشند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "docs/decisions/ADR-0018-explicit-peer-trust-and-identity-verification.md",
        "src/Shared.Security/",
        "src/Client.Android/app/src/main/java/com/whitep/client/network/",
        "src/Host.Windows/Security/",
        "tests/HostLifecycle.Tests/",
        "update/Step three/UPD-S1-007/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0129, T0130, T0131, T0132, T0133, T0134, T0135, T0136, T0137, T0138, T0178, T0179, T0180, T0181, T0182, T0183, T0184, T0185, T0186, T0187, T0188, T0206, T0226, T0227. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-008",
      "title": "تهیهٔ نقشهٔ اثر تغییر برای مراحل دوم و سوم",
      "section": "2. تثبیت نیازمندی‌ها و نقشهٔ اثر تغییر",
      "source_task_id": "UPD-S1-008",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0012",
        "T0020",
        "T0199",
        "T0214"
      ],
      "description": "تغییرهای تصویب‌شده در نیازمندی‌ها را به وظایف و وابستگی‌های متأثر در Step Two و Step Three نگاشت کن و بستهٔ ارجاعی تهیه کن. مالکیت فایل‌های مراحل دیگر را رعایت کن و گراف آن مراحل را بدون فرایند مربوطه بی‌صدا تغییر نده.",
      "depends_on": [
        "UPD-S1-006",
        "UPD-S1-007"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-008",
        "خروجی UPD-S1-006 و UPD-S1-007",
        "گراف/اسناد Step Two",
        "step three/project-graph.md",
        "فهرست وظایف متأثر در گزارش بازنگری",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-006, UPD-S1-007"
      ],
      "outputs": [
        "ماتریس تصمیم به وظیفهٔ متأثر",
        "فهرست وابستگی‌ها و ترتیب اجرای نیازمند بازنگری",
        "گزارش ارجاع برای صاحبان Step Two و Step Three"
      ],
      "implementation_focus": "تهیهٔ ماتریس اثر و حفظ مالکیت گراف‌های Step Two/Three؛ هر تغییر گراف باید با دلیل ثبت شود.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-006, UPD-S1-007 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. هر تصمیم تصویب‌شده به وظایف موجود مرتبط یا وظیفهٔ جدید پیشنهادی نگاشت شده باشد.\n2. وابستگی‌های جدید و حذف‌شده همراه دلیل مشخص باشند.\n3. موارد نیازمند تصمیم Step Two/Three جدا از موارد تأییدشده علامت‌گذاری شده باشند.",
      "acceptance_criteria": [
        "هر تصمیم تصویب‌شده به وظایف موجود مرتبط یا وظیفهٔ جدید پیشنهادی نگاشت شده باشد.",
        "وابستگی‌های جدید و حذف‌شده همراه دلیل مشخص باشند.",
        "موارد نیازمند تصمیم Step Two/Three جدا از موارد تأییدشده علامت‌گذاری شده باشند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "update/Step two/decision-impact-map.md",
        "update/Step three/UPD-S1-008/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0012, T0020, T0199, T0214. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-009",
      "title": "ممیزی و تکمیل مدیریت محلی موجود گفت‌وگوها در اندروید",
      "section": "3. تکمیل تجربهٔ چت و رابط Android",
      "source_task_id": "UPD-S1-009",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0141",
        "T0142",
        "T0143",
        "T0144",
        "T0145",
        "T0146",
        "T0147",
        "T0148",
        "T0149",
        "T0150",
        "T0222"
      ],
      "description": "قابلیت‌های فعلی Room، فهرست و جزئیات گفت‌وگو و ViewModel پیام را ممیزی کن؛ ایجاد، بازکردن، تغییر عنوان، حذف و بازیابی تاریخچه را با آزمون بررسی کن و فقط شکاف واقعی لازم برای تجربهٔ محصول را تکمیل کن. پایگاه داده یا صفحه‌های موجود را از نو نساز و migration غیرضروری اضافه نکن.",
      "depends_on": [
        "UPD-S1-006",
        "UPD-S1-008",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-009",
        "نیازمندی‌های تصویب‌شده",
        "ClientDatabase/Room DAO و ConversationListScreen و ConversationDetailScreen فعلی",
        "ConversationMessagesViewModel فعلی",
        "نتیجهٔ ممیزی UPD-S1-001",
        "ADR-0020 و تست‌های فعلی ذخیره‌سازی",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-006, UPD-S1-008, UPD-S1-024"
      ],
      "outputs": [
        "گزارش شکاف قابلیت‌های فعلی گفت‌وگو و تاریخچه",
        "اصلاح محدود فقط برای رفتارهای ناقص یا missing acceptance",
        "آزمون CRUD و بازیابی تاریخچه بر پایهٔ Room موجود"
      ],
      "implementation_focus": "از Room و صفحه‌های گفت‌وگوی موجود reuse کن؛ بازنویسی کامل فقط با شاهد نقص مجاز است.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-006, UPD-S1-008, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. کد و آزمون موجود برای ایجاد، بازکردن، تغییر عنوان، حذف و بازیابی تاریخچه پیش از تغییر بررسی شده باشند.\n2. قابلیت‌های موجود بدون بازنویسی غیرضروری حفظ شوند.\n3. هر شکاف واقعی با تغییر کوچک و تست مشخص پوشش داده شود؛ اگر شکافی وجود ندارد، گزارش و تست موجود خروجی پذیرفته‌شده باشد.\n4. ترتیب پیام‌ها و رابطهٔ آن‌ها با گفت‌وگو پس از راه‌اندازی مجدد حفظ شود.",
      "acceptance_criteria": [
        "کد و آزمون موجود برای ایجاد، بازکردن، تغییر عنوان، حذف و بازیابی تاریخچه پیش از تغییر بررسی شده باشند.",
        "قابلیت‌های موجود بدون بازنویسی غیرضروری حفظ شوند.",
        "هر شکاف واقعی با تغییر کوچک و تست مشخص پوشش داده شود؛ اگر شکافی وجود ندارد، گزارش و تست موجود خروجی پذیرفته‌شده باشد.",
        "ترتیب پیام‌ها و رابطهٔ آن‌ها با گفت‌وگو پس از راه‌اندازی مجدد حفظ شود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/data/",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/",
        "src/Client.Android/app/src/test/java/com/whitep/client/data/",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/",
        "update/Step three/UPD-S1-009/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0141, T0142, T0143, T0144, T0145, T0146, T0147, T0148, T0149, T0150, T0222. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-010",
      "title": "اتصال رابط گفت‌وگوی موجود به چرخهٔ ارسال و دریافت",
      "section": "3. تکمیل تجربهٔ چت و رابط Android",
      "source_task_id": "UPD-S1-010",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0088",
        "T0090",
        "T0092",
        "T0139",
        "T0145",
        "T0146",
        "T0147",
        "T0148",
        "T0149",
        "T0150",
        "T0190",
        "T0200",
        "T0202",
        "T0223"
      ],
      "description": "ConversationDetailScreen و ConversationMessagesViewModel موجود را به جریان واقعی request runtime متصل کن: اعتبارسنجی ورودی، رضایت، انتخاب خودکار مدل، discovery، تخصیص میزبان، transport، دریافت پاسخ یا خطای typed و ذخیره در گفت‌وگوی باز. وضعیت‌های داخلی شبکه/میزبان را به پیام‌های روشن کاربر تبدیل کن و IP، Port و PeerId را از مسیر عادی UI حذف کن. صفحهٔ چت و navigation را از نو نساز.",
      "depends_on": [
        "UPD-S1-009",
        "UPD-S1-017",
        "UPD-S1-022",
        "UPD-S1-023",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-010",
        "رابط و ViewModel موجود Android",
        "خروجی UPD-S1-009",
        "قرارداد پیام و وضعیت مصوب در UPD-S1-017",
        "gateway/runtime/transport فعلی",
        "تصمیم اعتماد UPD-S1-022",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-009, UPD-S1-017, UPD-S1-022, UPD-S1-023, UPD-S1-024"
      ],
      "outputs": [
        "جریان اجرایی ارسال از صفحهٔ گفت‌وگوی فعلی",
        "دریافت پاسخ میزبان و درج آن در همان گفت‌وگو",
        "نمایش وضعیت‌های قابل‌فهم discovery/model/connection/processing/response/retry/failure/cancel",
        "نمایش خطاهای typed بدون افشای IP، Port یا PeerId در UI عادی",
        "آزمون integration و UI برای مسیر ارسال/دریافت و نگاشت وضعیت‌ها"
      ],
      "implementation_focus": "چرخهٔ واقعی UI → Android runtime → transport → Windows host → پاسخ باید متصل شود؛ Room-only یا mocked-only کافی نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-009, UPD-S1-017, UPD-S1-022, UPD-S1-023, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. پیام از صفحهٔ گفت‌وگوی موجود به مسیر runtime واقعی فرستاده شود، نه فقط Room.\n2. پیش از ارسال، رضایت و قواعد trust مصوب و سیاست انتخاب مدل اجرا شوند.\n3. پاسخ واقعی یا خطای typed به همان گفت‌وگو بازگردد و قابل مشاهده باشد.\n4. وضعیت‌های در حال یافتن میزبان، بررسی سازگاری مدل، اتصال امن، پردازش، دریافت پاسخ، میزبان در دسترس نیست، میزبان سازگار پیدا نشد، قطع اتصال/تلاش مجدد و لغو با متن قابل فهم نمایش داده شوند.\n5. هیچ IP، Port یا PeerId یا شناسهٔ شناسایی غیرضروری میزبان در متن و کنترل‌های مسیر عادی چت افشا نشود؛ جزئیات عیب‌یابی فقط در بخش پیشرفتهٔ جداگانه مجاز باشد.\n6. UI موجود بدون بازسازی غیرضروری حفظ شود و تست مسیر واقعی/mock boundary به‌صراحت محدودیت mock را اعلام کند.",
      "acceptance_criteria": [
        "پیام از صفحهٔ گفت‌وگوی موجود به مسیر runtime واقعی فرستاده شود، نه فقط Room.",
        "پیش از ارسال، رضایت و قواعد trust مصوب و سیاست انتخاب مدل اجرا شوند.",
        "پاسخ واقعی یا خطای typed به همان گفت‌وگو بازگردد و قابل مشاهده باشد.",
        "وضعیت‌های در حال یافتن میزبان، بررسی سازگاری مدل، اتصال امن، پردازش، دریافت پاسخ، میزبان در دسترس نیست، میزبان سازگار پیدا نشد، قطع اتصال/تلاش مجدد و لغو با متن قابل فهم نمایش داده شوند.",
        "هیچ IP، Port یا PeerId یا شناسهٔ شناسایی غیرضروری میزبان در متن و کنترل‌های مسیر عادی چت افشا نشود؛ جزئیات عیب‌یابی فقط در بخش پیشرفتهٔ جداگانه مجاز باشد.",
        "UI موجود بدون بازسازی غیرضروری حفظ شود و تست مسیر واقعی/mock boundary به‌صراحت محدودیت mock را اعلام کند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/",
        "update/Step three/UPD-S1-010/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0088, T0090, T0092, T0139, T0145, T0146, T0147, T0148, T0149, T0150, T0190, T0200, T0202, T0223. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-011",
      "title": "حذف تنظیمات فنی از مسیر عادی و افزودن رضایت اندروید",
      "section": "3. تکمیل تجربهٔ چت و رابط Android",
      "source_task_id": "UPD-S1-011",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0139",
        "T0186"
      ],
      "description": "مسیر عادی چت را از ورود دستی Rendezvous URL، Peer ID، IP، Port، Exact model ID و تعداد توکن مستقل کن. مدل و سقف خروجی را فقط از سیاست خودکار و deterministic وظیفهٔ UPD-S1-023 بگیر و پیش از اولین درخواست اجتماع‌محور رضایت و هشدار حریم خصوصی را نمایش بده.",
      "depends_on": [
        "UPD-S1-006",
        "UPD-S1-007",
        "UPD-S1-014",
        "UPD-S1-023",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-011",
        "نیازمندی‌های تصویب‌شده",
        "UPD-S1-002 و UPD-S1-007",
        "کاتالوگ قابلیت مدل/میزبان",
        "رابط فعلی Android",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-006, UPD-S1-007, UPD-S1-014, UPD-S1-023, UPD-S1-024"
      ],
      "outputs": [
        "جریان onboarding رضایت و هشدار",
        "مسیر خودکار انتخاب مدل و میزبان",
        "بخش پیشرفتهٔ جدا برای عیب‌یابی در صورت نیاز"
      ],
      "implementation_focus": "کاربر عادی نباید مدل/PeerId/IP/Port را برای هر درخواست وارد کند؛ onboarding رضایت و privacy قبل از اولین ارسال لازم است.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-006, UPD-S1-007, UPD-S1-014, UPD-S1-023, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. ارسال یک پیام عادی بدون ورود هیچ‌یک از فیلدهای فنی نام‌برده ممکن باشد.\n2. مدل و max output tokens طبق سیاست مصوب و نتیجهٔ اعتبارسنجی قابلیت‌های واقعی خودکار انتخاب شوند؛ کاربر آن‌ها را برای هر پیام وارد نکند.\n3. هویت میزبان نامعتبر در لایهٔ سیستم رد شود.\n4. رضایت و هشدار حریم خصوصی پیش از اولین درخواست نمایش داده شود و قابل رد کردن باشد.\n5. شناسهٔ میزبان یا IP/Port غیرضروری در رابط عادی افشا نشود.",
      "acceptance_criteria": [
        "ارسال یک پیام عادی بدون ورود هیچ‌یک از فیلدهای فنی نام‌برده ممکن باشد.",
        "مدل و max output tokens طبق سیاست مصوب و نتیجهٔ اعتبارسنجی قابلیت‌های واقعی خودکار انتخاب شوند؛ کاربر آن‌ها را برای هر پیام وارد نکند.",
        "هویت میزبان نامعتبر در لایهٔ سیستم رد شود.",
        "رضایت و هشدار حریم خصوصی پیش از اولین درخواست نمایش داده شود و قابل رد کردن باشد.",
        "شناسهٔ میزبان یا IP/Port غیرضروری در رابط عادی افشا نشود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/",
        "update/Step three/UPD-S1-011/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0139, T0186. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-012",
      "title": "پیاده‌سازی مشارکت اختیاری در میزبان ویندوز",
      "section": "4. تکمیل میزبان Windows و کنترل منابع",
      "source_task_id": "UPD-S1-012",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0041",
        "T0042",
        "T0043",
        "T0044",
        "T0045",
        "T0046",
        "T0047",
        "T0048",
        "T0108",
        "T0109",
        "T0110",
        "T0111",
        "T0112",
        "T0113",
        "T0114",
        "T0115",
        "T0193"
      ],
      "description": "کنترل روشن/خاموش مشارکت را با پیش‌فرض خاموش، نمایش وضعیت میزبان و توقف فوری ارائه کن. روشن‌کردن مشارکت باید با رضایت صریح مالک انجام شود و خروج یا خاموش‌کردن مشارکت مانع پذیرش درخواست جدید شود.",
      "depends_on": [
        "UPD-S1-006",
        "UPD-S1-008",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-012",
        "نیازمندی‌های تصویب‌شده",
        "کد فعلی میزبان ویندوز",
        "خروجی UPD-S1-005",
        "معماری UI ویندوز",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-006, UPD-S1-008, UPD-S1-024"
      ],
      "outputs": [
        "کنترل مشارکت و وضعیت‌های خاموش/روشن/متصل/آماده/مشغول/خطا",
        "رفتار توقف پذیرش درخواست جدید",
        "آزمون وضعیت مشارکت"
      ],
      "implementation_focus": "opt-in واقعی و listener lifecycle در Windows؛ default باید خاموش باشد.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-006, UPD-S1-008, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. پس از نصب و اجرای اولیه مشارکت خاموش باشد.\n2. فقط اقدام صریح مالک آن را فعال کند.\n3. در حالت خاموش هیچ درخواست جدیدی پذیرفته نشود.\n4. توقف فوری بدون حذف یا آسیب به مدل‌های محلی ممکن باشد.",
      "acceptance_criteria": [
        "پس از نصب و اجرای اولیه مشارکت خاموش باشد.",
        "فقط اقدام صریح مالک آن را فعال کند.",
        "در حالت خاموش هیچ درخواست جدیدی پذیرفته نشود.",
        "توقف فوری بدون حذف یا آسیب به مدل‌های محلی ممکن باشد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Host.Windows/MainWindow.xaml",
        "src/Host.Windows/MainWindow.xaml.cs",
        "src/Host.Windows/App.xaml",
        "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs",
        "src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs",
        "tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs",
        "update/Step three/UPD-S1-012/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0041, T0042, T0043, T0044, T0045, T0046, T0047, T0048, T0108, T0109, T0110, T0111, T0112, T0113, T0114, T0115, T0193. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-013",
      "title": "اعمال محدودیت منابع و آمادگی واقعی میزبان ویندوز",
      "section": "4. تکمیل میزبان Windows و کنترل منابع",
      "source_task_id": "UPD-S1-013",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0049",
        "T0050",
        "T0051",
        "T0052",
        "T0053",
        "T0054",
        "T0055",
        "T0056",
        "T0098",
        "T0099",
        "T0100",
        "T0101",
        "T0102",
        "T0103",
        "T0104",
        "T0105",
        "T0106",
        "T0107",
        "T0196"
      ],
      "description": "پیش از پذیرش هر درخواست، وضعیت مشارکت، آمادگی backend مدل، ظرفیت واقعی، محدودیت‌های مالک و شرایطی مانند بیکار بودن یا اتصال برق را بررسی کن. سقف پردازش هم‌زمان مصوب را اعمال کن.",
      "depends_on": [
        "UPD-S1-005",
        "UPD-S1-012",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-013",
        "خروجی UPD-S1-005",
        "موتور inference و APIهای منابع سیستم",
        "state machine میزبان",
        "معیارهای readiness",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-005, UPD-S1-012, UPD-S1-024"
      ],
      "outputs": [
        "سرویس ارزیابی آمادگی و ظرفیت",
        "تنظیمات و اعمال محدودیت منابع",
        "رفتار رد درخواست در شرایط نامناسب"
      ],
      "implementation_focus": "readiness باید از وضعیت واقعی backend، مدل و منابع استفاده کند؛ مقدار نامعلوم نباید حدس زده شود.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-005, UPD-S1-012, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. درخواست در حالت عدم مشارکت، مدل آماده‌نشده یا ظرفیت ناکافی رد شود.\n2. سقف inference هم‌زمان از مقدار مصوب فراتر نرود.\n3. شرایط برق/idle و محدودیت منابع فقط در صورت پشتیبانی واقعی سیستم اعمال شوند.\n4. تمام ردها با کد وضعیت قابل مشاهده و آزمون باشند.",
      "acceptance_criteria": [
        "درخواست در حالت عدم مشارکت، مدل آماده‌نشده یا ظرفیت ناکافی رد شود.",
        "سقف inference هم‌زمان از مقدار مصوب فراتر نرود.",
        "شرایط برق/idle و محدودیت منابع فقط در صورت پشتیبانی واقعی سیستم اعمال شوند.",
        "تمام ردها با کد وضعیت قابل مشاهده و آزمون باشند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Host.Windows/WindowsHostHardwareDiscovery.cs",
        "src/Host.Application/HostReadinessEvaluator.cs",
        "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs",
        "src/Host.Windows/Requests/HostRequestCoordinator.cs",
        "tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs",
        "tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs",
        "update/Step three/UPD-S1-013/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0049, T0050, T0051, T0052, T0053, T0054, T0055, T0056, T0098, T0099, T0100, T0101, T0102, T0103, T0104, T0105, T0106, T0107, T0196. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-014",
      "title": "تکمیل کشف metadata-only و غربالگری میزبان‌های قابل‌انتخاب",
      "section": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "source_task_id": "UPD-S1-014",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0116",
        "T0117",
        "T0118",
        "T0119",
        "T0120",
        "T0121",
        "T0122",
        "T0123",
        "T0124",
        "T0125",
        "T0126",
        "T0127",
        "T0128"
      ],
      "description": "اعلان‌های metadata-only فعلی را با TTL/heartbeat معتبر نگهداری و تمدید کن و در کلاینت، دادهٔ directory را با live status probe احراز هویت‌شده، مشارکت فعال، آمادگی backend، ظرفیت، مدل و محدودیت‌های مالک تطبیق بده. selector موجود را برای ranking قطعی و قابل آزمون گسترش بده. سرویس directory نباید prompt، response یا تاریخچه دریافت کند؛ محل انتخاب مرکزی یا سمت کلاینت تابع تصمیم مصوب معماری است.",
      "depends_on": [
        "UPD-S1-002",
        "UPD-S1-003",
        "UPD-S1-013",
        "UPD-S1-017",
        "UPD-S1-022",
        "UPD-S1-023",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-014",
        "تصمیم‌های UPD-S1-002 و UPD-S1-003",
        "PeerAnnouncementDirectory و RendezvousPeerDirectoryClient فعلی",
        "AuthenticatedHostDiscovery و HostCandidateSelector فعلی",
        "وضعیت واقعی میزبان از UPD-S1-013",
        "trust policy در UPD-S1-022",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-002, UPD-S1-003, UPD-S1-013, UPD-S1-017, UPD-S1-022, UPD-S1-023, UPD-S1-024"
      ],
      "outputs": [
        "قواعد معتبر ثبت/تمدید/انقضای metadata حضور",
        "غربالگری live و احراز هویت‌شدهٔ میزبان",
        "سیاست eligibility و ranking قطعی بر اساس دادهٔ تازه",
        "آزمون عدم ورود prompt/response/history به directory"
      ],
      "implementation_focus": "directory فقط metadata؛ eligibility باید live/authenticated باشد و prompt/response/history وارد rendezvous نشود.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-002, UPD-S1-003, UPD-S1-013, UPD-S1-017, UPD-S1-022, UPD-S1-023, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. اعلان منقضی، میزبان opt-out، ناشناس/غیرقابل‌اعتماد، غیرقابل‌دسترسی یا non-ready انتخاب نشود.\n2. مدل، ظرفیت، تعداد inference فعال و محدودیت منابع مالک با دادهٔ تازه بررسی شوند.\n3. بار و کیفیت اتصال فقط با شاخص‌های واقعی و قابل‌اندازه‌گیری در ranking اثر بگذارند؛ مقدار نامعلوم حدس زده نشود.\n4. selection از سقف میزبان مصوب در UPD-S1-003 عبور نکند و deterministic باشد.\n5. هیچ prompt، response یا history به سرویس directory ارسال یا از آن دریافت نشود.\n6. قابلیت مدل تنها با شناسه و metadata معتبر و backend واقعاً آماده قابل قبول باشد؛ صرف advertised بودن مدل برای پذیرش کافی نیست.",
      "acceptance_criteria": [
        "اعلان منقضی، میزبان opt-out، ناشناس/غیرقابل‌اعتماد، غیرقابل‌دسترسی یا non-ready انتخاب نشود.",
        "مدل، ظرفیت، تعداد inference فعال و محدودیت منابع مالک با دادهٔ تازه بررسی شوند.",
        "بار و کیفیت اتصال فقط با شاخص‌های واقعی و قابل‌اندازه‌گیری در ranking اثر بگذارند؛ مقدار نامعلوم حدس زده نشود.",
        "selection از سقف میزبان مصوب در UPD-S1-003 عبور نکند و deterministic باشد.",
        "هیچ prompt، response یا history به سرویس directory ارسال یا از آن دریافت نشود.",
        "قابلیت مدل تنها با شناسه و metadata معتبر و backend واقعاً آماده قابل قبول باشد؛ صرف advertised بودن مدل برای پذیرش کافی نیست."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt",
        "src/Host.Windows/Networking/PeerAnnouncementClient.cs",
        "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs",
        "src/Client.Android/app/src/test/java/com/whitep/client/requests/",
        "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs",
        "update/Step three/UPD-S1-014/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0116, T0117, T0118, T0119, T0120, T0121, T0122, T0123, T0124, T0125, T0126, T0127, T0128. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-015",
      "title": "تخصیص میزبان طبق cardinality مصوب و retry امن",
      "section": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "source_task_id": "UPD-S1-015",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0125",
        "T0126",
        "T0161",
        "T0162",
        "T0163",
        "T0164",
        "T0165",
        "T0166",
        "T0167",
        "T0168",
        "T0169",
        "T0170",
        "T0171",
        "T0172",
        "T0173",
        "T0174",
        "T0175",
        "T0176",
        "T0177",
        "T0203",
        "T0204",
        "T0224",
        "T0225"
      ],
      "description": "بر اساس تصمیم مصوب تعداد میزبان، مجموعهٔ میزبان یا میزبان‌های واجد شرایط را انتخاب و تلاش‌ها را زمان‌بندی کن. اگر یک میزبان در هر درخواست تصویب شد، fallback ترتیبی فقط پس از تعیین تکلیف امن تلاش قبلی انجام شود؛ اگر مدل چندمیزبانه حفظ شد، batch و چند پاسخ مستقل طبق همان تصمیم اجرا شوند. از broadcast به همهٔ میزبان‌های آنلاین خودداری کن.",
      "depends_on": [
        "UPD-S1-003",
        "UPD-S1-014",
        "UPD-S1-016",
        "UPD-S1-017",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-015",
        "تصمیم UPD-S1-003",
        "فهرست میزبان‌های UPD-S1-014",
        "deadline و وضعیت‌های قرارداد",
        "سیاست لغو و idempotency",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-003, UPD-S1-014, UPD-S1-016, UPD-S1-017, UPD-S1-024"
      ],
      "outputs": [
        "scheduler تخصیص درخواست",
        "سیاست retry ترتیبی و سقف تلاش",
        "ثبت وضعیت تلاش‌ها بدون محتوای پیام"
      ],
      "implementation_focus": "cardinality و retry دقیقاً مطابق تصمیم UPD-S1-003؛ ارسال هم‌زمان پنهان یا retry نامحدود پذیرفته نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-003, UPD-S1-014, UPD-S1-016, UPD-S1-017, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. تعداد میزبان در هر تلاش دقیقاً با تصمیم UPD-S1-003 تطابق داشته باشد.\n2. در حالت تک‌میزبان، تلاش بعدی فقط پس از terminal شدن، timeout یا لغو امن تلاش قبلی آغاز شود.\n3. در حالت چندمیزبانه، تعداد و roundها از حدود تصمیم مصوب تجاوز نکنند.\n4. هیچ درخواست به همهٔ میزبان‌های آنلاین broadcast نشود و پاسخ دیرهنگام/تکراری به تلاش نادرست نسبت داده نشود.\n5. deadline، سقف retry و idempotency با آزمون قطعی پوشش داده شوند.",
      "acceptance_criteria": [
        "تعداد میزبان در هر تلاش دقیقاً با تصمیم UPD-S1-003 تطابق داشته باشد.",
        "در حالت تک‌میزبان، تلاش بعدی فقط پس از terminal شدن، timeout یا لغو امن تلاش قبلی آغاز شود.",
        "در حالت چندمیزبانه، تعداد و roundها از حدود تصمیم مصوب تجاوز نکنند.",
        "هیچ درخواست به همهٔ میزبان‌های آنلاین broadcast نشود و پاسخ دیرهنگام/تکراری به تلاش نادرست نسبت داده نشود.",
        "deadline، سقف retry و idempotency با آزمون قطعی پوشش داده شوند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
        "src/Client.Android/app/src/test/java/com/whitep/client/requests/",
        "update/Step three/UPD-S1-015/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0125, T0126, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0203, T0204, T0224, T0225. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-016",
      "title": "پیاده‌سازی انتقال امن و مسیر جایگزین مصوب",
      "section": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "source_task_id": "UPD-S1-016",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0116",
        "T0117",
        "T0118",
        "T0119",
        "T0120",
        "T0129",
        "T0130",
        "T0131",
        "T0132",
        "T0133",
        "T0134",
        "T0135",
        "T0136",
        "T0137",
        "T0138",
        "T0206",
        "T0226",
        "T0227"
      ],
      "description": "مسیر ارتباطی مصوب در UPD-S1-002 را با استفاده از transport و کتابخانه‌های معتبر موجود تکمیل کن. اتصال مستقیم P2P و احراز هویت فعلی را بدون دلیل بازنویسی نکن. Relay یا مسیر مدیریت‌شده فقط در صورت تصویب صریح فعال شود و باید انتقال opaque با امنیت endpoint-to-endpoint داشته باشد؛ fallback خاموش به Relay مجاز نیست.",
      "depends_on": [
        "UPD-S1-002",
        "UPD-S1-007",
        "UPD-S1-008",
        "UPD-S1-017",
        "UPD-S1-022"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-016",
        "تصمیم معماری UPD-S1-002",
        "Libp2pInferenceRequestTransport و PeerTransportFallbackConnector فعلی",
        "PeerTrustStore و سیاست trust در UPD-S1-022",
        "قراردادهای مشترک و تست‌های transport",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-002, UPD-S1-007, UPD-S1-008, UPD-S1-017, UPD-S1-022"
      ],
      "outputs": [
        "transport امن در مسیر اصلی",
        "مسیر Relay یا fallback در صورت تصویب",
        "مدیریت قطع اتصال و خطای handshake"
      ],
      "implementation_focus": "در مسیر انتقال، محتوا فقط به میزبان مقصد پردازشگر برسد؛ سیاست relay و fallback باید با تصمیم UPD-S1-002 تطبیق داشته باشد.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-002, UPD-S1-007, UPD-S1-008, UPD-S1-017, UPD-S1-022 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. مسیر فعال با ADR مصوب و نسخهٔ فعلی محصول تطابق داشته باشد.\n2. هویت endpointها طبق trust policy معتبر بررسی و همتای ناشناخته/نامعتبر رد شود.\n3. اگر Relay تصویب شد، relay نتواند payload برنامه را رمزگشایی، parse، log یا persist کند و آزمون runtime سه‌peer عبور بایت را اثبات کند.\n4. اگر Relay تصویب نشد، Relay در نسخهٔ هدف فعال نشود و شکست اتصال مستقیم خطای صریح باقی بماند.\n5. هیچ الگوریتم رمزنگاری اختصاصی اضافه نشود.",
      "acceptance_criteria": [
        "مسیر فعال با ADR مصوب و نسخهٔ فعلی محصول تطابق داشته باشد.",
        "هویت endpointها طبق trust policy معتبر بررسی و همتای ناشناخته/نامعتبر رد شود.",
        "اگر Relay تصویب شد، relay نتواند payload برنامه را رمزگشایی، parse، log یا persist کند و آزمون runtime سه‌peer عبور بایت را اثبات کند.",
        "اگر Relay تصویب نشد، Relay در نسخهٔ هدف فعال نشود و شکست اتصال مستقیم خطای صریح باقی بماند.",
        "هیچ الگوریتم رمزنگاری اختصاصی اضافه نشود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt",
        "src/Host.Windows/Networking/Libp2pPeerTransport.cs",
        "src/Host.Windows/Networking/PeerTransportFallbackConnector.cs",
        "src/Host.Windows/Networking/IPeerTransport.cs",
        "tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs",
        "src/Client.Android/app/src/test/java/com/whitep/client/network/",
        "update/Step three/UPD-S1-016/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0116, T0117, T0118, T0119, T0120, T0129, T0130, T0131, T0132, T0133, T0134, T0135, T0136, T0137, T0138, T0206, T0226, T0227. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-017",
      "title": "تعریف قرارداد پیام و وضعیت چرخهٔ درخواست",
      "section": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "source_task_id": "UPD-S1-017",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0032",
        "T0033",
        "T0034",
        "T0035",
        "T0036",
        "T0107",
        "T0108",
        "T0109",
        "T0110",
        "T0111",
        "T0112",
        "T0161",
        "T0162",
        "T0163",
        "T0164",
        "T0165",
        "T0166",
        "T0167",
        "T0168",
        "T0169",
        "T0170",
        "T0171",
        "T0172",
        "T0173",
        "T0174",
        "T0175",
        "T0176",
        "T0177",
        "T0189",
        "T0190",
        "T0191",
        "T0192"
      ],
      "description": "قرارداد مشترک درخواست و پاسخ را با شناسهٔ درخواست و تلاش، مدل/قابلیت موردنیاز، deadline، status، خطا، پایان، timeout و cancellation تعریف و در Android و Windows یکسان اعمال کن. اگر Streaming تصویب شد، ترتیب و اعتبار قطعه‌ها را نیز پوشش بده.",
      "depends_on": [
        "UPD-S1-002",
        "UPD-S1-003",
        "UPD-S1-004"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-017",
        "خروجی UPD-S1-003 و UPD-S1-004",
        "قراردادهای موجود Shared",
        "وضعیت‌های UI و میزبان",
        "سیاست retry",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-002, UPD-S1-003, UPD-S1-004"
      ],
      "outputs": [
        "schema نسخه‌دار پیام",
        "state machine درخواست و تلاش",
        "کدهای خطا و قواعد پایان/لغو"
      ],
      "implementation_focus": "یک قرارداد نسخه‌دار canonical برای Kotlin/C#؛ کپی‌های دستی و schemaهای ناسازگار ممنوع.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-002, UPD-S1-003, UPD-S1-004 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. شناسه‌های درخواست و تلاش یکتا و قابل اعتبارسنجی باشند.\n2. وضعیت‌های پایان، خطا، timeout و لغو در هر دو برنامه معنا و نگاشت یکسان داشته باشند.\n3. در صورت فعال بودن Streaming، ترتیب قطعه‌ها و پایان نهایی قابل تشخیص باشد.\n4. درخواست منقضی یا تکراری طبق سیاست مصوب رد شود.",
      "acceptance_criteria": [
        "شناسه‌های درخواست و تلاش یکتا و قابل اعتبارسنجی باشند.",
        "وضعیت‌های پایان، خطا، timeout و لغو در هر دو برنامه معنا و نگاشت یکسان داشته باشند.",
        "در صورت فعال بودن Streaming، ترتیب قطعه‌ها و پایان نهایی قابل تشخیص باشد.",
        "درخواست منقضی یا تکراری طبق سیاست مصوب رد شود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "protocol/whitep/v1/messages.proto",
        "src/Shared.Contracts/",
        "src/Client.Android/app/src/main/java/com/whitep/client/protocol/",
        "src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs",
        "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs",
        "tests/contracts/",
        "update/Step three/UPD-S1-017/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0032, T0033, T0034, T0035, T0036, T0107, T0108, T0109, T0110, T0111, T0112, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0189, T0190, T0191, T0192. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-018",
      "title": "اجرای کنترل‌های حریم خصوصی و پاک‌سازی داده",
      "section": "5. تکمیل کشف میزبان، انتقال، قرارداد و حریم خصوصی",
      "source_task_id": "UPD-S1-018",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0105",
        "T0178",
        "T0179",
        "T0180",
        "T0181",
        "T0182",
        "T0183",
        "T0184",
        "T0185",
        "T0186",
        "T0187",
        "T0188",
        "T0206",
        "T0227"
      ],
      "description": "ثبت log، نگهداری موقت payload، مدیریت کلید، جلوگیری از replay و پاک‌سازی داده‌های موقت را مطابق مدل امنیت مصوب پیاده‌سازی کن. محتوای روشن فقط در میزبان پردازشگر در محدودهٔ ضروری inference قابل دسترسی باشد.",
      "depends_on": [
        "UPD-S1-007",
        "UPD-S1-016",
        "UPD-S1-017"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-018",
        "مدل امنیت UPD-S1-007",
        "سیاست نگهداری داده",
        "T0138 و T0178 تا T0188",
        "قرارداد UPD-S1-017",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-007, UPD-S1-016, UPD-S1-017"
      ],
      "outputs": [
        "allowlist فیلدهای log",
        "مدیریت امن اسرار و کلیدها",
        "پاک‌سازی پس از پایان/خطا/لغو/timeout",
        "آزمون‌های privacy و replay"
      ],
      "implementation_focus": "logها payload-free؛ کلیدها و بافرهای حساس طبق policy پاک‌سازی شوند.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-007, UPD-S1-016, UPD-S1-017 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. log عادی شامل متن درخواست، پاسخ، capsule یا اسرار نباشد.\n2. داده‌های موقت پس از پایان همهٔ مسیرهای موفق و ناموفق طبق سیاست پاک شوند.\n3. کلیدهای خصوصی در ذخیره‌سازی امن سیستم قرار گیرند.\n4. آزمون replay و دستکاری پیام و جست‌وجوی log/file برای دادهٔ حساس موفق باشد.",
      "acceptance_criteria": [
        "log عادی شامل متن درخواست، پاسخ، capsule یا اسرار نباشد.",
        "داده‌های موقت پس از پایان همهٔ مسیرهای موفق و ناموفق طبق سیاست پاک شوند.",
        "کلیدهای خصوصی در ذخیره‌سازی امن سیستم قرار گیرند.",
        "آزمون replay و دستکاری پیام و جست‌وجوی log/file برای دادهٔ حساس موفق باشد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/security/",
        "src/Shared.Security/",
        "src/Host.Windows/Security/",
        "tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs",
        "update/Step three/UPD-S1-018/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0105, T0178, T0179, T0180, T0181, T0182, T0183, T0184, T0185, T0186, T0187, T0188, T0206, T0227. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-019",
      "title": "افزودن آزمون‌های واحد و یکپارچه",
      "section": "6. آزمون یکپارچه و تحویل نصب‌پذیر",
      "source_task_id": "UPD-S1-019",
      "source_change_operation": "add",
      "affected_task_ids": [
        "T0150",
        "T0176",
        "T0196",
        "T0197",
        "T0198",
        "T0199",
        "T0200",
        "T0201",
        "T0202",
        "T0203",
        "T0204",
        "T0205",
        "T0206",
        "T0207",
        "T0208",
        "T0229"
      ],
      "description": "برای رابط Android، state machine میزبان، انتخاب و retry، سازگاری مدل، قرارداد مشترک، رمزگذاری و حریم خصوصی آزمون‌های خودکار اضافه کن. آزمون‌ها باید سناریوهای شکست و لغو را نیز بپوشانند.",
      "depends_on": [
        "UPD-S1-009",
        "UPD-S1-010",
        "UPD-S1-011",
        "UPD-S1-012",
        "UPD-S1-013",
        "UPD-S1-014",
        "UPD-S1-015",
        "UPD-S1-016",
        "UPD-S1-017",
        "UPD-S1-018",
        "UPD-S1-023",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-019",
        "کد تکمیل‌شدهٔ UPD-S1-009 تا UPD-S1-018",
        "معیارهای پذیرش مصوب",
        "زیرساخت CI و test موجود",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-009, UPD-S1-010, UPD-S1-011, UPD-S1-012, UPD-S1-013, UPD-S1-014, UPD-S1-015, UPD-S1-016, UPD-S1-017, UPD-S1-018, UPD-S1-023, UPD-S1-024"
      ],
      "outputs": [
        "مجموعه آزمون واحد و یکپارچه",
        "گزارش CI شامل فرمان و نتیجه",
        "فهرست سناریوهای پوشش داده‌شده و محدودیت‌ها"
      ],
      "implementation_focus": "آزمون‌های منفی، لغو، timeout، عدم سازگاری، replay و boundaryهای امنیتی؛ تست‌ها باید CI را اجرا کنند.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-009, UPD-S1-010, UPD-S1-011, UPD-S1-012, UPD-S1-013, UPD-S1-014, UPD-S1-015, UPD-S1-016, UPD-S1-017, UPD-S1-018, UPD-S1-023, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. آزمون‌های مسیر موفق، نبود میزبان، میزبان مشغول، مدل ناسازگار، قطع اتصال، timeout و لغو وجود داشته باشند.\n2. آزمون‌های امنیتی هویت نامعتبر، replay، دستکاری و عدم افشای محتوای روشن را بررسی کنند.\n3. آزمون‌ها در CI اجرا شوند و نتیجهٔ واقعی ثبت شود.\n4. هیچ test یا build اجرا‌نشده‌ای موفق گزارش نشود.\n5. آزمون‌های انتخاب مدل و سقف خروجی، نبود مدل سازگار، مدل‌های هم‌امتیاز و metadata نامعتبر را پوشش دهند.\n6. آزمون‌های UI نگاشت وضعیت‌های درخواست و عدم افشای IP، Port و PeerId در مسیر عادی را بررسی کنند.",
      "acceptance_criteria": [
        "آزمون‌های مسیر موفق، نبود میزبان، میزبان مشغول، مدل ناسازگار، قطع اتصال، timeout و لغو وجود داشته باشند.",
        "آزمون‌های امنیتی هویت نامعتبر، replay، دستکاری و عدم افشای محتوای روشن را بررسی کنند.",
        "آزمون‌ها در CI اجرا شوند و نتیجهٔ واقعی ثبت شود.",
        "هیچ test یا build اجرا‌نشده‌ای موفق گزارش نشود.",
        "آزمون‌های انتخاب مدل و سقف خروجی، نبود مدل سازگار، مدل‌های هم‌امتیاز و metadata نامعتبر را پوشش دهند.",
        "آزمون‌های UI نگاشت وضعیت‌های درخواست و عدم افشای IP، Port و PeerId در مسیر عادی را بررسی کنند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/test/",
        "src/Client.Android/app/src/androidTest/",
        "tests/HostLifecycle.Tests/",
        "tests/contracts/",
        ".github/workflows/product-build.yml",
        "update/Step three/UPD-S1-019/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=add; affected_task_ids=T0150, T0176, T0196, T0197, T0198, T0199, T0200, T0201, T0202, T0203, T0204, T0205, T0206, T0207, T0208, T0229. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-020",
      "title": "اعتبارسنجی انتها‌به‌انتها روی دستگاه‌های واقعی",
      "section": "6. آزمون یکپارچه و تحویل نصب‌پذیر",
      "source_task_id": "UPD-S1-020",
      "source_change_operation": "add",
      "affected_task_ids": [
        "T0200",
        "T0201",
        "T0202",
        "T0203",
        "T0204",
        "T0205",
        "T0206",
        "T0223",
        "T0224",
        "T0225",
        "T0226",
        "T0227",
        "T0229"
      ],
      "description": "یک سناریوی عملیاتی کامل را از اپ اندروید تا مدل محلی واقعی روی میزبان ویندوز اجرا کن و پاسخ را به همان گفت‌وگو بازگردان. مسیر اصلی و fallback مصوب و شرایط شکست را با شواهد ثبت کن.",
      "depends_on": [
        "UPD-S1-019"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-020",
        "buildهای Android و Windows",
        "مدل محلی و backend واقعی",
        "شبکه و transport مصوب",
        "گزارش آزمون‌های UPD-S1-019",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-019"
      ],
      "outputs": [
        "گزارش end-to-end با دستگاه/نسخه/فرمان/نتیجه",
        "شواهد درخواست و پاسخ واقعی بدون افشای متن حساس",
        "نتایج قطع اتصال، میزبان مشغول، لغو و retry"
      ],
      "implementation_focus": "آزمون روی دستگاه فیزیکی Android و میزبان فیزیکی/واقعی Windows؛ fake transport فقط برای unit/integration جداگانه مجاز است.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-019 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. پیام از اندروید به میزبان ویندوز برسد و پاسخ مدل محلی واقعی به همان گفت‌وگو بازگردد.\n2. مسیر ارتباطی و هویت میزبان طبق تصمیم مصوب اعتبارسنجی شوند.\n3. سناریوهای قطع، لغو و retry نتیجهٔ پیش‌بینی‌پذیر داشته باشند.\n4. شواهد قابل بازبینی ثبت شوند و متن حساس در گزارش عمومی قرار نگیرد.",
      "acceptance_criteria": [
        "پیام از اندروید به میزبان ویندوز برسد و پاسخ مدل محلی واقعی به همان گفت‌وگو بازگردد.",
        "مسیر ارتباطی و هویت میزبان طبق تصمیم مصوب اعتبارسنجی شوند.",
        "سناریوهای قطع، لغو و retry نتیجهٔ پیش‌بینی‌پذیر داشته باشند.",
        "شواهد قابل بازبینی ثبت شوند و متن حساس در گزارش عمومی قرار نگیرد."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidWindowsLanIntegrationTest.kt",
        "tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs",
        "tests/HostLifecycle.Tests/Invoke-AndroidWindowsLanAcceptance.ps1",
        ".github/workflows/android-windows-lan-acceptance.yml",
        "update/Step three/UPD-S1-020/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=add; affected_task_ids=T0200, T0201, T0202, T0203, T0204, T0205, T0206, T0223, T0224, T0225, T0226, T0227, T0229. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-021",
      "title": "آماده‌سازی اسناد، بسته‌ها و شواهد تحویل",
      "section": "6. آزمون یکپارچه و تحویل نصب‌پذیر",
      "source_task_id": "UPD-S1-021",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0027",
        "T0207",
        "T0208",
        "T0209",
        "T0210",
        "T0211",
        "T0212",
        "T0213",
        "T0214",
        "T0215",
        "T0216",
        "T0217",
        "T0218",
        "T0228",
        "T0229"
      ],
      "description": "راهنمای استفادهٔ ساده برای Android و Windows، راهنمای عیب‌یابی فنی، محدودیت‌های انتشار و بسته‌های قابل نصب را بر اساس رفتار واقعی نسخه تهیه کن. انتشار را تا عبور از معیارهای پذیرش باز نکن.",
      "depends_on": [
        "UPD-S1-020",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-021",
        "خروجی UPD-S1-020",
        "نتایج CI و build",
        "نیازمندی‌ها و تصمیم‌های نهایی",
        "مجوز وابستگی‌ها و مدل نمونه",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-020, UPD-S1-024"
      ],
      "outputs": [
        "راهنمای نصب و استفادهٔ دو برنامه",
        "راهنمای عیب‌یابی و محدودیت‌های شناخته‌شده",
        "بسته‌های قابل نصب و checksum",
        "گزارش آمادگی انتشار"
      ],
      "implementation_focus": "بسته‌های installable واقعی با checksum و نصب/اجرای تمیز؛ وجود فایل build یا artifact به‌تنهایی release-ready نیست.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-020, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. بسته‌های Windows و Android از کد جاری تولید و روی محیط تمیز نصب/اجرا شده باشند.\n2. راهنماها با رابط واقعی و بدون دستورهای منسوخ سازگار باشند.\n3. checksum و مجوزهای وابستگی‌ها و مدل نمونه ثبت شده باشند.\n4. انتشار فقط پس از موفقیت معیارهای پذیرش و ثبت محدودیت‌های باقی‌مانده اعلام شود.",
      "acceptance_criteria": [
        "بسته‌های Windows و Android از کد جاری تولید و روی محیط تمیز نصب/اجرا شده باشند.",
        "راهنماها با رابط واقعی و بدون دستورهای منسوخ سازگار باشند.",
        "checksum و مجوزهای وابستگی‌ها و مدل نمونه ثبت شده باشند.",
        "انتشار فقط پس از موفقیت معیارهای پذیرش و ثبت محدودیت‌های باقی‌مانده اعلام شود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        ".github/workflows/android-release.yml",
        ".github/workflows/release-assets.yml",
        "tools/build_windows_installer.ps1",
        "README.md",
        "docs/",
        "update/Step three/UPD-S1-021/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0027, T0207, T0208, T0209, T0210, T0211, T0212, T0213, T0214, T0215, T0216, T0217, T0218, T0228, T0229. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-022",
      "title": "تعریف اعتماد اولیهٔ یک‌باره و مدیریت اعتماد ماندگار همتا",
      "section": "7. قابلیت‌های مشترک اعتماد، مدل و هویت بصری",
      "source_task_id": "UPD-S1-022",
      "source_change_operation": "modify",
      "affected_task_ids": [
        "T0130",
        "T0131",
        "T0132",
        "T0133",
        "T0134",
        "T0135",
        "T0136",
        "T0137",
        "T0143",
        "T0145",
        "T0186",
        "T0226"
      ],
      "description": "سیاست trust را طوری مشخص کن که اتصال‌های بعدی به همتای شناخته‌شده نیازمند تأیید تکراری برای هر درخواست نباشند، اما PeerId ناشناخته بدون اعتماد صریح یا onboarding امنِ مصوب پذیرفته نشود. نحوهٔ نمایش هویت در اولین enrollment، ذخیرهٔ تصمیم، reconnect، تغییر کلید/PeerId، revoke و حذف اعتماد را برای Android و Windows تعریف کن.",
      "depends_on": [
        "UPD-S1-001",
        "UPD-S1-007",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-022",
        "گزارش UPD-S1، بخش 2.2 و 8.3",
        "ADR-0018: Explicit peer trust and identity verification",
        "AndroidPeerTrustStore و Windows PeerTrustStore فعلی",
        "جریان discoverPeersForTrust و وضعیت UI فعلی",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001, UPD-S1-007, UPD-S1-024"
      ],
      "outputs": [
        "سیاست محصول/امنیت برای اولین اعتماد و اتصال‌های بعدی",
        "طرح onboarding و نمایش/تأیید هویت همتا در Android و Windows",
        "قواعد نگهداری، تغییر هویت، ابطال و حذف اعتماد",
        "سناریوهای آزمون برای unknown, trusted, changed-key, revoked و reconnect"
      ],
      "implementation_focus": "اولین اعتماد صریح، trust پایدار و revoke/mismatch fail-closed در هر دو پلتفرم.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001, UPD-S1-007, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. کاربر برای هر درخواست به تأیید دوبارهٔ PeerId مورد اعتماد نیاز نداشته باشد.\n2. PeerId ناشناخته خودکار trusted نشود؛ اولین اعتماد طبق onboarding مصوب و قابل مشاهده انجام شود.\n3. هویت مورد اعتماد، تغییر PeerId/کلید و revoke رفتار صریح و fail-closed داشته باشند.\n4. قواعد در Android و Windows سازگار و دارای آزمون برای enrollment، reconnect، mismatch و revoke باشند.\n5. هیچ اطلاعات هویتی غیرضروری در پیام عادی چت نمایش داده نشود.",
      "acceptance_criteria": [
        "کاربر برای هر درخواست به تأیید دوبارهٔ PeerId مورد اعتماد نیاز نداشته باشد.",
        "PeerId ناشناخته خودکار trusted نشود؛ اولین اعتماد طبق onboarding مصوب و قابل مشاهده انجام شود.",
        "هویت مورد اعتماد، تغییر PeerId/کلید و revoke رفتار صریح و fail-closed داشته باشند.",
        "قواعد در Android و Windows سازگار و دارای آزمون برای enrollment، reconnect، mismatch و revoke باشند.",
        "هیچ اطلاعات هویتی غیرضروری در پیام عادی چت نمایش داده نشود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt",
        "src/Shared.Security/PeerTrustStore.cs",
        "src/Host.Windows/Security/WindowsHostIdentityProvider.cs",
        "src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerTrustStoreTest.kt",
        "tests/HostLifecycle.Tests/WindowsPeerTrustStoreAcceptanceTests.cs",
        "update/Step three/UPD-S1-022/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0130, T0131, T0132, T0133, T0134, T0135, T0136, T0137, T0143, T0145, T0186, T0226. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-023",
      "title": "تعریف سیاست انتخاب خودکار مدل و سقف خروجی",
      "section": "7. قابلیت‌های مشترک اعتماد، مدل و هویت بصری",
      "source_task_id": "UPD-S1-023",
      "source_change_operation": "add",
      "affected_task_ids": [
        "T0057",
        "T0058",
        "T0059",
        "T0060",
        "T0061",
        "T0062",
        "T0063",
        "T0064",
        "T0065",
        "T0066",
        "T0067",
        "T0068",
        "T0069",
        "T0070",
        "T0071",
        "T0072",
        "T0073",
        "T0074",
        "T0075",
        "T0098",
        "T0099",
        "T0100",
        "T0101",
        "T0102",
        "T0103",
        "T0104",
        "T0105",
        "T0106",
        "T0107",
        "T0124",
        "T0196",
        "T0221"
      ],
      "description": "سیاست قطعی و قابل آزمونی برای انتخاب مدل و max output tokens هر درخواست تعریف و در لایهٔ دامنه/runtime پیاده‌سازی کن. ورودی سیاست باید نیاز درخواست، مدل‌های واقعاً نصب/آماده، capabilityهای معتبر میزبان، backend سازگار و محدودیت‌های محصول/پروتکل باشد. شناسهٔ فنی مدل و سقف خروجی نباید از کاربر عادی برای هر پیام خواسته شود. اولویت و tie-break میان مدل‌های سازگار، clamp سقف خروجی و رفتار نبود گزینهٔ معتبر را مشخص کن.",
      "depends_on": [
        "UPD-S1-001",
        "UPD-S1-005",
        "UPD-S1-006",
        "UPD-S1-024"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-023",
        "ممیزی خط مبنا از UPD-S1-001",
        "نیازمندی‌ها و تصمیم‌های مصوب از UPD-S1-006",
        "سیاست ظرفیت/مشارکت میزبان از UPD-S1-005",
        "Model catalog، HostCapabilities و live HostStatus در کد فعلی",
        "محدودیت‌های اندازه/توکن در قراردادهای Shared و backend محلی",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001, UPD-S1-005, UPD-S1-006, UPD-S1-024"
      ],
      "outputs": [
        "سیاست deterministic انتخاب مدل و tie-break",
        "resolver قابل استفاده در مسیر request runtime برای تعیین مدل سازگار",
        "سیاست محاسبه و clamp کردن max output tokens",
        "خطای typed و قابل فهم برای نبود مدل مناسب یا metadata نامعتبر",
        "آزمون‌های واحد برای چند مدل، مدل ناموجود، میزبان ناآماده و سقف خروجی"
      ],
      "implementation_focus": "مدل و max tokens از metadata واقعی و readiness انتخاب شوند؛ شناسهٔ مدل/سقف خروجی از کاربر عادی خواسته نشود.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001, UPD-S1-005, UPD-S1-006, UPD-S1-024 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. در مسیر عادی کاربر نیازی به واردکردن modelId یا maxOutputTokens نباشد.\n2. مدل انتخابی در catalog/installed state و live readiness میزبان با شناسهٔ دقیق سازگار باشد؛ رشته‌کاوی نام مدل یا advertised-only بودن کافی نباشد.\n3. انتخاب میان چند مدل سازگار deterministic باشد و ترجیح/اولویت محصول و tie-break ثبت‌شده داشته باشد.\n4. maxOutputTokens مثبت باشد و از کمترین سقف معتبر بین سیاست محصول، پروتکل و capability واقعی میزبان عبور نکند.\n5. اگر مدل سازگار یا metadata معتبر وجود ندارد، درخواست با خطای typed و قابل فهم متوقف شود؛ مدل نامعتبر یا پیش‌فرض حدسی انتخاب نشود.\n6. آزمون‌ها حالت صفر/یک/چند مدل، tie، backend ناآماده، model mismatch، metadata نامعتبر و clamp سقف خروجی را پوشش دهند.",
      "acceptance_criteria": [
        "در مسیر عادی کاربر نیازی به واردکردن modelId یا maxOutputTokens نباشد.",
        "مدل انتخابی در catalog/installed state و live readiness میزبان با شناسهٔ دقیق سازگار باشد؛ رشته‌کاوی نام مدل یا advertised-only بودن کافی نباشد.",
        "انتخاب میان چند مدل سازگار deterministic باشد و ترجیح/اولویت محصول و tie-break ثبت‌شده داشته باشد.",
        "maxOutputTokens مثبت باشد و از کمترین سقف معتبر بین سیاست محصول، پروتکل و capability واقعی میزبان عبور نکند.",
        "اگر مدل سازگار یا metadata معتبر وجود ندارد، درخواست با خطای typed و قابل فهم متوقف شود؛ مدل نامعتبر یا پیش‌فرض حدسی انتخاب نشود.",
        "آزمون‌ها حالت صفر/یک/چند مدل، tie، backend ناآماده، model mismatch، metadata نامعتبر و clamp سقف خروجی را پوشش دهند."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/",
        "src/Host.Application/IInferenceBackend.cs",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt",
        "tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs",
        "src/Client.Android/app/src/test/java/com/whitep/client/requests/",
        "update/Step three/UPD-S1-023/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=add; affected_task_ids=T0057, T0058, T0059, T0060, T0061, T0062, T0063, T0064, T0065, T0066, T0067, T0068, T0069, T0070, T0071, T0072, T0073, T0074, T0075, T0098, T0099, T0100, T0101, T0102, T0103, T0104, T0105, T0106, T0107, T0124, T0196, T0221. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    },
    {
      "id": "UPD-S1-024",
      "title": "تعریف و اعمال هویت بصری مشترک، آیکون P و پالت پنج‌رنگ در ویندوز و اندروید",
      "section": "7. قابلیت‌های مشترک اعتماد، مدل و هویت بصری",
      "source_task_id": "UPD-S1-024",
      "source_change_operation": "add",
      "affected_task_ids": [
        "T0015",
        "T0016",
        "T0041",
        "T0042",
        "T0046",
        "T0055",
        "T0070",
        "T0071",
        "T0088",
        "T0139",
        "T0143",
        "T0145",
        "T0210",
        "T0211",
        "T0216",
        "T0217",
        "T0219",
        "T0229"
      ],
      "description": "برای اپلیکیشن میزبان Windows و اپلیکیشن Android یک هویت بصری واحد پیاده‌سازی کن. آیکون هر دو برنامه باید تصویر یک سکهٔ سفید با حرف بزرگ انگلیسی P در مرکز آن باشد؛ ظاهر باید کلاسیک، زیبا، خلوت و در اندازه‌های کوچک قابل‌تشخیص بماند. مطابق visual_identity یک منبع برداری اصلی بساز و خروجی‌های هر دو پلتفرم را از همان منبع تولید کن. هر دو برنامه باید دقیقاً از پالت پنج‌رنگ و نقش‌های معنایی مشترک استفاده کنند؛ رنگ‌های پلتفرمی مستقل یا مقادیر پراکنده و هاردکدشده ایجاد نکن.",
      "depends_on": [
        "UPD-S1-001",
        "UPD-S1-006",
        "UPD-S1-008"
      ],
      "inputs": [
        "منبع نیازمندی: update/Step one/Output/V1.10.json — UPD-S1-024",
        "visual_identity در همین فایل V1.7.json",
        "step one/V1.0.md: T0015 و T0016 برای پشته‌های Windows و Android",
        "step one/V1.0.md: T0041 و T0042 برای نقطهٔ ورود و لایهٔ UI ویندوز",
        "step one/V1.0.md: T0139، T0143 و T0145 برای برنامه و صفحات اصلی اندروید",
        "ساختار واقعی دارایی‌ها، manifestها، پروژه‌های UI و تنظیمات build در کامیت مبنای main",
        "خروجی‌های تأییدشدهٔ پیش‌نیازها: UPD-S1-001, UPD-S1-006, UPD-S1-008"
      ],
      "outputs": [
        "یک منبع برداری اصلی و مستند برای نشان سکهٔ سفید با P بزرگ و حاشیهٔ کلاسیک",
        "خروجی‌های چنداندازهٔ Windows و دارایی‌های adaptive/density اندروید تولیدشده از همان منبع",
        "توکن‌های رنگی مرکزی و نگاشت نقش‌های معنایی یکسان برای هر دو برنامه",
        "استفادهٔ واقعی از تم مشترک در سطوح، متن‌ها، کنترل‌ها، وضعیت‌های انتخاب و اجزای قابل‌مشاهدهٔ هر دو اپلیکیشن",
        "شواهد مقایسهٔ بصری و آزمون خوانایی/کنتراست در اندازه‌ها و وضوح‌های هدف"
      ],
      "implementation_focus": "یک master vector؛ icon/theme واقعی در Windows و Android؛ کنتراست و build هر دو پلتفرم سنجیده شود.",
      "start_condition": "همهٔ پیش‌نیازهای UPD-S1-001, UPD-S1-006, UPD-S1-008 باید در همین گراف status=done داشته باشند و actual_output_paths و evidence و آزمون موفق آن‌ها تأیید شده باشد؛ خروجی قدیمی یا صرفاً مستنداتی کافی نیست.",
      "acceptance": "1. آیکون Windows و Android از نظر نشان، نسبت‌ها و ترکیب رنگ یکسان باشند: زمینهٔ سرمه‌ای، سکهٔ سفید، حرف بزرگ P به رنگ سرمه‌ای در مرکز و حاشیهٔ ظریف طلایی.\n2. حرف P در اندازه‌های کوچک launcher، taskbar و window icon واضح و از مرکز سکه قابل‌تشخیص باشد.\n3. خروجی‌های هر دو پلتفرم از یک منبع برداری اصلی تولید شده باشند؛ فایل‌های مستقل با طراحی متفاوت پذیرفته نیستند.\n4. هر دو برنامه دقیقاً از پنج رنگ تعریف‌شده در visual_identity استفاده کنند و هر توکن در هر دو پلتفرم نقش معنایی یکسان داشته باشد.\n5. رنگ‌های رابط در فایل‌های مرکزی/توکن‌های نام‌گذاری‌شده نگهداری شوند و رنگ‌های برند به‌صورت پراکنده یا مختص یک پلتفرم hard-code نشده باشند.\n6. کنتراست متن و کنترل‌ها در اندازه‌های واقعی بررسی شده باشد؛ طلایی روی زمینهٔ روشن برای متن کوچک استفاده نشود.\n7. صفحهٔ اصلی، فهرست/جزئیات گفت‌وگو، کنترل‌های اصلی و صفحات مهم میزبان در هر دو سیستم از همان تم پیروی کنند.\n8. build واقعی هر دو اپلیکیشن با دارایی‌های نهایی موفق باشد و تصاویر/گزارش بررسی launcher، taskbar و صفحات اصلی ثبت شوند.\n9. کنتراست متن معمولی در هر دو برنامه حداقل 4.5:1، متن بزرگ حداقل 3:1 و مرز/نشانگر بصری ضروری کنترل‌ها حداقل 3:1 باشد؛ این آستانه‌ها مطابق معیارهای کنتراست WCAG 2.2 AA سنجیده شوند.\n10. muted_teal روی porcelain برای متن معمولی استفاده نشود؛ secondary text روی سطوح اصلی روشن از midnight_ink استفاده کند. استفادهٔ متنی از muted_teal یا هر جفت‌رنگ دیگر فقط در صورت اثبات عددی آستانهٔ مربوط مجاز است.\n11. antique_gold روی سطوح روشن فقط نقش تزئینی/تأکیدی داشته باشد و نه متن کوچک یا تنها نشانگر دیداری یک کنترل/وضعیت ضروری.",
      "acceptance_criteria": [
        "آیکون Windows و Android از نظر نشان، نسبت‌ها و ترکیب رنگ یکسان باشند: زمینهٔ سرمه‌ای، سکهٔ سفید، حرف بزرگ P به رنگ سرمه‌ای در مرکز و حاشیهٔ ظریف طلایی.",
        "حرف P در اندازه‌های کوچک launcher، taskbar و window icon واضح و از مرکز سکه قابل‌تشخیص باشد.",
        "خروجی‌های هر دو پلتفرم از یک منبع برداری اصلی تولید شده باشند؛ فایل‌های مستقل با طراحی متفاوت پذیرفته نیستند.",
        "هر دو برنامه دقیقاً از پنج رنگ تعریف‌شده در visual_identity استفاده کنند و هر توکن در هر دو پلتفرم نقش معنایی یکسان داشته باشد.",
        "رنگ‌های رابط در فایل‌های مرکزی/توکن‌های نام‌گذاری‌شده نگهداری شوند و رنگ‌های برند به‌صورت پراکنده یا مختص یک پلتفرم hard-code نشده باشند.",
        "کنتراست متن و کنترل‌ها در اندازه‌های واقعی بررسی شده باشد؛ طلایی روی زمینهٔ روشن برای متن کوچک استفاده نشود.",
        "صفحهٔ اصلی، فهرست/جزئیات گفت‌وگو، کنترل‌های اصلی و صفحات مهم میزبان در هر دو سیستم از همان تم پیروی کنند.",
        "build واقعی هر دو اپلیکیشن با دارایی‌های نهایی موفق باشد و تصاویر/گزارش بررسی launcher، taskbar و صفحات اصلی ثبت شوند.",
        "کنتراست متن معمولی در هر دو برنامه حداقل 4.5:1، متن بزرگ حداقل 3:1 و مرز/نشانگر بصری ضروری کنترل‌ها حداقل 3:1 باشد؛ این آستانه‌ها مطابق معیارهای کنتراست WCAG 2.2 AA سنجیده شوند.",
        "muted_teal روی porcelain برای متن معمولی استفاده نشود؛ secondary text روی سطوح اصلی روشن از midnight_ink استفاده کند. استفادهٔ متنی از muted_teal یا هر جفت‌رنگ دیگر فقط در صورت اثبات عددی آستانهٔ مربوط مجاز است.",
        "antique_gold روی سطوح روشن فقط نقش تزئینی/تأکیدی داشته باشد و نه متن کوچک یا تنها نشانگر دیداری یک کنترل/وضعیت ضروری."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "assets/brand/whitep-coin.svg",
        "src/Host.Windows/Assets/WhiteP.ico",
        "src/Host.Windows/WhiteP.Host.Windows.csproj",
        "src/Host.Windows/App.xaml",
        "src/Host.Windows/MainWindow.xaml",
        "src/Client.Android/app/src/main/res/mipmap-anydpi-v26/ic_launcher.xml",
        "src/Client.Android/app/src/main/res/values/colors.xml",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/theme/",
        "update/Step three/UPD-S1-024/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=add; affected_task_ids=T0015, T0016, T0041, T0042, T0046, T0055, T0070, T0071, T0088, T0139, T0143, T0145, T0210, T0211, T0216, T0217, T0219, T0229. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 0,
        "failed_attempt_count": 0,
        "last_attempt_id": null,
        "last_attempt_result": null,
        "changed_files": [],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": []
    }
  ],
  "edges": [
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-002",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-002 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-003",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-003 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-004",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-004 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-004",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-004 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-005",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-005 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-006",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-006 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-003",
      "to": "UPD-S1-006",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-006 در V1.10، UPD-S1-003 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-004",
      "to": "UPD-S1-006",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-006 در V1.10، UPD-S1-004 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-005",
      "to": "UPD-S1-006",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-006 در V1.10، UPD-S1-005 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-007",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-007 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-005",
      "to": "UPD-S1-007",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-007 در V1.10، UPD-S1-005 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-008",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-008 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-007",
      "to": "UPD-S1-008",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-008 در V1.10، UPD-S1-007 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-009",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-009 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-008",
      "to": "UPD-S1-009",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-009 در V1.10، UPD-S1-008 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-009",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-009 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-009",
      "to": "UPD-S1-010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-010 در V1.10، UPD-S1-009 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-010 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-022",
      "to": "UPD-S1-010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-010 در V1.10، UPD-S1-022 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-023",
      "to": "UPD-S1-010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-010 در V1.10، UPD-S1-023 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-010 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-011",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-011 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-007",
      "to": "UPD-S1-011",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-011 در V1.10، UPD-S1-007 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-014",
      "to": "UPD-S1-011",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-011 در V1.10، UPD-S1-014 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-023",
      "to": "UPD-S1-011",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-011 در V1.10، UPD-S1-023 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-011",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-011 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-012",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-012 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-008",
      "to": "UPD-S1-012",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-012 در V1.10، UPD-S1-008 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-012",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-012 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-005",
      "to": "UPD-S1-013",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-013 در V1.10، UPD-S1-005 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-012",
      "to": "UPD-S1-013",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-013 در V1.10، UPD-S1-012 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-013",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-013 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-003",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-003 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-013",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-013 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-022",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-022 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-023",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-023 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-014 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-003",
      "to": "UPD-S1-015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-015 در V1.10، UPD-S1-003 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-014",
      "to": "UPD-S1-015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-015 در V1.10، UPD-S1-014 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-016",
      "to": "UPD-S1-015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-015 در V1.10، UPD-S1-016 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-015 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-015 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-016 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-007",
      "to": "UPD-S1-016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-016 در V1.10، UPD-S1-007 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-008",
      "to": "UPD-S1-016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-016 در V1.10، UPD-S1-008 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-016 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-022",
      "to": "UPD-S1-016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-016 در V1.10، UPD-S1-022 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-002",
      "to": "UPD-S1-017",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-017 در V1.10، UPD-S1-002 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-003",
      "to": "UPD-S1-017",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-017 در V1.10، UPD-S1-003 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-004",
      "to": "UPD-S1-017",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-017 در V1.10، UPD-S1-004 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-007",
      "to": "UPD-S1-018",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-018 در V1.10، UPD-S1-007 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-016",
      "to": "UPD-S1-018",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-018 در V1.10، UPD-S1-016 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-018",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-018 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-009",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-009 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-010",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-010 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-011",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-011 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-012",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-012 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-013",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-013 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-014",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-014 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-015",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-015 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-016",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-016 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-017",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-017 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-018",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-018 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-023",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-023 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-019 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-019",
      "to": "UPD-S1-020",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-020 در V1.10، UPD-S1-019 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-020",
      "to": "UPD-S1-021",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-021 در V1.10، UPD-S1-020 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-021",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-021 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-022",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-022 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-007",
      "to": "UPD-S1-022",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-022 در V1.10، UPD-S1-007 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-022",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-022 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-023 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-005",
      "to": "UPD-S1-023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-023 در V1.10، UPD-S1-005 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-023 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-024",
      "to": "UPD-S1-023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-023 در V1.10، UPD-S1-024 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-024",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-024 در V1.10، UPD-S1-001 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-006",
      "to": "UPD-S1-024",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-024 در V1.10، UPD-S1-006 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    },
    {
      "from": "UPD-S1-008",
      "to": "UPD-S1-024",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "validation_status": "unreviewed",
      "rationale": "UPD-S1-024 در V1.10، UPD-S1-008 را صریحاً پیش‌نیاز اعلام کرده است. این گراف رابطه را حفظ می‌کند؛ ماهیت فنی/قراردادی آن باید پیش از اجرای گسترده با کد و آزمون اعتبارسنجی شود.",
      "evidence": [
        "update/Step one/Output/V1.10.json"
      ]
    }
  ],
  "execution_order": [
    "UPD-S1-001",
    "UPD-S1-002",
    "UPD-S1-003",
    "UPD-S1-005",
    "UPD-S1-004",
    "UPD-S1-007",
    "UPD-S1-006",
    "UPD-S1-017",
    "UPD-S1-008",
    "UPD-S1-024",
    "UPD-S1-009",
    "UPD-S1-012",
    "UPD-S1-022",
    "UPD-S1-023",
    "UPD-S1-010",
    "UPD-S1-013",
    "UPD-S1-016",
    "UPD-S1-014",
    "UPD-S1-018",
    "UPD-S1-011",
    "UPD-S1-015",
    "UPD-S1-019",
    "UPD-S1-020",
    "UPD-S1-021"
  ],
  "execution_waves": [
    {
      "wave": 1,
      "task_ids": [
        "UPD-S1-001"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    },
    {
      "wave": 2,
      "task_ids": [
        "UPD-S1-002",
        "UPD-S1-003",
        "UPD-S1-005"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 3,
      "task_ids": [
        "UPD-S1-004",
        "UPD-S1-007"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 4,
      "task_ids": [
        "UPD-S1-006",
        "UPD-S1-017"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 5,
      "task_ids": [
        "UPD-S1-008"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    },
    {
      "wave": 6,
      "task_ids": [
        "UPD-S1-024"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    },
    {
      "wave": 7,
      "task_ids": [
        "UPD-S1-009",
        "UPD-S1-012",
        "UPD-S1-022",
        "UPD-S1-023"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 8,
      "task_ids": [
        "UPD-S1-010",
        "UPD-S1-013",
        "UPD-S1-016"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 9,
      "task_ids": [
        "UPD-S1-014",
        "UPD-S1-018"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 10,
      "task_ids": [
        "UPD-S1-011",
        "UPD-S1-015"
      ],
      "note": "این وظایف از نظر پیش‌نیازهای فعلی مستقل‌اند و فقط در صورت جدایی فایل/قرارداد و ظرفیت بازبین می‌توانند موازی اجرا شوند."
    },
    {
      "wave": 11,
      "task_ids": [
        "UPD-S1-019"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    },
    {
      "wave": 12,
      "task_ids": [
        "UPD-S1-020"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    },
    {
      "wave": 13,
      "task_ids": [
        "UPD-S1-021"
      ],
      "note": "این موج یک گره دارد؛ موج بعدی تا تأیید خروجی آن نباید از آن عبور کند."
    }
  ],
  "dependency_model": {
    "current_model": "explicit_prerequisite_graph_from_V1.10",
    "warning": "Source order is not execution order. For example UPD-S1-024 and UPD-S1-022/023 appear late in V1.10 but must complete before several UI, discovery and test consumers.",
    "edge_classifications": [
      "hard_dependency",
      "contract_or_interface_dependency",
      "sequencing_only",
      "unreviewed"
    ],
    "initial_edge_state": "unreviewed",
    "validation_required_before_broad_execution": true,
    "edge_change_record_required": true,
    "edge_change_record_fields": [
      "changed_at",
      "edge_from",
      "edge_to",
      "old_classification",
      "new_classification",
      "reason",
      "evidence",
      "reviewed_by"
    ],
    "parallelism_rule": "Only nodes without unresolved dependency paths may run in parallel. Shared files/contracts require a single owner or explicit coordination; run downstream consumer tests after a contract change.",
    "rule": "Explicit prerequisites from V1.10 are preserved. A prerequisite can be reclassified only with code-level rationale, consumer analysis and relevant test evidence; no edge is removed for convenience."
  },
  "execution_tracking_policy": {
    "purpose": "Make the update graph an executable plan and an append-only implementation ledger for the Android client, Windows host and shared runtime.",
    "rules": [
      "Before starting a node, verify every prerequisite node is done and its actual outputs/evidence exist in the recorded commit.",
      "Use the current repository tree and source files as baseline; audit before rewriting and prefer small compatible changes.",
      "Record every attempt, including failed, partial, blocked or reopened attempts; never delete earlier execution history.",
      "For each attempt record changed files, commit SHA or explicit not-committed statement, exact test command and observed result.",
      "A node is done only after its output files exist, implementation is non-placeholder, all node acceptance criteria are verified and tests/evidence are recorded.",
      "A report, decision, README or architecture document alone cannot make an implementation node done.",
      "For Android and Windows changes, run the relevant platform build and tests; when an environment cannot run them locally, record the limitation and use CI/device acceptance instead of pretending the test passed.",
      "After changing shared protocol, trust, privacy, status mapping, model selection or theme tokens, identify direct/transitive consumers and rerun their acceptance tests.",
      "For UPD-S1-020, run on a physical Android device and a real Windows host on a supported network; record device/OS/build/model/commands/results and redact sensitive payloads.",
      "For UPD-S1-021, install and launch generated packages on clean supported environments; record checksums and package provenance.",
      "Use actual_output_paths only for files confirmed in the repository at the referenced commit; keep expected paths separate.",
      "Update the graph in the same commit as implementation when practical, otherwise immediately after; reconcile status to repository state and test results."
    ],
    "required_attempt_fields": [
      "attempt_id",
      "started_at",
      "ended_at",
      "result",
      "summary",
      "changed_files",
      "commit_sha",
      "tests",
      "failure_details",
      "blocked_reason",
      "reopened_reason",
      "contract_changes",
      "follow_up"
    ],
    "allowed_attempt_results": [
      "succeeded",
      "failed",
      "partial",
      "blocked",
      "reopened"
    ],
    "contract_revalidation_states": [
      "not_required",
      "required",
      "in_review",
      "passed",
      "failed"
    ],
    "completion_test_rule": "Every done node must list at least one passing test or explicit acceptance verification, and evidence must include a test source file or CI/device run appropriate to its scope."
  },
  "product_acceptance_gates": [
    {
      "id": "GATE-ANDROID-01",
      "target": "Android client",
      "requirements": [
        "The existing Android app installs and launches on a supported physical Android device.",
        "A user can create/open a conversation, type and send a normal message without entering model ID, host IP, port or peer ID.",
        "Consent/trust and privacy onboarding occurs before the first request; meaningful request states and typed failures are visible.",
        "A real host response is returned and persisted in the same conversation; no hard-coded or mocked product response is used."
      ],
      "owners": [
        "UPD-S1-009",
        "UPD-S1-010",
        "UPD-S1-011",
        "UPD-S1-020",
        "UPD-S1-021"
      ]
    },
    {
      "id": "GATE-WINDOWS-01",
      "target": "Windows host",
      "requirements": [
        "The Windows host installs/launches on a supported Windows x64 environment.",
        "Host participation is opt-in and disabled by default; turning it off stops new admissions.",
        "A compatible local inference backend/model is actually ready before admission, and host capacity limits are enforced.",
        "The host processes a real request and returns the model output to the Android client."
      ],
      "owners": [
        "UPD-S1-012",
        "UPD-S1-013",
        "UPD-S1-020",
        "UPD-S1-021"
      ]
    },
    {
      "id": "GATE-INTEGRATION-01",
      "target": "Whole product",
      "requirements": [
        "A real Android-to-Windows request/response succeeds over the approved transport without an inference-content relay through the rendezvous service.",
        "Trust, identity mismatch/revoke, cancellation, timeout, unavailable/busy host and retry paths are verified.",
        "No prompt/response/history is exposed in ordinary logs or rendezvous metadata.",
        "Both apps use the same P coin icon source, five semantic color tokens and verified contrast pairings.",
        "Unit/integration tests, physical-device acceptance and clean package installation have evidence; no source-only or document-only completion."
      ],
      "owners": [
        "UPD-S1-014",
        "UPD-S1-015",
        "UPD-S1-016",
        "UPD-S1-017",
        "UPD-S1-018",
        "UPD-S1-019",
        "UPD-S1-020",
        "UPD-S1-021",
        "UPD-S1-022",
        "UPD-S1-023",
        "UPD-S1-024"
      ]
    }
  ],
  "progress_snapshot": {
    "as_of": "2026-11-07",
    "total_tasks": 24,
    "not_started": 24,
    "in_progress": 0,
    "done": 0,
    "blocked": 0,
    "verified_completion_percentage": 0,
    "calculation": "done nodes with real output and verified acceptance evidence / 24 * 100"
  },
  "visual_identity": {
    "scope": [
      "Windows desktop application",
      "Android application"
    ],
    "app_icon": {
      "concept": "A refined, classic coin emblem with a large uppercase English P centered on the coin.",
      "composition": {
        "canvas": "#142537",
        "coin_face": "#FFFFFF",
        "coin_edge": "#B89A62",
        "letter": "P",
        "letter_case": "uppercase",
        "letter_color": "#142537",
        "letter_position": "center",
        "style": "minimal, elegant, classical, high-contrast and recognizable at small sizes",
        "effects": "subtle dimensional rim only; avoid busy ornament, gradients that alter the palette, and fine detail that disappears at small sizes"
      },
      "source_of_truth": "One canonical vector master asset; generate platform-specific files from that master instead of drawing separate Android and Windows variants.",
      "platform_exports": {
        "windows": "A multi-resolution .ico set for executable, window, taskbar and installer identity, generated from the canonical master.",
        "android": "Adaptive launcher icon foreground/background and required density assets generated from the same canonical master; preserve Android adaptive-icon safe-zone rules."
      },
      "consistency": [
        "The coin, P letterform, proportions, rim and color relationship must remain visually identical across both platforms.",
        "Do not add a different monogram, wordmark or platform-specific symbol.",
        "Verify the icon at small launcher, taskbar and window sizes."
      ]
    },
    "color_palette": {
      "rule": "Use exactly these five shared brand colors as the core application palette in both applications; do not introduce platform-specific brand colors or arbitrary color literals.",
      "colors": [
        {
          "token": "midnight_ink",
          "hex": "#142537",
          "role": "Primary dark color, icon canvas, primary text and strong contrast."
        },
        {
          "token": "porcelain",
          "hex": "#F4F0E8",
          "role": "Shared light application background and main page surfaces."
        },
        {
          "token": "pure_white",
          "hex": "#FFFFFF",
          "role": "Elevated surfaces, cards and the coin face in the app icon."
        },
        {
          "token": "antique_gold",
          "hex": "#B89A62",
          "role": "Classic accent, selected-state details, borders and coin rim; not small body text on light surfaces."
        },
        {
          "token": "muted_teal",
          "hex": "#587C80",
          "role": "Secondary interactive accent and non-text emphasis; text use is allowed only on a background pairing that passes the applicable contrast threshold."
        }
      ],
      "usage_rules": [
        "Use exactly the five defined brand colors as the core palette in both applications; do not create platform-specific palettes or undocumented brand-color literals.",
        "Map the same semantic token to the same UI role in Windows and Android.",
        "Use midnight_ink for primary and normal-size secondary text on porcelain and pure_white surfaces.",
        "Use muted_teal as a secondary interactive accent or non-text emphasis; never use it as normal-size text on porcelain. Text use is permitted only where the measured foreground/background pairing meets the applicable threshold.",
        "Use antique_gold mainly for decorative emphasis and the coin rim; do not use it as small text or as the sole visual indicator of an essential control/state on light surfaces.",
        "Maintain at least 4.5:1 contrast for normal text, 3:1 for large text, and 3:1 for essential UI component boundaries and graphical state indicators, following WCAG 2.2 AA.",
        "Use icons, labels and text in addition to color to communicate state; do not rely on color alone.",
        "Do not add a sixth brand color to solve contrast; choose a compliant pairing among the five defined colors."
      ]
    },
    "theme": {
      "model": "One shared five-color design system and a consistent light visual theme across Windows and Android.",
      "shared_roles": {
        "app_background": "porcelain",
        "elevated_surface": "pure_white",
        "primary_text": "midnight_ink",
        "secondary_text": "midnight_ink",
        "primary_brand_accent": "antique_gold",
        "secondary_action": "muted_teal"
      },
      "implementation": "Centralize named color tokens and component styles so both applications consume the same documented values and semantic roles. Secondary text uses midnight_ink on the main light surfaces; muted_teal remains a secondary action/non-text accent unless its exact text pairing passes the applicable contrast threshold."
    },
    "accessibility": {
      "standard": "WCAG 2.2 Level AA contrast criteria used as the measurable design target.",
      "thresholds": {
        "normal_text": "at least 4.5:1",
        "large_text": "at least 3:1",
        "essential_ui_components_and_graphical_objects": "at least 3:1"
      },
      "verified_palette_pairings": {
        "muted_teal_on_porcelain": "approximately 4.01:1 — not permitted for normal-size text",
        "muted_teal_on_pure_white": "approximately 4.56:1 — text use only after final in-context verification",
        "midnight_ink_on_porcelain": "approximately 13.69:1",
        "midnight_ink_on_pure_white": "approximately 15.56:1",
        "antique_gold_on_porcelain": "approximately 2.36:1 — decorative only; not small text or sole essential indicator"
      },
      "rules": [
        "Check actual rendered foreground/background pairs in both applications, including focus, selected, hover, disabled and error states.",
        "Do not map muted_teal to normal-size secondary text on porcelain.",
        "Use midnight_ink for normal-size primary and secondary text on the principal light surfaces.",
        "Do not use antique_gold as small text or as the only visual cue for an essential control or state on light surfaces."
      ]
    }
  },
  "change_tracking": {
    "source_version": "V1.10",
    "fields": {
      "source_task_id": "Exact UPD-S1 task ID from V1.10.",
      "source_change_operation": "One of modify, add, remove, preserve, defer from V1.10.",
      "affected_task_ids": "Exact legacy Txxxx IDs in step one/V1.0.md; traceability only, not additional graph nodes."
    },
    "rule": "Do not renumber or silently drop tasks. Any change to task scope, prerequisite or affected legacy IDs must cite the updated source version and preserve previous execution records."
  }
}
```

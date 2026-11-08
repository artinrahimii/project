# گراف وابستگی و دفتر اجرای به‌روزرسانی White-P — v1.2

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

## اصلاحات کنترلی نسخهٔ ۱.۱

- مرجع main هنگام تهیهٔ این نسخه 827d1586e367d8495ef052ac71a8cc83db0f4680 است؛ مقایسهٔ commitها نشان داده V 3.1.12 فقط گراف Update را اضافه کرده و آخرین تغییر کد محصول 8fa9aa812aeb49086a6d55e92ba17586534135a4 باقی مانده است. SHA قدیمی داخل UPD-S1-001 حذف و قاعدهٔ خواندن HEAD واقعی هنگام اجرا اضافه شد.
- گراف Update با دو اجرای graph_guard در workflow کنترل کیفیت اعتبارسنجی می‌شود.
- آزمون LAN با پاسخ ثابت صرفاً آزمون کمکی transport است؛ UPD-S1-020 به آزمون فیزیکی UI Android تا مدل واقعی Windows و ذخیره در همان گفت‌وگو نیاز دارد.
- تمام ۱۶۸ شناسهٔ یکتای Txxxx متأثر در گراف معتبر Step Three با ۲۳۰ وظیفه پیدا و به UPD-S1های مربوط متصل شده‌اند. وضعیت هر ۱۶۸ وظیفه «بازبینی مجدد در انتظار» است؛ وضعیت‌های قبلی تا بررسی شواهد حفظ شده‌اند و UPD-S1-008 هنوز تکمیل‌شده نیست.
- UPD-S1-021 اکنون آزمون ارتقای درجا در Android و Windows و حفظ تاریخچه، تنظیمات، مدل محلی، هویت میزبان و trust را نیز الزامی می‌کند.

این اصلاحات معیارها و کنترل‌ها را تقویت می‌کنند و به معنی اجرای وظایف V1.10 یا انجام آزمون‌های فیزیکی نیستند.

## اصلاح reconciliation وظایف قدیمی در نسخهٔ ۱.۲

گراف Step Three در checkpoint مورد بررسی دارای ۲۳۰ وظیفه بود و هر ۱۶۸ شناسهٔ یکتای متأثر از V1.10 در آن پیدا شد. در V 3.1.14 هر گرهٔ متأثر با شناسه‌های UPD-S1 مرتبط و وضعیت «بازبینی مجدد در انتظار» علامت‌گذاری می‌شود. وضعیت اصلی هیچ وظیفه‌ای صرفاً به دلیل وجود در فهرست اثر تغییر نمی‌کند. فایل `update/Step two/legacy-task-reconciliation.json` فهرست کامل این ارتباط را نگه می‌دارد.

این مرحله نگاشت و علامت‌گذاری را انجام می‌دهد، نه بازبینی محتوایی همهٔ ۱۶۸ وظیفه؛ تا ثبت شواهد برای هر مورد، UPD-S1-008 کامل نیست. همچنین regex آزمون قراردادی در CI اصلاح شده است.

## قالب ویرایش

دادهٔ کامل در بلوک JSON زیر، قابل پردازش ماشینی و سازگار با `tools/graph_guard.py` است. هنگام به‌روزرسانی، ساختار JSON را حفظ کن و فقط وضعیت‌ها/تلاش‌ها/یال‌های دارای شاهد را به‌روزرسانی کن.

## دادهٔ کامل گراف v1.0

```json
{
  "schema_version": "1.0",
  "document": "update-step-two-v1.2",
  "title": "گراف وابستگی و دفتر اجرای به‌روزرسانی White-P بر پایهٔ V1.10 — v1.2",
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
    "base_commit_sha": "827d1586e367d8495ef052ac71a8cc83db0f4680",
    "base_commit_message": "V 3.1.12",
    "purpose": "Update graph for the current integrated Windows host, Android client and shared components; not a plan to recreate the initial product.",
    "base_commit_role": "Latest main checkpoint when v1.1 was prepared; V 3.1.12 added only the update graph.",
    "product_code_baseline_sha": "8fa9aa812aeb49086a6d55e92ba17586534135a4",
    "product_code_baseline_message": "V 3.1.11",
    "baseline_delta_summary": "The verified comparison from V 3.1.11 to V 3.1.12 contains exactly one added file, update/Step two/project-graph.md. No Android, Windows, shared runtime or test source was changed. The product-code baseline therefore remains V 3.1.11 while the graph-creation main checkpoint is V 3.1.12."
  },
  "commit_intent": "V 3.1.37",
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
    "no_mock_completion": "Mocks may support isolated tests only. A mocked response, hard-coded response, sample-only code or stub cannot satisfy a real product acceptance gate.",
    "base_commit_policy": "base_repository.base_commit_sha is the main-branch checkpoint used to prepare this graph revision. At task execution, resolve and record the real current main HEAD; audit all commits after the checkpoint. product_code_baseline_sha identifies the last product-code revision when the checkpoint commit changed only graph metadata. The original v1.1 checkpoint remains historical; this v1.2 correction does not rewrite the product-code baseline.",
    "legacy_task_sync_policy": "affected_task_ids are mapped to real Step Three nodes. Every impacted legacy node must carry update_impact with the corresponding UPD-S1 IDs and remain pending re-review until behavior and evidence are inspected. Traceability metadata alone never changes legacy status.",
    "real_e2e_policy": "The real product E2E gate must traverse the Android chat UI, Android runtime, approved direct P2P transport, installed Windows host, loaded local inference model and the same Android Room conversation. Transport fixtures and standalone model tests are supplemental only.",
    "upgrade_policy": "Release acceptance requires in-place upgrade tests on both platforms in addition to clean installation. Preserve or explicitly migrate Room history, settings, local model integrity/paths, host identity and peer trust. Never use uninstall/clear-data as an upgrade test."
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
        "کامیت main هنگام ایجاد گراف v1.1: 827d1586e367d8495ef052ac71a8cc83db0f4680 — V 3.1.12؛ این کامیت فقط گراف Update را اضافه کرد.",
        "کد و فایل‌های build پروژه",
        "step one/V1.0.md",
        "گزارش update/Step one/2026-11-07-automatic-chat-and-host-routing.md",
        "آخرین کامیت تغییر‌دهندهٔ کد محصول در checkpoint گراف: 8fa9aa812aeb49086a6d55e92ba17586534135a4 — V 3.1.11؛ V 3.1.12 فقط فایل گراف را اضافه کرد."
      ],
      "outputs": [
        "گزارش موجودی قابلیت‌های واقعی در Android و Windows",
        "نقشهٔ مسیر فعلی پیام، کشف میزبان و inference",
        "دستورهای build/test موجود و نتیجهٔ اجرای واقعی یا دلیل اجرا نشدن آن‌ها",
        "فهرست فاصله‌های کد با نیازمندی پیشنهادی"
      ],
      "implementation_focus": "ممیزی واقعی و بدون فرض وضعیت فعلی؛ خروجی این گره نقشهٔ مسیر و شواهد خط مبناست.",
      "start_condition": "در زمان اجرا ابتدا آخرین SHA واقعی شاخهٔ main را دریافت و در گزارش ممیزی ثبت کن. SHA داخل base_repository نقطهٔ ثبت نسخهٔ ۱.۱ گراف است، نه فرضی دربارهٔ HEAD آینده. تغییرات main پس از این checkpoint را بررسی کن؛ اگر کد محصول تغییر کرده، ممیزی را روی HEAD واقعی انجام بده و SHA/tree دقیق را ثبت کن. SHA تاریخی 6f70586b4b7ff69c463f7e5b02293090c63e78ec ورودی منسوخ است و نباید مبنای ممیزی باشد. محتوای V1.10 را نیز با blob SHA ثبت‌شده در بخش source تطبیق بده.",
      "acceptance": "1. مسیر واقعی پروژه‌های Android، Windows و Shared از روی مخزن مشخص شده باشد.\n2. جریان فعلی درخواست از UI تا میزبان و بازگشت پاسخ با ارجاع به فایل‌ها ثبت شده باشد.\n3. هر build یا test فقط در صورت اجرای واقعی دارای فرمان و نتیجه باشد.\n4. موارد ناموجود یا اثبات‌نشده صریحاً از قابلیت‌های پیاده‌سازی‌شده جدا شده باشند.\n5. SHA واقعی main در لحظهٔ ممیزی ثبت شده و با checkpoint ایجاد گراف و آخرین تغییر کد محصول اشتباه گرفته نشده باشد.\n6. هیچ ورودی ممیزی از SHA تاریخی منسوخ 6f70586b4b7ff69c463f7e5b02293090c63e78ec استفاده نکند.",
      "acceptance_criteria": [
        "مسیر واقعی پروژه‌های Android، Windows و Shared از روی مخزن مشخص شده باشد.",
        "جریان فعلی درخواست از UI تا میزبان و بازگشت پاسخ با ارجاع به فایل‌ها ثبت شده باشد.",
        "هر build یا test فقط در صورت اجرای واقعی دارای فرمان و نتیجه باشد.",
        "موارد ناموجود یا اثبات‌نشده صریحاً از قابلیت‌های پیاده‌سازی‌شده جدا شده باشند.",
        "SHA واقعی main در لحظهٔ ممیزی ثبت شده و با checkpoint ایجاد گراف و آخرین تغییر کد محصول اشتباه گرفته نشده باشد.",
        "هیچ ورودی ممیزی از SHA تاریخی منسوخ 6f70586b4b7ff69c463f7e5b02293090c63e78ec استفاده نکند."
      ],
      "status": "done",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
      ],
      "actual_output_paths": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md",
        "tests/test_update_baseline_audit.py"
      ],
      "evidence": [
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md",
        "tests/test_update_baseline_audit.py",
        "https://github.com/artinrahimii/project/actions/runs/42401485764",
        "https://github.com/artinrahimii/project/actions/runs/42401485830",
        "https://github.com/artinrahimii/project/actions/runs/42401485784",
        "https://github.com/artinrahimii/project/actions/runs/42408460108"
      ],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=preserve; affected_task_ids=T0015, T0016, T0041, T0042, T0139, T0216, T0217. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 3,
        "failed_attempt_count": 0,
        "last_attempt_id": "UPD-S1-001-20261107195525",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "tests/test_update_baseline_audit.py",
          "update/Step three/UPD-S1-001/execution-report.md",
          "update/Step three/UPD-S1-001/evidence/baseline-audit.md",
          "update/Step two/project-graph.md"
        ],
        "related_commits": [
          "760c3882fe6a6e680c149d0b374790e26db7a4a9",
          "c10b59eb6ede67261643e6994f2c08c18fa591ec"
        ],
        "tests_passed": [
          "Base commit quality run 42401485764 passed: Python tests, compileall and both graph guards.",
          "Base commit product build run 42401485830 passed: Windows WPF build/smoke/installer/lifecycle and Android lint/build/JVM/emulator tests.",
          "Base commit Rendezvous container run 42401485784 passed.",
          "Correction commit quality run 42408460108 passed: 458 Python tests, compileall, legacy graph guard (0 errors, 1 preserved metadata warning), update graph guard (0 errors, 0 warnings).",
          "New tests/test_update_baseline_audit.py passed in run 42408460108."
        ],
        "tests_failed": [
          "CI run 42408388222: audit test expected ASCII group separator; evidence correctly used Persian thousands separator.",
          "CI run 42408388222: audit test expected explicit SemaphoreSlim constructor; source uses target-typed new(1, 1)."
        ],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": [
        {
          "attempt_id": "UPD-S1-001-20261107195322",
          "started_at": "2026-11-07T19:53:22.752Z",
          "ended_at": "2026-11-07T19:53:22.752Z",
          "result": "partial",
          "summary": "Baseline code, graph, protocol and CI audit completed; new regression tests have been added but are awaiting execution on the resulting commit.",
          "changed_files": [
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-001/execution-report.md",
            "update/Step three/UPD-S1-001/evidence/baseline-audit.md",
            "tests/test_update_baseline_audit.py"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Baseline quality gate",
              "command": "python -m unittest discover -s tests -v; python -m compileall -q tools tests; python tools/graph_guard.py on both graphs",
              "result": "passed",
              "run_id": 42401485764,
              "url": "https://github.com/artinrahimii/project/actions/runs/42401485764",
              "note": "Run belongs to audited base commit; added regression test not included."
            },
            {
              "name": "Baseline Windows and Android product build",
              "command": "workflow product-build.yml",
              "result": "passed",
              "run_id": 42401485830,
              "url": "https://github.com/artinrahimii/project/actions/runs/42401485830",
              "note": "Windows and Android CI tasks passed; emulator evidence is not physical-device E2E."
            },
            {
              "name": "Baseline Rendezvous container smoke",
              "command": "docker build and tests/rendezvous_container_smoke.py",
              "result": "passed",
              "run_id": 42401485784,
              "url": "https://github.com/artinrahimii/project/actions/runs/42401485784"
            },
            {
              "name": "New baseline audit regression test",
              "command": "python -m unittest tests.test_update_baseline_audit -v",
              "result": "pending",
              "note": "Will run after this commit triggers the quality workflow."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Verify the newly added baseline audit regression suite and graph guard on the resulting main commit; then record its exact commit SHA in the next graph update and reassess done criteria.",
          "device_and_host_details_redacted": "Not applicable to a source-and-CI baseline audit; no physical Android device or separate Windows host was accessed.",
          "model_revision_and_sha256": "Not applicable to UPD-S1-001.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable to UPD-S1-001."
        },
        {
          "attempt_id": "UPD-S1-001-20261107195420",
          "started_at": "2026-11-07T19:54:20.340Z",
          "ended_at": "2026-11-07T19:54:20.340Z",
          "result": "partial",
          "summary": "The first CI run exposed two false-negative expectations in the newly added audit tests. Both assertions were corrected against the exact repository content; a clean CI rerun is pending.",
          "changed_files": [
            "tests/test_update_baseline_audit.py",
            "update/Step three/UPD-S1-001/execution-report.md",
            "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "First baseline regression CI run",
              "command": "python -m unittest discover -s tests -v",
              "result": "failed",
              "run_id": 42408388222,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408388222",
              "failure_details": [
                "Audit assertion expected ASCII 4,014,945; evidence used Persian grouping separator.",
                "Audit assertion expected explicit SemaphoreSlim(1, 1); implementation used _inferenceSemaphore = new(1, 1)."
              ]
            },
            {
              "name": "Corrected baseline regression CI",
              "command": "python -m unittest discover -s tests -v",
              "result": "pending",
              "note": "Awaiting CI on the correction commit."
            }
          ],
          "failure_details": "Two audit-only assertions did not match the exact evidence/source formatting. They were corrected; no Android, Windows or Shared product source was changed.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Wait for the correction commit's quality workflow; if green, record the verified commit SHA in a follow-up metadata update and reassess UPD-S1-001 acceptance.",
          "device_and_host_details_redacted": "Not applicable to source audit; no physical device or separate Windows host accessed.",
          "model_revision_and_sha256": "Not applicable to UPD-S1-001.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable to UPD-S1-001."
        },
        {
          "attempt_id": "UPD-S1-001-20261107195525",
          "started_at": "2026-11-07T19:55:25.306Z",
          "ended_at": "2026-11-07T19:55:25.306Z",
          "result": "succeeded",
          "summary": "Baseline audit evidence, source-flow findings and regression checks passed. The initial two audit assertion mismatches were corrected in V 3.1.19; the full 458-test suite and both update-graph guard modes then passed.",
          "changed_files": [
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-001/execution-report.md",
            "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
          ],
          "commit_sha": "c10b59eb6ede67261643e6994f2c08c18fa591ec",
          "tests": [
            {
              "name": "Full Python quality suite",
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "run_id": 42408460108,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408460108",
              "summary": "Ran 458 tests; OK."
            },
            {
              "name": "Python syntax check",
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "run_id": 42408460108,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408460108"
            },
            {
              "name": "Legacy graph guard",
              "command": "python tools/graph_guard.py step three/project-graph.md and --repo-root .",
              "result": "passed_with_warning",
              "run_id": 42408460108,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408460108",
              "summary": "0 errors; 1 warning preserving legacy T0001 metadata string as unmapped dependency."
            },
            {
              "name": "Update graph guard",
              "command": "python tools/graph_guard.py update/Step two/project-graph.md and --repo-root .",
              "result": "passed",
              "run_id": 42408460108,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408460108",
              "summary": "24 tasks, 80 edges; 0 errors and 0 warnings."
            },
            {
              "name": "Baseline audit regression tests",
              "command": "included in python -m unittest discover -s tests -v",
              "result": "passed",
              "run_id": 42408460108,
              "url": "https://github.com/artinrahimii/project/actions/runs/42408460108",
              "summary": "All 6 tests in UpdateBaselineAuditTests passed."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Proceed to UPD-S1-002 as the next dependency-ready task; validate every edge before changing its classification and record architectural decisions with source-level evidence.",
          "device_and_host_details_redacted": "Not applicable to UPD-S1-001; this task is a repository baseline audit, not a physical product acceptance test.",
          "model_revision_and_sha256": "Not applicable to UPD-S1-001.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable to UPD-S1-001."
        }
      ]
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
      "status": "done",
      "status_fa": "تکمیل‌شده",
      "expected_output_paths": [
        "docs/decisions/",
        "update/Step three/UPD-S1-002/execution-report.md"
      ],
      "actual_output_paths": [
        "docs/decisions/ADR-0002-direct-p2p-data-path.md",
        "docs/decisions/README.md",
        "tests/test_update_transport_trust_boundary.py",
        "update/Step three/UPD-S1-002/execution-report.md",
        "update/Step three/UPD-S1-002/evidence/verification.md",
        "docs/decisions/ADR-0004-encrypted-relay-boundary.md"
      ],
      "evidence": [
        "update/Step three/UPD-S1-002/execution-report.md",
        "update/Step three/UPD-S1-002/evidence/verification.md",
        "tests/test_update_transport_trust_boundary.py",
        "tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs",
        "docs/decisions/ADR-0004-encrypted-relay-boundary.md",
        "docs/decisions/ADR-0002-direct-p2p-data-path.md",
        "docs/decisions/README.md",
        "https://github.com/artinrahimii/project/actions/runs/42410600411",
        "https://github.com/artinrahimii/project/actions/runs/42410600404",
        "https://github.com/artinrahimii/project/actions/runs/42410600403"
      ],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0019, T0118, T0119, T0120, T0129, T0130, T0131, T0132, T0133, T0134, T0206, T0226, T0227. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 6,
        "failed_attempt_count": 2,
        "last_attempt_id": "UPD-S1-002-20261107203120",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "update/Step two/project-graph.md",
          "update/Step three/UPD-S1-002/execution-report.md",
          "update/Step three/UPD-S1-002/evidence/verification.md"
        ],
        "related_commits": [
          "47a597c7e0e5df14234e717dd10685d6816c6e40",
          "f6c153ee44a509493a6d88d901a32956af7b0f6e"
        ],
        "tests_passed": [
          {
            "name": "V3.1.22 Rendezvous container",
            "run_id": 42409867998,
            "result": "passed"
          },
          {
            "name": "V3.1.23 Rendezvous container build and smoke",
            "run_id": 42410464340,
            "result": "passed"
          },
          {
            "name": "Step Three quality gate",
            "command": ".github/workflows/quality.yml on exact commit V 3.1.24",
            "result": "passed",
            "run_id": 42410600411,
            "url": "https://github.com/artinrahimii/project/actions/runs/42410600411",
            "summary": "All steps passed: Python unittest suite, compileall, legacy graph structure and output/evidence guards, update graph structure and output/evidence guards."
          },
          {
            "name": "Windows and Android product build",
            "command": ".github/workflows/product-build.yml on exact commit V 3.1.24",
            "result": "passed",
            "run_id": 42410600404,
            "url": "https://github.com/artinrahimii/project/actions/runs/42410600404",
            "summary": "Windows WPF restore/build/publish/startup smoke, trust-storage acceptance and installer acceptance passed; Android clean debug/release build, lint, JVM tests, Keystore instrumentation tests, protocol generation and coverage checks passed; HostLifecycle build/tests/coverage passed."
          },
          {
            "name": "Rendezvous container build and smoke",
            "command": ".github/workflows/rendezvous-container.yml on exact commit V 3.1.24",
            "result": "passed",
            "run_id": 42410600403,
            "url": "https://github.com/artinrahimii/project/actions/runs/42410600403",
            "summary": "Container build and service smoke workflow completed successfully."
          }
        ],
        "tests_failed": [
          {
            "name": "V3.1.22 Step Three quality gate",
            "run_id": 42409868004,
            "result": "failed",
            "summary": "464 tests, 9 assertion failures, 455 passed."
          },
          {
            "name": "V3.1.23 Step Three quality gate",
            "run_id": 42410464261,
            "result": "failed",
            "summary": "459 tests; one failure and one error due missing closing JSON fence."
          }
        ],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": []
        }
      },
      "execution_history": [
        {
          "attempt_id": "UPD-S1-002-20261107201128",
          "started_at": "2026-11-07T20:11:28.347Z",
          "ended_at": "2026-11-07T20:11:28.347Z",
          "result": "partial",
          "summary": "Direct authenticated P2P selected as primary; rendezvous is metadata-only; relay remains disabled and direct failure is fail-closed. Reviewed actual Windows/Android/rendezvous sources, retention behavior and relevant legacy reports. Updated ADR and decision register and added source-contract regression checks; exact-commit CI remains pending.",
          "changed_files": [
            "docs/decisions/ADR-0002-direct-p2p-data-path.md",
            "docs/decisions/README.md",
            "tests/test_update_transport_trust_boundary.py",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Update transport/trust boundary regression tests",
              "command": "python -m unittest discover -s tests -v",
              "result": "pending",
              "note": "Run on the commit containing this task's changes."
            },
            {
              "name": "Rendezvous and host loopback acceptance",
              "command": "dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release",
              "result": "pending",
              "note": "Run on the exact task commit; no local execution claimed."
            },
            {
              "name": "Python syntax and graph validation",
              "command": "python -m compileall -q tools tests; python tools/graph_guard.py on both graphs",
              "result": "pending",
              "note": "Run on the exact task commit."
            },
            {
              "name": "Windows and Android CI builds",
              "command": "workflow product-build.yml",
              "result": "pending",
              "note": "Build result for the task commit not yet available."
            },
            {
              "name": "Rendezvous container smoke",
              "command": "workflow rendezvous-container.yml",
              "result": "pending",
              "note": "Container run for task commit not yet available."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Observe exact-commit CI. If all relevant checks pass, record the exact SHA and run IDs in a follow-up graph update and complete UPD-S1-002. Preserve AC-P2P-01 and deployment-log retention as separate unverified gates.",
          "device_and_host_details_redacted": "No physical Android device or separate Windows host was accessed. This task is an architecture/source/loopback-boundary decision.",
          "model_revision_and_sha256": "Not applicable to UPD-S1-002.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable to UPD-S1-002."
        },
        {
          "attempt_id": "UPD-S1-002-20261107201143",
          "started_at": "2026-11-07T20:11:43Z",
          "ended_at": "2026-11-07T20:11:50Z",
          "result": "failed",
          "summary": "The exact V3.1.22 quality gate ran 464 tests and exposed nine assertion mismatches: legacy ADR wording contracts lost during the rewrite and new source assertions that did not match actual identifiers/comments. No product implementation source was changed by the ADR update.",
          "changed_files": [
            "docs/decisions/ADR-0002-direct-p2p-data-path.md",
            "docs/decisions/README.md",
            "tests/test_update_transport_trust_boundary.py",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": "c5542cdc74072b135fb3d7af88a856e65faf421e",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "python -m unittest discover -s tests -v",
              "result": "failed",
              "run_id": 42409868004,
              "url": "https://github.com/artinrahimii/project/actions/runs/42409868004",
              "summary": "464 tests; 9 assertion failures; 455 passed."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": "workflow rendezvous-container.yml",
              "result": "passed",
              "run_id": 42409867998,
              "url": "https://github.com/artinrahimii/project/actions/runs/42409867998"
            },
            {
              "name": "Windows WPF build",
              "command": "workflow product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42409868020
            },
            {
              "name": "HostLifecycle tests",
              "command": "workflow product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42409868020
            },
            {
              "name": "Android APK and emulator checks",
              "command": "workflow product-build.yml / Build Android client APK",
              "result": "pending",
              "run_id": 42409868020,
              "note": "Job was still running at last check."
            }
          ],
          "failure_details": [
            "The prior ADR test suite required exact direct-P2P, metadata-only control-plane, connection-unavailable and no-silent-relay statements.",
            "New assertions incorrectly expected a Kotlin symbol named expectedPeerIdentity, an uncapitalized renewal comment and text not matching ADR-0004; AC-P2P-01 was wrapped in Markdown code ticks."
          ],
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Restore the established policy phrases and correct new tests to exact production-source identifiers; rerun CI on a new main commit.",
          "device_and_host_details_redacted": "Windows WPF, HostLifecycle and Rendezvous CI passed; no physical Android handset or separate external Windows host was used.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-002-20261107201850",
          "started_at": "2026-11-07T20:18:50.328Z",
          "ended_at": "2026-11-07T20:18:50.328Z",
          "result": "partial",
          "summary": "Restored established direct-P2P and relay-fail-closed ADR phrases and corrected the new source-contract assertions to actual Android peer ID comparison, renewal comment, ADR-0004 visibility wording and Markdown task label. Exact-commit CI is pending.",
          "changed_files": [
            "docs/decisions/ADR-0002-direct-p2p-data-path.md",
            "tests/test_update_transport_trust_boundary.py",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "python -m unittest discover -s tests -v; python -m compileall -q tools tests; graph_guard on both graphs",
              "result": "pending"
            },
            {
              "name": "Windows and Android product build",
              "command": "workflow product-build.yml",
              "result": "pending"
            },
            {
              "name": "Rendezvous container smoke",
              "command": "workflow rendezvous-container.yml",
              "result": "pending"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Check the exact corrective commit's quality, product and Rendezvous workflows; store real SHA/run evidence in a later graph update.",
          "device_and_host_details_redacted": "No physical Android device or separate Windows host was used.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-002-20261107201905",
          "started_at": "2026-11-07T20:19:05Z",
          "ended_at": "2026-11-07T20:19:13Z",
          "result": "failed",
          "summary": "The exact V3.1.23 quality workflow ran 459 Python tests and reported one failure plus one error because the update graph Markdown JSON code fence had no closing delimiter. The first complete JSON object remained intact and could be recovered; the structural wrapper was malformed by the previous graph rewrite.",
          "changed_files": [
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md"
          ],
          "commit_sha": "c6b4b082df9241afbc490381ab3ac8fa93ec4c71",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "python -m unittest discover -s tests -v; graph fence and graph_guard checks",
              "result": "failed",
              "run_id": 42410464261,
              "url": "https://github.com/artinrahimii/project/actions/runs/42410464261",
              "summary": "459 tests; 1 failure and 1 error caused by the missing closing JSON fence in update/Step two/project-graph.md."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": "workflow rendezvous-container.yml",
              "result": "passed",
              "run_id": 42410464340,
              "url": "https://github.com/artinrahimii/project/actions/runs/42410464340"
            }
          ],
          "failure_details": [
            "The closing Markdown JSON fence was not emitted. Two quality checks reported the same structural problem: JSON graph fence not closed / unclosed JSON fence. The first balanced JSON object was complete; a duplicated trailing old JSON body was removed during recovery."
          ],
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Restore one complete JSON block and closing fence, verify the parsed node/edge counts and rerun both graph_guard modes and the full exact-head quality gate.",
          "device_and_host_details_redacted": "Rendezvous container passed. Product build was still in progress. No physical handset or separate Windows device used.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-002-20261107202026",
          "started_at": "2026-11-07T20:20:26.226Z",
          "ended_at": "2026-11-07T20:20:26.226Z",
          "result": "partial",
          "summary": "Recovered the first balanced JSON object (24 nodes/80 edges) from the malformed Markdown file, removed only the duplicated trailing JSON body, and restored the exact closing Markdown fence. The history records the graph-formatting failure and the repair; exact-commit validation is pending.",
          "changed_files": [
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Balanced graph JSON extraction and parse",
              "command": "brace-balanced extraction followed by JSON.parse; verify 24 nodes and 80 edges",
              "result": "passed",
              "summary": "Extracted JSON length 206406 characters; 24 nodes and 80 edges."
            },
            {
              "name": "Graph guard and quality gate",
              "command": "python tools/graph_guard.py on both graphs; python -m unittest discover -s tests -v",
              "result": "pending",
              "note": "Must pass on exact repair commit."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Check V3.1.24 exact-commit graph guard, complete Python quality suite, Windows/Android builds and Rendezvous container workflow; then update task status only after all task2 acceptance conditions are verified.",
          "device_and_host_details_redacted": "No physical Android or external Windows device involved.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-002-20261107203120",
          "started_at": "2026-11-07T20:31:20.309Z",
          "ended_at": "2026-11-07T20:31:20.309Z",
          "result": "succeeded",
          "summary": "Final exact-head review confirmed the approved direct authenticated P2P data path, metadata-only rendezvous boundary, fail-closed behavior with relay disabled, explicit metadata visibility/retention limits, and required legacy-task cross-reference. Exact V 3.1.24 quality, product-build and rendezvous workflows all passed. No physical Android/Windows device test or external provider-log audit is claimed; those are separate product acceptance gates.",
          "changed_files": [
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-002/execution-report.md",
            "update/Step three/UPD-S1-002/evidence/verification.md"
          ],
          "commit_sha": "f6c153ee44a509493a6d88d901a32956af7b0f6e",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": ".github/workflows/quality.yml on exact commit V 3.1.24",
              "result": "passed",
              "run_id": 42410600411,
              "url": "https://github.com/artinrahimii/project/actions/runs/42410600411",
              "summary": "All steps passed: Python unittest suite, compileall, legacy graph structure and output/evidence guards, update graph structure and output/evidence guards."
            },
            {
              "name": "Windows and Android product build",
              "command": ".github/workflows/product-build.yml on exact commit V 3.1.24",
              "result": "passed",
              "run_id": 42410600404,
              "url": "https://github.com/artinrahimii/project/actions/runs/42410600404",
              "summary": "Windows WPF restore/build/publish/startup smoke, trust-storage acceptance and installer acceptance passed; Android clean debug/release build, lint, JVM tests, Keystore instrumentation tests, protocol generation and coverage checks passed; HostLifecycle build/tests/coverage passed."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": ".github/workflows/rendezvous-container.yml on exact commit V 3.1.24",
              "result": "passed",
              "run_id": 42410600403,
              "url": "https://github.com/artinrahimii/project/actions/runs/42410600403",
              "summary": "Container build and service smoke workflow completed successfully."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Continue with UPD-S1-003 in graph order. Keep physical Android-to-Windows acceptance and deployment-provider logging review as independent gates.",
          "device_and_host_details_redacted": "No physical Android handset or separate Windows host was accessed in this attempt; CI used hosted Windows and Android emulator/instrumentation environments.",
          "model_revision_and_sha256": "Not applicable to UPD-S1-002.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable to UPD-S1-002."
        }
      ]
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
      "status": "done",
      "status_fa": "انجام‌شده",
      "expected_output_paths": [
        "docs/decisions/",
        "update/Step three/UPD-S1-003/execution-report.md"
      ],
      "actual_output_paths": [
        "docs/decisions/ADR-0022-single-host-sequential-retry.md",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
        "src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt",
        "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt",
        "update/Step three/UPD-S1-003/execution-report.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.36.md"
      ],
      "evidence": [
        "update/Step three/UPD-S1-003/evidence/decision-matrix.md",
        "update/Step three/UPD-S1-003/evidence/ci-baseline.md",
        "tests/test_android_distributed_request_contract.py",
        "tests/test_android_host_candidate_selector_contract.py",
        "tests/test_android_inference_attempt_budget_acceptance.py",
        "tests/test_android_manual_retry_contract.py",
        "tests/test_request_retry_budget_boundary.py",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.30.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.31.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.32.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.33.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.34.md",
        "update/Step three/UPD-S1-003/evidence/ci-3.1.36.md"
      ],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0125, T0126, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0203, T0204, T0224, T0225. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است. New decision ADR-0022 records the first-release one-host/sequential-retry policy. The generic three-host selector and dispatcher remain compatibility primitives and do not change the production runtime's one-host admission boundary. The implementation attempt is partial pending exact-commit CI. V 3.1.29 quality gate found five residual contract/documentation assertions; the exact tests and register row were corrected in the next partial attempt. Do not mark done until CI passes. چهار معیار پذیرش با CI موفق روی SHA دقیق V3.1.36 تأیید و این گره بسته شد؛ آزمون فیزیکی Android-to-Windows و upgrade در این تأیید ادعا نشده است.",
      "execution_summary": {
        "attempt_count": 10,
        "failed_attempt_count": 6,
        "last_attempt_id": "UPD-S1-003-20261108073419",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
          "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
          "src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt",
          "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt",
          "tests/test_android_distributed_request_contract.py",
          "tests/test_android_host_candidate_selector_contract.py",
          "tests/test_android_inference_attempt_budget_acceptance.py",
          "tests/test_android_manual_retry_contract.py",
          "tests/test_request_retry_budget_boundary.py",
          "docs/decisions/README.md",
          "docs/decisions/ADR-0022-single-host-sequential-retry.md",
          "update/Step three/UPD-S1-003/evidence/decision-matrix.md",
          "update/Step three/UPD-S1-003/evidence/ci-baseline.md",
          "update/Step three/UPD-S1-003/execution-report.md",
          "tests/test_android_inference_round_response_collector_contract.py",
          "update/Step two/project-graph.md",
          "update/Step three/UPD-S1-003/evidence/ci-3.1.30.md",
          "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt",
          "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt",
          "tests/test_android_libp2p_transport_contract.py"
        ],
        "related_commits": [
          "89042d5feba3c3859d63a7527aac0c7b2669cb88",
          "cb6050e9b6e9f8fd60d2266efbf57af2534a16c0",
          "719f2a4daf0b82c013c617a1a0a4778c625c474c",
          "10303ad6faddd7d4a50096d702d0e9f4a10e2b7d",
          "b881d980419be1998e32eeb6a55d0d13d5a4cc3e",
          "40d9128be6d3bd0df3531d44b893946f24e82961",
          "b8c664315ba3071080e91190ba79eef7ae58caa7",
          "d027e72f3968ef7c8c826f889042eb67a8533fa4"
        ],
        "tests_passed": [
          "Rendezvous container build and smoke",
          "Host lifecycle unit and integration tests",
          "Step Three quality gate — run 42417548666",
          "Windows WPF build and installer — run 42417548687",
          "HostLifecycle unit and integration tests — run 42417548687",
          "Rendezvous container build and smoke — run 42417548709",
          "Step Three quality gate — run 42455296797",
          "Rendezvous container build and smoke — run 42455296891",
          "Windows WPF build/publish/startup/installer — run 42455296829",
          "HostLifecycle tests — run 42455296829",
          "Step Three quality gate — run 42455653912",
          "Rendezvous container build/smoke — run 42455653892",
          "Windows WPF build and installer — run 42455653927",
          "HostLifecycle unit/integration and coverage — run 42455653927",
          "Android ktlint — run 42455653927",
          "Android debug/release APK build and permission checks — run 42455653927",
          "Step Three quality gate — run 42456615858",
          "Rendezvous container build/smoke — run 42456615879",
          "Windows WPF build and installer — run 42456615891",
          "HostLifecycle unit/integration and coverage — run 42456615891",
          "Windows WPF build and installer — run 42456934059",
          "HostLifecycle unit/integration and coverage — run 42456934059",
          "Android ktlint — run 42456934059",
          "Android debug/release APK and merged-manifest permissions — run 42456934059",
          "V3.1.36 exact-SHA quality PASS (42457409001), Product Build PASS (42457409040; Android JVM/Keystore instrumentation, Windows WPF/installer and HostLifecycle), Rendezvous PASS (42457409058)."
        ],
        "tests_failed": [
          "Step Three quality gate",
          "Android JVM unit tests — 33 of 213 failed in run 42417548687",
          "Android ktlintTestSourceSetCheck — run 42455296829; JVM/instrumentation tests not run",
          "Android JVM :app:testDebugUnitTest — 5 of 214 failed in run 42455653927",
          "Android ktlintTestSourceSetCheck — run 42456615891; APK/JVM/instrumentation not run",
          "Step Three Python quality gate — five stale contract-test failures in run 42456934121",
          "Android :app:compileDebugUnitTestKotlin — unresolved InferenceHostAttempt.roundId at two transport test references in run 42456934059; JVM methods not run"
        ],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": [
            "update/Step three/UPD-S1-003/evidence/decision-matrix.md",
            "update/Step three/UPD-S1-003/evidence/ci-3.1.36.md"
          ]
        }
      },
      "execution_history": [
        {
          "attempt_id": "UPD-S1-003-20261107203548",
          "started_at": "2026-11-07T20:35:48.198Z",
          "ended_at": "2026-11-07T20:35:48.198Z",
          "result": "partial",
          "summary": "Baseline and design audit started. Reviewed V1.10 task UPD-S1-003, the 8.2 routing proposal, Android selector/dispatcher/response collector/timer/ViewModel, direct transport cancellation, Windows request lifecycle, and all 23 affected legacy task records. Existing implementation dispatches up to three hosts concurrently per round with up to nine attempts per request; manual retries are user-triggered. No production changes have been made in this attempt.",
          "changed_files": [],
          "commit_sha": null,
          "tests": [],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Approve a precise first-release cardinality and sequential retry policy, map each affected legacy task to preserve/rework/future scope, then change the live Android request path and tests. Retain generic multi-host primitives only where compatibility/future-scope is explicit.",
          "device_and_host_details_redacted": "Source and CI audit only; no physical Android handset or external Windows host was available.",
          "model_revision_and_sha256": "Not applicable to design audit start.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-003-20261107214517",
          "started_at": "2026-11-07T21:45:17.833Z",
          "ended_at": "2026-11-07T21:45:17.833Z",
          "result": "partial",
          "summary": "Selected and documented first-release policy: one active peer per logical request, at most three sequential attempts across three rounds, manual retry only, one shared absolute deadline, terminal cancellation and correlation-safe late-response rejection. Corrected DistributedRequest validation and factory defaults so the logical request cannot exceed three total hosts. Retained generic three-host selector/dispatcher compatibility. Updated Android domain/UI tests and source-contract guards; fixed two baseline ktlint violations. Exact-commit CI has not yet run.",
          "changed_files": [
            "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
            "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
            "src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt",
            "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt",
            "tests/test_android_distributed_request_contract.py",
            "tests/test_android_host_candidate_selector_contract.py",
            "tests/test_android_inference_attempt_budget_acceptance.py",
            "tests/test_android_manual_retry_contract.py",
            "tests/test_request_retry_budget_boundary.py",
            "docs/decisions/README.md",
            "docs/decisions/ADR-0022-single-host-sequential-retry.md",
            "update/Step three/UPD-S1-003/evidence/decision-matrix.md",
            "update/Step three/UPD-S1-003/evidence/ci-baseline.md",
            "update/Step three/UPD-S1-003/execution-report.md"
          ],
          "commit_sha": "89042d5feba3c3859d63a7527aac0c7b2669cb88",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "python -m unittest discover -s tests -v; python -m compileall -q tools tests; graph_guard and graph output validation",
              "result": "failed",
              "run_id": 42417360215,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417360215",
              "summary": "464 tests; five residual source-contract failures: generic selector compatibility constant, two outdated ViewModel test-name references, two outdated retry/cancel marker references, and ADR-0022 table formatting."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": "workflow rendezvous-container.yml",
              "result": "passed",
              "run_id": 42417360202,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417360202"
            },
            {
              "name": "Windows WPF build",
              "command": "workflow product-build.yml / Build Windows WPF host",
              "result": "pending",
              "run_id": 42417360194
            },
            {
              "name": "Android client APK",
              "command": "workflow product-build.yml / Build Android client APK",
              "result": "pending",
              "run_id": 42417360194
            },
            {
              "name": "Host lifecycle unit and integration tests",
              "command": "workflow product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42417360194,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417360194"
            }
          ],
          "failure_details": "V 3.1.29 quality CI failed with five residual Python source-contract errors. The tests still expected the selector's deprecated first-release constant rather than MultiHostCompatibilityLimits; the ViewModel collector contract expected old multi-host test names; the manual retry contract expected removed multi-host test names; and the ADR-0022 decision register row had escaped literal newlines and short source paths. Rendezvous workflow and HostLifecycle suite passed; Android APK and Windows WPF build were still in progress at the last status check.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "FirstReleaseInferenceLimits: three rounds, one active host per round, three total sequential attempts.",
            "DistributedRequest maxTotalHosts validation/default now uses the first-release single-host policy rather than generic three-host compatibility limits.",
            "Production Android runtime remains capped at one selected host.",
            "Generic MultiHostCompatibilityLimits remains three hosts per batch for compatibility-only selector/dispatcher usage."
          ],
          "follow_up": "V 3.1.30 corrects the five source-contract/register issues while retaining the generic multi-host selector/dispatcher tests. Re-run exact commit quality and product build workflows before considering UPD-S1-003 complete.",
          "device_and_host_details_redacted": "Source, test and CI-log inspection only. No physical Android device or external Windows host was attached to this execution.",
          "model_revision_and_sha256": "Not applicable; local model inference not run.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; install/upgrade acceptance not run."
        },
        {
          "attempt_id": "UPD-S1-003-20261107214742",
          "started_at": "2026-11-07T21:47:42.902Z",
          "ended_at": "2026-11-07T21:47:42.902Z",
          "result": "partial",
          "summary": "Corrected the five residual V 3.1.29 quality-gate contract failures: selector budget tests now distinguish generic MultiHostCompatibilityLimits; collector/ViewModel contract tests target the actual one-host success/failure and single-attempt cancellation test names; manual-retry guards expect the implemented single-host sequential test names; and ADR-0022 is now a valid five-column register row with full repository-relative source/test paths.",
          "changed_files": [
            "tests/test_android_inference_attempt_budget_acceptance.py",
            "tests/test_android_inference_round_response_collector_contract.py",
            "tests/test_android_manual_retry_contract.py",
            "docs/decisions/README.md"
          ],
          "commit_sha": null,
          "tests": [],
          "failure_details": "This repair is committed pending exact-commit CI. The prior commit V 3.1.29 had five Python quality failures; the Android APK and Windows WPF jobs were still in progress, although HostLifecycle tests and Rendezvous container build had passed.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Generic selector's MAX_HOSTS_PER_ROUND is tested against MultiHostCompatibilityLimits, not the first-release domain constant.",
            "ViewModel UI contract markers now match the one-host product flow; generic multi-host collector tests remain unchanged.",
            "ADR-0022 register row uses exactly five columns and full existing repository-relative source/test paths."
          ],
          "follow_up": "Run quality, Android APK, Windows WPF and HostLifecycle workflows for this commit. Only after all exact-commit checks pass and acceptance evidence is verified can UPD-S1-003 be considered for done.",
          "device_and_host_details_redacted": "GitHub CI only; no physical Android handset or independent Windows host was available for a real-device product test.",
          "model_revision_and_sha256": "Not applicable; no real local model inference executed.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; clean install and in-place upgrade were not executed."
        },
        {
          "attempt_id": "UPD-S1-003-20261107214746",
          "started_at": "2026-11-07T21:47:46Z",
          "ended_at": "2026-11-07T21:54:06Z",
          "result": "failed",
          "summary": "V 3.1.30: Step Three quality gate and Rendezvous passed; Windows WPF build and HostLifecycle tests passed. Android debug/release APK builds and permission checks passed, but the Android JVM suite failed with 33 of 213 tests failing. All failures originate from DistributedRequest rejecting generic compatibility fixtures with total budgets of 4, 6 or 9, even though the production RequestViewModel/runtime separately enforce three sequential attempts and one selected peer per round. UPD-S1-003 remains in_progress.",
          "changed_files": [
            "docs/decisions/README.md",
            "tests/test_android_inference_attempt_budget_acceptance.py",
            "tests/test_android_inference_round_response_collector_contract.py",
            "tests/test_android_manual_retry_contract.py",
            "update/Step three/UPD-S1-003/evidence/ci-baseline.md",
            "update/Step three/UPD-S1-003/execution-report.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": "cb6050e9b6e9f8fd60d2266efbf57af2534a16c0",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "GitHub Actions workflow .github/workflows/quality.yml",
              "result": "passed",
              "run_id": 42417548666,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417548666",
              "summary": "Completed successfully on cb6050e9b6e9f8fd60d2266efbf57af2534a16c0."
            },
            {
              "name": "Android JVM unit tests",
              "command": "product-build.yml / Build Android client APK / :app:testDebugUnitTest",
              "result": "failed",
              "run_id": 42417548687,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417548687",
              "summary": "213 tests completed; 33 failed with IllegalArgumentException in DistributedRequest-based compatibility fixtures. APK debug/release, lint and permission checks passed before this step; instrumentation and artifact upload steps were skipped."
            },
            {
              "name": "Windows WPF build and installer",
              "command": "product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42417548687,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417548687",
              "summary": "Clean build, publish, startup smoke, identity/trust checks and installer acceptance passed."
            },
            {
              "name": "Host lifecycle unit and integration tests",
              "command": "product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42417548687,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417548687",
              "summary": "HostLifecycle build, tests and coverage validation passed."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": "GitHub Actions workflow .github/workflows/rendezvous-container.yml",
              "result": "passed",
              "run_id": 42417548709,
              "url": "https://github.com/artinrahimii/project/actions/runs/42417548709",
              "summary": "Completed successfully on the exact V 3.1.30 commit."
            }
          ],
          "failure_details": "The 33 Android JVM failures are constructor exceptions from compatibility fixtures: HostCandidateSelectorTest and InferenceAttemptDispatcherTest use generic budgets up to nine attempts, and Libp2pInferenceRequestTransportTest uses six attempts across two rounds. The first-release runtime remains separately constrained: RequestViewModel builds a maximum three-attempt budget, AndroidInferenceRequestRuntime accepts one selected peer per round, and maxRounds is at most three. The common value-object validation was too restrictive for compatibility tests.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "The V 3.1.30 Android JVM run exposed a regression between the generic compatibility budget model and the first-release request policy.",
            "Repair must permit generic budgets up to MultiHostCompatibilityLimits while preserving the first-release default and runtime one-host-per-round enforcement."
          ],
          "follow_up": "Align the shared DistributedRequest budget envelope with MultiHostCompatibilityLimits, keep RequestViewModel and AndroidInferenceRequestRuntime first-release controls unchanged, then rerun exact-commit Android JVM tests, quality gate, Windows build and HostLifecycle tests. Do not mark UPD-S1-003 done until all acceptance evidence passes.",
          "device_and_host_details_redacted": "No physical Android handset or external Windows host was attached. CI Windows runner performed build/startup smoke only; no real Android-to-Windows local-model E2E was run.",
          "model_revision_and_sha256": "Not applicable; no local model inference was run.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; no in-place upgrade was run."
        },
        {
          "attempt_id": "UPD-S1-003-20261108063417",
          "started_at": "2026-11-08T06:34:17Z",
          "ended_at": "2026-11-08T06:37:23Z",
          "result": "failed",
          "summary": "V 3.1.31 exact-commit CI: quality gate and Rendezvous container passed; Windows WPF build/publish/startup/installer and HostLifecycle tests passed. The Android job failed at ktlintTestSourceSetCheck before Android APK/JVM/instrumentation steps. Source inspection identified a duplicate blank line between JVM test declarations in DistributedRequestTest.kt; the same commit also exposed a stale ADR description of the generic budget boundary. Both are corrected in this follow-up; UPD-S1-003 remains in_progress.",
          "changed_files": [
            "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
            "src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt",
            "tests/test_android_distributed_request_contract.py",
            "tests/test_android_inference_attempt_budget_acceptance.py",
            "tests/test_request_retry_budget_boundary.py",
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-003/execution-report.md",
            "update/Step three/UPD-S1-003/evidence/ci-3.1.30.md"
          ],
          "commit_sha": "719f2a4daf0b82c013c617a1a0a4778c625c474c",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "GitHub Actions .github/workflows/quality.yml on V 3.1.31",
              "result": "passed",
              "run_id": 42455296797,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455296797",
              "summary": "Exact commit quality gate completed successfully."
            },
            {
              "name": "Rendezvous container build and smoke",
              "command": "GitHub Actions .github/workflows/rendezvous-container.yml on V 3.1.31",
              "result": "passed",
              "run_id": 42455296891,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455296891",
              "summary": "Exact commit container build and smoke completed successfully."
            },
            {
              "name": "Windows WPF build/publish/startup/installer",
              "command": "product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42455296829,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455296829",
              "summary": "Exact commit Windows build, publish, smoke, and installer checks completed successfully."
            },
            {
              "name": "HostLifecycle tests",
              "command": "product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42455296829,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455296829",
              "summary": "Exact commit host lifecycle suite and coverage passed."
            },
            {
              "name": "Android ktlint",
              "command": "product-build.yml / Build Android client APK / gradle --no-daemon ktlintCheck",
              "result": "failed",
              "run_id": 42455296829,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455296829",
              "summary": "ktlintTestSourceSetCheck failed. Android APK build, Android JVM tests, Keystore instrumentation, and Android artifacts did not run after the lint gate failed."
            }
          ],
          "failure_details": "The failed task was :app:ktlintTestSourceSetCheck, not the Android tests themselves. The fetched source at V 3.1.31 contained two consecutive blank lines before supportsNineAttemptsForGenericCompatibilityCallers() in DistributedRequestTest.kt (lines 90-91). This was introduced in the V 3.1.31 test edit and has been reduced to one blank line. The ADR-0022 budget paragraph also still claimed that the domain object rejected all budgets above three; that is stale because the shared value object now supports bounded generic compatibility up to nine while RequestViewModel/AndroidInferenceRequestRuntime retain the first-release three-sequential/one-per-round limits. The ADR is corrected in this follow-up.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "V 3.1.31 verified generic DistributedRequest budget acceptance at source/contract level, but its Android formatting gate failed before JVM tests.",
            "The shared domain object accepts generic compatibility budgets up to three rounds x three hosts; default first-release request creation remains capped at three total attempts.",
            "AndroidInferenceRequestRuntime continues to reject more than one selected host per product round; the product retry policy remains sequential and user-triggered."
          ],
          "follow_up": "Run quality, Android ktlint, Android JVM tests, Android Keystore instrumentation, Windows build, HostLifecycle tests, and Rendezvous checks on the next exact commit. Do not mark UPD-S1-003 done until all acceptance criteria have fresh passing evidence.",
          "device_and_host_details_redacted": "GitHub Actions runner evidence only. No physical Android handset or externally installed Windows host was attached; no physical Android-to-Windows model inference or install/upgrade test was run.",
          "model_revision_and_sha256": "Not applicable; no local model inference was executed.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; no in-place upgrade was executed."
        },
        {
          "attempt_id": "UPD-S1-003-20261108063938",
          "started_at": "2026-11-08T06:39:38Z",
          "ended_at": "2026-11-08T06:45:30Z",
          "result": "failed",
          "summary": "V 3.1.32 exact-commit CI: quality gate, Rendezvous, Windows WPF build/installer, HostLifecycle suite/coverage, Android lint, debug/release APK builds, and merged-manifest permission checks passed. Android JVM unit tests ran 214 tests and failed 5. Four failures were legacy libp2p transport tests that submitted three peers in one call to AndroidInferenceRequestRuntime, which correctly rejected the product-path batch with TOO_MANY_HOSTS. The fifth was a generic dispatcher compatibility test comparing its nine-host budget with the first-release three-attempt constant. Android Keystore instrumentation was skipped after the JVM failure.",
          "changed_files": [
            "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt",
            "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"
          ],
          "commit_sha": "10303ad6faddd7d4a50096d702d0e9f4a10e2b7d",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": ".github/workflows/quality.yml on V 3.1.32",
              "result": "passed",
              "run_id": 42455653912,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653912",
              "summary": "Exact commit quality gate passed."
            },
            {
              "name": "Rendezvous container build/smoke",
              "command": ".github/workflows/rendezvous-container.yml on V 3.1.32",
              "result": "passed",
              "run_id": 42455653892,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653892",
              "summary": "Exact commit container build and smoke passed."
            },
            {
              "name": "Windows WPF build and installer",
              "command": "product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Clean Windows build, publish, startup smoke, identity/trust gates and installer acceptance passed."
            },
            {
              "name": "HostLifecycle unit/integration and coverage",
              "command": "product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Host lifecycle suite and coverage validation passed."
            },
            {
              "name": "Android lint",
              "command": "gradle --no-daemon ktlintCheck",
              "result": "passed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Kotlin formatting and lint passed."
            },
            {
              "name": "Android debug and release APK builds",
              "command": ":app:assembleDebug and release variant using a disposable CI signing key",
              "result": "passed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Both variants built; temporary signing key removed."
            },
            {
              "name": "Android merged manifest permission checks",
              "command": "Verify minimum permissions in merged Android APK",
              "result": "passed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Minimum required permissions validation passed."
            },
            {
              "name": "Android JVM unit tests",
              "command": ":app:testDebugUnitTest",
              "result": "failed",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "214 tests completed; five failed: four tests passed three peers to the single-host production runtime and one generic dispatcher test compared nine compatibility attempts with the three-attempt product limit."
            },
            {
              "name": "Android Keystore instrumentation",
              "command": "Connected Android instrumentation test task",
              "result": "skipped",
              "run_id": 42455653927,
              "url": "https://github.com/artinrahimii/project/actions/runs/42455653927",
              "summary": "Skipped after JVM test failure."
            }
          ],
          "failure_details": "The JUnit HTML report artifact 13608070526 (ZIP SHA-256 23142a5339df2e98e969b8bf6b6c2738a4bc3095a0803ddc372a2a474d9020cd) identifies four tests that submit three peers in one AndroidInferenceRequestRuntime.dispatchRound and are rejected with TOO_MANY_HOSTS, plus one generic dispatcher test comparing nine compatibility attempts to FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS=3. The first-release runtime restriction remains intentional. Repairs keep transport/integration tests to one selected peer and explicit subsequent attempts only after timeout; generic dispatcher tests explicitly request the generic nine-attempt budget.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "AndroidInferenceRequestRuntime remains limited to one selected peer per round and three total first-release attempts.",
            "Kotlin and .NET transport tests no longer submit multiple peers in one product round; the failover test advances only after the previous peer's response window times out.",
            "Generic dispatcher compatibility tests use MultiHostCompatibilityLimits.MAX_TOTAL_HOSTS explicitly."
          ],
          "follow_up": "Re-run exact-commit Android lint, JVM, Keystore instrumentation, APK builds, Windows and HostLifecycle checks. Do not mark UPD-S1-003 done until all acceptance evidence passes.",
          "device_and_host_details_redacted": "CI runner only. Native .NET interop uses local peer-process fixtures; no external physical Windows host or model was used. No physical Android handset or in-place upgrade was tested.",
          "model_revision_and_sha256": "Not applicable; no local model inference executed.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; no install or upgrade was run."
        },
        {
          "attempt_id": "UPD-S1-003-20261108065423",
          "started_at": "2026-11-08T06:54:23Z",
          "ended_at": "2026-11-08T06:58:08Z",
          "result": "failed",
          "summary": "V 3.1.33 exact-commit CI: Windows WPF build/installer and HostLifecycle suite/coverage passed; Android failed at ktlintTestSourceSetCheck before APK build, JVM tests, or Keystore instrumentation. The workflow did not upload the ktlint report. Exact-commit source inspection found two 121-character Kotlin test-source lines, now wrapped in this follow-up. The five V 3.1.32 JVM failures were not rerun on V 3.1.33 because lint stopped the job before the JVM step.",
          "changed_files": [
            "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt",
            "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"
          ],
          "commit_sha": "b881d980419be1998e32eeb6a55d0d13d5a4cc3e",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": ".github/workflows/quality.yml on V 3.1.33",
              "result": "passed",
              "run_id": 42456615858,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456615858",
              "summary": "Exact commit quality gate passed."
            },
            {
              "name": "Rendezvous container build/smoke",
              "command": ".github/workflows/rendezvous-container.yml on V 3.1.33",
              "result": "passed",
              "run_id": 42456615879,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456615879",
              "summary": "Exact commit container build and smoke passed."
            },
            {
              "name": "Windows WPF build and installer",
              "command": "product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42456615891,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456615891",
              "summary": "Windows clean build, publish/startup checks, host identity/trust and installer acceptance passed."
            },
            {
              "name": "HostLifecycle unit/integration and coverage",
              "command": "product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42456615891,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456615891",
              "summary": "Host lifecycle suite and coverage validation passed."
            },
            {
              "name": "Android ktlint",
              "command": "gradle --no-daemon ktlintCheck",
              "result": "failed",
              "run_id": 42456615891,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456615891",
              "summary": "ktlintTestSourceSetCheck failed. Debug/release APK, JVM unit tests and Keystore instrumentation did not run after lint stopped the job."
            }
          ],
          "failure_details": "The V 3.1.33 Android job failed specifically at :app:ktlintTestSourceSetCheck. The workflow did not upload the report. Source inspection of the exact commit identified two Kotlin test-source lines of 121 characters: the capsule expression in the native .NET sequential failover test and the generic dispatcher request factory call. Both are line-wrapped in this follow-up. The five V 3.1.32 JVM failures were not re-tested in V 3.1.33 because lint stopped the job first.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Production Android runtime remains one selected peer per request round and at most three first-release sequential attempts.",
            "V 3.1.33 changed transport tests to one-host product dispatch and sequential failover, while generic dispatcher tests explicitly use MultiHostCompatibilityLimits.",
            "V 3.1.33 follow-up only wraps two long Kotlin test lines; no product runtime code is loosened."
          ],
          "follow_up": "Run exact-commit Android lint, debug/release APK, JVM suite, Keystore instrumentation, Windows and HostLifecycle checks. Do not mark UPD-S1-003 done until all required tests pass.",
          "device_and_host_details_redacted": "CI runners only. No physical Android device, external target Windows host, local model inference, or in-place upgrade was used.",
          "model_revision_and_sha256": "Not applicable; no local inference model loaded.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable; no installation/upgrade tested."
        },
        {
          "attempt_id": "UPD-S1-003-20261108065938",
          "started_at": "2026-11-08T06:59:38Z",
          "ended_at": "2026-11-08T06:59:44Z",
          "result": "failed",
          "summary": "V 3.1.34 exact-commit Step Three quality gate ran 464 Python tests and reported five contract subtest failures. The failures were stale test expectations for the generic nine-attempt dispatcher marker, escaped tab source literals in Kotlin, removed multi-host test names and attemptStatusSummary, and an outdated runtimeClient variable name. Product build on the same commit was still running when this quality result was recorded; Windows/Android product results are tracked separately after that workflow completes.",
          "changed_files": [
            "tests/test_android_inference_attempt_budget_acceptance.py",
            "tests/test_android_libp2p_transport_contract.py",
            "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt"
          ],
          "commit_sha": "40d9128be6d3bd0df3531d44b893946f24e82961",
          "tests": [
            {
              "name": "Step Three Python quality gate",
              "command": ".github/workflows/quality.yml / Run automated tests",
              "result": "failed",
              "run_id": 42456934121,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934121",
              "summary": "464 tests ran; five source-contract subtest failures were stale assertions. Graph validation, output validation and subsequent workflow steps were skipped after this failure."
            }
          ],
          "failure_details": "Five failures from the Python quality suite: the generic dispatcher test expected FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS instead of MultiHostCompatibilityLimits.MAX_TOTAL_HOSTS; diagnostic and ready-record checks expected escaped textual \\\\t sequences but V 3.1.34 had literal tab code points; native .NET interop assertions expected the removed three-peer function and attemptStatusSummary; the single-host integration test expected the removed multi-host test name and the previous runtimeClient variable. The product runtime remains one host per round.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Generic dispatcher tests explicitly use MultiHostCompatibilityLimits; production remains at three sequential attempts and one host per round.",
            "Cross-runtime source-contract tests are aligned with renamed single-host dispatch and failover tests.",
            "Kotlin source represents tab delimiters with escaped \\\\t literals."
          ],
          "follow_up": "Rerun exact-commit Python quality, graph validation, Android lint, debug/release builds, Android JVM and Keystore instrumentation, and Windows/HostLifecycle checks. Keep UPD-S1-003 in_progress until all required results pass.",
          "device_and_host_details_redacted": "CI runners only. No physical Android device, target Windows host with real model, or installation/upgrade test was performed by this Python quality run.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-003-20261108065950",
          "started_at": "2026-11-08T06:59:50Z",
          "ended_at": "2026-11-08T07:05:09Z",
          "result": "failed",
          "summary": "V 3.1.34 exact-commit product build: Windows WPF clean build, publish, startup/security/identity/trust checks and installer acceptance passed; HostLifecycle unit/integration/coverage passed. Android ktlint, debug APK, release APK and merged-manifest permission validation passed. The Android JVM step failed during test-source compilation (before any JVM tests ran): two references attempted to read roundId from InferenceHostAttempt, whose actual contract only contains ephemeralHostId, attemptId and submissionStatus. Android Keystore instrumentation and final APK artifact were skipped. The round ID remains in the protobuf request/response and is correlated there; the test will assert it is nonblank and that response roundId matches the received request roundId.",
          "changed_files": [
            "src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt"
          ],
          "commit_sha": "b8c664315ba3071080e91190ba79eef7ae58caa7",
          "tests": [
            {
              "name": "Windows WPF build and installer",
              "command": "product-build.yml / Build Windows WPF host",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Clean build, publish, startup, security/identity/trust checks and installer acceptance passed."
            },
            {
              "name": "HostLifecycle unit/integration and coverage",
              "command": "product-build.yml / Host lifecycle unit and integration tests",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "HostLifecycle suite and coverage passed."
            },
            {
              "name": "Android Kotlin lint",
              "command": "gradle --no-daemon ktlintCheck",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Kotlin formatting and lint passed."
            },
            {
              "name": "Android debug APK",
              "command": ":app:assembleDebug",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Debug APK built from clean outputs."
            },
            {
              "name": "Android release APK",
              "command": "Release variant build with disposable CI signing key; key removed",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Release variant built and temporary key cleanup succeeded."
            },
            {
              "name": "Android merged manifest permission checks",
              "command": "Verify minimum permissions in merged Android APK",
              "result": "passed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Minimum permission checks passed."
            },
            {
              "name": "Android JVM unit tests",
              "command": ":app:testDebugUnitTest",
              "result": "failed",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Test-source compilation failed at :app:compileDebugUnitTestKotlin with unresolved InferenceHostAttempt.roundId at transport test lines 429 and 1002; JVM test methods did not run."
            },
            {
              "name": "Android Keystore instrumentation",
              "command": "Connected Android instrumentation test task",
              "result": "skipped",
              "run_id": 42456934059,
              "url": "https://github.com/artinrahimii/project/actions/runs/42456934059",
              "summary": "Skipped after JVM test compilation failed."
            }
          ],
          "failure_details": "Exact Gradle output: :app:compileDebugUnitTestKotlin FAILED. Compiler errors at Libp2pInferenceRequestTransportTest.kt:429:42 and :1002:44: Unresolved reference 'roundId'. `InferenceHostAttempt` is defined in InferenceAttemptDispatcher.kt and contains only `ephemeralHostId`, `attemptId`, and `submissionStatus`; it has no `roundId`. The request received by the responder and final InferenceResponse do carry roundId. Correct test evidence should therefore assert received.roundId is nonblank and response.roundId equals the received/returned correlated roundId, without adding a redundant field to InferenceHostAttempt.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "No production request model change is required. InferenceHostAttempt intentionally contains only peer ID, attempt ID and submission status.",
            "The round identity is part of InferenceRequest and InferenceResponse wire messages and is checked by the transport's PendingAttempt correlation logic.",
            "The two test assertions now validate nonblank round IDs in the wire request/response instead of reading an absent attempt.roundId property."
          ],
          "follow_up": "Rerun exact-commit ktlint, Android debug/release, JVM unit tests, Keystore instrumentation and coverage after correcting the two test references. Keep UPD-S1-003 in_progress until all acceptance criteria pass.",
          "device_and_host_details_redacted": "CI runner only. Windows is a CI runner, not a physical customer host with a loaded model; no real Android handset-to-Windows local inference occurred. No in-place upgrade was run.",
          "model_revision_and_sha256": "Not applicable; no real model inference was run.",
          "pre_upgrade_post_upgrade_snapshot": "Not applicable."
        },
        {
          "attempt_id": "UPD-S1-003-20261108073419",
          "started_at": "2026-11-08T07:34:19.650Z",
          "ended_at": "2026-11-08T07:34:19.650Z",
          "result": "succeeded",
          "summary": "The first-release policy and all four acceptance criteria were verified. One selected host is active in each round; at most three sequential attempts over distinct hosts are allowed and retry is explicit/manual. requestId and absolute deadline are preserved, attemptId/roundId are refreshed, cancellation is terminal, and late/duplicate/mismatched responses are rejected. The decision matrix explicitly resolves the 23 impacted legacy task IDs while generic multi-host compatibility remains separate. Exact V3.1.36 quality, Product Build (including Android JVM and Keystore instrumentation), and Rendezvous CI all passed.",
          "changed_files": [
            "docs/decisions/ADR-0022-single-host-sequential-retry.md",
            "update/Step three/UPD-S1-003/evidence/decision-matrix.md",
            "update/Step three/UPD-S1-003/execution-report.md",
            "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt",
            "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
            "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt",
            "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt",
            "update/Step three/UPD-S1-003/evidence/ci-3.1.36.md"
          ],
          "commit_sha": "d027e72f3968ef7c8c826f889042eb67a8533fa4",
          "tests": [
            {
              "name": "Quality gate",
              "command": ".github/workflows/quality.yml on exact V3.1.36 SHA",
              "result": "passed",
              "run_id": 42457409001,
              "url": "https://github.com/artinrahimii/project/actions/runs/42457409001"
            },
            {
              "name": "Android/Windows Product Build and HostLifecycle",
              "command": ".github/workflows/product-build.yml on exact V3.1.36 SHA",
              "result": "passed",
              "run_id": 42457409040,
              "url": "https://github.com/artinrahimii/project/actions/runs/42457409040"
            },
            {
              "name": "Rendezvous container",
              "command": ".github/workflows/rendezvous-container.yml on exact V3.1.36 SHA",
              "result": "passed",
              "run_id": 42457409058,
              "url": "https://github.com/artinrahimii/project/actions/runs/42457409058"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "First-release policy is one active host per round and three sequential attempts over different hosts; generic multi-host compatibility is preserved separately.",
            "Exact-commit Android request runtime, correlation, timeout/cancel, Windows admission and CI test matrix are green."
          ],
          "follow_up": "Continue UPD-S1-005 and later full product E2E/install acceptance under UPD-S1-020/021.",
          "device_and_host_details_redacted": "CI runners only. This task's contract/decision acceptance does not claim physical Android-to-Windows testing.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "No installation or in-place upgrade was part of this task."
        }
      ]
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
      "status": "in_progress",
      "status_fa": "در حال انجام",
      "expected_output_paths": [
        "docs/decisions/",
        "protocol/whitep/v1/messages.proto",
        "src/Shared.Contracts/",
        "src/Client.Android/app/src/main/java/com/whitep/client/protocol/",
        "update/Step three/UPD-S1-004/execution-report.md"
      ],
      "actual_output_paths": [
        "docs/decisions/ADR-0023-one-shot-inference-v1.md",
        "protocol/whitep/v1/messages.proto",
        "src/Host.Infrastructure/LlamaCppServerBackend.cs",
        "src/Host.Windows/Inference/LlamaCppInferenceEngine.cs",
        "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt",
        "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt",
        "tests/test_proto_contract.py",
        "tests/test_android_libp2p_transport_contract.py",
        "tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs",
        "tests/test_inference_streaming_scope.py",
        "update/Step three/UPD-S1-004/execution-report.md",
        "update/Step three/UPD-S1-004/evidence/decision-review.md"
      ],
      "evidence": [
        "update/Step three/UPD-S1-004/evidence/decision-review.md",
        "tests/test_inference_streaming_scope.py",
        "tests/test_proto_contract.py",
        "tests/test_android_libp2p_transport_contract.py",
        "tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs"
      ],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0032, T0107, T0110, T0111, T0112, T0147, T0161, T0162, T0163, T0164, T0165, T0166, T0167, T0168, T0169, T0170, T0171, T0172, T0173, T0174, T0175, T0176, T0177, T0200, T0202, T0223. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است. تصمیم اجرای نسخهٔ نخست: پاسخ terminal یک‌باره؛ Streaming تا نسخهٔ بعد deferred. فیلد supports_streaming فقط metadata باقی می‌ماند و backend فعلی false گزارش می‌کند؛ قرارداد و کد سالم Protobuf تغییر نکرده‌اند. گزارش و آزمون regression افزوده شده‌اند؛ CI دقیق commit این تلاش هنوز pending است.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "UPD-S1-004-20261108082200",
        "last_attempt_result": "partial",
        "changed_files": [
          "docs/decisions/ADR-0023-one-shot-inference-v1.md",
          "tests/test_inference_streaming_scope.py",
          "update/Step three/UPD-S1-004/execution-report.md",
          "update/Step three/UPD-S1-004/evidence/decision-review.md",
          "update/Step two/project-graph.md"
        ],
        "related_commits": [],
        "tests_passed": [
          "Existing Protobuf contract, Android transport contract and Windows backend capability tests pass in the exact V 3.1.38 CI suite."
        ],
        "tests_failed": [],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [],
          "affected_by_contract_changes": [],
          "review_evidence": [
            "docs/decisions/ADR-0023-one-shot-inference-v1.md",
            "tests/test_inference_streaming_scope.py"
          ]
        }
      },
      "execution_history": [
        {
          "attempt_id": "UPD-S1-004-20261108082200",
          "started_at": "2026-11-08T08:22:00Z",
          "ended_at": "2026-11-08T08:22:00Z",
          "result": "partial",
          "summary": "Audited the shared Protobuf contract, Windows backend and engine, Android transport/runtime/ViewModel, and existing tests. Chose one complete terminal response for the first release and deferred streaming. Preserved the protocol, added ADR-0023 and a focused regression test. Exact-commit CI for the newly added test is pending, so this task remains in_progress.",
          "changed_files": [
            "docs/decisions/ADR-0023-one-shot-inference-v1.md",
            "tests/test_inference_streaming_scope.py",
            "update/Step three/UPD-S1-004/execution-report.md",
            "update/Step three/UPD-S1-004/evidence/decision-review.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Existing exact-base quality gate",
              "command": "quality.yml on V 3.1.38 base",
              "result": "passed",
              "run_id": 42459356154,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459356154",
              "summary": "Existing test suite and both graph validators passed before adding the new regression test."
            },
            {
              "name": "New streaming scope regression test",
              "command": "python -m unittest discover -s tests -p test_inference_streaming_scope.py -v",
              "result": "pending",
              "note": "Must pass on the exact V 3.1.39 commit."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Run the new regression test and full quality gate on V 3.1.39; only then mark UPD-S1-004 done.",
          "device_and_host_details_redacted": "Source inspection and GitHub-hosted CI evidence only. No physical Android handset or target Windows host was accessed.",
          "model_revision_and_sha256": "No live model inference required for this scope decision; real local inference on the parent commit is documented in UPD-S1-005 and is not Android-to-Windows product acceptance.",
          "pre_upgrade_post_upgrade_snapshot": "Not performed; no product installation or in-place upgrade."
        }
      ]
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
      "status": "done",
      "status_fa": "انجام‌شده",
      "expected_output_paths": [
        "src/Host.Domain/",
        "src/Host.Application/",
        "src/Host.Windows/",
        "tests/HostLifecycle.Tests/",
        "update/Step three/UPD-S1-005/execution-report.md"
      ],
      "actual_output_paths": [
        "src/Host.Infrastructure/LocalHostSettingsStore.cs",
        "src/Host.Windows/MainWindow.xaml.cs",
        "src/Host.Windows/MainWindow.xaml",
        "src/Host.Windows/WindowsHostHardwareDiscovery.cs",
        "src/Host.Application/HostReadinessEvaluator.cs",
        "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs",
        "src/Host.Windows/Requests/HostRequestCoordinator.cs",
        "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs",
        "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
        "tests/test_host_resource_settings.py",
        "tests/test_decision_traceability.py",
        "tests/test_inference_backend_abstraction.py",
        "tests/HostLifecycle.Tests/Program.cs",
        "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md",
        "update/Step three/UPD-S1-005/execution-report.md",
        "update/Step three/UPD-S1-005/evidence/resource-policy-review.md"
      ],
      "evidence": [
        "update/Step three/UPD-S1-005/evidence/resource-policy-review.md",
        "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
        "tests/test_host_resource_settings.py",
        "tests/test_decision_traceability.py",
        "https://github.com/artinrahimii/project/actions/runs/42459356154",
        "https://github.com/artinrahimii/project/actions/runs/42459356153",
        "https://github.com/artinrahimii/project/actions/runs/42459356156",
        "https://github.com/artinrahimii/project/actions/runs/42459356182"
      ],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0049, T0050, T0051, T0052, T0053, T0054, T0055, T0056, T0098, T0099, T0100, T0101, T0102, T0103, T0104, T0105, T0106, T0107, T0196. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است. حدهای RAM و فضای دیسکِ قابل اندازه‌گیری به تنظیمات پایدار منتقل شدند. CPU/GPU/VRAM/idle/AC به‌علت نبود سنجش و enforce معتبر، محدودیت قابل‌تنظیم معرفی نشده‌اند. CI کد جدید pending است. CI دقیق V 3.1.37: کیفیت 467/468 تست Python موفق و یک assertion قدیمی شکست؛ HostLifecycle و real-inference نیز پیش از اجرای واقعی inference به‌خاطر انتظار schemaVersion=1 متوقف شدند. Rendezvous موفق بود. دو assertion در commit بعدی اصلاح می‌شوند. تأیید نهایی: CI دقیق V 3.1.38 با SHA 2cc12c32cfc2663316c93064543e6ce62caaa5cb روی هر چهار workflow موفق شد؛ دو assertion قدیمی V 3.1.37 در V 3.1.38 اصلاح شده و regressionها اکنون سبز هستند. هیچ آزمون فیزیکی Android-to-Windows یا ارتقای درجا در این وظیفه ادعا نمی‌شود.",
      "execution_summary": {
        "attempt_count": 3,
        "failed_attempt_count": 0,
        "last_attempt_id": "UPD-S1-005-20261108073426",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "src/Host.Infrastructure/LocalHostSettingsStore.cs",
          "src/Host.Windows/MainWindow.xaml.cs",
          "src/Host.Windows/MainWindow.xaml",
          "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
          "tests/test_host_resource_settings.py",
          "tests/test_decision_traceability.py",
          "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md",
          "update/Step three/UPD-S1-005/execution-report.md",
          "update/Step three/UPD-S1-005/evidence/resource-policy-review.md",
          "tests/test_inference_backend_abstraction.py",
          "tests/HostLifecycle.Tests/Program.cs",
          "update/Step two/project-graph.md",
          "src/Host.Windows/WindowsHostHardwareDiscovery.cs",
          "src/Host.Application/HostReadinessEvaluator.cs",
          "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs",
          "src/Host.Windows/Requests/HostRequestCoordinator.cs",
          "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs"
        ],
        "related_commits": [
          "b2992d2764ca2aac7a61c56bb87d6050a52a1a9c",
          "2cc12c32cfc2663316c93064543e6ce62caaa5cb"
        ],
        "tests_passed": [
          "Exact V 3.1.38 quality gate: Python tests, compileall and Update/legacy graph guards all passed (run 42459356154).",
          "Exact V 3.1.38 product build: Windows WPF, installer acceptance, Android lint/debug/release/Keystore instrumentation and HostLifecycle suite passed (run 42459356153).",
          "Exact V 3.1.38 real inference: pinned llama.cpp and GGUF loaded; generation and cancellation passed (run 42459356156).",
          "Exact V 3.1.38 rendezvous build and smoke passed (run 42459356182).",
          "Host capacity concurrency test admitted 1 of 64 requests; 63 were Busy and maximum concurrent inference was 1."
        ],
        "tests_failed": [
          "V 3.1.37 quality run 42459097705: one stale Save-constructor source-contract assertion; 467 other Python tests passed.",
          "V 3.1.37 HostLifecycle job in product run 42459097749: stale schemaVersion==1 assertion at Program.cs:1132.",
          "V 3.1.37 real-inference run 42459097704: pinned artifacts verified but HostLifecycle stopped before inference due same stale schema-version assertion."
        ],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "not_required",
          "triggering_contracts": [
            "settings schema v2",
            "WPF save-with preservation",
            "live RAM/disk readiness",
            "single-inference admission"
          ],
          "affected_by_contract_changes": [],
          "review_evidence": [
            "update/Step three/UPD-S1-005/evidence/resource-policy-review.md",
            "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
            "tests/test_host_resource_settings.py",
            "https://github.com/artinrahimii/project/actions/runs/42459356154",
            "https://github.com/artinrahimii/project/actions/runs/42459356153",
            "https://github.com/artinrahimii/project/actions/runs/42459356156"
          ]
        }
      },
      "execution_history": [
        {
          "attempt_id": "UPD-S1-005-20261108073039",
          "started_at": "2026-11-08T07:30:39.022Z",
          "ended_at": "2026-11-08T07:30:39.022Z",
          "result": "partial",
          "summary": "Persisted owner-configured live-measured RAM and system-drive free-space limits in settings schema v2, added schema-v1 migration, restored values in WPF, and wired save/revalidation before connection, local inference and peer hosting. Added C# migration/round-trip/bounds tests and Python wiring contracts. Exact-commit CI pending.",
          "changed_files": [
            "src/Host.Infrastructure/LocalHostSettingsStore.cs",
            "src/Host.Windows/MainWindow.xaml.cs",
            "src/Host.Windows/MainWindow.xaml",
            "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
            "tests/test_host_resource_settings.py",
            "tests/test_decision_traceability.py",
            "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md",
            "update/Step three/UPD-S1-005/execution-report.md",
            "update/Step three/UPD-S1-005/evidence/resource-policy-review.md"
          ],
          "commit_sha": "b2992d2764ca2aac7a61c56bb87d6050a52a1a9c",
          "tests": [
            {
              "name": "V 3.1.36 Windows build and installer baseline",
              "command": "product-build.yml",
              "result": "passed",
              "run_id": 42457409040,
              "url": "https://github.com/artinrahimii/project/actions/runs/42457409040",
              "summary": "Baseline before this change only."
            },
            {
              "name": "V 3.1.36 HostLifecycle baseline",
              "command": "product-build.yml",
              "result": "passed",
              "run_id": 42457409040,
              "url": "https://github.com/artinrahimii/project/actions/runs/42457409040",
              "summary": "Baseline before this change only."
            },
            {
              "name": "Schema-v1 migration, schema-v2 round-trip and bounds",
              "command": "LocalHostSettingsPersistenceTests.Run",
              "result": "pending",
              "note": "Must pass on exact commit."
            },
            {
              "name": "Python resource settings and traceability contracts",
              "command": "python -m unittest discover -s tests -v",
              "result": "pending",
              "note": "Must pass on exact commit."
            },
            {
              "name": "Quality gate on V 3.1.37",
              "command": ".github/workflows/quality.yml",
              "result": "failed",
              "run_id": 42459097705,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459097705",
              "summary": "One stale Python WPF test expected Save(new LocalHostSettings(...)); current code preserves the expanded record via Save(_settings with ...). 467 other tests passed."
            },
            {
              "name": "Host lifecycle acceptance on V 3.1.37",
              "command": "HostLifecycle.Tests clean checkout runner",
              "result": "failed",
              "run_id": 42459097749,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459097749",
              "summary": "All preceding host assertions passed; the harness stopped at Program.cs line 1132 because it still expected schema version 1 rather than current version 2."
            },
            {
              "name": "Real local inference workflow on V 3.1.37",
              "command": ".github/workflows/real-inference.yml",
              "result": "failed",
              "run_id": 42459097704,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459097704",
              "summary": "Pinned llama.cpp b11443 archive SHA-256 verified and pinned SmolLM2 GGUF revision downloaded (105,454,144 bytes; SHA-256 8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684). The HostLifecycle harness then stopped at the same schema-version-1 assertion before launching llama-server; no inference response was produced."
            },
            {
              "name": "Rendezvous container on V 3.1.37",
              "command": ".github/workflows/rendezvous-container.yml",
              "result": "passed",
              "run_id": 42459097708,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459097708",
              "summary": "Container build and service smoke passed."
            }
          ],
          "failure_details": "V 3.1.37 exposed two stale assertions after settings schema moved from v1 to v2: tests/test_inference_backend_abstraction.py expected the prior Save(new LocalHostSettings(...)) constructor, and tests/HostLifecycle.Tests/Program.cs expected schemaVersion 1. The real-inference workflow had already verified the pinned llama.cpp CPU archive and GGUF model (model SHA-256 8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684) but the harness failed before starting inference.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "LocalHostSettings schema v2 adds owner-configured RAM and minimum free system-drive thresholds; schema v1 loads with 4/1 GiB defaults and upgrades on next save.",
            "WPF restores/persists both thresholds and revalidates before local inference and peer hosting."
          ],
          "follow_up": "V 3.1.38 updates both stale contract assertions, checks persistence of the schema-v2 default RAM/disk values, and reruns Quality, Product Build, Real local inference and Rendezvous.",
          "device_and_host_details_redacted": "No physical Windows host or Android handset was accessed.",
          "model_revision_and_sha256": "Not applicable.",
          "pre_upgrade_post_upgrade_snapshot": "No product in-place upgrade; settings migration test awaits CI."
        },
        {
          "attempt_id": "UPD-S1-005-20261108073419",
          "started_at": "2026-11-08T07:34:19.650Z",
          "ended_at": "2026-11-08T07:34:19.650Z",
          "result": "partial",
          "summary": "Aligned the Python WPF connect test with settings-record preservation through Save(_settings with ...), and aligned HostLifecycle settings JSON assertions with the current schema version while checking persisted default RAM/disk thresholds. Exact CI for these corrections is pending.",
          "changed_files": [
            "tests/test_inference_backend_abstraction.py",
            "tests/HostLifecycle.Tests/Program.cs",
            "update/Step two/project-graph.md",
            "update/Step three/UPD-S1-005/execution-report.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "name": "Quality gate corrections",
              "command": ".github/workflows/quality.yml",
              "result": "pending",
              "note": "Must pass on exact V3.1.38 commit."
            },
            {
              "name": "HostLifecycle suite including settings schema",
              "command": "dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release",
              "result": "pending",
              "note": "Must pass on exact V3.1.38 commit."
            },
            {
              "name": "Real llama.cpp process and GGUF response",
              "command": ".github/workflows/real-inference.yml",
              "result": "pending",
              "note": "Must reach a real generated response and verify model/server hashes; hosted Windows runner is not the separate customer Android/Windows product E2E."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "HostLifecycle and Python tests now describe schema-v2 persistence and the validated WPF Save-with expression without weakening loopback validation."
          ],
          "follow_up": "Inspect exact V3.1.38 jobs and logs; do not mark UPD-S1-005 done until all four acceptance criteria and required tests are verified.",
          "device_and_host_details_redacted": "Hosted GitHub Windows runner only; no physical Android handset or external Windows host was connected.",
          "model_revision_and_sha256": "Pending exact V3.1.38 real-inference run.",
          "pre_upgrade_post_upgrade_snapshot": "Product upgrade not tested; only local settings schema migration is covered."
        },
        {
          "attempt_id": "UPD-S1-005-20261108073426",
          "started_at": "2026-11-08T07:34:26Z",
          "ended_at": "2026-11-08T07:44:32Z",
          "result": "succeeded",
          "summary": "The exact V 3.1.38 commit passed all four workflow gates. The settings schema-v2 persistence/migration and measured-capacity checks passed; participation remained opt-in and fail-closed; the host admitted exactly one of 64 concurrent requests and returned Busy for 63. Windows WPF/installer, Android lint/debug/release/Keystore instrumentation, HostLifecycle, Python quality/graph guards, rendezvous, and a real pinned llama.cpp + GGUF inference/cancellation run succeeded. Physical Android-to-Windows and in-place upgrade remain separate and untested.",
          "changed_files": [
            "src/Host.Infrastructure/LocalHostSettingsStore.cs",
            "src/Host.Windows/MainWindow.xaml.cs",
            "src/Host.Windows/MainWindow.xaml",
            "src/Host.Windows/WindowsHostHardwareDiscovery.cs",
            "src/Host.Application/HostReadinessEvaluator.cs",
            "src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs",
            "src/Host.Windows/Requests/HostRequestCoordinator.cs",
            "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs",
            "tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs",
            "tests/test_host_resource_settings.py",
            "tests/test_decision_traceability.py",
            "tests/test_inference_backend_abstraction.py",
            "tests/HostLifecycle.Tests/Program.cs",
            "docs/decisions/ADR-0012-local-settings-validation-and-persistence.md",
            "update/Step three/UPD-S1-005/execution-report.md",
            "update/Step three/UPD-S1-005/evidence/resource-policy-review.md",
            "update/Step two/project-graph.md"
          ],
          "commit_sha": "2cc12c32cfc2663316c93064543e6ce62caaa5cb",
          "tests": [
            {
              "name": "Step Three quality gate",
              "command": "quality.yml on exact V 3.1.38 commit",
              "result": "passed",
              "run_id": 42459356154,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459356154",
              "summary": "All Python tests, compileall and both graph_guard structure/output checks passed."
            },
            {
              "name": "Windows, Android and HostLifecycle product build",
              "command": "product-build.yml on exact V 3.1.38 commit",
              "result": "passed",
              "run_id": 42459356153,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459356153",
              "summary": "Clean WPF build/startup/installer acceptance, Android lint/debug/release and Keystore instrumentation, HostLifecycle tests and coverage all passed."
            },
            {
              "name": "Real local inference",
              "command": "real-inference.yml on exact V 3.1.38 commit",
              "result": "passed",
              "run_id": 42459356156,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459356156",
              "summary": "Pinned llama.cpp CPU build 11443 and pinned SmolLM2 GGUF checksum were verified; real generation succeeded, active cancellation was observed and owned process exited cleanly."
            },
            {
              "name": "Rendezvous service",
              "command": "rendezvous-container.yml on exact V 3.1.38 commit",
              "result": "passed",
              "run_id": 42459356182,
              "url": "https://github.com/artinrahimii/project/actions/runs/42459356182",
              "summary": "Container build and smoke test passed."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Persisted RAM and system-drive free-space thresholds in settings schema v2, with schema-v1 migration and fail-closed validation.",
            "WPF restores/saves the thresholds and revalidates before inference and peer hosting; readiness only relies on live-measured RAM/disk.",
            "Network participation remains owner opt-in; request admission remains exactly one active inference without queuing."
          ],
          "follow_up": "Continue UPD-S1-004 and subsequent tasks. Physical Android-to-Windows, product clean install and in-place upgrade remain separate acceptance gates.",
          "device_and_host_details_redacted": "GitHub-hosted Windows runner performed WPF/HostLifecycle/llama.cpp checks; Linux/KVM Android runner performed build, JVM and Keystore instrumentation. No physical user Android handset or external target Windows host was connected.",
          "model_revision_and_sha256": "llama.cpp b11443 archive SHA-256 98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6; executable SHA-256 4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968; QuantFactory/SmolLM2-135M-Instruct-GGUF revision a6b7826ed969c3926a98359e39912af137b464cb, size 105454144 bytes, SHA-256 8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684.",
          "pre_upgrade_post_upgrade_snapshot": "Not performed in this task; product clean install and in-place upgrade are separate acceptance requirements."
        }
      ]
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
      "description": "تغییرهای تصویب‌شده را برای تک‌تک شناسه‌های Txxxx متأثر به وظایف و وابستگی‌های Step Two و Step Three نگاشت کن، نشان بازبینی را در هر گرهٔ واقعی Step Three ثبت کن و سپس رفتار و شواهد هر وظیفه را جداگانه بازبینی کن. ثبت affected_task_ids به‌تنهایی همگام‌سازی یا تأیید سازگاری نیست.",
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
        "ماتریس تصمیم به وظیفهٔ متأثر با رکورد مستقل برای هر Txxxx",
        "نگاشت ماشینی update/Step two/legacy-task-reconciliation.json",
        "نشان update_impact با شناسه‌های UPD-S1 متناظر در هر گرهٔ متأثر Step Three",
        "فهرست وابستگی‌های تغییرکرده و ترتیب اجرای نیازمند بازنگری همراه دلیل و شاهد",
        "گزارش تصمیم و شواهد بازبینی برای تک‌تک وظایف متأثر"
      ],
      "implementation_focus": "تمام 168 شناسهٔ متأثر باید در Step Three پیدا شوند، با update_impact به UPD-S1های مرتبط پیوند بخورند و به حالت pending re-review بروند. سپس هر رفتار و آزمون واقعی بازبینی شود؛ وضعیت‌های فعلی legacy فقط پس از بررسی شواهد تغییر کنند. نقشهٔ شناسه‌ها به‌تنهایی تکمیل UPD-S1-008 نیست.",
      "start_condition": "UPD-S1-006 و UPD-S1-007 باید done و دارای شواهد باشند. گراف معتبر Step Three و فایل V1.0 باید خوانده شوند؛ همهٔ شناسه‌های affected_task_ids باید در nodes گراف پیدا شوند. شناسهٔ مفقود یا گراف غیرقابل‌پردازش، reconciliation را مسدود می‌کند.",
      "acceptance": "1. هر شناسهٔ یکتای Txxxx در فهرست اثر گراف Update دقیقاً یک رکورد reconciliation داشته باشد؛ هیچ شناسه‌ای حذف یا با حدس ساخته نشود.\n2. تمام 168 شناسهٔ متأثر در گراف Step Three موجود و هر کدام دارای update_impact با تمام شناسه‌های UPD-S1 مرتبط باشند.\n3. برای هر شناسه، baseline status حفظ شده و review_status تا بررسی واقعی pending باقی بماند؛ هیچ وضعیت قدیمی صرفاً از روی traceability تغییر نکند.\n4. برای هر وظیفه، رفتار/قرارداد متأثر، کد و آزمون فعلی، تصمیم re_review یا unchanged و شواهد و کامیت بازبینی ثبت شود.\n5. وابستگی‌های مستقیم و مصرف‌کنندگان متأثر تغییر قراردادها بررسی و تغییرهای لازم با دلیل ثبت شوند.\n6. UPD-S1-008 فقط وقتی done شود که همهٔ 168 رکورد بازبینی شده، تصمیم و شواهد دارند و هیچ رکورد اجباری pending نیست.",
      "acceptance_criteria": [
        "هر شناسهٔ یکتای Txxxx در فهرست اثر گراف Update دقیقاً یک رکورد reconciliation داشته باشد؛ هیچ شناسه‌ای حذف یا با حدس ساخته نشود.",
        "تمام 168 شناسهٔ متأثر در گراف Step Three موجود و هر کدام دارای update_impact با تمام شناسه‌های UPD-S1 مرتبط باشند.",
        "برای هر شناسه، baseline status حفظ شده و review_status تا بررسی واقعی pending باقی بماند؛ هیچ وضعیت قدیمی صرفاً از روی traceability تغییر نکند.",
        "برای هر وظیفه، رفتار/قرارداد متأثر، کد و آزمون فعلی، تصمیم re_review یا unchanged و شواهد و کامیت بازبینی ثبت شود.",
        "وابستگی‌های مستقیم و مصرف‌کنندگان متأثر تغییر قراردادها بررسی و تغییرهای لازم با دلیل ثبت شوند.",
        "UPD-S1-008 فقط وقتی done شود که همهٔ 168 رکورد بازبینی شده، تصمیم و شواهد دارند و هیچ رکورد اجباری pending نیست."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "update/Step two/decision-impact-map.md",
        "update/Step two/legacy-task-reconciliation.json",
        "update/Step two/legacy-task-reconciliation.md",
        "step three/project-graph.md",
        "update/Step three/UPD-S1-008/execution-report.md"
      ],
      "actual_output_paths": [],
      "evidence": [],
      "notes": "این گره از update/Step one/Output/V1.10.json کپی معنایی شده است. change_operation=modify; affected_task_ids=T0012, T0020, T0199, T0214. مسیرهای expected فقط مقصد هستند و وجود فایل را ثابت نمی‌کنند. هیچ خروجی اجرایی در زمان ساخت گراف ادعا نشده است. در V 3.1.14 همهٔ شناسه‌های متأثر در گراف واقعی Step Three پیدا و علامت pending re-review گرفتند؛ وضعیت‌های قدیمی فعلاً حفظ شده‌اند و بازبینی شواهد هنوز انجام نشده است.",
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
      "execution_history": [],
      "legacy_task_reconciliation": {
        "required": true,
        "affected_legacy_task_ids": [
          "T0012",
          "T0013",
          "T0015",
          "T0016",
          "T0019",
          "T0020",
          "T0027",
          "T0032",
          "T0033",
          "T0034",
          "T0035",
          "T0036",
          "T0041",
          "T0042",
          "T0043",
          "T0044",
          "T0045",
          "T0046",
          "T0047",
          "T0048",
          "T0049",
          "T0050",
          "T0051",
          "T0052",
          "T0053",
          "T0054",
          "T0055",
          "T0056",
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
          "T0088",
          "T0090",
          "T0092",
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
          "T0108",
          "T0109",
          "T0110",
          "T0111",
          "T0112",
          "T0113",
          "T0114",
          "T0115",
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
          "T0128",
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
          "T0139",
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
          "T0189",
          "T0190",
          "T0191",
          "T0192",
          "T0193",
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
          "T0219",
          "T0221",
          "T0222",
          "T0223",
          "T0224",
          "T0225",
          "T0226",
          "T0227",
          "T0228",
          "T0229"
        ],
        "require_one_record_per_id": true,
        "require_marker_in_authoritative_step_three_graph": true,
        "allowed_decisions": [
          "re_review",
          "unchanged",
          "blocked",
          "new_task_required"
        ],
        "forbid_automatic_status_transition_from_traceability_only": true,
        "completion_requires_evidence_for_each_record": true,
        "completion_requires_zero_pending_reviews": true
      }
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
      "description": "آزمون پذیرش واقعی محصول را از رابط کاربری عادی Android تا برنامهٔ نصب‌شدهٔ Windows، مسیر مستقیم P2P مصوب، backend و مدل محلی واقعی اجرا کن؛ پاسخ تولیدشده توسط همان مدل باید به همان گفت‌وگوی Room برگردد و ذخیره شود. آزمون باید روی دستگاه فیزیکی Android و میزبان واقعی Windows باشد، نه همتای تست یا پاسخ از پیش تعیین‌شده.",
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
        "گزارش E2E با SHA کد، نسخهٔ APK/Windows، مدل و SHA-256، دستگاه/OS، شبکه، فرمان‌ها و نتیجه",
        "شواهد correlation درخواست/پاسخ و تولید واقعی خروجی مدل بدون ثبت متن حساس",
        "نتایج قطع اتصال، میزبان مشغول، لغو، timeout، revoke و retry",
        "آزمون instrumentation آغازشونده از چت عادی Android که ذخیرهٔ پاسخ در همان Room conversation را تأیید کند",
        "آزمون Windows که برنامهٔ واقعی و backend مدل را اجرا کند، نه AndroidInteropPeerServer fixture",
        "گزارش CI/device acceptance مستقل از تست LAN fixture"
      ],
      "implementation_focus": "آزمون باید از UI واقعی اپ Android شروع شود، به Windows host product واقعی برسد، مدل GGUF واقعاً load شده را از backend اصلی اجرا کند و پاسخ همان inference را در همان گفتگو ذخیره کند. AndroidWindowsLanIntegrationTest و AndroidInteropPeerServer فقط برای آزمون تکمیلی transport/authentication مجازند و هرگز به‌تنهایی یا همراه real-inference.yml مستقل، معیار پذیرش E2E نیستند.",
      "start_condition": "UPD-S1-019 باید done و دارای شواهد باشد؛ buildهای release-like هر دو برنامه، دستگاه فیزیکی Android، میزبان واقعی Windows، مدل محلی با مجوز/نسخه/SHA ثبت‌شده و مسیر P2P مصوب آماده باشند. اگر محیط فیزیکی یا مدل واقعی در دسترس نیست، این وظیفه blocked می‌ماند؛ اجرای fixture یا emulator جایگزین پذیرش واقعی نمی‌شود.",
      "acceptance": "1. کاربر از UI گفت‌وگوی عادی Android پیام ارسال کند؛ در مسیر عادی نیاز به واردکردن model ID، IP، port یا PeerId نداشته باشد.\n2. درخواست از runtime واقعی Android و transport مستقیم P2P مصوب به برنامهٔ نصب‌شدهٔ Windows برسد؛ rendezvous فقط metadata ببیند و payload درخواست/پاسخ را دریافت یا relay نکند.\n3. میزبان Windows با رضایت opt-in فعال باشد، backend واقعی را آماده تشخیص دهد و مدل محلی مشخص‌شده با SHA-256 را واقعاً load و اجرا کند.\n4. خروجی از inference همان مدل تولید شود، به درخواست/attempt صحیح correlate شود، در UI نشان داده شود و در همان conversation و Room database باقی بماند.\n5. تست دست‌کم identity mismatch/revoke، unavailable/busy host، قطع اتصال، timeout، cancel و retry را با نتیجهٔ قابل‌انتظار پوشش دهد.\n6. گزارش دستگاه/سیستم‌عامل/نسخهٔ اپ/مدل و checksum/کامیت/نتیجه را ثبت کند، اما متن prompt/response، کلید، توکن یا شناسهٔ حساس را افشا نکند.\n7. آزمون مستقل از AndroidWindowsLanIntegrationTest و آزمون مستقل llama.cpp اجرا شده باشد؛ پاسخ ثابت، mock، fixture، test peer یا emulator مدرک تکمیل نیست.",
      "acceptance_criteria": [
        "کاربر از UI گفت‌وگوی عادی Android پیام ارسال کند؛ در مسیر عادی نیاز به واردکردن model ID، IP، port یا PeerId نداشته باشد.",
        "درخواست از runtime واقعی Android و transport مستقیم P2P مصوب به برنامهٔ نصب‌شدهٔ Windows برسد؛ rendezvous فقط metadata ببیند و payload درخواست/پاسخ را دریافت یا relay نکند.",
        "میزبان Windows با رضایت opt-in فعال باشد، backend واقعی را آماده تشخیص دهد و مدل محلی مشخص‌شده با SHA-256 را واقعاً load و اجرا کند.",
        "خروجی از inference همان مدل تولید شود، به درخواست/attempt صحیح correlate شود، در UI نشان داده شود و در همان conversation و Room database باقی بماند.",
        "تست دست‌کم identity mismatch/revoke، unavailable/busy host، قطع اتصال، timeout، cancel و retry را با نتیجهٔ قابل‌انتظار پوشش دهد.",
        "گزارش دستگاه/سیستم‌عامل/نسخهٔ اپ/مدل و checksum/کامیت/نتیجه را ثبت کند، اما متن prompt/response، کلید، توکن یا شناسهٔ حساس را افشا نکند.",
        "آزمون مستقل از AndroidWindowsLanIntegrationTest و آزمون مستقل llama.cpp اجرا شده باشد؛ پاسخ ثابت، mock، fixture، test peer یا emulator مدرک تکمیل نیست."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        "src/Client.Android/app/src/androidTest/java/com/whitep/client/product/RealAndroidWindowsInferenceAcceptanceTest.kt",
        "tests/HostLifecycle.Tests/RealProductInferenceAcceptanceTests.cs",
        "tests/HostLifecycle.Tests/Invoke-RealAndroidWindowsProductAcceptance.ps1",
        ".github/workflows/android-windows-real-product-acceptance.yml",
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
      "execution_history": [],
      "verification_contract": {
        "must_start_from_android_chat_ui": true,
        "must_use_physical_android_device": true,
        "must_use_installed_windows_product_host": true,
        "must_use_direct_p2p_inference_data_path": true,
        "must_execute_loaded_local_model": true,
        "must_verify_model_revision_and_sha256": true,
        "must_persist_response_in_same_room_conversation": true,
        "must_exercise_failure_and_trust_paths": true,
        "prohibited_as_primary_acceptance_evidence": [
          "dotnet-host-response deterministic response from AndroidInteropPeerServer",
          "AndroidWindowsLanIntegrationTest.kt transport-only LAN fixture",
          "real-inference.yml executed independently from the Android client",
          "emulator-only, mock-only, fake transport, protocol fixture or canned response"
        ],
        "existing_lan_test_role": "Supplemental authenticated-transport verification only; not product E2E."
      }
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
      "description": "اسناد و بسته‌های قابل نصب Android و Windows را آماده کن و علاوه بر نصب پاک، ارتقای درجا از نسخهٔ قبلی نصب‌شده را بدون ازدست‌رفتن داده‌ها و هویت کاربر/میزبان آزمون کن. انتشار را تا عبور از هر دو مسیر clean-install و in-place-upgrade باز نکن.",
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
        "راهنمای نصب، استفاده و ارتقای دو برنامه",
        "راهنمای عیب‌یابی و محدودیت‌های شناخته‌شده",
        "بسته‌های قابل نصب و checksum و منشأ build",
        "گزارش clean install و in-place upgrade برای Android و Windows",
        "snapshot مقایسه‌ای پیش/پس از ارتقا با داده‌های مصنوعی آزمون و بدون افشای secrets",
        "گزارش آمادگی انتشار"
      ],
      "implementation_focus": "بسته‌های installable واقعی با checksum و نصب پاک؛ همچنین نصب نسخهٔ قبلی و ارتقای همان نصب با حفظ داده‌ها. آزمون upgrade نباید با uninstall/clear-data یا پاک‌سازی پوشه‌های داده اجرا شود. از دادهٔ آزمایشی مصنوعی استفاده کن و فایل مدل واقعی یا secret را در گزارش قرار نده.",
      "start_condition": "UPD-S1-020 و UPD-S1-024 باید done و دارای شواهد باشند. برای آزمون upgrade باید بستهٔ قبلی و جدید با هویت امضای سازگار، محیط آزمایشی مجزا و داده‌های seed شده در دسترس باشند. اگر signing identity یا روش upgrade واقعی فراهم نیست، معیار upgrade blocked است و clean install به‌تنهایی برای done کافی نیست.",
      "acceptance": "1. بسته‌های Windows و Android از کد جاری تولید و روی محیط تمیز نصب/اجرا شده باشند.\n2. راهنماها با رابط واقعی و بدون دستورهای منسوخ سازگار باشند.\n3. checksum و مجوزهای وابستگی‌ها و مدل نمونه ثبت شده باشند.\n4. Android از نسخهٔ قبلی با همان package/signing identity و روش نصب upgrade شود؛ Room conversation/message IDs، ترتیب و شمار پیام‌ها، schema/migrations، تنظیمات کاربر، Android Keystore identity و trust state حفظ یا طبق migration مصوب بازاعتبارسنجی شوند.\n5. Windows از نسخهٔ قبلی با installer واقعی upgrade شود؛ تنظیمات، مسیر و SHA-256 مدل‌های محلی، host identity، اسرار محافظت‌شدهٔ سیستم‌عامل، peer trust و وضعیت opt-in با سیاست نسخهٔ جدید حفظ یا به‌صورت مستند مهاجرت داده شوند.\n6. پیش و پس از upgrade snapshot قابل‌مقایسه از شناسه‌ها، شمارها، نسخهٔ schema، مسیر/checksum مدل و fingerprintهای غیرقابل‌بازگشت تهیه شود؛ prompt، secret یا کلید خصوصی در گزارش ثبت نشود.\n7. هیچ uninstall، clear-data، reset identity یا حذف فایل مدل در مسیر upgrade رخ ندهد؛ شکست migration آشکار و قابل‌بازیابی باشد.\n8. انتشار فقط پس از موفقیت clean install و in-place upgrade در هر دو پلتفرم و ثبت محدودیت‌های باقی‌مانده اعلام شود.",
      "acceptance_criteria": [
        "بسته‌های Windows و Android از کد جاری تولید و روی محیط تمیز نصب/اجرا شده باشند.",
        "راهنماها با رابط واقعی و بدون دستورهای منسوخ سازگار باشند.",
        "checksum و مجوزهای وابستگی‌ها و مدل نمونه ثبت شده باشند.",
        "Android از نسخهٔ قبلی با همان package/signing identity و روش نصب upgrade شود؛ Room conversation/message IDs، ترتیب و شمار پیام‌ها، schema/migrations، تنظیمات کاربر، Android Keystore identity و trust state حفظ یا طبق migration مصوب بازاعتبارسنجی شوند.",
        "Windows از نسخهٔ قبلی با installer واقعی upgrade شود؛ تنظیمات، مسیر و SHA-256 مدل‌های محلی، host identity، اسرار محافظت‌شدهٔ سیستم‌عامل، peer trust و وضعیت opt-in با سیاست نسخهٔ جدید حفظ یا به‌صورت مستند مهاجرت داده شوند.",
        "پیش و پس از upgrade snapshot قابل‌مقایسه از شناسه‌ها، شمارها، نسخهٔ schema، مسیر/checksum مدل و fingerprintهای غیرقابل‌بازگشت تهیه شود؛ prompt، secret یا کلید خصوصی در گزارش ثبت نشود.",
        "هیچ uninstall، clear-data، reset identity یا حذف فایل مدل در مسیر upgrade رخ ندهد؛ شکست migration آشکار و قابل‌بازیابی باشد.",
        "انتشار فقط پس از موفقیت clean install و in-place upgrade در هر دو پلتفرم و ثبت محدودیت‌های باقی‌مانده اعلام شود."
      ],
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "expected_output_paths": [
        ".github/workflows/android-release.yml",
        ".github/workflows/release-assets.yml",
        ".github/workflows/product-upgrade-acceptance.yml",
        "tools/build_windows_installer.ps1",
        "tools/Invoke-WhitePInPlaceUpgradeAcceptance.ps1",
        "src/Client.Android/app/src/androidTest/java/com/whitep/client/upgrade/AndroidInPlaceUpgradeAcceptanceTest.kt",
        "tests/HostLifecycle.Tests/WindowsInPlaceUpgradeAcceptanceTests.cs",
        "docs/release/upgrade-acceptance.md",
        "README.md",
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
      "execution_history": [],
      "upgrade_contract": {
        "required": true,
        "test_type": "in_place_upgrade_from_previous_installed_version",
        "platforms": [
          "Android",
          "Windows"
        ],
        "data_domains_must_survive_or_be_migrated": [
          "Android Room conversations and messages, identifiers, order and schema",
          "Android user preferences and non-secret app settings",
          "Android Keystore identity and peer trust state",
          "Windows host settings and explicit participation/consent state",
          "Windows host identity and OS-protected secrets",
          "Windows peer trust and revocation state",
          "local model files, configured paths and SHA-256 integrity"
        ],
        "pre_upgrade_post_upgrade_snapshot_required": true,
        "uninstall_or_clear_data_prohibited": true,
        "signing_or_identity_mismatch_behavior": "Fail closed and report blocked; never silently reset identity or trust.",
        "completion_requires_both_clean_install_and_upgrade": true
      }
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
      "dependency_classification": "sequencing_only",
      "validation_status": "validated",
      "rationale": "UPD-S1-002 نیازمند نتیجهٔ ممیزی خط مبنای UPD-S1-001 است تا تصمیم مسیر را بر اساس وضعیت واقعی کد، آزمون و شکاف‌ها بگیرد. این پیش‌نیاز، قرارداد یا artifact اجرایی/رابطی را میان دو task مبادله نمی‌کند؛ بنابراین ماهیت فنی آن sequencing_only است. ترتیب محافظه‌کارانه و type=blocks حفظ می‌شود. شواهد: گزارش و baseline-audit وظیفهٔ 001.",
      "evidence": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
      ]
    },
    {
      "from": "UPD-S1-001",
      "to": "UPD-S1-003",
      "type": "blocks",
      "dependency_classification": "sequencing_only",
      "validation_status": "validated",
      "rationale": "UPD-S1-003 uses the completed baseline audit to determine which three-host behaviors are implemented and which request lifecycle guarantees already exist. It does not consume a new protocol/interface artifact from UPD-S1-001, so the technical dependency is sequencing_only. The graph's existing blocks edge and conservative execution order are retained.",
      "evidence": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md"
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
      "dependency_classification": "sequencing_only",
      "validation_status": "validated",
      "rationale": "UPD-S1-005 consumes the completed baseline audit to distinguish existing owner-consent, readiness, measurement and admission behavior from real implementation gaps. It consumes no new API, type or executable artifact from UPD-S1-001; the dependency is sequencing_only and the conservative blocks order remains.",
      "evidence": [
        "update/Step three/UPD-S1-001/execution-report.md",
        "update/Step three/UPD-S1-001/evidence/baseline-audit.md",
        "update/Step three/UPD-S1-005/evidence/resource-policy-review.md"
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
      "Update the graph in the same commit as implementation when practical, otherwise immediately after; reconcile status to repository state and test results.",
      "For UPD-S1-008, create one evidence-backed reconciliation record for every unique affected Txxxx ID. Do not automatically change legacy statuses from traceability metadata; an empty or invalid Step Three graph blocks completion.",
      "For UPD-S1-020, LAN interop fixtures and standalone real-inference are supplemental only. Acceptance requires one physical Android-to-installed-Windows-product request through the real local model and persistence in the same Room conversation.",
      "For UPD-S1-021, both clean installation and in-place upgrade must pass on Android and Windows. Compare pre/post state and model checksums; do not uninstall, clear data or reset identities.",
      "Before marking UPD-S1-008 done, reconcile every affected Txxxx ID against Step Three, inspect actual code/tests for the changed behavior and record evidence-backed decisions. The initial pending markers do not mean the review is complete."
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
      "follow_up",
      "device_and_host_details_redacted",
      "model_revision_and_sha256",
      "pre_upgrade_post_upgrade_snapshot"
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
    "completion_test_rule": "Every done node must list at least one passing test or explicit acceptance verification, and evidence must include a test source file or CI/device run appropriate to its scope. UPD-S1-020 requires full physical-device product E2E, not a transport fixture or isolated inference test. UPD-S1-021 requires clean-install and in-place-upgrade evidence on each platform. UPD-S1-008 requires zero pending legacy review records and evidence-backed per-task decisions in both graphs."
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
        "Unit/integration tests, physical-device acceptance and clean package installation have evidence; no source-only or document-only completion.",
        "End-to-end evidence must start in the normal Android chat UI, execute a loaded local model in the installed Windows product host, and persist the response in the same Room conversation.",
        "AndroidWindowsLanIntegrationTest / AndroidInteropPeerServer deterministic fixture and standalone real-inference.yml are supplemental only; neither satisfies product E2E."
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
    },
    {
      "id": "GATE-UPGRADE-01",
      "target": "Installed-version upgrade and data continuity",
      "requirements": [
        "Android upgrades in place from a previous signed installation without losing Room conversations/messages, settings, Keystore identity or peer trust.",
        "Windows upgrades in place without losing host settings, local model paths/files/checksums, host identity, protected secrets or peer trust.",
        "Pre/post snapshots show continuity or an explicitly approved migration; uninstall/clear-data is not used.",
        "Clean-install and in-place-upgrade acceptance both pass on both platforms."
      ],
      "owners": [
        "UPD-S1-021"
      ]
    },
    {
      "id": "GATE-LEGACY-RECONCILIATION-01",
      "target": "Legacy task graph reconciliation",
      "requirements": [
        "Every unique Txxxx ID affected by V1.10 has an individual evidence-backed reconciliation decision.",
        "The authoritative Step Three graph is valid and parseable before legacy status synchronization is claimed.",
        "No legacy status is changed solely because its ID appears in affected_task_ids."
      ],
      "owners": [
        "UPD-S1-008"
      ]
    }
  ],
  "progress_snapshot": {
    "as_of": "2026-11-08T08:22:00Z",
    "total_tasks": 24,
    "not_started": 19,
    "in_progress": 1,
    "done": 4,
    "blocked": 0,
    "verified_completion_percentage": 16.67,
    "calculation": "Only update tasks with real outputs, verified acceptance evidence and successful required tests count as done; 4 / 24 = 16.67%. UPD-S1-004 remains in_progress pending its exact-commit regression CI."
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
  },
  "updated_at": "2026-11-08T08:22:00Z",
  "legacy_task_sync": {
    "state": "review_pending",
    "reason_code": "LEGACY_TASK_REVIEW_NOT_YET_PERFORMED",
    "checkpoint_commit_sha": "142640d24c97b8b65b1f4eb24559b0184e4bb4d0",
    "source_graph_path": "step three/project-graph.md",
    "source_graph_revision": "v2.1",
    "source_graph_node_count": 230,
    "source_graph_state_at_checkpoint": "valid_parseable_graph",
    "affected_legacy_task_ids": [
      "T0012",
      "T0013",
      "T0015",
      "T0016",
      "T0019",
      "T0020",
      "T0027",
      "T0032",
      "T0033",
      "T0034",
      "T0035",
      "T0036",
      "T0041",
      "T0042",
      "T0043",
      "T0044",
      "T0045",
      "T0046",
      "T0047",
      "T0048",
      "T0049",
      "T0050",
      "T0051",
      "T0052",
      "T0053",
      "T0054",
      "T0055",
      "T0056",
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
      "T0088",
      "T0090",
      "T0092",
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
      "T0108",
      "T0109",
      "T0110",
      "T0111",
      "T0112",
      "T0113",
      "T0114",
      "T0115",
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
      "T0128",
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
      "T0139",
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
      "T0189",
      "T0190",
      "T0191",
      "T0192",
      "T0193",
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
      "T0219",
      "T0221",
      "T0222",
      "T0223",
      "T0224",
      "T0225",
      "T0226",
      "T0227",
      "T0228",
      "T0229"
    ],
    "affected_task_id_count": 168,
    "matched_legacy_task_id_count": 168,
    "missing_legacy_task_ids": [],
    "review_status_summary": {
      "pending": 168,
      "reviewed": 0
    },
    "reconciliation_artifact_path": "update/Step two/legacy-task-reconciliation.json",
    "status_change_policy": "All 168 legacy nodes are tagged as re-review pending in the authoritative Step Three graph. Their existing statuses remain unchanged until per-task implementation and acceptance evidence is reviewed.",
    "completion_blocker": "The impact-to-task mapping now exists, but all 168 task-level reviews and evidence records remain pending. UPD-S1-008 cannot be marked done until each review is completed or an evidence-backed unchanged decision is recorded.",
    "rules": [
      "Every unique affected Txxxx ID has exactly one record in the reconciliation artifact and an update_impact marker on its Step Three node.",
      "Every marker lists all corresponding UPD-S1 task IDs and records the legacy status at the start of review.",
      "Do not automatically change a legacy task status based only on affected_task_ids.",
      "Review each impacted behavior and its current implementation/tests; append decision, evidence, and review commit.",
      "UPD-S1-008 remains incomplete while any required review is pending."
    ]
  }
}
```

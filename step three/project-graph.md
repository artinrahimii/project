# v2.1 — گراف وابستگی و دفتر ثبت اجرای پروژه

## هدف
این فایل نسخهٔ v2.1 گراف وابستگی و دفتر ثبت اجرای قابل‌ردیابی برای تمام 229 وظیفهٔ موجود در `step one/V1.0.md` است. منبع اصلی شرح وظایف همان فایل Step One است؛ این فایل شناسه‌ها، ترتیب اجرا، ورودی، خروجی، شرط شروع، معیار پذیرش، وضعیت و محل فایل‌های خروجی را ثبت می‌کند.

## معنی یال‌ها و وضعیت‌ها
- یال `A → B` یعنی وظیفهٔ `A` پیش‌نیاز `B` است و باید پیش از آن تکمیل و تأیید شود.
- `not_started` / «انجام‌نشده»: خروجی واقعی و شواهد پذیرش هنوز تأیید نشده‌اند.
- `in_progress` / «در حال انجام»: کار شروع شده اما معیار پذیرش کامل نشده است.
- `done` / «انجام‌شده»: خروجی واقعی در مخزن وجود دارد و آزمون/شاهد پذیرش موفق است.
- `blocked` / «مسدود»: پیش‌نیاز، دسترسی یا زیرساخت لازم فراهم نیست.
- `expected_output_paths` مسیر هدف است، نه اثبات وجود فایل. `actual_output_paths` فقط پس از ایجاد و بررسی فایل واقعی پر می‌شود.
- فایل مستندات، طرح معماری، کد stub، mock یا پاسخ هاردکدشده به‌تنهایی معیار تکمیل نیست.

## سیاست وابستگی

گراف فعلی از ترتیب بخش‌های Step One برای ایجاد یک مسیر محافظه‌کارانه استفاده می‌کند: درون هر بخش، هر وظیفه به وظیفهٔ پیشین همان بخش وابسته است و نخستین وظیفهٔ هر بخش به آخرین وظیفهٔ بخش قبلی متصل می‌شود. **این زنجیره به‌تنهایی اثبات نمی‌کند که همهٔ یال‌ها وابستگی فنی واقعی هستند.** بعضی یال‌ها ممکن است فقط ترتیب اجرا را ساده کرده باشند و امکان اجرای موازی را پنهان کنند.

پیش از اجرای گسترده، هر یال باید با قراردادها، رابط‌ها، ورودی/خروجی‌ها و شواهد مخزن بررسی و در یکی از دسته‌های زیر طبقه‌بندی شود:

- `hard_dependency`: وظیفهٔ مقصد بدون خروجی معتبر وظیفهٔ مبدأ نمی‌تواند تکمیل شود.
- `contract_or_interface_dependency`: دو وظیفه به قرارداد یا رابط مشترک وابسته‌اند.
- `sequencing_only`: ترتیب فعلی محافظه‌کارانه است، اما وابستگی فنی مستقیم هنوز ثابت نشده است.

یال‌ها را صرفاً برای موازی‌سازی حذف نکن. هر تغییر در یال باید همراه با دلیل، شواهد و ثبت نوع قبلی و جدید باشد. تا زمان تأیید، ترتیب فعلی فقط مبنای امن اولیه است، نه مدل نهایی وابستگی فنی.

## قرارداد داده‌های وابستگی

فیلد `type` موجود در هر یال حفظ می‌شود و به‌تنهایی طبقه‌بندی فنی وابستگی را مشخص نمی‌کند. هر یال همچنین فیلدهای مستقل زیر را دارد:

- `dependency_classification`: یکی از `hard_dependency`، `contract_or_interface_dependency)، `sequencing_only) یا `unreviewed).
- `rationale`: دلیل مستند و مبتنی بر شواهد برای طبقه‌بندی؛ تا زمان بررسی خالی می‌ماند.
- `validation_status`: یکی از `unreviewed`، `validated` یا `disputed`.
- `evidence`: ارجاع به شواهد واقعی که طبقه‌بندی را پشتیبانی می‌کنند.

در این نسخه، طبقه‌بندی تمام یال‌ها عمداً `unreviewed` و وضعیت اعتبارسنجی آن‌ها `unreviewed` است؛ هیچ نوعی را صرفاً از روی `type: blocks` حدس نزن. پیش از تغییر به طبقه‌بندی تأییدشده، ورودی‌ها و خروجی‌های وظایف، قراردادها و شواهد مخزن را بررسی و دلیل و شواهد را ثبت کن. نوع رابطهٔ موجود را بی‌دلیل تغییر نده.

وضعیت گره با نتیجهٔ تلاش تفاوت دارد: وضعیت‌های گره `not_started`، `in_progress`، `done` و `blocked` هستند. شکست یک تلاش در `execution_history.result: failed` ثبت می‌شود و به‌تنهایی وضعیت مستقل گره نیست. پیشرفت ناقص معمولاً `in_progress` باقی می‌ماند؛ `blocked` فقط وقتی استفاده شود که مانع واقعی ادامهٔ امن کار را متوقف کند.

## سیاست ثبت اجرای وظایف

وضعیت فعلی هر وظیفه به‌تنهایی سابقهٔ اجرای آن را نشان نمی‌دهد. بنابراین هر گره دو بخش دارد:

- `execution_summary`: خلاصهٔ جاری شامل تعداد تلاش‌ها، تلاش‌های ناموفق، فایل‌های تغییرکرده، کامیت‌های مرتبط، آزمون‌های موفق/ناموفق، علت مسدودشدن یا بازگشایی و وضعیت بازبینی قراردادها.
- `execution_history`: فهرست افزایشیِ تلاش‌های اجرا؛ تلاش‌های قبلی را پاک یا با تلاش جدید جایگزین نکن. هر تلاش باید نتیجه، زمان، فایل‌های تغییرکرده، کامیت (یا تصریحِ کامیت‌نشدن)، آزمون‌ها، شکست‌ها، مسدودشدن و پیگیری بعدی را ثبت کند.

قواعد اجرایی:

1. پیش از شروع، پیش‌نیازها و شواهد واقعی آن‌ها را بررسی کن؛ فقط به ترتیب ظاهری گراف تکیه نکن.
2. پس از هر تلاش، حتی اگر شکست خورده یا ناقص بوده، سابقهٔ آن را ثبت کن تا کار تکراریِ بی‌دلیل انجام نشود.
3. `done` فقط زمانی مجاز است که خروجی واقعی بررسی شده و معیار پذیرش با آزمون یا شاهد قابل‌بررسی تأیید شده باشد.
4. برای `blocked` علت، پیش‌نیاز یا دسترسی مفقود و شرط رفع مانع را ثبت کن.
5. اگر وظیفهٔ تکمیل‌شده دوباره باز شد، سابقهٔ تکمیل قبلی را حفظ و دلیل بازگشایی را ثبت کن.
6. پس از تغییر قرارداد یا رابط مشترک، مصرف‌کنندگان مستقیم و غیرمستقیم آن را پیدا کن؛ وظایف متأثر را برای بازبینی علامت بزن و آزمون‌های پذیرش مرتبط را دوباره اجرا کن. تا پایان بازبینی، وضعیت قبلی `done` را بدون بررسی معتبر فرض نکن.
7. مسیر مورد انتظار، فایل واقعی محسوب نمی‌شود. `actual_output_paths` فقط پس از بررسی فایل در نسخهٔ مخزن مربوط پر شود.
8. وضعیت گراف را در همان تغییر یا بلافاصله پس از آن به‌روز کن و آن را با فایل‌ها، کامیت و نتایج آزمون تطبیق بده.

هر رکورد تلاش از فیلدهای زیر استفاده می‌کند: `attempt_id`, `started_at`, `ended_at`, `result`, `summary`, `changed_files`, `commit_sha`, `tests`, `failure_details`, `blocked_reason`, `reopened_reason`, `contract_changes`, `follow_up`. نتیجه می‌تواند `succeeded`, `failed`, `partial`, `blocked` یا `reopened` باشد.

این فیلدها در نسخهٔ فعلی برای هر 229 گره مقدار اولیه دارند؛ مقدار صفر و آرایه‌های خالی یعنی هنوز سابقه‌ای در این نسخه ثبت نشده است، نه اینکه آزمونی اجرا شده یا فایل خروجی وجود دارد.
## قرارداد استاندارد فضای کاری هر وظیفه

برای هر یک از 229 وظیفه، پوشه‌ای مستقل با شناسهٔ دقیق گره در این مسیر ایجاد کن:

- پوشهٔ وظیفه: `step three/{TASK_ID}/`؛ برای نمونه `step three/T0001/`.
- گزارش اجرای وظیفه: `step three/{TASK_ID}/execution-report.md`.
- پوشهٔ شواهد و نتایج آزمون: `step three/{TASK_ID}/evidence/`.

در هر تلاش، گزارش باید دست‌کم شناسهٔ وظیفه و تلاش، زمان، نتیجه، خلاصهٔ کار، فایل‌های تغییرکرده، آزمون‌هایی که واقعاً اجرا شده‌اند و نتیجهٔ آن‌ها، شواهد، شکست‌ها یا محدودیت‌ها، موانع و پیگیری بعدی را ثبت کند. شواهد باید به فایل‌ها، گزارش‌ها یا خروجی‌های واقعی و قابل‌بررسی اشاره کنند؛ صرفاً ساختن پوشه یا گزارش، معیار پذیرش نیست. تاریخچهٔ تلاش‌ها را پاک نکن و گزارش را با تاریخچهٔ مرکزی همان گره در این فایل هماهنگ نگه دار.

کد، پیکربندی و آزمون‌های واقعی برنامه را در مسیر مناسب ساختار اصلی مخزن قرار بده، نه اینکه همهٔ کد را در پوشه‌های وظایف پراکنده کنی. مسیر فایل‌های واقعی تغییرکرده باید در گزارش وظیفه و فیلدهای مربوط گراف ثبت شود. از ایجاد نسخه‌های تکراریِ غیرضروری از کد یا شواهد خودداری کن. این قرارداد برای همهٔ وظایف یکسان است، مگر اینکه استثنایی مستدل ثبت شود.

## نمای کلی بخش‌ها
- **قانون تکمیل وظیفه** — T0001 تا T0010؛ 10 وظیفه؛ نقطهٔ شروع گراف
- **1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی** — T0011 تا T0020؛ 10 وظیفه; وابسته به T0010
- **2. ساختار مخزن و ابزارهای توسعه** — T0021 تا T0030؛ 10 وظیفه; وابسته به T0020
- **3. قراردادهای مشترک شبکه و داده** — T0031 تا T0040؛ 10 وظیفه; وابسته به T0030
- **4. اسکلت قابل‌اجرای میزبان ویندوز** — T0041 تا T0048؛ 8 وظیفه; وابسته به T0040
- **5. تشخیص سخت‌افزار و ظرفیت** — T0049 تا T0056؛ 8 وظیفه; وابسته به T0048
- **6. موتور استنتاج محلی واقعی** — T0057 تا T0067؛ 11 وظیفه; وابسته به T0056
- **7. کاتالوگ، مجوز و فایل‌های مدل** — T0068 تا T0075؛ 8 وظیفه; وابسته به T0067
- **8. دانلود، نصب، انتخاب و حذف مدل** — T0076 تا T0087؛ 12 وظیفه; وابسته به T0075
- **9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل** — T0088 تا T0097؛ 10 وظیفه; وابسته به T0087
- **10. صف درخواست و محدودیت یک پردازش هم‌زمان** — T0098 تا T0107؛ 10 وظیفه; وابسته به T0097
- **11. سرویس شبکه روی میزبان ویندوز** — T0108 تا T0115؛ 8 وظیفه; وابسته به T0107
- **12. معماری P2P و کشف میزبان** — T0116 تا T0128؛ 13 وظیفه; وابسته به T0115
- **13. هویت ارتباطی و رمزنگاری** — T0129 تا T0138؛ 10 وظیفه; وابسته به T0128
- **14. اپلیکیشن واقعی اندروید** — T0139 تا T0150؛ 12 وظیفه; وابسته به T0138
- **15. مخزن زمینهٔ فشردهٔ گفت‌وگو** — T0151 تا T0160؛ 10 وظیفه; وابسته به T0150
- **16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد** — T0161 تا T0177؛ 17 وظیفه; وابسته به T0160
- **17. امنیت داده و سیاست نگهداری** — T0178 تا T0188؛ 11 وظیفه; وابسته به T0177
- **18. خطا، مشاهده‌پذیری و پایداری** — T0189 تا T0195؛ 7 وظیفه; وابسته به T0188
- **19. آزمون‌های واقعی و معیارهای قبولی** — T0196 تا T0208؛ 13 وظیفه; وابسته به T0195
- **20. بسته‌بندی و تحویل قابل‌اجرا** — T0209 تا T0218؛ 10 وظیفه; وابسته به T0208
- **21. بازبینی نهایی ضدِ پیاده‌سازی صوری** — T0219 تا T0229؛ 11 وظیفه; وابسته به T0218

## قالب هر گره در نسخهٔ v2.1
هر گره دارای این فیلدهاست:
- `id`: شناسهٔ پایدار و یکتا؛
- `title` و `section`: شرح و بخش مبدأ؛
- `depends_on`: شناسهٔ پیش‌نیازها؛
- `inputs`: ورودی‌های لازم؛
- `outputs`: خروجی مورد انتظار؛
- `start_condition`: شرط آغاز؛
- `acceptance`: معیار تکمیل قابل‌بررسی؛
- `status` و `status_fa`: وضعیت جاری؛
- `expected_output_paths`: مسیر هدف؛
- `actual_output_paths`: مسیر واقعی فایل‌های ایجادشده، خالی تا زمان تأیید؛
- `evidence`: مسیر گزارش آزمون، CI یا شواهد دیگر.

## دادهٔ کامل گراف v2.1
ساختار زیر JSON معتبر و قابل‌پردازش ماشینی است. فهرست `nodes` همهٔ وظایف را دارد و `edges` یال‌های جهت‌دار وابستگی را ارائه می‌کند.

```json
{
  "schema_version": "1.0",
  "document": "v2.1",
  "title": "گراف وابستگی اجرای پروژهٔ شبکهٔ همتابه‌همتای هوش مصنوعی محلی",
  "source": {
    "path": "step one/V1.0.md",
    "revision": "V 1.2.0",
    "task_count": 229
  },
  "commit_intent": "V 2.1.0",
  "status_legend": {
    "not_started": "انجام‌نشده",
    "in_progress": "در حال انجام",
    "done": "انجام‌شده؛ فقط پس از وجود خروجی و شواهد پذیرش",
    "blocked": "مسدود؛ پیش‌نیاز یا دسترسی موجود نیست"
  },
  "graph_policy": {
    "direction": "A -> B یعنی A پیش‌نیاز B است",
    "dependency_rule": "The current chain is a conservative ordering baseline, not proof that every edge is a true technical dependency. Preserve the existing type field as the relationship/blocking meaning. Every edge separately records dependency_classification (hard_dependency, contract_or_interface_dependency, sequencing_only, or unreviewed), rationale, validation_status, and evidence. Initially all edges are unreviewed; do not infer a dependency class from type=blocks alone. Validate each edge against real task inputs/outputs, contracts, code, and evidence. Record a grounded rationale and evidence before changing an edge to a validated classification. Change or remove edges only with a recorded reason and preserve a safe execution order.",
    "completion_rule": "تولید سند یا فایل به‌تنهایی کافی نیست؛ باید کد واقعی، آزمون موفق و شاهد پذیرش وجود داشته باشد.",
    "status_truth": "وضعیت‌ها هنگام ایجاد گراف از روی وجود کدهای اجرایی و شواهد در مخزن ارزیابی شده‌اند؛ چون مخزن در این مرحله عمدتاً فایل برنامه‌ریزی دارد، همهٔ وظایف پیاده‌سازی در وضعیت انجام‌نشده قرار می‌گیرند.",
    "actual_paths_rule": "actual_output_paths تا زمان تأیید خروجی واقعی خالی می‌ماند. expected_output_paths مسیر هدف است، نه ادعای وجود فایل.",
    "execution_tracking": "Every task maintains an append-only execution_history and a current summary of attempts, failed attempts, changed files, related commits, test results, blocking/reopening reasons, and contract-triggered revalidation. A task is done only with real output and verifiable acceptance evidence.",
    "edge_classification_values": [
      "hard_dependency",
      "contract_or_interface_dependency",
      "sequencing_only",
      "unreviewed"
    ],
    "edge_validation_status_values": [
      "unreviewed",
      "validated",
      "disputed"
    ],
    "node_status_rule": "Node status values are not_started, in_progress, done, and blocked. A failed attempt is recorded in execution_history.result=failed and execution_summary; it is not a separate node status. Use partial progress as in_progress, and blocked only when an actual blocker prevents safe progress."
  },
  "task_workspace_policy": {
    "applies_to": "all 229 task nodes unless a documented, justified exception applies",
    "task_folder_template": "step three/{TASK_ID}/",
    "execution_report_template": "step three/{TASK_ID}/execution-report.md",
    "evidence_directory_template": "step three/{TASK_ID}/evidence/",
    "report_requirements": [
      "task_id and attempt_id",
      "start/end time and result",
      "summary of work performed",
      "actual changed files and related commits when applicable",
      "tests actually run and their observed results",
      "evidence references",
      "failures, limitations, blockers, reopening reasons, and follow-up"
    ],
    "source_code_rule": "Keep real application source, configuration, and tests in the coherent project structure appropriate to the architecture; do not scatter all source code into per-task folders. Link actual changed paths from the task report and graph.",
    "evidence_rule": "Folders, reports, expected paths, stubs, mocks, or claims alone do not prove acceptance. Evidence must refer to real, inspectable outputs or test results. Keep attempt history append-only and synchronize task reports with each node's execution_history and execution_summary.",
    "duplication_rule": "Avoid unnecessary duplicate copies of source code or evidence."
  },
  "sections": [
    {
      "name": "قانون تکمیل وظیفه",
      "first_task": "T0001",
      "last_task": "T0010",
      "depends_on_section": null,
      "task_count": 10
    },
    {
      "name": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "first_task": "T0011",
      "last_task": "T0020",
      "depends_on_section": "قانون تکمیل وظیفه",
      "task_count": 10
    },
    {
      "name": "2. ساختار مخزن و ابزارهای توسعه",
      "first_task": "T0021",
      "last_task": "T0030",
      "depends_on_section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "task_count": 10
    },
    {
      "name": "3. قراردادهای مشترک شبکه و داده",
      "first_task": "T0031",
      "last_task": "T0040",
      "depends_on_section": "2. ساختار مخزن و ابزارهای توسعه",
      "task_count": 10
    },
    {
      "name": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "first_task": "T0041",
      "last_task": "T0048",
      "depends_on_section": "3. قراردادهای مشترک شبکه و داده",
      "task_count": 8
    },
    {
      "name": "5. تشخیص سخت‌افزار و ظرفیت",
      "first_task": "T0049",
      "last_task": "T0056",
      "depends_on_section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "task_count": 8
    },
    {
      "name": "6. موتور استنتاج محلی واقعی",
      "first_task": "T0057",
      "last_task": "T0067",
      "depends_on_section": "5. تشخیص سخت‌افزار و ظرفیت",
      "task_count": 11
    },
    {
      "name": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "first_task": "T0068",
      "last_task": "T0075",
      "depends_on_section": "6. موتور استنتاج محلی واقعی",
      "task_count": 8
    },
    {
      "name": "8. دانلود، نصب، انتخاب و حذف مدل",
      "first_task": "T0076",
      "last_task": "T0087",
      "depends_on_section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "task_count": 12
    },
    {
      "name": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "first_task": "T0088",
      "last_task": "T0097",
      "depends_on_section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "task_count": 10
    },
    {
      "name": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "first_task": "T0098",
      "last_task": "T0107",
      "depends_on_section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "task_count": 10
    },
    {
      "name": "11. سرویس شبکه روی میزبان ویندوز",
      "first_task": "T0108",
      "last_task": "T0115",
      "depends_on_section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "task_count": 8
    },
    {
      "name": "12. معماری P2P و کشف میزبان",
      "first_task": "T0116",
      "last_task": "T0128",
      "depends_on_section": "11. سرویس شبکه روی میزبان ویندوز",
      "task_count": 13
    },
    {
      "name": "13. هویت ارتباطی و رمزنگاری",
      "first_task": "T0129",
      "last_task": "T0138",
      "depends_on_section": "12. معماری P2P و کشف میزبان",
      "task_count": 10
    },
    {
      "name": "14. اپلیکیشن واقعی اندروید",
      "first_task": "T0139",
      "last_task": "T0150",
      "depends_on_section": "13. هویت ارتباطی و رمزنگاری",
      "task_count": 12
    },
    {
      "name": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "first_task": "T0151",
      "last_task": "T0160",
      "depends_on_section": "14. اپلیکیشن واقعی اندروید",
      "task_count": 10
    },
    {
      "name": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "first_task": "T0161",
      "last_task": "T0177",
      "depends_on_section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "task_count": 17
    },
    {
      "name": "17. امنیت داده و سیاست نگهداری",
      "first_task": "T0178",
      "last_task": "T0188",
      "depends_on_section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "task_count": 11
    },
    {
      "name": "18. خطا، مشاهده‌پذیری و پایداری",
      "first_task": "T0189",
      "last_task": "T0195",
      "depends_on_section": "17. امنیت داده و سیاست نگهداری",
      "task_count": 7
    },
    {
      "name": "19. آزمون‌های واقعی و معیارهای قبولی",
      "first_task": "T0196",
      "last_task": "T0208",
      "depends_on_section": "18. خطا، مشاهده‌پذیری و پایداری",
      "task_count": 13
    },
    {
      "name": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "first_task": "T0209",
      "last_task": "T0218",
      "depends_on_section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "task_count": 10
    },
    {
      "name": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "first_task": "T0219",
      "last_task": "T0229",
      "depends_on_section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "task_count": 11
    }
  ],
  "nodes": [
    {
      "id": "T0001",
      "title": "برای هر وظیفهٔ کدنویسی، فایل یا فایل‌های واقعی خروجی را در مسیر تعیین‌شده ایجاد کن؛ ثبت توضیح یا طرح به‌تنهایی تکمیل محسوب نمی‌شود.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "مفروضات/ابزارهای توسعه و نیازمندی‌های همین بخش"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای هر وظیفهٔ کدنویسی، فایل یا فایل‌های واقعی خروجی را در مسیر تعیین‌شده ایجاد کن؛ ثبت توضیح یا طرح به‌تنهایی تکمیل محسوب نمی‌شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای هر وظیفهٔ کدنویسی، فایل یا فایل‌های واقعی خروجی را در مسیر تعیین‌شده ایجاد کن؛ ثبت توضیح یا طرح به‌تنهایی تکمیل محسوب نمی‌شود.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای مفروضات/ابزارهای توسعه و نیازمندی‌های همین بخش تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "انجام‌شده",
      "actual_output_paths": [
        "tools/graph_guard.py",
        "tools/__init__.py",
        "tests/test_graph_guard.py",
        "docs/acceptance.md",
        "step three/T0001/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [
        "step three/T0001/execution-report.md",
        "tests/test_graph_guard.py",
        "docs/acceptance.md"
      ],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود. اجرای T0001 در تلاش T0001-A001 به کمک ابزار اعتبارسنجی گراف، ۱۱ آزمون موفق و بررسی compileall ثبت شد. عبارت متنی depends_on که شناسهٔ گره نیست، حفظ و به‌عنوان هشدار گزارش شد.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0001-A001",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "tools/graph_guard.py",
          "tools/__init__.py",
          "tests/test_graph_guard.py",
          "docs/acceptance.md",
          "step three/T0001/execution-report.md",
          "step three/project-graph.md"
        ],
        "related_commits": [],
        "tests_passed": [
          "python -m unittest discover -s tests -v — 11 passed",
          "python -m compileall -q tools tests — passed"
        ],
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
      "execution_history": [
        {
          "attempt_id": "T0001-A001",
          "started_at": "2026-10-21T17:44:00+03:30",
          "ended_at": "2026-10-21T17:50:00+03:30",
          "result": "succeeded",
          "summary": "Added a standard-library graph guard, eleven automated tests, and an initial executable acceptance policy. Unit tests and bytecode compilation passed.",
          "changed_files": [
            "tools/graph_guard.py",
            "tools/__init__.py",
            "tests/test_graph_guard.py",
            "docs/acceptance.md",
            "step three/T0001/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "summary": "11 tests passed"
            },
            {
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "summary": "No syntax compilation errors"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Next task is T0002. This record is included in V 3.0.3; resolve its commit SHA from the online main history because self-referential commit IDs cannot be embedded in the commit itself."
        }
      ]
    },
    {
      "id": "T0002",
      "title": "برای هر قابلیت، آزمون خودکار متناسب بنویس و اجرای موفق آن را در CI ثبت کن.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0001"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای هر قابلیت، آزمون خودکار متناسب بنویس و اجرای موفق آن را در CI ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای هر قابلیت، آزمون خودکار متناسب بنویس و اجرای موفق آن را در CI ثبت کن.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0001 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "تکمیل‌شده",
      "actual_output_paths": [
        ".github/workflows/quality.yml",
        "step three/T0002/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [
        "step three/T0002/execution-report.md",
        "tests/test_graph_guard.py"
      ],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود. تلاش T0002-A001 یک workflow واقعی CI و gate آزمون‌های محلی ایجاد کرد. وضعیت تا مشاهدهٔ اجرای موفق GitHub Actions عمداً در حال انجام باقی مانده است. CI آنلاین در commit b35fe908672f1232dee0bf6b7c5d5babd6146a4b با موفقیت اجرا شد؛ تمام هفت مرحلهٔ job موفق بودند.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0002-A001",
        "last_attempt_result": "succeeded",
        "changed_files": [
          ".github/workflows/quality.yml",
          "step three/T0002/execution-report.md",
          "step three/project-graph.md"
        ],
        "related_commits": [
          "b35fe908672f1232dee0bf6b7c5d5babd6146a4b"
        ],
        "tests_passed": [
          "python -m unittest discover -s tests -v — 11 passed locally",
          "python -m compileall -q tools tests — passed",
          "workflow static checks — passed",
          "GitHub Actions run 39617156456 — success; all seven workflow steps passed."
        ],
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
      "execution_history": [
        {
          "attempt_id": "T0002-A001",
          "started_at": "2026-10-21T17:57:02+03:30",
          "ended_at": "2026-10-21T18:29:06+03:30",
          "result": "succeeded",
          "summary": "Added the quality-gate workflow; local checks and the actual GitHub Actions run passed.",
          "changed_files": [
            ".github/workflows/quality.yml",
            "step three/T0002/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": "b35fe908672f1232dee0bf6b7c5d5babd6146a4b",
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "summary": "11 tests passed locally"
            },
            {
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "summary": "Python files compiled"
            },
            {
              "command": "GitHub Actions quality workflow",
              "result": "passed",
              "summary": "GitHub Actions run 39617156456 completed successfully on commit b35fe908672f1232dee0bf6b7c5d5babd6146a4b."
            },
            {
              "command": "GitHub Actions run 39617156456",
              "result": "passed",
              "summary": "All workflow steps succeeded on commit b35fe908672f1232dee0bf6b7c5d5babd6146a4b.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39617156456"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Inspect the workflow run for commit V 3.0.4. If green, record CI URL/result and mark T0002 done in a subsequent verified version commit; otherwise inspect logs and fix."
        }
      ]
    },
    {
      "id": "T0003",
      "title": "وظیفه را فقط وقتی تمام‌شده علامت بزن که کد، آزمون و معیار پذیرش آن وجود داشته باشند.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0002"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وظیفه را فقط وقتی تمام‌شده علامت بزن که کد، آزمون و معیار پذیرش آن وجود داشته باشند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وظیفه را فقط وقتی تمام‌شده علامت بزن که کد، آزمون و معیار پذیرش آن وجود داشته باشند.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0002 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "تکمیل‌شده",
      "actual_output_paths": [
        "tools/graph_guard.py",
        "tests/test_graph_guard.py",
        "docs/acceptance.md",
        "step three/T0003/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [
        "tests/test_graph_guard.py",
        "docs/acceptance.md",
        "step three/T0003/execution-report.md"
      ],
      "notes": "Completion verified by GitHub Actions run 39620382487 on commit b7f96e5cb1dc6d0cec3d7f209d4757397d3aeee1. This validates the graph guard's acceptance/evidence checks; it does not claim the complete Windows/Android/P2P product is implemented. Edge T0002 → T0003 remains type=blocks and validation_status=unreviewed; no edge classification was changed.",
      "execution_summary": {
        "attempt_count": 2,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0003-A002",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "tools/graph_guard.py",
          "tests/test_graph_guard.py",
          "docs/acceptance.md",
          "step three/project-graph.md",
          "step three/T0003/execution-report.md"
        ],
        "related_commits": [
          "b7f96e5cb1dc6d0cec3d7f209d4757397d3aeee1"
        ],
        "tests_passed": [
          "GitHub Actions run 39620382487: automated tests",
          "GitHub Actions run 39620382487: Python syntax check",
          "GitHub Actions run 39620382487: graph structure validation",
          "GitHub Actions run 39620382487: recorded outputs and evidence validation"
        ],
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
      "execution_history": [
        {
          "attempt_id": "T0003-A001",
          "started_at": "2026-10-21T18:35:00+03:30",
          "ended_at": "2026-10-21T18:35:00+03:30",
          "result": "partial",
          "summary": "Added acceptance-criteria and test-source evidence requirements to the graph guard and added regression tests. Local interpreter execution was unavailable in this tool context; CI confirmation is pending.",
          "changed_files": [
            "tools/graph_guard.py",
            "tests/test_graph_guard.py",
            "docs/acceptance.md",
            "step three/project-graph.md",
            "step three/T0003/execution-report.md"
          ],
          "commit_sha": null,
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "not_run",
              "summary": "No local Python execution tool available in this run; await GitHub Actions."
            },
            {
              "command": "GitHub Actions quality workflow",
              "result": "pending",
              "summary": "Must be checked after this commit is pushed."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Check the quality workflow for this commit; if green, update T0003 to done in a subsequent verified commit. If red, inspect logs and correct the implementation."
        },
        {
          "attempt_id": "T0003-A002",
          "started_at": "2026-10-21T18:50:00+03:30",
          "ended_at": "2026-10-21T18:50:00+03:30",
          "result": "succeeded",
          "summary": "CI quality gate passed on commit b7f96e5cb1dc6d0cec3d7f209d4757397d3aeee1 (run 39620382487). Automated tests, Python syntax, graph structure, and recorded-output/evidence validation all succeeded. The task output paths and regression tests are present; this confirms the graph-guard behavior, not the complete Windows/Android/P2P product.",
          "changed_files": [
            "tools/graph_guard.py",
            "tests/test_graph_guard.py",
            "docs/acceptance.md",
            "step three/project-graph.md",
            "step three/T0003/execution-report.md"
          ],
          "commit_sha": "b7f96e5cb1dc6d0cec3d7f209d4757397d3aeee1",
          "tests": [
            {
              "command": "GitHub Actions run 39620382487 — Run automated tests",
              "result": "passed",
              "summary": "Job step completed successfully."
            },
            {
              "command": "GitHub Actions run 39620382487 — Check Python syntax",
              "result": "passed",
              "summary": "Job step completed successfully."
            },
            {
              "command": "GitHub Actions run 39620382487 — Validate graph structure",
              "result": "passed",
              "summary": "Job step completed successfully."
            },
            {
              "command": "GitHub Actions run 39620382487 — Validate recorded outputs and evidence",
              "result": "passed",
              "summary": "Job step completed successfully."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Begin T0004 only after reviewing its specific acceptance criteria and validating its dependency edge. Keep all unreviewed edge classifications unchanged."
        }
      ]
    },
    {
      "id": "T0004",
      "title": "هیچ فایل ساختگی، تابع خالی، TODO، دادهٔ نمونه یا شبیه‌ساز را به‌عنوان پیاده‌سازی نهایی قابلیت معرفی نکن.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0003"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هیچ فایل ساختگی، تابع خالی، TODO، دادهٔ نمونه یا شبیه‌ساز را به‌عنوان پیاده‌سازی نهایی قابلیت معرفی نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هیچ فایل ساختگی، تابع خالی، TODO، دادهٔ نمونه یا شبیه‌ساز را به‌عنوان پیاده‌سازی نهایی قابلیت معرفی نکن.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0003 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "تکمیل‌شده",
      "actual_output_paths": [
        "tools/graph_guard.py",
        "tests/test_graph_guard.py",
        "docs/acceptance.md",
        "step three/T0004/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [
        "tests/test_graph_guard.py",
        "docs/acceptance.md",
        "step three/T0004/execution-report.md"
      ],
      "notes": "T0004-A001 failed CI due to false positives and is preserved in history. T0004-A002 corrected the detector; GitHub Actions run 39622120539 on code commit 357b9b280b73084d582283a654222cf8ce3b1dfc passed automated tests, syntax, graph structure, and recorded-output/evidence validation. This verifies the known-placeholder detector's defined scope only, not overall product correctness.",
      "execution_summary": {
        "attempt_count": 2,
        "failed_attempt_count": 1,
        "last_attempt_id": "T0004-A002",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "tools/graph_guard.py",
          "tests/test_graph_guard.py",
          "docs/acceptance.md",
          "step three/project-graph.md",
          "step three/T0004/execution-report.md"
        ],
        "related_commits": [
          "1cdf8ebd12fa258c49e211d0f33ff430d29029a0",
          "357b9b280b73084d582283a654222cf8ce3b1dfc"
        ],
        "tests_passed": [
          "GitHub Actions run 39622120539: Run automated tests",
          "GitHub Actions run 39622120539: Check Python syntax",
          "GitHub Actions run 39622120539: Validate graph structure",
          "GitHub Actions run 39622120539: Validate recorded outputs and evidence"
        ],
        "tests_failed": [
          "GitHub Actions run 39621942397: Validate recorded outputs and evidence"
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
          "attempt_id": "T0004-A001",
          "started_at": "2026-10-21T19:00:00+03:30",
          "ended_at": "2026-10-21T19:01:00+03:30",
          "result": "failed",
          "summary": "Implemented a repository-backed static guard that rejects known placeholder markers in source files listed as actual outputs for done tasks. Added regression tests for rejecting NotImplementedError and accepting ordinary executable source, and documented the scope and limitations. CI has not yet run on this change; completion is not claimed. A code review correction excludes the guard itself and test-source directories from scanning to prevent false positives from detector patterns and intentional negative tests.",
          "changed_files": [
            "tools/graph_guard.py",
            "tests/test_graph_guard.py",
            "docs/acceptance.md",
            "step three/project-graph.md",
            "step three/T0004/execution-report.md"
          ],
          "commit_sha": "1cdf8ebd12fa258c49e211d0f33ff430d29029a0",
          "tests": [
            {
              "command": "GitHub Actions run 39621942397 — Validate recorded outputs and evidence",
              "result": "failed",
              "summary": "Repository-backed validation failed because the detector matched its own marker strings and intentional negative test fixtures; the failure was diagnosed from the workflow step result."
            }
          ],
          "failure_details": "CI run 39621942397 failed at 'Validate recorded outputs and evidence'. Root cause: placeholder detector scanned its own source and test fixtures, causing false positives.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Corrected in T0004-A002 by excluding the guard implementation and test-source directories from the detector scan; verify the correction commit CI."
        },
        {
          "attempt_id": "T0004-A002",
          "started_at": "2026-10-21T19:02:00+03:30",
          "ended_at": "2026-10-21T19:05:00+03:30",
          "result": "succeeded",
          "summary": "Corrected false positives by excluding the guard itself and test-source directories from the source placeholder scan. Awaiting CI on commit 357b9b280b73084d582283a654222cf8ce3b1dfc.",
          "changed_files": [
            "tools/graph_guard.py",
            "docs/acceptance.md",
            "step three/project-graph.md",
            "step three/T0004/execution-report.md"
          ],
          "commit_sha": "357b9b280b73084d582283a654222cf8ce3b1dfc",
          "tests": [
            {
              "command": "GitHub Actions run 39622120539 — Run automated tests",
              "result": "passed",
              "summary": "All automated unit tests passed."
            },
            {
              "command": "GitHub Actions run 39622120539 — Check Python syntax",
              "result": "passed",
              "summary": "Python syntax check passed."
            },
            {
              "command": "GitHub Actions run 39622120539 — Validate graph structure",
              "result": "passed",
              "summary": "Graph structure validation passed."
            },
            {
              "command": "GitHub Actions run 39622120539 — Validate recorded outputs and evidence",
              "result": "passed",
              "summary": "Repository-backed output and evidence validation passed."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "T0004 acceptance criteria verified on commit 357b9b280b73084d582283a654222cf8ce3b1dfc. Continue with T0005 after verifying its prerequisite and preserving unreviewed edge classifications."
        }
      ]
    },
    {
      "id": "T0005",
      "title": "برای هر بخش، یک مسیر اجرای واقعی تعریف کن و در صورت شکست، خطا را اصلاح کن؛ صرف تولید فایل بدون build موفق کافی نیست.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0004"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای هر بخش، یک مسیر اجرای واقعی تعریف کن و در صورت شکست، خطا را اصلاح کن؛ صرف تولید فایل بدون build موفق کافی نیست."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای هر بخش، یک مسیر اجرای واقعی تعریف کن و در صورت شکست، خطا را اصلاح کن؛ صرف تولید فایل بدون build موفق کافی نیست.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0004 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "blocked",
      "status_fa": "مسدودشده",
      "actual_output_paths": [
        "step three/T0005/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [
        "step three/T0005/execution-report.md",
        "docs/acceptance.md",
        ".github/workflows/quality.yml"
      ],
      "notes": "Blocked: repository snapshot has no actual Windows/.NET, Android/Gradle, or shared-contract project/build files. Existing CI validates only Python graph tooling, so T0005 cannot claim a successful product build. Edge T0004 -> T0005 remains unreviewed.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0005-A001",
        "last_attempt_result": "blocked",
        "changed_files": [
          "step three/project-graph.md",
          "step three/T0005/execution-report.md",
          "docs/acceptance.md"
        ],
        "related_commits": [],
        "tests_passed": [],
        "tests_failed": [],
        "blocked_reason": "No application project or reproducible Windows/Android build entrypoint exists in the inspected main snapshot.",
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
          "attempt_id": "T0005-A001",
          "started_at": "2026-10-21T18:15:00+03:00",
          "ended_at": "2026-10-21T18:20:00+03:00",
          "result": "blocked",
          "summary": "Inspected the main tree, existing quality workflow, T0004 evidence, and current Actions status. Only Python graph tooling/tests are buildable in the current snapshot; product build paths for Windows, Android, and shared contracts are absent. Did not claim a product build or create a dummy implementation.",
          "changed_files": [
            "step three/project-graph.md",
            "step three/T0005/execution-report.md",
            "docs/acceptance.md"
          ],
          "commit_sha": null,
          "tests": [],
          "failure_details": null,
          "blocked_reason": "No application project/build entrypoints exist in the inspected snapshot.",
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Implement the real project scaffolds/build configuration in their dependency order, add actual Windows and Android build jobs, then retry T0005."
        }
      ]
    },
    {
      "id": "T0006",
      "title": "اگر قابلیت به زیرساخت یا سخت‌افزار در دسترس وابسته است، آزمون واحد و آزمون یکپارچهٔ قابل‌اجرا بساز و محدودیت آزمون واقعی را صریح ثبت کن.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0005"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اگر قابلیت به زیرساخت یا سخت‌افزار در دسترس وابسته است، آزمون واحد و آزمون یکپارچهٔ قابل‌اجرا بساز و محدودیت آزمون واقعی را صریح ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اگر قابلیت به زیرساخت یا سخت‌افزار در دسترس وابسته است، آزمون واحد و آزمون یکپارچهٔ قابل‌اجرا بساز و محدودیت آزمون واقعی را صریح ثبت کن.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0005 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0007",
      "title": "هر تصمیم فنی را در کد اجرایی، پیکربندی و آزمون‌های متناظر منعکس کن؛ سند به‌تنهایی معیار پذیرش نیست.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0006"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هر تصمیم فنی را در کد اجرایی، پیکربندی و آزمون‌های متناظر منعکس کن؛ سند به‌تنهایی معیار پذیرش نیست."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هر تصمیم فنی را در کد اجرایی، پیکربندی و آزمون‌های متناظر منعکس کن؛ سند به‌تنهایی معیار پذیرش نیست.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0006 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0008",
      "title": "هر ادعای «محلی»، «P2P»، «رمزگذاری‌شده» یا «بدون ذخیره‌سازی» را با آزمون یا بررسی قابل‌تکرار اثبات کن.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0007"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هر ادعای «محلی»، «P2P»، «رمزگذاری‌شده» یا «بدون ذخیره‌سازی» را با آزمون یا بررسی قابل‌تکرار اثبات کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هر ادعای «محلی»، «P2P»، «رمزگذاری‌شده» یا «بدون ذخیره‌سازی» را با آزمون یا بررسی قابل‌تکرار اثبات کن.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0007 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0009",
      "title": "هیچ API مرکزی برای دریافت متن درخواست، تولید پاسخ هوش مصنوعی یا ذخیرهٔ تاریخچهٔ مکالمه اضافه نکن.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0008"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هیچ API مرکزی برای دریافت متن درخواست، تولید پاسخ هوش مصنوعی یا ذخیرهٔ تاریخچهٔ مکالمه اضافه نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هیچ API مرکزی برای دریافت متن درخواست، تولید پاسخ هوش مصنوعی یا ذخیرهٔ تاریخچهٔ مکالمه اضافه نکن.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0008 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0010",
      "title": "تمام مراحل ساخت و آزمون را از محیط تمیز و طبق دستورهای ثبت‌شده قابل‌تکرار نگه دار.",
      "section": "قانون تکمیل وظیفه",
      "depends_on": [
        "T0009"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تمام مراحل ساخت و آزمون را از محیط تمیز و طبق دستورهای ثبت‌شده قابل‌تکرار نگه دار."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تمام مراحل ساخت و آزمون را از محیط تمیز و طبق دستورهای ثبت‌شده قابل‌تکرار نگه دار.» در مسیر docs/acceptance.md + tests/ (معیار و آزمون متناظر)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0009 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/acceptance.md + tests/ (معیار و آزمون متناظر)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0011",
      "title": "فایل `README.md` را با هدف محصول، اجزای اجرایی، محدودهٔ نسخهٔ نخست و محدودیت‌های واقعی ایجاد کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فایل `README.md` را با هدف محصول، اجزای اجرایی، محدودهٔ نسخهٔ نخست و محدودیت‌های واقعی ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «فایل `README.md` را با هدف محصول، اجزای اجرایی، محدودهٔ نسخهٔ نخست و محدودیت‌های واقعی ایجاد کن.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "این وظیفه می‌تواند مستقل از تکمیل T0010 آغاز شود؛ README باید وضعیت واقعی اجزای محصول و محدودیت build را صادقانه بیان کند. مسیرهای ساخت قابل‌تکرار در T0010 همچنان جداگانه الزامی‌اند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "انجام‌شده",
      "actual_output_paths": [
        "README.md",
        "tests/test_readme_contract.py",
        "step three/T0011/execution-report.md"
      ],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [
        "tests/test_readme_contract.py",
        "step three/T0011/execution-report.md"
      ],
      "notes": "README محصول و آزمون قراردادی افزوده شدند. GitHub Actions run 39625575272 روی commit ec9aa13ff4bd78ea82b6ea75413967975a399e13 موفق شد؛ آزمون‌های خودکار، بررسی syntax، ساختار گراف و خروجی/شواهد همگی موفق بودند. این وظیفه فقط مستندات صادقانهٔ هدف/محدوده را تکمیل می‌کند و به معنای ساخت محصول Windows/Android/P2P نیست.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0011-A001",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "README.md",
          "tests/test_readme_contract.py",
          "step three/T0011/execution-report.md",
          "step three/project-graph.md"
        ],
        "related_commits": [
          "ec9aa13ff4bd78ea82b6ea75413967975a399e13"
        ],
        "tests_passed": [
          "python -m unittest discover -s tests -v — passed in GitHub Actions run 39625575272",
          "python -m compileall -q tools tests — passed in GitHub Actions run 39625575272",
          "python tools/graph_guard.py step three/project-graph.md — passed in GitHub Actions run 39625575272",
          "python tools/graph_guard.py step three/project-graph.md --repo-root . — passed in GitHub Actions run 39625575272"
        ],
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
      "execution_history": [
        {
          "attempt_id": "T0011-A001",
          "started_at": "2026-10-21T18:55:00+03:30",
          "ended_at": "2026-10-21T18:57:00+03:30",
          "result": "succeeded",
          "summary": "Created product README and five automated README contract tests. GitHub Actions run 39625575272 passed all quality-gate steps.",
          "changed_files": [
            "README.md",
            "tests/test_readme_contract.py",
            "step three/T0011/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": "ec9aa13ff4bd78ea82b6ea75413967975a399e13",
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "summary": "The automated test step passed in GitHub Actions run 39625575272."
            },
            {
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "summary": "The syntax-check step passed in GitHub Actions run 39625575272."
            },
            {
              "command": "python tools/graph_guard.py step three/project-graph.md",
              "result": "passed",
              "summary": "Graph structure validation passed in GitHub Actions run 39625575272."
            },
            {
              "command": "python tools/graph_guard.py step three/project-graph.md --repo-root .",
              "result": "passed",
              "summary": "Output/evidence validation passed in GitHub Actions run 39625575272."
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "Continue with T0012 after reviewing its dependencies; do not infer application build completion from this documentation task."
        }
      ]
    },
    {
      "id": "T0012",
      "title": "فایل `docs/requirements.md` را با نیازمندی‌های قابل‌آزمون و شناسهٔ یکتا برای هر الزام ایجاد کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0011"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فایل `docs/requirements.md` را با نیازمندی‌های قابل‌آزمون و شناسهٔ یکتا برای هر الزام ایجاد کن."
      ],
      "outputs": [
        "A testable requirements specification at docs/requirements.md with unique IDs, measurable acceptance criteria, verification methods, and traceability to planned task IDs; automated contract tests under tests/."
      ],
      "start_condition": "T0011 must be complete so the requirements can refine the documented product scope. The T0010 edge has been validated as sequencing_only: reproducible product builds remain required elsewhere but are not a technical prerequisite to specifying testable requirements. Do not infer product build readiness from this task.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "انجام‌شده",
      "actual_output_paths": [
        "docs/requirements.md",
        "tests/test_requirements_contract.py",
        "step three/T0012/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/requirements.md",
        "tests/test_requirements_contract.py",
        "step three/T0012/execution-report.md"
      ],
      "evidence": [
        "tests/test_requirements_contract.py",
        "step three/T0012/execution-report.md"
      ],
      "notes": "The requirements baseline and its automated contract tests are present. GitHub Actions run 39634235372 on commit df28b70a13b215d4050dff048b3bf935d6711b23 succeeded: automated tests, Python syntax, graph structure, and repository-backed output/evidence validation all passed. This confirms the requirements-document contract, not implementation of the Windows/Android/P2P product. T0010 remains outstanding and T0013 is still gated by T0010.",
      "execution_summary": {
        "attempt_count": 1,
        "failed_attempt_count": 0,
        "last_attempt_id": "T0012-A001",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "docs/requirements.md",
          "tests/test_requirements_contract.py",
          "step three/T0012/execution-report.md",
          "step three/project-graph.md"
        ],
        "related_commits": [
          "df28b70a13b215d4050dff048b3bf935d6711b23"
        ],
        "tests_passed": [
          "python -m unittest discover -s tests -v — passed in GitHub Actions run 39634235372",
          "python -m compileall -q tools tests — passed in GitHub Actions run 39634235372",
          "python tools/graph_guard.py \"step three/project-graph.md\" — passed in GitHub Actions run 39634235372",
          "python tools/graph_guard.py \"step three/project-graph.md\" --repo-root . — passed in GitHub Actions run 39634235372"
        ],
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
      "execution_history": [
        {
          "attempt_id": "T0012-A001",
          "started_at": "2026-10-21T19:10:00+03:30",
          "ended_at": "2026-10-21T19:17:00+03:30",
          "result": "succeeded",
          "summary": "Created a 20-requirement testable specification and automated contract tests. CI quality gate passed on implementation commit df28b70a13b215d4050dff048b3bf935d6711b23 (run 39634235372).",
          "changed_files": [
            "docs/requirements.md",
            "tests/test_requirements_contract.py",
            "step three/T0012/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": "df28b70a13b215d4050dff048b3bf935d6711b23",
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "summary": "Automated test step succeeded in GitHub Actions run 39634235372.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39634235372"
            },
            {
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "summary": "Python syntax check succeeded in GitHub Actions run 39634235372.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39634235372"
            },
            {
              "command": "python tools/graph_guard.py \"step three/project-graph.md\"",
              "result": "passed",
              "summary": "Graph structure validation succeeded in GitHub Actions run 39634235372.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39634235372"
            },
            {
              "command": "python tools/graph_guard.py \"step three/project-graph.md\" --repo-root .",
              "result": "passed",
              "summary": "Recorded outputs and evidence validation succeeded in GitHub Actions run 39634235372.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39634235372"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Validated T0011 → T0012 as contract_or_interface_dependency with rationale and evidence.",
            "Validated T0010 → T0012 as sequencing_only with rationale and evidence; T0010 remains outstanding."
          ],
          "follow_up": "Proceed to T0013 only after T0010 is genuinely completed; otherwise resolve the T0005 build-path blocker and continue the prerequisite chain without claiming product-build success."
        }
      ]
    },
    {
      "id": "T0013",
      "title": "فایل `docs/acceptance.md` را با آزمون‌های قبولی نهایی میزبان ویندوز، اندروید و تبادل P2P ایجاد کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0012",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فایل `docs/acceptance.md` را با آزمون‌های قبولی نهایی میزبان ویندوز، اندروید و تبادل P2P ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «فایل `docs/acceptance.md` را با آزمون‌های قبولی نهایی میزبان ویندوز، اندروید و تبادل P2P ایجاد کن.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "T0012 must be complete so each acceptance gate traces to the approved requirements baseline. T0010 is a sequencing-only prerequisite for authoring this acceptance matrix, but its reproducible-build requirement remains mandatory before final release acceptance. Do not mark runtime, platform-build, privacy, or P2P gates passed until their real tests run.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "done",
      "status_fa": "انجام‌شده",
      "actual_output_paths": [
        "docs/acceptance.md",
        "tests/test_acceptance_contract.py",
        "step three/T0013/execution-report.md"
      ],
      "expected_output_paths": [
        "docs/acceptance.md",
        "tests/test_acceptance_contract.py",
        "step three/T0013/execution-report.md"
      ],
      "evidence": [
        "tests/test_acceptance_contract.py",
        "step three/T0013/execution-report.md"
      ],
      "notes": "The acceptance matrix, requirement-to-gate traceability table, and automated document-contract tests are present. GitHub Actions run 39638208711 on commit d5e0f9d8837594e5de88de39b5722604fd03d8b0 succeeded: 29 unit tests, Python syntax, graph structure, and recorded-output/evidence validation all passed. This confirms the acceptance-document contract only; every runtime/build/network gate remains NOT RUN until real Windows, Android, local-inference, and P2P projects and test harnesses exist. T0010 remains outstanding; T0005 remains blocked.",
      "execution_summary": {
        "attempt_count": 2,
        "failed_attempt_count": 1,
        "last_attempt_id": "T0013-A002",
        "last_attempt_result": "succeeded",
        "changed_files": [
          "docs/acceptance.md",
          "tests/test_acceptance_contract.py",
          "step three/T0013/execution-report.md",
          "step three/project-graph.md"
        ],
        "related_commits": [
          "22bf4cd30722ddeed647884cc15fa977015d9745",
          "d5e0f9d8837594e5de88de39b5722604fd03d8b0"
        ],
        "tests_passed": [
          "python -m unittest discover -s tests -v — passed (29 tests) in GitHub Actions run 39638208711",
          "python -m compileall -q tools tests — passed in GitHub Actions run 39638208711",
          "python tools/graph_guard.py \"step three/project-graph.md\" — passed in GitHub Actions run 39638208711",
          "python tools/graph_guard.py \"step three/project-graph.md\" --repo-root . — passed in GitHub Actions run 39638208711"
        ],
        "tests_failed": [
          "python -m unittest discover -s tests -v — failed in run 39637948316; traceability regex did not allow spaces in multi-gate rows. Corrected in T0013-A002 and rerun successfully."
        ],
        "blocked_reason": null,
        "reopened_reason": null,
        "contract_revalidation": {
          "state": "completed",
          "triggering_contracts": [
            "REQ-001 through REQ-020"
          ],
          "affected_by_contract_changes": [],
          "review_evidence": [
            "docs/requirements.md",
            "docs/acceptance.md",
            "tests/test_acceptance_contract.py",
            "GitHub Actions run 39638208711"
          ]
        }
      },
      "execution_history": [
        {
          "attempt_id": "T0013-A001",
          "started_at": "2026-10-21T20:12:00+03:30",
          "ended_at": "2026-10-21T20:17:43+03:30",
          "result": "failed",
          "summary": "GitHub Actions run 39637948316 executed 29 tests and failed two subtests in the requirement-to-gate traceability regex for rows with multiple gates; the acceptance matrix content was present. The test pattern is being corrected.",
          "changed_files": [
            "docs/acceptance.md",
            "tests/test_acceptance_contract.py",
            "step three/T0013/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": "22bf4cd30722ddeed647884cc15fa977015d9745",
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "failed",
              "summary": "29 tests ran; two subtests failed because regex AC-[A-Z0-9,-]+ rejected the space after a comma in multi-gate mappings.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39637948316"
            }
          ],
          "failure_details": "The new traceability test used a character class that excluded spaces between multiple gate IDs. It incorrectly rejected REQ-001 and REQ-009 mappings.",
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [
            "Validated T0012 → T0013 as contract_or_interface_dependency.",
            "Validated T0010 → T0013 as sequencing_only for authoring the acceptance matrix; final build gates remain mandatory."
          ],
          "follow_up": "Correct the traceability regex to accept comma-separated gate IDs with spaces, then rerun the full GitHub Actions quality workflow."
        },
        {
          "attempt_id": "T0013-A002",
          "started_at": "2026-10-21T20:19:00+03:30",
          "ended_at": "2026-10-21T20:19:34+03:30",
          "result": "succeeded",
          "summary": "Corrected the traceability regex to accept comma-separated gate IDs with spaces. GitHub Actions run 39638208711 passed all 29 unit tests, Python syntax validation, graph validation, and recorded-output/evidence validation.",
          "changed_files": [
            "tests/test_acceptance_contract.py",
            "step three/T0013/execution-report.md",
            "step three/project-graph.md"
          ],
          "commit_sha": "d5e0f9d8837594e5de88de39b5722604fd03d8b0",
          "tests": [
            {
              "command": "python -m unittest discover -s tests -v",
              "result": "passed",
              "summary": "29 tests passed in GitHub Actions.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39638208711"
            },
            {
              "command": "python -m compileall -q tools tests",
              "result": "passed",
              "summary": "Python syntax validation passed.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39638208711"
            },
            {
              "command": "python tools/graph_guard.py \"step three/project-graph.md\"",
              "result": "passed",
              "summary": "Graph structure validation passed.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39638208711"
            },
            {
              "command": "python tools/graph_guard.py \"step three/project-graph.md\" --repo-root .",
              "result": "passed",
              "summary": "Recorded-output and evidence validation passed.",
              "url": "https://github.com/artinrahimii/project/actions/runs/39638208711"
            }
          ],
          "failure_details": null,
          "blocked_reason": null,
          "reopened_reason": null,
          "contract_changes": [],
          "follow_up": "T0013 is complete for the acceptance-matrix/document contract. Do not treat the matrix as product runtime evidence; proceed to the next graph task only after checking its dependencies and resolving the T0010/T0005 build prerequisites where required."
        }
      ]
    },
    {
      "id": "T0014",
      "title": "نسخهٔ نخست ویندوز و Android SDK حداقل را در تنظیمات واقعی build تثبیت کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0013",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نسخهٔ نخست ویندوز و Android SDK حداقل را در تنظیمات واقعی build تثبیت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نسخهٔ نخست ویندوز و Android SDK حداقل را در تنظیمات واقعی build تثبیت کن.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0013 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0015",
      "title": "پشتهٔ میزبان را به C# و .NET LTS با رابط کاربری WPF انتخاب و پروژهٔ اجرایی آن را بساز.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0014",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پشتهٔ میزبان را به C# و .NET LTS با رابط کاربری WPF انتخاب و پروژهٔ اجرایی آن را بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پشتهٔ میزبان را به C# و .NET LTS با رابط کاربری WPF انتخاب و پروژهٔ اجرایی آن را بساز.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0014 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0016",
      "title": "پشتهٔ اندروید را به Kotlin، Jetpack Compose و Gradle انتخاب و پروژهٔ اجرایی آن را بساز.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0015",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پشتهٔ اندروید را به Kotlin، Jetpack Compose و Gradle انتخاب و پروژهٔ اجرایی آن را بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پشتهٔ اندروید را به Kotlin، Jetpack Compose و Gradle انتخاب و پروژهٔ اجرایی آن را بساز.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0015 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0017",
      "title": "قراردادهای تبادل را با Protobuf تعریف کن و فایل‌های تولیدشده را از فایل منبع قرارداد تولید کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0016",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: قراردادهای تبادل را با Protobuf تعریف کن و فایل‌های تولیدشده را از فایل منبع قرارداد تولید کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «قراردادهای تبادل را با Protobuf تعریف کن و فایل‌های تولیدشده را از فایل منبع قرارداد تولید کن.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0016 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0018",
      "title": "موتور محلی را پشت رابط مستقل قرار بده و برای نسخهٔ نخست یک backend واقعی قابل‌اجرا مانند llama.cpp server محلی را پشتیبانی کن.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0017",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: موتور محلی را پشت رابط مستقل قرار بده و برای نسخهٔ نخست یک backend واقعی قابل‌اجرا مانند llama.cpp server محلی را پشتیبانی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «موتور محلی را پشت رابط مستقل قرار بده و برای نسخهٔ نخست یک backend واقعی قابل‌اجرا مانند llama.cpp server محلی را پشتیبانی کن.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0017 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0019",
      "title": "مشخص کن اتصال مستقیم P2P مسیر پیش‌فرض داده است و سرویس کمکی احتمالی فقط برای معرفی/هماهنگی اتصال استفاده می‌شود.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0018",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مشخص کن اتصال مستقیم P2P مسیر پیش‌فرض داده است و سرویس کمکی احتمالی فقط برای معرفی/هماهنگی اتصال استفاده می‌شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مشخص کن اتصال مستقیم P2P مسیر پیش‌فرض داده است و سرویس کمکی احتمالی فقط برای معرفی/هماهنگی اتصال استفاده می‌شود.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0018 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0020",
      "title": "تصمیم‌های انتخاب فناوری را در `docs/decisions/` ثبت کن و هر تصمیم را به کد و آزمون متناظر پیوند بده.",
      "section": "1. تعریف نسخهٔ قابل‌ساخت و تصمیم‌های فنی",
      "depends_on": [
        "T0019",
        "T0010"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تصمیم‌های انتخاب فناوری را در `docs/decisions/` ثبت کن و هر تصمیم را به کد و آزمون متناظر پیوند بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تصمیم‌های انتخاب فناوری را در `docs/decisions/` ثبت کن و هر تصمیم را به کد و آزمون متناظر پیوند بده.» در مسیر README.md + docs/decisions/ + فایل‌های تنظیمات build؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0019 و T0010 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "README.md + docs/decisions/ + فایل‌های تنظیمات build"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0021",
      "title": "ساختار واقعی `src/Host.Windows/`، `src/Client.Android/`، `src/Shared.Contracts/`، `tests/` و `docs/` را ایجاد کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ساختار واقعی `src/Host.Windows/`، `src/Client.Android/`، `src/Shared.Contracts/`، `tests/` و `docs/` را ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ساختار واقعی `src/Host.Windows/`، `src/Client.Android/`، `src/Shared.Contracts/`، `tests/` و `docs/` را ایجاد کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0022",
      "title": "پروژهٔ solution دات‌نت و پروژه‌های buildable آن را در `src/Host.Windows/` و `src/Shared.Contracts/` ایجاد کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0021",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پروژهٔ solution دات‌نت و پروژه‌های buildable آن را در `src/Host.Windows/` و `src/Shared.Contracts/` ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پروژهٔ solution دات‌نت و پروژه‌های buildable آن را در `src/Host.Windows/` و `src/Shared.Contracts/` ایجاد کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0021 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0023",
      "title": "پروژهٔ Android Gradle را در `src/Client.Android/` با ماژول app اجرایی ایجاد کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0022",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پروژهٔ Android Gradle را در `src/Client.Android/` با ماژول app اجرایی ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پروژهٔ Android Gradle را در `src/Client.Android/` با ماژول app اجرایی ایجاد کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0022 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0024",
      "title": "نسخهٔ ابزارهای .NET، JDK، Android Gradle Plugin و Gradle را در فایل‌های واقعی قفل کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0023",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نسخهٔ ابزارهای .NET، JDK، Android Gradle Plugin و Gradle را در فایل‌های واقعی قفل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نسخهٔ ابزارهای .NET، JDK، Android Gradle Plugin و Gradle را در فایل‌های واقعی قفل کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0023 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0025",
      "title": "قواعد قالب‌بندی و lint برای C# و Kotlin را به ابزارهای قابل‌اجرا وصل کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0024",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: قواعد قالب‌بندی و lint برای C# و Kotlin را به ابزارهای قابل‌اجرا وصل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «قواعد قالب‌بندی و lint برای C# و Kotlin را به ابزارهای قابل‌اجرا وصل کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0024 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0026",
      "title": "فایل‌های نمونهٔ تنظیمات را بدون کلید محرمانه بساز و فایل‌های محلی محرمانه را در `.gitignore` قرار بده.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0025",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فایل‌های نمونهٔ تنظیمات را بدون کلید محرمانه بساز و فایل‌های محلی محرمانه را در `.gitignore` قرار بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «فایل‌های نمونهٔ تنظیمات را بدون کلید محرمانه بساز و فایل‌های محلی محرمانه را در `.gitignore` قرار بده.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0025 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0027",
      "title": "دستورهای restore، build، test و package را در `README.md` دقیق و قابل‌تکرار ثبت کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0026",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دستورهای restore، build، test و package را در `README.md` دقیق و قابل‌تکرار ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دستورهای restore، build، test و package را در `README.md` دقیق و قابل‌تکرار ثبت کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0026 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0028",
      "title": "workflow پیوستهٔ GitHub Actions برای build و آزمون کد دات‌نت ایجاد کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0027",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: workflow پیوستهٔ GitHub Actions برای build و آزمون کد دات‌نت ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «workflow پیوستهٔ GitHub Actions برای build و آزمون کد دات‌نت ایجاد کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0027 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0029",
      "title": "workflow پیوستهٔ GitHub Actions برای lint و build اپلیکیشن اندروید ایجاد کن.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0028",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: workflow پیوستهٔ GitHub Actions برای lint و build اپلیکیشن اندروید ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «workflow پیوستهٔ GitHub Actions برای lint و build اپلیکیشن اندروید ایجاد کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0028 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0030",
      "title": "CI را طوری تنظیم کن که شکست build، lint یا آزمون باعث شکست workflow شود.",
      "section": "2. ساختار مخزن و ابزارهای توسعه",
      "depends_on": [
        "T0029",
        "T0020"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: CI را طوری تنظیم کن که شکست build، lint یا آزمون باعث شکست workflow شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «CI را طوری تنظیم کن که شکست build، lint یا آزمون باعث شکست workflow شود.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0029 و T0020 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0031",
      "title": "پیام Protobuf نسخه‌دار `InferenceRequest` را با شناسهٔ موقت، متن پیام، capsule، محدودیت خروجی و زمان انقضا تعریف کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام Protobuf نسخه‌دار `InferenceRequest` را با شناسهٔ موقت، متن پیام، capsule، محدودیت خروجی و زمان انقضا تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام Protobuf نسخه‌دار `InferenceRequest` را با شناسهٔ موقت، متن پیام، capsule، محدودیت خروجی و زمان انقضا تعریف کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0032",
      "title": "پیام `InferenceResponse` را با شناسهٔ درخواست، وضعیت، متن پاسخ، خطای قابل‌نمایش و زمان تولید تعریف کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0031",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام `InferenceResponse` را با شناسهٔ درخواست، وضعیت، متن پاسخ، خطای قابل‌نمایش و زمان تولید تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام `InferenceResponse` را با شناسهٔ درخواست، وضعیت، متن پاسخ، خطای قابل‌نمایش و زمان تولید تعریف کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0031 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0033",
      "title": "پیام‌های `HostAnnouncement` و `HostCapabilities` را بدون متن درخواست یا شناسهٔ دائمی کاربر تعریف کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0032",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام‌های `HostAnnouncement` و `HostCapabilities` را بدون متن درخواست یا شناسهٔ دائمی کاربر تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام‌های `HostAnnouncement` و `HostCapabilities` را بدون متن درخواست یا شناسهٔ دائمی کاربر تعریف کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0032 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0034",
      "title": "پیام‌های `CancelRequest`، `HostBusy`، `UnsupportedModel` و `ProtocolError` را تعریف کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0033",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام‌های `CancelRequest`، `HostBusy`، `UnsupportedModel` و `ProtocolError` را تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام‌های `CancelRequest`، `HostBusy`، `UnsupportedModel` و `ProtocolError` را تعریف کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0033 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0035",
      "title": "شمارهٔ نسخهٔ پروتکل و سیاست رد نسخه‌های ناسازگار را در قراردادها پیاده‌سازی کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0034",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: شمارهٔ نسخهٔ پروتکل و سیاست رد نسخه‌های ناسازگار را در قراردادها پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «شمارهٔ نسخهٔ پروتکل و سیاست رد نسخه‌های ناسازگار را در قراردادها پیاده‌سازی کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0034 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0036",
      "title": "کد تولید قراردادهای C# و Kotlin را در build تولید کن، نه با ویرایش دستی.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0035",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: کد تولید قراردادهای C# و Kotlin را در build تولید کن، نه با ویرایش دستی."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «کد تولید قراردادهای C# و Kotlin را در build تولید کن، نه با ویرایش دستی.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0035 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0037",
      "title": "آزمون round-trip برای serialize/deserialize همهٔ پیام‌های اصلی در هر دو زبان بنویس.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0036",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون round-trip برای serialize/deserialize همهٔ پیام‌های اصلی در هر دو زبان بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون round-trip برای serialize/deserialize همهٔ پیام‌های اصلی در هر دو زبان بنویس.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0036 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0038",
      "title": "آزمون سازگاری قرارداد C# و Kotlin را با داده‌های fixture مشترک اجرا کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0037",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون سازگاری قرارداد C# و Kotlin را با داده‌های fixture مشترک اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون سازگاری قرارداد C# و Kotlin را با داده‌های fixture مشترک اجرا کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0037 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0039",
      "title": "محدودیت اندازهٔ پیام، capsule و پاسخ را در قرارداد و اعتبارسنجی اجرایی اعمال کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0038",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: محدودیت اندازهٔ پیام، capsule و پاسخ را در قرارداد و اعتبارسنجی اجرایی اعمال کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «محدودیت اندازهٔ پیام، capsule و پاسخ را در قرارداد و اعتبارسنجی اجرایی اعمال کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0038 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0040",
      "title": "آزمون رد پیام ناقص، بزرگ، نسخه‌ناسازگار یا دارای مقدار نامعتبر را اضافه کن.",
      "section": "3. قراردادهای مشترک شبکه و داده",
      "depends_on": [
        "T0039",
        "T0030"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون رد پیام ناقص، بزرگ، نسخه‌ناسازگار یا دارای مقدار نامعتبر را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون رد پیام ناقص، بزرگ، نسخه‌ناسازگار یا دارای مقدار نامعتبر را اضافه کن.» در مسیر src/Shared.Contracts/ + tests/contracts/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0039 و T0030 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Contracts/ + tests/contracts/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0041",
      "title": "نقطهٔ ورود واقعی برنامهٔ WPF را بساز و پنجرهٔ اصلی را بدون خطا اجرا کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نقطهٔ ورود واقعی برنامهٔ WPF را بساز و پنجرهٔ اصلی را بدون خطا اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نقطهٔ ورود واقعی برنامهٔ WPF را بساز و پنجرهٔ اصلی را بدون خطا اجرا کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0042",
      "title": "لایه‌های UI، Application، Domain و Infrastructure را به پروژه‌های buildable جدا کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0041",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: لایه‌های UI، Application، Domain و Infrastructure را به پروژه‌های buildable جدا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «لایه‌های UI، Application، Domain و Infrastructure را به پروژه‌های buildable جدا کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0041 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0043",
      "title": "مدل دامنهٔ `HostState` را با وضعیت‌های Stopped، Starting، Ready، Busy و Error پیاده‌سازی کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0042",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مدل دامنهٔ `HostState` را با وضعیت‌های Stopped، Starting، Ready، Busy و Error پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مدل دامنهٔ `HostState` را با وضعیت‌های Stopped، Starting، Ready، Busy و Error پیاده‌سازی کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0042 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0044",
      "title": "تغییر وضعیت میزبان را با state machine محدود و آزمون واحد پیاده‌سازی کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0043",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تغییر وضعیت میزبان را با state machine محدود و آزمون واحد پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تغییر وضعیت میزبان را با state machine محدود و آزمون واحد پیاده‌سازی کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0043 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0045",
      "title": "سرویس تنظیمات محلی را با ذخیره‌سازی نسخه‌دار و اعتبارسنجی ایجاد کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0044",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سرویس تنظیمات محلی را با ذخیره‌سازی نسخه‌دار و اعتبارسنجی ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سرویس تنظیمات محلی را با ذخیره‌سازی نسخه‌دار و اعتبارسنجی ایجاد کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0044 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0046",
      "title": "رابط وضعیت اصلی را به وضعیت واقعی backend و شبکه وصل کن؛ مقدار نمایشی ثابت ممنوع است.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0045",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: رابط وضعیت اصلی را به وضعیت واقعی backend و شبکه وصل کن؛ مقدار نمایشی ثابت ممنوع است."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «رابط وضعیت اصلی را به وضعیت واقعی backend و شبکه وصل کن؛ مقدار نمایشی ثابت ممنوع است.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0045 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0047",
      "title": "پیام‌های خطا را به خطای قابل‌فهم کاربر نگاشت کن، بدون افشای مسیرهای محرمانه یا اسرار.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0046",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام‌های خطا را به خطای قابل‌فهم کاربر نگاشت کن، بدون افشای مسیرهای محرمانه یا اسرار."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام‌های خطا را به خطای قابل‌فهم کاربر نگاشت کن، بدون افشای مسیرهای محرمانه یا اسرار.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0046 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0048",
      "title": "آزمون راه‌اندازی، خاموش‌کردن و بازیابی از خطای راه‌اندازی میزبان را اضافه کن.",
      "section": "4. اسکلت قابل‌اجرای میزبان ویندوز",
      "depends_on": [
        "T0047",
        "T0040"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون راه‌اندازی، خاموش‌کردن و بازیابی از خطای راه‌اندازی میزبان را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون راه‌اندازی، خاموش‌کردن و بازیابی از خطای راه‌اندازی میزبان را اضافه کن.» در مسیر src/Host.Windows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0047 و T0040 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0049",
      "title": "سرویس تشخیص سیستم‌عامل، RAM و فضای دیسک را با API واقعی ویندوز پیاده‌سازی کن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سرویس تشخیص سیستم‌عامل، RAM و فضای دیسک را با API واقعی ویندوز پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سرویس تشخیص سیستم‌عامل، RAM و فضای دیسک را با API واقعی ویندوز پیاده‌سازی کن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0050",
      "title": "تشخیص GPU و backendهای موجود را با قابلیت‌های واقعی قابل‌دسترسی پیاده‌سازی کن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0049",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تشخیص GPU و backendهای موجود را با قابلیت‌های واقعی قابل‌دسترسی پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تشخیص GPU و backendهای موجود را با قابلیت‌های واقعی قابل‌دسترسی پیاده‌سازی کن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0049 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0051",
      "title": "سرویس تخمین سازگاری مدل را با نیاز حافظهٔ اعلام‌شدهٔ مدل و منابع میزبان پیاده‌سازی کن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0050",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سرویس تخمین سازگاری مدل را با نیاز حافظهٔ اعلام‌شدهٔ مدل و منابع میزبان پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سرویس تخمین سازگاری مدل را با نیاز حافظهٔ اعلام‌شدهٔ مدل و منابع میزبان پیاده‌سازی کن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0050 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0052",
      "title": "وضعیت ظرفیت را به مدل دامنهٔ `HostCapabilities` تبدیل کن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0051",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وضعیت ظرفیت را به مدل دامنهٔ `HostCapabilities` تبدیل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وضعیت ظرفیت را به مدل دامنهٔ `HostCapabilities` تبدیل کن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0051 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0053",
      "title": "آزمون کمبود RAM، فضای دیسک ناکافی و backend پشتیبانی‌نشده را بنویس.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0052",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون کمبود RAM، فضای دیسک ناکافی و backend پشتیبانی‌نشده را بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون کمبود RAM، فضای دیسک ناکافی و backend پشتیبانی‌نشده را بنویس.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0052 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0054",
      "title": "میزبان را هنگام آماده‌نبودن مدل یا ناکافی‌بودن منابع از حالت Ready خارج کن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0053",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: میزبان را هنگام آماده‌نبودن مدل یا ناکافی‌بودن منابع از حالت Ready خارج کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «میزبان را هنگام آماده‌نبودن مدل یا ناکافی‌بودن منابع از حالت Ready خارج کن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0053 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0055",
      "title": "اطلاعات CPU، GPU و حافظه را در رابط میزبان نمایش بده و مقادیر را از سیستم واقعی بخوان.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0054",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اطلاعات CPU، GPU و حافظه را در رابط میزبان نمایش بده و مقادیر را از سیستم واقعی بخوان."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اطلاعات CPU، GPU و حافظه را در رابط میزبان نمایش بده و مقادیر را از سیستم واقعی بخوان.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0054 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0056",
      "title": "در صورت نامشخص‌بودن ظرفیت، آن را Unknown نشان بده و به‌اشتباه Ready اعلام نکن.",
      "section": "5. تشخیص سخت‌افزار و ظرفیت",
      "depends_on": [
        "T0055",
        "T0048"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: در صورت نامشخص‌بودن ظرفیت، آن را Unknown نشان بده و به‌اشتباه Ready اعلام نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «در صورت نامشخص‌بودن ظرفیت، آن را Unknown نشان بده و به‌اشتباه Ready اعلام نکن.» در مسیر src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0055 و T0048 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ (سرویس تشخیص سخت‌افزار و UI)"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0057",
      "title": "رابط `IInferenceEngine` برای کشف backend، بارگذاری مدل، تولید پاسخ، لغو و آزادسازی منابع ایجاد کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: رابط `IInferenceEngine` برای کشف backend، بارگذاری مدل، تولید پاسخ، لغو و آزادسازی منابع ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «رابط `IInferenceEngine` برای کشف backend، بارگذاری مدل، تولید پاسخ، لغو و آزادسازی منابع ایجاد کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0058",
      "title": "پیاده‌سازی واقعی `LlamaCppInferenceEngine` را برای اتصال به موتور محلی نصب‌شده ایجاد کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0057",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیاده‌سازی واقعی `LlamaCppInferenceEngine` را برای اتصال به موتور محلی نصب‌شده ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیاده‌سازی واقعی `LlamaCppInferenceEngine` را برای اتصال به موتور محلی نصب‌شده ایجاد کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0057 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0059",
      "title": "فرایند موتور محلی را با آرگومان‌های امن و بدون shell interpolation اجرا کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0058",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فرایند موتور محلی را با آرگومان‌های امن و بدون shell interpolation اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «فرایند موتور محلی را با آرگومان‌های امن و بدون shell interpolation اجرا کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0058 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0060",
      "title": "health check واقعی backend را پیش از اعلام آمادگی مدل اجرا کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0059",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: health check واقعی backend را پیش از اعلام آمادگی مدل اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «health check واقعی backend را پیش از اعلام آمادگی مدل اجرا کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0059 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0061",
      "title": "درخواست inference را به API محلی backend نگاشت و پاسخ واقعی مدل را دریافت کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0060",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: درخواست inference را به API محلی backend نگاشت و پاسخ واقعی مدل را دریافت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «درخواست inference را به API محلی backend نگاشت و پاسخ واقعی مدل را دریافت کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0060 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0062",
      "title": "timeout، خطای فرایند، قطع ارتباط و پاسخ ناقص backend را مدیریت کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0061",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: timeout، خطای فرایند، قطع ارتباط و پاسخ ناقص backend را مدیریت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «timeout، خطای فرایند، قطع ارتباط و پاسخ ناقص backend را مدیریت کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0061 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0063",
      "title": "لغو inference را به backend منتقل کن و در صورت عدم پشتیبانی، فرایند اختصاصی را به‌صورت کنترل‌شده متوقف کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0062",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: لغو inference را به backend منتقل کن و در صورت عدم پشتیبانی، فرایند اختصاصی را به‌صورت کنترل‌شده متوقف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «لغو inference را به backend منتقل کن و در صورت عدم پشتیبانی، فرایند اختصاصی را به‌صورت کنترل‌شده متوقف کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0062 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0064",
      "title": "آزمون یکپارچه با مدل کوچک دارای مجوز قابل‌قبول را به‌صورت opt-in بساز.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0063",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون یکپارچه با مدل کوچک دارای مجوز قابل‌قبول را به‌صورت opt-in بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون یکپارچه با مدل کوچک دارای مجوز قابل‌قبول را به‌صورت opt-in بساز.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0063 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0065",
      "title": "آزمون اثبات کند در مسیر inference هیچ اتصال API به ارائه‌دهندهٔ ابری برقرار نمی‌شود.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0064",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون اثبات کند در مسیر inference هیچ اتصال API به ارائه‌دهندهٔ ابری برقرار نمی‌شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون اثبات کند در مسیر inference هیچ اتصال API به ارائه‌دهندهٔ ابری برقرار نمی‌شود.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0064 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0066",
      "title": "نسخه و checksum باینری موتور را پیش از اجرا بررسی کن.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0065",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نسخه و checksum باینری موتور را پیش از اجرا بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نسخه و checksum باینری موتور را پیش از اجرا بررسی کن.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0065 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0067",
      "title": "backend را پشت رابط نگه دار تا افزودن موتور جایگزین نیازمند تغییر منطق گفت‌وگو یا شبکه نباشد.",
      "section": "6. موتور استنتاج محلی واقعی",
      "depends_on": [
        "T0066",
        "T0056"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: backend را پشت رابط نگه دار تا افزودن موتور جایگزین نیازمند تغییر منطق گفت‌وگو یا شبکه نباشد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «backend را پشت رابط نگه دار تا افزودن موتور جایگزین نیازمند تغییر منطق گفت‌وگو یا شبکه نباشد.» در مسیر src/Host.Windows/Inference/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0066 و T0056 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Inference/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0068",
      "title": "schema نسخه‌دار کاتالوگ مدل شامل شناسه، نام، فرمت، اندازه، checksum، backend و مجوز ایجاد کن.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: schema نسخه‌دار کاتالوگ مدل شامل شناسه، نام، فرمت، اندازه، checksum، backend و مجوز ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «schema نسخه‌دار کاتالوگ مدل شامل شناسه، نام، فرمت، اندازه، checksum، backend و مجوز ایجاد کن.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0069",
      "title": "parser کاتالوگ را با رد ورودی نامعتبر و URL غیرمجاز پیاده‌سازی کن.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0068",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: parser کاتالوگ را با رد ورودی نامعتبر و URL غیرمجاز پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «parser کاتالوگ را با رد ورودی نامعتبر و URL غیرمجاز پیاده‌سازی کن.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0068 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0070",
      "title": "صفحهٔ کاتالوگ را از دادهٔ واقعی کاتالوگ بخوان، نه از فهرست هاردکدشدهٔ UI.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0069",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: صفحهٔ کاتالوگ را از دادهٔ واقعی کاتالوگ بخوان، نه از فهرست هاردکدشدهٔ UI."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «صفحهٔ کاتالوگ را از دادهٔ واقعی کاتالوگ بخوان، نه از فهرست هاردکدشدهٔ UI.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0069 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0071",
      "title": "برای هر مدل، صفحهٔ اطلاعات شامل مجوز، منبع، اندازه و محدودیت استفاده بساز.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0070",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای هر مدل، صفحهٔ اطلاعات شامل مجوز، منبع، اندازه و محدودیت استفاده بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای هر مدل، صفحهٔ اطلاعات شامل مجوز، منبع، اندازه و محدودیت استفاده بساز.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0070 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0072",
      "title": "مدل فاقد اطلاعات مجوز معتبر را به‌عنوان «نیازمند بررسی» علامت بزن.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0071",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مدل فاقد اطلاعات مجوز معتبر را به‌عنوان «نیازمند بررسی» علامت بزن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مدل فاقد اطلاعات مجوز معتبر را به‌عنوان «نیازمند بررسی» علامت بزن.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0071 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0073",
      "title": "آزمون کاتالوگ شامل مدل مجاز، مدل فاقد مجوز، checksum نامعتبر و URL ناامن را بنویس.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0072",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون کاتالوگ شامل مدل مجاز، مدل فاقد مجوز، checksum نامعتبر و URL ناامن را بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون کاتالوگ شامل مدل مجاز، مدل فاقد مجوز، checksum نامعتبر و URL ناامن را بنویس.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0072 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0074",
      "title": "یک کاتالوگ نمونهٔ واقعی با مدل‌های دارای منابع و مجوزهای بررسی‌شده اضافه کن.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0073",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: یک کاتالوگ نمونهٔ واقعی با مدل‌های دارای منابع و مجوزهای بررسی‌شده اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «یک کاتالوگ نمونهٔ واقعی با مدل‌های دارای منابع و مجوزهای بررسی‌شده اضافه کن.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0073 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0075",
      "title": "نبود دسترسی اینترنت را در کاتالوگ مدیریت کن و آخرین دادهٔ معتبر را فقط در صورت وجود نمایش بده.",
      "section": "7. کاتالوگ، مجوز و فایل‌های مدل",
      "depends_on": [
        "T0074",
        "T0067"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نبود دسترسی اینترنت را در کاتالوگ مدیریت کن و آخرین دادهٔ معتبر را فقط در صورت وجود نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نبود دسترسی اینترنت را در کاتالوگ مدیریت کن و آخرین دادهٔ معتبر را فقط در صورت وجود نمایش بده.» در مسیر src/Host.Windows/Models/Catalog/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0074 و T0067 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/Catalog/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0076",
      "title": "دانلودکنندهٔ streaming بساز که فایل را به‌صورت تکه‌ای دریافت کند و کل مدل را در RAM نگه ندارد.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دانلودکنندهٔ streaming بساز که فایل را به‌صورت تکه‌ای دریافت کند و کل مدل را در RAM نگه ندارد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دانلودکنندهٔ streaming بساز که فایل را به‌صورت تکه‌ای دریافت کند و کل مدل را در RAM نگه ندارد.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0077",
      "title": "دانلود را به مسیر موقت محدود کن و پس از تکمیل checksum را بررسی کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0076",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دانلود را به مسیر موقت محدود کن و پس از تکمیل checksum را بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دانلود را به مسیر موقت محدود کن و پس از تکمیل checksum را بررسی کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0076 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0078",
      "title": "دانلود ناقص را با حفظ وضعیت امن ادامه بده یا فایل ناقص را پاک کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0077",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دانلود ناقص را با حفظ وضعیت امن ادامه بده یا فایل ناقص را پاک کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دانلود ناقص را با حفظ وضعیت امن ادامه بده یا فایل ناقص را پاک کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0077 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0079",
      "title": "پیشرفت دانلود و سرعت تقریبی را از بایت‌های واقعی گزارش کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0078",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیشرفت دانلود و سرعت تقریبی را از بایت‌های واقعی گزارش کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیشرفت دانلود و سرعت تقریبی را از بایت‌های واقعی گزارش کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0078 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0080",
      "title": "لغو دانلود را پیاده‌سازی کن و پس از لغو، فایل نهایی ناقص باقی نماند.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0079",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: لغو دانلود را پیاده‌سازی کن و پس از لغو، فایل نهایی ناقص باقی نماند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «لغو دانلود را پیاده‌سازی کن و پس از لغو، فایل نهایی ناقص باقی نماند.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0079 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0081",
      "title": "نصب مدل را فقط پس از موفقیت checksum و اعتبارسنجی فرمت نهایی کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0080",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نصب مدل را فقط پس از موفقیت checksum و اعتبارسنجی فرمت نهایی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نصب مدل را فقط پس از موفقیت checksum و اعتبارسنجی فرمت نهایی کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0080 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0082",
      "title": "انتخاب مدل فعال را در تنظیمات پایدار ذخیره کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0081",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: انتخاب مدل فعال را در تنظیمات پایدار ذخیره کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «انتخاب مدل فعال را در تنظیمات پایدار ذخیره کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0081 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0083",
      "title": "تغییر مدل فعال را در حالی که inference در حال اجراست رد یا تا پایان امن آن به تعویق بینداز.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0082",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تغییر مدل فعال را در حالی که inference در حال اجراست رد یا تا پایان امن آن به تعویق بینداز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تغییر مدل فعال را در حالی که inference در حال اجراست رد یا تا پایان امن آن به تعویق بینداز.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0082 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0084",
      "title": "حذف مدل فعال را با درخواست تأیید و آزادسازی backend پیاده‌سازی کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0083",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: حذف مدل فعال را با درخواست تأیید و آزادسازی backend پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «حذف مدل فعال را با درخواست تأیید و آزادسازی backend پیاده‌سازی کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0083 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0085",
      "title": "به‌روزرسانی مدل را با دریافت نسخهٔ جدید و جایگزینی اتمی انجام بده.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0084",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: به‌روزرسانی مدل را با دریافت نسخهٔ جدید و جایگزینی اتمی انجام بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «به‌روزرسانی مدل را با دریافت نسخهٔ جدید و جایگزینی اتمی انجام بده.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0084 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0086",
      "title": "آزمون فضای ناکافی، خطای شبکه، checksum نامعتبر، لغو و حذف مدل را اضافه کن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0085",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون فضای ناکافی، خطای شبکه، checksum نامعتبر، لغو و حذف مدل را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون فضای ناکافی، خطای شبکه، checksum نامعتبر، لغو و حذف مدل را اضافه کن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0085 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0087",
      "title": "هیچ مدل یا وزن دارای محدودیت مجوز را بدون اطلاع روشن به‌عنوان متن‌باز معرفی نکن.",
      "section": "8. دانلود، نصب، انتخاب و حذف مدل",
      "depends_on": [
        "T0086",
        "T0075"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هیچ مدل یا وزن دارای محدودیت مجوز را بدون اطلاع روشن به‌عنوان متن‌باز معرفی نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هیچ مدل یا وزن دارای محدودیت مجوز را بدون اطلاع روشن به‌عنوان متن‌باز معرفی نکن.» در مسیر src/Host.Windows/Models/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0086 و T0075 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Models/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0088",
      "title": "صفحهٔ گفت‌وگوی محلی با فهرست پیام‌ها، ورودی متن و دکمهٔ ارسال بساز.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: صفحهٔ گفت‌وگوی محلی با فهرست پیام‌ها، ورودی متن و دکمهٔ ارسال بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «صفحهٔ گفت‌وگوی محلی با فهرست پیام‌ها، ورودی متن و دکمهٔ ارسال بساز.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0089",
      "title": "ارسال پیام را به موتور استنتاج واقعی متصل کن و وضعیت تولید را نشان بده.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0088",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ارسال پیام را به موتور استنتاج واقعی متصل کن و وضعیت تولید را نشان بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ارسال پیام را به موتور استنتاج واقعی متصل کن و وضعیت تولید را نشان بده.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0088 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0090",
      "title": "پیام‌های کاربر و مدل را با ترتیب قطعی در حافظهٔ نشست نمایش بده.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0089",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام‌های کاربر و مدل را با ترتیب قطعی در حافظهٔ نشست نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام‌های کاربر و مدل را با ترتیب قطعی در حافظهٔ نشست نمایش بده.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0089 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0091",
      "title": "توقف تولید و پاک‌کردن نشست را پیاده‌سازی کن.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0090",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: توقف تولید و پاک‌کردن نشست را پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «توقف تولید و پاک‌کردن نشست را پیاده‌سازی کن.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0090 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0092",
      "title": "خطاهای مدل را در UI نمایش بده و آن‌ها را به متن پاسخ موفق تبدیل نکن.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0091",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: خطاهای مدل را در UI نمایش بده و آن‌ها را به متن پاسخ موفق تبدیل نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «خطاهای مدل را در UI نمایش بده و آن‌ها را به متن پاسخ موفق تبدیل نکن.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0091 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0093",
      "title": "رابط ابزارهای عامل را فقط برای ابزارهای allowlist شده تعریف کن.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0092",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: رابط ابزارهای عامل را فقط برای ابزارهای allowlist شده تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «رابط ابزارهای عامل را فقط برای ابزارهای allowlist شده تعریف کن.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0092 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0094",
      "title": "اجرای ابزار را با تأیید کاربر برای عملیات دارای اثر جانبی پیاده‌سازی کن.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0093",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اجرای ابزار را با تأیید کاربر برای عملیات دارای اثر جانبی پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اجرای ابزار را با تأیید کاربر برای عملیات دارای اثر جانبی پیاده‌سازی کن.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0093 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0095",
      "title": "ابزارها را در فرایند محدود و با timeout اجرا کن؛ اجرای فرمان دلخواه سیستم به‌طور پیش‌فرض ممنوع باشد.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0094",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ابزارها را در فرایند محدود و با timeout اجرا کن؛ اجرای فرمان دلخواه سیستم به‌طور پیش‌فرض ممنوع باشد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ابزارها را در فرایند محدود و با timeout اجرا کن؛ اجرای فرمان دلخواه سیستم به‌طور پیش‌فرض ممنوع باشد.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0094 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0096",
      "title": "آزمون رد فراخوانی ابزار ناشناخته، ورودی نامعتبر و عملیات نیازمند تأیید را بنویس.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0095",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون رد فراخوانی ابزار ناشناخته، ورودی نامعتبر و عملیات نیازمند تأیید را بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون رد فراخوانی ابزار ناشناخته، ورودی نامعتبر و عملیات نیازمند تأیید را بنویس.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0095 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0097",
      "title": "قابلیت‌های Agent را تنها در صورت پشتیبانی واقعی مدل/backend فعال کن.",
      "section": "9. گفت‌وگوی مستقیم در میزبان و ابزارهای عامل",
      "depends_on": [
        "T0096",
        "T0087"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: قابلیت‌های Agent را تنها در صورت پشتیبانی واقعی مدل/backend فعال کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «قابلیت‌های Agent را تنها در صورت پشتیبانی واقعی مدل/backend فعال کن.» در مسیر src/Host.Windows/Chat/ و src/Host.Windows/Tools/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0096 و T0087 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Chat/ و src/Host.Windows/Tools/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0098",
      "title": "`HostRequestCoordinator` را با semaphore تک‌ظرفیتی پیاده‌سازی کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: `HostRequestCoordinator` را با semaphore تک‌ظرفیتی پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «`HostRequestCoordinator` را با semaphore تک‌ظرفیتی پیاده‌سازی کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0099",
      "title": "درخواست دوم را هنگام مشغول‌بودن با وضعیت Busy رد کن و آن را در صف نامحدود پنهان نکن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0098",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: درخواست دوم را هنگام مشغول‌بودن با وضعیت Busy رد کن و آن را در صف نامحدود پنهان نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «درخواست دوم را هنگام مشغول‌بودن با وضعیت Busy رد کن و آن را در صف نامحدود پنهان نکن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0098 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0100",
      "title": "پیش از پذیرش درخواست، مشارکت فعال، مدل آماده، ظرفیت و deadline را بررسی کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0099",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیش از پذیرش درخواست، مشارکت فعال، مدل آماده، ظرفیت و deadline را بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیش از پذیرش درخواست، مشارکت فعال، مدل آماده، ظرفیت و deadline را بررسی کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0099 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0101",
      "title": "برای هر درخواست شناسهٔ تصادفی رمزنگارانه با مولد امن ایجاد کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0100",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای هر درخواست شناسهٔ تصادفی رمزنگارانه با مولد امن ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای هر درخواست شناسهٔ تصادفی رمزنگارانه با مولد امن ایجاد کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0100 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0102",
      "title": "deadline را هنگام دریافت ثبت و پیش از inference و هنگام ارسال پاسخ دوباره بررسی کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0101",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: deadline را هنگام دریافت ثبت و پیش از inference و هنگام ارسال پاسخ دوباره بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «deadline را هنگام دریافت ثبت و پیش از inference و هنگام ارسال پاسخ دوباره بررسی کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0101 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0103",
      "title": "لغو درخواست را به coordinator، موتور inference و ارتباط شبکه منتقل کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0102",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: لغو درخواست را به coordinator، موتور inference و ارتباط شبکه منتقل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «لغو درخواست را به coordinator، موتور inference و ارتباط شبکه منتقل کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0102 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0104",
      "title": "نتیجهٔ هر درخواست را حداکثر یک‌بار به مسیر پاسخ‌دهی تحویل بده.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0103",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نتیجهٔ هر درخواست را حداکثر یک‌بار به مسیر پاسخ‌دهی تحویل بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نتیجهٔ هر درخواست را حداکثر یک‌بار به مسیر پاسخ‌دهی تحویل بده.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0103 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0105",
      "title": "اطلاعات موقت درخواست را در finally پاک کن، حتی اگر inference با exception تمام شود.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0104",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اطلاعات موقت درخواست را در finally پاک کن، حتی اگر inference با exception تمام شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اطلاعات موقت درخواست را در finally پاک کن، حتی اگر inference با exception تمام شود.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0104 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0106",
      "title": "آزمون هم‌زمانی اثبات کند هیچ دو inference ورودی هم‌زمان روی یک میزبان اجرا نمی‌شوند.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0105",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون هم‌زمانی اثبات کند هیچ دو inference ورودی هم‌زمان روی یک میزبان اجرا نمی‌شوند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون هم‌زمانی اثبات کند هیچ دو inference ورودی هم‌زمان روی یک میزبان اجرا نمی‌شوند.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0105 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0107",
      "title": "آزمون درخواست تکراری، deadline منقضی، لغو حین اجرا و خطای موتور را اضافه کن.",
      "section": "10. صف درخواست و محدودیت یک پردازش هم‌زمان",
      "depends_on": [
        "T0106",
        "T0097"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون درخواست تکراری، deadline منقضی، لغو حین اجرا و خطای موتور را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون درخواست تکراری، deadline منقضی، لغو حین اجرا و خطای موتور را اضافه کن.» در مسیر src/Host.Windows/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0106 و T0097 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0108",
      "title": "listener شبکه را فقط وقتی مشارکت فعال است راه‌اندازی کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: listener شبکه را فقط وقتی مشارکت فعال است راه‌اندازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «listener شبکه را فقط وقتی مشارکت فعال است راه‌اندازی کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0109",
      "title": "listener را هنگام غیرفعال‌کردن مشارکت یا خروج برنامه به‌صورت کامل متوقف کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0108",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: listener را هنگام غیرفعال‌کردن مشارکت یا خروج برنامه به‌صورت کامل متوقف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «listener را هنگام غیرفعال‌کردن مشارکت یا خروج برنامه به‌صورت کامل متوقف کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0108 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0110",
      "title": "parser ورودی شبکه را به قرارداد Protobuf مشترک وصل کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0109",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: parser ورودی شبکه را به قرارداد Protobuf مشترک وصل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «parser ورودی شبکه را به قرارداد Protobuf مشترک وصل کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0109 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0111",
      "title": "اندازهٔ پیام، نسخهٔ پروتکل، شناسه، deadline و مجوز ارسال درخواست را اعتبارسنجی کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0110",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اندازهٔ پیام، نسخهٔ پروتکل، شناسه، deadline و مجوز ارسال درخواست را اعتبارسنجی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اندازهٔ پیام، نسخهٔ پروتکل، شناسه، deadline و مجوز ارسال درخواست را اعتبارسنجی کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0110 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0112",
      "title": "پاسخ‌های Busy، UnsupportedModel، InvalidRequest و InternalError را به وضعیت‌های قرارداد نگاشت کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0111",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاسخ‌های Busy، UnsupportedModel، InvalidRequest و InternalError را به وضعیت‌های قرارداد نگاشت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاسخ‌های Busy، UnsupportedModel، InvalidRequest و InternalError را به وضعیت‌های قرارداد نگاشت کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0111 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0113",
      "title": "متن درخواست و پاسخ را در log عادی ثبت نکن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0112",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: متن درخواست و پاسخ را در log عادی ثبت نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «متن درخواست و پاسخ را در log عادی ثبت نکن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0112 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0114",
      "title": "listener را از دسترسی عمومی غیرضروری شبکه محدود و پورت/وضعیت اتصال را به کاربر نمایش بده.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0113",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: listener را از دسترسی عمومی غیرضروری شبکه محدود و پورت/وضعیت اتصال را به کاربر نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «listener را از دسترسی عمومی غیرضروری شبکه محدود و پورت/وضعیت اتصال را به کاربر نمایش بده.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0113 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0115",
      "title": "آزمون listener با اتصال قطع‌شده، پیام مخرب، اتصال تکراری و shutdown را اضافه کن.",
      "section": "11. سرویس شبکه روی میزبان ویندوز",
      "depends_on": [
        "T0114",
        "T0107"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون listener با اتصال قطع‌شده، پیام مخرب، اتصال تکراری و shutdown را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون listener با اتصال قطع‌شده، پیام مخرب، اتصال تکراری و shutdown را اضافه کن.» در مسیر src/Host.Windows/Networking/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0114 و T0107 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/Networking/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0116",
      "title": "رابط `IPeerTransport` برای اتصال امن، ارسال پیام، دریافت پیام و قطع ارتباط ایجاد کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: رابط `IPeerTransport` برای اتصال امن، ارسال پیام، دریافت پیام و قطع ارتباط ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «رابط `IPeerTransport` برای اتصال امن، ارسال پیام، دریافت پیام و قطع ارتباط ایجاد کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0117",
      "title": "پیاده‌سازی انتقال P2P را با کتابخانهٔ متن‌باز انتخاب‌شده و مجوز سازگار به کد واقعی وصل کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0116",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیاده‌سازی انتقال P2P را با کتابخانهٔ متن‌باز انتخاب‌شده و مجوز سازگار به کد واقعی وصل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیاده‌سازی انتقال P2P را با کتابخانهٔ متن‌باز انتخاب‌شده و مجوز سازگار به کد واقعی وصل کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0116 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0118",
      "title": "برای شبکه‌هایی که اتصال مستقیم در آن‌ها ممکن نیست، روش اتصال جایگزین را بدون عبور متن درخواست از سرور معرفی‌کننده پیاده‌سازی کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0117",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای شبکه‌هایی که اتصال مستقیم در آن‌ها ممکن نیست، روش اتصال جایگزین را بدون عبور متن درخواست از سرور معرفی‌کننده پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای شبکه‌هایی که اتصال مستقیم در آن‌ها ممکن نیست، روش اتصال جایگزین را بدون عبور متن درخواست از سرور معرفی‌کننده پیاده‌سازی کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0117 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0119",
      "title": "اگر relay انتقال رمزگذاری‌شده لازم است، آن را به‌عنوان مسیر عبور بایت‌ها طراحی کن و صریحاً ثبت کن که relay می‌تواند فرادادهٔ اتصال را ببیند.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0118",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اگر relay انتقال رمزگذاری‌شده لازم است، آن را به‌عنوان مسیر عبور بایت‌ها طراحی کن و صریحاً ثبت کن که relay می‌تواند فرادادهٔ اتصال را ببیند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اگر relay انتقال رمزگذاری‌شده لازم است، آن را به‌عنوان مسیر عبور بایت‌ها طراحی کن و صریحاً ثبت کن که relay می‌تواند فرادادهٔ اتصال را ببیند.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0118 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0120",
      "title": "سرویس معرفی گره را به پیام‌های announcement و capability محدود کن و ارسال محتوای درخواست به آن را در کد ممنوع کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0119",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سرویس معرفی گره را به پیام‌های announcement و capability محدود کن و ارسال محتوای درخواست به آن را در کد ممنوع کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سرویس معرفی گره را به پیام‌های announcement و capability محدود کن و ارسال محتوای درخواست به آن را در کد ممنوع کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0119 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0121",
      "title": "اعلام حضور میزبان را با TTL و تمدید دوره‌ای پیاده‌سازی کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0120",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اعلام حضور میزبان را با TTL و تمدید دوره‌ای پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اعلام حضور میزبان را با TTL و تمدید دوره‌ای پیاده‌سازی کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0120 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0122",
      "title": "میزبان منقضی یا قطع‌شده را از فهرست قابل‌انتخاب حذف کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0121",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: میزبان منقضی یا قطع‌شده را از فهرست قابل‌انتخاب حذف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «میزبان منقضی یا قطع‌شده را از فهرست قابل‌انتخاب حذف کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0121 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0123",
      "title": "وضعیت آنلاین را از heartbeat واقعی تعیین کن، نه از زمان آخرین دادهٔ کش‌شده بدون اعتبارسنجی.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0122",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وضعیت آنلاین را از heartbeat واقعی تعیین کن، نه از زمان آخرین دادهٔ کش‌شده بدون اعتبارسنجی."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وضعیت آنلاین را از heartbeat واقعی تعیین کن، نه از زمان آخرین دادهٔ کش‌شده بدون اعتبارسنجی.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0122 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0124",
      "title": "انتخاب گره‌ها را بر اساس reachability، readiness، ظرفیت و سازگاری مدل پیاده‌سازی کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0123",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: انتخاب گره‌ها را بر اساس reachability، readiness، ظرفیت و سازگاری مدل پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «انتخاب گره‌ها را بر اساس reachability، readiness، ظرفیت و سازگاری مدل پیاده‌سازی کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0123 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0125",
      "title": "انتخاب سه میزبان را با سه هویت گرهٔ متمایز تضمین کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0124",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: انتخاب سه میزبان را با سه هویت گرهٔ متمایز تضمین کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «انتخاب سه میزبان را با سه هویت گرهٔ متمایز تضمین کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0124 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0126",
      "title": "وقتی کمتر از سه میزبان معتبر وجود دارد، رفتار تعریف‌شده را اجرا و کمبود را به کاربر اعلام کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0125",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وقتی کمتر از سه میزبان معتبر وجود دارد، رفتار تعریف‌شده را اجرا و کمبود را به کاربر اعلام کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وقتی کمتر از سه میزبان معتبر وجود دارد، رفتار تعریف‌شده را اجرا و کمبود را به کاربر اعلام کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0125 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0127",
      "title": "آزمون قطع شبکه، انقضای announcement، میزبان تکراری و نبود میزبان را اضافه کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0126",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون قطع شبکه، انقضای announcement، میزبان تکراری و نبود میزبان را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون قطع شبکه، انقضای announcement، میزبان تکراری و نبود میزبان را اضافه کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0126 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0128",
      "title": "آزمون end-to-end را روی دو رایانهٔ واقعی یا دو گرهٔ مستقل شبکه اجرا و نتیجه را ثبت کن.",
      "section": "12. معماری P2P و کشف میزبان",
      "depends_on": [
        "T0127",
        "T0115"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون end-to-end را روی دو رایانهٔ واقعی یا دو گرهٔ مستقل شبکه اجرا و نتیجه را ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون end-to-end را روی دو رایانهٔ واقعی یا دو گرهٔ مستقل شبکه اجرا و نتیجه را ثبت کن.» در مسیر src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0127 و T0115 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Networking/ یا src/Host.Windows/Networking/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0129",
      "title": "از کتابخانهٔ رمزنگاری معتبر و نگهداری‌شده استفاده کن؛ الگوریتم رمزنگاری اختصاصی نساز.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: از کتابخانهٔ رمزنگاری معتبر و نگهداری‌شده استفاده کن؛ الگوریتم رمزنگاری اختصاصی نساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «از کتابخانهٔ رمزنگاری معتبر و نگهداری‌شده استفاده کن؛ الگوریتم رمزنگاری اختصاصی نساز.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0130",
      "title": "هویت کلید میزبان را بساز و کلید خصوصی را در ذخیره‌سازی امن سیستم نگه دار.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0129",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هویت کلید میزبان را بساز و کلید خصوصی را در ذخیره‌سازی امن سیستم نگه دار."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هویت کلید میزبان را بساز و کلید خصوصی را در ذخیره‌سازی امن سیستم نگه دار.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0129 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0131",
      "title": "روش اعتماد اولیه و تأیید هویت همتا را پیاده‌سازی کن.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0130",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: روش اعتماد اولیه و تأیید هویت همتا را پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «روش اعتماد اولیه و تأیید هویت همتا را پیاده‌سازی کن.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0130 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0132",
      "title": "محرمانگی، تمامیت و احراز هویت ارتباط را با TLS یا پروتکل امن معتبر برقرار کن.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0131",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: محرمانگی، تمامیت و احراز هویت ارتباط را با TLS یا پروتکل امن معتبر برقرار کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «محرمانگی، تمامیت و احراز هویت ارتباط را با TLS یا پروتکل امن معتبر برقرار کن.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0131 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0133",
      "title": "در صورت استفاده از اتصال P2P با رمزنگاری سرتاسری، کلیدهای نشست و اعتبارسنجی همتا را با کتابخانهٔ استاندارد مدیریت کن.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0132",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: در صورت استفاده از اتصال P2P با رمزنگاری سرتاسری، کلیدهای نشست و اعتبارسنجی همتا را با کتابخانهٔ استاندارد مدیریت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «در صورت استفاده از اتصال P2P با رمزنگاری سرتاسری، کلیدهای نشست و اعتبارسنجی همتا را با کتابخانهٔ استاندارد مدیریت کن.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0132 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0134",
      "title": "گواهی یا کلید نامعتبر، همتای ناشناخته و نسخهٔ رمزنگاری ناسازگار را رد کن.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0133",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: گواهی یا کلید نامعتبر، همتای ناشناخته و نسخهٔ رمزنگاری ناسازگار را رد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «گواهی یا کلید نامعتبر، همتای ناشناخته و نسخهٔ رمزنگاری ناسازگار را رد کن.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0133 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0135",
      "title": "شناسهٔ درخواست را از شناسهٔ کاربر و شناسهٔ دائمی گفت‌وگو مستقل نگه دار.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0134",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: شناسهٔ درخواست را از شناسهٔ کاربر و شناسهٔ دائمی گفت‌وگو مستقل نگه دار."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «شناسهٔ درخواست را از شناسهٔ کاربر و شناسهٔ دائمی گفت‌وگو مستقل نگه دار.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0134 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0136",
      "title": "برای درخواست‌های مختلف شناسه‌های تصادفی مستقل تولید کن؛ هش ساده را جایگزین شناسهٔ امن نکن.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0135",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برای درخواست‌های مختلف شناسه‌های تصادفی مستقل تولید کن؛ هش ساده را جایگزین شناسهٔ امن نکن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برای درخواست‌های مختلف شناسه‌های تصادفی مستقل تولید کن؛ هش ساده را جایگزین شناسهٔ امن نکن.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0135 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0137",
      "title": "آزمون دستکاری پیام، جعل پاسخ، replay و اتصال به همتای نامعتبر را بنویس.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0136",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون دستکاری پیام، جعل پاسخ، replay و اتصال به همتای نامعتبر را بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون دستکاری پیام، جعل پاسخ، replay و اتصال به همتای نامعتبر را بنویس.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0136 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0138",
      "title": "اسناد حریم خصوصی را صریحاً به‌گونه‌ای بنویس که میزبان پردازشگر می‌تواند متن درخواست و capsule را بخواند.",
      "section": "13. هویت ارتباطی و رمزنگاری",
      "depends_on": [
        "T0137",
        "T0128"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اسناد حریم خصوصی را صریحاً به‌گونه‌ای بنویس که میزبان پردازشگر می‌تواند متن درخواست و capsule را بخواند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اسناد حریم خصوصی را صریحاً به‌گونه‌ای بنویس که میزبان پردازشگر می‌تواند متن درخواست و capsule را بخواند.» در مسیر src/Shared.Security/ + tests/security/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0137 و T0128 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + tests/security/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0139",
      "title": "برنامهٔ اندروید را با صفحهٔ اصلی قابل‌اجرا و ناوبری Compose بساز.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: برنامهٔ اندروید را با صفحهٔ اصلی قابل‌اجرا و ناوبری Compose بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «برنامهٔ اندروید را با صفحهٔ اصلی قابل‌اجرا و ناوبری Compose بساز.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0140",
      "title": "معماری ViewModel، repository و لایهٔ داده را با کد اجرایی ایجاد کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0139",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: معماری ViewModel، repository و لایهٔ داده را با کد اجرایی ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «معماری ViewModel، repository و لایهٔ داده را با کد اجرایی ایجاد کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0139 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0141",
      "title": "پایگاه دادهٔ محلی Room برای گفت‌وگوها، پیام‌ها و وضعیت درخواست‌ها ایجاد کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0140",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پایگاه دادهٔ محلی Room برای گفت‌وگوها، پیام‌ها و وضعیت درخواست‌ها ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پایگاه دادهٔ محلی Room برای گفت‌وگوها، پیام‌ها و وضعیت درخواست‌ها ایجاد کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0140 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0142",
      "title": "مهاجرت نسخهٔ پایگاه داده و آزمون migration را اضافه کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0141",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مهاجرت نسخهٔ پایگاه داده و آزمون migration را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مهاجرت نسخهٔ پایگاه داده و آزمون migration را اضافه کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0141 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0143",
      "title": "صفحهٔ فهرست گفت‌وگوها را به دادهٔ واقعی Room متصل کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0142",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: صفحهٔ فهرست گفت‌وگوها را به دادهٔ واقعی Room متصل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «صفحهٔ فهرست گفت‌وگوها را به دادهٔ واقعی Room متصل کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0142 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0144",
      "title": "ایجاد، تغییر عنوان، حذف و بازکردن گفت‌وگو را پیاده‌سازی کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0143",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ایجاد، تغییر عنوان، حذف و بازکردن گفت‌وگو را پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ایجاد، تغییر عنوان، حذف و بازکردن گفت‌وگو را پیاده‌سازی کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0143 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0145",
      "title": "صفحهٔ گفت‌وگو با پیام‌های مرتب‌شده، ورودی متن و ارسال را بساز.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0144",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: صفحهٔ گفت‌وگو با پیام‌های مرتب‌شده، ورودی متن و ارسال را بساز."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «صفحهٔ گفت‌وگو با پیام‌های مرتب‌شده، ورودی متن و ارسال را بساز.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0144 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0146",
      "title": "ارسال پیام خالی یا بیش از سقف طول را در UI و لایهٔ دامنه رد کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0145",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ارسال پیام خالی یا بیش از سقف طول را در UI و لایهٔ دامنه رد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ارسال پیام خالی یا بیش از سقف طول را در UI و لایهٔ دامنه رد کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0145 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0147",
      "title": "وضعیت‌های queued، contacting، waiting، partial، completed، failed و cancelled را نمایش بده.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0146",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وضعیت‌های queued، contacting، waiting، partial، completed، failed و cancelled را نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وضعیت‌های queued، contacting، waiting، partial، completed، failed و cancelled را نمایش بده.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0146 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0148",
      "title": "بازسازی UI پس از چرخش صفحه و فرایند recreation را با ViewModel و ذخیره‌سازی درست کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0147",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: بازسازی UI پس از چرخش صفحه و فرایند recreation را با ViewModel و ذخیره‌سازی درست کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «بازسازی UI پس از چرخش صفحه و فرایند recreation را با ViewModel و ذخیره‌سازی درست کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0147 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0149",
      "title": "پیام‌ها و وضعیت‌های درخواست را پس از بستن و بازکردن اپ بازیابی کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0148",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پیام‌ها و وضعیت‌های درخواست را پس از بستن و بازکردن اپ بازیابی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پیام‌ها و وضعیت‌های درخواست را پس از بستن و بازکردن اپ بازیابی کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0148 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0150",
      "title": "آزمون رابط و repository برای ایجاد گفت‌وگو، ارسال پیام و بازیابی تاریخچه اضافه کن.",
      "section": "14. اپلیکیشن واقعی اندروید",
      "depends_on": [
        "T0149",
        "T0138"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون رابط و repository برای ایجاد گفت‌وگو، ارسال پیام و بازیابی تاریخچه اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون رابط و repository برای ایجاد گفت‌وگو، ارسال پیام و بازیابی تاریخچه اضافه کن.» در مسیر src/Client.Android/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0149 و T0138 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0151",
      "title": "مدل نسخه‌دار `ConversationContextCapsule` را برای هدف، اطلاعات مهم، تصمیم‌ها، محدودیت‌ها و موارد حل‌نشده تعریف کن.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مدل نسخه‌دار `ConversationContextCapsule` را برای هدف، اطلاعات مهم، تصمیم‌ها، محدودیت‌ها و موارد حل‌نشده تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مدل نسخه‌دار `ConversationContextCapsule` را برای هدف، اطلاعات مهم، تصمیم‌ها، محدودیت‌ها و موارد حل‌نشده تعریف کن.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0152",
      "title": "capsule را در Room و با رابطهٔ مستقل برای هر گفت‌وگو ذخیره کن.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0151",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: capsule را در Room و با رابطهٔ مستقل برای هر گفت‌وگو ذخیره کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «capsule را در Room و با رابطهٔ مستقل برای هر گفت‌وگو ذخیره کن.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0151 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0153",
      "title": "رابط مشاهده، ویرایش و حذف capsule را بساز و تغییرات را پایدار ذخیره کن.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0152",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: رابط مشاهده، ویرایش و حذف capsule را بساز و تغییرات را پایدار ذخیره کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «رابط مشاهده، ویرایش و حذف capsule را بساز و تغییرات را پایدار ذخیره کن.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0152 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0154",
      "title": "محدودیت اندازهٔ capsule را در تنظیمات قابل‌تغییر و در لایهٔ دامنه اعمال کن.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0153",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: محدودیت اندازهٔ capsule را در تنظیمات قابل‌تغییر و در لایهٔ دامنه اعمال کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «محدودیت اندازهٔ capsule را در تنظیمات قابل‌تغییر و در لایهٔ دامنه اعمال کن.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0153 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0155",
      "title": "خلاصه‌ساز زمینه را از طریق رابط قابل‌تعویض پیاده‌سازی کن و خروجی آن را با schema اعتبارسنجی کن.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0154",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: خلاصه‌ساز زمینه را از طریق رابط قابل‌تعویض پیاده‌سازی کن و خروجی آن را با schema اعتبارسنجی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «خلاصه‌ساز زمینه را از طریق رابط قابل‌تعویض پیاده‌سازی کن و خروجی آن را با schema اعتبارسنجی کن.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0154 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0156",
      "title": "به‌روزرسانی capsule را از تاریخچهٔ محلی انجام بده و تاریخچهٔ کامل را در payload شبکه قرار نده.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0155",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: به‌روزرسانی capsule را از تاریخچهٔ محلی انجام بده و تاریخچهٔ کامل را در payload شبکه قرار نده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «به‌روزرسانی capsule را از تاریخچهٔ محلی انجام بده و تاریخچهٔ کامل را در payload شبکه قرار نده.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0155 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0157",
      "title": "وقتی پاسخ‌ها با هم تعارض دارند، موارد متعارض را به‌عنوان پیشنهاد نیازمند تأیید نگه دار.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0156",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: وقتی پاسخ‌ها با هم تعارض دارند، موارد متعارض را به‌عنوان پیشنهاد نیازمند تأیید نگه دار."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «وقتی پاسخ‌ها با هم تعارض دارند، موارد متعارض را به‌عنوان پیشنهاد نیازمند تأیید نگه دار.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0156 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0158",
      "title": "تغییرات capsule را با منشأ و زمان نگه دار، بدون افزودن شناسهٔ دائمی گفت‌وگو به درخواست شبکه.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0157",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تغییرات capsule را با منشأ و زمان نگه دار، بدون افزودن شناسهٔ دائمی گفت‌وگو به درخواست شبکه."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تغییرات capsule را با منشأ و زمان نگه دار، بدون افزودن شناسهٔ دائمی گفت‌وگو به درخواست شبکه.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0157 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0159",
      "title": "آزمون capsule شامل سقف اندازه، دادهٔ خراب، ویرایش کاربر، حذف و تعارض پاسخ‌ها را بنویس.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0158",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون capsule شامل سقف اندازه، دادهٔ خراب، ویرایش کاربر، حذف و تعارض پاسخ‌ها را بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون capsule شامل سقف اندازه، دادهٔ خراب، ویرایش کاربر، حذف و تعارض پاسخ‌ها را بنویس.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0158 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0160",
      "title": "آزمون شبکه ثابت کند payload شامل پیام فعلی و capsule است و نه تاریخچهٔ کامل.",
      "section": "15. مخزن زمینهٔ فشردهٔ گفت‌وگو",
      "depends_on": [
        "T0159",
        "T0150"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون شبکه ثابت کند payload شامل پیام فعلی و capsule است و نه تاریخچهٔ کامل."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون شبکه ثابت کند payload شامل پیام فعلی و capsule است و نه تاریخچهٔ کامل.» در مسیر src/Client.Android/Context/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0159 و T0150 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Context/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0161",
      "title": "مدل دامنهٔ `DistributedRequest` با deadline، حداکثر دور، شناسهٔ موقت و وضعیت ایجاد کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مدل دامنهٔ `DistributedRequest` با deadline، حداکثر دور، شناسهٔ موقت و وضعیت ایجاد کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مدل دامنهٔ `DistributedRequest` با deadline، حداکثر دور، شناسهٔ موقت و وضعیت ایجاد کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0162",
      "title": "الگوریتم انتخاب میزبان را به سه گرهٔ آماده، متفاوت و سازگار با قابلیت درخواست محدود کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0161",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: الگوریتم انتخاب میزبان را به سه گرهٔ آماده، متفاوت و سازگار با قابلیت درخواست محدود کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «الگوریتم انتخاب میزبان را به سه گرهٔ آماده، متفاوت و سازگار با قابلیت درخواست محدود کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0161 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0163",
      "title": "درخواست هر دور را با شناسهٔ تلاش و شناسهٔ موقت قابل‌تطبیق ارسال کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0162",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: درخواست هر دور را با شناسهٔ تلاش و شناسهٔ موقت قابل‌تطبیق ارسال کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «درخواست هر دور را با شناسهٔ تلاش و شناسهٔ موقت قابل‌تطبیق ارسال کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0162 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0164",
      "title": "timer هر دور را با clock قابل‌آزمایش و timeout پیکربندی‌شده پیاده‌سازی کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0163",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: timer هر دور را با clock قابل‌آزمایش و timeout پیکربندی‌شده پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «timer هر دور را با clock قابل‌آزمایش و timeout پیکربندی‌شده پیاده‌سازی کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0163 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0165",
      "title": "هر پاسخ را با شناسهٔ درخواست، دور، میزبان و deadline اعتبارسنجی کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0164",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هر پاسخ را با شناسهٔ درخواست، دور، میزبان و deadline اعتبارسنجی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هر پاسخ را با شناسهٔ درخواست، دور، میزبان و deadline اعتبارسنجی کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0164 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0166",
      "title": "پاسخ معتبر هر میزبان را جداگانه ذخیره و در UI به‌صورت کارت مستقل نمایش بده.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0165",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاسخ معتبر هر میزبان را جداگانه ذخیره و در UI به‌صورت کارت مستقل نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاسخ معتبر هر میزبان را جداگانه ذخیره و در UI به‌صورت کارت مستقل نمایش بده.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0165 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0167",
      "title": "پاسخ‌های چند میزبان را بدون ادغام خودکار نمایش بده.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0166",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاسخ‌های چند میزبان را بدون ادغام خودکار نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاسخ‌های چند میزبان را بدون ادغام خودکار نمایش بده.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0166 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0168",
      "title": "اگر حداقل یک پاسخ معتبر در دور رسید، دریافت سایر پاسخ‌ها را تا پایان پنجرهٔ پاسخ تعریف‌شده ادامه بده.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0167",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اگر حداقل یک پاسخ معتبر در دور رسید، دریافت سایر پاسخ‌ها را تا پایان پنجرهٔ پاسخ تعریف‌شده ادامه بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اگر حداقل یک پاسخ معتبر در دور رسید، دریافت سایر پاسخ‌ها را تا پایان پنجرهٔ پاسخ تعریف‌شده ادامه بده.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0167 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0169",
      "title": "اگر هیچ پاسخ معتبری در پنجرهٔ دور نرسید، سه میزبان متفاوت بعدی را انتخاب کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0168",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اگر هیچ پاسخ معتبری در پنجرهٔ دور نرسید، سه میزبان متفاوت بعدی را انتخاب کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اگر هیچ پاسخ معتبری در پنجرهٔ دور نرسید، سه میزبان متفاوت بعدی را انتخاب کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0168 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0170",
      "title": "سقف دورها و سقف کل میزبان‌های امتحان‌شده را به تنظیمات نسخهٔ نخست محدود کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0169",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سقف دورها و سقف کل میزبان‌های امتحان‌شده را به تنظیمات نسخهٔ نخست محدود کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سقف دورها و سقف کل میزبان‌های امتحان‌شده را به تنظیمات نسخهٔ نخست محدود کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0169 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0171",
      "title": "گره‌ای را که در دور قبلی امتحان شده، در دور بعد دوباره انتخاب نکن مگر آنکه سیاست صریح retry همان گره تعریف شده باشد.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0170",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: گره‌ای را که در دور قبلی امتحان شده، در دور بعد دوباره انتخاب نکن مگر آنکه سیاست صریح retry همان گره تعریف شده باشد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «گره‌ای را که در دور قبلی امتحان شده، در دور بعد دوباره انتخاب نکن مگر آنکه سیاست صریح retry همان گره تعریف شده باشد.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0170 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0172",
      "title": "پاسخ دیرهنگام پس از deadline را رد کن و آن را به درخواست جدید نسبت نده.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0171",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاسخ دیرهنگام پس از deadline را رد کن و آن را به درخواست جدید نسبت نده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاسخ دیرهنگام پس از deadline را رد کن و آن را به درخواست جدید نسبت نده.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0171 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0173",
      "title": "درخواست پایان‌یافته را idempotent کن تا پاسخ تکراری دوباره نمایش داده نشود.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0172",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: درخواست پایان‌یافته را idempotent کن تا پاسخ تکراری دوباره نمایش داده نشود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «درخواست پایان‌یافته را idempotent کن تا پاسخ تکراری دوباره نمایش داده نشود.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0172 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0174",
      "title": "دکمهٔ لغو را به پیام CancelRequest برای تمام میزبان‌های انتخاب‌شده وصل کن.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0173",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دکمهٔ لغو را به پیام CancelRequest برای تمام میزبان‌های انتخاب‌شده وصل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دکمهٔ لغو را به پیام CancelRequest برای تمام میزبان‌های انتخاب‌شده وصل کن.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0173 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0175",
      "title": "در پایان تلاش‌ها، وضعیت failed و امکان retry دستی را نشان بده.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0174",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: در پایان تلاش‌ها، وضعیت failed و امکان retry دستی را نشان بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «در پایان تلاش‌ها، وضعیت failed و امکان retry دستی را نشان بده.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0174 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0176",
      "title": "آزمون‌های قطعی برای صفر، یک، دو و سه پاسخ، پاسخ دیرهنگام، پاسخ تکراری، timeout و لغو بنویس.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0175",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون‌های قطعی برای صفر، یک، دو و سه پاسخ، پاسخ دیرهنگام، پاسخ تکراری، timeout و لغو بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون‌های قطعی برای صفر، یک، دو و سه پاسخ، پاسخ دیرهنگام، پاسخ تکراری، timeout و لغو بنویس.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0175 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0177",
      "title": "آزمون الگوریتم تضمین کند هر دور حداکثر سه میزبان متفاوت و هر درخواست حداکثر سقف مجاز تلاش دارد.",
      "section": "16. انتخاب سه میزبان، چندپاسخی و تلاش مجدد",
      "depends_on": [
        "T0176",
        "T0160"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون الگوریتم تضمین کند هر دور حداکثر سه میزبان متفاوت و هر درخواست حداکثر سقف مجاز تلاش دارد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون الگوریتم تضمین کند هر دور حداکثر سه میزبان متفاوت و هر درخواست حداکثر سقف مجاز تلاش دارد.» در مسیر src/Client.Android/Requests/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0176 و T0160 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Client.Android/Requests/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0178",
      "title": "سیاست پیش‌فرض نگهداری میزبان را به عدم ذخیرهٔ پایدار متن درخواست و پاسخ تبدیل کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سیاست پیش‌فرض نگهداری میزبان را به عدم ذخیرهٔ پایدار متن درخواست و پاسخ تبدیل کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سیاست پیش‌فرض نگهداری میزبان را به عدم ذخیرهٔ پایدار متن درخواست و پاسخ تبدیل کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0179",
      "title": "log ساختاریافته را با allowlist فیلدهای مجاز پیاده‌سازی کن و متن درخواست، capsule، کلیدها و شناسه‌های حساس را حذف کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0178",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: log ساختاریافته را با allowlist فیلدهای مجاز پیاده‌سازی کن و متن درخواست، capsule، کلیدها و شناسه‌های حساس را حذف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «log ساختاریافته را با allowlist فیلدهای مجاز پیاده‌سازی کن و متن درخواست، capsule، کلیدها و شناسه‌های حساس را حذف کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0178 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0180",
      "title": "پاک‌سازی payloadها و بافرهای موقت را در پایان موفق، خطا، لغو و timeout اجرا کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0179",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاک‌سازی payloadها و بافرهای موقت را در پایان موفق، خطا، لغو و timeout اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاک‌سازی payloadها و بافرهای موقت را در پایان موفق، خطا، لغو و timeout اجرا کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0179 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0181",
      "title": "تنظیم نگهداری اختیاری میزبان را با رضایت روشن صاحب رایانه پیاده‌سازی کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0180",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تنظیم نگهداری اختیاری میزبان را با رضایت روشن صاحب رایانه پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تنظیم نگهداری اختیاری میزبان را با رضایت روشن صاحب رایانه پیاده‌سازی کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0180 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0182",
      "title": "پاک‌سازی دادهٔ موقت را پس از راه‌اندازی مجدد نیز بررسی کن و فایل موقت یتیم را حذف کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0181",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: پاک‌سازی دادهٔ موقت را پس از راه‌اندازی مجدد نیز بررسی کن و فایل موقت یتیم را حذف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «پاک‌سازی دادهٔ موقت را پس از راه‌اندازی مجدد نیز بررسی کن و فایل موقت یتیم را حذف کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0181 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0183",
      "title": "از Android Keystore برای اسرار و کلیدهای محلی استفاده کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0182",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: از Android Keystore برای اسرار و کلیدهای محلی استفاده کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «از Android Keystore برای اسرار و کلیدهای محلی استفاده کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0182 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0184",
      "title": "از رمزگذاری ذخیره‌سازی حساس در اندروید استفاده کن و کلید را در کد منبع قرار نده.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0183",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: از رمزگذاری ذخیره‌سازی حساس در اندروید استفاده کن و کلید را در کد منبع قرار نده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «از رمزگذاری ذخیره‌سازی حساس در اندروید استفاده کن و کلید را در کد منبع قرار نده.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0183 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0185",
      "title": "درخواست مجوزهای Android را به حداقل لازم محدود کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0184",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: درخواست مجوزهای Android را به حداقل لازم محدود کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «درخواست مجوزهای Android را به حداقل لازم محدود کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0184 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0186",
      "title": "متن رضایت و هشدار حریم خصوصی را در رابط پیش از اتصال به میزبان نمایش بده.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0185",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: متن رضایت و هشدار حریم خصوصی را در رابط پیش از اتصال به میزبان نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «متن رضایت و هشدار حریم خصوصی را در رابط پیش از اتصال به میزبان نمایش بده.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0185 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0187",
      "title": "آزمون جست‌وجوی log و فایل‌های موقت را برای نبود متن درخواست، capsule و اسرار اضافه کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0186",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون جست‌وجوی log و فایل‌های موقت را برای نبود متن درخواست، capsule و اسرار اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون جست‌وجوی log و فایل‌های موقت را برای نبود متن درخواست، capsule و اسرار اضافه کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0186 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0188",
      "title": "threat model و محدودیت ناشناس‌بودن را در `docs/security.md` با پیوند به کنترل‌های اجرایی ثبت کن.",
      "section": "17. امنیت داده و سیاست نگهداری",
      "depends_on": [
        "T0187",
        "T0177"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: threat model و محدودیت ناشناس‌بودن را در `docs/security.md` با پیوند به کنترل‌های اجرایی ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «threat model و محدودیت ناشناس‌بودن را در `docs/security.md` با پیوند به کنترل‌های اجرایی ثبت کن.» در مسیر src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0187 و T0177 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Shared.Security/ + src/Host.Windows/ + src/Client.Android/ + docs/security.md"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0189",
      "title": "کد خطای پایدار و قابل‌نمایش برای خطاهای شبکه، مدل، منابع و اعتبارسنجی تعریف کن.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: کد خطای پایدار و قابل‌نمایش برای خطاهای شبکه، مدل، منابع و اعتبارسنجی تعریف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «کد خطای پایدار و قابل‌نمایش برای خطاهای شبکه، مدل، منابع و اعتبارسنجی تعریف کن.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0190",
      "title": "گزارش وضعیت شبکه و inference را بدون محتوای پیام به کاربر نمایش بده.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0189",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: گزارش وضعیت شبکه و inference را بدون محتوای پیام به کاربر نمایش بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «گزارش وضعیت شبکه و inference را بدون محتوای پیام به کاربر نمایش بده.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0189 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0191",
      "title": "مدیریت لغو، timeout و exception را در همهٔ مرزهای async پیاده‌سازی کن.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0190",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: مدیریت لغو، timeout و exception را در همهٔ مرزهای async پیاده‌سازی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «مدیریت لغو، timeout و exception را در همهٔ مرزهای async پیاده‌سازی کن.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0190 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0192",
      "title": "retryهای داخلی را محدود کن و با retry توزیع‌شدهٔ درخواست تداخل نده.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0191",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: retryهای داخلی را محدود کن و با retry توزیع‌شدهٔ درخواست تداخل نده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «retryهای داخلی را محدود کن و با retry توزیع‌شدهٔ درخواست تداخل نده.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0191 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0193",
      "title": "هنگام خاموش‌شدن میزبان، پذیرش درخواست جدید را متوقف و کار جاری را امن خاتمه بده.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0192",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هنگام خاموش‌شدن میزبان، پذیرش درخواست جدید را متوقف و کار جاری را امن خاتمه بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هنگام خاموش‌شدن میزبان، پذیرش درخواست جدید را متوقف و کار جاری را امن خاتمه بده.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0192 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0194",
      "title": "هنگام بسته‌شدن اپ اندروید، وضعیت درخواست‌های جاری را بازیابی‌پذیر یا صریحاً منقضی کن.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0193",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: هنگام بسته‌شدن اپ اندروید، وضعیت درخواست‌های جاری را بازیابی‌پذیر یا صریحاً منقضی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «هنگام بسته‌شدن اپ اندروید، وضعیت درخواست‌های جاری را بازیابی‌پذیر یا صریحاً منقضی کن.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0193 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0195",
      "title": "آزمون قطع اینترنت، قطع میزبان، crash موتور، کمبود دیسک و خاموش‌شدن برنامه را اضافه کن.",
      "section": "18. خطا، مشاهده‌پذیری و پایداری",
      "depends_on": [
        "T0194",
        "T0188"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون قطع اینترنت، قطع میزبان، crash موتور، کمبود دیسک و خاموش‌شدن برنامه را اضافه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون قطع اینترنت، قطع میزبان، crash موتور، کمبود دیسک و خاموش‌شدن برنامه را اضافه کن.» در مسیر src/Host.Windows/ + src/Client.Android/ + tests/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0194 و T0188 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "src/Host.Windows/ + src/Client.Android/ + tests/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0196",
      "title": "آزمون واحد برای state machine میزبان، اعتبارسنجی درخواست و ظرفیت تک‌درخواستی بنویس.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون واحد برای state machine میزبان، اعتبارسنجی درخواست و ظرفیت تک‌درخواستی بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون واحد برای state machine میزبان، اعتبارسنجی درخواست و ظرفیت تک‌درخواستی بنویس.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0197",
      "title": "آزمون واحد برای capsule، اندازهٔ payload و تعارض پاسخ‌ها بنویس.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0196",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون واحد برای capsule، اندازهٔ payload و تعارض پاسخ‌ها بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون واحد برای capsule، اندازهٔ payload و تعارض پاسخ‌ها بنویس.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0196 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0198",
      "title": "آزمون واحد برای انتخاب میزبان، دورها، timeout، deadline و idempotency بنویس.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0197",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون واحد برای انتخاب میزبان، دورها، timeout، deadline و idempotency بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون واحد برای انتخاب میزبان، دورها، timeout، deadline و idempotency بنویس.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0197 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0199",
      "title": "آزمون قرارداد مشترک را در CI برای کد C# و Kotlin اجرا کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0198",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون قرارداد مشترک را در CI برای کد C# و Kotlin اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون قرارداد مشترک را در CI برای کد C# و Kotlin اجرا کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0198 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0200",
      "title": "آزمون یکپارچهٔ اندروید را با transport آزمایشی و میزبان محلی اجرا کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0199",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون یکپارچهٔ اندروید را با transport آزمایشی و میزبان محلی اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون یکپارچهٔ اندروید را با transport آزمایشی و میزبان محلی اجرا کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0199 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0201",
      "title": "آزمون یکپارچهٔ موتور inference را با backend واقعی و مدل کوچک opt-in اجرا کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0200",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون یکپارچهٔ موتور inference را با backend واقعی و مدل کوچک opt-in اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون یکپارچهٔ موتور inference را با backend واقعی و مدل کوچک opt-in اجرا کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0200 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0202",
      "title": "تست شبکهٔ واقعی میان کلاینت اندروید و میزبان ویندوز را روی شبکهٔ محلی اجرا کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0201",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تست شبکهٔ واقعی میان کلاینت اندروید و میزبان ویندوز را روی شبکهٔ محلی اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تست شبکهٔ واقعی میان کلاینت اندروید و میزبان ویندوز را روی شبکهٔ محلی اجرا کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0201 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0203",
      "title": "سناریوی سه میزبان مستقل را با سه فرایند/دستگاه مجزا آزمایش کن و مستقل‌بودن پاسخ‌ها را بررسی کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0202",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سناریوی سه میزبان مستقل را با سه فرایند/دستگاه مجزا آزمایش کن و مستقل‌بودن پاسخ‌ها را بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سناریوی سه میزبان مستقل را با سه فرایند/دستگاه مجزا آزمایش کن و مستقل‌بودن پاسخ‌ها را بررسی کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0202 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0204",
      "title": "سناریوی دور دوم را با خاموش‌کردن یا بی‌پاسخ‌گذاشتن سه میزبان نخست آزمایش کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0203",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: سناریوی دور دوم را با خاموش‌کردن یا بی‌پاسخ‌گذاشتن سه میزبان نخست آزمایش کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «سناریوی دور دوم را با خاموش‌کردن یا بی‌پاسخ‌گذاشتن سه میزبان نخست آزمایش کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0203 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0205",
      "title": "آزمون لغو ثابت کند میزبان لغوشده پس از دریافت cancellation کار را ادامه نمی‌دهد، جز محدودیت مستند backend.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0204",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون لغو ثابت کند میزبان لغوشده پس از دریافت cancellation کار را ادامه نمی‌دهد، جز محدودیت مستند backend."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون لغو ثابت کند میزبان لغوشده پس از دریافت cancellation کار را ادامه نمی‌دهد، جز محدودیت مستند backend.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0204 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0206",
      "title": "آزمون privacy ثابت کند معرفی‌کنندهٔ گره متن درخواست و capsule دریافت نمی‌کند.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0205",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون privacy ثابت کند معرفی‌کنندهٔ گره متن درخواست و capsule دریافت نمی‌کند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون privacy ثابت کند معرفی‌کنندهٔ گره متن درخواست و capsule دریافت نمی‌کند.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0205 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0207",
      "title": "آزمون build پاک را روی runner تازهٔ CI اجرا کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0206",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: آزمون build پاک را روی runner تازهٔ CI اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «آزمون build پاک را روی runner تازهٔ CI اجرا کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0206 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0208",
      "title": "گزارش پوشش آزمون و نتایج آزمون‌های بحرانی را در artifactهای CI منتشر کن.",
      "section": "19. آزمون‌های واقعی و معیارهای قبولی",
      "depends_on": [
        "T0207",
        "T0195"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: گزارش پوشش آزمون و نتایج آزمون‌های بحرانی را در artifactهای CI منتشر کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «گزارش پوشش آزمون و نتایج آزمون‌های بحرانی را در artifactهای CI منتشر کن.» در مسیر tests/ + .github/workflows/؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0207 و T0195 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "tests/ + .github/workflows/"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0209",
      "title": "بستهٔ نصب ویندوز را از build واقعی تولید کن و نسخه، ناشر و مجوزهای وابستگی‌ها را مشخص کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: بستهٔ نصب ویندوز را از build واقعی تولید کن و نسخه، ناشر و مجوزهای وابستگی‌ها را مشخص کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «بستهٔ نصب ویندوز را از build واقعی تولید کن و نسخه، ناشر و مجوزهای وابستگی‌ها را مشخص کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0210",
      "title": "اجرای برنامهٔ نصب‌شده را روی Windows پشتیبانی‌شده از محیط تمیز بررسی کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0209",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: اجرای برنامهٔ نصب‌شده را روی Windows پشتیبانی‌شده از محیط تمیز بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «اجرای برنامهٔ نصب‌شده را روی Windows پشتیبانی‌شده از محیط تمیز بررسی کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0209 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0211",
      "title": "APK قابل‌نصب اندروید را از Gradle تولید کن و نصب و launch آن را روی emulator بررسی کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0210",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: APK قابل‌نصب اندروید را از Gradle تولید کن و نصب و launch آن را روی emulator بررسی کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «APK قابل‌نصب اندروید را از Gradle تولید کن و نصب و launch آن را روی emulator بررسی کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0210 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0212",
      "title": "امضای release اندروید را با راهکار امن خارج از مخزن مدیریت کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0211",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: امضای release اندروید را با راهکار امن خارج از مخزن مدیریت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «امضای release اندروید را با راهکار امن خارج از مخزن مدیریت کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0211 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0213",
      "title": "checksum فایل‌های انتشار را تولید کن و در release قرار بده.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0212",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: checksum فایل‌های انتشار را تولید کن و در release قرار بده."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «checksum فایل‌های انتشار را تولید کن و در release قرار بده.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0212 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0214",
      "title": "دستورهای build، test، run و package را از clone تمیز طبق README اجرا کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0213",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: دستورهای build، test، run و package را از clone تمیز طبق README اجرا کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «دستورهای build، test، run و package را از clone تمیز طبق README اجرا کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0213 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0215",
      "title": "فهرست وابستگی‌ها، مجوز نرم‌افزارها و مجوز مدل نمونه را در خروجی انتشار همراه کن.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0214",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: فهرست وابستگی‌ها، مجوز نرم‌افزارها و مجوز مدل نمونه را در خروجی انتشار همراه کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «فهرست وابستگی‌ها، مجوز نرم‌افزارها و مجوز مدل نمونه را در خروجی انتشار همراه کن.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0214 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0216",
      "title": "راهنمای نصب و عیب‌یابی میزبان ویندوز را بر اساس UI واقعی بنویس.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0215",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: راهنمای نصب و عیب‌یابی میزبان ویندوز را بر اساس UI واقعی بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «راهنمای نصب و عیب‌یابی میزبان ویندوز را بر اساس UI واقعی بنویس.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0215 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0217",
      "title": "راهنمای نصب، اتصال به میزبان و استفاده از گفت‌وگو را برای اندروید بر اساس اپ واقعی بنویس.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0216",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: راهنمای نصب، اتصال به میزبان و استفاده از گفت‌وگو را برای اندروید بر اساس اپ واقعی بنویس."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «راهنمای نصب، اتصال به میزبان و استفاده از گفت‌وگو را برای اندروید بر اساس اپ واقعی بنویس.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0216 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0218",
      "title": "release را فقط زمانی آماده اعلام کن که هر دو اپ build شوند، آزمون‌های بحرانی پاس شوند و سناریوی end-to-end مستندشده موفق باشد.",
      "section": "20. بسته‌بندی و تحویل قابل‌اجرا",
      "depends_on": [
        "T0217",
        "T0208"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: release را فقط زمانی آماده اعلام کن که هر دو اپ build شوند، آزمون‌های بحرانی پاس شوند و سناریوی end-to-end مستندشده موفق باشد."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «release را فقط زمانی آماده اعلام کن که هر دو اپ build شوند، آزمون‌های بحرانی پاس شوند و سناریوی end-to-end مستندشده موفق باشد.» در مسیر artifacts/releases/ + docs/ + GitHub Releases؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0217 و T0208 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "artifacts/releases/ + docs/ + GitHub Releases"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0219",
      "title": "همهٔ قابلیت‌های رابط کاربری را به سرویس اجرایی واقعی وصل کن و کنترل‌های بدون عملکرد را حذف یا غیرفعال کن.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: همهٔ قابلیت‌های رابط کاربری را به سرویس اجرایی واقعی وصل کن و کنترل‌های بدون عملکرد را حذف یا غیرفعال کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «همهٔ قابلیت‌های رابط کاربری را به سرویس اجرایی واقعی وصل کن و کنترل‌های بدون عملکرد را حذف یا غیرفعال کن.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0220",
      "title": "تمام TODOها، stubها، mockها و پاسخ‌های هاردکدشده را در مسیر انتشار جست‌وجو و تعیین تکلیف کن.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0219",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: تمام TODOها، stubها، mockها و پاسخ‌های هاردکدشده را در مسیر انتشار جست‌وجو و تعیین تکلیف کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «تمام TODOها، stubها، mockها و پاسخ‌های هاردکدشده را در مسیر انتشار جست‌وجو و تعیین تکلیف کن.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0219 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0221",
      "title": "ثابت کن انتخاب مدل، دانلود، نصب، اجرا و حذف مدل روی فایل‌های واقعی کار می‌کنند.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0220",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن انتخاب مدل، دانلود، نصب، اجرا و حذف مدل روی فایل‌های واقعی کار می‌کنند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن انتخاب مدل، دانلود، نصب، اجرا و حذف مدل روی فایل‌های واقعی کار می‌کنند.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0220 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0222",
      "title": "ثابت کن گفت‌وگوها و capsule بعد از راه‌اندازی مجدد اندروید حفظ می‌شوند و capsule قابل‌ویرایش/حذف است.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0221",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن گفت‌وگوها و capsule بعد از راه‌اندازی مجدد اندروید حفظ می‌شوند و capsule قابل‌ویرایش/حذف است."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن گفت‌وگوها و capsule بعد از راه‌اندازی مجدد اندروید حفظ می‌شوند و capsule قابل‌ویرایش/حذف است.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0221 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0223",
      "title": "ثابت کن درخواست از اندروید به میزبان ویندوز می‌رسد و متن پاسخ را از مدل محلی واقعی برمی‌گرداند.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0222",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن درخواست از اندروید به میزبان ویندوز می‌رسد و متن پاسخ را از مدل محلی واقعی برمی‌گرداند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن درخواست از اندروید به میزبان ویندوز می‌رسد و متن پاسخ را از مدل محلی واقعی برمی‌گرداند.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0222 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0224",
      "title": "ثابت کن سه میزبان مستقل می‌توانند برای یک درخواست پاسخ جداگانه بدهند و پاسخ‌ها در اپ جدا نمایش داده می‌شوند.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0223",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن سه میزبان مستقل می‌توانند برای یک درخواست پاسخ جداگانه بدهند و پاسخ‌ها در اپ جدا نمایش داده می‌شوند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن سه میزبان مستقل می‌توانند برای یک درخواست پاسخ جداگانه بدهند و پاسخ‌ها در اپ جدا نمایش داده می‌شوند.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0223 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0225",
      "title": "ثابت کن دور بعد فقط پس از شرایط retry تعریف‌شده آغاز می‌شود و سقف تلاش رعایت می‌شود.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0224",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن دور بعد فقط پس از شرایط retry تعریف‌شده آغاز می‌شود و سقف تلاش رعایت می‌شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن دور بعد فقط پس از شرایط retry تعریف‌شده آغاز می‌شود و سقف تلاش رعایت می‌شود.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0224 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0226",
      "title": "ثابت کن ارتباط با میزبان دارای احراز هویت و رمزگذاری است و همتای نامعتبر رد می‌شود.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0225",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن ارتباط با میزبان دارای احراز هویت و رمزگذاری است و همتای نامعتبر رد می‌شود."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن ارتباط با میزبان دارای احراز هویت و رمزگذاری است و همتای نامعتبر رد می‌شود.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0225 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0227",
      "title": "ثابت کن هیچ سرویس مرکزی متن درخواست، پاسخ یا تاریخچهٔ مکالمه را دریافت یا ذخیره نمی‌کند.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0226",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: ثابت کن هیچ سرویس مرکزی متن درخواست، پاسخ یا تاریخچهٔ مکالمه را دریافت یا ذخیره نمی‌کند."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «ثابت کن هیچ سرویس مرکزی متن درخواست، پاسخ یا تاریخچهٔ مکالمه را دریافت یا ذخیره نمی‌کند.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0226 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0228",
      "title": "محدودیت‌های باقی‌مانده، قابلیت‌های پیاده‌سازی‌نشده و آزمایش‌های انجام‌نشده را به‌روشنی در `docs/release-limitations.md` ثبت کن.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0227",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: محدودیت‌های باقی‌مانده، قابلیت‌های پیاده‌سازی‌نشده و آزمایش‌های انجام‌نشده را به‌روشنی در `docs/release-limitations.md` ثبت کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «محدودیت‌های باقی‌مانده، قابلیت‌های پیاده‌سازی‌نشده و آزمایش‌های انجام‌نشده را به‌روشنی در `docs/release-limitations.md` ثبت کن.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0227 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "id": "T0229",
      "title": "نسخهٔ نهایی را از clone تمیز بساز، نصب کن و تمام معیارهای `docs/acceptance.md` را با شواهد اجرا تأیید کن.",
      "section": "21. بازبینی نهایی ضدِ پیاده‌سازی صوری",
      "depends_on": [
        "T0228",
        "T0218"
      ],
      "inputs": [
        "خروجی تأییدشدهٔ پیش‌نیازها",
        "نیازمندی مرتبط: نسخهٔ نهایی را از clone تمیز بساز، نصب کن و تمام معیارهای `docs/acceptance.md` را با شواهد اجرا تأیید کن."
      ],
      "outputs": [
        "خروجی قابل بررسی مربوط به «نسخهٔ نهایی را از clone تمیز بساز، نصب کن و تمام معیارهای `docs/acceptance.md` را با شواهد اجرا تأیید کن.» در مسیر docs/release-limitations.md + گزارش‌های CI و شواهد آزمون؛ همراه با آزمون/شاهد پذیرش متناسب."
      ],
      "start_condition": "همهٔ پیش‌نیازهای T0228 و T0218 تکمیل و معیارهای پذیرش آن‌ها پاس شده باشند.",
      "acceptance": "خروجی واقعی در مسیر مورد انتظار وجود داشته باشد؛ آزمون مربوطه اجرا و موفق شود؛ نتیجه در CI یا گزارش آزمون قابل بررسی باشد.",
      "status": "not_started",
      "status_fa": "انجام‌نشده",
      "actual_output_paths": [],
      "expected_output_paths": [
        "docs/release-limitations.md + گزارش‌های CI و شواهد آزمون"
      ],
      "evidence": [],
      "notes": "مسیر مورد انتظار، مسیر طراحی‌شده است؛ تا زمان تولید و تأیید واقعی خروجی، به‌عنوان مسیر فایل موجود تلقی نشود.",
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
      "from": "T0001",
      "to": "T0002",
      "type": "blocks",
      "dependency_classification": "sequencing_only",
      "rationale": "The wording and outputs of T0001 establish an output/evidence policy; T0002 asks for automatic tests and CI. T0002 does not consume a specific artifact produced by T0001, so a technical dependency is not evidenced. Preserve the existing edge and order as a conservative sequencing constraint.",
      "validation_status": "validated",
      "evidence": [
        "step one/V1.0.md",
        "step three/T0001/execution-report.md"
      ]
    },
    {
      "from": "T0002",
      "to": "T0003",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0003",
      "to": "T0004",
      "type": "blocks",
      "dependency_classification": "contract_or_interface_dependency",
      "rationale": "T0004 extends the graph-guard implementation and its test contract created by T0003; it modifies the same guard API and regression-test suite, so the shared implementation/test interface is a concrete dependency. This classification does not imply the edge type itself proves dependency.",
      "validation_status": "validated",
      "evidence": [
        "tools/graph_guard.py",
        "tests/test_graph_guard.py",
        "step three/T0003/execution-report.md"
      ]
    },
    {
      "from": "T0004",
      "to": "T0005",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0005",
      "to": "T0006",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0006",
      "to": "T0007",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0007",
      "to": "T0008",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0008",
      "to": "T0009",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0009",
      "to": "T0010",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0011",
      "type": "blocks",
      "dependency_classification": "sequencing_only",
      "rationale": "T0010 requires reproducible build/test commands across the eventual application projects; it does not produce an input consumed by writing README.md. The README can be authored accurately before build entrypoints exist, provided it explicitly states that the application projects and product builds are not yet present. This is an execution-order relation, not a technical dependency.",
      "validation_status": "validated",
      "evidence": [
        "step one/V1.0.md: T0010 concerns reproducible build/test workflow; T0011 concerns product purpose, components, first-release scope, and current limitations.",
        "step three/T0005/execution-report.md: confirms application build projects are absent and product build is not yet available."
      ]
    },
    {
      "from": "T0011",
      "to": "T0012",
      "type": "blocks",
      "dependency_classification": "contract_or_interface_dependency",
      "rationale": "The requirements document refines the product goal, first-release scope, and explicit limitations already declared in README.md; that scope is a real input to defining measurable requirements. This is a content/contract dependency, not proof that any product feature is implemented.",
      "validation_status": "validated",
      "evidence": [
        "README.md: product goal, first-release scope, and current implementation limitations",
        "step one/V1.0.md: T0012 requires uniquely identified, testable requirements"
      ]
    },
    {
      "from": "T0010",
      "to": "T0012",
      "type": "blocks",
      "dependency_classification": "sequencing_only",
      "rationale": "T0010 concerns reproducible build/test entrypoints for the application projects, while T0012 defines measurable requirements and verification methods. A requirements specification can be authored before build scripts exist and does not consume build artifacts. Build reproducibility remains an outstanding task and is not claimed by T0012.",
      "validation_status": "validated",
      "evidence": [
        "step one/V1.0.md: T0010 defines reproducible build/test workflow; T0012 defines requirements",
        "step three/T0005/execution-report.md: Windows, Android, and shared-contract build entrypoints are absent in the inspected main snapshot",
        "README.md: product build and platform projects are explicitly disclosed as not yet present"
      ]
    },
    {
      "from": "T0012",
      "to": "T0013",
      "type": "blocks",
      "dependency_classification": "contract_or_interface_dependency",
      "rationale": "T0013's acceptance matrix must be traceable to the measurable functional, security, privacy, and release requirements defined by T0012. Those requirements are a real input to the acceptance criteria; this does not assert that the corresponding features are implemented.",
      "validation_status": "validated",
      "evidence": [
        "docs/requirements.md: REQ-001 through REQ-020 provide the acceptance source requirements and verification methods.",
        "step three/T0012/execution-report.md: records T0012 as done with a successful CI run."
      ]
    },
    {
      "from": "T0010",
      "to": "T0013",
      "type": "blocks",
      "dependency_classification": "sequencing_only",
      "rationale": "Writing a testable final-acceptance matrix does not consume build artifacts and can be completed before platform build entrypoints exist. However, the matrix explicitly requires clean Windows/Android builds and runtime/P2P evidence before release acceptance; this classification does not waive T0010 or claim product builds pass.",
      "validation_status": "validated",
      "evidence": [
        "docs/acceptance.md: final release gates require clean platform builds and runtime/network evidence before release approval.",
        "docs/requirements.md: REQ-018 explicitly states that Python graph checks alone do not satisfy reproducible product builds.",
        "step three/T0005/execution-report.md: confirms the current repository snapshot lacks Windows/.NET and Android/Gradle application build entrypoints."
      ]
    },
    {
      "from": "T0013",
      "to": "T0014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0014",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0014",
      "to": "T0015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0015",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0015",
      "to": "T0016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0016",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0016",
      "to": "T0017",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0017",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0017",
      "to": "T0018",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0018",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0018",
      "to": "T0019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0019",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0019",
      "to": "T0020",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0010",
      "to": "T0020",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0021",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0021",
      "to": "T0022",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0022",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0022",
      "to": "T0023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0023",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0023",
      "to": "T0024",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0024",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0024",
      "to": "T0025",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0025",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0025",
      "to": "T0026",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0026",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0026",
      "to": "T0027",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0027",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0027",
      "to": "T0028",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0028",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0028",
      "to": "T0029",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0029",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0029",
      "to": "T0030",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0020",
      "to": "T0030",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0031",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0031",
      "to": "T0032",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0032",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0032",
      "to": "T0033",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0033",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0033",
      "to": "T0034",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0034",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0034",
      "to": "T0035",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0035",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0035",
      "to": "T0036",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0036",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0036",
      "to": "T0037",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0037",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0037",
      "to": "T0038",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0038",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0038",
      "to": "T0039",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0039",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0039",
      "to": "T0040",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0030",
      "to": "T0040",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0041",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0041",
      "to": "T0042",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0042",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0042",
      "to": "T0043",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0043",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0043",
      "to": "T0044",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0044",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0044",
      "to": "T0045",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0045",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0045",
      "to": "T0046",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0046",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0046",
      "to": "T0047",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0047",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0047",
      "to": "T0048",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0040",
      "to": "T0048",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0049",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0049",
      "to": "T0050",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0050",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0050",
      "to": "T0051",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0051",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0051",
      "to": "T0052",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0052",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0052",
      "to": "T0053",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0053",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0053",
      "to": "T0054",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0054",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0054",
      "to": "T0055",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0055",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0055",
      "to": "T0056",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0048",
      "to": "T0056",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0057",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0057",
      "to": "T0058",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0058",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0058",
      "to": "T0059",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0059",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0059",
      "to": "T0060",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0060",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0060",
      "to": "T0061",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0061",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0061",
      "to": "T0062",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0062",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0062",
      "to": "T0063",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0063",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0063",
      "to": "T0064",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0064",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0064",
      "to": "T0065",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0065",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0065",
      "to": "T0066",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0066",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0066",
      "to": "T0067",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0056",
      "to": "T0067",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0068",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0068",
      "to": "T0069",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0069",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0069",
      "to": "T0070",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0070",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0070",
      "to": "T0071",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0071",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0071",
      "to": "T0072",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0072",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0072",
      "to": "T0073",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0073",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0073",
      "to": "T0074",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0074",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0074",
      "to": "T0075",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0067",
      "to": "T0075",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0076",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0076",
      "to": "T0077",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0077",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0077",
      "to": "T0078",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0078",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0078",
      "to": "T0079",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0079",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0079",
      "to": "T0080",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0080",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0080",
      "to": "T0081",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0081",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0081",
      "to": "T0082",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0082",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0082",
      "to": "T0083",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0083",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0083",
      "to": "T0084",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0084",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0084",
      "to": "T0085",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0085",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0085",
      "to": "T0086",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0086",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0086",
      "to": "T0087",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0075",
      "to": "T0087",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0088",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0088",
      "to": "T0089",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0089",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0089",
      "to": "T0090",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0090",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0090",
      "to": "T0091",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0091",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0091",
      "to": "T0092",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0092",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0092",
      "to": "T0093",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0093",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0093",
      "to": "T0094",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0094",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0094",
      "to": "T0095",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0095",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0095",
      "to": "T0096",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0096",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0096",
      "to": "T0097",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0087",
      "to": "T0097",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0098",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0098",
      "to": "T0099",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0099",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0099",
      "to": "T0100",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0100",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0100",
      "to": "T0101",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0101",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0101",
      "to": "T0102",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0102",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0102",
      "to": "T0103",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0103",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0103",
      "to": "T0104",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0104",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0104",
      "to": "T0105",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0105",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0105",
      "to": "T0106",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0106",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0106",
      "to": "T0107",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0097",
      "to": "T0107",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0108",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0108",
      "to": "T0109",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0109",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0109",
      "to": "T0110",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0110",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0110",
      "to": "T0111",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0111",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0111",
      "to": "T0112",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0112",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0112",
      "to": "T0113",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0113",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0113",
      "to": "T0114",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0114",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0114",
      "to": "T0115",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0107",
      "to": "T0115",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0116",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0116",
      "to": "T0117",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0117",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0117",
      "to": "T0118",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0118",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0118",
      "to": "T0119",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0119",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0119",
      "to": "T0120",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0120",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0120",
      "to": "T0121",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0121",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0121",
      "to": "T0122",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0122",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0122",
      "to": "T0123",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0123",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0123",
      "to": "T0124",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0124",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0124",
      "to": "T0125",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0125",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0125",
      "to": "T0126",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0126",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0126",
      "to": "T0127",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0127",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0127",
      "to": "T0128",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0115",
      "to": "T0128",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0129",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0129",
      "to": "T0130",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0130",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0130",
      "to": "T0131",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0131",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0131",
      "to": "T0132",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0132",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0132",
      "to": "T0133",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0133",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0133",
      "to": "T0134",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0134",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0134",
      "to": "T0135",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0135",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0135",
      "to": "T0136",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0136",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0136",
      "to": "T0137",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0137",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0137",
      "to": "T0138",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0128",
      "to": "T0138",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0139",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0139",
      "to": "T0140",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0140",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0140",
      "to": "T0141",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0141",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0141",
      "to": "T0142",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0142",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0142",
      "to": "T0143",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0143",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0143",
      "to": "T0144",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0144",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0144",
      "to": "T0145",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0145",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0145",
      "to": "T0146",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0146",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0146",
      "to": "T0147",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0147",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0147",
      "to": "T0148",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0148",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0148",
      "to": "T0149",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0149",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0149",
      "to": "T0150",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0138",
      "to": "T0150",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0151",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0151",
      "to": "T0152",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0152",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0152",
      "to": "T0153",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0153",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0153",
      "to": "T0154",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0154",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0154",
      "to": "T0155",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0155",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0155",
      "to": "T0156",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0156",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0156",
      "to": "T0157",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0157",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0157",
      "to": "T0158",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0158",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0158",
      "to": "T0159",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0159",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0159",
      "to": "T0160",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0150",
      "to": "T0160",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0161",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0161",
      "to": "T0162",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0162",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0162",
      "to": "T0163",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0163",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0163",
      "to": "T0164",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0164",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0164",
      "to": "T0165",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0165",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0165",
      "to": "T0166",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0166",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0166",
      "to": "T0167",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0167",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0167",
      "to": "T0168",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0168",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0168",
      "to": "T0169",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0169",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0169",
      "to": "T0170",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0170",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0170",
      "to": "T0171",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0171",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0171",
      "to": "T0172",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0172",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0172",
      "to": "T0173",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0173",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0173",
      "to": "T0174",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0174",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0174",
      "to": "T0175",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0175",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0175",
      "to": "T0176",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0176",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0176",
      "to": "T0177",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0160",
      "to": "T0177",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0178",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0178",
      "to": "T0179",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0179",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0179",
      "to": "T0180",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0180",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0180",
      "to": "T0181",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0181",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0181",
      "to": "T0182",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0182",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0182",
      "to": "T0183",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0183",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0183",
      "to": "T0184",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0184",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0184",
      "to": "T0185",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0185",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0185",
      "to": "T0186",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0186",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0186",
      "to": "T0187",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0187",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0187",
      "to": "T0188",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0177",
      "to": "T0188",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0189",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0189",
      "to": "T0190",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0190",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0190",
      "to": "T0191",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0191",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0191",
      "to": "T0192",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0192",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0192",
      "to": "T0193",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0193",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0193",
      "to": "T0194",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0194",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0194",
      "to": "T0195",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0188",
      "to": "T0195",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0196",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0196",
      "to": "T0197",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0197",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0197",
      "to": "T0198",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0198",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0198",
      "to": "T0199",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0199",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0199",
      "to": "T0200",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0200",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0200",
      "to": "T0201",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0201",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0201",
      "to": "T0202",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0202",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0202",
      "to": "T0203",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0203",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0203",
      "to": "T0204",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0204",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0204",
      "to": "T0205",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0205",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0205",
      "to": "T0206",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0206",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0206",
      "to": "T0207",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0207",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0207",
      "to": "T0208",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0195",
      "to": "T0208",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0209",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0209",
      "to": "T0210",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0210",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0210",
      "to": "T0211",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0211",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0211",
      "to": "T0212",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0212",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0212",
      "to": "T0213",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0213",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0213",
      "to": "T0214",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0214",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0214",
      "to": "T0215",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0215",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0215",
      "to": "T0216",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0216",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0216",
      "to": "T0217",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0217",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0217",
      "to": "T0218",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0208",
      "to": "T0218",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0219",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0219",
      "to": "T0220",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0220",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0220",
      "to": "T0221",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0221",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0221",
      "to": "T0222",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0222",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0222",
      "to": "T0223",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0223",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0223",
      "to": "T0224",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0224",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0224",
      "to": "T0225",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0225",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0225",
      "to": "T0226",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0226",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0226",
      "to": "T0227",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0227",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0227",
      "to": "T0228",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0228",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0228",
      "to": "T0229",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    },
    {
      "from": "T0218",
      "to": "T0229",
      "type": "blocks",
      "dependency_classification": "unreviewed",
      "rationale": "",
      "validation_status": "unreviewed",
      "evidence": []
    }
  ],
  "execution_order": [
    "T0001",
    "T0002",
    "T0003",
    "T0004",
    "T0005",
    "T0006",
    "T0007",
    "T0008",
    "T0009",
    "T0010",
    "T0011",
    "T0012",
    "T0013",
    "T0014",
    "T0015",
    "T0016",
    "T0017",
    "T0018",
    "T0019",
    "T0020",
    "T0021",
    "T0022",
    "T0023",
    "T0024",
    "T0025",
    "T0026",
    "T0027",
    "T0028",
    "T0029",
    "T0030",
    "T0031",
    "T0032",
    "T0033",
    "T0034",
    "T0035",
    "T0036",
    "T0037",
    "T0038",
    "T0039",
    "T0040",
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
    "T0076",
    "T0077",
    "T0078",
    "T0079",
    "T0080",
    "T0081",
    "T0082",
    "T0083",
    "T0084",
    "T0085",
    "T0086",
    "T0087",
    "T0088",
    "T0089",
    "T0090",
    "T0091",
    "T0092",
    "T0093",
    "T0094",
    "T0095",
    "T0096",
    "T0097",
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
    "T0140",
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
    "T0151",
    "T0152",
    "T0153",
    "T0154",
    "T0155",
    "T0156",
    "T0157",
    "T0158",
    "T0159",
    "T0160",
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
    "T0194",
    "T0195",
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
    "T0220",
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
  "dependency_model": {
    "current_model": "conservative_sequential_baseline",
    "warning": "Sequential edges may overstate technical dependencies and hide safe parallelism. Do not interpret every edge as a proven technical requirement.",
    "edge_classifications": [
      "hard_dependency",
      "contract_or_interface_dependency",
      "sequencing_only"
    ],
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
    "rule": "Keep an edge until its removal or reclassification is justified by task contracts, repository evidence, and tests. Do not remove dependencies merely to increase parallelism."
  },
  "execution_tracking_policy": {
    "purpose": "Make the graph an execution ledger as well as a dependency map.",
    "rules": [
      "Record every implementation attempt, including failed and partial attempts; do not overwrite earlier history.",
      "Record changed files and related commit SHA for each attempt. If work is not committed, state that explicitly.",
      "Record tests run, commands or test identifiers, pass/fail result, and a concise failure summary; do not claim tests were run if they were not.",
      "A blocked task must include its blocker, missing prerequisite, owner or next action where known, and the condition for unblocking.",
      "If a completed task is reopened, preserve its earlier completion evidence and record why it was reopened.",
      "When a shared contract or interface changes, identify direct and transitive consumers, mark affected tasks for review, and rerun relevant acceptance tests before retaining their done status.",
      "Do not mark a task done based only on documentation, planned output paths, stubs, mocks, or unverified claims.",
      "Use actual_output_paths only for files verified to exist in the repository at the recorded revision.",
      "Update the graph in the same change or immediately after the task change, and cross-check task status against the repository and test evidence."
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
    ]
  }
}
```

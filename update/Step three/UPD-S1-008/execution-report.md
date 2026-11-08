# گزارش اجرای UPD-S1-008 — ممیزی خط مبنا و آغاز reconciliation

## نتیجهٔ این تلاش
- وضعیت: `in_progress`؛ این تلاش فقط ممیزی خط مبنا و راستی‌آزمایی کامل نگاشت را انجام داد.
- شناسهٔ تلاش: `UPD-S1-008-20261108123027`
- شروع/پایان: `2026-11-08T12:30:27.605Z`
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit مبنا: `ceb4527da84d7c2d5c08f18f68ea66e2b719e77b` — `V 3.1.54`
- commit حاصل این تلاش: پس از ایجاد commit واقعی در تاریخچهٔ گراف ثبت می‌شود.
- عملیات گراف: `modify`؛ کد محصول تغییر نکرد و هیچ وضعیت legacy تغییر داده نشد.

## انجام‌شده
1. HEAD آنلاین main و Git tree همان commit تثبیت شد.
2. گراف Update، V1.10، گراف اولیه، V1.0 و reconciliation از commit دقیق دریافت شدند.
3. تمام پنج Blob مهم با اندازهٔ بایتی و SHA1 واقعی Git Blob تطبیق داده شدند.
4. گراف Update با 24 گره، 80 یال و 13 موج Parse شد؛ DAG و ترتیب اجرا بررسی شد.
5. گراف قدیمی با 230 گره و 422 یال از Git Blob مستقیم بازیابی و Parse شد؛ DAG بدون چرخه بود. ارجاع متنی قدیمی T0001 به‌عنوان warning حفظ شد.
6. 168 شناسهٔ متأثر، 168 رکورد یکتا و 168 نشانگر update_impact بررسی شدند؛ هیچ شناسه یا رکوردی مفقود/تکراری نبود.
7. baseline_status، legacy_status_at_review_start و وضعیت واقعی هر گره برای تمام 168 شناسه تطبیق داده شدند.
8. update_task_ids در تمام نشانگرها با رکورد reconciliation برابر بود.
9. وجود 605 مسیر خروجی ثبت‌شده و 562 مسیر شواهد ثبت‌شده در Git tree بررسی شد؛ مسیر مفقودی پیدا نشد.
10. ماتریس اثر و گزارش وضعیت reconciliation ایجاد شدند.

## خروجی‌های این تلاش
- `update/Step three/UPD-S1-008/execution-report.md`
- `update/Step three/UPD-S1-008/evidence/reconciliation-baseline-audit.md`
- `update/Step two/decision-impact-map.md`
- `update/Step two/legacy-task-reconciliation.md`

## مرزهای باز و کار باقیمانده
- بازبینی محتوایی کد و آزمون برای هر Txxxx هنوز انجام نشده است: 0/168.
- تمام 168 رکورد در reconciliation همچنان `pending` است.
- تصمیم `re_review_required` فعلی برای هر 168 مورد حفظ شده است.
- مصرف‌کنندگان مستقیم/غیرمستقیم هر قرارداد باید در بازبینی هر خوشه بررسی شوند.
- هیچ وضعیت قدیمی یا وابستگی حذف/تغییر داده نشده است.
- آزمون‌های graph_guard و test_update_graph_contract روی commit حاصل از این تلاش باید در CI دقیق بررسی شوند.

## آزمون‌ها و کنترل‌های واقعی
| کنترل | نتیجه |
|---|---|
| Blob SHA و اندازهٔ بایتی پنج منبع | موفق؛ تطبیق کامل |
| Parse گراف Update | موفق؛ 24/80/13 |
| Parse گراف قدیمی از Blob | موفق؛ 230/422 |
| یکتایی شناسه‌های قدیمی و شناسه‌های متأثر | موفق |
| تطبیق رکورد/marker/baseline status | موفق؛ 168/168 |
| وجود actual_output_paths | موفق؛ 605/605 |
| وجود evidence paths | موفق؛ 562/562 |
| DAG گراف قدیمی | موفق؛ 230 گره در ترتیب توپولوژیک؛ یک warning ارجاع metadata T0001 |
| اجرای graph_guard روی commit این تلاش | پس از push در CI بررسی می‌شود |
| بازبینی محتوایی code/test برای 168 شناسه | اجرا نشده |

## وضعیت reconciliation و پیشرفت
- شناسه‌های متأثر: 168
- شناسه‌های بازبینی محتوایی‌شده: 0
- باقی‌ماندهٔ بازبینی محتوایی: 168
- درصد تکمیل reconciliation: 0%
- UPD-S1-008: `in_progress`
- پیشرفت گراف Update پس از آغاز این تلاش: 8/24 = 33.33%.

## محدودیت محیط
این اجرا از GitHub API متصل استفاده کرد؛ clone محلی مخزن در محیط اجرایی این نشست به‌دلیل خطای DNS برای github.com ممکن نشد. در نتیجه build یا آزمون محلی Android/Windows ادعا نمی‌شود. آزمون‌های CI دقیق commitهای قبلی به‌عنوان شواهد تاریخی ثبت شده‌اند، نه جایگزین آزمون جدید این commit.


## تلاش دوم — بازبینی محتوایی T0012 و T0013 روی V 3.1.56

- Attempt: `UPD-S1-008-20261108123530`; زمان `2026-11-08T12:35:30Z` تا `2026-11-08T12:37:35Z`.
- commit دقیق بررسی‌شده: `795a2de651f4e43133f352699e34623a3a10df0a` — `V 3.1.56`.
- `T0012`: `unchanged`؛ سند نیازمندی‌ها 24 شناسهٔ یکتا و فیلدهای requirement/acceptance/verification/trace دارد؛ آزمون‌های اختصاصی در quality CI دقیق موفق شدند.
- `T0013`: `unchanged`؛ ماتریس پذیرش 21 gate دارد و تمام 24 requirement را نگاشت می‌کند؛ NOT RUN و fail-closed حفظ شده‌اند؛ آزمون‌های اختصاصی در quality CI دقیق موفق شدند.
- وضعیت legacy دو وظیفه `done` باقی ماند؛ هیچ status یا dependency قدیمی تغییر نکرد.
- شواهد تفصیلی: `update/Step three/UPD-S1-008/evidence/review-T0012-T0013.md`.
- Quality CI [42480966736](https://github.com/artinrahimii/project/actions/runs/42480966736) موفق: full Python suite، compileall و graph guards.
- Product build [42480966731](https://github.com/artinrahimii/project/actions/runs/42480966731) هنگام بررسی در حال اجرا بود؛ Windows و HostLifecycle موفق، Android JVM tests در حال اجرا و instrumentation pending بود؛ build کامل Android ادعا نمی‌شود.
- reconciliation: **2/168 reviewed** و **166 pending**؛ پیشرفت **1.19%**. UPD-S1-008 همچنان `in_progress`.


## اصلاح اعتبارسنجی پس از V 3.1.57

- CI دقیق commit `38624cbc7537637086c8ef07aa7f4d15d8315e99` (V 3.1.57)، workflow [42481482201](https://github.com/artinrahimii/project/actions/runs/42481482201)، در `test_all_legacy_impact_ids_are_mapped_for_re_review` شکست خورد.
- علت: قرارداد آزمون گراف قدیمی الزام می‌کند نشانگرهای `update_impact` برای هر 168 وظیفه همچنان `review_status=pending` و `decision=re_review_required` باشند و خلاصهٔ گراف قدیمی `pending=168` را حفظ کند. این نشانگر، ثبت اثر اولیهٔ V1.10 است؛ وضعیت بازبینی واقعی و تصمیم نهایی به‌صورت مستقل در `legacy-task-reconciliation.json` نگهداری می‌شود.
- دو نشانگر T0012/T0013 و شمارندهٔ گراف قدیمی که در V3.1.57 به reviewed تغییر کرده بودند، در این اصلاح به نسخهٔ قراردادی اولیه برمی‌گردند. رکوردهای واقعی reconciliation برای دو وظیفه بدون تغییر `reviewed/unchanged` باقی می‌مانند؛ 166 مورد دیگر `pending` هستند.
- هیچ آزمون یا شرط پذیرشی تضعیف/حذف نشده است. اجرای مجدد quality workflow روی commit اصلاحی V3.1.58 برای تأیید نهایی لازم است.


## اصلاح دوم اعتبارسنجی — V 3.1.58

- CI دقیق [42481831997](https://github.com/artinrahimii/project/actions/runs/42481831997): suite خودکار **479/479 موفق**؛ گراف قدیمی و کنترل خروجی/شواهد آن بدون خطا؛ سپس guard گراف Update با خطای `FAILED_ATTEMPT_COUNT_MISMATCH: summary=1, history=0` متوقف شد.
- علت: شمارندهٔ `failed_attempt_count` یک مورد داشت، ولی تلاش تاریخی مربوطه با نتیجهٔ `partial` ثبت شده بود؛ guard فقط رکوردهای `result=failed` را می‌شمارد.
- در این اصلاح، تلاش سوم به‌عنوان تلاش شکست‌خورده ثبت و یک تلاش چهارم برای اصلاح ledger اضافه شد. مقدار شمارنده اکنون با تعداد رکوردهای failed برابر است: 1.
- هیچ test، guard یا معیار پذیرشی حذف/تضعیف نشده است. بررسی نهایی به quality run روی V 3.1.59 وابسته است.


## تلاش پنجم — بازبینی محتوایی T0019 روی V 3.1.59

- Attempt: `UPD-S1-008-20261108124900`; زمان `2026-11-08T12:49:00Z` تا `2026-11-08T12:50:26Z`.
- commit دقیق بررسی‌شده: `32015598e6ff38f3984efb0bc8c5d1797b99cdef` — `V 3.1.59`.
- وضعیت پایهٔ T0019: `done`; تصمیم: `unchanged`; وضعیت قدیمی حفظ شد.
- وابستگی‌ها: T0018 و T0010 هر دو done؛ هیچ یال یا ترتیب اجرایی تغییر نکرد.
- ADR-0002، REQ-010/011، README، docs/acceptance، آزمون قراردادی و مصرف‌کنندگان اصلی metadata/rendezvous/direct peer بررسی شدند. ADR-0004/0005/0006 با تصمیم مستقیم P2P سازگارند.
- تست `tests/test_p2p_data_path_decision.py` در quality run [42481932091](https://github.com/artinrahimii/project/actions/runs/42481932091) اجرا شد؛ 479 آزمون Python، compileall و هر دو graph guard موفق شدند.
- محدودیت مهم: README و ADR صریحاً می‌گویند انتقال P2P هنوز در مسیر عادی گفت‌وگو یکپارچه نشده و `AC-P2P-01` همچنان NOT RUN است. این بازبینی فقط تصمیم معماری/مستندسازی T0019 را تأیید می‌کند؛ پذیرش runtime نیست.
- reconciliation: **3/168 reviewed**، **165 pending**، پیشرفت **1.79%**. UPD-S1-008 همچنان `in_progress`.


## تلاش ششم — بازبینی محتوایی T0020 روی V 3.1.60

- Attempt: `UPD-S1-008-20261108162441`; زمان `2026-11-08T16:24:41+03:30` تا `2026-11-08T16:25:55+03:30`.
- commit بررسی‌شده: `c31ac02d39993965bd693b53f626d5cb09be7b2c` — `V 3.1.60`.
- وضعیت پایهٔ T0020: `done`; تصمیم: `unchanged`; وضعیت قدیمی حفظ شد.
- دفتر `docs/decisions/README.md` و ADR-0001/0002/0003 بازبینی شدند. Platform links به `global.json`, WPF csproj و Android Gradle وصل‌اند؛ ADR backend به `LlamaCppServerBackend`, host UI, lifecycle tests و AC-LOCAL-01/02 متصل است.
- `tests/test_decision_traceability.py` و `tests/test_product_build_contract.py` در quality run [42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315) موفق شدند؛ کل suite شامل 479 تست، compileall و graph guards موفق بود.
- این نتیجه فقط scope دفتر تصمیم‌ها و traceability را می‌پوشاند؛ live model، نصب واقعی یا Android-to-Windows E2E از آن نتیجه‌گیری نشده است. `AC-LOCAL-01/02` و `AC-P2P-01` همچنان NOT RUN هستند.
- reconciliation: **4/168 reviewed**, **164 pending**, پیشرفت **2.38%**. UPD-S1-008 همچنان `in_progress`.


## تلاش هفتم — بازبینی محتوایی T0015 و T0016 روی V 3.1.61

- Attempt: `UPD-S1-008-20261108130231`; زمان ثبت: `2026-11-08T13:02:31.000Z`.
- HEAD بررسی‌شده: `a3fa1f54d99db809cc670ece1cfc244239f6a727` — `V 3.1.61`. فایل‌های محصولِ بررسی‌شده در همان snapshotی هستند که در CI روی `c31ac02d39993965bd693b53f626d5cb09be7b2c` build شدند؛ V3.1.61 تغییرات را به ledger، گراف و گزارش reconciliation محدود کرد.
- `T0015`: انتخاب C#/.NET 10/WPF، پروژهٔ WPF، پنجره و تنظیمات build بررسی شد. تصمیم `unchanged`؛ وضعیت پایهٔ `done` حفظ شد. پیش‌نیازهای `T0014`، `T0013` و `T0010` تکمیل‌شده‌اند. Windows build/publish/startup smoke/installer و HostLifecycle در run [42482227320](https://github.com/artinrahimii/project/actions/runs/42482227320) موفق شدند.
- `T0016`: پشتهٔ Kotlin/Jetpack Compose/Gradle، تنظیمات Gradle، manifest و MainActivity بررسی شد. تصمیم `unchanged`؛ وضعیت پایهٔ `done` حفظ شد. lint، debug/release build و JVM tests در run [42482227320](https://github.com/artinrahimii/project/actions/runs/42482227320) موفق بودند؛ Keystore instrumentation در آخرین مشاهده هنوز در حال اجرا بود و کل job Android موفق اعلام نمی‌شود.
- Quality run [42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315) نیز موفق بود: 479 تست Python، compileall و هر دو graph guard.
- شواهد تفصیلی: `update/Step three/UPD-S1-008/evidence/review-T0015.md` و `update/Step three/UPD-S1-008/evidence/review-T0016.md`.
- هیچ وضعیت پایه یا وابستگی قدیمی تغییر نکرد؛ نشانگرهای ثابت `update_impact` در گراف Step Three نیز دست‌نخورده ماندند. `UPD-S1-024` هنوز اجرای مستقل آیکون/پالت را لازم دارد.
- reconciliation: **6/168 reviewed**، **162 pending**، پیشرفت **3.57%**. `UPD-S1-008` همچنان `in_progress`.


## تأیید پس از commit — V 3.1.62

- commit شواهد و ledger روی شاخهٔ آنلاین `main`: `422dcd40547f98fab33096e10cd88a1d40366d68` — `V 3.1.62`؛ SHA واقعی در رکورد تلاش هفتم `execution_history.commit_sha` ثبت شد.
- Quality workflow دقیق همین commit: [42483059242](https://github.com/artinrahimii/project/actions/runs/42483059242) — **success**؛ 479 آزمون Python، compileall، Update graph validation و Step Three graph/output-evidence guards موفق شدند.
- Product build run [42483059335](https://github.com/artinrahimii/project/actions/runs/42483059335) در آخرین مشاهده هنوز `in_progress` بود؛ نتیجهٔ نهایی Android، Windows و HostLifecycle برای V3.1.62 هنوز ادعا نمی‌شود.
- فایل reconciliation اصلی `update/Step two/legacy-task-reconciliation.json` نیز به خروجی‌های واقعی UPD-S1-008 افزوده شد. شمارش بازبینی همچنان 6/168 reviewed و 162 pending است.


## بازگشایی مجدد T0020 — یکتایی شناسهٔ ADR

- مبنا: `89a7b12ff24449f2b730200939053dedcd91f56f` — `V 3.1.63`.
- دو تصمیم مستقل شناسهٔ ADR-0003 تکراری داشتند؛ برای ارجاع بدون ابهام، TCP framing به ADR-0024 منتقل و مسیر قدیمی redirect تاریخی شد.
- شواهد قبلی حفظ شدند؛ T0020 تا قبولی regression جدید در CI دقیق به `pending/re_review_required` برگشت. وضعیت پایهٔ legacy همچنان `done` است.
- شواهد اصلاح: `update/Step three/UPD-S1-008/evidence/review-T0020-ADR-uniqueness.md`. Reconciliation موقت: 5/168 reviewed، 163 pending.


## تأیید CI و بستن بازبینی T0020

- اصلاح ADR و آزمون یکتایی روی commit `365c734e178013fc6ea9499bf08855e91f5d0cbe` — `V 3.1.64` ثبت شد.
- Quality workflow دقیق همان commit [42483596179](https://github.com/artinrahimii/project/actions/runs/42483596179) با نتیجهٔ **success** پایان یافت: آزمون یکتایی ADR، full Python suite، compileall و هر دو graph guard موفق شدند.
- T0020 با تصمیم `unchanged` و `review_status=reviewed` ثبت شد؛ وضعیت پایهٔ legacy یعنی `done` حفظ شده و هیچ وضعیت/یال گراف قدیمی تغییر نکرده است.
- reconciliation فعلی: **6/168 reviewed** و **162 pending**. `UPD-S1-008` هنوز `in_progress` است.
- آزمون محصول کامل Android/Windows، دستگاه فیزیکی Android، live inference، Android-to-Windows، clean install و in-place upgrade در این تلاش اجرا نشده‌اند.


## تلاش نهم — بازبینی محتوایی T0199 و T0214 روی V 3.1.67

- Attempt: `UPD-S1-008-20261108131900`; بازبینی از `2026-11-08T13:19:00Z` تا `2026-11-08T13:19:41.169Z`.
- مبنای کد و گراف: `fe489b7f141a9a969f550b851093e3fcf45c8a19` — `V 3.1.67`.
- `T0199`: وضعیت پایهٔ `done` حفظ شد؛ schema مشترک، 11 پیام اصلی، شناسه‌های attempt/round، آزمون‌های C#/Kotlin و fixtureهای مشترک بررسی شدند. آزمون جدید، فیلدهای attempt/round را در Busy/UnsupportedModel/ProtocolError، محدودیت‌های ModelCapability و جلوگیری از prompt/response/history در metadata discovery کنترل می‌کند. Quality CI دقیق V3.1.67 موفق شد؛ Product CI برای schema/مصرف‌کنندگان بدون تغییر در V3.1.61 سه job را موفق گذرانده است. تصمیم `re_review_required` است تا پس از UPD-S1-019 آزمون‌های توسعه‌یافته دوباره بررسی شوند.
- `T0214`: وضعیت پایهٔ `done` حفظ شد؛ دستورهای README و آزمون‌های command/reproducible-build با workflowهای clean checkout تطبیق داده شدند. Product run V3.1.61 همهٔ jobهای Windows/Android/HostLifecycle و upload artifactها را موفق تکمیل کرد. تصمیم `re_review_required` است تا پس از مستندات، بسته‌بندی نهایی و upgrade در UPD-S1-021 بازبینی شود.
- Quality CI دقیق V3.1.67: [42484306209](https://github.com/artinrahimii/project/actions/runs/42484306209) — success. Product CI دقیق V3.1.67: [42484306205](https://github.com/artinrahimii/project/actions/runs/42484306205) در زمان بازبینی هنوز `in_progress` بود؛ success ادعا نمی‌شود.
- هیچ وضعیت قدیمی یا وابستگی گراف اولیه تغییر نکرد. آزمون فیزیکی Android-to-Windows، مدل واقعی روی میزبان Windows، clean install و in-place upgrade در این تلاش اجرا نشده‌اند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0199.md` و `update/Step three/UPD-S1-008/evidence/review-T0214.md`.
- reconciliation: **8/168 reviewed**، **160 pending**، **4.76%**؛ UPD-S1-008 همچنان `in_progress`.


## تأیید پس از ثبت شواهد — V 3.1.68

- commit آنلاین: `851753e3d1e668714fbc020a8d6437ecd6afae0a` — `V 3.1.68`، شاخهٔ `main`.
- Quality CI دقیق commit: [42484508893](https://github.com/artinrahimii/project/actions/runs/42484508893) — **success**.
- شناسهٔ commit واقعی در `execution_history.commit_sha` تلاش نهم ثبت شد.
- Product build دقیق V3.1.68، run [42484508866](https://github.com/artinrahimii/project/actions/runs/42484508866)، هنگام آخرین مشاهده هنوز `in_progress` بود.
- وضعیت UPD-S1-008 همچنان `in_progress` است: 8/168 legacy reviews finalized, 160 pending.


## تلاش دهم — بازبینی T0027 و T0032 تا T0036 روی V 3.1.69

- Attempt: `UPD-S1-008-20261108132200`; بررسی در `2026-11-08T13:24:39.020Z`; مبنا `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`.
- شش گره به‌صورت جداگانه از روی عنوان، شرح، وضعیت پایه، پیش‌نیازها، فایل اجرایی، مصرف‌کنندگان و آزمون پذیرش بررسی شدند.
- `T0027`: فرمان‌های README با `test_readme_build_commands.py` و `test_reproducible_build_contract.py` و workflowهای واقعی build تطبیق داده شدند؛ تصمیم `re_review_required` به‌دلیل UPD-S1-021.
- `T0032`: InferenceResponse؛ تصمیم `re_review_required` برای UPD-S1-004/017.
- `T0033`: HostAnnouncement/HostCapabilities/ModelCapability و محرمانگی metadata؛ تصمیم `re_review_required` برای UPD-S1-017.
- `T0034`: CancelRequest/HostBusy/UnsupportedModel/ProtocolError و شناسه‌های attempt/round؛ تصمیم `re_review_required` برای UPD-S1-017.
- `T0035`: ProtocolVersionPolicy، نسخه 1.0.0 و خطاهای typed؛ تصمیم `re_review_required` برای UPD-S1-017.
- `T0036`: تولید Protobuf C#/Java/Kotlin از schema canonical و آزمون round-trip؛ تصمیم `re_review_required` برای UPD-S1-017.
- همهٔ وضعیت‌های پایهٔ `done` و تمام وابستگی‌های مستقیم قدیمی حفظ شدند. Quality CI دقیق V3.1.69 (run 42484597069) موفق شد؛ Product build V3.1.61 (run 42482672544) سه job را موفق تکمیل کرد. Product build دقیق V3.1.69 در زمان این بررسی queued/in_progress بود و success ادعا نمی‌شود.
- آزمون دستگاه واقعی، Android-to-Windows end-to-end، live inference، clean install و in-place upgrade اجرا نشده‌اند.
- شواهد مستقل در `update/Step three/UPD-S1-008/evidence/review-T0027.md` و `review-T0032.md` تا `review-T0036.md` ثبت شده‌اند.
- reconciliation: **14/168 reviewed**، **154 pending**، **8.33%**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی T0041 — نقطهٔ ورود WPF

- مبنا: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`.
- وضعیت پایهٔ T0041 و T0040 هر دو `done` هستند؛ یال `T0040 → T0041` به‌صورت `sequencing_only` حفظ شد.
- Blobهای WPF entry point، MainWindow، smoke test و product-build workflow در V3.1.70 با Product commit `a3fa1f54d99db809cc670ece1cfc244239f6a727` یکسان‌اند؛ run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) سه job Windows/Android/HostLifecycle را موفق تکمیل کرد. گزارش task-specific نیز [39862296386](https://github.com/artinrahimii/project/actions/runs/39862296386) را موفق ثبت کرده است.
- تصمیم T0041: `re_review_required`؛ علت: UPD-S1-024 هنوز اجرا نشده و صفحات/هویت بصری میزبان را متأثر می‌کند. وضعیت پایهٔ قدیمی تغییر نکرد.
- Quality run دقیق V3.1.70 [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق بود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0041.md`; reconciliation اکنون 15/168 reviewed، 153 pending است.
- نصب فیزیکی، مدل محلی زنده و ارتباط Android-to-Windows در این بازبینی اجرا نشده‌اند.


## بازبینی T0042 — لایه‌های buildable میزبان

- مبنای source review: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`; HEAD جاری: `a259b29171131ce4ec17fecaadca7c24cc7523f2` — `V 3.1.71`.
- پروژه‌های Domain/Application/Infrastructure/Windows UI و Shared.Contracts و project references بررسی شدند. Application به Domain و Shared.Contracts وابسته است، نه concrete Infrastructure؛ Shared.Contracts مالک یکتای تولید C# Protobuf است. سه آزمون ساختاری در `tests/test_host_layer_projects.py` ثبت‌اند.
- Quality run دقیق V3.1.70 [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق بود؛ Product run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) Windows WPF و HostLifecycle را با موفقیت اجرا کرد، اما Android job هنوز in_progress بود. Build اصلی هنگام تکمیل T0042 [39907751958](https://github.com/artinrahimii/project/actions/runs/39907751958) همهٔ jobهای Windows/Android/HostLifecycle را موفق تکمیل کرده بود.
- تصمیم reconciliation T0042: `re_review_required/reviewed`. علت: UPD-S1-012 و UPD-S1-024 هنوز اجرا نشده‌اند و UI/resources پروژه را تحت تأثیر قرار خواهند داد. baseline قدیمی `done` حفظ شد؛ هیچ وابستگی یا status قدیمی تغییر نکرد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0042.md`; reconciliation برابر 16/168 reviewed و 152 pending است. UPD-S1-008 همچنان `in_progress` است.
- دستگاه فیزیکی، live inference، Android-to-Windows E2E، clean install و in-place upgrade در این بررسی اجرا نشدند.


## بازبینی T0043 — HostState lifecycle

- کد بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df`; HEAD فعلی مخزن: `d22f96dfaf1997593ac3907f2870713804247cb9`.
- مدل Domain و آزمون‌ها پنج وضعیت Stopped/Starting/Ready/Busy/Error و transitionهای معتبر/نامعتبر را پوشش می‌دهند؛ پیام خالی رد و پیام معتبر trim می‌شود.
- Quality [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق؛ Product CI [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) هر سه job ویندوز، اندروید و HostLifecycle را موفق تکمیل کرد. Blobهای source و تست در Product build و HEAD ممیزی یکسان‌اند.
- تصمیم reconciliation T0043 برابر `re_review_required/reviewed` است، چون UPD-S1-012 هنوز opt-in و stop behavior را یکپارچه نکرده است. baseline قدیمی `done` و وابستگی‌ها دست‌نخورده‌اند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0043.md`; reconciliation: 17/168 reviewed، 151 pending.
- دستگاه فیزیکی، live inference، Android-to-Windows E2E و upgrade در این بازبینی انجام نشده‌اند.


## بازبینی T0044 — ماتریس انتقال HostLifecycle

- source/test مبنا: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`.
- ماتریس Stopped/Starting/Ready/Busy/Error، انتقال‌های مجاز و غیرمجاز و رد پیام خالی در HostLifecycle و آزمون‌های آن بررسی شد.
- Quality run [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) و Product run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) موفق بودند؛ Windows WPF، Android APK و HostLifecycle jobs همگی سبز شدند.
- تصمیم T0044: `re_review_required/reviewed` به دلیل UPD-S1-012 که opt-in/stop را به lifecycle اضافه می‌کند. وضعیت پایهٔ قدیمی `done` بدون تغییر ماند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0044.md`; reconciliation اکنون 18/168 reviewed و 150 pending است.
- آزمون دستگاه واقعی، live inference، Android-to-Windows، clean install و in-place upgrade اجرا نشده‌اند.


## بازبینی T0045 تا T0048 — V 3.1.74

- `T0045`: schema فعلی تنظیمات میزبان نسخهٔ ۲، مهاجرت schema نسخهٔ ۱، اعتبارسنجی endpoint/model/timeout/capacity و ذخیره‌سازی اتمیک بررسی شد. آزمون‌های اختصاصی persistence از harness اصلی اجرا می‌شوند.
- `T0046`: status panel از HostStatus و ظرفیت واقعی استفاده می‌کند؛ direct P2P را پیاده‌سازی‌نشده اعلام می‌کند و Online کاذب نمایش نمی‌دهد.
- `T0047`: نگاشت خطاهای typed و پیام‌های ثابت بررسی شد؛ آزمون‌های C# مسیرها و tokenهای محرمانه را به‌عنوان ورودی استفاده می‌کنند و عدم افشا را assert می‌کنند. یال قدیمی `T0040 → T0047` با حفظ `type=blocks` به `sequencing_only/validated` طبقه‌بندی شد، چون mapper از enum مشترک استفاده می‌کند اما parser/validator تولیدشده در T0040 را مستقیماً مصرف نمی‌کند؛ rationale در `edge_change_log` ثبت شد.
- `T0048`: شکست startup، جلوگیری از Ready کاذب، retry و recovery، cancellation/drain و cleanup زمان بستن WPF بررسی شد.
- تصمیم هر چهار شناسه `re_review_required` است؛ وضعیت‌های پایهٔ `done` تغییر نکردند. T0045/T0047/T0048 پس از UPD-S1-012 و T0046 پس از UPD-S1-012 و UPD-S1-024 دوباره بررسی می‌شوند.
- Quality Gate دقیق V3.1.74، run [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109)، موفق بود. Product run source-identical V3.1.70، [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793)، هر سه job Windows، Android و HostLifecycle را موفق تکمیل کرد. Product run دقیق V3.1.74، [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134)، هنگام آخرین بررسی هنوز `in_progress` بود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0045.md` تا `review-T0048.md`. تعداد reconciliation اکنون **22/168 reviewed**، **146 pending** و **13.10%** است؛ UPD-S1-008 همچنان `in_progress`.
- آزمون مدل محلی واقعی، Android-to-Windows فیزیکی، Room E2E، نصب تمیز و in-place upgrade در این بازبینی اجرا نشده‌اند.


## اصلاح قالب و SHA کامیت تلاش — V 3.1.76

- commit واقعی تلاش پیشین `UPD-S1-008-20261108134807` اکنون با SHA کامل `4420970596b16fd09dc7fbf6ef686204e8d27a12` در فیلد `commit_sha` ثبت شده است.
- بلوک JSON گراف به‌روزرسانی با تورفتگی دو فاصله بازقالب‌بندی شد تا ساختار Markdown و قالب چندخطی تاریخی فایل حفظ شود؛ محتوا و وضعیت گره‌ها/یال‌ها از نسخهٔ V3.1.75 خوانده و Parse مجدد شد.
- این commit اصلاحی فقط metadata و قالب نگهداری را اصلاح می‌کند؛ هیچ وضعیت وظیفه یا معیار پذیرشی را تکمیل‌شده اعلام نمی‌کند.


## بازبینی تکمیلی T0049 تا T0055 — تلاش UPD-S1-008-20261108165337

### T0049 — کشف سیستم‌عامل، RAM و فضای دیسک

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- سرویس Windows-only اطلاعات OS/version را از RuntimeInformation/Environment می‌خواند؛ CPU name از Registry و تعداد پردازندهٔ منطقی از Environment.ProcessorCount دریافت می‌شود.
- RAM واقعی با GlobalMemoryStatusEx و ظرفیت/free space درایو سیستم با DriveInfo اندازه‌گیری و sanity-check می‌شوند.
- آزمون Windows startup در تاریخچهٔ وظیفه Discover() را روی runner ویندوز فراخوانی کرده و OS، RAM غیرصفر و ظرفیت دیسک را بررسی کرده است؛ source contract نیز API و سیم‌کشی startup را می‌سنجد.
- محدودیت‌های نهایی مالک و admission باید پس از اجرای UPD-S1-013 روی سرویس اندازه‌گیری اعمال و دوباره آزموده شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0049.md`.

### T0050 — کشف GPU و backendهای قابل‌دسترسی

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- EnumDisplayDevicesW آداپتورهای نمایشگر را فهرست و mirroring driverها را حذف می‌کند؛ این display inventory است و قابلیت compute یا VRAM را اثبات نمی‌کند.
- رابط این محدودیت را صریح بیان می‌کند و وضعیت backend را از health/status موتور محلی می‌گیرد؛ از inventory ادعای پشتیبانی GPU compute ساخته نمی‌شود.
- آزمون‌های source contract و WPF entrypoint مسیر نمایش و تفکیک display inventory از compute capability را بررسی می‌کنند.
- پشتیبانی محاسباتی GPU/VRAM در این baseline اثبات نشده است؛ قابلیت جدید فقط پس از اندازه‌گیری و اعمال واقعی در UPD-S1-013 قابل پذیرش است.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0050.md`.

### T0051 — تخمین سازگاری حافظهٔ مدل

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- Estimator مقدار مثبت requiredMemoryBytes را با AvailablePhysicalMemoryBytes مقایسه و ورودی غیرمثبت را رد می‌کند.
- پیام نتیجه صریحاً اعلام می‌کند GPU/VRAM، context length، فرمت مدل و overhead backend ارزیابی نشده‌اند؛ این برآورد محدود RAM است، نه تأیید قطعی سازگاری مدل.
- تست‌ها استفاده از RAM آزاد واقعی، رد ورودی نامعتبر و نمایش محدودیت‌ها در UI را بررسی می‌کنند.
- سیاست انتخاب مدل و محدودیت خروجی هنوز نیازمند اجرای UPD-S1-023 است؛ estimator به‌تنهایی تأیید کامل مدل/backend را فراهم نمی‌کند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0051.md`.

### T0052 — مدل دامنهٔ HostCapabilities

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- HostCapabilities اندازه‌گیری RAM و system-drive را nullable نگه می‌دارد و بررسی‌های memory/disk/backend را جدا ثبت می‌کند.
- ظرفیت Unknown می‌ماند تا اندازه‌گیری‌ها معتبر و همهٔ بررسی‌ها مثبت باشند؛ fail شناخته‌شده state را Insufficient می‌کند و CanRunInference فقط برای Ready است.
- HostLifecycle CI روی HEAD دقیق، رفتار Unknown و fail-closed برای measurement ناقص/نامعتبر را PASS ثبت کرده است.
- UPD-S1-013 باید این مدل را با محدودیت‌های قابل‌اندازه‌گیری مالک و admission واقعی یکپارچه کند؛ شروط idle/power غیرقابل‌سنجش نباید حدس زده شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0052.md`.

### T0053 — آزمون ظرفیت ناکافی و backend پشتیبانی‌نشده

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- HostCapacityAcceptanceTests مسیرهای low RAM، low disk و unsupported backend را پوشش می‌دهد؛ harness اصلی Unknown capacity را نیز بررسی می‌کند.
- اجرای HostLifecycle روی HEAD جاری پیام PASS مشخص برای low-RAM، low-disk، unsupported-backend و unknown-capacity ثبت کرده است.
- tests/test_host_resource_settings.py دوام تنظیمات RAM/disk، migration schema و admission fail-closed/single-inference را بررسی می‌کند.
- پس از تغییر معیارهای admission یا افزودن منبع پشتیبانی‌شده در UPD-S1-013، ماتریس آزمون باید برای هر قید جدید توسعه و دوباره اجرا شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0053.md`.

### T0054 — خارج‌کردن میزبان از Ready در حالت نامناسب

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` لازم است.
- HostReadinessEvaluator ورودی RAM/disk را finite و مثبت اعتبارسنجی و نیازمندی را با RAM آزاد و فضای درایو سیستم مقایسه می‌کند؛ Unknown یا ناکافی fail-closed است.
- HostRequestAdmissionPolicy مشارکت خاموش، host/model غیر Ready، mismatch مدل، ظرفیت Unknown/Insufficient و deadline نامعتبر/منقضی را پیش از admission رد می‌کند.
- HostInferenceSession پیش از inference readiness/backend را ارزیابی می‌کند؛ HostLifecycle CI جاری مسیرهای ظرفیت/admission را PASS ثبت کرده است.
- UPD-S1-013 هنوز قیود نهایی ظرفیت و اعمال آن‌ها را کامل نکرده است؛ admission/readiness باید پس از آن بازآزمایی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0054.md`.

### T0055 — نمایش CPU، GPU inventory و RAM در رابط میزبان

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required`؛ رکورد reconciliation به `reviewed` تغییر کرد، زیرا بازبینی محتوایی انجام شد؛ بازبینی مجدد پس از اجرای `UPD-S1-005` و `UPD-S1-013` و `UPD-S1-024` لازم است.
- MainWindow نام CPU، تعداد logical processor، RAM کل/آزاد و display adapterها را از WindowsHostHardwareDiscovery می‌گیرد؛ WPF startup smoke صفحه و کنترل‌های لازم را بازبینی می‌کند.
- رابط صریحاً display inventory را از GPU compute/VRAM capability تفکیک می‌کند و آن را آمادهٔ پردازش فرض نمی‌کند.
- tests/test_windows_hardware_discovery.py و tests/test_wpf_entrypoint.py source wiring را بررسی می‌کنند؛ Product CI جاری نیز MainWindow startup و clean shutdown را تأیید کرد.
- پس از UPD-S1-013 باید readiness و محدودیت‌های قابل اعمال در UI منعکس شوند؛ پس از UPD-S1-024 صفحات، رنگ و آیکون باید دوباره بررسی شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0055.md`.

### CI و محدودیت‌های این تلاش

- مبنای دقیق: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`.
- Quality [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) موفق؛ Product Build [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) با سه job موفق.
- WPF startup smoke و installer smoke موفق؛ HostLifecycle دارای ۶۷ PASS و پوشش خطی ۸۵٫۸۸٪ است. آزمون اختیاری real-model در این run SKIP شد و موفقیت inference واقعی ادعا نمی‌شود.
- آزمون Android-to-Windows فیزیکی، Room E2E، clean install هدف و in-place upgrade اجرا نشده‌اند.
- reconciliation: **29/168 reviewed** و **139 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0056 تا T0060 — تلاش UPD-S1-008-20261108165615

### T0056 — نمایش ظرفیت نامشخص به‌صورت Unknown

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ ارتباط رسمی: `UPD-S1-005`، `UPD-S1-013`.
- HostCapabilities اندازه‌گیری ناقص یا بررسی انجام‌نشده را Unknown نگه می‌دارد و CanRunInference فقط برای Ready واقعی true می‌شود.
- MainWindow هنگام شکست اندازه‌گیری زنده، Capacity را Unknown و میزبان را not Ready نشان می‌دهد؛ جزئیات CPU/RAM ناموجود نیز با پیام امن نمایش داده می‌شوند.
- Python source contracts و HostLifecycle Program مسیر Unknown را پوشش می‌دهند؛ Quality CI دقیق V3.1.77 موفق شده است.
- UPD-S1-013 هنوز سیاست نهایی ظرفیت و admission را اعمال نکرده است؛ هر منبع یا قید جدید باید پس از آن در UI و domain دوباره بررسی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0056.md`.

### T0057 — قرارداد IInferenceEngine

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ ارتباط رسمی: `UPD-S1-023`.
- IInferenceEngine قرارداد Discover/Load/Generate با cancellation، Cancel، Unload و IAsyncDisposable را تعریف می‌کند؛ آزمون contract امضاها و DiscoveryResult را با reflection کنترل می‌کند.
- در composition واقعی MainWindow، HostInferenceSession با LocalInferenceBackendFactory.Create ساخته می‌شود؛ factory، LlamaCppServerBackend را از طریق IInferenceBackend بازمی‌گرداند. MainWindow ارجاعی به IInferenceEngine یا LlamaCppInferenceEngine ندارد.
- بنابراین قرارداد و adapter وجود دارند، اما اتصال آن‌ها به مسیر اصلی پنجرهٔ میزبان احراز نشد؛ آزمون فعلی عمدتاً آزمون قرارداد API است، نه اثبات integration مسیر فعال.
- UPD-S1-023 باید مالکیت قرارداد مدل/backend را تعیین کند و مسیر فعال را بدون ایجاد دو پیاده‌سازی موازی یکپارچه یا adapter بلااستفاده را مستنداً کنار بگذارد. وضعیت پایهٔ legacy حفظ می‌شود اما بازبینی مجدد ضروری است.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0057.md`.

### T0058 — adapter موتور محلی LlamaCppInferenceEngine

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ ارتباط رسمی: `UPD-S1-023`.
- Adapter از /health و /v1/models برای بررسی backend و وجود model ID پیکربندی‌شده استفاده می‌کند؛ LoadModelAsync وجود مدلی را که process بارگذاری کرده تأیید می‌کند و ادعای بارگذاری مستقل ندارد.
- GenerateAsync به LlamaCppServerBackend واگذار می‌شود؛ cancellation و operation gate وجود دارند و UnloadModelAsync ادعا نمی‌کند حافظهٔ مدل روی server آزاد شده است.
- تست‌های source-contract اتصال به backend، مدل بارگذاری‌شده، generation cancellation و عدم log کردن prompt/response را بررسی می‌کنند؛ اما MainWindow مستقیماً مسیر IInferenceBackend را به‌کار می‌گیرد.
- این adapter در composition اصلی فعلی متصل نیست و با backend مستقیم هم‌پوشانی دارد. UPD-S1-023 باید انتخاب مدل و backend را به یک مسیر فعال و deterministic تبدیل کند؛ تا آن زمان re-review لازم است.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0058.md`.

### T0059 — اجرای امن فرایند موتور محلی

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ ارتباط رسمی: `UPD-S1-023`.
- Launcher در MainWindow استفاده می‌شود؛ پیش از start نسخه و SHA-256 executable را با مقادیر مورد اعتماد verifier کنترل می‌کند، executable و GGUF را validate می‌کند و فقط loopback/port/alias مجاز را می‌پذیرد.
- ProcessStartInfo از UseShellExecute=false و ArgumentList جداگانه استفاده می‌کند؛ Stop/Dispose کل process tree را متوقف می‌کند.
- تست process launcher مسیر دارای فاصله و shell metacharacter، رد non-loopback، alias command-like و executable نامعتبر را بررسی می‌کند؛ این آزمون argument construction است و به‌تنهایی اجرای مدل واقعی نیست.
- منبع و سیاست catalog برای executable/model pinning و اتصال checksum با مدل انتخابی باید در UPD-S1-023 یکپارچه شوند. اجرای real local inference از تست fixture اثبات نمی‌شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0059.md`.

### T0060 — health check واقعی backend پیش از Ready

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ ارتباط رسمی: `UPD-S1-023`.
- LlamaCppServerBackend.CheckHealthAsync درخواست GET /health واقعی می‌فرستد و خطا/timeout را با پیام امن و IsAvailable=false ثبت می‌کند.
- HostInferenceSession.CheckBackendCoreAsync ابتدا CheckHealthAsync را اجرا می‌کند، سپس Readiness را ارزیابی و فقط پس از موفقیت هر دو HostState.Ready را تنظیم می‌کند.
- همین backend مدل فعال را از /v1/models و context را از /props می‌خواند؛ Program test صریحاً شکست health قبل از capacity readiness را PASS می‌کند.
- UPD-S1-023 باید health/model capability را با catalog معتبر، شناسهٔ دقیق و سقف خروجی انتخاب‌شده یکپارچه کند؛ health endpoint به‌تنهایی انتخاب مدل و آمادگی کل درخواست را اثبات نمی‌کند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0060.md`.

### CI و محدودیت این تلاش

- Quality دقیق V3.1.77 [run 42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق است.
- Product Build دقیق V3.1.77 [run 42502195216](https://github.com/artinrahimii/project/actions/runs/42502195216) هنگام این ثبت هنوز در حال اجراست؛ موفقیت آن ادعا نمی‌شود.
- اجرای واقعی مدل و Android-to-Windows فیزیکی در این بازبینی انجام نشده است.
- reconciliation: **34/168 reviewed** و **134 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0061 تا T0065 — تلاش UPD-S1-008-20261108170039

### T0061 — نگاشت inference به API محلی و دریافت پاسخ واقعی

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ UPD-S1-023 trigger بازبینی مجدد است.
- LlamaCppServerBackend درخواست را به POST /v1/chat/completions با model ID تنظیم‌شده، prompt و max_tokens تبدیل می‌کند؛ پاسخ choices[0].message.content را استخراج، validate و برمی‌گرداند. پیش از inference health check اجرا می‌شود و model capability از /v1/models و /props خوانده می‌شود.
- آزمون opt-in واقعی process را از طریق launcher اجرا می‌کند، loopback و health را کنترل و پاسخ غیرخالی را از backend واقعی می‌گیرد؛ پاسخ fixture را صریحاً رد می‌کند.
- workflow باینری llama.cpp b11443 را با archive SHA ثابت اعتبارسنجی و GGUF را به revision مشخص pin کرده؛ تاریخچهٔ exact run 40077575618 پاسخ واقعی مدل را PASS کرده است.
- UPD-S1-023 هنوز انتخاب deterministic مدل از catalog نصب‌شده و capability واقعی را به مسیر عادی وصل نکرده است؛ T0061 باید پس از اجرای سیاست مدل دوباره بررسی شود. این شواهد اثبات Android-to-P2P نیستند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0061.md`.

### T0062 — مدیریت timeout، خطای فرایند، قطع ارتباط و پاسخ ناقص

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ UPD-S1-023 trigger بازبینی مجدد است.
- backend timeout داخلی را به TimeoutException، قطع transport را به HttpRequestException امن و JSON/response ناقص را به InvalidDataException تبدیل می‌کند؛ caller cancellation از timeout جداست.
- Program.cs regressionهای timeout، اتصال قطع‌شده، malformed/truncated JSON و عدم افشای جزئیات transport را پوشش می‌دهد. process-kill integration جداگانه برای T0062 ادعا نشده و متعلق به T0063 است.
- HostLifecycle regression در run 40078502247 و real local inference در 40078502151 موفق ثبت شده‌اند.
- UPD-S1-023 ممکن است قرارداد typed error و مصرف‌کنندگان آن را تغییر دهد؛ پس از یکپارچه‌سازی resolver، callerها و error mapping باید دوباره بازبینی شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0062.md`.

### T0063 — لغو inference و توقف process اختصاصی

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ UPD-S1-023 trigger بازبینی مجدد است.
- MainWindow مالک LocalInferenceProcessLauncher است و token عملیات generation را به HostInferenceSession می‌دهد؛ در مسیر managed local engine امکان توقف process اختصاصی دارد.
- آزمون opt-in واقعی generation دوم را شروع می‌کند، in-flight ماندن پس از 150ms را الزام، token را cancel و OperationCanceledException را بررسی می‌کند؛ سپس process متعلق به test را stop و endpoint را unavailable می‌سنجد.
- تاریخچهٔ T0063-A004 exact commit 694b0fcf847a9a2087af292910112c9c041d86df را با active generation cancellation و توقف تمیز process گزارش می‌کند؛ jobهای Windows/Android/quality همان commit نیز موفق بوده‌اند.
- این پذیرش تاریخی مربوط به managed local inference است. پس از تغییر catalog/backend composition در UPD-S1-023 باید cancellation و process ownership دوباره آزموده شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0063.md`.

### T0064 — آزمون opt-in با مدل کوچک دارای مجوز

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ UPD-S1-023 trigger بازبینی مجدد است.
- workflow llama.cpp CPU binary را با archive SHA ثابت دانلود و مدل GGUF را به revision a6b7826ed969c3926a98359e39912af137b464cb pin می‌کند؛ size و GGUF magic را بررسی و WHITEP_RUN_REAL_INFERENCE_TEST=1 را فعال می‌کند.
- مدل pin شده در شواهد قبلی Apache-2.0 ثبت شده؛ workflow exact run 40092245411 پاسخ واقعی، cancellation فعال و توقف process را PASS کرده است.
- Product Build عادی الزاماً مسیر live model را اجرا نمی‌کند؛ یک SKIP در test harness موفقیت inference واقعی نیست.
- UPD-S1-023 باید مدل pin شده، license/checksum و metadata را با catalog محصول سازگار کند؛ پس از آن real-model workflow روی مسیر انتخاب‌شدهٔ جدید باید اجرا شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0064.md`.

### T0065 — کنترل عدم استفاده از API ارائه‌دهندگان ابری در inference

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ UPD-S1-023 trigger بازبینی مجدد است.
- NoCloudProviderInferenceTests به‌صورت source-level endpointها و SDKهای شناخته‌شده OpenAI/Anthropic/Google/Mistral/Cohere را در دو implementation محلی رد و URI میزبان، /health و /v1/chat/completions را assert می‌کند.
- خود آزمون صریحاً می‌گوید packet capture یا proof نبود همه outbound traffic نیست؛ گزارش T0065 نیز همین scope را پذیرفته است.
- Exact source CI quality run 40094709423 و product run 40094709452 روی SHA 49823014f48f79ab55525126c5d7b3b649d75ceb موفق گزارش شده‌اند.
- UPD-S1-023 مسیر backend فعال را تغییر می‌دهد؛ پس از تغییر composition باید implementation paths و guard منبع ابری دوباره inventory شوند. این تست packet-level isolation نیست.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0065.md`.

### CI و مرز شواهد

- Quality V3.1.78 [42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) موفق؛ Rendezvous [42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) موفق.
- Product Build V3.1.78 [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) هنگام این ثبت هنوز در حال اجرا بود؛ موفقیت آن ادعا نمی‌شود.
- شواهد real-model/cancellation از runهای تاریخی و commitهای مشخص‌شده‌اند؛ inference زنده روی V3.1.78 ادعا نمی‌شود.
- packet capture، Android-to-Windows فیزیکی و نصب/ارتقای فیزیکی اجرا نشده‌اند.
- reconciliation: **39/168 reviewed** و **129 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0066 تا T0070 — تلاش UPD-S1-008-20261108170257

### T0066 — اعتبارسنجی نسخه و checksum باینری موتور

- وضعیت پایه: `done`؛ حفظ شد، `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- Verifier قبل از اجرای version probe، SHA-256 را با FixedTimeEquals مقایسه می‌کند؛ نام فایل llama-server.exe را محدود، --version را با ProcessStartInfo.ArgumentList و UseShellExecute=false اجرا، timeout را enforce و پس از probe hash را دوباره بررسی می‌کند.
- LocalInferenceProcessLauncher پیش از Process.Start verifier را فراخوانی می‌کند و env version/SHA مورد اعتماد را لازم می‌داند؛ regression tests ترتیب hash-before-probe، recheck، shell-free/timeout و verify-before-start را assert می‌کنند.
- workflow باینری رسمی llama.cpp b11443 را با SHA archive ثابت بررسی و SHA executable را به launcher می‌دهد؛ historical real inference run 40104554823 و product run 40104554847 موفق ثبت شده‌اند.
- UPD-S1-023 باید executable pin و مدل catalog را به یک سیاست انتخاب backend/model وصل کند؛ مسیر انتخاب و منبع مقادیر مورد اعتماد باید پس از این کار دوباره بررسی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0066.md`.

### T0067 — قرار دادن backend پشت interface مشترک

- وضعیت پایه: `done`؛ حفظ شد، `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- MainWindow از backendFactory: LocalInferenceBackendFactory.Create استفاده می‌کند؛ HostInferenceSession فقط IInferenceBackend را نگه می‌دارد و GenerateAsync را از interface صدا می‌زند، نه از concrete LlamaCppServerBackend.
- Factory شناسهٔ پشتیبانی‌شدهٔ llama.cpp را ارائه و instance LlamaCppServerBackend را به عنوان IInferenceBackend برمی‌گرداند؛ harness یک alternate backend را بدون تغییر منطق conversation/network اجرا کرده و PASS ثبت کرده است.
- تست Python جداسازی window/session/factory و تست C# اجرای alternate backend را بررسی می‌کنند؛ runs 40105391421، 40105391441 و real inference 40105391434 موفق ثبت شده‌اند.
- UPD-S1-023 انتخاب بین backendهای واقعاً پشتیبانی‌شده و capability واقعی را اضافه می‌کند؛ factory فعلی فقط llama.cpp را expose می‌کند. قرارداد و آزمون alternate backend باید با resolver جدید دوباره اعتبارسنجی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0067.md`.

### T0068 — schema نسخه‌دار metadata مدل

- وضعیت پایه: `done`؛ حفظ شد، `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- Schema v1 id/name/GGUF format/size/checksum SHA-256/backend/source URL/license metadata را تعریف می‌کند؛ اضافات را رد و backend را به llama.cpp محدود می‌سازد.
- license metadata اختیاری است اما missing به review_required و classification unknown منتهی می‌شود؛ schema تصریح می‌کند metadata قانونی اجازهٔ redistribution را ثابت نمی‌کند.
- tests schema required fields، hash regex، GGUF/backend limits، HTTPS fields و copy-to-output/publish را بررسی می‌کنند؛ runs 40106114209 و 40106114148 موفق‌اند.
- UPD-S1-023 نیاز به deterministic selection و max-output policy دارد؛ schema v1 به‌تنهایی capability/token-window/backend runtime را نمایش نمی‌دهد. توسعهٔ schema مستلزم بازاعتبارسنجی parser و مصرف‌کنندگان است.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0068.md`.

### T0069 — parser fail-closed و URL allowlist کاتالوگ

- وضعیت پایه: `done`؛ حفظ شد، `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- Parser سقف ۲MB/10,000 مدل دارد، duplicate properties و duplicate model IDs را رد می‌کند، format/backend و SHA-256 را validate و HTTPS URL را به DNS allowlist بدون credentials/port/fragment محدود می‌کند.
- License status/classification را fail-closed می‌گیرد؛ مدل بدون license را review_required/unknown می‌کند و open_source را بدون status verified + identifier + URL نمی‌پذیرد. Parser خودش network fetch ندارد.
- C# acceptance و Python source contracts invalid schema، duplicate ID/property، checksum نامعتبر، backend پشتیبانی‌نشده، URL ناامن، license ناموجود، oversize و allowlist خالی را پوشش می‌دهند؛ runs 40113006243 و 40113006228 موفق‌اند.
- UPD-S1-023 باید parser output را به resolver deterministic و readiness واقعی متصل کند؛ parser فقط metadata را validate می‌کند و نصب یا فعال‌بودن مدل را ثابت نمی‌کند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0069.md`.

### T0070 — اتصال صفحهٔ کاتالوگ به فایل واقعی

- وضعیت پایه: `done`؛ حفظ شد، `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`، `UPD-S1-024`.
- WPF LoadCatalogButton مسیر OfflineModelCatalogStore را اجرا می‌کند؛ store فایل محلی را با ModelCatalogFileLoader/Parser اعتبارسنجی و cache معتبر offline را دوباره parse می‌کند. CatalogItems.ItemsSource از catalog.Models واقعی تغذیه و جزئیات source/license/size/checksum/backend نمایش داده می‌شود.
- SelectionChanged مقدار ActiveCatalogModelId را فقط به عنوان preference ذخیره می‌کند؛ کد صریحاً می‌گوید مدل را download/install یا در inference backend activate نمی‌کند. ConnectButton_Click هنوز ModelId.Text دستی لازم دارد و GeneratePromptAsync مقدار ثابت 1024 را می‌فرستد.
- UI contract tests اتصال list به catalog.Models، details، loader، strict UTF-8/byte limit، cache validation و backend allowlist را بررسی می‌کنند؛ HostLifecycle parser/loader acceptance نمونهٔ کاتالوگ را از disk می‌خواند. این صفحه metadata/catalog است، نه auto model selection.
- UPD-S1-023 باید preference را به مدل واقعاً نصب‌شده/ready، backend resolver و max-output clamp وصل کند؛ UPD-S1-024 نیز UI صفحات مهم میزبان را از نظر هویت بصری و کنتراست تغییر می‌دهد. پس از هر دو باید re-review شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0070.md`.

### CI و محدودیت این تلاش

- Quality V3.1.79 [42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) موفق؛ Rendezvous [42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) موفق.
- Product Build V3.1.79 [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) هنگام این بررسی در حال اجرا بود؛ موفقیت آن ادعا نمی‌شود.
- آزمون انتخاب خودکار مدل بدون ورود Model ID، تولید پاسخ از مدل کاتالوگ، و visual contrast پس از UPD-S1-024 در این تلاش اجرا نشده‌اند.
- reconciliation: **44/168 reviewed** و **124 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0071 تا T0075 — تلاش UPD-S1-008-20261108170611

### T0071 — جزئیات مجوز، منبع، اندازه و محدودیت مدل

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`، `UPD-S1-024`.
- پنل جزئیات WPF پس از انتخاب ModelCatalogEntry واقعی، نام/شناسه، فرمت، اندازه، URL منبع، وضعیت و شناسهٔ license، URL مجوز، backendهای پشتیبانی‌شده، checksum و محدودیت‌های استفاده را نمایش می‌دهد.
- لیبل مجوز بین review_required، restricted و ادعای open_source تمایز می‌گذارد؛ حتی وضعیت verified را تأیید مستقل حقوقی جلوه نمی‌دهد. UI و Python contract test وجود این فیلدها و عدم overclaim را assert می‌کنند.
- تاریخچهٔ exact source commit 33f6ada3bddda0aad99c5322b2960454be0afc35، Quality run 40115099003 و Windows Product run 40115099007 را موفق ثبت کرده است؛ هیچ model download یا inference در این task ادعا نشده.
- UPD-S1-023 اتصال کاتالوگ به انتخاب مدل/backend فعال را تغییر می‌دهد و UPD-S1-024 UI میزبان را از نظر palette/icon/contrast بازطراحی می‌کند؛ پس از هر دو باید جزئیات واقعی و rendered UI دوباره بررسی شوند.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0071.md`.

### T0072 — هشدار واضح برای مجوز ناموجود یا نامعتبر

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- ModelLicenseMetadata.RequiresReview برای statusهای review_required/unknown و classification unknown true است؛ ReviewLabel برای missing/unknown هشدار می‌دهد و برای open_source اعلام می‌کند این فقط ادعای کاتالوگ است، نه بررسی حقوقی مستقل.
- یک ستون Review state در ListView وضعیت را بدون انتخاب سطر نیز نشان می‌دهد؛ acceptance در C# و Python مسیر missing/unknown/declared verified را بررسی می‌کند تا UI مجوز را overclaim نکند.
- تاریخچه exact commit 3ee8fffe1624c17addfb9e8f07744d4c6a7e18d1، HostLifecycle run40122299496 job120956288798، WPF job120956288933 و real-inference run40122299478 را موفق ثبت کرده است.
- UPD-S1-023 سیاست catalog/model activation را تغییر می‌دهد؛ هشدار مجوز باید روی مدل انتخابی واقعی، مسیر update/download و هر نمایش جدید دوباره بررسی شود. این برچسب خودش اجازهٔ استفاده یا redistribution نیست.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0072.md`.

### T0073 — آزمون parser، checksum، backend و URLهای ناامن

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- C# acceptance و Python source tests ورودی معتبر/نامعتبر، checksum اشتباه، backend و format پشتیبانی‌نشده، duplicate model ID و JSON property، URL غیر HTTPS/خارج allowlist، license ناموجود/نامشخص و محدودیت اندازه/تعداد را پوشش می‌دهند.
- parser 2MB و سقف 10,000 entry دارد، duplicate properties را رد و allowlist source/license را از trusted configuration می‌گیرد؛ parser خودش HTTP fetch یا Process.Start نمی‌کند.
- Exact V3.0.227 Quality run40122299470 و HostLifecycle parser harness run40122299496 job120956288798 به‌عنوان evidence تاریخی ثبت شده‌اند؛ آن‌ها اجرای parser tests هستند، نه آزمون auto-selection مدل در UI.
- UPD-S1-023 resolver مدل و سقف خروجی را اضافه می‌کند؛ آزمون‌ها باید بعد از اتصال واقعی catalog -> installed model -> ready backend گسترش یابند.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0073.md`.

### T0074 — کاتالوگ نمونه با metadata، checksum و license declaration

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- فایل sample-model-catalog.json یک رکورد metadata برای SmolLM2-135M-Instruct Q4_K_M دارد: source bartowski، size اعلامی 105454432 bytes، SHA-256 برابر 2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d، backend llama.cpp و license claim Apache-2.0.
- C# acceptance آن را از فایل واقعی disk با ModelCatalogFileLoader و supported backend registry و allowlistهای صریح بارگذاری می‌کند؛ این آزمون اعتبارسنجی metadata است، نه دانلود/بارگذاری/اجرای وزن‌ها.
- این sample با artifact workflow تاریخی real-inference یکسان فرض نمی‌شود: workflow از QuantFactory revision a6b7826ed969c3926a98359e39912af137b464cb، size 105454144 و SHA 8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684 استفاده می‌کرد. دو source/revision/checksum متفاوت‌اند و تطبیق byte-level ندارند.
- تاریخچه exact V3.0.228 run40123058807 quality، run40123058861 Product (Windows/Android/HostLifecycle jobs) و run40123058748 real inference را برای همان commit گزارش کرده است؛ real inference evidence به model workflow pin شده خودش مربوط است، نه این sample JSON.
- UPD-S1-023 باید catalog model identity/checksum/license را با دانلود، نصب و backend واقعی مرتبط کند؛ metadata sample به‌تنهایی اثبات مدل نصب‌شده یا اجرای همان وزن‌ها نیست.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0074.md`.

### T0075 — fallback آفلاین فقط به آخرین کاتالوگ معتبر

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم reconciliation: `re_review_required/reviewed`؛ وظایف به‌روزرسانی مرتبط: `UPD-S1-023`.
- OfflineModelCatalogStore فایل منبع را با loader/parser strict UTF-8 و size/schema checks اعتبارسنجی می‌کند و فقط پس از موفقیت، نسخهٔ معتبر را به cache محلی می‌نویسد.
- در شکست فایل منبع، cache موجود را با همان allowlist و parser مجدداً validate می‌کند؛ اگر cache نیز missing یا invalid باشد InvalidDataException می‌دهد. Cache یک catalog metadata است، نه مدل وزن‌دار.
- MainWindow صریحاً هنگام استفاده از cache می‌گوید catalog metadata از آخرین cache محلی معتبر نمایش داده شده و هیچ مدل دانلود یا نصب نشده است. Acceptance test مسیر malformed-source/cache fallback را پوشش می‌دهد.
- تاریخچهٔ T0075 run40161618780 quality، HostLifecycle job121074200657، WPF job121074200813 و real local inference run40161618719 را روی exact source head f2758759cb5a28fcf9cfd2251918d21d93fcf3ce موفق ثبت کرده است؛ real inference مربوط به engine است، نه اثبات catalog auto-selection.
- UPD-S1-023 باید catalog selection را به مدل نصب‌شده و backend ready وصل کند؛ fallback آفلاین فقط metadata را برمی‌گرداند و مسیر inference را تغییر نمی‌دهد. پس از تغییر resolver و تنظیمات، cache و UI باید دوباره آزموده شوند.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0075.md`.

### CI و مرز شواهد

- Quality V3.1.80 [42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) موفق؛ Rendezvous [42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) موفق.
- HostLifecycle در Product Build [42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) موفق، ۶۷ PASS و ۸۵٫۸۹٪ coverage، یک opt-in real-model test SKIP.
- Windows WPF و Android APK jobs در زمان این ثبت هنوز in_progress بودند؛ Product Build کلی موفق اعلام نمی‌شود.
- انتخاب خودکار مدل، inference واقعی با مدل نمونهٔ کاتالوگ، آزمون بصری UPD-S1-024، Android-to-Windows فیزیکی، clean install و in-place upgrade در این دسته اجرا نشده‌اند.
- reconciliation: **49/168 reviewed**, **119 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0088، T0090، T0092، T0098 و T0099 — تلاش UPD-S1-008-20261108171030

### T0088 — صفحهٔ گفت‌وگوی محلی میزبان

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف update: `UPD-S1-010`، `UPD-S1-024`.
- MainWindow.xaml دارای پیام‌های چت، ورودی متن و کنترل ارسال است؛ MainWindow.xaml.cs مسیر ارسال را به HostInferenceSession متصل می‌کند و پیام user/assistant را جداگانه در UI نمایش می‌دهد.
- تاریخچهٔ T0088، build و startup smoke واقعی WPF، HostLifecycle و Android/Protobuf build را در Product run 40179954088 و Quality run 40179954129 روی source commit وظیفه ثبت کرده است.
- یال T0087→T0088 از نوع sequencing_only/validated و T0088→T0089 از نوع contract_or_interface_dependency/validated است؛ rationale/evidence قبلی حفظ می‌شوند.
- این task صفحهٔ چت میزبان Windows را پوشش می‌دهد، نه چت Android و Room. UPD-S1-010 چرخهٔ Android را به runtime واقعی متصل و UPD-S1-024 هویت بصری صفحات را تغییر می‌دهد؛ پس از هر دو باید صفحه دوباره بازبینی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0088.md`.

### T0090 — ترتیب قطعی پیام‌های مکالمهٔ میزبان

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف update: `UPD-S1-010`.
- ChatConversation پیام‌های user/assistant/system را به ترتیب در حافظه نگه می‌دارد؛ MainWindow مسیر append مرکزی دارد تا پاسخ پس از پیام کاربر درج شود.
- HostConversationRetentionPolicy نگهداری را محدود می‌کند؛ WindowsHostConversationStore رمزگذاری DPAPI و نگهداری اختیاری با رضایت مالک را پیاده می‌کند. این history محلی Windows است، نه Room اندروید.
- Acceptance tests ترتیب role/content، شناسه و timestamp پیام، clear و retention/restore را بررسی می‌کنند. Historic quality run 40181923409 و product run 40181923441 موفق بوده‌اند؛ یک source assertion قدیمی در تاریخچه حفظ و بعداً اصلاح شده است.
- یال‌های T0089→T0090 و T0090→T0091 از نوع contract_or_interface_dependency/validated و T0087→T0090 از نوع sequencing_only/validated هستند.
- این component مکالمهٔ in-memory میزبان است و persistence اختیاری DPAPI جایگزین Room اندروید نیست. UPD-S1-010 باید ترتیب و persistence همان چت Android را تضمین کند؛ مصرف‌کنندگان پس از اتصال runtime جدید دوباره بررسی شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0090.md`.

### T0092 — نمایش خطا به‌صورت پیام system، نه پاسخ موفق مدل

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف update: `UPD-S1-010`.
- HostErrorPresentation دسته‌های خطا و متن امن کاربر را جدا می‌کند؛ در مسیر Generate، خطا یا لغو به پیام System می‌رود و به‌عنوان Assistant response نمایش داده نمی‌شود.
- Regression tests در تاریخچهٔ T0092 مسیر خطا را از پاسخ موفق جدا می‌کنند؛ Quality run 40232583373 و Product run 40232583374 برای کد/آزمون مبنا موفق ثبت شده‌اند.
- یال T0091→T0092 از نوع contract_or_interface_dependency/validated و یال‌های T0087→T0092 و T0092→T0093 از نوع sequencing_only/validated هستند.
- این شواهد مربوط به WPF میزبان است. UPD-S1-010 باید typed error mapping قابل‌فهم در Android را بسازد و retry/cancel/disconnect را بدون نمایش خطا به‌صورت پاسخ موفق مدیریت کند؛ سپس بازبینی مجدد لازم است.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0092.md`.

### T0098 — هماهنگ‌کنندهٔ درخواست با semaphore تک‌ظرفیتی

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف update: `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-023`.
- HostRequestCoordinator با SemaphoreSlim(1,1) و WaitAsync(0) پذیرش غیرصفی را اعمال می‌کند؛ slot پس از success/failure/cancel آزاد می‌شود و درخواست هم‌زمان رد می‌شود.
- HostRequestAdmissionPolicy پیش از inference، participation، Ready state، model match، capacity state و deadline را fail-closed بررسی می‌کند.
- HostLifecycle روی exact V3.1.81 پیام PASS صریح دارد که از 64 درخواست هم‌زمان دقیقاً یک مورد پذیرفته و 63 مورد Busy شده‌اند؛ busy regressionها صف را رد و آزادشدن slot را کنترل می‌کنند.
- یال T0097→T0098 از نوع sequencing_only/validated و T0098→T0099 از نوع hard_dependency/validated است.
- UPD-S1-013 هنوز قیود نهایی منابع و UPD-S1-023 انتخاب مدل/backend را کامل نکرده‌اند؛ admission و ظرفیت باید پس از اعمال این دو وظیفه دوباره روی مسیر نهایی آزموده شوند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0098.md`.

### T0099 — رد fail-fast درخواست دوم با وضعیت Busy

- وضعیت پایه: `done`؛ حفظ شد و `status_changed=false`.
- تصمیم: `re_review_required/reviewed`؛ وظایف update: `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-023`.
- در حالت slot اشغال، Coordinator نتیجهٔ Busy را فوراً برمی‌گرداند؛ callback درخواست دوم اجرا و وارد صف نمی‌شود. Cancellation token حفظ می‌شود و پس از آزادشدن slot درخواست جدید پذیرفته می‌شود.
- HostRequestCoordinatorBusyTests درخواست‌های هم‌زمان را بدون queue بررسی می‌کند؛ HostRequestAdmissionTests مدل not-ready/mismatch، capacity Unknown/Insufficient و deadline expired را پیش از inference رد می‌کنند.
- تاریخچهٔ T0099 Product run 40309351792 و Quality run 40309351849 را موفق ثبت کرده؛ revalidation admission context در V3.0.313 run 40312672878 نیز پیام PASS دارد.
- یال‌های T0098→T0099 و T0099→T0100 از نوع hard_dependency/validated و T0097→T0099 از نوع sequencing_only/validated باقی می‌مانند.
- UPD-S1-013 و UPD-S1-023 هنوز سیاست ظرفیت و مدل/backend را تغییر می‌دهند؛ پس از اتصال آن‌ها Busy/admission/cancellation باید دوباره بازآزمایی شود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0099.md`.

### CI و مرز شواهد

- Quality V3.1.81 [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) موفق؛ Rendezvous [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) موفق.
- HostLifecycle در Product [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) موفق: ۶۷ PASS، یک real-model SKIP، پوشش ۸۵٫۸۸٪.
- Product Build کلی موفق اعلام نمی‌شود؛ jobهای Windows و Android در زمان بررسی در حال اجرا بودند.
- Android فیزیکی، Android-to-Windows واقعی، Room E2E، clean install و in-place upgrade اجرا نشده‌اند.
- reconciliation: **54/168 reviewed** و **114 pending**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی محتوایی T0100 تا T0109 — 2026-11-08T18:06:25.553Z

- مبنای بررسی: `63ae554809f1d1bc6217b0ad48632f416f3db0f2` — `V 3.1.82`، شاخهٔ آنلاین `main`.
- ده گرهٔ قدیمی T0100 تا T0109 از نظر کد تولیدی، آزمون ثبت‌شده، معیار legacy و یال‌های مستقیم جداگانه بررسی شدند؛ baseline status همه `done` و بدون تغییر است.
- هر ده رکورد با تصمیم `re_review_required` و `review_status=reviewed` در JSON reconciliation ثبت شد. این تصمیم به معنی تأیید نهایی محصول نیست؛ وظایف Update مرتبط هنوز اجرا نشده‌اند.
- T0100 admission context را در مسیر واقعی MainWindow به coordinator می‌رساند. T0101 مولد شناسهٔ آزموده‌شده دارد اما call-site تولیدی آن در مسیر inbound Windows اثبات نشد. T0104 gate یک‌بارمصرف آزمون دارد اما اتصال آن به همهٔ response pathهای زنده اثبات نشد. T0108/T0109 helper listener از PeerAnnouncementHostLifecycle واقعی جداگانه بررسی شد.
- وابستگی‌های مستقیم مربوطه از قبل rationale/evidence و `validated` داشتند؛ هیچ طبقه‌بندی یا تاریخچه‌ای تغییر نکرد.
- Product Build دقیق V3.1.82 [42503829586](https://github.com/artinrahimii/project/actions/runs/42503829586) موفق شد: jobهای Android، Windows WPF و HostLifecycle همگی successful بودند.
- HostLifecycle: ۶۷ PASS، پوشش خطی ۸۵٫۸۹٪ و یک آزمون real-model اختیاری با وضعیت SKIP؛ این دسته inference واقعی مدل را ثابت نمی‌کند.
- Quality Gate دقیق V3.1.82 [42503829570](https://github.com/artinrahimii/project/actions/runs/42503829570) موفق بود؛ CI جدید برای اعتبارسنجی همین تغییر ledger پس از commit اجرا خواهد شد.
- شواهد مستقل:
  - `update/Step three/UPD-S1-008/evidence/review-T0100.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0101.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0102.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0103.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0104.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0105.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0106.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0107.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0108.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0109.md`
- نتیجهٔ reconciliation: **64/168 reviewed (38.10%)** و **104 pending**؛ UPD-S1-008 همچنان `in_progress`.



## بازبینی محتوایی T0110 تا T0115 — 2026-11-08T18:10:19.333Z

- مبنای دقیق کد: `58df066c3feb44020e59d75870a3d1c66642d604` — `V 3.1.83`، شاخهٔ `main`.
- هر شش وظیفه جداگانه در برابر parser، validator، response mapper، listener، logging tests، production peer lifecycle و گراف قدیمی بررسی شدند؛ وضعیت پایهٔ هر شش `done` حفظ شد.
- T0110: parser مشترک در TCP loopback frame reader و نیز در مسیر تولیدی libp2p مستقیماً استفاده می‌شود؛ قاب‌بندی چهار‌بایتی TCP را با message framing libp2p یکی فرض نکردم.
- T0111: semantic validator اندازه، نسخه، شناسه‌ها، مدل، prompt، deadline و سقف خروجی را fail-closed کنترل می‌کند؛ auth peer و opt-in نیز پیش از handler انجام می‌شود.
- T0112: helper mapper typed به‌طور مستقل آزموده شده، ولی مسیر production peer پاسخ InferenceResponse را از سازنده‌های lifecycle/MainWindow می‌سازد؛ یکسان‌سازی/بازبینی مجدد لازم است.
- T0113: runtime canary و source scan از عدم ثبت prompt/response در sinkهای بررسی‌شده حمایت می‌کنند؛ بافرهای request/response در lifecycle پاک می‌شوند.
- T0114: listener legacy TCP loopback-only است؛ production peer hosting از bind/advertised address مشخص و تأیید operator/trust/consent استفاده می‌کند؛ reachability عمومی توسط برنامه probe نمی‌شود.
- T0115: آزمون‌های disconnect، malformed/oversized/truncated، repeated connection، cancellation و shutdown در loopback وجود دارند؛ آزمون مستقل شبکهٔ عمومی یا Android فیزیکی نیستند.
- CI دقیق V3.1.83: Quality [42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) موفق؛ HostLifecycle در Product [42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) موفق؛ Rendezvous [42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) موفق. Android JVM/Keystore و Windows installer در زمان ثبت هنوز in_progress بودند.
- شواهد مستقل:
  - `update/Step three/UPD-S1-008/evidence/review-T0110.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0111.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0112.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0113.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0114.md`
  - `update/Step three/UPD-S1-008/evidence/review-T0115.md`
- نتیجهٔ reconciliation: **70/168 reviewed (41.67%)** و **98 pending (58.33%)**؛ UPD-S1-008 همچنان `in_progress`.



## بازبینی محتوایی T0116 تا T0125 — 2026-11-08T18:16:26.015Z

- مبنای کد: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84، شاخهٔ آنلاین main.
- ده task از نظر قرارداد transport، پیاده‌سازی libp2p، fallback مستقیم TCP/QUIC، مرز relay، directory metadata-only، TTL/expiry، heartbeat احرازشده، انتخاب میزبان و تصمیم cardinality بازبینی شدند.
- T0116/T0117: IPeerTransport و پیاده‌سازی واقعی libp2p با trust/peer identity و آزمون direct TCP, QUIC, fallback, plaintext/mismatch reject و disconnect موجود است؛ مسیر نهایی Android و شبکهٔ مستقل هنوز باید جداگانه آزموده شود.
- T0118: fallback فعلی صرفاً direct TCP-to-QUIC برای یک PeerId است؛ NAT traversal و relay را ادعا نمی‌کند.
- T0119: تصمیم unchanged — Relay در first release عمداً غیرفعال است؛ مسیر direct و خطای صریح عدم اتصال حفظ می‌شود.
- T0120: تصمیم unchanged — Rendezvous فقط metadata شامل PeerId/address/capability/expiry دارد؛ prompt/response/history ارسال نمی‌شود.
- T0121/T0122: TTL و renewal پنج‌دقیقه‌ای/یک‌دقیقه‌ای، expiry removal در service و Android client filtering/cache بررسی شد؛ public HTTPS و reachability مستقل تأیید نشده است.
- T0123: status heartbeat از peer احرازشده با timestamp تازه و بدون cache به Android می‌رسد؛ آزمون loopback/Android JVM وجود دارد.
- T0124: selector deterministic، authenticated live-probe، READY/capacity/model eligibility دارد؛ اما modelId/maxOutputTokens را از قبل دریافت می‌کند و خودش انتخاب مدل را انجام نمی‌دهد؛ UPD-S1-023 این شکاف را پوشش می‌دهد.
- T0125: ADR-0022 سیاست first release را یک میزبان در هر round و تا سه تلاش ترتیبی/manual retry تأیید می‌کند؛ generic multi-host فقط compatibility است.
- وابستگی‌های مستقیم بازبینی‌شده در گراف قدیمی validated بودند؛ هیچ edge یا وضعیت قدیمی تغییر نکرد.
- Product Build V3.1.84: HostLifecycle و Windows موفق؛ Android هنوز in_progress در snapshot. Quality و Rendezvous موفق.
- شواهد مستقل:
  - update/Step three/UPD-S1-008/evidence/review-T0116.md
  - update/Step three/UPD-S1-008/evidence/review-T0117.md
  - update/Step three/UPD-S1-008/evidence/review-T0118.md
  - update/Step three/UPD-S1-008/evidence/review-T0119.md
  - update/Step three/UPD-S1-008/evidence/review-T0120.md
  - update/Step three/UPD-S1-008/evidence/review-T0121.md
  - update/Step three/UPD-S1-008/evidence/review-T0122.md
  - update/Step three/UPD-S1-008/evidence/review-T0123.md
  - update/Step three/UPD-S1-008/evidence/review-T0124.md
  - update/Step three/UPD-S1-008/evidence/review-T0125.md
- نتیجهٔ reconciliation: **80/168 reviewed (47.62%)** و **88 pending (52.38%)**؛ UPD-S1-008 همچنان in_progress.


## بازبینی محتوایی T0126 تا T0135 — 2026-11-08T18:21:15.285Z

- مبنای کد: 7082162e902e1c7e32718e0af687f8f62b7d92dc — V 3.1.85، شاخهٔ آنلاین main.
- T0126: feedback از candidateهای واقعاً probeشده تولید می‌شود؛ zero host مانع dispatch است. سیاست first-release یک host در هر round است، پس آزمون legacy سه‌میزبانه فقط compatibility است و باید با UPD-S1-003 هماهنگ شود.
- T0127: آزمون‌های Android انقضای lease، duplicate peer، stale/untrusted/busy status و no-host را پوشش می‌دهند؛ این‌ها آزمون مستقل شبکهٔ عمومی نیستند.
- T0128: وضعیت deferred_final حفظ شد؛ پذیرش دو رایانه/node مستقل و public HTTPS Rendezvous اجرا نشده و loopback/LAN/CI جایگزین آن نیست.
- T0129: unchanged — کتابخانهٔ maintained libp2p ادامه دارد و الگوریتم اختصاصی رمزنگاری وجود ندارد.
- T0130/T0131: Windows DPAPI CurrentUser و Android Keystore برای هویت و trust؛ explicit PeerId confirmation، persistence و fail-closed tests بررسی شدند.
- T0132/T0133: secure channel و session-key establishment به library libp2p سپرده شده؛ transport از plaintext downgrade و pinned-PeerId mismatch جلوگیری می‌کند.
- T0134: malformed identity، unknown/untrusted peer، mismatch و plaintext-only رد می‌شوند؛ تمام نسخه‌های handshake داخل library به‌طور مستقل ممیزی نشده‌اند.
- T0135: شناسهٔ request از user/device/conversation پایدار مستقل است؛ generator 128-bit، finite sample، descriptorهای بدون stable IDs و request/attempt/round correlation بررسی شدند.
- V3.1.85 Quality [42509179813] و Rendezvous [42509179886] موفق؛ HostLifecycle در Product [42509179878] موفق؛ Android و Windows jobs در snapshot هنوز در حال اجرا بودند.
- HostLifecycle یک آزمون real-model را SKIP کرد؛ هیچ inference واقعی با مدل محلی ادعا نمی‌شود.
- شواهد مستقل:
- update/Step three/UPD-S1-008/evidence/review-T0126.md
- update/Step three/UPD-S1-008/evidence/review-T0127.md
- update/Step three/UPD-S1-008/evidence/review-T0128.md
- update/Step three/UPD-S1-008/evidence/review-T0129.md
- update/Step three/UPD-S1-008/evidence/review-T0130.md
- update/Step three/UPD-S1-008/evidence/review-T0131.md
- update/Step three/UPD-S1-008/evidence/review-T0132.md
- update/Step three/UPD-S1-008/evidence/review-T0133.md
- update/Step three/UPD-S1-008/evidence/review-T0134.md
- update/Step three/UPD-S1-008/evidence/review-T0135.md
- نتیجهٔ reconciliation: **90/168 reviewed (53.57%)** و **78 pending (46.43%)**؛ UPD-S1-008 همچنان in_progress و T0128 همچنان deferred_final است.


## بازبینی محتوایی T0136 تا T0145 — 2026-11-08T18:28:01.737Z

- مبنای بررسی: c4b7076392dba6034db3d22200f8d9ab86419814 — V 3.1.86، شاخهٔ آنلاین main.
- T0136: شناسهٔ request با RNG رمزنگارانه در helper Windows و UUID در Android تولید می‌شود؛ request/attempt/round از شناسه‌های پایدار مکالمه/کاربر جدا هستند؛ تست‌های RNG و Protobuf descriptor این مرز را کنترل می‌کنند.
- T0137: replay guard، response correlation و secure peer identity در مسیر میزبان وجود دارد؛ آزمون‌های HostLifecycle replay، tamper و peer identity rejection را پوشش می‌دهند، نه حملهٔ شبکهٔ عمومی.
- T0138: unchanged؛ ADR-0019 و README/requirements/acceptance، افشای قابل‌خواندن بودن prompt و capsule برای میزبان را مستند کرده‌اند؛ Android نیز در prompt field، pre-trust dialog و Privacy page آن را تکرار می‌کند.
- T0139: Compose launcher و navigation shell واقعی با Home، Request، Connections، Privacy و conversation list/detail وجود دارد؛ build/test قبلی موفق، اما visual inspection فیزیکی و chat runtime integration هنوز جداگانه لازم‌اند.
- T0141: Room واقعی با conversation/message/request-state/draft/capsule metadata وجود دارد؛ SQLCipher و Keystore-protected DB key، persistence/reopen و fail-closed missing key.
- T0142: Room migrations 1→2→3→4→5 و legacy plaintext-to-SQLCipher migration با checkpoint, integrity check و atomic rename وجود دارد؛ migration و instrument tests وجود دارند، اما این معادل نصب و ارتقای واقعی کل برنامه نیست.
- T0143/T0144: فهرست و CRUD به Flow واقعی Room متصل‌اند و create/rename/open/delete/cascade را اجرا می‌کنند؛ تست‌های ViewModel/contract آن‌ها را پوشش می‌دهند.
- T0145: صفحه detail پیام user را در Room ذخیره می‌کند، اما source صریحاً می‌گوید درخواست به میزبان ارسال نمی‌شود و repository وضعیت TRANSPORT_UNAVAILABLE می‌دهد؛ پاسخ assistant واقعی هنوز به همان مکالمه وصل نیست. این شکاف برای UPD-S1-010 باقی است.
- T0140 در mapping دارای update_task_ids خالی است؛ بنابراین عضو ۱۶۸ شناسهٔ متأثر نیست و رکورد reconciliation برایش ایجاد نشد.
- V3.1.86 Quality [42509600997] و Rendezvous [42509601113] موفق؛ HostLifecycle در Product [42509601072] موفق؛ Windows و Android jobs در snapshot مبنا در حال اجرا بودند.
- Full Product Build V3.1.82 [42503829586] هر سه job را گذراند؛ Android 226 pass، 1 skipped، 0 failed از 227 tests؛ Windows WPF publish/startup/installer نیز موفق. کد محصول بررسی‌شده در commitهای V3.1.83 تا V3.1.86 تغییر نکرد.
- شواهد مستقل:
- update/Step three/UPD-S1-008/evidence/review-T0136.md
- update/Step three/UPD-S1-008/evidence/review-T0137.md
- update/Step three/UPD-S1-008/evidence/review-T0138.md
- update/Step three/UPD-S1-008/evidence/review-T0139.md
- update/Step three/UPD-S1-008/evidence/review-T0141.md
- update/Step three/UPD-S1-008/evidence/review-T0142.md
- update/Step three/UPD-S1-008/evidence/review-T0143.md
- update/Step three/UPD-S1-008/evidence/review-T0144.md
- update/Step three/UPD-S1-008/evidence/review-T0145.md
- reconciliation: **99/168 reviewed (58.93%)**، **69 pending (41.07%)**؛ UPD-S1-008 همچنان in_progress.


## بازبینی محتوایی T0146 تا T0150 — 2026-11-08T18:30:26.354Z

- مبنای کد: 01c2374e37b8e8d874ea06d1498991adeeefe15d — V 3.1.87، شاخهٔ main.
- T0146: validator پیام خالی و بیش‌ازحد را پیش از درج Room رد می‌کند؛ این اعتبارسنجی محلی composer است نه prompt validation نهایی در transport.
- T0147: RequestStateEntity و RequestStatusPresentation وضعیت‌های typed و cardهای UI را دارند؛ اما source chat هنوز preview بدون host transport است، پس نمایش status type تکمیل request واقعی را ثابت نمی‌کند.
- T0148: SavedStateHandle متن unsent draft را با handle بازیابی‌شده به ViewModel جدید می‌دهد؛ تست واحد دارد اما Android process kill/relaunch فیزیکی اجرا نشده.
- T0149: آزمون file-backed Room دیتابیس را می‌بندد و دوباره باز می‌کند و message/request-state را بازیابی می‌کند؛ این معادل in-place upgrade کل اپ و Keystore نیست.
- T0150: آزمون integration محلی create/send/reopen را در ViewModel/Room بررسی می‌کند؛ ارسال به میزبان ندارد و پاسخ assistant واقعی در آن نیست.
- یال‌های T0145→T0146، T0146→T0147، T0147→T0148، T0148→T0149 و T0149→T0150 دارای rationale/evidence بودند و حفظ شدند؛ downstream unreviewed edgeهای نامرتبط دست‌نخورده ماندند.
- V3.1.87 Quality [42510164070] و Rendezvous [42510164017] موفق؛ HostLifecycle در Product [42510164037] موفق؛ Android build/JVM/instrumentation و Windows installer در زمان snapshot هنوز in_progress بودند.
- Full Product Build V3.1.82 [42503829586] Android و Windows را با موفقیت ساخت؛ Android 226 pass، 1 skip، 0 failed از 227 تست.
- شواهد مستقل:
- update/Step three/UPD-S1-008/evidence/review-T0146.md
- update/Step three/UPD-S1-008/evidence/review-T0147.md
- update/Step three/UPD-S1-008/evidence/review-T0148.md
- update/Step three/UPD-S1-008/evidence/review-T0149.md
- update/Step three/UPD-S1-008/evidence/review-T0150.md
- reconciliation: **104/168 reviewed (61.90%)**، **64 pending (38.10%)**؛ UPD-S1-008 همچنان in_progress.


## بازبینی محتوایی T0161 تا T0165 — 2026-11-08T18:59:26.040Z

- **مبنای دقیق:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`.
- پنج گرهٔ legacy یعنی T0161، T0162، T0163، T0164 و T0165 به‌صورت مستقل از روی کد، آزمون‌ها، قرارداد مشترک، وابستگی‌ها و معیار پذیرش بازبینی شدند.
- وضعیت پایهٔ هر پنج گره `done` بود و بدون تغییر حفظ شد. تصمیم هر پنج مورد `re_review_required` است؛ وضعیت reconciliation آن‌ها `reviewed` خواهد بود، اما به‌دلیل وابستگی به UPD-S1-015 و بازآزمایی مصرف‌کنندگان، بازاعتبارسنجی آینده همچنان الزامی است.
- T0161: مدل DistributedRequest، شناسهٔ UUID، deadline، بودجهٔ دور/تلاش و تاریخچهٔ peer بررسی شد.
- T0162: selector عمومی تا سه میزبان را برای compatibility نگه می‌دارد؛ مسیر production first-release در runtime انتخاب و dispatch را به یک میزبان محدود می‌کند. این تفاوت صریحاً ثبت شد.
- T0163: تولید attempt ID، اعتبارسنجی payload پیش از dispatch و correlation transport بر اساس request/attempt/peer بررسی شد.
- T0164: Clock تزریق‌پذیر، timeout مستقل هر round و محدودشدن آن به deadline مطلق درخواست بررسی شد.
- T0165: قرارداد Protobuf و اعتبارسنجی correlation در Android و Windows، شامل شناسه‌ها، peer identity، model و deadline بررسی شد.
- آزمون‌های Android JVM و Keystore instrumentation، آزمون‌های Windows HostLifecycle و build هر دو محصول در Product Build دقیق همین commit، run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)، موفق شدند. Quality Gate دقیق همین commit، run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597)، نیز موفق بود.
- اجرای محلی مستقل در این محیط انجام نشد؛ clone از GitHub به‌علت خطای DNS ممکن نبود. آزمون‌های فوق از CI دقیق commit خوانده شده‌اند.
- شواهد مستقل: `review-T0161.md` تا `review-T0165.md` در مسیر `update/Step three/UPD-S1-008/evidence/`.
- هیچ کد محصول، وضعیت پایهٔ legacy، وابستگی یا edge تغییر نکرد. آزمون فیزیکی Android-to-Windows، live local inference، same Room persistence، clean install و in-place upgrade در این تلاش اجرا نشدند.
- reconciliation پس از ثبت این پنج مورد: **109/168 reviewed (64.88%)** و **59 pending (35.12%)**؛ `UPD-S1-008` همچنان `in_progress` است.


## بازبینی محتوایی T0166 تا T0170 — 2026-11-08T19:05:25.645Z

- مبنای main در شروع بازبینی: `744c5c4a8c794bd79f0073fb142b1679f4e965fe` (`V 3.1.90`). کد محصول در source baseline `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`) بررسی شد؛ تغییرات بعدی تا مبنای فعلی فقط مستندات/گراف/reconciliation بوده‌اند.
- T0166 تا T0170 جداگانه از نظر کد Kotlin، تست‌های واحد و contract، مصرف‌کنندگان UI/runtime، وابستگی‌های مستقیم و اثر وظایف UPD-S1 بازبینی شدند.
- وضعیت پایهٔ هر پنج گره `done` است و حفظ شد؛ تصمیم هر پنج `re_review_required` است، چون UPD-S1-015/017 سیاست نهایی میزبان/retry را تغییر می‌دهند و UPD-S1-010 اتصال پاسخ به همان گفت‌وگو و Room را هنوز تکمیل نکرده است.
- T0166/T0167: کارت مستقل پاسخ با کلید request/attempt و رد پاسخ تکراری پیاده شده و در صفحهٔ New Request نمایش داده می‌شود؛ این state موقت است و ذخیرهٔ پاسخ در همان conversation/Room را اثبات نمی‌کند.
- T0168: collector با coroutineهای مجزا و awaitAll تا پایان پنجرهٔ مشترک round برای همهٔ attemptهای صف‌شده منتظر می‌ماند؛ failureها جدا و typed می‌مانند.
- T0169/T0170: generic selector/dispatcher برای سه میزبان در هر round و حداکثر نه attempt سازگاری دارد؛ مسیر production first-release یک میزبان فعال در هر round، حداکثر سه تلاش ترتیبی، retry دستی و بدون auto-retry/broadcast را اجرا می‌کند. این تفاوت با عنوان legacy صریحاً ثبت شد.
- Product CI دقیق source baseline: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) شامل Android و Windows و HostLifecycle موفق؛ Quality Gate دقیق همان source: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق.
- اجرای محلی در این محیط انجام نشد، زیرا clone دسترسی DNS به GitHub نداشت. هیچ آزمون فیزیکی Android-to-Windows، پاسخ live model، همان گفت‌وگوی Room، clean install هدف واقعی یا in-place upgrade در این batch اجرا نشده است.
- شواهد مستقل: `review-T0166.md` تا `review-T0170.md` در مسیر `update/Step three/UPD-S1-008/evidence/`.
- هیچ کد محصول، وضعیت پایهٔ legacy یا edge در این بازبینی تغییر نکرد.
- reconciliation پیش از ثبت این batch: 109/168 reviewed و 59 pending؛ بعد از ثبت این پنج مورد باید 114/168 reviewed و 54 pending شود. `UPD-S1-008` همچنان `in_progress` می‌ماند.


## بازبینی محتوایی T0171 تا T0175 — 2026-11-08T19:08:16.181Z

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` (`V 3.1.92`)؛ source محصول دقیقاً از snapshot `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`) بررسی شد. commitهای بعدی تا مبنای جاری، مستندات/گراف/reconciliation بوده‌اند.
- T0171: تاریخچهٔ hostهای امتحان‌شده، حذف پیش‌فرض آن‌ها از round بعدی، allowlist محدود retry و fail-closed در نبود تاریخچهٔ معتبر بررسی شد.
- T0172: correlation کامل request/attempt/round/host/model و deadline هر دو زمان تکمیل میزبان و دریافت محلی بررسی شد؛ آزمون پاسخ stale، round mismatch و پاسخ بعد deadline وجود دارد.
- T0173: dedup کارت بر اساس requestId/attemptId با lock، کلید pending transport و replay guard میزبان بررسی شد؛ persistence پس از process restart در همان Room conversation هنوز احراز نشده است.
- T0174: اتصال دکمهٔ لغو به CancelRequest دارای شناسه‌های دقیق، parser/validator مشترک و لغو token متناظر در Windows بررسی شد؛ first release در هر round فقط یک host فعال دارد.
- T0175: status mapping، lifecycle persistence، retry دستی، requestId ثابت، attemptId جدید و exclusion میزبان قبلی بررسی شد؛ retry پس از success/cancel ممنوع و بودجه/deadline محدود است.
- آزمون‌های Android JVM/transport و Windows HostLifecycle در Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شدند؛ Quality CI دقیق همان source [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.
- اجرای محلی در این محیط انجام نشد (عدم دسترسی DNS برای clone). شواهد CI شامل loopback/native .NET host integration است؛ نه دستگاه Android واقعی، شبکهٔ عمومی، مدل inference واقعی کاربر یا ذخیرهٔ پاسخ در همان Room chat.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ تصمیم هر پنج `re_review_required` است و هیچ status قدیمی تغییر نکرد. شواهد مستقل در `review-T0171.md` تا `review-T0175.md` ذخیره می‌شود.
- reconciliation پیش از ثبت این batch: 114/168 reviewed و 54 pending؛ بعد از ثبت باید 119/168 reviewed و 49 pending شود. UPD-S1-008 همچنان `in_progress` باقی می‌ماند.


## بازبینی محتوایی T0176 تا T0180 — 2026-11-08T19:11:37.767Z

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` (`V 3.1.94`)؛ source محصول دقیقاً از `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`) بررسی شد.
- T0176: آزمون صفر، یک و سه پاسخ، duplicate، late response، timeout و cancellation وجود دارد؛ تست صریح دو پاسخ معتبر در فهرست تست‌ها پیدا نشد، بنابراین این شکاف برای re-review ثبت شد.
- T0177: سقف generic سه peer در هر round/نه تلاش با سقف first-release تک‌میزبان/سه تلاش ترتیبی تفکیک شده؛ تطبیق نهایی با UPD-S1-003/015 باقی است.
- T0178: retention میزبان به‌صورت پیش‌فرض process-session-only است؛ تاریخچهٔ پایدار فقط با رضایت جداگانهٔ مالک Windows و محافظت DPAPI فعال می‌شود. محدودیت crash dump/paging/backup در هشدار UI اعلام می‌شود.
- T0179: API تشخیصی Android و Windows فقط enumهای ثابت، outcome و duration را می‌پذیرد و سقف ۸۰ رکورد/process دارد؛ آزمون‌ها numeric schema و عدم حضور canaryهای حساس را می‌سنجند.
- T0180: پاک‌سازی بافرهای موقت در decoder/transport/lifecycle و نگهداری bounded بافرهای Windows تا ack/response یا teardown بررسی شد؛ پاک‌سازی فوری همهٔ بافرها در لحظهٔ timeout و forensic حافظه اثبات نشده است.
- Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند. اجرای محلی در این محیط به‌علت عدم دسترسی DNS ممکن نشد.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ تصمیم `re_review_required` و شواهد مستقل در `review-T0176.md` تا `review-T0180.md` ذخیره می‌شود.
- reconciliation قبل از این batch: 119/168 reviewed و 49 pending؛ پس از آن: **124/168 reviewed (73.81%)** و **44 pending (26.19%)**. UPD-S1-008 همچنان `in_progress` می‌ماند.


## بازبینی T0181 تا T0185 — 2026-11-08T19:16:57.662Z

- مبنای main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd` (`V 3.1.96`)، source محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`).
- T0181: رضایت جداگانهٔ مالک، default-off و DPAPI history بررسی شد.
- T0182: startup cleanup محدود به فایل موقت با نام/شناسهٔ معتبر، سن حداقل ۳۰ ثانیه و عدم استفادهٔ هم‌زمان است.
- T0183: AES-256-GCM و کلیدهای جداگانهٔ non-exportable Keystore برای هویت و trust، همراه fail-closed و revoke بررسی شد.
- T0184: Room/SQLCipher، حفاظت کلید با Keystore و انتقال دیتابیس plaintext قدیمی با integrity check و atomic replacement بررسی شد.
- T0185: فقط INTERNET در source manifest است؛ permission خصوصی signature-level AndroidX در APK به‌عنوان مورد dependency مجاز است و verifier در Product Build اجرا می‌شود.
- Product CI run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق بودند. اجرای محلی جداگانه در این محیط ممکن نشد.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ T0181-T0184 با تصمیم `re_review_required` و T0185 با تصمیم `unchanged` ثبت می‌شوند.
- شواهد مستقل: `review-T0181.md` تا `review-T0185.md` در همین پوشهٔ evidence.
- reconciliation پیش از این batch: 124/168 reviewed و 44 pending؛ پس از ثبت: 129/168 reviewed و 39 pending. UPD-S1-008 همچنان `in_progress` است.
- در این batch هیچ کد محصول یا وضعیت گراف اولیه تغییر نکرد؛ آزمون نصب/ارتقای واقعی Windows و Android انجام نشد.


## بازبینی محتوایی T0186 تا T0190 — 2026-11-08T19:21:07.886Z

- مبنای main: `fc006d4c7f01de25c27346fc85670d1dc503e231` (`V 3.1.98`). source محصول بررسی‌شده `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`) است؛ commitهای بعدی فقط reconciliation، گزارش و گراف را تغییر دادند.
- T0186: رضایت صریح Android پیش از ذخیره trust و هشدار مستقل Windows پیش از ایجاد peer-host lifecycle بررسی شد.
- T0187: آزمون canary اسکن محتوای حساس در فایل‌های متعلق به برنامه، logها و دیتابیس را پوشش می‌دهد؛ OS logs، crash dump، backup و ابزارهای ثالث خارج دامنه‌اند.
- T0188: threat model تفصیلی و محدودیت anonymity/PeerId/IP/traffic ثبت شده و AC-P2P-01/AC-PRIV-01 صریحاً NOT RUN هستند.
- T0189: خطاهای پروتکل typed و correlation-bound هستند؛ parser و UI Android متن غیرقابل‌اعتماد host را به پیام پایدار تبدیل نمی‌کنند.
- T0190: Room status و presentationهای structured وجود دارند، اما صفحهٔ conversation هنوز می‌گوید درخواست به host ارسال نمی‌شود؛ Windows network presenter نیز ادعای عدم پیاده‌سازی P2P دارد، با وجود lifecycle میزبان. این تناقض‌ها و عدم اتصال گفت‌وگوی معمولی به inference ثبت شد.
- Product Build دقیق source baseline [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق بود؛ Quality Gate روی main جاری [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) نیز موفق شد. Product Build جدید روی main در زمان بررسی هنوز in_progress بود؛ بنابراین آن را موفق اعلام نمی‌کنیم.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ تصمیم هر پنج `re_review_required` است و شواهد مستقل در `review-T0186.md` تا `review-T0190.md` ثبت می‌شود.
- reconciliation پیش از این batch 129/168 reviewed و 39 pending؛ پس از ثبت 134/168 reviewed و 34 pending خواهد بود.
- آزمون Android-to-Windows روی دستگاه واقعی، پاسخ مدل محلی، ذخیره در همان گفت‌وگو، clean install و in-place upgrade در این batch اجرا نشدند.


## بازبینی محتوایی T0191، T0192، T0193، T0196 و T0197 — 2026-11-08T19:24:50.032Z

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b` (`V 3.1.100`)، کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` (`V 3.1.88`).
- T0191: coordinator یک inference فعال را می‌پذیرد؛ duplicate با Busy رد می‌شود، deadline مطلق cancellation واقعی می‌سازد و failure/cancel gate را آزاد می‌کند.
- T0192: retry inference فقط در Android و با budget مشخص است؛ fallback اتصال حداکثر ۸ آدرس با هویت PeerId یکسان را امتحان می‌کند؛ Busy retryable نیست و delivery پاسخ مبهم دوباره‌فرستاده نمی‌شود.
- T0193: host lifecycle هنگام Stop ابتدا admission را می‌بندد و receive loop را cancel می‌کند؛ HostInferenceSession هنگام dispose کار جاری را cancel/drain کرده و سپس backend را dispose می‌کند.
- T0196: ظرفیت RAM/disk/backend و حالت Unknown fail-closed است؛ تست‌ها ۳۲ و ۶۴ درخواست هم‌زمان و تنها یک inference پذیرفته‌شده را بررسی می‌کنند.
- T0197: capsule با schema محدود و انتخاب محدود context ساخته می‌شود؛ UTF-8، سقف payload، JSON/metadata و request/response correlation آزمایش شده‌اند.
- Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند. اجرای محلی در این محیط انجام نشد.
- وضعیت پایهٔ هر پنج گره `done` حفظ شد و تصمیم هر پنج `re_review_required` است. T0194/T0195 در این دستهٔ affected IDs نیستند و پوشه یا رکوردی برایشان ایجاد نشد.
- reconciliation پیش از batch 134/168 reviewed و 34 pending؛ پس از batch باید 139/168 reviewed و 29 pending باشد.
- آزمون با میزبان و دستگاه واقعی، live model inference، clean install و in-place upgrade در این batch انجام نشد.


## بازبینی محتوایی T0198، T0200، T0201، T0202 و T0203 — 2026-11-08T19:31:52.816Z

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70` (`V 3.1.102`). آزمون‌های تاریخی در source `a074815979aff371650c5101c514989f42180968` (`V 3.0.922`) اجرا شده‌اند؛ source فعلی با آن snapshot یکسان نیست.
- T0198: selector/timer/dispatcher/request tests اکنون شامل سه host compatibility، first-release تک‌میزبان، budget، timeout/deadline و idempotency هستند؛ پاسخ collector صفر/یک/سه را پوشش می‌دهد و test صریح دو پاسخ همچنان پیدا نشد.
- T0200: source فعلی تست authenticated transport و native .NET peer fixture را به‌شکل one-host نگه می‌دارد؛ تست سه-processهای تاریخی در فایل فعلی transport وجود ندارد. fixture loopback و پاسخ تستی است، نه مدل واقعی یا Windows نصب‌شده.
- T0201: اجرای واقعی llama.cpp + GGUF روی Windows GitHub runner موفق بود. llama.cpp b11443 archive SHA `98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6`؛ executable SHA `4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968`؛ GGUF revision `a6b7826ed969c3926a98359e39912af137b464cb`، size 105454144 bytes، SHA `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684`. Run [41886745964](https://github.com/artinrahimii/project/actions/runs/41886745964) موفق بود.
- T0202: وضعیت legacy `deferred_final` حفظ شد. workflow دستی LAN هنوز اجرا نشده؛ runner ویندوز `whitep-lan` و Android فیزیکی روی LAN شرط رفع مانع است.
- T0203: اجرای تاریخی سه .NET process روی loopback و یک runner، با response text یکسان، correlation مستقل host/attempt را آزمود؛ اما test multi-process در source فعلی وجود ندارد و production first-release یک میزبان در هر round dispatch می‌کند. پذیرش current multi-host انجام‌شده محسوب نمی‌شود.
- Product Build current HEAD در زمان بررسی هنوز `in_progress` بود؛ run تاریخی 41886746018 را فقط به‌عنوان شاهد source قدیمی ثبت می‌کنیم. اجرای محلی این environment انجام نشد.
- وضعیت پایهٔ T0198/T0200/T0201/T0203 `done` و T0202 برابر `deferred_final` بدون تغییر حفظ شد؛ تصمیم هر پنج `re_review_required` و review_status برابر `reviewed` ثبت می‌شود.
- شواهد مستقل در `review-T0198.md`، `review-T0200.md`، `review-T0201.md`، `review-T0202.md` و `review-T0203.md` ذخیره شدند. Reconciliation از 139/168 به 144/168 reviewed و از 29 به 24 pending خواهد رسید.


## بازبینی محتوایی T0204 تا T0208 — 2026-11-08T19:37:38.773Z

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916` (`V 3.1.104`).
- T0204: اجرای تاریخی دور دوم پس از سه peer بی‌پاسخ در run [41911308402](https://github.com/artinrahimii/project/actions/runs/41911308402) موفق بود؛ ولی تست سه+سه peer در source فعلی وجود ندارد و current first-release تک‌میزبانه است.
- T0205: cancellation مسیر peer-host تا HTTP generation operation با request/attempt correlation آزموده شد؛ لغو HTTP ثابت است، اما توقف داخلی محاسبات process جداگانهٔ llama.cpp اثبات نشده. run [41915992536](https://github.com/artinrahimii/project/actions/runs/41915992536) موفق.
- T0206: source service، metadata DTO و Android directory client با Blob SHA دقیق از source baseline تا main تغییر نکرده‌اند؛ فیلد prompt/capsule/capsule رد می‌شود و directory پس از رد خالی است. تصمیم `unchanged`.
- T0207: run تاریخی clean build [41925547942](https://github.com/artinrahimii/project/actions/runs/41925547942) موفق بود؛ اما product-build.yml و app/build.gradle.kts از آن commit تا main فعلی تغییر کرده‌اند. run exact-current-head [42515602200](https://github.com/artinrahimii/project/actions/runs/42515602200) هنوز در حال اجرا بود؛ Windows clean build و HostLifecycle success، Android و installer هنوز ناتمام.
- T0208: artifactهای تاریخی شامل Android 223/224 passed و 1 skipped، و Windows HostLifecycle coverage 85.70% و 67 PASS/1 SKIP بود؛ current validator و workflow نیز بازبینی شدند. روی current run، HostLifecycle coverage artifact 111,852 bytes آپلود شده، اما Android coverage و سایر artifactها هنوز کامل نشده‌اند.
- وضعیت پایهٔ T0204/T0205/T0207/T0208 `done` و T0206 `done` بدون تغییر حفظ شد؛ تصمیم‌ها به‌ترتیب `re_review_required`, `re_review_required`, `unchanged`, `re_review_required`, `re_review_required` هستند. هیچ status در گراف اولیه تغییر نکرد.
- شواهد مستقل در `review-T0204.md` تا `review-T0208.md` قرار گرفتند. Reconciliation از 144/168 به 149/168 reviewed و از 24 به 19 pending تغییر می‌کند.


## بازبینی محتوایی T0209 تا T0213 — 2026-11-08T20:18:05.645Z

- مبنای بررسی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` (`V 3.1.106`)؛ شواهد current-head یا run تاریخی با source workflow یکسان و artifact قابل‌تأیید بررسی شدند.
- T0209: installer واقعی روی Windows runner ساخته، نصب، smoke-launch و uninstall شد؛ 83 package در license inventory و یک model license notice ثبت شد. Artifact `13631034913` بارگذاری شد. هشدار padding metadata در PE ثبت شد، اما Publisher نصب‌شده به‌طور مستقل PASS بود.
- T0210: executable نصب‌شده روی Windows clean runner با `--smoke-test` اجرا شد؛ MainWindow و کنترل‌های الزامی بررسی و خروج تمیز تأیید شد.
- T0211: APK روی Android Emulator نصب و MainActivity اجرا شد؛ PID=4319 و topResumedActivity ثبت شد. یک آزمون non-loopback LAN عمداً SKIPPED بود؛ ارتباط فیزیکی Android↔Windows اثبات نشده است.
- T0212: production signing در run `42114568183` روی `179efa9fe07c3ba007885e9e3198eba620092617` موفق بود؛ 15 مرحله، fingerprint verification، upload artifact و key cleanup موفق شدند. Blob workflow امضا با HEAD جاری یکسان است.
- T0213: سه checksum مستقل در manifest تأیید و Draft Release `436740935` با چهار asset موجود است. run قدیمی در final Draft lookup شکست خورد؛ اصلاح بعدی Quality Gate را گذرانده، اما اجرای tag جدید پس از اصلاح هنوز انجام نشده است.
- هر پنج رکورد: تصمیم `re_review_required`، review_status برابر `reviewed`، baseline `done` حفظ، status_changed=false. دلیل re-review وابستگی‌های تحویل UPD-S1-021 و هویت بصری UPD-S1-024 است.
- شواهد مستقل در evidence پوشهٔ همین وظیفه درج شده‌اند. Reconciliation پس از این تلاش: 154/168 reviewed (91.67%) و 14/168 pending (8.33%).
- commit مرجع این تلاش پس از ساخت ثبت خواهد شد؛ فیلد review_commit_sha فعلاً null است و SHA حدس زده نمی‌شود.


### اتصال بازبینی‌ها به commit ثبت‌شده

- commit ثبت شواهد پنج بازبینی: `4eba36c5fd9196fb616b5c0bd888c010cefeea58` — `V 3.1.107`، شاخهٔ `main`، تأییدشده در GitHub.
- فیلد `review_commit_sha` رکوردهای T0209 تا T0213 با همین SHA واقعی پر شده است؛ این تغییر فقط metadata ردیابی است و وضعیت baseline یا تصمیم reconciliation را تغییر نمی‌دهد.


## Current-main reconciliation review: T0215–T0229 — 2026-11-09

Review baseline: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`). The fourteen remaining legacy IDs were reviewed individually against current source, direct consumers, tests, workflows and acceptance criteria. Each has a separate evidence file.

| Legacy ID | Baseline status | Decision | Finding |
|---|---|---|---|
| T0215 | `in_progress` | `re_review_required` | Release licence inventory and sample-model notice are built into the Windows installer pipeline; current-head Product Build passed |
| T0216 | `not_started` | `re_review_required` | The existing Windows installer document describes build/install/package details, not a complete first-run, backend/model setup, opt-in participation, peer trust, stop-participation and troubleshooting guide |
| T0217 | `not_started` | `re_review_required` | There is no accurate Android user guide for the requested host-connected ordinary-chat flow |
| T0218 | `not_started` | `re_review_required` | Current-head Quality, Product Build and Rendezvous container workflows passed, but acceptance still lacks physical Android-to-Windows inference, a real target-host local-model session, dual-platform in-place upgrade and same-tag release validation |
| T0219 | `not_started` | `re_review_required` | The ordinary ConversationDetailScreen send path calls the local-only ConversationMessagesViewModel, which writes user messages to Room and does not invoke the host runtime |
| T0221 | `not_started` | `re_review_required` | Catalog validation, bounded HTTPS download, SHA-256/GGUF verification and atomic update of an existing model are real |
| T0222 | `not_started` | `re_review_required` | Room v5 and migration chain, conversation/message persistence, and context-capsule edit/save/delete are implemented; migration tests preserve records from a v1 DB |
| T0223 | `not_started` | `re_review_required` | Android request runtime and Windows local inference components exist separately, but ordinary Android chat is still local-only |
| T0224 | `not_started` | `re_review_required` | The approved V1 |
| T0225 | `not_started` | `re_review_required` | The current runtime implements bounded sequential attempts, unique request/attempt/round correlation, shared round deadlines, timeout collection and cancellation handling |
| T0226 | `not_started` | `re_review_required` | Peer identity validation, explicit trust, OS/Keystore-protected persistence, libp2p Noise/TLS secure channels and replay fencing are implemented and have automated tests |
| T0227 | `not_started` | `re_review_required` | Rendezvous stores expiring peer IDs, direct addresses and capability metadata only; request/response transport is direct authenticated libp2p |
| T0228 | `not_started` | `re_review_required` | A single consolidated current release-limitations register was absent |
| T0229 | `not_started` | `re_review_required` | Current-head CI proves clean hosted-checkout validation, Android/Windows builds, Windows installer smoke, HostLifecycle tests and Rendezvous container smoke |

### Outstanding product limitations

- Ordinary Android conversation remains local-only and is not connected to the request runtime.
- Physical Android-to-Windows inference on a real device and installed Windows host has not been run.
- The current-head Product Build proves CI builds and runner smoke, not a target-device install or live target-host model session.
- Model catalogue selection does not perform initial install/activation; removal leaves the model file on disk.
- Room migrations and capsule CRUD are implemented, but physical reboot and in-place upgrade preservation are unverified.
- The approved first-release policy is one host at a time with up to three sequential attempts; the prior three-host-per-round task is superseded.
- Peer trust, libp2p secure channels and metadata-only rendezvous have source/tests; physical security acceptance and external provider-log audit remain unrun.
- A consolidated `docs/release-limitations.md` was added, but it does not approve release.

Exact-head CI before this change: Quality Gate [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303), Product Build [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320), Rendezvous container [42519571350](https://github.com/artinrahimii/project/actions/runs/42519571350); all passed. These runs precede this reconciliation commit, so the new commit must be validated separately.

**Record-level reconciliation: 168/168 reviewed (100%); 0 record pending.** All legacy baseline statuses and original Step Three `update_impact` markers remain unchanged. This is not the completion percentage of the 24 update tasks. `UPD-S1-008` remains `in_progress` because 70/80 Update graph edges are still `unreviewed` and the dependency audit remains open.


## Dependency-edge audit — 2026-11-09

- Exact source main commit: `ae7725fd30f98a75968edb78082fe0d64ce409cd` (`V 3.1.109`). All 70 previously `unreviewed` edges were checked against the corresponding `prerequisites` arrays in approved `V1.10.json`; every edge was an explicit prerequisite, and all referenced source/test evidence paths exist in the exact Git tree.
- Classification of the 70 reviewed edges: 19 `hard_dependency`, 31 `contract_or_interface_dependency`, 20 `sequencing_only`. No edge was deleted, no order was relaxed, and the 13 execution waves were preserved.
- Combined edge totals: **80/80 validated** — 19 hard, 34 contract/interface, 27 sequencing-only; 0 unreviewed and 0 disputed.
- Edge-level rationale and evidence: `update/Step three/UPD-S1-008/evidence/dependency-edge-review.md`.
- All 168 reconciliation rows now contain decisions/evidence and the actual review commit SHA `ae7725fd30f98a75968edb78082fe0d64ce409cd`; baseline Step Three task statuses remain unchanged.
- This attempt is partial: the old graph's 168 `update_impact` markers and global summary have not yet been synchronized with the ledger, and the old-graph contract test must be updated. `UPD-S1-008` remains `in_progress` until both graph guards pass after that synchronization.
- Product Build for exact commit `V 3.1.109` is still in progress as of the latest poll; Windows and HostLifecycle jobs have passed, while Android is still uploading coverage/finishing its final steps. This is not physical Android-to-Windows acceptance.

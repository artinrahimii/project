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

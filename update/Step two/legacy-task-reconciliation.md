# وضعیت بازبینی وظایف قدیمی — UPD-S1-008

## خط مبنا
- مخزن: `artinrahimii/project`
- commit: `795a2de651f4e43133f352699e34623a3a10df0a` (`V 3.1.56`)
- گراف قدیمی: `step three/project-graph.md` — 230 گره / 422 یال
- فایل reconciliation: `update/Step two/legacy-task-reconciliation.json`
- وظایف متأثر: 168 شناسهٔ یکتا از 230 شناسه.

## وضعیت پایهٔ 168 وظیفه
- `done`: 152
- `deferred_final`: 2
- `in_progress`: 1
- `not_started`: 13

## نگاشت به‌روزرسانی
- `UPD-S1-001`: 7 شناسهٔ قدیمی
- `UPD-S1-002`: 13 شناسهٔ قدیمی
- `UPD-S1-003`: 23 شناسهٔ قدیمی
- `UPD-S1-004`: 26 شناسهٔ قدیمی
- `UPD-S1-005`: 19 شناسهٔ قدیمی
- `UPD-S1-006`: 3 شناسهٔ قدیمی
- `UPD-S1-007`: 24 شناسهٔ قدیمی
- `UPD-S1-008`: 4 شناسهٔ قدیمی
- `UPD-S1-009`: 11 شناسهٔ قدیمی
- `UPD-S1-010`: 14 شناسهٔ قدیمی
- `UPD-S1-011`: 2 شناسهٔ قدیمی
- `UPD-S1-012`: 17 شناسهٔ قدیمی
- `UPD-S1-013`: 19 شناسهٔ قدیمی
- `UPD-S1-014`: 13 شناسهٔ قدیمی
- `UPD-S1-015`: 23 شناسهٔ قدیمی
- `UPD-S1-016`: 18 شناسهٔ قدیمی
- `UPD-S1-017`: 32 شناسهٔ قدیمی
- `UPD-S1-018`: 14 شناسهٔ قدیمی
- `UPD-S1-019`: 16 شناسهٔ قدیمی
- `UPD-S1-020`: 13 شناسهٔ قدیمی
- `UPD-S1-021`: 15 شناسهٔ قدیمی
- `UPD-S1-022`: 12 شناسهٔ قدیمی
- `UPD-S1-023`: 32 شناسهٔ قدیمی
- `UPD-S1-024`: 18 شناسهٔ قدیمی

## کنترل‌های انجام‌شده
- همهٔ 168 شناسه در گراف قدیمی وجود دارند.
- هر شناسه دقیقاً یک رکورد reconciliation دارد.
- هر گره نشانگر `update_impact` دارد.
- وضعیت پایه در گراف و رکورد و نشانگر یکسان است.
- تمام ارتباط‌های Update در رکورد و نشانگر همسان‌اند.
- تمام 605 مسیر خروجی ثبت‌شده و 562 مسیر شواهد فایل‌محور موجودند.
- گراف قدیمی چرخه ندارد؛ یک وابستگی متنی metadata در T0001 برای حفظ سابقه هشدار داده می‌شود.

## وضعیت بازبینی محتوایی
**149/168 بازبینی محتوایی ثبت شده‌اند.** تصمیم `unchanged` برای T0012، T0013، T0015، T0016، T0019 و T0020؛ و تصمیم `re_review_required/reviewed` برای شناسه‌های بازبینی‌شده در JSON مرجع، از T0027 تا T0109 (به‌جز شناسه‌های خارج از فهرست متأثر) ثبت شده است. 24 رکورد همچنان `pending` هستند.

## سیاست تغییر وضعیت
هیچ وضعیت پایهٔ قدیمی تغییر نکرده است. تصمیم‌ها و وضعیت بررسی هر شناسه در JSON مرجع ثبت می‌شوند. در snapshot جاری 129 مورد reviewed و 39 مورد pending است؛ نشانگرهای ثابت `update_impact` تا تکمیل هر 168 بررسی pending می‌مانند. نشانگرهای ثابت `update_impact` در گراف اولیه مطابق قرارداد تا تکمیل کل reconciliation pending می‌مانند؛ وضعیت واقعی هر بررسی در JSON ثبت شده است.


## نتیجهٔ تکمیلی در V 3.1.56

- مبنای بررسی: `795a2de651f4e43133f352699e34623a3a10df0a` — `V 3.1.56`.
- `T0012`: تصمیم `unchanged`؛ وضعیت پایهٔ `done` حفظ شد.
- `T0013`: تصمیم `unchanged`؛ وضعیت پایهٔ `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0012-T0013.md`؛ quality CI دقیق [42480966736](https://github.com/artinrahimii/project/actions/runs/42480966736) موفق.
- جمع‌بندی: 2 reviewed، 166 pending؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0019 روی V 3.1.59

- مبنای بررسی: `32015598e6ff38f3984efb0bc8c5d1797b99cdef` — `V 3.1.59`.
- تصمیم: `unchanged`؛ وضعیت پایهٔ `done` حفظ شد؛ وابستگی‌های `T0018` و `T0010` هر دو `done` هستند.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0019.md` و [quality CI 42481932091](https://github.com/artinrahimii/project/actions/runs/42481932091).
- نتیجهٔ تصمیم معماری: direct P2P مسیر دادهٔ مصوب است؛ rendezvous فقط control-plane metadata است؛ نبود پیاده‌سازی کامل Android-to-Windows و اجرای `AC-P2P-01` صریحاً حفظ شده است.
- جمع‌بندی: 3 reviewed، 165 pending؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0020 روی V 3.1.60

- مبنای بررسی: `c31ac02d39993965bd693b53f626d5cb09be7b2c` — `V 3.1.60`.
- تصمیم `unchanged`؛ وضعیت پایهٔ `done` حفظ شد.
- دفتر تصمیم‌های فناوری، ADR-0001 تا ADR-0003 و پیوندهای configuration/source/test بررسی شدند.
- آزمون‌های traceability و build-contract در quality run [42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315) موفق شدند؛ full suite شامل 479 تست، compileall و هر دو graph guard نیز موفق بود.
- این بازبینی اثبات اجرای مدل زنده یا Android-to-Windows نیست؛ `AC-LOCAL-01/02` و `AC-P2P-01` جدا و NOT RUN باقی می‌مانند.
- مجموع: 4 reviewed، 164 pending؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی تکمیلی T0015 و T0016 روی V 3.1.61

- `T0015`: انتخاب پشتهٔ C#/.NET 10/WPF و فایل اجرایی پروژه بررسی شد؛ پیش‌نیازهای `T0014`، `T0013` و `T0010` در گراف قدیمی `done` هستند. build/publish، startup smoke، installer و HostLifecycle در run [42482227320](https://github.com/artinrahimii/project/actions/runs/42482227320) موفق شدند.
- `T0016`: پشتهٔ Kotlin/Jetpack Compose/Gradle و ساختار پروژه بررسی شد؛ `T0015` و پیش‌نیازهای زنجیره‌ای آن `done` هستند. lint، debug/release build و JVM tests موفق شدند؛ Keystore instrumentation هنگام آخرین مشاهده هنوز در حال اجرا بود.
- تصمیم هر دو `unchanged` است؛ وضعیت پایهٔ legacy حفظ شد. هیچ نشانگر ثابت `update_impact` در گراف Step Three تغییر نکرد.
- Quality run [42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315) موفق بود. این بررسی، آزمون دستگاه فیزیکی، E2E Android-to-Windows، هویت بصری مشترک یا in-place upgrade را اثبات نمی‌کند.
- مجموع reconciliation: 6 reviewed، 162 pending؛ پیشرفت 3.57%.


## بازگشایی T0020 پس از کشف شناسهٔ ADR تکراری

در ممیزی V3.1.63 کشف شد دو تصمیم مستقل شناسهٔ ADR-0003 دارند. بازبینی قبلی حفظ شده، اما رکورد T0020 تا اصلاح شماره‌گذاری و اجرای آزمون یکتایی در CI دقیق، `pending/re_review_required` است. وضعیت پایهٔ `done` حفظ شده است. جزئیات: `update/Step three/UPD-S1-008/evidence/review-T0020-ADR-uniqueness.md`. شمار فعلی: 5/168 reviewed و 163/168 pending.


## بستن بازبینی مجدد T0020 پس از V 3.1.64

- commit بررسی‌شده: `365c734e178013fc6ea9499bf08855e91f5d0cbe` — `V 3.1.64`.
- Quality CI دقیق همان commit: [run 42483596179](https://github.com/artinrahimii/project/actions/runs/42483596179) — **success**.
- آزمون جدید یکتایی شناسه‌های ADR، suite کامل Python، compileall و هر دو graph guard موفق شدند.
- تصمیم T0020: `unchanged`؛ وضعیت پایهٔ `done` حفظ شد. SHA بررسی‌شده در `review_commit_sha` فایل JSON ثبت شده است.
- نتیجهٔ جاری: 6/168 reviewed، 162/168 pending؛ `UPD-S1-008` همچنان `in_progress`.


## بازبینی T0199 و T0214 روی V 3.1.67

- مبنای هر دو بازبینی: `fe489b7f141a9a969f550b851093e3fcf45c8a19` — `V 3.1.67`.
- `T0199`: قرارداد canonical Protobuf، round-trip دو زبان، 11 fixture مشترک، پیام‌های cancel/busy/unsupported/protocol-error، فیلدهای attempt/round، محدودیت‌های ModelCapability و ممنوعیت دادهٔ prompt/response/history در metadata discovery بررسی شدند. Quality CI دقیق V3.1.67 (run [42484306209](https://github.com/artinrahimii/project/actions/runs/42484306209)) موفق شد؛ Product CI منطبق با کد C#/Kotlin در V3.1.61 (run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544)) نیز سه job Windows/Android/HostLifecycle را موفق تکمیل کرد.
- `T0214`: README، آزمون‌های تطبیق فرمان، تست reproducible build و workflowهای clean checkout بررسی شدند. Product run فوق خروجی Windows host/publish/installer و Android APK را ایجاد کرد. digestهای ثبت‌شده مربوط به آرشیوهای GitHub artifact هستند، نه checksum باینری داخلی APK/installer.
- Product build دقیق V3.1.67 (run [42484306205](https://github.com/artinrahimii/project/actions/runs/42484306205)) هنگام ثبت بازبینی هنوز `in_progress` بود؛ نتیجهٔ موفق برای آن ادعا نمی‌شود.
- تصمیم T0199 برابر `re_review_required` به‌دلیل توسعهٔ آزمون‌ها در UPD-S1-019؛ تصمیم T0214 برابر `re_review_required` به‌دلیل بسته‌بندی، مستندات و in-place upgrade در UPD-S1-021 است. وضعیت پایهٔ هر دو `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0199.md` و `update/Step three/UPD-S1-008/evidence/review-T0214.md`.
- نتیجه: **8/168 reviewed (4.76%)، 160 pending (95.24%)**؛ UPD-S1-008 همچنان `in_progress`.


## تأیید پس از commit — V 3.1.68

- شواهد T0199 و T0214، reconciliation و تاریخچهٔ اجرا روی شاخهٔ آنلاین `main` در commit `851753e3d1e668714fbc020a8d6437ecd6afae0a` با پیام `V 3.1.68` ثبت شدند.
- Quality gate دقیق همین commit: [run 42484508893](https://github.com/artinrahimii/project/actions/runs/42484508893) — **success**؛ اعتبارسنجی گراف به‌روزرسانی، گراف قدیمی، مسیرهای شواهد و آزمون‌های Python موفق شدند.
- Product build دقیق همین commit: [run 42484508866](https://github.com/artinrahimii/project/actions/runs/42484508866) در آخرین بررسی `in_progress` بود؛ هیچ نتیجهٔ نهایی برای آن ادعا نمی‌شود.
- reconciliation: **8/168 reviewed** و **160 pending**؛ تصمیم‌های T0199/T0214 برای بازبینی مجدد پس از UPD-S1-019/021 حفظ شده‌اند.


## بازبینی شش وظیفهٔ قدیمی دیگر روی V 3.1.69

- مبنای بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`.
- `T0027`: README و فرمان‌های restore/build/test/package با آزمون فرمان‌ها و workflowهای clean-checkout تطبیق داده شدند؛ تصمیم `re_review_required` برای بازبینی مجدد پس از UPD-S1-021.
- `T0032`: قرارداد InferenceResponse و round-trip دو زبان با fixtureهای مشترک بررسی شد؛ بازبینی مجدد پس از تصمیم streaming در UPD-S1-004 و تغییرات runtime در UPD-S1-017 لازم است.
- `T0033`: HostAnnouncement/HostCapabilities/ModelCapability و منع prompt/response/user identity/history در metadata discovery بررسی شد؛ بازبینی مجدد پس از UPD-S1-017.
- `T0034`: CancelRequest، HostBusy، UnsupportedModel و ProtocolError و correlation IDs بررسی شدند؛ assertionهای جدید attempt/round در CI موفق‌اند؛ بازبینی مجدد پس از UPD-S1-017.
- `T0035`: سیاست version `1.0.0` و نگاشت typed invalid/unsupported version errors به schema و HostLifecycle tests پیوند داده شد؛ بازبینی مجدد پس از UPD-S1-017.
- `T0036`: تولید C# و Kotlin از یک schema واحد و مصرف generated types بررسی شد؛ build/round-trip دو پلتفرم در Product CI موفق است؛ بازبینی مجدد پس از UPD-S1-017.
- Quality run دقیق V3.1.69 [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) موفق بود. Product run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) روی snapshot بدون تغییر در schema/consumer/build نسبت به V3.1.69 سه job Windows/Android/HostLifecycle را موفق تکمیل کرد؛ Product run دقیق V3.1.69 هنوز در صف/اجرا بود.
- وابستگی‌های مستقیم قدیمی هر شش وظیفه در گراف Step Three `done` هستند؛ هیچ وضعیت پایه، وابستگی یا نشانگر ثابت `update_impact` تغییر داده نشد.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0027.md`, `update/Step three/UPD-S1-008/evidence/review-T0032.md`, `update/Step three/UPD-S1-008/evidence/review-T0033.md`, `update/Step three/UPD-S1-008/evidence/review-T0034.md`, `update/Step three/UPD-S1-008/evidence/review-T0035.md`, `update/Step three/UPD-S1-008/evidence/review-T0036.md`.
- نتیجه: **14/168 reviewed (8.33%)، 154 pending (91.67%)**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی T0041 بر مبنای V 3.1.70

- commit بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df`; Quality run دقیق [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق شد.
- Blobهای App.xaml، App.xaml.cs، MainWindow XAML/code-behind، آزمون `test_wpf_entrypoint.py` و `product-build.yml` با Product commit `a3fa1f54d99db809cc670ece1cfc244239f6a727` یکسان‌اند. Product run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) روی آن commit با موفقیت کامل سه job Windows، Android و HostLifecycle را اجرا کرد؛ smoke اختصاصی T0041 نیز در [39862296386](https://github.com/artinrahimii/project/actions/runs/39862296386) موفق بود.
- تصمیم T0041 برابر `re_review_required` است، چون UPD-S1-024 هنوز شروع نشده و هویت بصری/صفحهٔ اصلی Windows را متأثر می‌کند. وضعیت پایهٔ `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0041.md`. جمع reconciliation: 15/168 reviewed و 153 pending.


## بازبینی T0042 — تفکیک لایه‌های Host — V 3.1.70

- کد بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df`; HEAD جاری در زمان بازبینی: `a259b29171131ce4ec17fecaadca7c24cc7523f2`.
- پروژه‌های Domain، Application، Infrastructure، Windows UI و Shared.Contracts و جهت وابستگی‌ها ممیزی شدند؛ آزمون `tests/test_host_layer_projects.py` سه قرارداد مرزی را پوشش می‌دهد.
- Quality CI دقیق V3.1.70 [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق بود. در Product run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793)، Windows WPF و HostLifecycle موفق شدند؛ Android job هنوز in_progress بود. Product build اولیهٔ T0042 [39907751958](https://github.com/artinrahimii/project/actions/runs/39907751958) به‌طور کامل موفق بود.
- تصمیم T0042 برابر `re_review_required` ثبت شد، زیرا UPD-S1-012 و UPD-S1-024 هنوز اجرا نشده و UI/منابع میزبان را تغییر خواهند داد. وضعیت پایهٔ `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0042.md`. جمع فعلی: 16/168 reviewed و 152 pending.


## بازبینی T0043 بر مبنای V 3.1.70

- منبع کد/آزمون: `8e9aacc84a37e14f04baf48ccd691b59d5de59df`; HEAD فعلی: `d22f96dfaf1997593ac3907f2870713804247cb9`.
- Quality CI دقیق [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق شد؛ Product CI دقیق [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) نیز Windows WPF، Android APK و HostLifecycle را با موفقیت تکمیل کرد.
- HostLifecycle.cs و test project فعلی از نظر Blob SHA با sourceهای اجراشده در Product CI یکسان‌اند؛ state machine پنج‌حالته و مسیرهای مجاز/غیرمجاز در آزمون پوشش داده شده‌اند.
- تصمیم T0043 برابر `re_review_required` است، زیرا UPD-S1-012 هنوز اجرا نشده و opt-in/stop و وضعیت‌های مشارکت را در میزبان ادغام خواهد کرد. وضعیت پایهٔ `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0043.md`. جمع فعلی: 17/168 reviewed، 151 pending.


## بازبینی T0044 — ماتریس HostLifecycle — V 3.1.70

- source/test commit: `8e9aacc84a37e14f04baf48ccd691b59d5de59df`; HEAD فعلی: `fd15be99a60d34fc4f08cc21857b436f1e747884`.
- state machine پنج‌حالته و ماتریس مجاز/غیرمجاز در source و HostLifecycle tests بررسی شدند.
- Quality [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) و Product [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) موفق؛ سه job ویندوز، اندروید و HostLifecycle در Product CI موفق شدند.
- تصمیم T0044 برابر `re_review_required` است، چون UPD-S1-012 هنوز opt-in/stop behavior را اضافه نکرده و ممکن است transitionهای lifecycle را متأثر کند. baseline `done` حفظ شد.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0044.md`. مجموع: 18/168 reviewed و 150 pending.


## بازبینی T0045 تا T0048 — V 3.1.74

- `T0045`: schema فعلی تنظیمات میزبان نسخهٔ ۲، مهاجرت schema نسخهٔ ۱، اعتبارسنجی endpoint/model/timeout/capacity و ذخیره‌سازی اتمیک بررسی شد. آزمون‌های اختصاصی persistence از harness اصلی اجرا می‌شوند.
- `T0046`: status panel از HostStatus و ظرفیت واقعی استفاده می‌کند؛ direct P2P را پیاده‌سازی‌نشده اعلام می‌کند و Online کاذب نمایش نمی‌دهد.
- `T0047`: نگاشت خطاهای typed و پیام‌های ثابت بررسی شد؛ آزمون‌های C# مسیرها و tokenهای محرمانه را به‌عنوان ورودی استفاده می‌کنند و عدم افشا را assert می‌کنند. یال قدیمی `T0040 → T0047` با حفظ `type=blocks` به `sequencing_only/validated` طبقه‌بندی شد، چون mapper از enum مشترک استفاده می‌کند اما parser/validator تولیدشده در T0040 را مستقیماً مصرف نمی‌کند؛ rationale در `edge_change_log` ثبت شد.
- `T0048`: شکست startup، جلوگیری از Ready کاذب، retry و recovery، cancellation/drain و cleanup زمان بستن WPF بررسی شد.
- تصمیم هر چهار شناسه `re_review_required` است؛ وضعیت‌های پایهٔ `done` تغییر نکردند. T0045/T0047/T0048 پس از UPD-S1-012 و T0046 پس از UPD-S1-012 و UPD-S1-024 دوباره بررسی می‌شوند.
- Quality Gate دقیق V3.1.74، run [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109)، موفق بود. Product run source-identical V3.1.70، [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793)، هر سه job Windows، Android و HostLifecycle را موفق تکمیل کرد. Product run دقیق V3.1.74، [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134)، هنگام آخرین بررسی هنوز `in_progress` بود.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0045.md` تا `review-T0048.md`. تعداد reconciliation اکنون **22/168 reviewed**، **146 pending** و **13.10%** است؛ UPD-S1-008 همچنان `in_progress`.
- آزمون مدل محلی واقعی، Android-to-Windows فیزیکی، Room E2E، نصب تمیز و in-place upgrade در این بازبینی اجرا نشده‌اند.


## بازبینی محتوایی T0049 تا T0055 — V 3.1.76

- مبنای کد: `6c77fded48253e0b9ddbadd37033e788a43944a5`، commit دقیق `V 3.1.76`.
- هفت وظیفهٔ `T0049` تا `T0055` مستقل در برابر source، test، acceptance history و مصرف‌کنندگان فعلی بررسی شدند.
- وضعیت پایهٔ هر هفت وظیفه `done` باقی ماند؛ تصمیم هرکدام `re_review_required` و review_status در JSON برابر `reviewed` ثبت شد.
- UPD-S1-013 هنوز اجرا نشده است. UPD-S1-024 نیز T0055 را متأثر می‌کند. T0050 صرفاً display-adapter inventory را کشف می‌کند و GPU compute/VRAM را اثبات نمی‌کند؛ T0051 estimator محدود RAM است، نه تأیید کامل مدل.
- شواهد: `update/Step three/UPD-S1-008/evidence/review-T0049.md` تا `review-T0055.md`.
- Quality CI دقیق HEAD [42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) و Product CI دقیق HEAD [42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) موفق بودند. Product CI شامل Windows WPF، Android و HostLifecycle است؛ ۶۷ PASS و یک SKIP اختیاری real-model ثبت شد.
- جمع reconciliation: **29/168 reviewed (17.26%)**، **139 pending**. هیچ وضعیت قدیمی تغییر نکرد. نشانگرهای `update_impact` در گراف قدیمی طبق قرارداد فعلی تا تکمیل کل reconciliation pending باقی می‌مانند؛ نتیجهٔ فردی در JSON مرجع ثبت می‌شود.
- آزمون Android-to-Windows فیزیکی، live-model integration در همین run، clean install هدف و in-place upgrade در این دسته اجرا نشده‌اند.


## بازبینی محتوایی T0056 تا T0060 — V 3.1.77

- مبنای کد: `a19595c475752832fdd3701e109dc0e1a088721f` — `V 3.1.77`.
- T0056: ظرفیت نامشخص در HostCapabilities و WPF به Unknown و عدم آمادگی ختم می‌شود؛ وضعیت پایه done حفظ شد.
- T0057 و T0058: قرارداد و adapter IInferenceEngine وجود دارند، اما composition فعال MainWindow از IInferenceBackend و LlamaCppServerBackend استفاده می‌کند؛ adapter در مسیر اصلی پنجره به‌کارگرفته نشده است. این مغایرت برای UPD-S1-023 ثبت شد.
- T0059: launcher امن در MainWindow استفاده می‌شود؛ executable version/SHA-verified، فایل GGUF، loopback، argv جدا و shutdown process-tree دارد. تست فعلی argument construction را بررسی می‌کند، نه inference واقعی.
- T0060: backend از /health استفاده می‌کند و HostInferenceSession پیش از Ready، health را بررسی می‌کند؛ شکست health آزموده شده است.
- هر پنج baseline status برابر done ماندند؛ تصمیم‌ها re_review_required و رکوردها reviewed هستند؛ UPD-S1-023 باید قرارداد مدل/backend را بازبینی کند.
- Quality CI دقیق [42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق بود. Product Build دقیق V3.1.77 هنگام این ثبت هنوز در حال اجرا بود؛ موفق فرض نشده است.
- جمع reconciliation: **34/168 reviewed (20.24%)**، **134 pending**. نشانگرهای قدیمی تا بسته‌شدن کل reconciliation pending می‌مانند.


## بازبینی محتوایی T0061 تا T0065 — V 3.1.78

- مبنای کد: `bc9882a8ad1fcd29b6a2e42e31509a76e926f2f6` — `V 3.1.78`.
- T0061/T0062: POST /v1/chat/completions، health/model probing، timeout و پاسخ نامعتبر در backend محلی بررسی شدند. گزارش‌های exact-commit live-model runs 40077575618 و 40078502151 پاسخ non-empty از مدل GGUF pin شده را ثبت کرده‌اند.
- T0063/T0064: آزمون opt-in generation cancellation و process stop در run 40092245411 روی commit `694b0fcf847a9a2087af292910112c9c041d86df` موفق ثبت شده؛ این شواهد تاریخی‌اند و نتیجهٔ V3.1.78 نیستند.
- T0065: source-level guard، endpointها و SDKهای cloud-provider شناخته‌شده را بررسی می‌کند؛ packet capture یا نبود همهٔ outbound traffic را اثبات نمی‌کند.
- هر پنج baseline status برابر done ماندند؛ هر پنج تصمیم `re_review_required/reviewed` ثبت شدند، زیرا UPD-S1-023 هنوز catalog/selection/backend composition را تغییر خواهد داد.
- Quality V3.1.78 [42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) و Rendezvous [42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) موفق‌اند؛ Product Build V3.1.78 [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) هنگام این بازبینی هنوز in_progress بود.
- جمع reconciliation: **39/168 reviewed (23.21%)**، **129 pending**. وضعیت قدیمی و نشانگرهای update_impact تغییر نکرده‌اند.


## بازبینی محتوایی T0066 تا T0070 — V 3.1.79

- مبنای کد: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`.
- T0066: verifier قبل از probe نسخه SHA-256 executable را fixed-time compare و پس از probe دوباره بررسی می‌کند؛ launcher قبل از process start آن را فراخوانی می‌کند. Tests و historical real inference run 40104554823 این مسیر را پوشش داده‌اند.
- T0067: MainWindow از LocalInferenceBackendFactory و HostInferenceSession از IInferenceBackend استفاده می‌کند؛ alternate backend در C# harness اجرا شده و factory فعلی فقط llama.cpp را پشتیبانی می‌کند.
- T0068/T0069: schema نسخه‌دار و parser fail-closed، checksum، backend allowlist، HTTPS host allowlist، uniqueness و license status را اعتبارسنجی می‌کنند؛ parser network fetch نمی‌کند.
- T0070: صفحهٔ WPF رکوردهای واقعی کاتالوگ را بارگذاری و جزئیات را نمایش می‌دهد؛ انتخاب فقط preference است و backend را فعال نمی‌کند. Connect هنوز ModelId دستی و Generate سقف ثابت 1024 دارد.
- T0066–T0069 به UPD-S1-023 و T0070 به UPD-S1-023/024 نگاشت شده‌اند؛ baseline status همه done حفظ شد.
- Quality V3.1.79 [42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) و Rendezvous [42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) موفق‌اند؛ Product Build V3.1.79 [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) هنگام این بازبینی هنوز in_progress بود.
- جمع reconciliation: **44/168 reviewed (26.19%)**، **124 pending**؛ وضعیت‌های قدیمی و نشانگرهای update_impact تغییر نکردند.


## بازبینی محتوایی T0071 تا T0075 — V 3.1.80

- مبنای کد: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`.
- T0071: جزئیات نام/شناسه/فرمت/اندازه/source/license/backend/checksum و limitation نمایش داده می‌شود؛ وضعیت مجوز فقط metadata است، نه legal opinion. این UI با UPD-S1-024 نیز تحت تأثیر است.
- T0072: وضعیت مجوز ناموجود/نامشخص و برچسب REVIEW REQUIRED به‌صورت fail-closed نمایش داده می‌شود؛ وضعیت verified در کاتالوگ به معنای تأیید حقوقی مستقل نیست.
- T0073: C# و Python tests مسیرهای checksum، duplicate IDs/properties، backend/format/URL allowlists، license states و اندازه را پوشش می‌دهند؛ این tests auto model selection را اثبات نمی‌کنند.
- T0074: sample metadata یک entry از bartowski، size 105454432 و SHA-256 `2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d` دارد. مدل workflow تاریخی real-inference از QuantFactory revision `a6b7826ed969c3926a98359e39912af137b464cb`, size 105454144 و SHA-256 `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684` استفاده می‌کرد؛ این دو artifact یکسان فرض نمی‌شوند.
- T0075: OfflineModelCatalogStore فقط metadata معتبر را cache و دوباره validate می‌کند؛ fallback به cache به معنای نصب یا فعال‌بودن مدل نیست. این محدودیت در MainWindow نیز نمایش داده می‌شود.
- هر پنج baseline status برابر done حفظ شدند؛ تصمیم همه re_review_required/reviewed است، چون UPD-S1-023 activation/selection مدل و T0071 همچنین UPD-S1-024 UI بصری را تغییر می‌دهد.
- Quality V3.1.80 [42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) و Rendezvous [42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) موفق. HostLifecycle در Product run [42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) موفق با ۶۷ PASS، پوشش ۸۵٫۸۹٪ و یک SKIP اختیاری real-model است؛ Windows و Android jobs در زمان این ثبت در حال اجرا بودند.
- جمع reconciliation: **49/168 reviewed (29.17%)**، **119 pending**. وضعیت‌های قدیمی و update_impact markers تغییر نکردند.


## بازبینی محتوایی T0088، T0090، T0092، T0098 و T0099 — V 3.1.81

- T0088: صفحهٔ WPF میزبان و پیام‌های چت، ورودی و ارسال بررسی شد؛ Product run 40179954088 و Quality run 40179954129 برای source commit وظیفه موفق ثبت شده‌اند. این صفحه Android chat UI/Room نیست و پس از UPD-S1-010 و UPD-S1-024 بازبینی می‌شود.
- T0090: ChatConversation ترتیب user/assistant/system را نگه می‌دارد؛ WindowsHostConversationStore history را با DPAPI و رضایت مالک نگه می‌دارد، اما این persistence محلی Windows است و جایگزین Room اندروید نیست. Historic quality 40181923409 و product 40181923441 موفق‌اند.
- T0092: HostErrorPresentation و regressionها خطا/لغو را به پیام System نگاشت می‌کنند، نه پاسخ Assistant؛ historic quality 40232583373 و product 40232583374 موفق‌اند. typed error اندروید هنوز UPD-S1-010 است.
- T0098/T0099: HostRequestCoordinator با semaphore یک و WaitAsync(0) پذیرش fail-fast دارد؛ Busy بدون queue، cancellation و release آزموده شده‌اند. Exact current V3.1.81 HostLifecycle [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) PASS ثبت کرده؛ از ۶۴ درخواست هم‌زمان دقیقاً یک مورد پذیرفته و ۶۳ مورد Busy شده‌اند.
- یال‌های مرتبط از قبل validated هستند و تغییر نکردند: T0087→T0088/T0090/T0092 و T0097→T0098/T0099 sequencing_only؛ T0088→T0089، T0089→T0090، T0090→T0091، T0091→T0092 contract/interface؛ T0098→T0099 و T0099→T0100 hard_dependency.
- وضعیت پایهٔ تمام پنج وظیفه done حفظ شد؛ تصمیم‌ها re_review_required/reviewed هستند. UPD-S1-010/024 برای سه وظیفهٔ نخست، و UPD-S1-013/023 برای دو مورد آخر trigger بازبینی مجددند.
- Exact V3.1.81 Quality [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) و Rendezvous [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) موفق؛ HostLifecycle در Product [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) موفق با ۶۷ PASS، ۸۵٫۸۸٪ coverage و یک SKIP اختیاری real-model. Windows و Android Product jobs هنگام ثبت در حال اجرا بودند.
- reconciliation: **70/168 reviewed (41.67%)**، **98 pending**؛ وضعیت‌های قدیمی و نشانگرهای update_impact تغییر نکردند.


## بازبینی T0100 تا T0109 روی V 3.1.82

- commit کد بررسی‌شده: `63ae554809f1d1bc6217b0ad48632f416f3db0f2` — `V 3.1.82`.
- هر ده task از نظر کد، آزمون، معیار پذیرش و وابستگی‌ها بازبینی شدند؛ baseline status `done` و تاریخچه حفظ شد.
- تصمیم هر ده رکورد `re_review_required` و `review_status=reviewed` است؛ re-review نهایی پس از اجرای UPD-S1 مرتبط لازم خواهد بود.
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
- Product Build دقیق [42503829586](https://github.com/artinrahimii/project/actions/runs/42503829586) موفق؛ Windows، Android و HostLifecycle jobs موفق شدند.
- Quality Gate دقیق [42503829570](https://github.com/artinrahimii/project/actions/runs/42503829570) موفق بود.
- HostLifecycle: ۶۷ PASS، ۸۵٫۸۹٪ پوشش خطی و یک آزمون real-model اختیاری SKIP. هیچ inference مدل واقعی یا Android-to-Windows فیزیکی در این دسته ادعا نمی‌شود.
- گراف قدیمی و نشانگرهای ثابت `update_impact` تغییر نکردند.
- جمع reconciliation: **64/168 reviewed (38.10%)**، **104 pending (61.90%)**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی T0110 تا T0115 روی V 3.1.83

- commit کد بررسی‌شده: `58df066c3feb44020e59d75870a3d1c66642d604` — `V 3.1.83`.
- شش task از نظر کد، تست، معیار legacy و edgeهای مستقیم جداگانه بررسی شدند؛ baseline status هر شش `done` باقی ماند.
- تصمیم‌ها `re_review_required/reviewed` هستند؛ UPD-S1-004/012/017 هنوز در قرارداد parser/transport/error، UPD-S1-012 در exposure و listener، و UPD-S1-018 در privacy/cleanup اثر دارند.
- `update/Step three/UPD-S1-008/evidence/review-T0110.md`
- `update/Step three/UPD-S1-008/evidence/review-T0111.md`
- `update/Step three/UPD-S1-008/evidence/review-T0112.md`
- `update/Step three/UPD-S1-008/evidence/review-T0113.md`
- `update/Step three/UPD-S1-008/evidence/review-T0114.md`
- `update/Step three/UPD-S1-008/evidence/review-T0115.md`
- Quality Gate [42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358)، HostLifecycle در Product [42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) و Rendezvous [42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) موفق بودند. Android/Windows product jobs در زمان snapshot در حال اجرا بودند.
- نشانگرهای گراف قدیمی تغییر نکردند؛ T0115→T0128 هنوز unreviewed است.
- مجموع: **70/168 reviewed (41.67%)**، **98 pending (58.33%)**؛ UPD-S1-008 همچنان `in_progress`.


## بازبینی T0116 تا T0125 روی V 3.1.84

- commit کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de.
- ده وظیفه از نظر کد، تست، معیار legacy و edgeها بررسی شدند؛ وضعیت پایهٔ هر ده done حفظ شد.
- T0119/T0120 تصمیم unchanged دارند: relay غیرفعال و directory metadata-only حفظ می‌شود. هشت مورد دیگر re_review_required هستند.
- ADR-0022 برای first release یک peer در هر round، حداکثر سه تلاش ترتیبی و retry دستی تعیین کرده است؛ سه میزبان در هر دور صرفاً generic compatibility است.
- Product Build V3.1.84: HostLifecycle و Windows موفق، Android در snapshot هنوز در حال اجرا؛ Quality و Rendezvous موفق.
- reconciliation: **80/168 reviewed (47.62%)**، **88 pending (52.38%)**؛ UPD-S1-008 همچنان in_progress.
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


## بازبینی T0126 تا T0135 روی V 3.1.85

- commit کد بررسی‌شده: 7082162e902e1c7e32718e0af687f8f62b7d92dc.
- ده شناسه جداگانه در برابر کد، تست، معیار legacy و edgeهای مستقیم بررسی شدند؛ وضعیت پایه حفظ شد.
- تصمیم unchanged فقط برای T0129 ثبت شد. T0128 deferred_final باقی ماند و آزمون شبکهٔ عمومی/دو گرهٔ مستقل اجرا نشده است.
- V3.1.85 Quality و Rendezvous موفق؛ HostLifecycle موفق و Android/Windows product jobs در snapshot هنوز در حال اجرا بودند.
- reconciliation: **90/168 reviewed (53.57%)**، **78 pending (46.43%)**؛ UPD-S1-008 in_progress.
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


## بازبینی T0136 تا T0145 روی V 3.1.86

- نه شناسهٔ متأثر T0136-T0139 و T0141-T0145 در برابر کد، تست و معیار legacy بررسی شدند؛ T0140 به علت نبود affected_task_ids خارج از این مجموعه است.
- T0138 تصمیم unchanged دارد؛ افشای قابل‌خواندن بودن prompt/capsule برای میزبان در اسناد و UI موجود است.
- T0145 همچنان UI/local message persistence است؛ درخواست به میزبان در همین صفحه ارسال نمی‌شود. پاسخ assistant به همان Room باید در UPD-S1-010 متصل شود.
- Quality و Rendezvous V3.1.86 موفق؛ HostLifecycle موفق؛ Windows و Android product jobs در snapshot مبنا در حال اجرا بودند. Product Build V3.1.82 هر دو برنامه را کامل ساخت.
- reconciliation: **99/168 reviewed (58.93%)**، **69 pending (41.07%)**.
- update/Step three/UPD-S1-008/evidence/review-T0136.md
- update/Step three/UPD-S1-008/evidence/review-T0137.md
- update/Step three/UPD-S1-008/evidence/review-T0138.md
- update/Step three/UPD-S1-008/evidence/review-T0139.md
- update/Step three/UPD-S1-008/evidence/review-T0141.md
- update/Step three/UPD-S1-008/evidence/review-T0142.md
- update/Step three/UPD-S1-008/evidence/review-T0143.md
- update/Step three/UPD-S1-008/evidence/review-T0144.md
- update/Step three/UPD-S1-008/evidence/review-T0145.md


## بازبینی T0146 تا T0150 روی V 3.1.87

- پنج شناسهٔ متأثر T0146 تا T0150 در برابر کد، تست، معیار پذیرش و edgeهای مستقیم بررسی شدند؛ وضعیت پایهٔ همه حفظ شد.
- status UI و SavedStateHandle/Room storage پیاده‌سازی واقعی دارند، اما runtime درخواست میزبان در chat detail هنوز preview/local-only است.
- آزمون file-backed close/reopen و Android CI با آزمون in-place upgrade روی دستگاه نصب‌شده یکسان نیست.
- Quality و Rendezvous V3.1.87 موفق؛ HostLifecycle موفق؛ Android و Windows installer jobs در زمان snapshot در حال اجرا بودند.
- reconciliation: **104/168 reviewed (61.90%)**، **64 pending (38.10%)**.
- update/Step three/UPD-S1-008/evidence/review-T0146.md
- update/Step three/UPD-S1-008/evidence/review-T0147.md
- update/Step three/UPD-S1-008/evidence/review-T0148.md
- update/Step three/UPD-S1-008/evidence/review-T0149.md
- update/Step three/UPD-S1-008/evidence/review-T0150.md



## بازبینی محتوایی T0161 تا T0165 روی V 3.1.88

- مبنای کد: `47c078dc39ea69e2b88db57f057e71353fc2ec8c` — `V 3.1.89` مستندات بر همان snapshot کد `V 3.1.88` را بازبینی می‌کنند.
- T0161 تا T0165 جداگانه بررسی شدند؛ وضعیت پایهٔ هر پنج مورد `done` حفظ شد و هیچ status قدیمی تغییر نکرد.
- تصمیم هر پنج مورد `re_review_required` و وضعیت بررسی در JSON reconciliation برابر `reviewed` ثبت شد؛ دلیل re-review این است که UPD-S1-015 و آزمون‌های runtime نهایی هنوز انجام نشده‌اند.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0161.md` تا `review-T0165.md`؛ commit شواهد: `47c078dc39ea69e2b88db57f057e71353fc2ec8c`.
- Quality CI دقیق snapshot کد [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) و Product CI دقیق همان snapshot [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شدند.
- reconciliation جاری: **109/168 reviewed (64.88%)** و **59 pending (35.12%)**. `UPD-S1-008` همچنان `in_progress` است.
- گراف اولیه و markerهای ثابت `update_impact` عمداً تغییر نکردند؛ تا تکمیل همهٔ 168 مورد و مطابق آزمون قراردادی pending می‌مانند.
- آزمون فیزیکی Android-to-Windows، inference واقعی مدل محلی، same Room E2E، clean install روی دستگاه واقعی و in-place upgrade در این تلاش اجرا نشدند.


## بازبینی محتوایی T0166 تا T0170 روی V 3.1.90

- پنج وظیفه به‌صورت مستقل از روی کد، UI/runtime، آزمون‌های واحد و قراردادی و وابستگی‌ها بررسی شدند؛ شواهد: `update/Step three/UPD-S1-008/evidence/review-T0166.md` تا `review-T0170.md`.
- وضعیت پایهٔ هر پنج گره `done` حفظ شد؛ تصمیم هر پنج `re_review_required` و وضعیت reconciliation برابر `reviewed` ثبت شد. دلیل بازاعتبارسنجی باز، سیاست UPD-S1-015 و اتصال پاسخ همان گفت‌وگو/Room در UPD-S1-010 است.
- کد کارت پاسخ مستقل و collector چند-attempt وجود دارد، اما مسیر اجرایی نسخهٔ نخست یک میزبان فعال در هر round و حداکثر سه تلاش ترتیبی را اجرا می‌کند؛ retry دستی است. generic سه‌میزبانهٔ سازگار را نباید با multi-host production یکسان دانست.
- Product CI دقیق source بررسی‌شده [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند.
- reconciliation: **114/168 reviewed (67.86%)** و **54 pending (32.14%)**؛ `UPD-S1-008` همچنان `in_progress` است. گراف اولیه و markerهای ثابت آن تا تکمیل همهٔ 168 مورد دست‌نخورده باقی ماندند.
- آزمون فیزیکی Android-to-Windows، پاسخ live مدل محلی، ذخیرهٔ پاسخ در همان Room، clean install واقعی و in-place upgrade اجرا نشده‌اند.


## بازبینی محتوایی T0171 تا T0175 روی V 3.1.92

- T0171 تا T0175 از روی کد Android/Windows، آزمون‌های واحد/قراردادی و مصرف‌کنندگان مستقیم بررسی شدند. شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0171.md` تا `review-T0175.md`.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ تصمیم هر پنج `re_review_required` و review_status برابر `reviewed` ثبت شد.
- انتخاب مجدد میزبان پیشین به‌طور پیش‌فرض ممنوع، correlation و deadline پاسخ کنترل‌شده، پاسخ تکراری در حافظه deduplicate، لغو با CancelRequest هم‌بسته و retry فقط دستی و محدود است. idempotency پایدار پاسخ در Room و آزمون فیزیکی E2E هنوز اثبات نشده است.
- Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند.
- reconciliation جاری: **119/168 reviewed (70.83%)** و **49 pending (29.17%)**؛ UPD-S1-008 همچنان `in_progress` است. گراف اولیه و markerهای pending آن عمداً تغییر نکردند.


## بازبینی محتوایی T0176 تا T0180 روی V 3.1.94

- پنج وظیفهٔ T0176 تا T0180 از روی کد محصول، آزمون‌ها و سیاست نگهداری/لاگ/پاک‌سازی بازبینی شدند؛ شواهد: `update/Step three/UPD-S1-008/evidence/review-T0176.md` تا `review-T0180.md`.
- وضعیت پایهٔ هر پنج مورد `done` حفظ شد؛ تصمیم هر پنج `re_review_required` و review_status برابر `reviewed` ثبت شد.
- در T0176 تست مستقل دقیقاً دو پاسخ معتبر پیدا نشد؛ zero/one/three، duplicate، late، timeout و cancel پوشش دارند. این مورد follow-up باز است.
- T0178 retention میزبان به‌طور پیش‌فرض غیرپایدار است؛ ذخیرهٔ پایدار فقط با رضایت جداگانه و DPAPI ممکن می‌شود. T0179 log تشخیصی را به کدهای عددی/نتیجه/مدت محدود می‌کند. T0180 بافرها را در failure/ack/teardown پاک می‌کند، اما پاک‌سازی فوری همهٔ بافرهای Windows در لحظهٔ timeout اثبات نشده است.
- Product CI دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند.
- reconciliation جاری: **124/168 reviewed (73.81%)** و **44 pending (26.19%)**؛ UPD-S1-008 همچنان `in_progress` است. گراف اولیه و نشانگرهای pending آن دست‌نخورده‌اند.


## آخرین snapshot reconciliation — V 3.1.98

- مبنا: `bf9ba99b17d12df86197064e3576afb0046153b0`، commit شواهد T0181-T0185.
- **129/168 reviewed (76.79%)**؛ **39/168 pending (23.21%)**.
- T0181-T0184 تصمیم `re_review_required` و T0185 تصمیم `unchanged` دارند؛ وضعیت پایهٔ هر پنج مورد `done` حفظ شد.
- `UPD-S1-008` همچنان `in_progress` است؛ گراف اولیه تا تکمیل همهٔ بررسی‌ها و بازاعتبارسنجی markerها دست‌نخورده می‌ماند.


## آخرین snapshot reconciliation — V 3.1.100

- مبنای شواهد: `c569a8a87379f0fa6d2bc672189f07987cdbe65f` — بازبینی T0186 تا T0190.
- **134/168 reviewed (79.76%)**؛ **34/168 pending (20.24%)**.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد و تصمیم بازبینی هر پنج `re_review_required` است.
- ناسازگاری‌های T0190 و اتصال درخواست گفت‌وگوی عادی به inference هنوز باز هستند؛ `UPD-S1-008` همچنان `in_progress` است.


## آخرین snapshot reconciliation — V 3.1.102

- مبنای شواهد: `1236955162e57c1a70a576fc1d19f679d1fadeff` — بازبینی T0191، T0192، T0193، T0196 و T0197.
- **139/168 reviewed (82.74%)**؛ **29/168 pending (17.26%)**.
- وضعیت پایهٔ هر پنج وظیفه `done` حفظ شد؛ تصمیم بازبینی هر پنج `re_review_required` است.
- `UPD-S1-008` همچنان `in_progress` است؛ E2E فیزیکی و بازاعتبارسنجی اثر تغییرات جدید باقی است.


## آخرین snapshot reconciliation — V 3.1.104

- مبنای شواهد: `4e976f8b8c478498708d578d363d8529edb850c6` — بررسی T0198، T0200، T0201، T0202 و T0203.
- **144/168 reviewed (85.71%)**؛ **24/168 pending (14.29%)**.
- وضعیت پایهٔ T0198/T0200/T0201/T0203 برابر `done` و T0202 برابر `deferred_final` حفظ شد؛ تصمیم هر پنج `re_review_required` است.
- T0202 هنوز آزمون فیزیکی LAN ندارد. UPD-S1-008 همچنان `in_progress` است.


## آخرین snapshot reconciliation — V 3.1.106

- مبنای شواهد: `dba2cb984f0d4990417922f8a5f56b81c82191a8` — بازبینی T0204 تا T0208.
- **149/168 reviewed (88.69%)**؛ **19/168 pending (11.31%)**.
- وضعیت پایهٔ هر پنج شناسه `done` حفظ شد؛ تصمیم T0206 `unchanged` و تصمیم T0204/T0205/T0207/T0208 برابر `re_review_required` است.
- T0207/T0208 تا پایان Product Build جاری دوباره بررسی می‌شوند؛ T0202 هنوز مانع LAN فیزیکی جداگانه دارد.


## بازبینی محتوایی T0209 تا T0213 — 2026-11-08T20:18:05.645Z

- مبنای هر پنج بازبینی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` (`V 3.1.106`). وضعیت baseline هر پنج `done` حفظ شد و هیچ وضعیت قدیمی تغییر نکرد.
- T0209/T0210: Windows clean runner، clean build/publish، نصب‌کنندهٔ واقعی، inventory مجوز 83 package، model license notice، نصب ایزوله، Publisher registry، WPF smoke و uninstall در Product Build `42516118989` موفق شدند.
- T0211: APK clean build، JVM/Keystore instrumentation و نصب/launch روی Emulator موفق شدند؛ PID=4319 و resumed MainActivity ثبت شد. آزمون non-loopback LAN صریحاً SKIPPED بود.
- T0212: امضای production در run `42114568183` با 15 مرحلهٔ موفق، fingerprint pin check و artifact واقعی تأیید شد؛ Blob workflow امضا با current HEAD یکسان است.
- T0213: سه ورودی manifest با SHA-256 تأیید و Draft Release `436740935` با چهار asset موجود است. شکست تاریخی final lookup و نیاز به اجرای tag مجدد پس از اصلاح حفظ می‌شود.
- تصمیم هر پنج: `re_review_required`؛ وضعیت بازبینی: `reviewed`. UPD-S1-021 و (برای T0210/T0211) UPD-S1-024 باید خروجی‌های نهایی را مجدداً بررسی کنند.
- شواهد مستقل: `update/Step three/UPD-S1-008/evidence/review-T0209.md` تا `review-T0213.md`.
- snapshot تازه: **154/168 reviewed (91.67%)**؛ **14/168 pending (8.33%)**. شناسه‌های pending: `T0215,T0216,T0217,T0218,T0219,T0221,T0222,T0223,T0224,T0225,T0226,T0227,T0228,T0229`. UPD-S1-008 همچنان `in_progress` است.


- ثبت SHA بازبینی‌های T0209 تا T0213: `4eba36c5fd9196fb616b5c0bd888c010cefeea58` — `V 3.1.107` در شاخهٔ آنلاین `main`.


## بازبینی تکمیلی T0215 تا T0229 — 2026-11-09

- مبنای بررسی تمام ۱۴ مورد: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` — `V 3.1.108` در شاخهٔ آنلاین `main`.
- شناسه‌های T0215، T0216، T0217، T0218، T0219، T0221، T0222، T0223، T0224، T0225، T0226، T0227، T0228 و T0229 تک‌به‌تک با کد، مصرف‌کنندگان، آزمون‌ها، workflowها و معیارهای پذیرش تطبیق داده شدند.
- تصمیم هر ۱۴ مورد `re_review_required` و وضعیت بازبینی `reviewed` است؛ `status_changed=false`. هیچ وضعیت پایه‌ای تغییر نکرد: T0215 همچنان `in_progress` و ۱۳ مورد دیگر همچنان `not_started` هستند.
- شواهد مستقل در `update/Step three/UPD-S1-008/evidence/review-T0215.md` تا `review-T0229.md` ثبت شدند.
- سند `docs/release-limitations.md` برای تجمیع محدودیت‌ها و آزمون‌های اجرا‌نشده اضافه شد؛ این سند تأیید انتشار نیست.
- Quality Gate دقیق HEAD `42519571303`، Product Build `42519571320` و Rendezvous container `42519571350` پیش از این تغییرات موفق بودند؛ تغییرات جدید نیازمند CI مجدد هستند.
- snapshot reconciliation محتوایی: **168/168 reviewed (100%)**؛ **0 pending**. این عدد به معنی تکمیل UPD-S1-008 یا ۲۴ وظیفهٔ Update نیست.
- نشانگرهای `update_impact` گراف اولیه مطابق قرارداد untouched و `pending` باقی می‌مانند. گراف اولیه و وضعیت‌های پایه تغییر نکردند.
- `UPD-S1-008` همچنان `in_progress` است؛ ۷۰ یال از ۸۰ یال گراف Update هنوز `unreviewed` هستند. باید هر یال بر اساس شواهد فنی، قراردادی یا صرفاً ترتیبی بررسی شود؛ هیچ یالی برای موازی‌سازی حذف نشده است.


## ممیزی وابستگی‌های گراف — 2026-11-09

- خط مبنای بررسی: `ae7725fd30f98a75968edb78082fe0d64ce409cd` — `V 3.1.109`؛ منبع نیازمندی: `V1.10.json`.
- هر ۷۰ یال باقی‌مانده با آرایهٔ `prerequisites` وظیفهٔ مقصد تطبیق داده شد؛ تمام ۷۰ رابطه در نسخهٔ تأییدشدهٔ V1.10 صریحاً اعلام شده‌اند و مسیرهای شواهد تولیدکننده/مصرف‌کننده در Git tree موجودند.
- طبقه‌بندی این ۷۰ یال: ۱۹ `hard_dependency`، ۳۱ `contract_or_interface_dependency` و ۲۰ `sequencing_only`. مجموع گراف: **۸۰/۸۰ یال validated**، شامل ۱۹ hard، ۳۴ contract/interface و ۲۷ sequencing-only؛ بدون یال unreviewed یا disputed.
- هیچ یالی حذف نشد، `blocks` به‌تنهایی معادل hard dependency تلقی نشد و ترتیب و ۱۳ موج اجرایی تغییر نکردند.
- گزارش خط‌به‌خط با دلیل و شواهد: `update/Step three/UPD-S1-008/evidence/dependency-edge-review.md`.
- هر ۱۶۸ رکورد reconciliation دارای تصمیم، شواهد و SHA واقعی commit بازبینی است؛ وضعیت پایهٔ گراف قدیمی تغییر نکرده است.
- `UPD-S1-008` هنوز `in_progress` است؛ همگام‌سازی ۱۶۸ نشانگر `update_impact` گراف اولیه و به‌روزرسانی آزمون قراردادی آن باقی مانده و باید هر دو graph guard پس از تغییر موفق شوند.


## وضعیت ممیزی خط مبنای V 3.1.110 — 2026-11-09

- 168 شناسهٔ متأثر در گراف و ledger موجودند؛ 168 رکورد یکتا و نگاشت کامل `update_task_ids` بدون اختلاف است.
- 168 رکورد ledger از نظر محتوایی `reviewed` هستند: 156 `re_review_required` و 12 `unchanged`. تمام وضعیت‌های legacy حفظ شده‌اند.
- تمام شواهد فایل‌محور در Git tree مبنا موجودند و هر 30 SHA یکتای commit بازبینی در تاریخچهٔ main وجود دارد.
- هر 80 یال گراف Update بررسی و validated است؛ 19 hard، 34 contract/interface و 27 sequencing-only؛ 13 موج و ترتیب اجرا حفظ شده‌اند.
- **همگام‌سازی گراف اولیه انجام نشده است:** هر 168 marker هنوز `pending/re_review_required` و خلاصهٔ گراف قدیمی pending=168/reviewed=0 است. در 12 مورد، تصمیم marker با تصمیم `unchanged` در ledger ناسازگار است. `review_state` مرکزی نیز تا همگام‌سازی گراف pending می‌ماند.
- دلیل عدم تغییر: دو بار ارسال Blob کامل 4,016,404 بایتی از GitHub connector با خطای H2 body-transfer شکست خورد و clone محلی نیز به‌علت DNS برای github.com ممکن نشد. remote main پس از خطا دوباره بررسی شد و تغییری نکرد.
- `UPD-S1-008` همچنان `in_progress` است؛ باید ابتدا Blob کامل Step Three، markerها و آزمون قراردادی همگام شوند و سپس هر دو graph guard و CI دقیق موفق شوند.

## وضعیت نهایی reconciliation — ۹ نوامبر ۲۰۲۶

- 168/168 بازبینی محتوایی و سند مختص وظیفه؛ 0 pending.
- تصمیم‌ها: 156 re_review_required، 12 unchanged.
- 168 نشانگر update_impact با ledger همگام شد؛ صفر تغییر در وضعیت پایه یا تاریخچهٔ Txxxx.
- 80/80 یال Update اعتبارسنجی شده؛ 13 موج و ترتیب قبلی حفظ شدند.
- تصمیم re_review_required بازبینی بعدی را پس از پذیرش وظیفهٔ اجرایی مرتبط الزام می‌کند؛ رکورد جاری reconciliation reviewed است.


## بازبینی تکمیلی پس از پذیرش UPD-S1-014 — V 3.1.197

- هشت سند پس از پذیرش برای T0116، T0117، T0118، T0121، T0122، T0123، T0124 و T0127 بررسی شدند؛ تصمیم هر هشت unchanged است.
- وضعیت پایه done و تاریخچهٔ وظایف قدیمی حفظ شد.
- مجموع reconciliation: 168 reviewed، صفر pending؛ 20 unchanged و 148 re_review_required.

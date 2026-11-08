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
**39/168 بازبینی محتوایی ثبت شده‌اند.** تصمیم `unchanged` برای T0012، T0013، T0015، T0016، T0019 و T0020؛ و تصمیم `re_review_required/reviewed` برای T0027، T0032–T0036، T0041–T0065، T0199 و T0214 ثبت شده است. 129 رکورد همچنان `pending` هستند.

## سیاست تغییر وضعیت
هیچ وضعیت پایهٔ قدیمی تغییر نکرده است. تصمیم `unchanged` برای 6 رکورد، `re_review_required/reviewed` برای 33 رکورد (T0027، T0032–T0036، T0041–T0044، T0199 و T0214)، و `re_review_required/pending` برای 129 رکورد ثبت شده است. نشانگرهای ثابت `update_impact` در گراف اولیه مطابق قرارداد تا تکمیل کل reconciliation pending می‌مانند؛ وضعیت واقعی هر بررسی در JSON ثبت شده است.


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

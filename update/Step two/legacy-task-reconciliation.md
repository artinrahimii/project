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
**5/168 بازبینی محتوایی نهایی شده‌اند.** T0012، T0013، T0015، T0016 و T0019 reviewed هستند؛ T0020 پس از کشف شناسهٔ تکراری ADR دوباره pending شده و 163 مورد pending باقی مانده‌اند. کد، آزمون و معیار پذیرش هر شناسه باید در مرحله‌های بعدی جداگانه بررسی شود. وجود مسیر فایل، evidence یا اجرای قدیمی CI به‌تنهایی تأیید وضعیت امروز آن وظیفه نیست.

## سیاست تغییر وضعیت
هیچ وضعیت پایهٔ قدیمی تغییر نکرده است. تصمیم فعلی برای 5 رکورد بازبینی‌شده `unchanged` و برای 163 رکورد باقی‌مانده `re_review_required` است. نشانگرهای ثابت `update_impact` در گراف اولیه مطابق قرارداد تا تکمیل کل reconciliation در حالت `pending/re_review_required` باقی می‌مانند؛ وضعیت واقعی هر بررسی در همین فایل JSON ثبت می‌شود.


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

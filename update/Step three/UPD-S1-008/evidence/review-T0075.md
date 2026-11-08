# بازبینی محتوایی T0075 — fallback آفلاین فقط به آخرین کاتالوگ معتبر

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Models/Catalog/OfflineModelCatalogStore.cs` — `0b8ef8984ed26079412b62ebc45e2c277ecab2b4`
- `src/Host.Windows/Models/Catalog/ModelCatalogFileLoader.cs` — `2422dfc6584343a973af853ea83c7751d8e1b234`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs` — `ca9c4965f5cbf0fe8d40892a401b7e5657340412`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`

## یافته‌های کد و آزمون
- OfflineModelCatalogStore فایل منبع را با loader/parser strict UTF-8 و size/schema checks اعتبارسنجی می‌کند و فقط پس از موفقیت، نسخهٔ معتبر را به cache محلی می‌نویسد.
- در شکست فایل منبع، cache موجود را با همان allowlist و parser مجدداً validate می‌کند؛ اگر cache نیز missing یا invalid باشد InvalidDataException می‌دهد. Cache یک catalog metadata است، نه مدل وزن‌دار.
- MainWindow صریحاً هنگام استفاده از cache می‌گوید catalog metadata از آخرین cache محلی معتبر نمایش داده شده و هیچ مدل دانلود یا نصب نشده است. Acceptance test مسیر malformed-source/cache fallback را پوشش می‌دهد.
- تاریخچهٔ T0075 run40161618780 quality، HostLifecycle job121074200657، WPF job121074200813 و real local inference run40161618719 را روی exact source head f2758759cb5a28fcf9cfd2251918d21d93fcf3ce موفق ثبت کرده است؛ real inference مربوط به engine است، نه اثبات catalog auto-selection.

## علت بازبینی مجدد
- UPD-S1-023 باید catalog selection را به مدل نصب‌شده و backend ready وصل کند؛ fallback آفلاین فقط metadata را برمی‌گرداند و مسیر inference را تغییر نمی‌دهد. پس از تغییر resolver و تنظیمات، cache و UI باید دوباره آزموده شوند.

## شواهد CI دقیق V3.1.80
- Quality Gate: [run 42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) — passed.
- Rendezvous container build/smoke: [run 42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) — passed.
- HostLifecycle job: [run 42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) — passed; 67 PASS markers, 85.89% line coverage, one optional real-model integration SKIP.
- Product jobs for Windows and Android were still in progress during this review; no full Product Build success is claimed.
- Historical exact task-specific runs are described above and in the old task report; they are not current V3.1.80 results.
- No physical Android-to-Windows E2E, clean install, in-place upgrade, actual model download/activation, or target-device visual contrast review was performed in this attempt.

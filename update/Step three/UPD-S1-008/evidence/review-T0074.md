# بازبینی محتوایی T0074 — کاتالوگ نمونه با metadata، checksum و license declaration

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Models/Catalog/sample-model-catalog.json` — `f91c7cb68c151b216260fce14c2f30a8a027a06c`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json` — `57a8beac9317c8767c4409d9ce16dd9ccde8c3ac`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs` — `ca9c4965f5cbf0fe8d40892a401b7e5657340412`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`

## یافته‌های کد و آزمون
- فایل sample-model-catalog.json یک رکورد metadata برای SmolLM2-135M-Instruct Q4_K_M دارد: source bartowski، size اعلامی 105454432 bytes، SHA-256 برابر 2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d، backend llama.cpp و license claim Apache-2.0.
- C# acceptance آن را از فایل واقعی disk با ModelCatalogFileLoader و supported backend registry و allowlistهای صریح بارگذاری می‌کند؛ این آزمون اعتبارسنجی metadata است، نه دانلود/بارگذاری/اجرای وزن‌ها.
- این sample با artifact workflow تاریخی real-inference یکسان فرض نمی‌شود: workflow از QuantFactory revision a6b7826ed969c3926a98359e39912af137b464cb، size 105454144 و SHA 8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684 استفاده می‌کرد. دو source/revision/checksum متفاوت‌اند و تطبیق byte-level ندارند.
- تاریخچه exact V3.0.228 run40123058807 quality، run40123058861 Product (Windows/Android/HostLifecycle jobs) و run40123058748 real inference را برای همان commit گزارش کرده است؛ real inference evidence به model workflow pin شده خودش مربوط است، نه این sample JSON.

## علت بازبینی مجدد
- UPD-S1-023 باید catalog model identity/checksum/license را با دانلود، نصب و backend واقعی مرتبط کند؛ metadata sample به‌تنهایی اثبات مدل نصب‌شده یا اجرای همان وزن‌ها نیست.

## شواهد CI دقیق V3.1.80
- Quality Gate: [run 42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) — passed.
- Rendezvous container build/smoke: [run 42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) — passed.
- HostLifecycle job: [run 42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) — passed; 67 PASS markers, 85.89% line coverage, one optional real-model integration SKIP.
- Product jobs for Windows and Android were still in progress during this review; no full Product Build success is claimed.
- Historical exact task-specific runs are described above and in the old task report; they are not current V3.1.80 results.
- No physical Android-to-Windows E2E, clean install, in-place upgrade, actual model download/activation, or target-device visual contrast review was performed in this attempt.

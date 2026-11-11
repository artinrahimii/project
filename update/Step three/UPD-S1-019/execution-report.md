# گزارش اجرای UPD-S1-019 — آزمون‌های واحد و یکپارچه

## خط مبنا
- مخزن: `artinrahimii/project`؛ شاخه: `main`.
- commit مبنا: `235dff60a9f18f45ba4350203941ed6594847aee` — `V 3.1.206`؛ درخت: `b16e0bd23a17d48a0dfb364ce867f35adfce1b41`.
- فایل گراف به‌روزرسانی: ۹۸۷٬۳۳۳ بایت؛ Blob SHA: `4fcb564767c228c3533697b5e555bedc0f22401d`.
- گراف اولیه: ۴٬۰۳۰٬۹۰۵ بایت؛ Blob SHA: `9026969f6036eae22b25851eb0b29cd1d6c57c7a`.
- نیازمندی V1.10: ۹۳٬۳۴۶ بایت؛ Blob SHA: `5cea3ec095d0c00e32ed3e067796ff698aedd982`.
- reconciliation: ۱۷۱٬۵۹۷ بایت؛ Blob SHA: `9637ac3df743c00da778d5446da930ab8cdc3c98`.
- هر چهار Git Blob SHA با محاسبهٔ `SHA-1("blob " + byte_length + NUL + content)` تطبیق داده شدند؛ اندازه‌ها برحسب UTF-8 byte هستند و دریافت کامل است.

## بررسی پیش‌نیاز
- UPD-S1-009، 010، 012، 013، 014، 015، 016، 017، 018، 023 و 024 در گراف مبنا done هستند.
- UPD-S1-011 در وضعیت `deferred_final` است؛ downstream allowlist همان وظیفه اجرای آزمون‌های خودکار UPD-S1-019 را مجاز می‌کند. endpoint عمومی HTTPS و دسترسی شبکهٔ مستقل هنوز اثبات نشده‌اند.

## کار انجام‌شده
- ممیزی آزمون‌های Android، Windows، transport، مدل، امنیت و UI نشان داد بیشتر سناریوهای پذیرش از قبل در مسیرهای آزمون واقعی وجود دارند و در workflow مربوط به محصول اجرا می‌شوند.
- یک شکاف واقعی در تست Android peer-trust برای key rotation شناسایی شد. آزمون `rotatedHostIdentityDoesNotInheritPriorTrustAndRequiresFreshConfirmation` به `AndroidPeerTrustStoreTest.kt` افزوده شد؛ این آزمون PeerId قبلی و جدید را از دو کلید Ed25519 متفاوت می‌سازد، عدم انتقال trust را بررسی می‌کند، revoke هویت قبلی و تأیید صریح هویت جدید را می‌سنجد.
- از ایجاد تست‌های تکراری برای منطق موجود خودداری شد؛ آزمون‌های Windows trust/revoke، mismatch رمزنگاری‌شده، replay/tamper، busy/one-inference، timeout/disconnect، model readiness و UI mapping از قبل موجود و به workflow اجرا متصل‌اند.
- فایل‌های محصول و قرارداد پیام تغییر نکردند.

## فایل‌های تغییر‌یافته در این تلاش
- `src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerTrustStoreTest.kt`
- `update/Step three/UPD-S1-019/execution-report.md`
- `update/Step three/UPD-S1-019/evidence/acceptance-test-matrix.md`
- `update/Step two/project-graph.md`

## آزمون و وضعیت
- آزمون محلی در محیط متصل اجرا نشده است؛ اجرای اجباری با workflowهای exact-head در حال انجام است.
- وضعیت وظیفه تا عبور Quality Gate و Windows/Android Product Build برابر `in_progress` باقی می‌ماند.
- آزمون فیزیکی Android-to-Windows، public rendezvous، clean install و in-place upgrade در این وظیفه اجرا نشده‌اند؛ این معیارها در وظایف پایانی جداگانه باقی می‌مانند.

## محدودیت و پیگیری
- نتیجهٔ هر workflow فقط پس از مشاهدهٔ run مربوط به SHA دقیق commit ثبت می‌شود.
- پس از موفقیت، همین گزارش با SHA کامل، run ID و شمار واقعی آزمون‌ها تکمیل و وضعیت گراف به‌روزرسانی می‌شود.

 
## نتیجهٔ نهایی تلاش A001 — CI دقیق commit V 3.1.207

- کد آزمون و مبنای اجرا: `33fdfcca9efbf10045a9a822cc654313c8568f69`; پیام `V 3.1.207`.
- Quality Gate، run [43076943275](https://github.com/artinrahimii/project/actions/runs/43076943275): موفق؛ 502 آزمون Python؛ دو حالت graph_guard برای گراف به‌روزرسانی بدون خطا و بدون هشدار؛ گراف قدیمی بدون خطا و یک هشدار metadata حفظ‌شده.
- Product Build، run [43076943268](https://github.com/artinrahimii/project/actions/runs/43076943268): موفق؛ هر سه job Windows WPF، Android و HostLifecycle موفق.
- Android JVM: 239 تست؛ 238 موفق، 0 failure، 0 error، 1 skipped. کلاس AndroidPeerTrustStoreTest شامل 4 تست بود؛ آزمون تازهٔ `rotatedHostIdentityDoesNotInheritPriorTrustAndRequiresFreshConfirmation` موفق شد.
- Android instrumentation: 11 موفق و یک آزمون LAN-only از `AndroidWindowsLanIntegrationTest` skipped؛ هیچ failure وجود نداشت. این شواهد emulator/Keystore instrumentation هستند، نه تست دستگاه فیزیکی.
- Windows HostLifecycle: 68 نشانگر PASS؛ پوشش خط 86.14% (9286/10780)؛ آزمون مستقل مدل واقعی که opt-in است skipped ماند.
- Windows installer: بستهٔ 61,498,504 بایتی تولید، روی مسیر موقت نصب، smoke-launch و سپس uninstall شد؛ 83 بستهٔ NuGet و مجوز یک مدل نمونه ثبت شدند. هشدار `NU1902` برای `System.Formats.Cbor 10.0.0` با شناسهٔ `GHSA-vmv3-wvjp-wqpj` در build مشاهده شد و باید پیش از انتشار بررسی شود.
- Android: debug و release build موفق؛ release فقط با کلید موقت CI امضا و پس از بررسی حذف شد. Production signing key در این job استفاده نشده است.
- Rendezvous container build/smoke، run [43076943292](https://github.com/artinrahimii/project/actions/runs/43076943292): موفق.
- مرز پذیرش: هیچ آزمون فیزیکی Android-to-Windows، اجرای مدل واقعی در همان مسیر UI، ذخیرهٔ پاسخ واقعی در همان Room conversation، public rendezvous reachability یا in-place upgrade انجام نشد؛ این‌ها خارج از دامنهٔ تکمیل UPD-S1-019 و در UPD-S1-011/020/021 باقی می‌مانند.
- وضعیت UPD-S1-019: `done` با شواهد فوق؛ هیچ ادعایی دربارهٔ تکمیل کل محصول یا release production مطرح نمی‌شود.

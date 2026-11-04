# T0187 — شواهد بازبینی و اجرای آزمون حساسیت log/temporary artifact

## Snapshot
- شاخه: `main`; مبنا: `e528d6d9e9daeb5a4c9540dfefe7d136770b90af` (`V 3.0.876`).
- گراف از Blob دقیق tree دریافت و SHA-1 Git Blob آن با `step three/project-graph.md` تطبیق داده شد؛ JSON کامل با ۲۳۰ node و ۴۲۲ edge parse شد.
- وضعیت قبل از اجرا: ۱۸۵ done، ۲ deferred_final، ۴۳ not_started؛ T0187 not_started.

## شواهد پیش‌نیازها و یال‌ها
- T0186: disclosure consent در Android و Windows، آزمون‌های `test_peer_inference_privacy_disclosure.py` و WPF/HostLifecycle در وضعیت done.
- T0177: `InferenceAttemptDispatcher`/`HostCandidateSelector` و آزمون‌های بودجه تلاش در وضعیت done.
- T0186 → T0187 و T0177 → T0187: type=blocks حفظ شد؛ طبقه‌بندی sequencing_only / validated شد؛ دلیل و فهرست شواهد در graph edge fields و edge_change_log ثبت شد.
- T0187 → T0188 هنوز unreviewed است.

## مبنای فنی بررسی‌شده
- Windows `SafeDiagnosticLog` فقط enumهای عددی و مدت زمان را می‌پذیرد و EventSource فقط سه مقدار عددی صادر می‌کند.
- Android `SafeDiagnosticLog.formatRecord` فقط schema/event/outcome/duration را تولید می‌کند.
- `WindowsHostConversationStore.Save` ابتدا `_protector.Protect(plaintext)` را صدا می‌زند؛ سپس فایل موقت را می‌سازد و فقط `protectedPayload` را می‌نویسد؛ plaintext و ciphertext bufferها در finally صفر می‌شوند.
- Android `EncryptedClientDatabaseFactory` از `sqlcipher_export` به فایل موقت مهاجرت می‌کند، مقصد رمز‌شده را validate می‌کند، سپس rename اتمی انجام می‌دهد و در خطا temp/sidecar را حذف می‌کند.

## فایل‌های تغییر‌یافته
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/SafeDiagnosticLogTest.kt`
- `tests/test_sensitive_artifact_scanning.py`
- `docs/security.md`
- `step three/T0187/execution-report.md`
- `step three/T0187/evidence/verification.md`
- `step three/project-graph.md`

## نتیجهٔ آزمون
- در زمان ایجاد snapshot، هیچ آزمون CI exact-head اجرا نشده بود؛ همهٔ نتیجه‌های وابسته به commit جدید pending هستند.
- اسکن‌ها فقط markerهای ساختگی را در artifactهای test-owned بررسی می‌کنند؛ ادعای اسکن کل filesystem یا دادهٔ خارج از کنترل برنامه مطرح نمی‌شود.


## A001 — نتیجهٔ شکست و شواهد اصلاح
- Commit: `876288a825eec50e8b23906a520ee21332fe7fae` (`V 3.0.877`).
- Quality workflow `41818883061`, job `126536652274`: ۳۷۷ آزمون اجرا شد و تنها شکست، انتظار متنی نادرست در `tests/test_sensitive_artifact_scanning.py` بود. عبارت واقعی منبع `record.Payload is not { Count: 3 } payload` است.
- Product workflow `41818883046`, HostLifecycle job `126536651616`: موفق؛ تست runtime canary برای Windows temp/DPAPI و EventSource اجرا و پاس شد.
- Rendezvous container workflow `41818882949`: موفق.
- WPF job `126536652244` و Android job `126536652473` در زمان ثبت شکست هنوز در حال اجرا بودند؛ موفقیت برای آن‌ها ادعا نشده است.
- اصلاح A002 فقط assertion source-contract را با عبارت واقعی هم‌تراز می‌کند؛ کنترل runtime و منطق امنیتی بدون تغییر می‌ماند.

 
## A003 — اصلاح پوشش capsule و فایل‌های موقت
 
- آزمون اندروید اکنون marker را در JSON معتبر `ConversationContextCapsule` و ستون واقعی `serializedCapsule` ذخیره می‌کند و پس از مهاجرت رمز‌شده از DAO بازیابی و decode می‌کند؛ از فیلد عنوان به‌عنوان جانشین capsule استفاده نمی‌شود.
- آزمون منبعی writerهای temporary در HostIdentityStore، PeerTrustStore، LocalHostSettingsStore و VerifiedModelInstaller را پوشش می‌دهد.
- ممیزی log به همهٔ `src/Host.Windows` گسترش یافت و حداقل یک log sink واقعی را الزام می‌کند.
- CI روی نسخهٔ قبل، `V 3.0.878`: quality، WPF، HostLifecycle، Android JVM/instrumentation و Rendezvous موفق بودند؛ تغییرات A003 برای پذیرش نیازمند اجرای مجدد CI هستند.


## A004 — شکست style و اصلاح حداقلی

- Commit `9b646018e4ec4d11b898aac5ade2c3ce9fa09af1`، `V 3.0.879`.
- Quality workflow `41820347836`: موفق.
- Rendezvous workflow `41820348035`: موفق.
- Product run `41820347748`: Windows WPF job `126541644091` موفق؛ HostLifecycle job `126541644094` موفق؛ Android job `126541644002` ناموفق.
- خروجی Android KtLint دقیقاً دو محل را نشان داد: خط 111 و 184، هر دو `A multiline expression should start on a new line`. این step پیش از build APK و JVM/instrumentation اجرا می‌شود؛ بنابراین برای V3.0.879 نتیجه‌ای دربارهٔ اجرای آن آزمون‌ها وجود ندارد.
- A004 دو assignment را به فرم چندخطی تبدیل می‌کند و منطق آزمون capsule BLOB، SQLCipher migration و scan canary را حفظ می‌کند.


## A005 — جزئیات KtLint

- در `V 3.0.880` دو خطا از نوع `Unexpected indentation` در پارامترهای فراخوانی `ConversationContextCapsule` و `checkNotNull` مشاهده شد: expected indentation به‌ترتیب 20/16 و 24/20 ستون.
- A005 فقط indentation پارامترها و پرانتز بسته را مطابق همین مقادیر اصلاح می‌کند. هیچ test assertion یا production behavior تغییر نکرد.


## پذیرش نهایی exact-head — V 3.0.881

- Commit: `bf790e4c6aeb962bee27007fdbb4527d7ac05cd4`، پیام `V 3.0.881`، شاخه `main`.
- Quality gate: [workflow 41821419666](https://github.com/artinrahimii/project/actions/runs/41821419666) — موفق؛ 381 آزمون Python، خروجی `Ran 381 tests in 0.235s` و `OK`.
- Product workflow: [workflow 41821419520](https://github.com/artinrahimii/project/actions/runs/41821419520) — موفق؛ Android job `126545230910`، Windows WPF job `126545231223`، HostLifecycle job `126545231339`.
- Android: KtLint، APK build، permission policy، JVM tests و Android Keystore instrumentation همگی موفق. تمام 26 stepهای job با conclusion=success خاتمه یافتند؛ artifact گزارش instrumentation با Artifact ID `13318300037` بارگذاری شد.
- Windows WPF: build و smoke/startup و policy/DPAPI checks موفق؛ artifact میزبان با Artifact ID `13317485661` بارگذاری شد.
- HostLifecycle: job کامل و موفق؛ آزمون‌های Windows temporary history/DPAPI و EventSource canary پاس شدند.
- Rendezvous: [workflow 41821419570](https://github.com/artinrahimii/project/actions/runs/41821419570) — build و smoke test موفق.
- GitHub Actions دقیقاً روی SHA `bf790e4c6aeb962bee27007fdbb4527d7ac05cd4` اجرا شدند؛ commit نهایی metadata/status برای بستن وظیفه، در نسخهٔ بعدی ثبت می‌شود.

## محدودیت‌های پوشش

- آزمون‌ها مسیرهای log، فایل‌های موقت و artifacts تحت کنترل خود برنامه را بررسی می‌کنند؛ این شواهد تضمین حذف داده از pagefile، crash dump، logهای ابزارهای خارجی یا snapshotهای سیستم‌عامل نیست.
- فایل staging دانلود مدل برای محتوای مدل است، نه request/capsule/secret و این تفاوت در آزمون منبعی ثبت شده است.
- آزمون‌های محلی در محیط اجرا انجام نشدند؛ .NET SDK نصب نبود و clone شبکه‌ای به GitHub به‌دلیل DNS ممکن نبود. پذیرش عملی با CI آنلاین exact-head انجام شد.


## تطبیق metadata و بازبینی successor

- commit `V 3.0.882` وضعیت تکمیل T0187 را روی شاخهٔ `main` ثبت کرد، اما graph guard به‌دلیل استفاده از مقدار غیرمجاز `success` به‌جای `succeeded` در نتیجهٔ A005 شکست خورد.
- این نوبت فقط مقدار نتیجه را به enum مجاز `succeeded` نرمال می‌کند؛ attempt ID، SHA، زمان‌ها، آزمون‌های موفق/ناموفق و تعداد پنج تلاش را حفظ می‌کند.
- دو یال ورودی T0188 با شواهد واقعی `sequencing_only / validated` شدند و در edge_change_log ثبت شدند؛ type=blocks و ترتیب اصلی تغییر نکرد.

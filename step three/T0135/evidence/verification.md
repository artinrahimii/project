# T0135 — شواهد و اعتبارسنجی نهایی

## گراف در نسخهٔ دقیق مبنا

- Branch: `main`; commit: `0f2e7efcd21d5a1d44f116054021a8adcd84ddf1` (`V 3.0.475`); tree: `b958d8d3c7da90d16211907bce5b7fcb0fd63f68`.
- مسیر: `step three/project-graph.md`; Blob tree: `d00d88bb7da70254eca07ec92f10dadfa80153de`.
- Blob از Git Trees API و سپس Git Blob API دریافت شد.
- اندازهٔ اصلی در tree / UTF-8 محاسبه‌شده: **2,300,005 بایت**؛ طول رشته **2,177,905 نویسه**.
- Git Blob SHA-1 روی کل محتوای اصلی، از جمله fenced Markdown، با `d00d88bb7da70254eca07ec92f10dadfa80153de` برابر بود؛ JSON کامل parse شد.
- آمار گراف: 229 گره، 420 یال فعال؛ 126 `done`، یک `blocked`، 101 `not_started` و T0135 در حال انجام بود؛ 196 یال `unreviewed`.

## کد/قرارداد

- `src/Host.Windows/Requests/SecureRequestIdGenerator.cs`: شناسهٔ opaque با ۱۶ بایت رمزنگاری‌شده، بدون ورودی هویتی.
- `protocol/whitep/v1/messages.proto`: request/attempt IDs مستقل و بدون فیلد پایدار کاربر/گفت‌وگو.
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`: حفظ request ID در پاسخ خطای typed.
- `tests/HostLifecycle.Tests/RequestIdentityIndependenceAcceptanceTests.cs` و `tests/HostLifecycle.Tests/Program.cs`: آزمون پذیرش و اتصال به runner.
- `docs/decisions/ADR-0013-temporary-request-identifiers.md`: تصمیم معماری مرتبط.

## نتایج CI روی دقیقاً V 3.0.475

- HostLifecycle: **PASS**, شامل خروجی صریح آزمون جدید — [run 40733512116 / job 122975807228](https://github.com/artinrahimii/project/actions/runs/40733512116/job/122975807228).
- Step Three quality gate: **PASS** — [run 40733512114](https://github.com/artinrahimii/project/actions/runs/40733512114).
- Windows WPF host: **PASS** — [run 40733512116 / job 122975807074](https://github.com/artinrahimii/project/actions/runs/40733512116/job/122975807074).
- Android APK/lint/Protobuf: **PASS** — [run 40733512116 / job 122975806646](https://github.com/artinrahimii/project/actions/runs/40733512116/job/122975806646).
- Real local inference integration: **PASS** — [run 40733512148](https://github.com/artinrahimii/project/actions/runs/40733512148).
- تلاش اول V 3.0.474 با `CS1061` شکست خورد و در execution history نگه داشته شده است؛ تلاش دوم بعد از اصلاح موفق شد.

## مرز نتیجه

T0135 مطابق معیار پذیرش قرارداد/شناسهٔ درخواست تکمیل شد. این نتیجه اجرای end-to-end روی دو شبکهٔ مستقل نیست؛ `T0128` همچنان `not_started` باقی می‌ماند.

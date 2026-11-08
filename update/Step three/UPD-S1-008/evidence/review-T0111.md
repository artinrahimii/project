# بازبینی محتوایی T0111

## خط مبنا و تصمیم
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit دقیق بررسی‌شده: `58df066c3feb44020e59d75870a3d1c66642d604` — `V 3.1.83`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد؛ `status_changed=false`
- وظایف Update مرتبط: `UPD-S1-004`، `UPD-S1-012`، `UPD-S1-017`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- review_commit_sha: `58df066c3feb44020e59d75870a3d1c66642d604`

## یافته‌های پیاده‌سازی و آزمون
`ProtocolMessageParser` اندازهٔ بایت‌های پیام را قبل از Protobuf parsing کنترل می‌کند و سپس validator مشترک را اجرا می‌کند. Validator شناسه‌های request/attempt، مدل، prompt، نسخهٔ دقیق پروتکل، deadline معتبر/آینده، Max Output Tokens مثبت و سقف پروتکل، اندازهٔ prompt و capsule را کنترل می‌کند. `PeerAnnouncementHostLifecycle` هویت peer احرازشده را با PeerId مورد انتظار مقایسه و replay guard را پیش از admission اعمال می‌کند؛ MainWindow نیز readiness و opt-in را دوباره در admission بررسی می‌کند. HostLifecycle فعلی، frame/parser/protocol rejection و دستکاری شناسه را PASS ثبت کرده است. آزمون Kotlin `ProtoRoundTripTest` و Python contract tests وجود دارند، اما در لحظهٔ این بازبینی Android JVM test job روی V3.1.83 هنوز در حال اجرا بود؛ بنابراین آن آزمون cross-platform را به‌عنوان PASS دقیق همین commit ثبت نمی‌کنم.

## فایل‌های بررسی‌شده
- `src/Shared.Contracts/ProtocolMessageValidator.cs`
- `src/Shared.Contracts/ProtocolMessageLimits.cs`
- `src/Host.Infrastructure/ProtocolMessageParser.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt`
- `tests/test_proto_contract.py`
- `tests/test_message_validation.py`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0110 -> T0111 contract_or_interface_dependency/validated
- T0107 -> T0111 sequencing_only/validated
- T0111 -> T0112 contract_or_interface_dependency/validated
- هیچ وضعیت قدیمی، تاریخچه یا یال بررسی‌شده بازنویسی نشد. یال unreviewed به `T0128` برای T0115 عمداً باز ماند.

## CI دقیق
- Step Three Quality Gate: [run 42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) — success.
- HostLifecycle در Product Build: [run 42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) — success؛ PASS markerهای مربوط به parser، response mapping و privacy در log موجودند.
- Rendezvous container: [run 42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) — success.
- در همان لحظهٔ ثبت بازبینی، Android JVM/Keystore و Windows installer مراحل هنوز در حال اجرا بودند؛ این وضعیت به‌عنوان success کامل محصول گزارش نمی‌شود.

## دلیل بازبینی مجدد
UPD-S1-004/012/017 ممکن است فیلدها و قواعد اعتبارسنجی را تغییر دهند؛ round-trip هر دو زبان و تمام ردهای semantic را روی قرارداد نهایی دوباره اجرا کن.

## مرز شواهد
این بازبینی کد و CI موجود را ثبت می‌کند، نه آزمون عمومی اینترنت، نه دستگاه Android فیزیکی متصل به میزبان نصب‌شده و نه تحویل مدل واقعی end-to-end. گراف اولیه تا تکمیل کل reconciliation نشانگرهای `update_impact.review_status=pending` را حفظ می‌کند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation است.

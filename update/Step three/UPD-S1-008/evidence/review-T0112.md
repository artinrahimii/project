# بازبینی محتوایی T0112

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
`NetworkInferenceRequestResponseMapper` پاسخ‌های typed مانند HostBusy، UnsupportedModel و ProtocolError را از failureها می‌سازد، شناسه‌های request/attempt/round را حفظ و متن ثابت و پاک‌سازی‌شده برمی‌گرداند؛ acceptance testهای آن Busy، mismatch مدل، invalid/version/expired، capacity و internal error را بررسی می‌کنند. با این حال، جست‌وجوی call-site در مسیر زنده نشان داد MainWindow و PeerAnnouncementHostLifecycle از سازنده‌های `InferenceResponse` مخصوص همان مسیر استفاده می‌کنند و mapper مستقل T0112 در آن مسیر مستقیماً فراخوانی نمی‌شود. خود `PeerAnnouncementHostLifecycle` پاسخ Busy/replay/cancel و response correlation را روی قرارداد `InferenceResponse` می‌سازد و ارسال می‌کند؛ پس رفتار معادل در مسیر تولیدی وجود دارد، اما دو محل mapping باید با قرارداد واحد و Android `RemoteRequestErrorParser` بازبینی و همگام شوند. تست mapper ثابت نمی‌کند که یک پیام HostBusy/UnsupportedModel مستقل روی شبکه تحویل شده است.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RemoteRequestErrorParser.kt`
- `protocol/whitep/v1/messages.proto`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RemoteRequestErrorParserTest.kt`

## وابستگی‌ها
- T0111 -> T0112 contract_or_interface_dependency/validated
- T0107 -> T0112 contract_or_interface_dependency/validated
- T0112 -> T0113 sequencing_only/validated
- هیچ وضعیت قدیمی، تاریخچه یا یال بررسی‌شده بازنویسی نشد. یال unreviewed به `T0128` برای T0115 عمداً باز ماند.

## CI دقیق
- Step Three Quality Gate: [run 42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) — success.
- HostLifecycle در Product Build: [run 42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) — success؛ PASS markerهای مربوط به parser، response mapping و privacy در log موجودند.
- Rendezvous container: [run 42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) — success.
- در همان لحظهٔ ثبت بازبینی، Android JVM/Keystore و Windows installer مراحل هنوز در حال اجرا بودند؛ این وضعیت به‌عنوان success کامل محصول گزارش نمی‌شود.

## دلیل بازبینی مجدد
UPD-S1-004/017 قرارداد پاسخ و typed errors اندروید را تغییر می‌دهند؛ mapping مستقل و helperهای تولیدی را به یک قرارداد سازگار تبدیل/تأیید کن و correlation، cancellation، Busy، mismatch و internal error را روی مسیر libp2p دوباره تست کن.

## مرز شواهد
این بازبینی کد و CI موجود را ثبت می‌کند، نه آزمون عمومی اینترنت، نه دستگاه Android فیزیکی متصل به میزبان نصب‌شده و نه تحویل مدل واقعی end-to-end. گراف اولیه تا تکمیل کل reconciliation نشانگرهای `update_impact.review_status=pending` را حفظ می‌کند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation است.

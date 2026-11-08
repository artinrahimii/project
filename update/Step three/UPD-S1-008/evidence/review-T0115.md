# بازبینی محتوایی T0115

## خط مبنا و تصمیم
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit دقیق بررسی‌شده: `58df066c3feb44020e59d75870a3d1c66642d604` — `V 3.1.83`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد؛ `status_changed=false`
- وظایف Update مرتبط: `UPD-S1-012`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- review_commit_sha: `58df066c3feb44020e59d75870a3d1c66642d604`

## یافته‌های پیاده‌سازی و آزمون
Suite TCP loopback برای client قطع‌شده، frame خراب/oversized/truncated، request semantic نامعتبر، اتصال‌های ترتیبی مستقل، غیرفعال‌بودن participation هنگام dispatch، عدم ثبت prompt در log و shutdown هنگام انتظار frame آزمون دارد و در HostLifecycle دقیق V3.1.83 PASS marker ثبت کرده است. علاوه‌بر آن، PeerAnnouncementClientAcceptanceTests چرخهٔ lifecycle و تبادل پیام در transport libp2p با rendezvous loopback را بررسی می‌کند؛ این تست، شبکهٔ عمومی یا دستگاه Android فیزیکی نیست. outgoing edgeهای validated حفظ شدند و یال T0115→T0128 که هنوز unreviewed است دست‌نخورده باقی ماند.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs`
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0114 -> T0115 sequencing_only/validated
- T0107 -> T0115 sequencing_only/validated
- T0115 -> T0116/T0117/T0118/T0119/T0120/T0121/T0122/T0123/T0124/T0125/T0126/T0127 sequencing_only/validated
- T0115 -> T0128 unreviewed/unreviewed (preserved)
- هیچ وضعیت قدیمی، تاریخچه یا یال بررسی‌شده بازنویسی نشد. یال unreviewed به `T0128` برای T0115 عمداً باز ماند.

## CI دقیق
- Step Three Quality Gate: [run 42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) — success.
- HostLifecycle در Product Build: [run 42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) — success؛ PASS markerهای مربوط به parser، response mapping و privacy در log موجودند.
- Rendezvous container: [run 42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) — success.
- در همان لحظهٔ ثبت بازبینی، Android JVM/Keystore و Windows installer مراحل هنوز در حال اجرا بودند؛ این وضعیت به‌عنوان success کامل محصول گزارش نمی‌شود.

## دلیل بازبینی مجدد
UPD-S1-012 production transport/listener lifecycle را تغییر می‌دهد؛ disconnect, malformed message, repeated attempt, stop/drain و shutdown را روی مسیر نهایی و سپس روی Android↔Windows واقعی دوباره اجرا کن.

## مرز شواهد
این بازبینی کد و CI موجود را ثبت می‌کند، نه آزمون عمومی اینترنت، نه دستگاه Android فیزیکی متصل به میزبان نصب‌شده و نه تحویل مدل واقعی end-to-end. گراف اولیه تا تکمیل کل reconciliation نشانگرهای `update_impact.review_status=pending` را حفظ می‌کند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation است.

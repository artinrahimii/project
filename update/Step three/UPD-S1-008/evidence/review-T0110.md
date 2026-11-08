# بازبینی محتوایی T0110

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
کد `NetworkInferenceRequestFrameReader` اندازهٔ raw frame را پیش از تخصیص payload کنترل می‌کند، prefix چهار‌بایتی big-endian را می‌خواند و Protobuf را از طریق `ProtocolMessageParser.ParseInferenceRequest` اعتبارسنجی می‌کند. آزمون فعلی frame-reader از loopback TCP واقعی استفاده می‌کند. مسیر تولیدی libp2p نیز در `PeerAnnouncementHostLifecycle` پیام دریافتی را مستقیماً با همان parser بررسی و سپس به handler ویندوز می‌دهد؛ در نتیجه parser مشترک در مسیر تولیدی مصرف می‌شود، اما framing چهار‌بایتی TCP مخصوص listener جداگانه است و نباید به‌عنوان framing مسیر libp2p معرفی شود. آزمون loopback قبول‌شدن frame و رد ورودی خراب/بزرگ/ناقص را تأیید می‌کند؛ مسیر callback در listener قدیمی به‌تنهایی inference یا پاسخ شبکه‌ای را ثابت نمی‌کند.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Networking/NetworkInferenceRequestFrameReader.cs`
- `src/Host.Infrastructure/ProtocolMessageParser.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0109 -> T0110 contract_or_interface_dependency/validated
- T0107 -> T0110 sequencing_only/validated
- T0110 -> T0111 contract_or_interface_dependency/validated
- هیچ وضعیت قدیمی، تاریخچه یا یال بررسی‌شده بازنویسی نشد. یال unreviewed به `T0128` برای T0115 عمداً باز ماند.

## CI دقیق
- Step Three Quality Gate: [run 42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) — success.
- HostLifecycle در Product Build: [run 42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) — success؛ PASS markerهای مربوط به parser، response mapping و privacy در log موجودند.
- Rendezvous container: [run 42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) — success.
- در همان لحظهٔ ثبت بازبینی، Android JVM/Keystore و Windows installer مراحل هنوز در حال اجرا بودند؛ این وضعیت به‌عنوان success کامل محصول گزارش نمی‌شود.

## دلیل بازبینی مجدد
UPD-S1-004/012/017 قرارداد پیام و transport را تغییر می‌دهند؛ framing legacy TCP و پیام‌های libp2p را جدا نگه دار و سازگاری Android/Windows را روی transport نهایی دوباره آزمون کن.

## مرز شواهد
این بازبینی کد و CI موجود را ثبت می‌کند، نه آزمون عمومی اینترنت، نه دستگاه Android فیزیکی متصل به میزبان نصب‌شده و نه تحویل مدل واقعی end-to-end. گراف اولیه تا تکمیل کل reconciliation نشانگرهای `update_impact.review_status=pending` را حفظ می‌کند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation است.

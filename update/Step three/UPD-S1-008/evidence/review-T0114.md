# بازبینی محتوایی T0114

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
کلاس legacy TCP listener هر bind غیر-loopback را پیش از بازکردن socket رد می‌کند و در تست constructor این مرز بررسی شده است. مسیر واقعی hosting WPF از `PeerAnnouncementHostLifecycle` استفاده می‌کند، نه از آن listener آزمایشی؛ WPF bind/advertised IP و port مشخص، rendezvous HTTPS (به‌جز loopback test)، PeerId مورد اعتماد و confirmation دستی reachability را درخواست می‌کند و پس از opt-in، readiness backend/capacity و رضایت privacy سرویس را فعال می‌کند. UI حالت hosting و تنظیمات IP/port را نشان می‌دهد، اما اپلیکیشن reachability عمومی را خودش probe نمی‌کند؛ این محدودیت به کاربر اعلام می‌شود. تست‌های Python این wiring و shutdown behavior را به صورت source-contract کنترل می‌کنند؛ پذیرش مستقل روی شبکهٔ عمومی هنوز اثبات نشده است.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/MainWindow.xaml`
- `tests/test_network_listener_exposure.py`
- `docs/decisions/ADR-0015-fail-closed-participation-listener-lifecycle.md`
- `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs`
- `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`
- `tests/test_network_listener_exposure.py`

## وابستگی‌ها
- T0113 -> T0114 sequencing_only/validated
- T0107 -> T0114 sequencing_only/validated
- T0114 -> T0115 sequencing_only/validated
- هیچ وضعیت قدیمی، تاریخچه یا یال بررسی‌شده بازنویسی نشد. یال unreviewed به `T0128` برای T0115 عمداً باز ماند.

## CI دقیق
- Step Three Quality Gate: [run 42508306358](https://github.com/artinrahimii/project/actions/runs/42508306358) — success.
- HostLifecycle در Product Build: [run 42508306341](https://github.com/artinrahimii/project/actions/runs/42508306341) — success؛ PASS markerهای مربوط به parser، response mapping و privacy در log موجودند.
- Rendezvous container: [run 42508306222](https://github.com/artinrahimii/project/actions/runs/42508306222) — success.
- در همان لحظهٔ ثبت بازبینی، Android JVM/Keystore و Windows installer مراحل هنوز در حال اجرا بودند؛ این وضعیت به‌عنوان success کامل محصول گزارش نمی‌شود.

## دلیل بازبینی مجدد
UPD-S1-012 transport/participation را تغییر می‌دهد؛ UI وضعیت bind/port و خاموش/روشن بودن را بعد از اتصال نهایی دوباره بررسی کن، و reachability واقعی را فقط با شاهد شبکهٔ مستقل تأیید کن.

## مرز شواهد
این بازبینی کد و CI موجود را ثبت می‌کند، نه آزمون عمومی اینترنت، نه دستگاه Android فیزیکی متصل به میزبان نصب‌شده و نه تحویل مدل واقعی end-to-end. گراف اولیه تا تکمیل کل reconciliation نشانگرهای `update_impact.review_status=pending` را حفظ می‌کند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation است.

# UPD-S1-012 — شواهد ممیزی مشارکت میزبان Windows

## خط مبنا و CI

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit ممیزی: `5aea044ec035aa9328ba5cc4f09f5df7b8537c74` (`V 3.1.144`)
- Windows/Android product build: https://github.com/artinrahimii/project/actions/runs/42784091033
- HostLifecycle build and acceptance job: https://github.com/artinrahimii/project/actions/runs/42784091033/job/129720877333
- Quality gate: https://github.com/artinrahimii/project/actions/runs/42784091020

## نگاشت معیار به کد

| معیار | مرجع کد |
|---|---|
| پیش‌فرض خاموش | `src/Host.Windows/MainWindow.xaml`؛ `MainWindow()` در `MainWindow.xaml.cs` |
| اقدام صریح مالک و هشدار حریم خصوصی | `NetworkParticipationToggle_Changed` و `ChangeNetworkParticipationAsync` در `MainWindow.xaml.cs` |
| trust صریح برای PeerId ناشناخته | `PeerTrustStore.TrustPeerAfterExplicitConfirmation` و گیت `EnsureTrusted` |
| آمادگی backend و ظرفیت پیش از فعال‌سازی | `_session.IsConnected`، `_capacityReady` و `HostReadinessEvaluator` |
| بستن admission پیش از shutdown | `PeerAnnouncementHostLifecycle.StopAsync`؛ نوشتن `_active = 0` قبل از cancellation |
| عدم پذیرش پس از خاموشی | guard وضعیت فعال در receive loop پیش از تخصیص slot و ثبت handler |
| توقف امن پنجره | `MainWindow_Closing`؛ cancel و await transition و سپس dispose سرویس |

## شواهد آزمون

- `PeerAnnouncementClientAcceptanceTests.RunAsync` اجرا می‌شود و `PeerHostLifecyclePublishesAndServesTrustedPeerAsync` را فراخوانی می‌کند.
- آزمون peer-host شامل start/listen، انتشار metadata، اتصال امن peer با هویت مورد انتظار، HostStatus، correlation پاسخ، رد replay، cancellation و انقضای lease پس از disconnect است.
- `ParticipationNetworkListenerLifecycleTests` توقف، آزادشدن endpoint، restart و رد فعال‌سازی پس از dispose را بررسی می‌کند. این کلاس lifecycle listener loopback را می‌آزماید و به‌تنهایی پیاده‌سازی peer production نیست.
- `tests/test_network_listener_exposure.py` اتصال واقعی WPF به `PeerAnnouncementHostLifecycle`، consent، trust، cleanup خطای فعال‌سازی و ترتیب shutdown را در source regression بررسی می‌کند.
- HostLifecycle CI job در اجرای محصول روی commit فوق موفق بوده و PASS markers مربوطه را ثبت کرده است.
- Quality CI، 491 آزمون Python را با نتیجه OK اجرا کرده است.

## محدودیت آزمون

در سناریوی peer-host، backend HTTP و خروجی inference آزمایشی‌اند و آدرس‌ها loopback هستند. این مدرک برای lifecycle و پروتکل مشارکت میزبان استفاده شده، نه برای ادعای inference واقعی یا دسترسی‌پذیری عمومی. آزمون دستگاه فیزیکی Android به Windows در این اجرا انجام نشده است.

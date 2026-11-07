# گزارش اجرای UPD-S1-003 — تعداد میزبان و سیاست retry

## وضعیت فعلی

- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit مبنای این تلاش: `38c63f441cd3991da105ba1edafc319c30dc7241` — `V 3.1.26`
- گرهٔ پیش‌نیاز UPD-S1-001: `done`
- وضعیت UPD-S1-003: `in_progress`؛ بررسی اولیه انجام شده، تصمیم نهایی و تغییر کد هنوز در این گزارش ثبت نشده است.
- شناسهٔ تلاش: `UPD-S1-003-20261107203548`
- زمان شروع: `2026-11-07T20:35:48.198Z`

## سؤال محصول

گزارش به‌روزرسانی در بخش 8.2، یک میزبان در هر درخواست و انتقال ترتیبی پس از پایان یا لغو ایمن تلاش قبلی را پیشنهاد می‌کند. در مقابل، نسخهٔ فعلی برنامه سه میزبان مستقل را برای هر دور انتخاب و هم‌زمان dispatch می‌کند. این اختلاف فقط تغییر UI نیست؛ الگوریتم، سقف تلاش، رفتار لغو و تست‌های محصول را تحت تأثیر قرار می‌دهد.

## ممیزی کد فعلی

- `FirstReleaseInferenceLimits` در `DistributedRequest.kt`: سه دور، حداکثر سه میزبان در هر دور و حداکثر نه میزبان برای یک درخواست.
- `DistributedRequest` به‌صورت پیش‌فرض برای سه دور، بودجهٔ نه میزبان ایجاد می‌کند.
- `HostCandidateSelector`: هویت‌های تکراری را رد می‌کند، PeerIdها را مرتب می‌کند، سازگاری مدل/capacity و آمادگی را بررسی می‌کند؛ `selectFirstRound` تا سقف سه میزبان برمی‌گرداند.
- `AuthenticatedHostDiscovery`: اعلان‌های Rendezvous را بر اساس PeerId مرتب می‌کند و تا رسیدن به تعداد درخواست‌شده، برای میزبان‌ها status probe احرازشده می‌فرستد. با `maxHosts=1` قادر است پس از اولین میزبان معتبر متوقف شود؛ نیازی به broadcast کردن prompt به همهٔ peers ندارد.
- `InferenceAttemptDispatcher`: برای فهرست میزبان‌های انتخاب‌شده، attempt مستقل می‌سازد و همه را dispatch می‌کند؛ سطح فعلی امکان ارسال هم‌زمان تا سه میزبان را دارد.
- `InferenceRoundResponseCollector`: attemptهای round را جمع‌آوری می‌کند و تا deadline مشترک پنجرهٔ پاسخ، نتیجه‌های مستقل را نگه می‌دارد.
- `InferenceRoundTimer`: timeout پیش‌فرض هر round برابر ۶۰ ثانیه است و deadline نهایی درخواست را نیز رعایت می‌کند.
- `RequestViewModel`: request deadline فعلی ۹۰ ثانیه است؛ ارسال اولیه تا سه میزبان را می‌خواهد؛ retry دستی و user-triggered است، attemptId و roundId تازه می‌سازد و requestId را حفظ می‌کند. برای retry، deadline را از نو تنظیم می‌کند و میزبان‌های موفق/لغوشده را مستثنا می‌کند.
- UI فعلی برای لغو، پیام correlated `CancelRequest` را برای تمام attemptهای queued ارسال می‌کند؛ وضعیت `CANCELLED` terminal است و retry نمی‌شود.
- transport Android لغو را با requestId و attemptId دقیق به stream احرازشده می‌فرستد. در میزبان Windows، CancellationToken به همان request/attempt پیوند دارد؛ در صورت disconnect stream، handler لغو و taskهای فعال drain می‌شوند. اگر لغو قبل از deadline پردازش شود، میزبان پاسخ `CANCELLED` correlated می‌فرستد؛ پس از deadline پاسخ لغو ممکن است ارسال نشود.
- پاسخ‌ها با requestId، attemptId، roundId، PeerId و deadline اعتبارسنجی می‌شوند. response دیررس یا تکراری نباید به تلاش یا درخواست دیگر نسبت داده شود.
- زمان‌بندی فعلی retry خودکار نیست؛ کاربر دکمهٔ retry را می‌زند. این رفتار باید در تصمیم جدید صریح بماند یا با دلیل محصولی تغییر کند.

## وظایف قدیمی متأثر (۲۳ شناسه)

### پیاده‌سازی سه‌میزبانهٔ موجود که باید حفظ شود یا به قابلیت آینده منتقل شود

- `T0125` و `T0126`: انتخاب سه PeerId متمایز و رفتار کمتر از سه میزبان.
- `T0162`: selection تا سه میزبان ready و سازگار.
- `T0166` و `T0167`: کارت‌های پاسخ مستقل و بدون merge خودکار.
- `T0168`: ادامهٔ دریافت پاسخ‌های مستقل تا پایان پنجرهٔ round.
- `T0169` و `T0170`: round بعدی در صورت نرسیدن پاسخ و سقف round/کل میزبان‌ها.
- `T0177`: بودجهٔ سه میزبان برای هر round.
- `T0203` و `T0204`: آزمون‌های سه فرایند میزبان و round دوم.

این قابلیت‌ها در گراف اولیه دارای سوابق `done` هستند. صرفاً به‌سبب تغییر سیاست نسخهٔ نخست نباید تاریخچه یا وضعیتشان پاک شود. تصمیم جدید باید روشن کند کدام بخش‌ها به compatibility/future scope منتقل می‌شوند و کدام معیارهای پذیرش نسخهٔ نخست تغییر می‌کنند.

### سازوکارهای عمومی که باید حفظ و با سیاست جدید سازگار شوند

- `T0161`: request lifecycle، deadline و budget.
- `T0163`: شناسه‌های مستقل attempt و correlation.
- `T0164`: timer تزریق‌پذیر و timeout.
- `T0165`: اعتبارسنجی correlation پاسخ.
- `T0171`: عدم retry روی همان PeerId مگر با سیاست صریح.
- `T0172`: رد پاسخ دیرهنگام.
- `T0173`: idempotency برای پاسخ تکراری.
- `T0174`: لغو correlated درخواست.
- `T0175`: وضعیت failed و retry دستی.
- `T0176`: آزمون صفر/یک/دو/سه پاسخ، timeout، late/duplicate و cancel؛ آزمون‌های چندمیزبانه می‌توانند برای compatibility باقی بمانند.

### وظایف برنامه‌ریزی‌شده ولی شروع‌نشده

- `T0224` و `T0225`: هنوز `not_started` و بدون execution history هستند. تکمیل آن‌ها در سیاست نسخهٔ نخست نباید جعل شود؛ در تصمیم باید صریحاً به‌عنوان کار آینده یا بازطراحی‌شده تعیین تکلیف شوند.

## تصمیم نهایی

**هنوز در این مرحله اتخاذ نشده است.** شواهد کد با پیشنهاد بخش 8.2 سازگارند، اما ابتدا باید سیاست نهایی cardinality، مجموع تلاش مجاز، deadline کلی، رفتار timeout/disconnect/cancel و طبقه‌بندی دقیق همهٔ ۲۳ شناسه مشخص و در ADR مستقل ثبت شود. کد محصول و آزمون‌ها پس از تثبیت این تصمیم اصلاح خواهند شد.

## شواهد این تلاش

- Update requirements: `update/Step one/Output/V1.10.json`, UPD-S1-003.
- Product proposal: `update/Step one/2026-11-07-automatic-chat-and-host-routing.md`, بخش 8.2.
- Android selector: `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`.
- Android discovery: `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`.
- Android dispatch/domain: `DistributedRequest.kt`, `InferenceAttemptDispatcher.kt`.
- Timer/collector: `InferenceRoundTimer.kt`, `InferenceRoundResponseCollector.kt`.
- UI retry/cancel: `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`.
- Android transport: `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`.
- Windows cancellation: `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`.
- Old graph source: `step three/project-graph.md`; 23 affected IDs were found and matched.

## آزمون‌ها

- در این تلاش فقط ممیزی منبع و تاریخچه انجام شد؛ آزمون جدید یا اجرای محصولی انجام نشده است.
- آزمون‌های baseline مربوط به V 3.1.24 روی commit مستقل خود قبلاً موفق بوده‌اند؛ این موفقیت‌ها جای آزمون تغییرات آتی UPD-S1-003 را نمی‌گیرند.

# گزارش اجرای UPD-S1-001 — ممیزی خط مبنای واقعی برنامه‌ها

## نتیجهٔ این تلاش

وضعیت اولیهٔ گزارش در زمان ثبت نخستین تلاش in_progress بود. نتیجهٔ نهایی پس از اصلاح آزمون‌ها و اجرای کامل CI در بخش «نتیجهٔ نهایی» درج شده است.

## خط مبنای مخزن

- مخزن: artinrahimii/project
- شاخهٔ آنلاین: main
- commit مبنا: afa79587ee403942b5ba2f1e91b3e7ccc2988f08
- پیام: V 3.1.17
- زمان commit: 2026-11-07T18:28:42Z
- Git tree: 44cd148a28d4232b481e92571a5713b741fd3cb4
- آخرین commit تغییر‌دهندهٔ کد محصول در checkpoint گراف: 8fa9aa812aeb49086a6d55e92ba17586534135a4 — V 3.1.11
- مقایسهٔ checkpoint گراف 827d1586e367d8495ef052ac71a8cc83db0f4680 با HEAD فعلی، تغییرات را به workflowها، گراف‌ها، reconciliation و آزمون قراردادی محدود می‌کند؛ در این بازه سورس اجرایی Android/Windows/Shared تغییر نکرده است.
- وضعیت worktree محلی از طریق GitHub API قابل خواندن نیست؛ این گزارش فقط وضعیت شاخه و درخت آنلاین را تأیید می‌کند.

## مسیرهای واقعی محصول

### Android

- برنامهٔ اصلی: src/Client.Android/؛ Kotlin، Jetpack Compose و Gradle.
- MainActivity.kt مسیرهای Home، Conversations، Conversation detail و New request را جداگانه register می‌کند.
- گفت‌وگوها: ConversationListScreen.kt و ConversationListViewModel.kt امکان ایجاد، بازکردن، تغییر عنوان و حذف گفت‌وگو را از طریق DAO دارند.
- پیام محلی: ConversationDetailScreen.kt و ConversationMessagesViewModel.kt پیام کاربر را با messageDao.upsert ذخیره می‌کنند. خود ViewModel تصریح می‌کند که محتوا را به میزبان پردازش نمی‌فرستد؛ صفحه نیز می‌گوید ارسال به میزبان هنوز در دسترس نیست.
- ذخیره‌سازی: ClientDatabase.kt، Room و migrationهای نسخه‌های ۱ تا ۵ را تعریف می‌کند.
- درخواست شبکه در صفحهٔ جداگانهٔ New request است: RequestViewModel.kt، AndroidInferenceRequestRuntime.kt، AuthenticatedHostDiscovery.kt، HostCandidateSelector.kt، InferenceAttemptDispatcher.kt و Libp2pInferenceRequestTransport.kt.
- فرم درخواست جداگانه، Rendezvous URL، Peer ID برای بررسی trust، شناسهٔ دقیق مدل و سقف توکن را به کاربر نشان می‌دهد. این مسیر هنوز با ارسال پیام در گفت‌وگوی Room یکی نشده است.
- runtime و transport کد واقعی دارند؛ وجود آن‌ها، به‌تنهایی، اتصال گفت‌وگوی عادی، دریافت پاسخ در همان conversation یا ذخیرهٔ پاسخ assistant در Room را اثبات نمی‌کند.

### Windows

- برنامهٔ WPF: src/Host.Windows/؛ entry point در App.xaml.cs و UI اصلی در MainWindow.xaml و MainWindow.xaml.cs.
- backend محلی: LlamaCppInferenceEngine.cs و LlamaCppServerBackend.cs؛ health check و فهرست مدل‌های endpoint محلی بررسی می‌شوند. کد می‌گوید خود llama.cpp server مدل را هنگام startup بارگذاری می‌کند؛ این موضوع جای اجرای مدل واقعی روی میزبان هدف را نمی‌گیرد.
- شروع process محلی از طریق LocalInferenceProcessLauncher.cs وجود دارد، اما مسیر UI فعلی همچنان server URL و model ID را پیکربندی می‌کند.
- مشارکت شبکه از طریق PeerAnnouncementHostLifecycle و ParticipationGatedNetworkListener پیاده شده و در startup وضعیت اولیه به‌صورت غیرفعال گزارش می‌شود؛ فعال‌سازی listener به مسیر کنترل مشارکت وابسته است.
- درخواست ورودی از HandlePeerInferenceMessageAsync به اعتبارسنجی پیام، کنترل مشارکت، سیاست پذیرش و HostRequestCoordinator می‌رود.
- HostRequestCoordinator با SemaphoreSlim(1, 1) هم‌زمانی inference را به یک درخواست محدود می‌کند.
- HostRequestAdmissionPolicy مشارکت فعال، وضعیت مدل، تطابق Model ID، ظرفیت و deadline را fail-closed کنترل می‌کند.

### قراردادها و امنیت مشترک

- منبع قرارداد نسخه‌دار: protocol/whitep/v1/messages.proto.
- کدهای مشترک: src/Shared.Contracts/ شامل validatorها و قرارداد اعلان میزبان.
- کدهای امنیتی: src/Shared.Security/ شامل trust store، replay guard و safe diagnostic logging.
- وجود سورس و آزمون‌های واحد/قراردادی، به‌تنهایی اثبات اجرای آزمون امنیتی روی دو دستگاه واقعی یا صحت کامل end-to-end نیست.

## شکاف‌های احرازشده

1. مسیر پیام عادی Android هنوز فقط پیام کاربر را در Room ذخیره می‌کند؛ runtime شبکه از مسیر صفحهٔ جداگانهٔ درخواست استفاده می‌شود.
2. پاسخ inference از مسیر واقعی P2P هنوز به همان conversation و جدول پیام‌های Room در UI عادی متصل نشده است.
3. مسیر عادی Android هنوز ورودی‌های فنی Rendezvous URL، model ID و max output tokens را در صفحهٔ مجزا نمایش می‌دهد.
4. Windows host، backend محلی و listener اجزای اجرایی جداگانه دارند؛ readiness و admission در کد موجودند، اما بارگذاری و inference با مدل واقعی روی میزبان فیزیکی هدف در شواهد این ممیزی تأیید نشده است.
5. دارایی‌های مورد انتظار هویت بصری مشترک، آزمون محصول واقعی Android-to-Windows و آزمون ارتقای درجا در مسیرهای مورد انتظار گراف، در tree این commit پیدا نشدند.
6. آزمون فیزیکی Android-to-Windows و clean install/upgrade روی دو پلتفرم از CI شناسایی‌شده قابل استنتاج نیستند.

## فرمان‌های ساخت و آزمون ثبت‌شده در مخزن

README ابزارهای لازم را این‌گونه مشخص می‌کند: .NET SDK 10.0.100 با latestFeature، JDK 21، Gradle 8.13، Android SDK 36 و Python 3.13. Repository Gradle Wrapper ندارد؛ بنابراین gradle باید در محیط اجرا موجود باشد.

- Windows restore: dotnet restore WhiteP.sln
- Windows build: dotnet build WhiteP.sln --configuration Release
- Windows lifecycle suite: dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release
- Windows publish: dotnet publish src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/windows-host
- Android lint: gradle --no-daemon ktlintCheck
- Android debug build: gradle --no-daemon assembleDebug
- Repository tests: python -m unittest discover -s tests -v
- Graph guard: python tools/graph_guard.py "update/Step two/project-graph.md" و حالت --repo-root .؛ workflow همان کنترل را برای گراف قدیمی نیز اجرا می‌کند.

## شواهد CI موجود در commit مبنا

- Quality gate، run 42401485764: موفق؛ آزمون‌های Python، compileall و هر دو حالت graph_guard برای هر دو گراف.
- Product build، run 42401485830: موفق؛ restore/clean/build/publish و smoke test میزبان WPF، آزمون‌های چرخهٔ عمر، installer ویندوز، lint و build اندروید، آزمون‌های JVM و آزمون‌های Keystore روی Android emulator.
- Rendezvous container، run 42401485784: موفق؛ build و smoke test سرویس در کانتینر non-root.
- این runها متعلق به commit مبنا هستند و regression test جدید tests/test_update_baseline_audit.py را اجرا نکرده‌اند؛ اجرای آن در CI پس از این commit هنوز لازم است.
- آزمون واقعی مدل محلی در Windows، آزمون Android-to-Windows با گوشی فیزیکی، نصب تمیز و ارتقای درجا در این runهای مبنا تأیید نشده‌اند.

## ارزیابی معیارهای پذیرش UPD-S1-001

| معیار | نتیجه |
|---|---|
| مسیرهای Android، Windows و Shared | شناسایی و در evidence ثبت شد |
| جریان درخواست تا میزبان و برگشت پاسخ | جریان‌های فعلی و نقطهٔ گسست چت/شبکه مشخص شد |
| فرمان build/test و نتیجه یا علت اجرا نشدن | فرمان‌ها از README/workflow استخراج؛ نتایج CI موجود تفکیک شد؛ آزمون محلی در این محیط اجرا نشده |
| تفکیک قابلیت واقعی از مستندات/ادعا | انجام شد؛ قابلیت‌های اثبات‌نشده صریحاً جدا شدند |
| SHA واقعی main و تمایز checkpointها | ثبت شد |
| عدم استفاده از SHA تاریخی ممنوع | رعایت شد |

## اقدام باقی‌مانده

آزمون regression ممیزی باید در CI روی این commit اجرا و موفق شود. پس از آن، شواهد نهایی و SHA commit واقعی در تاریخچه ثبت می‌شود و وضعیت task فقط پس از بازبینی مجدد معیارها قابل تغییر به done است.

این گزارش اجرای واقعی قابلیت جدید را ادعا نمی‌کند؛ خروجی این وظیفه، ممیزی خط مبنای قابل‌ردیابی است.


## ثبت نتیجهٔ نخستین regression CI و اصلاح

در commit بررسی ممیزی، quality gate با run شمارهٔ 42408388222 شکست خورد. از ۶ آزمون جدید ممیزی، ۲ assertion شکست کاذب داشتند:
1. آزمون اندازهٔ گراف، جداکنندهٔ ASCII را انتظار داشت؛ evidence اندازه را با جداکنندهٔ هزارگان فارسی ثبت کرده بود. انتظار آزمون به متن واقعی evidence اصلاح شد.
2. آزمون semaphore، فراخوانی صریح SemaphoreSlim(1, 1) را انتظار داشت؛ کد C# از مقداردهی target-typed به شکل _inferenceSemaphore = new(1, 1) استفاده می‌کند. انتظار آزمون به عبارت واقعی موجود در سورس اصلاح شد.

سایر چهار آزمون جدید موفق بودند و آزمون‌های قبلی در خروجی همان suite اجرا شدند. این خطاها فقط در assertionهای جدید audit بودند؛ هیچ سورس محصولی تغییر نکرده است. آزمون اصلاح‌شده هنوز در یک CI تازه اجرا نشده، بنابراین وضعیت UPD-S1-001 همچنان in_progress باقی می‌ماند.

- Workflow: https://github.com/artinrahimii/project/actions/runs/42408388222
- Changed test: tests/test_update_baseline_audit.py
- Execution checkpoint for this correction: 760c3882fe6a6e680c149d0b374790e26db7a4a9 (V 3.1.18)
- Next verification: run the full quality workflow on the correction commit; do not mark this task done before it passes.


## نتیجهٔ نهایی UPD-S1-001

<badge>done</badge> معیارهای ممیزی خط مبنا پس از اجرای موفق آزمون regression و کنترل‌های گراف تأیید شدند.

- وضعیت: done
- Attempt نهایی: UPD-S1-001-20261107195525
- Commit حاوی evidence و test source: c10b59eb6ede67261643e6994f2c08c18fa591ec — V 3.1.19
- Quality workflow: https://github.com/artinrahimii/project/actions/runs/42408460108
- نتیجهٔ آزمون‌ها: ۴۵۸ آزمون Python موفق؛ compileall موفق؛ graph guard گراف قدیمی بدون error و با یک warning metadata حفظ‌شده؛ graph guard گراف Update بدون error یا warning.
- شش آزمون baseline-audit تازه نیز در suite کامل موفق شدند.
- warning گراف قدیمی: T0001 به یک عبارت متنی metadata غیرشناسه‌ای ارجاع دارد؛ graph_guard آن را به‌عنوان metadata حفظ می‌کند و فقط warning می‌دهد. این warning پنهان یا به‌عنوان وابستگی معتبر طبقه‌بندی نشده است.
- هیچ سورس محصولی در این وظیفه تغییر نکرده است؛ هدف task ممیزی و تعیین خط مبنا بود.
- وضعیت reconciliation همچنان مستقل و pending است؛ هیچ‌یک از ۱۶۸ شناسهٔ قدیمی در این task بازبینی‌شده محسوب نمی‌شود.

# گزارش اجرای UPD-S1-014 — کشف metadata-only و غربالگری میزبان

- مخزن: `artinrahimii/project`
- شاخه: `main`
- مبنای دقیق ممیزی: `0392fb1a57a1e105f54fda19c6e2a03fe33e531d` — `V 3.1.182`
- شناسهٔ تلاش: `UPD-S1-014-20261111-A001`
- شروع تلاش: `2026-11-10T21:40:16.275Z`
- وضعیت تلاش: **partial / in_progress**
- عملیات منبع: `modify`؛ اجزای سالم موجود حفظ شده‌اند و فقط رتبه‌بندی selector اصلاح شده است.

## ممیزی پیاده‌سازی موجود

| مؤلفه | Blob SHA مبنای V 3.1.182 | نتیجهٔ ممیزی |
|---|---|---|
| `RendezvousPeerDirectoryClient.kt` | `2b93c0de23b398643a275e2b4e9cbe06200cce17` | HTTPS (به‌جز loopback)، GET فقط metadata، bounded response، no-store، cache کوتاه و محدود به زودترین expiry؛ refresh ناموفق دادهٔ stale را برنمی‌گرداند. |
| `AuthenticatedHostDiscovery.kt` | `ad982ec7d4c576edee3b8976066b281801bd9567` | فهرست directory را با probe زندهٔ HostStatus احرازشده تطبیق می‌دهد؛ PeerId، READY، زمان مشاهده، مدل و ظرفیت را بررسی می‌کند. |
| `HostCandidateSelector.kt` | `f0f89f8052ab60fe740e77fdc9fb39da7bdb1f8b` | identity تکراری، هویت ناسازگار، پروتکل، مدل، سقف prompt/context/output، وضعیت غیر READY و ظرفیت پر را رد می‌کند؛ انتخاب deterministic و تاریخچهٔ retry را حفظ می‌کند. |
| `PeerAnnouncementClient.cs` | `ad7b242c2f28b8c090ce7bc08c25b7b1da98b2bc` | سطح API محدود به announce و discover بر اساس capability است؛ پاسخ bounded و HTTPS اجباری است. |
| `PeerAnnouncementHostLifecycle.cs` | `fb4acb368d91f87d72fbe4eea2c3cf39d79caf77` | listener فقط پس از trust و opt-in صریح شروع می‌شود؛ lease metadata تمدید می‌شود؛ HostStatus تازه از provider درخواست می‌شود؛ خطای provider به UNAVAILABLE تبدیل می‌شود. |
| `PeerAnnouncementLeasePublisher.cs` | `aa9286271708a488608a2c41706f8702dc7bccf7` | TTL پیش‌فرض پنج دقیقه، تمدید یک‌دقیقه‌ای، توقف تمدید و انقضای طبیعی lease. |
| `PeerAnnouncementServiceHost.cs` و `PeerAnnouncementDirectory.cs` | `9218f1c05f281434dad3b940f7f925d86b54b91b` / `4a2a2d18c036b14f82589a976d29fda6263a9d26` | API فقط health، announcement و metadata discovery دارد؛ directory آخرین metadata را نگه می‌دارد، رکورد منقضی را حذف و نتایج را با PeerId مرتب می‌کند. |
| `MainWindow.xaml.cs` | `77f14b493e7621bd9d80fa2279130e002e7eb8f4` | شبکه پیش‌فرض خاموش؛ consent پیش از ساخت lifecycle؛ backend/model/capacity واقعی باید آماده باشد؛ مالک RAM و free-disk حداقل را تعیین می‌کند. |
| `HostRequestAdmissionPolicy.cs` | `ccc2c1170111b5a4e015dd252ffadd0af3f2ee87` | admission فقط با participation فعال، مدل دقیق آماده، سقف خروجی معتبر، ظرفیت READY و deadline صحیح. |

## انطباق با معیارهای پذیرش

1. **expiry / opt-out / trust / reachability / readiness:** expiry در directory parser/cache و پیش و پس از status probe بررسی می‌شود. با توقف مشارکت، lease دیگر renew نمی‌شود و listener بسته می‌شود؛ peer قدیمی حتی اگر موقتاً metadata داشته باشد، probe زندهٔ احرازشده را نمی‌تواند پاسخ دهد. ناشناس یا غیرقابل‌اعتماد پیش از probe رد می‌شود و status غیر READY پذیرفته نیست.
2. **مدل، ظرفیت و محدودیت مالک:** مدل از snapshot زندهٔ HostStatus می‌آید، نه capability عمومی directory. وضعیت backend از readiness check واقعی و ظرفیت RAM/disk از اندازه‌گیری Windows می‌آید. قرارداد first-release یک inference هم‌زمان برای هر میزبان دارد؛ HostStatus میزبان BUSY را با activeRequests=1 گزارش و از READY جدا می‌کند.
3. **ranking قابل اندازه‌گیری:** A001 selector موجود را تغییر می‌دهد تا candidateهای واجد شرایط ابتدا با نسبت اشغال ظرفیت `activeRequests / maxConcurrentRequests` مرتب شوند. مقایسه با cross-product صحیح انجام می‌شود و PeerId tie-break قطعی است. RTT، latency و throughput در قرارداد فعلی اندازه‌گیری نمی‌شوند؛ در نتیجه هیچ امتیاز شبکه‌ای حدس زده یا ساخته نشده است.
4. **cardinality و ترتیب:** ADR-0022 مسیر first-release را به یک میزبان در هر round محدود می‌کند. Android runtime مقدار maxHosts=1 را الزام می‌کند و prompt به میزبان‌های آنلاین broadcast نمی‌شود. probeهای discovery فقط HostStatus metadata هستند؛ با اولین READY معتبر در ترتیب PeerId متوقف می‌شوند. چون پیاده‌سازی میزبان اول-release ظرفیت یک دارد و READY تنها با activeRequests=0 گزارش می‌شود، اولین peer واجد شرایط حداقل بار قابل‌اندازه‌گیری را دارد؛ ادامهٔ probe بعد از آن برای first-release بار پایین‌تری پیدا نمی‌کند.
5. **مرز metadata-only:** directory فقط PeerId، addresses، capabilities و expiry را می‌پذیرد/برمی‌گرداند. Android GET body ندارد. تست‌های HostLifecycle payloadهای prompt/capsule را در ورودی کنترل رد می‌کنند؛ هیچ prompt، response یا history در مسیر Rendezvous قرار نمی‌گیرد.
6. **قابلیت مدل:** شناسه و metadata مدل در هر دو snapshot announcement/live status تطبیق داده می‌شود؛ سپس readiness واقعی backend و مدل بارگذاری‌شده در Windows تأیید می‌شود. capability tag عمومی `inference.v1` به‌تنهایی مدل را اثبات نمی‌کند.

## اصلاح کد در تلاش A001

- `HostCandidateSelector.select`: ترتیب PeerId-only برای candidateهای از قبل احرازشده به ranking بر مبنای utilization واقعی تغییر کرد؛ مقایسه بدون floating-point و با tie-break بر اساس PeerId است.
- `HostCandidateSelectorTest.kt`: آزمون ranking برای utilization صفر، یک‌چهارم و tie در نیمهٔ ظرفیت اضافه شد.
- `test_android_host_candidate_selector_contract.py`: قرارداد source-level با comparator جدید و آزمون تازه همگام شد.
- هیچ پروتکل، protobuf، API directory، رفتار trust، ظرفیت production یا سقف تک‌میزبان تغییر نکرد.
- مسیرهای موجود برای probe ترتیبی و توقف پس از اولین میزبان READY در first-release حفظ شدند؛ هیچ request payload به میزبان‌های دیگر broadcast نمی‌شود.

## مصرف‌کنندگان و وابستگی‌ها

- Android: `AndroidInferenceRequestRuntime`, `AuthenticatedHostDiscovery`, `AutomaticModelSelectionPolicy`, `Libp2pInferenceRequestTransport`, `HostCandidateSelector`.
- Windows: `MainWindow.GetPeerRuntimeStatusAsync`, `PeerAnnouncementHostLifecycle.CreateHostStatusResponseAsync`, `PeerAnnouncementLeasePublisher`, `HostRequestAdmissionPolicy`.
- Shared: `HostStatus`, `HostCapabilities`, `PeerAnnouncement`, protocol version and model capability records.
- آزمون‌های مرتبط: `AuthenticatedHostDiscoveryTest.kt`, `HostCandidateSelectorTest.kt`, `RendezvousPeerDirectoryClientTest.kt`, `PeerAnnouncementClientAcceptanceTests.cs`, `HostCapacityAcceptanceTests.cs`, و source-contract guard مربوط به selector.

## آزمون‌ها و محدودیت‌ها

- خط مبنای CI روی `V 3.1.182` قبل از A001 موفق بود: 496 آزمون Python، Android JVM/instrumentation و build، Windows WPF/installer و HostLifecycle، و Rendezvous smoke. این نتایج **baseline** هستند و اجرای تغییر A001 را ثابت نمی‌کنند.
- پس از تغییر A001، هنوز آزمون جدید اجرا نشده است؛ بنابراین هیچ نتیجه‌ای برای این diff به‌عنوان PASS ثبت نمی‌شود.
- آزمون دستگاه Android فیزیکی به Windows واقعی، سنجش RTT/throughput بین میزبان‌های مستقل و بررسی زیرساخت/logging ارائه‌دهندهٔ عمومی اجرا نشده‌اند.
- به دلیل نبود metric واقعی کیفیت اتصال، رتبه‌بندی بر اساس RTT/throughput افزوده نشده است.
- وضعیت task تا عبور exact-head CI از کیفیت، selector، Android JVM، Windows و Rendezvous `in_progress` باقی می‌ماند.

## فایل‌های واقعی و شواهد

- `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`
- `tests/test_android_host_candidate_selector_contract.py`
- `update/Step three/UPD-S1-014/execution-report.md`
- `update/Step three/UPD-S1-014/evidence/discovery-selector-A001.md`


## نتیجهٔ CI تلاش A001 — V 3.1.183

- Commit: `66fe4dc744e045b528bf9bade78e7613da6eb831`.
- Step Three Quality gate: **PASS**؛ ۴۹۶ آزمون Python، compileall و هر دو graph guard موفق. گراف قدیمی ۰ خطا و ۱ هشدار metadata حفظ‌شده؛ گراف به‌روزرسانی ۰ خطا و ۰ هشدار. Run [42907340773](https://github.com/artinrahimii/project/actions/runs/42907340773).
- HostLifecycle unit/integration/coverage: **PASS**؛ run [42907340561](https://github.com/artinrahimii/project/actions/runs/42907340561).
- Rendezvous container build/smoke: **PASS**؛ run [42907340870](https://github.com/artinrahimii/project/actions/runs/42907340870).
- Android Product Build: **FAIL** در `ktlintMainSourceSetCheck`؛ سه تخلف `standard:chain-method-continuation` در `HostCandidateSelector.kt` خطوط ۶۳، ۶۶ و ۷۴. خطاها فقط قالب‌بندی هستند. JVM، Keystore instrumentation و buildهای downstream در job Android پس از شکست lint **اجرا نشدند**؛ نتیجهٔ ranking test جدید هنوز اثبات نشده است.
- Windows Product Build در زمان بسته‌شدن A001 هنوز در مرحلهٔ installer acceptance قرار داشت؛ نتیجهٔ آن جداگانه پس از اتمام job ثبت خواهد شد.

## تلاش A002 — اصلاح قالب‌بندی بدون تغییر الگوریتم

- شروع: `2026-11-10T21:44:28.971Z`.
- سه تخلف زنجیرهٔ متدها با جداسازی فهرست candidateهای معتبر از مرحلهٔ `sortWith` اصلاح شدند؛ محاسبهٔ cross-product و tie-break بر اساس PeerId بدون تغییر باقی ماند.
- هیچ قرارداد پیام، protobuf، trust، cardinality یا سیاست میزبان تغییر نکرد.
- آزمون ranking و source-contract جدید همان آزمون‌های A001 هستند و حذف نشده‌اند.
- CI دقیق A002 هنوز اجرا نشده است؛ task همچنان `in_progress` می‌ماند.


## نتیجهٔ CI تلاش A002 — V 3.1.184

- Commit: `911ef42bdfaf8b32a46169360a710ff21a3b9146`.
- Quality run [42907998248](https://github.com/artinrahimii/project/actions/runs/42907998248): **FAIL**؛ ۴۹۶ آزمون اجرا و یک failure. آزمون `test_selector_keeps_three_host_batching_as_compatibility_only` هنوز عبارت قدیمی `.sortedWith { first, second ->` را انتظار داشت؛ پیاده‌سازی A002 برای رعایت ktlint از `eligibleCandidates.sortWith { first, second ->` استفاده می‌کند.
- به دلیل شکست همین آزمون، مراحل compileall و graph guard در آن workflow **اجرا نشدند**.
- HostLifecycle job [42907998263](https://github.com/artinrahimii/project/actions/runs/42907998263): **PASS**.
- Rendezvous [42907998204](https://github.com/artinrahimii/project/actions/runs/42907998204): **PASS**.
- در زمان ثبت این گزارش، Android و Windows Product Build هنوز در حال اجرا بودند؛ نتیجهٔ نهایی آن‌ها جداگانه ثبت می‌شود.

## تلاش A003 — همگام‌سازی source-contract

- شروع: `2026-11-10T21:47:21.939Z`.
- assertion Python از شکل زنجیره‌ای قدیمی به شکل واقعی و قالب‌بندی‌شدهٔ `eligibleCandidates.sortWith` به‌روزرسانی شد.
- کد selector، comparator، ترتیب utilization، tie-break و آزمون JVM ranking دست‌نخورده ماندند.
- A003 هنوز روی exact-head CI اجرا نشده است؛ task `in_progress` باقی می‌ماند.


## نتیجهٔ CI تلاش A003 — V 3.1.185

- Commit: `cc6c8c9f0839137867732bdda9ddef9c3045d2a5`.
- Step Three Quality gate: **PASS**؛ ۴۹۶ آزمون Python، compileall و graph guard هر دو گراف موفق.
- HostLifecycle: **PASS**؛ job پذیرش کامل شد.
- Rendezvous: **PASS**؛ container build/smoke موفق.
- Android Product Build: **FAIL** در ktlint؛ دو تخلف `standard:chain-method-continuation` در خطوط ۶۵ و ۶۷ از `HostCandidateSelector.kt` به‌علت chain شدن دسترسی به status/announcement/capabilities. JVM و Keystore instrumentation پس از lint اجرا نشدند.

## تلاش A004 — جداسازی دسترسی‌های property در comparator

- شروع: `2026-11-10T21:50:00Z`.
- دسترسی‌های زنجیره‌ای به status، announcement و capabilities در local variableهای جداگانه ذخیره شدند.
- فرمول مقایسهٔ utilization با cross-product صحیح و tie-break بر اساس PeerId بدون تغییر ماند.
- هیچ قرارداد، protobuf، transport، trust یا cardinality محصول تغییر نکرد.
- exact-head CI برای A004 هنوز اجرا نشده است؛ task همچنان `in_progress` است.

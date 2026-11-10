# شواهد تلاش A005 — اصلاح سقف آزمون dispatcher

- مخزن/شاخه: `artinrahimii/project` / `main`.
- مبنای دقیق: `7c95d5d8c817ac5d82c4aa63d65b3df436a8b0e7` — `V 3.1.181`.
- CI: quality [42896931063](https://github.com/artinrahimii/project/actions/runs/42896931063) موفق؛ Windows job [130112373952](https://github.com/artinrahimii/project/actions/runs/42896931040/job/130112373952) موفق؛ HostLifecycle job [130112373643](https://github.com/artinrahimii/project/actions/runs/42896931040/job/130112373643) موفق؛ Rendezvous [42896931302](https://github.com/artinrahimii/project/actions/runs/42896931302) موفق.
- Android: lint و debug/release APK موفق؛ JVM شامل ۲۲۵ آزمون بود که فقط `keepsSubmittingOtherHostsAfterTransportRejectsOrFails` شکست خورد.
- JUnit artifact: `13802330493`; SHA-256: `546dea024dd25a107c56a5445a446a3a5d003d7cc23978069418c0cbedf4faf8`.
- علت ریشه‌ای: این test چهار candidate را در یک round ارسال می‌کرد، درحالی‌که `HostCandidateSelector.MAX_HOSTS_PER_ROUND` برابر سه است. dispatcher صحیحاً قبل از ارسال، round را رد می‌کرد.
- اصلاح A005: آزمون اصلی به سه candidate کاهش یافت؛ حالت صریح `FAILED` در آزمون جداگانه و با دو candidate پوشش داده شد. سقف تولیدی یا کد runtime تغییر نکرد.
- آزمون CI اصلاح A005 هنوز اجرا نشده؛ هیچ نتیجهٔ قبولی برای این اصلاح ادعا نمی‌شود.


## نتیجهٔ نهایی exact-head CI — V 3.1.182

- Commit: `0392fb1a57a1e105f54fda19c6e2a03fe33e531d`.
- Quality run [42903745782](https://github.com/artinrahimii/project/actions/runs/42903745782): **PASS**؛ ۴۹۶ آزمون Python، compileall، گراف اولیه ۲۳۰/۴۲۲ با صفر خطا و یک هشدار metadata حفظ‌شده، گراف به‌روزرسانی ۲۴/۸۰ با صفر خطا و صفر هشدار.
- Product Build [42903745800](https://github.com/artinrahimii/project/actions/runs/42903745800): **PASS** در Android، Windows و HostLifecycle.
- Android: ktlint، debug/release APK و signature smoke، manifest permission audit، JVM و Keystore instrumentation موفق؛ JUnit تجمیعی ۲۳۷ آزمون/۲۳۶ موفق/۰ شکست/۱ skip و coverage reports معتبر. Emulator تست CI است و جای handset فیزیکی را نمی‌گیرد.
- Windows: WPF clean build/publish/startup smoke، DPAPI identity/trust و installer acceptance موفق.
- HostLifecycle: ۶۸ critical pass، ۱ skip و ۸۶٫۱۵٪ line coverage؛ real-model opt-in test skip شد.
- Rendezvous run [42903745740](https://github.com/artinrahimii/project/actions/runs/42903745740): **PASS**.
- A005 با این شواهد بسته می‌شود؛ physical Android↔Windows inference و in-place upgrade خارج از معیار UPD-S1-016 هستند.

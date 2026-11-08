# شواهد CI دقیق commit V 3.1.30

## خط مبنا

- Commit: `cb6050e9b6e9f8fd60d2266efbf57af2534a16c0`
- Branch: `main`
- Message: `V 3.1.30`

## نتایج

| کنترل | نتیجه | شواهد |
|---|---|---|
| Step Three quality gate | PASS | [Run 42417548666](https://github.com/artinrahimii/project/actions/runs/42417548666) |
| Rendezvous container build/smoke | PASS | [Run 42417548709](https://github.com/artinrahimii/project/actions/runs/42417548709) |
| Windows WPF build/publish/startup smoke/installer | PASS | [Run 42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687) |
| HostLifecycle unit/integration/coverage | PASS | [Run 42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687) |
| Android APK debug/release, lint, permissions | PASS | [Run 42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687) |
| Android JVM unit tests | FAIL | [Run 42417548687](https://github.com/artinrahimii/project/actions/runs/42417548687): 213 completed, 33 failed |
| Android Keystore instrumentation | NOT RUN | Instrumentation steps skipped after JVM test failure |
| Android debug APK artifact upload | NOT RUN | Conditional artifact step skipped after test failure |

## ریشه‌یابی

تمام ۳۳ شکست Android به `IllegalArgumentException` در ساخت `DistributedRequest` برمی‌گردند. Fixtureهای generic در `HostCandidateSelectorTest` و `InferenceAttemptDispatcherTest` بودجهٔ پیش‌فرض ۹ میزبان دارند؛ آزمون transport نیز بودجهٔ ۶ میزبان در دو round را می‌خواهد. این آزمون‌ها مسیر compatibility عمومی سه‌میزبانه را می‌سنجند و نباید به‌خاطر سقف first-release سه‌تلاشه از کار بیفتند.

محدودیت اجرایی محصول در جای درست برقرار است: UI حداکثر سه تلاش کل را می‌سازد، discovery حداکثر یک peer را در هر درخواست انتخاب می‌کند و runtime پیش از dispatch چند peer را رد می‌کند. اصلاح باید فقط validation عمومی budget را با سقف generic هم‌راستا کند و این کنترل‌های first-release را دست‌نخورده نگه دارد.

این commit آزمون فیزیکی Android-to-Windows یا ارتقای درجا را اثبات نمی‌کند.

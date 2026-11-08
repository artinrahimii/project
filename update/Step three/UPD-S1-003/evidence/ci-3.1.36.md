# شواهد CI دقیق V 3.1.36 — تکمیل UPD-S1-003

- commit: d027e72f3968ef7c8c826f889042eb67a8533fa4 (main; V 3.1.36)
- Quality gate: PASS — [42457409001](https://github.com/artinrahimii/project/actions/runs/42457409001)
- Product Build: PASS — [42457409040](https://github.com/artinrahimii/project/actions/runs/42457409040)
- Rendezvous: PASS — [42457409058](https://github.com/artinrahimii/project/actions/runs/42457409058)

| اجزا | نتیجه |
|---|---|
| Windows WPF build/publish/startup/installer | PASS |
| HostLifecycle unit/integration/coverage | PASS |
| Android ktlint و debug/release APK و manifest | PASS |
| Android JVM unit tests | PASS |
| Android Keystore instrumentation | PASS |

تصمیم نسخهٔ نخست: یک میزبان در هر round، تا سه تلاش ترتیبی روی میزبان‌های متمایز، retry دستی، requestId و deadline مشترک، attemptId/roundId تازه، cancel terminal و پاسخ دیررس/تکراری رد می‌شود. matrix سرنوشت T0125، T0126، T0161–T0177، T0203، T0204، T0224 و T0225 را ثبت کرده است؛ generic multi-host compatibility جداگانه حفظ شده است.

این CI میزبان‌شده است. آزمون فیزیکی Android-to-Windows با مدل محلی، clean install مشتری و in-place upgrade اجرا نشده‌اند و در UPD-S1-020/021 باقی می‌مانند.

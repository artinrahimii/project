# T0014 — گزارش اجرای واقعی

- وضعیت گره: `in_progress؛ build ویندوز و Android موفق‌اند، اما پیش‌نیازهای گرافی `T0013` و `T0010` باید جداگانه تأیید شوند.
- تنظیمات واقعی: `global.json` روی .NET SDK `10.0.100`؛ Android compile/target SDK برابر 36 و minSdk برابر 26.
- شواهد: [White-P product build — run 39641644577](https://github.com/artinrahimii/project/actions/runs/39641644577) موفق است؛ job ویندوز `119366422112` و job Android `119366421861` هر دو موفق‌اند.
- job Android گام‌های پیدا کردن `sdkmanager`، پذیرش مجوزها، نصب `platforms;android-36` و `build-tools;36.0.0` و ساخت debug APK را با موفقیت گذراند.
- شکست‌های قبلی (بستهٔ منسوخ، مسیر sdkmanager و YAML نامعتبر) در تاریخچه حفظ شده‌اند.
- نتیجه: شواهد build برای هر دو پلتفرم موجود است؛ اما به‌دلیل پیش‌نیازهای تکمیل‌نشدهٔ T0013 و T0010، وظیفه هنوز `done` نمی‌شود.

## Attempt T0014-A006 — platform configuration build verification
- Verification date: 2026-10-21.
- Verified main commit: `632c36d257ca079dd9bdee07f5415aa3c7c97cc3` (`V 3.0.35`).
- [White-P product build run 39656666786](https://github.com/artinrahimii/project/actions/runs/39656666786) succeeded, including the Windows WPF host build and Android debug APK build.
- Configuration inspected: `global.json` selects .NET SDK 10.0.100 with latest-feature roll-forward; `src/Client.Android/app/build.gradle.kts` sets compile/target SDK 36 and minSdk 26.
- Local execution: not run here; the result comes from GitHub Actions.
- Output-path check: the current main tree has no `docs/decisions/` directory, although T0014 expected output names it. This remains an outstanding acceptance item.
- Prerequisite/graph inconsistency: T0014 lists T0010 and T0013 in `depends_on` and in its start condition. T0013 is done, but T0010 is not. The T0010 → T0014 edge is marked `sequencing_only`, which conflicts with the node-level mandatory prerequisite. This discrepancy is recorded and has not been silently changed.
- Status: partial; T0014 remains `in_progress`.
- Follow-up: reconcile the graph contract explicitly and review all T0014 acceptance outputs before marking it done.


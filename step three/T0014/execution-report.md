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

## T0014-A007 — dependency contract reconciliation
- Date: 2026-10-21.
- Base commit reviewed: `a5c88bbe341d4d4a57a81bc481e730378ef5af09` (`V 3.0.36`).
- Evidence: the corresponding graph edge `T0010 → T0014` was already `validated` with classification `sequencing_only` and recorded evidence; current workflow `product-build.yml` successfully builds the platform project(s) on GitHub Actions.
- Graph correction: removed T0010 from `depends_on` and from the mandatory `start_condition`; the sequencing-only edge itself is retained with its existing evidence. No edge was deleted or reclassified.
- Verification boundary: this resolves the contradiction between node-level mandatory prerequisites and edge classification; it does not by itself satisfy every declared output or acceptance criterion.
- Status: `in_progress`; no completion claim is made.
- Follow-up: verify all declared output paths and task-specific acceptance; then decide whether T0014 can be marked done.


## Attempt T0014-A008 — platform baseline decision and truthful project status
- Date: 2026-10-21.
- Base commit reviewed: `bcb600e7e55b82c16c49cc3236068c106a5ccaf7` (`V 3.0.37`).
- Rechecked the current `global.json`, WPF project file, Android Gradle configuration, and `.github/workflows/product-build.yml`. The latest runs for the base commit passed the quality gate and product build workflow, including Windows host build, Android debug APK build, and lifecycle tests.
- Created `docs/decisions/ADR-0001-platform-build-baselines.md` documenting the actual .NET/Windows and Android/JDK/Gradle/SDK baselines, rationale, limits, and verification evidence.
- Updated `README.md`: the previous version incorrectly stated that Windows/Android projects and product build entrypoints were absent. The new text distinguishes existing application skeletons and CI builds from still-missing shared Protobuf contracts, real inference, P2P transport, and end-to-end acceptance.
- Updated `tests/test_readme_contract.py` so it tests the actual current repository state rather than the obsolete absence claim.
- Tests for this new revision have not run yet; the commit-triggered GitHub Actions workflows are the verification gate.
- Status: `in_progress` pending CI on the new commit. No runtime or P2P acceptance claim is made.

# T0015 — گزارش اجرای واقعی

- وضعیت: در حال اجرا تا تکمیل پیش‌نیاز T0014 و بازبینی گراف.
- خروجی واقعی: پروژهٔ WPF در `src/Host.Windows/WhiteP.Host.Windows.csproj`، `App.xaml`, `App.xaml.cs`, `MainWindow.xaml` و `MainWindow.xaml.cs`.
- آزمون واقعی: مرحلهٔ `Build host` در GitHub Actions run [39640743619](https://github.com/artinrahimii/project/actions/runs/39640743619) موفق شد.
- دامنه: اسکلت WPF کامپایل می‌شود؛ این آزمون عملکرد inference محلی، کشف سخت‌افزار یا شبکه را ثابت نمی‌کند.
- دلیل عدم بستن وظیفه: یال T0014 → T0015 با شواهد به‌عنوان وابستگی فنی سخت ثبت شده و T0014 تا تأیید پیکربندی Android هنوز در حال اجراست.
- گام بعدی: تکمیل T0014 پس از build موفق Android، سپس بازبینی معیار پذیرش T0015 و ثبت نتیجه.

## Attempt T0015-A002 — WPF project build verification
- Verification date: 2026-10-21.
- Verified main commit: `632c36d257ca079dd9bdee07f5415aa3c7c97cc3` (`V 3.0.35`).
- [White-P product build run 39656666786](https://github.com/artinrahimii/project/actions/runs/39656666786) reports the `Build Windows WPF host` job successful. The project targets `net10.0-windows` with `UseWPF=true`.
- Local execution: not run here; the build was performed by GitHub Actions on a Windows runner.
- Scope boundary: this proves the WPF shell builds, not that local inference or peer-to-peer features work.
- Status: partial; T0015 remains `in_progress` because T0014 and T0010 are still listed as prerequisites.
- Follow-up: reconcile the prerequisite/edge inconsistency and inspect any remaining T0015 acceptance requirements before completion.

## T0015-A003 — dependency contract reconciliation
- Date: 2026-10-21.
- Base commit reviewed: `a5c88bbe341d4d4a57a81bc481e730378ef5af09` (`V 3.0.36`).
- Evidence: the corresponding graph edge `T0010 → T0015` was already `validated` with classification `sequencing_only` and recorded evidence; current workflow `product-build.yml` successfully builds the platform project(s) on GitHub Actions.
- Graph correction: removed T0010 from `depends_on` and from the mandatory `start_condition`; the sequencing-only edge itself is retained with its existing evidence. No edge was deleted or reclassified.
- Verification boundary: this resolves the contradiction between node-level mandatory prerequisites and edge classification; it does not by itself satisfy every declared output or acceptance criterion.
- Status: `in_progress`; no completion claim is made.
- Follow-up: finish T0014 first, then verify host project acceptance and record current CI evidence.


## T0015-A004 — successful current-main verification
- Date: 2026-10-21; base commit `8b34e4fef0e304a382b97d3b07382e82fb4245dd` (`V 3.0.38`).
- [White-P product build run 39665189760](https://github.com/artinrahimii/project/actions/runs/39665189760) passed; job `Build Windows WPF host` completed successfully.
- Status: `done` for creating and building the WPF host project. Local inference, hardware discovery, and P2P behavior are outside this build proof.

# T0221 — ثابت کن انتخاب مدل، دانلود، نصب، اجرا و حذف مدل روی فایل‌های واقعی کار می‌کنند.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-023.
- Decision: `re_review_required`.

## Review finding

Catalog validation, bounded HTTPS download, SHA-256/GGUF verification and atomic update of an existing model are real. But catalogue selection stores preference only, initial download/install/activation is not wired, and Remove Active Model leaves the model file on disk. Full select/download/install/run/delete lifecycle is incomplete.

## Source and evidence

- `src/Host.Windows/Models/ModelStreamingDownloader.cs`
- `src/Host.Windows/Models/VerifiedModelInstaller.cs`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/ModelStreamingDownloaderAcceptanceTests.cs`
- `tests/test_model_catalog_ui.py`
- `tests/test_model_catalog_parser.py`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Re-review after the related UPD-S1 implementation and required real-product acceptance. Preserve the baseline status until the legacy task's own acceptance criteria are satisfied.

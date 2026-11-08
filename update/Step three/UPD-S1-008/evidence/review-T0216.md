# T0216 — راهنمای نصب و عیب‌یابی میزبان ویندوز را بر اساس UI واقعی بنویس.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-001, UPD-S1-021, UPD-S1-024.
- Decision: `re_review_required`.

## Review finding

The existing Windows installer document describes build/install/package details, not a complete first-run, backend/model setup, opt-in participation, peer trust, stop-participation and troubleshooting guide. WPF controls exist, but the requested user guide is not present.

## Source and evidence

- `docs/windows-installer.md`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `docs/deployment/rendezvous-service.md`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Re-review after the related UPD-S1 implementation and required real-product acceptance. Preserve the baseline status until the legacy task's own acceptance criteria are satisfied.

# T0228 — محدودیت‌های باقی‌مانده، قابلیت‌های پیاده‌سازی‌نشده و آزمایش‌های انجام‌نشده را به‌روشنی در `docs/release-limitations.md` ثبت کن.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-021.
- Decision: `re_review_required`.

## Review finding

A single consolidated current release-limitations register was absent. This commit adds docs/release-limitations.md, recording the verified CI state and still-open product/release acceptance gates. It is not a release approval.

## Source and evidence

- `docs/acceptance.md`
- `docs/release-assets.md`
- `docs/android-release-signing.md`
- `docs/windows-installer.md`
- `README.md`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.
- Exact-head Rendezvous container workflow: [42519571350](https://github.com/artinrahimii/project/actions/runs/42519571350) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Refresh this limitations register after each new acceptance run; it is not release approval.

# T0215 — فهرست وابستگی‌ها، مجوز نرم‌افزارها و مجوز مدل نمونه را در خروجی انتشار همراه کن.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `in_progress`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-021.
- Decision: `re_review_required`.

## Review finding

Release licence inventory and sample-model notice are built into the Windows installer pipeline; current-head Product Build passed. The signed Android release and release-assets workflow require a matching release tag and same-source artifacts; no successful post-fix tag/release run exists for this HEAD. Historic draft assets do not prove current release publication.

## Source and evidence

- `docs/windows-installer.md`
- `docs/release-assets.md`
- `docs/android-release-signing.md`
- `.github/workflows/product-build.yml`
- `.github/workflows/android-release.yml`
- `.github/workflows/release-assets.yml`
- `tests/test_release_assets_contract.py`
- `tests/test_generate_model_license_notice.py`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.
- Exact-head Rendezvous container workflow: [42519571350](https://github.com/artinrahimii/project/actions/runs/42519571350) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Run signed Android release and release-assets tag workflows on the same final source commit; revalidate assets and SHA-256 manifest.

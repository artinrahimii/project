# UPD-S1-024 A015 — exact-head review and remaining native icon capture

Baseline: `dcc3b2108a3e8e6e9a06fd5f5249103f60fca1cb` (`V 3.1.138`, online `main`).

## Exact-head workflow results

- Product build: https://github.com/artinrahimii/project/actions/runs/42706097854 — success.
- Quality gate: https://github.com/artinrahimii/project/actions/runs/42706098007 — success.
- Rendezvous build/smoke: https://github.com/artinrahimii/project/actions/runs/42706097807 — success.

Product-build artifacts from the same commit:
- Android review: artifact `13715125098`.
- Android unit tests: `13714990206` — 216 JVM tests, 0 failures.
- Android Keystore instrumentation: `13714750486` — 11 tests, 0 failures/errors, 1 explicitly skipped LAN acceptance test.
- Android debug APK: `13714446696`.
- Windows published host: `13714156408`.
- Windows installer: `13714051946`.
- Windows host output: `13712974059`.
- Windows review: `13712938177`.

## Manually reviewed visual evidence

Android review artifact includes Home, Conversations, Conversation detail, Messages, Connections and Privacy screenshots plus density-specific launcher assets for mdpi (48 px), hdpi (72 px), xhdpi (96 px), xxhdpi (144 px) and xxxhdpi (192 px), adaptive icon XML, the canonical SVG and token file. The Messages view still explicitly indicates that sending to a processing host is unavailable in this preview; that is a product-runtime limitation, not a visual theme pass.

Windows review artifact includes `wpf-main-window.png` and `WhiteP.ico`. The WPF screenshot visibly renders the action labels with the shared colors. Its current state correctly shows backend stopped, host not Ready, local engine not started and direct P2P transport not implemented. The ICO has seven embedded raster sizes: 16, 24, 32, 48, 64, 128 and 256 px.

The common vector drives both platform icon outputs. The five design tokens and numeric contrast assertions are covered by `tests/test_brand_identity.py`. Gold on light surfaces is decorative only; muted teal on porcelain is not used as normal text.

## Why A015 remains partial

The earlier build artifact does not include an actual Android app-drawer screenshot or a screenshot of the native Windows shell taskbar while White-P is running. A generated ICO preview alone is not proof of the operating-system taskbar result. A015 therefore adds automated capture and contract tests for these two missing visual acceptance views. The task stays `in_progress` until a later clean build publishes and the images are visually reviewed.

## Scope limitations

These screenshots run on GitHub-hosted emulator/Windows CI, not a personal physical Android device or the user's independent Windows machine. They do not prove Android-to-Windows inference, host readiness, a production-signed Android release, clean-install on user devices, or in-place upgrade.

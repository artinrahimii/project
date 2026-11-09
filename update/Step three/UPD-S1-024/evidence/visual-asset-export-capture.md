# Visual asset export capture evidence (A008)

This is automation/source evidence only. The resulting files must still be produced by a successful product build and visually inspected.

## Windows visual review artifact

The WPF startup smoke test captures `MainWindow` to `artifacts/brand-review/windows-host.png`; before upload, the workflow copies that screenshot and the generated `src/Host.Windows/Assets/WhiteP.ico` into `artifacts/brand-review/windows/`. The artifact therefore contains both the real application window and the actual multi-resolution ICO export.

## Android visual review artifact

After Android instrumentation and live MainActivity verification, `tools/capture_android_brand_review.py` captures:
- Home
- Conversations list
- newly created conversation detail
- Connections
- Privacy

It also copies all five density variants of:
- `ic_launcher.png`
- `ic_launcher_round.png`
- `ic_launcher_foreground.png`

The artifact includes the adaptive icon XML definitions, generated Android brand-color XML, the canonical SVG and the token JSON.

## Manual review still required

Inspect screenshots and icon files after successful upload. Verify the coin/P is recognizable at launcher/taskbar sizes, actual screen surfaces use the shared semantic palette, typography/control contrast passes thresholds, and no teal normal-size text on porcelain or gold small text on light surfaces exists. Artifact creation is not equivalent to design acceptance.

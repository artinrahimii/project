# UPD-S1-024 — execution report

## Attempts

- A001 — canonical vector/tokens, generator and platform wiring committed in `V 3.1.118`. Generator completed on both Windows and Android runners; a new Python assertion initially confused Kotlin ARGB literals with XML HEX strings.
- A002 — corrected the ARGB assertion in `V 3.1.120`. Android ktlint identified a generated object filename mismatch, composable naming and formatting issues.
- A003 — renamed the generated source to `WhitePBrandColors.kt`, corrected the function casing and formatting. ktlint then reported expression placement.
- A004 — moved the `lightColorScheme` call to a new line. ktlint required 12-space argument indentation and 8-space closing-parenthesis indentation.
- A005 — corrected that indentation in `V 3.1.123`. Clean CI rerun and native compilation remain pending.
- A006 — added real WPF RenderTargetBitmap capture and Android UIAutomator navigation/capture to the product build. These outputs are intended to be downloaded and inspected before acceptance. Screenshot artifacts and CI results are pending.

## Implemented sources

The one canonical SVG is `assets/brand/whitep-coin.svg`; the only five-color token source is `assets/brand/whitep-design-tokens.json`. `tools/generate_brand_assets.py` emits the multi-resolution ICO, Android density/adaptive launcher outputs and platform color resources. Android consumes `whitePTheme`; WPF loads the generated color dictionary and icon. Both product build platform jobs and the signed Android release job run the generator.

Visual evidence capture is now part of the product workflow:
- WPF smoke mode captures the rendered real `MainWindow` via `RenderTargetBitmap` to `artifacts/brand-review/windows-host.png`.
- Android emulator capture uses UIAutomator text bounds to navigate Home, Conversations, a newly created conversation detail, Connections and Privacy, then captures PNGs under `artifacts/brand-review/android/`.

## Source paths

- `assets/brand/whitep-design-tokens.json`
- `assets/brand/whitep-coin.svg`
- `tools/generate_brand_assets.py`
- `tools/capture_android_brand_review.py`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/WhitePBrandColors.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/theme/Theme.kt`
- `src/Client.Android/app/src/main/res/values/brand_colors.xml`
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- `src/Client.Android/app/src/main/AndroidManifest.xml`
- `src/Host.Windows/Assets/BrandColors.xaml`
- `src/Host.Windows/App.xaml`
- `src/Host.Windows/App.xaml.cs`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/WhiteP.Host.Windows.csproj`
- `.github/workflows/product-build.yml`
- `.github/workflows/android-release.yml`
- `tests/test_brand_identity.py`
- `docs/brand-identity.md`

## Validation state

- A003 quality gate: passed all 485 Python tests, Python syntax, both graph guards.
- Generator on Windows and Android runners: successful in earlier product runs.
- A005 lint correction and native Android/Windows builds: pending CI.
- WPF and Android visual screenshot capture: source and workflow wiring added in A006; screenshots are not yet available or inspected.
- Signed Android production release: not run.
- Physical Android handset or independent Windows host: not accessed; no physical cross-platform run is claimed.

Keep task `in_progress` until all 11 acceptance criteria, actual screenshot review and both clean native builds are verified.

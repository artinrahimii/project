# UPD-S1-024 — execution report

## Attempts

- A001 — canonical vector/token source, generator and platform wiring committed in V 3.1.118.
- A002 — corrected the test's Kotlin ARGB literal expectation in V 3.1.120.
- A003–A005 — corrected generated Kotlin filename, composable name, multiline call and indentation. In V 3.1.123, ktlint passed; the quality suite and both graph guards passed.
- A006 — added real WPF window and Android emulator screenshot capture in V 3.1.124. The Android UIAutomator bounds test passed. The existing reproducible-build contract test failed because it expected 8 artifacts/6 strict-file checks, but the workflow added two more screenshot uploads.
- A007 — updated the artifact-count contract to 10 uploads and 8 strict-file checks, and asserted both visual review artifacts. Committed in V 3.1.125.
- A008 — expanded visual artifacts to include actual icon exports in addition to screenshots: Windows ICO; Android launcher/round/foreground PNGs at every density; adaptive icon XML; Android color XML; canonical SVG and token JSON. Added regression assertions. Commit/CI pending.
- Current task state: in_progress.

## Implementation

Single canonical sources:
- `assets/brand/whitep-coin.svg`: navy canvas, white coin, gold rim, centered navy uppercase P.
- `assets/brand/whitep-design-tokens.json`: exact five palette tokens and shared semantic roles.
- `tools/generate_brand_assets.py`: standard-library asset generator.

Android uses `whitePTheme` and generated launcher resources; Windows loads `BrandColors.xaml` and uses the generated multi-resolution ICO for the executable/window. Product CI invokes the generator on clean Windows and Android jobs; signed Android release invokes it before Gradle compilation.

Visual review automation:
- Windows captures the actual WPF MainWindow in a PNG and now copies the generated ICO next to it.
- Android UIAutomator captures Home, Conversations, a newly created Conversation detail, Connections and Privacy, then copies every generated density/adaptive launcher export and shared vector/token source to the same review artifact.
- Artifact-count contract now expects the two additional upload steps and requires both screenshot artifact names.

## Current tracked source paths

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
- `tests/test_reproducible_build_contract.py`
- `docs/brand-identity.md`

## Verification still required

- A003 source ktlint and Python/graph quality passed.
- A006 failed the previous artifact-count contract; corrected in A007.
- A007 full quality/product workflow results are pending or running.
- A008 exact asset-copy regression and CI artifact publication are pending.
- Clean Android APK/release build, emulator instrumentation and visual captures require confirmation.
- Clean Windows build, WPF startup, screenshot capture and installer acceptance require confirmation.
- Production-signed Android release workflow has not been run.
- The screenshots/ICO/PNG artifacts must be downloaded and visually inspected at actual target sizes before all 11 criteria can pass.
- No physical Android handset or separate user Windows host has been used; this task does not prove physical cross-device operation.

Do not mark done until all 11 visual identity acceptance criteria are satisfied with actual native builds, exported assets, inspected screenshots and contrast review.

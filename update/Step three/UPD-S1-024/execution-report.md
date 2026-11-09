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


## A009 — harden Android visual-review navigation — 2026-11-09T21:09:51.640Z

- Exact source commit reviewed: `bf9ba88cb05a839e167395ce894976f71281a600` (`V 3.1.126`). Its Quality Gate, White-P product build and Rendezvous container workflows all completed successfully. The Product Build generated real Windows WPF, Android emulator and icon-export artifacts.
- Manual inspection of Product Build run [42685200855](https://github.com/artinrahimii/project/actions/runs/42685200855) found that `android-conversations.png` was byte-identical to `android-home.png` (local MD5 comparison: `6a0d6b514dcba4083ce7c90d537b9d1c`), and `android-conversation-detail.png` displayed `Loading conversation…` rather than a loaded conversation. Therefore the previous capture script did not prove the named routes were rendered; no visual acceptance criterion is promoted on that evidence.
- Updated `tools/capture_android_brand_review.py` to target clickable ancestors, wait for unique page landmarks with a bounded timeout, and refuse to save each page image unless its expected content is present immediately before capture. The list page, loaded detail, Connections and Privacy pages now each have route-specific landmarks.
- Added a regression test for exact visible text probes and required capture landmarks in `tests/test_brand_identity.py`. The Android emulator product-build job executes the capture script, so a route that stays on Home or remains Loading now fails the build instead of publishing misleading screenshots.
- Attempt result: partial; status remains `in_progress`. The new source-level tests and the full product workflow must run on the resulting commit, and the new screenshots/icons still require visual inspection. No physical Android handset, separate user Windows host, live Android-to-Windows inference or in-place upgrade was used for this attempt.
- The new commit SHA and exact-head CI results are recorded in the next append-only graph/report update after GitHub confirms the commit.


## A010 — use visible conversation landmarks and scroll-aware screenshots — 2026-11-10T21:32:33.791Z

- New baseline: `816a9a6576f886fae7687be57ca33fc59ba0ce15` (`V 3.1.127`), reverified as online `main` before edits.
- Product Build run [42699901550](https://github.com/artinrahimii/project/actions/runs/42699901550) completed Windows build, publish, installer, WPF startup smoke and HostLifecycle coverage. Android debug and release APK builds, Kotlin lint, JVM unit tests, and the Keystore instrumentation suite also completed. Instrumentation result from artifact `13712220302`: 11 tests, 0 failures, 1 skipped; the skipped case is the explicitly opt-in LAN acceptance test, not a Keystore failure.
- The Android job failed after those tests, in the visual-capture script: it waited for `Messages` in the initial UIAutomator dump. In `ConversationDetailScreen.kt`, the page is vertically scrollable and `Messages` appears after the conversation card, Context capsule section, and request-status section. The capture script had not scrolled, so the label may be outside the visible UI hierarchy. The run did not produce the Android screenshot artifact; it did not validate the physical-device requirement.
- Updated the capture helper to verify the loaded conversation using the visible `Back to conversations` button plus the `Created ` text prefix, then capture the upper detail viewport. It scrolls downward through the content until the empty-message landmark is visible and captures a second `android-conversation-messages.png` view that must show both `Messages` and `No messages yet...`. It then scrolls back to the list before continuing to other tabs.
- Timeout diagnostics report only the visible node count, not arbitrary visible UI text, to avoid leaking message content into CI logs.
- Added regression coverage for the Created prefix, scroll-aware Messages capture and the two required visible landmarks.
- The same CI artifact shows the emulator Keystore tests themselves had 0 failures (one explicitly skipped LAN-only test). The previous product-build conclusion remains failed because the visual capture step returned non-zero.
- Attempt result: partial; keep `UPD-S1-024` in progress until a new exact-head CI run produces and passes visual review of Android screenshots and platform icon exports. Physical Android handset, independent Windows host, live Android-to-Windows inference, signed production release and in-place upgrade remain unverified.

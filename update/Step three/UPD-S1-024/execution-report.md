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


## A011 — make WPF button labels visible on the shared dark action surface — 2026-11-10T21:35:33.964Z

- Reviewed the Windows visual-review artifact from Product Build run [42703515290](https://github.com/artinrahimii/project/actions/runs/42703515290), artifact `13713470695`, generated from `V 3.1.128`. The actual WPF startup screenshot showed dark navy button surfaces for `Connect`, `Start local engine` and `Estimate compatibility` with no visible labels. The same defect was visible in the earlier run 42699901550.
- The canonical palette and background were correct, but the default string Content rendering did not produce visible white button text in the actual screenshot. The implicit `TextBlock` style uses `midnight_ink`; the shared Button style previously set only the Button foreground and did not force the rendered string-content element to use the contrast-safe foreground.
- Added a shared Button `ContentTemplate` that renders string content through a `TextBlock` with explicit `PureWhiteBrush`. This applies centrally to the existing WPF buttons and retains the navy background; it does not change app behavior or button commands.
- Added a regression test that asserts the shared WPF Button style includes the explicit content template and white text foreground.
- Attempt result: partial; keep `UPD-S1-024` in progress until a fresh Windows screenshot demonstrates visible labels, Android screenshot captures are valid, and all remaining visual acceptance checks (including target sizes and contrast) pass. No release or physical-device criteria are inferred from this change.
- Source commit SHA is recorded in the graph after GitHub confirms it.


## Commit confirmation — A010 and A011

- A010 scroll-aware Android visual-capture source, regression test and failure evidence: `V 3.1.128`, SHA `f98c4cc5bd04d92785154b6847d9e4c89449fd40`.
- A011 WPF button-label rendering correction and regression test: `V 3.1.129`, SHA `3d3605279ef35c0a0dea8202ea6a5c9f6a367e6a`.
- Both commits were verified as ancestors of online `main`. Neither attempt marks the task complete; exact-head rendered screenshots remain mandatory.


## A012 — move WPF button text fix into the canonical asset generator — 2026-11-09T21:38:52.962Z

- Follow-up screenshot review from Product Build run [42703998918](https://github.com/artinrahimii/project/actions/runs/42703998918), artifact `13712811892`, showed the WPF button labels were still missing after A011.
- Diagnosis: `src/Host.Windows/Assets/BrandColors.xaml` is generated at clean-build time by `tools/generate_brand_assets.py`. A011 had changed only the generated output file; the clean product workflow regenerated it from the unchanged generator and removed the ContentTemplate correction. Therefore the A011 source-file edit could not survive the actual build.
- Moved the Button `ContentTemplate` rule into `_make_wpf_dictionary` in the canonical generator, regenerated the tracked `BrandColors.xaml` to match it, and changed the regression test to run the generator into a temporary clean output directory and compare the generated WPF dictionary byte-for-byte with the tracked output.
- Attempt result: partial. The next clean Windows build must produce visible button labels in the actual screenshot; the generator contract alone is not visual acceptance. Android screenshot capture and the remaining shared visual checks also remain open.


## Commit confirmation — A012

- A012 moved the button-label ContentTemplate into the canonical WPF asset generator and added a clean generated-output regression: `V 3.1.131`, SHA `725a1f59f690b819d7d8ce19b2d4861227cb9527`.
- The prior A011 screenshot artifact proves the generated output still lacked visible button labels after a clean build. The A012 commit is not visually accepted until the new clean WPF screenshot is inspected.


## A013 — align the tracked WPF resource and make the generator test semantic — 2026-11-09T21:41:03.311Z

- Step Three quality gate run [42704516200](https://github.com/artinrahimii/project/actions/runs/42704516200) failed one test out of 489: `test_wpf_button_string_content_renders_with_contrasting_foreground`. The failure was the test's byte-for-byte equality assertion between the generated WPF resource and the tracked resource; the tracked resource still lacked the ContentTemplate.
- Added the same ContentTemplate to the tracked `src/Host.Windows/Assets/BrandColors.xaml`.
- Changed the regression to parse both the freshly generated and tracked XAML and independently assert that each contains a Button ContentTemplate with a TextBlock using `PureWhiteBrush`. Formatting differences are no longer used as a proxy for the required rendered style; the clean Windows screenshot remains the visual acceptance evidence.
- Attempt result: partial; the new test and product build are pending on the next exact-head CI run.


## A013 — WPF visual acceptance and contrast audit — 2026-11-09T21:44:12.673Z

- Quality gate on `V 3.1.133`: 489 tests passed; both graph guards returned zero errors. The legacy graph retains one pre-existing metadata warning for T0001.
- WPF visual artifact from Product Build run [42704515813](https://github.com/artinrahimii/project/actions/runs/42704515813), artifact `13713870555`, was inspected at 900 × 850. Connect, Start local engine and Estimate compatibility are now visibly rendered in white on navy. The run’s overall Product Build remains in progress; this is a verified screenshot step, not an overall product-build success.
- The generated Windows ICO contains 7 embedded size entries: 16, 24, 32, 48, 64, 128 and 256 pixels. A contact sheet of the actual embedded raster sizes was visually reviewed.
- Contrast ratios and semantic pairing restrictions are recorded in `evidence/contrast-ratio-review-2026-11-10.md`. Muted teal on porcelain is reserved for non-text borders/controls (4.008:1); antique gold on light surfaces remains decorative only (2.358–2.680:1).
- Android visual capture and review remain pending on Product Build run [42704845236](https://github.com/artinrahimii/project/actions/runs/42704845236). Keep the node `in_progress`; no acceptance status is inferred from WPF-only visual proof.


## A014 — automate contrast thresholds and semantic-role regression — 2026-11-09T21:45:24.243Z

- Added a numeric WCAG relative-luminance contrast regression test using the exact five canonical palette tokens.
- The test requires ink-on-light and white-on-ink/teal text pairs to meet 4.5:1; teal-on-porcelain is checked against the 3:1 non-text component threshold and explicitly remains below the normal-text threshold; antique gold on light surfaces is required to remain below 3:1 and is semantically restricted to decorative accent; ink on gold is checked for the theme's gold-on-dark use.
- Added source-level checks for Android Compose role mappings and WPF TextBlock, Button ContentTemplate and teal control borders.
- Attempt result: partial; quality gate and exact-head product workflow are pending. This test supplements, but does not replace, review of real Android/Windows rendered screens.


## Commit confirmation — A014

- A014 added a numeric contrast-ratio regression and semantic-role checks: `V 3.1.135`, SHA `679dcf19d388854d9b0546f53d1875da5a493bf8`.
- Exact-head quality run [42705535861](https://github.com/artinrahimii/project/actions/runs/42705535861) and product run [42705535712](https://github.com/artinrahimii/project/actions/runs/42705535712) were still in progress when this append-only history record was created. Results remain pending; no status is inferred from a queued/running job.


## A014 CI result — 2026-11-09T21:47:55.186Z

- Exact commit `V 3.1.135`, SHA `679dcf19d388854d9b0546f53d1875da5a493bf8`.
- Quality Gate run [42705535861](https://github.com/artinrahimii/project/actions/runs/42705535861) passed: **490 tests**, compileall and both graph guards. The legacy graph had 0 errors and one preserved T0001 metadata warning; the update graph had 0 errors and 0 warnings.
- The new palette contrast regression passed in that run.
- Product Build run [42705535712](https://github.com/artinrahimii/project/actions/runs/42705535712) remains in progress; Android instrumentation and the Android visual screenshot artifact have not completed. `UPD-S1-024` remains `in_progress`.


## A014 follow-up synchronization — 2026-11-09T21:49:01.035Z

- The exact-head quality gate [42705535861](https://github.com/artinrahimii/project/actions/runs/42705535861) has passed 490 tests, compileall and both graph guards. The contrast regression is confirmed green.
- Only the full product build, Android instrumentation and Android visual screenshot review remain pending in the A014 follow-up. The task remains `in_progress`.


## A015 — capture the installed Android launcher and native Windows taskbar — 2026-11-09T22:23:42.847Z

- Revalidated exact online `main` at `V 3.1.138`, SHA `dcc3b2108a3e8e6e9a06fd5f5249103f60fca1cb`. Its product-build, quality-gate and Rendezvous workflows all completed successfully; the existing Windows screenshot and Android five/six page screenshots were manually inspected.
- The earlier artifact contained actual density-specific Android launcher exports and the generated Windows ICO, but no screenshot of the Android app drawer or the native Windows taskbar. I am not treating a generated ICO contact sheet as proof of its native taskbar rendering.
- Extended `tools/capture_android_brand_review.py` to return to Android Home, open the app drawer, require the installed `White-P` launcher label and capture `android-launcher.png`.
- Added `tools/capture_windows_brand_review.ps1` to launch the actual WPF executable, bring its visible top-level window to the foreground, and use the Windows shell taskbar bounds to capture both `windows-window-chrome.png` and `windows-taskbar.png`. The script fails closed if the process/window/taskbar is absent or the screenshots are invalid.
- Wired the native captures into `.github/workflows/product-build.yml` and added source-contract tests in `tests/test_brand_identity.py`.
- This attempt is **partial** until a new clean product-build run produces both new screenshots and those exact images are reviewed. The previous exact-head CI does not test these new capture changes.
- Existing exact-head baseline checks before this capture change: Android debug/release-variant builds passed; 216 JVM unit tests passed; Keystore instrumentation had 11 tests, 0 failures/errors and 1 explicit LAN-only skip; Windows WPF build/publish/startup and installer acceptance passed; HostLifecycle and Rendezvous jobs passed; quality gate and both graph guards passed.
- No physical Android phone, independent user Windows machine, production-signed Android release, live local-model inference, Android-to-Windows user flow or in-place upgrade was tested in this attempt.

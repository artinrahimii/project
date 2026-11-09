# Android visual-capture failure and diagnosis — 2026-11-10

## Exact evidence source

- Source commit: `816a9a6576f886fae7687be57ca33fc59ba0ce15` (`V 3.1.127`).
- Product Build: [run 42699901550](https://github.com/artinrahimii/project/actions/runs/42699901550).
- Android Keystore instrumentation artifact: `13712220302` (CI artifact ZIP, SHA-256 of ZIP: `69d1f7f2a6dd48343dd9f47dc0e8e5ddd7cbcb27428f0057d264c7428e4922ab`).
- Instrumentation XML: `TEST-emulator-5554 - 16-_app-.xml`: 11 tests, 0 failures, 0 errors, 1 skipped. The skipped test is `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan`, which explicitly states it is only run by the LAN-acceptance workflow.

## Failure

The Android product-build step failed after the instrumentation suite while running `tools/capture_android_brand_review.py`:

```text
RuntimeError: Android did not reach the expected screen content 'Messages' within 20.0 seconds
```

The screenshot upload was skipped because the capture command failed. The Android debug/release builds, Kotlin lint and JVM unit-test step had already completed successfully; this is not evidence of a Keystore instrumentation test failure.

## Code-based diagnosis

`ConversationDetailScreen.kt` places the `Messages` heading after the conversation card, Context capsule UI and request-status section inside a vertically scrollable Column. The prior automation tested for `Messages` before scrolling. UIAutomator's current visible hierarchy therefore may not expose that off-screen label yet. This is the most plausible cause from the actual screen structure; the failing run did not preserve a UI hierarchy dump, so it is not claimed as a directly captured runtime fact.

## Correction in A010

- Verify the loaded detail screen with visible top-of-page landmarks: `Back to conversations` and text beginning `Created `.
- Capture the detail header viewport.
- Scroll until the empty-message text is visible, then require both `Messages` and `No messages yet...` before capturing a second viewport.
- Scroll back to the top before returning to the conversation list.
- Failure diagnostics expose only a count of visible UI nodes, not arbitrary text or potential message contents.

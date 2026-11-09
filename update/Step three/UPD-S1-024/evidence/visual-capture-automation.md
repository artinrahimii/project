# UPD-S1-024 — visual capture automation

This evidence describes capture code and workflow wiring only; it is not a substitute for the resulting screenshots.

## Windows
- `src/Host.Windows/App.xaml.cs` captures the actual loaded/visible WPF `MainWindow` with `RenderTargetBitmap`.
- The smoke-test workflow sets `WHITEP_BRAND_REVIEW_SCREENSHOT` to `artifacts/brand-review/windows-host.png` and checks the resulting PNG before the screenshot artifact is uploaded.

## Android
- `tools/capture_android_brand_review.py` runs only after the existing emulator job has installed and launched the application and verified the resumed MainActivity.
- UIAutomator supplies real text-node bounds; the helper taps the visible navigation labels rather than using fixed screen coordinates.
- Intended captures: Home, Conversations list, a newly created Conversation detail, Connections and Privacy.
- The Android artifact upload step fails on missing output, so a green product workflow confirms that the capture script completed and all requested PNG files exist.

## Review still required
Download both artifacts from the successful workflow, inspect the actual PNG images at native dimensions, verify the P coin at launcher/window sizes and check typography/control contrast on the captured screens. A successful upload alone does not verify visual design quality.

# شواهد UPD-S1-017 — typed error و correlation

## Baseline
- Commit: `66f21b572b2766433d1d9eed40e98bf7c8ebba73` / `V 3.1.48`.
- گراف update: ۲۴ node، ۸۰ edge و ۱۳ wave.
- سه پیش‌نیاز مستقیم UPD-S1-002/003/004 در همین گراف status=done و دارای output/evidence/test record بودند.

## تغییرات wire additive
- `InferenceResponse.error_code = 10`.
- `ProtocolError.attempt_id = 5`, `round_id = 6`.
- `HostBusy.attempt_id = 5`, `round_id = 6`.
- `UnsupportedModel.attempt_id = 4`, `round_id = 5`.
- شمارهٔ فیلدهای موجود تغییر نکرده است.

## قانون correlation Android
خطای remote فقط زمانی pending attempt را fail می‌کند که:
- request ID و attempt ID کلید `AttemptKey` را دقیقاً بسازند؛
- round ID با pending round برابر باشد؛
- peer ID و stream controller با peer احرازشده و controller اصلی pending یکسان باشند؛
- host ID/model ID اختیاری، در صورت وجود، با pending peer/model برابر باشند.
پیام ناقص، stale، مربوط به round دیگر یا stream دیگر unmatched می‌ماند و attempt دیگری را fail نمی‌کند.

## نگاشت terminal
- SUCCEEDED و CANCELLED: error_code باید UNSPECIFIED باشد.
- FAILED: UNSPECIFIED (سازگاری قدیمی)، HOST_BUSY، RESOURCE_LIMIT یا INTERNAL.
- TIMED_OUT: UNSPECIFIED (سازگاری قدیمی) یا REQUEST_EXPIRED.
- REJECTED: INVALID_MESSAGE، UNSUPPORTED_VERSION، UNSUPPORTED_MODEL، REQUEST_EXPIRED، RESOURCE_LIMIT یا INTERNAL (به‌علاوهٔ UNSPECIFIED برای سازگاری).
- code ناشناخته یا ناسازگار با outcome رد می‌شود.

## آزمون
C# HostLifecycle/mapper/identity، Kotlin parser/ViewModel و Python source-contract tests اصلاح شده‌اند. نتیجهٔ اجرایی و run ID پس از CI دقیق commit ثبت خواهد شد.


## CI exact commit V 3.1.49

- Quality run 42477241092 passed: 479 Python tests; update graph guard 24 nodes/80 edges, zero errors/warnings; legacy guard 230/422, zero errors and one preserved T0001 metadata warning.
- HostLifecycle job 128721195755 passed including coverage.
- Windows WPF job 128721195868 passed clean build, publish, startup smoke, policy/DPAPI checks and installer artifact uploads.
- Rendezvous run 42477240994 passed.
- Android job 128721195746 failed at ktlintTestSourceSetCheck before APK/JVM/Keystore steps. Only two test files failed style checks; corrected in second attempt.
- Real local inference run 42477241035 failed before binary verification because GitHub returned a transient HTML error page instead of the pinned b11443 archive. No GGUF or inference ran.
- No physical device, Android-to-Windows, clean installation on a target user endpoint or in-place upgrade was tested.

## CI exact V 3.1.50
- Quality 42477696370: 479 tests passed; update graph guard 24/80 zero errors/warnings; legacy guard 230/422 zero errors/one preserved T0001 warning.
- HostLifecycle passed with coverage; Rendezvous container build and smoke passed.
- Android 42477696429 failed only at ktlint in RequestViewModelTest import order and two multiline response-card error assertions; parser file was clean after attempt 2.
- Windows WPF job remained in progress at this evidence update.
- APK/JVM/Keystore did not run on V3.1.50. No physical Android-to-Windows test or in-place upgrade performed.


## CI exact V 3.1.51
- Quality 42478015523 passed: 479 Python tests; update graph guard zero errors/warnings; legacy graph zero errors and one retained warning.
- HostLifecycle 128723595644 passed with coverage.
- Windows WPF 128723595595 passed clean build/publish/generated Protobuf/startup/policy/DPAPI/installer checks.
- Rendezvous 42478015406 passed.
- Android 128723595688 passed test-source lint but failed main-source ktlint in RequestViewModel terminal error-code validation; APK/JVM/Keystore steps skipped.

## CI exact V 3.1.52
- Quality 42478443061 passed: 479 Python tests and graph guards.
- Rendezvous 42478442987 passed.
- HostLifecycle 128724891789 passed with coverage.
- Windows WPF build/publish/generated Proto/smoke/policy passed; Windows installer was still running at report time.
- Android 128724891880 failed main-source ktlint on three multiline enum-code set expressions; APK/JVM/Keystore steps skipped.
- A second retry of real local inference failed while downloading the pinned llama.cpp archive; no SHA-256, GGUF or inference result.

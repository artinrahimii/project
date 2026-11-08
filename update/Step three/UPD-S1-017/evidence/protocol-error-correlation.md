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

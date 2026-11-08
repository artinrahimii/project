# بازبینی T0174 — ارسال CancelRequest به attemptهای منتخب

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` — `V 3.1.92`
- snapshot کد محصول بررسی‌شده: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد بررسی‌شده

- دکمهٔ لغو در Android به `RequestViewModel.cancelCurrentRequest` متصل است و فقط cancellation مربوط به dispatch فعال را اجرا می‌کند.
- ViewModel، request ID و attemptهای همان dispatch را به `cancelSelectedHosts` می‌دهد. Transport برای هر attempt صف‌شده یک `CancelRequest` شامل requestId، attemptId و reason معتبر می‌سازد، آن را به peer احرازشده می‌فرستد و buffer wire را پاک می‌کند.
- Windows با `ProtocolMessageParser.ParseCancelRequest` و validator مشترک، شناسه‌ها و reason را کنترل می‌کند؛ سپس فقط CancellationTokenSource متناظر با همان زوج request/attempt را لغو می‌کند.
- در سیاست first release فعلی dispatch یک میزبان فعال دارد؛ API transport برای چند attempt منتخب generic باقی مانده است.

## آزمون و شواهد

- `RequestViewModelTest.kt`: لغو از UI به attempt صف‌شدهٔ دقیق وصل است و پس از پاسخ لغو، run status برابر CANCELLED و retry غیرفعال می‌شود.
- `Libp2pInferenceRequestTransportTest.kt`: ارسال cancellation به چند peer احرازشده و آزمون interop با میزبان .NET را پوشش می‌دهد.
- `PeerMessageSecurityAcceptanceTests.cs`: پیام malformed، شناسهٔ ناقص و reason نامعتبر را پیش از dispatch رد می‌کند.
- Product CI دقیق کد: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Android و HostLifecycle test jobs در آن اجرا شدند. Quality CI [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.
- آزمون محلی مستقل اجرا نشد؛ loopback/native-host integration tests در CI شاهد هستند، نه تست فیزیکی شبکهٔ کاربر.

## تصمیم

وضعیت قدیمی `done` حفظ شد؛ `re_review_required` باقی می‌ماند تا پس از UPD-S1-015/017 رفتار لغو در برابر terminal race، timeout و retry و در آزمون Android-to-Windows واقعی دوباره تأیید شود.

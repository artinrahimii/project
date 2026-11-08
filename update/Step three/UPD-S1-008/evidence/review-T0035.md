# بازبینی T0035 — سیاست نسخهٔ پروتکل

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ پیش‌نیازهای `T0034` و `T0030` هر دو `done` هستند.
- تصمیم: `re_review_required`؛ وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-017`.
- `src/Shared.Contracts/ProtocolVersionPolicy.cs` نسخهٔ جاری `1.0.0` را تعریف و رد پیام نامعتبر/نسخهٔ ناسازگار را به wire error codeهای `PROTOCOL_ERROR_CODE_INVALID_MESSAGE` و `PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION` نگاشت می‌کند.
- schema پیام `ProtocolError` همین error codeها را تعریف می‌کند؛ آزمون‌های `tests/test_proto_contract.py` و `tests/HostLifecycle.Tests/Program.cs` وجود و رفتار policy را کنترل می‌کنند.
- Product run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) آزمون‌های HostLifecycle و build واقعی Windows/Android را موفق گذراند؛ Quality run [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) نیز موفق است.
- تغییر چرخهٔ runtime/protocol در `UPD-S1-017` ممکن است version negotiation/error mapping را متأثر کند؛ پس از آن مصرف‌کنندگان باید دوباره بررسی شوند. وضعیت پایه تغییر نکرده است.

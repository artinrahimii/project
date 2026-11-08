# بازبینی T0034 — پیام‌های لغو و خطا

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ پیش‌نیازهای `T0033` و `T0030` هر دو `done` هستند.
- تصمیم: `re_review_required`؛ وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-017`.
- schema پیام‌های `CancelRequest`، `HostBusy`، `UnsupportedModel` و `ProtocolError` را تعریف می‌کند. فیلدهای request/attempt/round، شناسهٔ مدل، retry_after، کد خطا و retryable در schema بررسی شدند.
- `tests/test_proto_contract.py` در V3.1.66/67 فیلدهای attempt_id و round_id را برای سه پیام Busy/UnsupportedModel/ProtocolError به assertions اجباری تبدیل کرد؛ Quality دقیق V3.1.67 [42484306209](https://github.com/artinrahimii/project/actions/runs/42484306209) و Quality دقیق V3.1.69 [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) موفق بودند.
- C# و Kotlin round-trip برای هر 11 پیام در run موفق Product build [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) اجرا شد.
- `UPD-S1-017` معنا و انتساب وضعیت‌های لغو، busy و error را در چرخهٔ واقعی درخواست گسترش می‌دهد؛ پس از آن semantics و مصرف‌کنندگان باید دوباره بررسی شوند. وضعیت پایهٔ `done` حفظ شد.

# بازبینی T0033 — HostAnnouncement و HostCapabilities

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ پیش‌نیازهای `T0032` و `T0030` هر دو `done` هستند.
- تصمیم: `re_review_required`؛ وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-017`.
- `HostAnnouncement` و `HostCapabilities` در schema canonical تعریف شده‌اند؛ اعلان فقط metadata میزبان و capabilities را حمل می‌کند، نه prompt، متن درخواست، شناسهٔ دائمی کاربر یا history.
- مدل `ModelCapability` دارای model_id، display_name، سقف context، سقف output و supports_streaming است. آزمون `test_proto_contract.py` این فیلدها و منع prompt/request_text/user identifiers/conversation_history/response_text را در metadata discovery کنترل می‌کند.
- Product build run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) تولید C#/Kotlin و آزمون‌های دو پلتفرم را موفق کرد؛ Quality run دقیق V3.1.69 [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) نیز موفق شد.
- `UPD-S1-017` چرخهٔ اعلان/آمادگی/مصرف metadata را تغییر می‌دهد؛ پس از اجرای آن باید محرمانگی metadata و مصرف‌کنندگان مستقیم/غیرمستقیم دوباره آزموده شوند. وضعیت پایه حفظ شد.

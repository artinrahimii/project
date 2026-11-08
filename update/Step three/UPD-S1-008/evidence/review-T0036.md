# بازبینی T0036 — تولید قرارداد C# و Kotlin از schema

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ پیش‌نیازهای `T0035` و `T0030` هر دو `done` هستند.
- تصمیم: `re_review_required`؛ وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-017`.
- `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` همان `protocol/whitep/v1/messages.proto` را با Protobuf build input می‌گیرد؛ Android Gradle نیز plugin `com.google.protobuf` را فعال و همان مسیر repository را برای تولید Java/Kotlin مصرف می‌کند.
- `ProtocolMessages.kt` از generated contract types استفاده می‌کند و `parseFrom`/`toByteArray` را به کار می‌برد؛ hand-written parallel schema یا lite runtime در تنظیمات بررسی‌شده پیدا نشد.
- آزمون `test_proto_contract.py` یگانگی schema، تولید source در workflow و bindingهای هر دو پلتفرم را می‌سنجد. Product run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) تولید C#/Java/Kotlin، round-trip C#/Kotlin و Android JVM tests را موفق اجرا کرد؛ Quality run [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) نیز سبز بود.
- `UPD-S1-017` قرارداد/چرخهٔ درخواست را گسترش می‌دهد؛ پس از آن باید regeneration، سازگاری generated API و آزمون‌های wire در هر دو پلتفرم دوباره بررسی شوند. وضعیت پایهٔ `done` حفظ شد.

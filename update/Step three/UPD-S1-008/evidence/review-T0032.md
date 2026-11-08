# بازبینی T0032 — قرارداد InferenceResponse

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ پیش‌نیازهای `T0031` و `T0030` هر دو `done` هستند.
- تصمیم: `re_review_required`؛ وظایف متأثر: `UPD-S1-004` و `UPD-S1-017`.
- پیام `InferenceResponse` در `protocol/whitep/v1/messages.proto` دارای شناسهٔ درخواست، attempt/round، status، متن پاسخ، خطا و metadata تولید است؛ schema از مسیر واحد مشترک نگهداری می‌شود.
- آزمون `tests/test_proto_contract.py` وجود فیلدهای قرارداد را می‌سنجد و suiteهای C# و Kotlin هر 11 پیام را با fixtureهای مشترک round-trip می‌کنند: `tests/HostLifecycle.Tests/Program.cs`, `ProtoRoundTripTest.kt`, `tests/contracts/protobuf-wire-fixtures.txt`.
- Product run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) روی snapshot schema/مصرف‌کنندهٔ یکسان، آزمون C#، JVM Android و تولید bindingها را با موفقیت اجرا کرد؛ Quality run دقیق V3.1.69 [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) نیز سبز بود.
- `UPD-S1-004` تصمیم streaming را تعیین می‌کند و `UPD-S1-017` چرخهٔ runtime/وضعیت را گسترش می‌دهد؛ پس از نهایی‌شدن آن‌ها schema، status/error semantics، round IDs و مصرف‌کنندگان باید مجدداً بررسی شوند. وضعیت قدیمی تغییر نکرده است.

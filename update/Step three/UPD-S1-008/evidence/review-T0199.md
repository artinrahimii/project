# بازبینی محتوایی T0199 — UPD-S1-008

## مبنا و وضعیت
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit دقیق بررسی: `fe489b7f141a9a969f550b851093e3fcf45c8a19` — `V 3.1.67`
- وظایف به‌روزرسانی مرتبط: `UPD-S1-008` و `UPD-S1-019`
- وضعیت پایه و فعلی T0199: `done`؛ هیچ تغییر وضعیتی انجام نشد.
- وابستگی‌های قدیمی T0199: `T0195` و `T0198`؛ هر دو در گراف قدیمی `done` هستند.
- تصمیم reconciliation: `re_review_required`؛ بازبینی فعلی انجام شده، اما پس از اجرای UPD-S1-019 باید مصرف‌کنندگان و معیارهای جدید دوباره بررسی شوند.

## کد، قرارداد و مصرف‌کنندگان
- `protocol/whitep/v1/messages.proto` منبع canonical قرارداد v1 است و 11 پیام اصلی دارد.
- `InferenceRequest` و `InferenceResponse` شناسه‌های request/attempt/round، نسخهٔ پروتکل، outcome و error را در قرارداد مشخص می‌کنند.
- `HostBusy`، `UnsupportedModel` و `ProtocolError` نیز شناسهٔ attempt و round را حمل می‌کنند؛ `tests/test_proto_contract.py` در commit بررسی‌شده فیلدهای این سه پیام را صریحاً کنترل می‌کند.
- همین آزمون، فیلدهای `CancelRequest` و پیام‌های اصلی را بررسی می‌کند، یکتایی شماره‌های protobuf را کنترل می‌کند، و تولید کد C# و Java/Kotlin را از همان schema canonical بررسی می‌کند.
- آزمون‌های `tests/HostLifecycle.Tests/Program.cs` و `src/Client.Android/app/src/test/java/com/whitep/client/protocol/ProtoRoundTripTest.kt` هر 11 پیام را با round-trip و fixtureهای مشترک `tests/contracts/protobuf-wire-fixtures.txt` می‌آزمایند.
- `tests/contracts/test_contract_layout.py` یگانگی مسیر schema را کنترل می‌کند؛ تنظیمات پروژهٔ Shared.Contracts و Android Gradle نیز مستقیماً همان فایل پروتکل را مصرف می‌کنند.
- مصرف‌کنندگان C# و Kotlin از یک تعریف schema و fixture مشترک استفاده می‌کنند؛ schema یا قرارداد موازی در بررسی حاضر مشاهده نشد.

## شواهد اجرای آزمون
- Quality workflow دقیق commit V3.1.67، run [42484306209](https://github.com/artinrahimii/project/actions/runs/42484306209)، با نتیجهٔ **success** پایان یافت. این اجرا آزمون جدید فیلدهای attempt/round در پیام‌های Busy/UnsupportedModel/ProtocolError، suite کامل Python، compileall و guardهای هر دو گراف را در همان commit اجرا کرد.
- Product build run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) روی commit V3.1.61 با موفقیت کامل شد؛ jobهای Windows، Android و HostLifecycle همگی موفق بودند. آزمون C# و آزمون JVM اندروید، تولید bindingهای Protobuf و instrumentation مربوط به Android Keystore نیز موفق شدند.
- از V3.1.61 تا V3.1.67، تغییرات مرتبط محصول/قرارداد برای این وظیفه محدود به افزودن assertionهای Python در `tests/test_proto_contract.py` بوده است؛ فایل schema، fixture، آزمون‌های C#/Kotlin و تنظیمات build در diff این بازه تغییر نکرده‌اند. آزمون‌های Python این تغییر دقیقاً در Quality run V3.1.67 اجرا شدند.
- Product build دقیق V3.1.67، run [42484306205](https://github.com/artinrahimii/project/actions/runs/42484306205)، در زمان ثبت این بازبینی هنوز `in_progress` بود؛ موفقیت این run ادعا نمی‌شود.

## تصمیم و پیگیری
- معیار legacy T0199 برای وجود قرارداد مشترک و اجرای آزمون‌های C#/Kotlin در CI با شواهد موجود برقرار است؛ وضعیت قدیمی `done` حفظ شد.
- تصمیم `re_review_required` به این دلیل ثبت می‌شود که `UPD-S1-019` آزمون‌های بیشتری برای timeout، لغو، retry، میزبان busy، mismatch، هویت/اعتماد، replay، دستکاری، مدل و نگاشت وضعیت اضافه می‌کند. پس از تغییر تست‌ها و قراردادها باید مصرف‌کنندگان مستقیم و غیرمستقیم دوباره بازبینی شوند.
- این بازبینی اثبات اتصال واقعی Android به Windows، پاسخ مدل محلی زنده یا آزمون فیزیکی end-to-end نیست. این دروازه‌ها جداگانه اجرا نشده‌اند.
- `review_status=reviewed` یعنی اثر فعلی ارزیابی شده است؛ `decision=re_review_required` الزام بازبینی بعدی پس از UPD-S1-019 را حفظ می‌کند.

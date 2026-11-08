# بازبینی محتوایی T0020 — UPD-S1-008

## مبنای بررسی
- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit بررسی‌شده: `c31ac02d39993965bd693b53f626d5cb09be7b2c` — `V 3.1.60`.
- quality workflow همان commit: [42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315) — موفق.
- وظایف به‌روزرسانی مؤثر: `UPD-S1-006` و `UPD-S1-008`.

## Task baseline و تصمیم
- شناسه: `T0020`.
- شرح: ثبت تصمیم‌های انتخاب فناوری در `docs/decisions/` و پیوند هر تصمیم با کد و آزمون متناظر.
- وضعیت پایه و نهایی: `done → done`.
- وابستگی‌های قدیمی: `T0019` و `T0010`؛ هر دو `done`.
- تصمیم reconciliation: `unchanged`.
- `status_changed=false`؛ وضعیت و وابستگی‌های قدیمی تغییر نکردند.

## بررسی دفتر تصمیم و ADRها
- `docs/decisions/README.md` دفتر تصمیم‌های فناوری است؛ برای هر تصمیم دامنه، source/configuration، test/verification و مرز اعتبار شواهد را نگه می‌دارد.
- دفتر ADR-0001 را به `global.json`, Windows WPF csproj، Android Gradle configuration و product build workflow متصل می‌کند.
- ADR-0002 مسیر direct P2P و محدودیت rendezvous به metadata را به requirements، source و آزمون‌های قراردادی وصل می‌کند.
- ADR-0003 backend محلی llama.cpp و lifecycle را به `src/Host.Infrastructure/LlamaCppServerBackend.cs`, رابط میزبان و HostLifecycle tests متصل می‌کند.
- ADR-0004 تا ADR-0006 و تصمیم‌های بعدی نیز در دفتر فهرست شده‌اند؛ خود دفتر تصریح می‌کند accepted design یا build success به معنی runtime delivery نیست.
- ADR-0001 مقادیر configuration موجود را توصیف می‌کند: .NET SDK 10.0.100، WPF/net10.0-windows و Android compile/target SDK 36 و min SDK 26؛ این‌ها baseline ساخت‌اند و device compatibility را اثبات نمی‌کنند.
- ADR-0003 backend را به llama.cpp server مستقل، health check و HTTP chat completion وصل می‌کند؛ live model اجرا/بسته‌بندی نشده و AC-LOCAL-01/02 به‌صراحت NOT RUN باقی می‌مانند.
- README دفتر تصمیم‌ها را لینک می‌کند و تصریح می‌کند وجود ADR و source path به معنی تکمیل محصول نیست.

## آزمون‌ها و مصرف‌کنندگان
- `tests/test_decision_traceability.py` وجود ADR/source/config/test paths را بررسی می‌کند و platform config را با ADR تطبیق می‌دهد.
- همین test تصمیم P2P را به REQ-010/011، Android request runtime، transport و AC-P2P-01 متصل می‌کند؛ تصمیم backend محلی را نیز به adapter، UI، lifecycle tests و AC-LOCAL-01/02 مرتبط می‌سازد.
- `tests/test_product_build_contract.py` قرارداد ورودی‌های build و workflow را بررسی می‌کند.
- هر دو test در quality workflow دقیق V3.1.60، run `42482227315`، موفق شدند. کل Python suite شامل **479 تست موفق** بود؛ compileall و graph structure/output-evidence guards نیز موفق شدند.
- Product build workflow `42482227320` هنگام بازبینی هنوز در حال اجرا بود؛ موفقیت build کامل به T0020 نسبت داده نشده است.

## مرز شواهد و نتیجه
- Scope اصلی T0020 یعنی ثبت و ردیابی تصمیم فناوری با لینک‌های واقعی و آزمون قراردادی، همچنان fulfilled است؛ تصمیم `unchanged` و status `done` حفظ شد.
- اجرای مدل زنده روی میزبان هدف اثبات نشده؛ `AC-LOCAL-01/02` NOT RUN.
- انتقال کامل Android-to-Windows اثبات نشده؛ `AC-P2P-01` NOT RUN.
- clean install، in-place upgrade و release-ready بودن هر دو پلتفرم از این بازبینی نتیجه‌گیری نمی‌شوند.

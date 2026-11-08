# گزارش اجرای UPD-S1-004 — تعیین محدودهٔ Streaming

- مبنای کد و قرارداد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` — `V 3.1.38`
- تصمیم و آزمون regression: `71ddc79726691c4c437ea4d8f97ca9866618eb9c` — `V 3.1.39`
- اصلاح آزمون و ثبت ADR: `380c89043d7e2cfce7bc50926419a1a682aaf0cb` — `V 3.1.40`
- **وضعیت نهایی: done** — تمام معیارهای این وظیفه با CI دقیق V 3.1.40 تأیید شدند.

## تصمیم نسخهٔ هدف

نسخهٔ نخست از **یک پاسخ کامل و نهایی برای هر تلاش** استفاده می‌کند. Streaming قطعه‌ای به نسخهٔ بعد موکول شده است؛ تا زمان تعریف و تصویب قرارداد نسخه‌دار chunk، هیچ UI یا backend حق ندارد صرفاً از روی metadata قابلیت `supports_streaming` مسیر streaming را فعال کند.

## ممیزی قرارداد و پیاده‌سازی موجود

- `protocol/whitep/v1/messages.proto` یک `InferenceResponse` با `output_text` کامل، outcome نهایی، زمان تکمیل و شناسه‌های request/attempt/round تعریف می‌کند؛ پیام chunk/delta یا شمارهٔ ترتیب chunk وجود ندارد.
- `ModelCapability.supports_streaming` فقط metadata قابلیت است و به‌تنهایی framing، ترتیب chunk، پایان یا خطای terminal را تعریف نمی‌کند.
- `LlamaCppServerBackend` قابلیت Streaming را `false` اعلام می‌کند و در payload درخواستش stream flag ندارد.
- `LlamaCppInferenceEngine.GenerateAsync` یک `Task<string>` کامل برمی‌گرداند و cancellation را به generation فعال منتقل می‌کند.
- انتقال Android یک پاسخ را برای هر request/attempt منتظر می‌ماند و آن را با peer احرازشده، round، model و deadline تطبیق می‌دهد. ViewModel پس از دریافت نتیجهٔ terminal متن کامل را به UI می‌رساند.
- `CancelRequest` به request و attempt دقیق وابسته است؛ success، failure، timeout، cancellation و rejection outcomeهای متمایز دارند.

## رفتار الزامی نسخهٔ نخست

پاسخ فقط پس از دریافت پاسخ موفق و correlate‌شده به گفت‌وگو اضافه می‌شود. متن ناقص به‌عنوان پاسخ موفق نمایش داده نمی‌شود. timeout، disconnect، cancellation، error و rejection وضعیت terminal مستقل می‌مانند. تغییر غیرضروری در schema مشترک انجام نشد و قرارداد سالم حفظ شد.

## خروجی‌های واقعی

- `docs/decisions/ADR-0023-one-shot-inference-v1.md`: تصمیم و محدودهٔ deferred streaming.
- `docs/decisions/README.md`: ثبت ADR-0023 با پیوند به source، آزمون‌ها و workflowها.
- `tests/test_inference_streaming_scope.py`: چهار آزمون regression شامل قرارداد terminal یک‌باره، عدم فعال‌بودن streaming backend، دریافت پاسخ correlate‌شده در Android و cancellation دقیق.
- این وظیفه هیچ تغییر wire protocol لازم نداشت؛ فایل canonical `messages.proto` ممیزی و حفظ شد.

## آزمون و CI دقیق

1. [Step Three quality gate — run 42463288660](https://github.com/artinrahimii/project/actions/runs/42463288660): **موفق**؛ تمام ۴۷۲ آزمون Python، از جمله هر چهار آزمون regression جدید، به همراه graph guard، update graph contract و decision traceability پاس شدند.
2. [White-P product build — run 42463288704](https://github.com/artinrahimii/project/actions/runs/42463288704): **موفق**؛ jobهای Android، Windows WPF/installer و HostLifecycle همگی سبز شدند.
3. گزارش Android: ۲۲۵ تست؛ ۲۲۴ موفق، صفر شکست، یک skipped. Artifactهای JUnit/coverage و Keystore instrumentation با شناسه‌های `13610895903`، `13610288694` و `13610099035` ثبت شدند. این آزمون emulator است، نه گوشی فیزیکی کاربر.
4. گزارش Windows HostLifecycle: ۶۷ نشانگر critical PASS، یک skip، و ۸۵٫۷۶٪ line coverage؛ artifact `13610527167`.
5. [Rendezvous container build and smoke — run 42463288598](https://github.com/artinrahimii/project/actions/runs/42463288598): **موفق**.

## خطاهای تاریخی و اصلاح

CI نسخهٔ `V 3.1.39` در run `42463139418` چهار شکست در آزمون/ردیابی داشت: regexهای بیش‌ازحد escape‌شده، فرض تک‌خطی بودن builder در assertion، و ثبت‌نشدن ADR-0023 در README. این موارد در `V 3.1.40` اصلاح شدند؛ سابقهٔ شکست در execution history باقی مانده و کیفیت دقیق commit اصلاحی سبز است.

## محدودیت شواهد

این تکمیل فقط معیارهای UPD-S1-004 را پوشش می‌دهد. آزمون Android-to-Windows با گوشی فیزیکی و Windows فیزیکی، clean install محصول در محیط هدف، in-place upgrade و release acceptance در این وظیفه اجرا نشده‌اند و نباید از CI runner به آن‌ها تعمیم داد.

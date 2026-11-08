# بازبینی T0177 — سقف میزبان متمایز و بودجهٔ تلاش

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` — `V 3.1.94`
- source code baseline: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و آزمون

- `DistributedRequest.kt` دو policy صریح دارد: first release سه round، یک میزبان فعال در هر round و سه تلاش کل؛ generic compatibility سه میزبان در هر round و حداکثر نه تلاش کل.
- `HostCandidateSelector` و `InferenceAttemptDispatcher` در compatibility mode سه میزبان متمایز را در هر round و سقف کل را رعایت می‌کنند.
- `InferenceAttemptDispatcherTest.kt` سه میزبان distinct در هر round و نه تلاش کل، retry یک peer با allowlist و بودجهٔ باقی‌مانده را بررسی می‌کند.
- `tests/test_android_inference_attempt_budget_acceptance.py` ثابت‌های first-release و compatibility، سقف selection و رد بودجهٔ بیش از حد قبل از transport را به source وصل می‌کند.

## شواهد اجرای CI

- Product Build دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Android JVM tests اجرا شدند.
- Quality Gate دقیق source: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- آزمون محلی در این محیط اجرا نشد.

## تفاوت سیاست و تصمیم

این آزمون‌ها مسیر generic سه‌میزبانه/نه‌تلاش را نیز پوشش می‌دهند؛ اما مسیر تولیدی نسخهٔ نخست عمداً فقط یک میزبان فعال در هر round و حداکثر سه تلاش ترتیبی را می‌پذیرد. این تفاوت باید پس از UPD-S1-015 با تصمیم مصوب و آزمون runtime نهایی تطبیق داده شود. وضعیت پایهٔ `done` حفظ، و `re_review_required` باقی ماند.

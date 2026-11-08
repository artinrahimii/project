# بازبینی T0175 — وضعیت failed و امکان retry دستی

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` — `V 3.1.92`
- snapshot کد محصول بررسی‌شده: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد بررسی‌شده

- `RequestViewModel.updateAggregatedRequestStatus` نتیجهٔ attemptها را به IDLE/SUCCEEDED/FAILED/CANCELLED نگاشت می‌کند، وضعیت typed را به lifecycle persistence می‌فرستد و فقط وقتی شکست، بودجه و deadline اجازه دهند `canRetryRequest` را روشن می‌کند.
- `MainActivity.kt` دکمهٔ «Try the next trusted host manually» را فقط هنگام مجازبودن retry نشان می‌دهد و هم‌زمانی درخواست/لغو را کنترل می‌کند.
- `retryFailedAttempts` تنها با اقدام صریح UI آغاز می‌شود؛ requestId ثابت می‌ماند، attemptId جدید تولید می‌شود، peer قبلی از انتخاب بعدی حذف می‌شود و retry خودکار انجام نمی‌شود.
- موفقیت یا لغو، retry بعدی را غیرفعال می‌کند؛ retry نمی‌تواند از سقف round/attempt یا deadline عبور کند.

## آزمون و شواهد

- `RequestViewModelTest.kt`: نمایش FAILED و retry فقط پس از اقدام کاربر، حفظ requestId، تولید attemptId جدید و انتخاب peer بعدی را آزمایش می‌کند؛ موفقیت باعث حذف امکان retry می‌شود.
- `tests/test_android_manual_retry_contract.py`: قرارداد manual retry و جلوگیری از retry خودکار را کنترل می‌کند.
- Product CI دقیق کد: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality CI [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند.
- آزمون محلی جداگانه در این محیط اجرا نشد.

## محدودیت و تصمیم

وضعیت legacy `done` حفظ شد. `re_review_required` ثبت می‌شود تا پس از UPD-S1-015، بودجه و UI با policy نهایی میزبان/timeout و رفتارهای جدید تطبیق داده شوند. پاسخ یا خطای این flow هنوز همان پاسخ پایدار در گفت‌وگوی Room نیست و آزمون فیزیکی Android-to-Windows محسوب نمی‌شود.

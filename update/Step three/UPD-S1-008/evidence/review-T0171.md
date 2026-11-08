# بازبینی T0171 — جلوگیری از انتخاب دوبارهٔ میزبان امتحان‌شده

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` — `V 3.1.92`
- snapshot کد محصول بررسی‌شده: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ commitهای بعدی در این مسیر فقط گراف و شواهد را تغییر داده‌اند.
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد بررسی‌شده

- `DistributedRequestProgress.afterDispatch(hostIds)` تاریخچهٔ مرتب‌شدهٔ peerها را نگه می‌دارد و شناسهٔ خالی/تکراری را رد می‌کند.
- `HostCandidateSelector.selectNextRound` request ID، تکمیل round قبلی، ظرفیت باقی‌مانده و سازگاری تاریخچه را کنترل می‌کند؛ به‌طور پیش‌فرض همهٔ peerهای امتحان‌شده را exclude می‌کند.
- allowlist retry فقط می‌تواند peerهایی را شامل شود که قبلاً در همان request امتحان شده‌اند؛ شناسهٔ ناآشنا یا خالی fail-closed می‌شود.
- مسیر اول انتشار در `RequestViewModel` hostهای قبلی را از selection بعدی حذف می‌کند؛ انتخاب مجدد فقط در سیاست صریح مجاز قابل انجام است.

## آزمون و شواهد

- `HostCandidateSelectorTest.kt`: exclusion همهٔ roundهای قبلی، fail-closed در نبود تاریخچهٔ کامل و allowlist retry را پوشش می‌دهد.
- `DistributedRequestTest.kt`: حفظ تاریخچهٔ peer را کنترل می‌کند.
- `Libp2pInferenceRequestTransportTest.kt`: مسیرهای انتخاب peer بعدی پس از timeout و عدم انتخاب هم‌زمان چند میزبان را پوشش می‌دهد.
- Product CI دقیق کد: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Quality CI دقیق کد [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.
- آزمون محلی مستقل اجرا نشد؛ CI دقیق همان source snapshot شاهد است.

## تصمیم

وضعیت قدیمی `done` باقی ماند. `re_review_required` حفظ می‌شود تا UPD-S1-015 retry ترتیبی، timeout، busy و allowlist را در policy نهایی تأیید کند. اجرای واقعی شبکهٔ عمومی یا دستگاه فیزیکی بخشی از این بازبینی نبود.

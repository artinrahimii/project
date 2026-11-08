# بازبینی T0203 — سه host مستقل و پاسخ‌های جداگانه

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70`، `V 3.1.102`
- کد تاریخی آزمون سه‌فرایندی: `a074815979aff371650c5101c514989f42180968`، `V 3.0.922`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وضعیت reconciliation: `reviewed`
- UPDهای مرتبط: UPD-S1-003، UPD-S1-015، UPD-S1-019، UPD-S1-020

## شواهد تاریخی

- گزارش T0203، Product Build run [41886746018](https://github.com/artinrahimii/project/actions/runs/41886746018)، Android job 126773137271 و JUnit artifact را ثبت کرده است.
- در source تاریخی، تست `dispatchesRoundToThreeNativeDotNetLibp2pHosts` سه process .NET جداگانه با libp2p identity و endpoint متفاوت اجرا می‌کرد و پاسخ را بر اساس host/attempt/request بررسی می‌کرد.
- گزارش همان آزمون صریحاً می‌گوید هر سه process روی یک CI runner و loopback اجرا شدند و متن پاسخ هر سه یکسان `dotnet-host-response` بود؛ بنابراین independence در correlation/identity بود، نه سه ماشین فیزیکی یا سه مدل متفاوت.

## کد فعلی و مغایرت

- در transport test فعلی، تست سه‌فرایندی فوق دیگر وجود ندارد. تست فعلی `dispatchesOnlyOneHostFromMultipleAuthenticatedPeerCandidates` سه candidate را می‌سازد اما فقط یک peer را dispatch می‌کند؛ تست .NET دیگر پاسخ peer بعدی را پس از timeout round اول بررسی می‌کند.
- policy محصول first-release در runtime انتخاب یک میزبان در هر round و سه تلاش ترتیبی را اجرا می‌کند. selector/dispatcher generic هنوز compatibility سه-host/round و نه تلاش را برای callerهای مجاز دارد.
- بنابراین اجرای تاریخی سه-host از سیاست current first-release تبعیت نمی‌کند و نباید به‌عنوان پذیرش current production multi-host عرضه شود.

## تصمیم

status پایهٔ `done` حفظ شد، اما `re_review_required` است تا UPD-S1-003/015 تصمیم نهایی تعداد میزبان را قطعی کند و آزمون‌های current-source متناسب با همان policy دوباره اجرا شوند. T0202 فیزیکی نیز مستقل و deferred_final باقی می‌ماند. هیچ تغییر پنهانی در گراف اولیه انجام نشد.

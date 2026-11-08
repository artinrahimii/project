# بازبینی T0200 — Android JVM و میزبان .NET محلی از طریق libp2p

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70`، `V 3.1.102`
- اجرای تاریخی suite: `a074815979aff371650c5101c514989f42180968`، `V 3.0.922`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وضعیت reconciliation: `reviewed`
- UPDهای مرتبط: UPD-S1-004، UPD-S1-010، UPD-S1-019، UPD-S1-020

## کد و آزمون فعلی

- `Libp2pInferenceRequestTransportTest.kt` در source فعلی آزمون authenticated Noise TCP، پاسخ correlate شده، CancelRequest به fixture .NET، انتخاب یک میزبان از سه candidate و انتخاب peer متفاوت پس از timeout peer قبلی را دارد.
- `AndroidInteropPeerServer.cs` یک فرایند .NET با هویت مستقل libp2p و trust صریح ایجاد می‌کند؛ در حالت پیش‌فرض روی loopback گوش می‌دهد و fixture پاسخ آزمون ثابت می‌دهد، نه inference model واقعی.
- `product-build.yml` ابتدا fixture .NET را build می‌کند، سپس DLL را در اختیار Android JVM tests می‌گذارد؛ این مسیر cross-runtime را واقعاً اجرا می‌کند، اما نصب جداگانهٔ Windows محصول نیست.
- گزارش قدیمی T0200 دو تست سه-peer را روی source قدیمی ثبت کرده بود. نام آن دو تست در transport فعلی وجود ندارد؛ مسیر فعلی one-host production و native .NET cancellation/response را نگه می‌دارد.

## شواهد و محدودیت

- Product Build تاریخی run [41886746018](https://github.com/artinrahimii/project/actions/runs/41886746018)، Android job 126773137271، موفق گزارش شده است.
- روی HEAD فعلی، Product Build run 42515003800 هنگام آخرین مشاهده هنوز `in_progress` بود؛ نتیجهٔ آن به‌عنوان pass استفاده نشده است.
- fixture یک endpoint loopback در یک CI runner است و prompt/response ثابت تستی می‌دهد. این آزمون Android فیزیکی به Windows نصب‌شده، شبکهٔ LAN یا مدل محلی واقعی نیست.
- اجرای محلی در این محیط انجام نشد.

## تصمیم

وضعیت قدیمی `done` حفظ می‌شود؛ `re_review_required` ثبت شده، زیرا سیاست multi-host از زمان تست تاریخی تغییر کرده و اتصال Conversation/Room هنوز از این fixture اثبات نمی‌شود. اجرای current-source CI و E2E فیزیکی باید قبل از بازاعتبارسنجی نهایی بررسی شوند.

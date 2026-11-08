# بازبینی T0205 — توقف پردازش میزبان پس از cancellation

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916`، `V 3.1.104`
- پیاده‌سازی تاریخی: `a3e50e03f105e54ad01a0d97d04b9436ebcb6dbd`، `V 3.0.940`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- UPDهای مرتبط: UPD-S1-019، UPD-S1-020

## مسیر آزمون بررسی‌شده

- `PeerAnnouncementClientAcceptanceTests.PeerHostLifecyclePublishesAndServesTrustedPeerAsync` از اجزای production شامل `PeerAnnouncementHostLifecycle`, `HostRequestCoordinator`, `HostInferenceSession` و `LlamaCppServerBackend` استفاده می‌کند.
- handler HTTP آزمون پس از health probe، درخواست POST generation را باز نگه می‌دارد؛ سپس peer احرازشده `CancelRequest` هم‌بسته با request/attempt می‌فرستد.
- آزمون منتظر مشاهدهٔ cancellation و پایان HTTP operation می‌ماند، پاسخ `CANCELLED` بدون output را بررسی می‌کند و آزادشدن single-request gate را assert می‌کند.
- Product Build run [41915992536](https://github.com/artinrahimii/project/actions/runs/41915992536)، Host Lifecycle job 126875674392، موفق بود؛ Quality Gate run 41915992559 نیز موفق بود.

## محدودیت صریح

backend adapter با HTTP cancellation متوقف می‌شود، اما llama.cpp یک process جداست و این تست اثبات نمی‌کند که محاسبات CPU/GPU داخل process سرور در همان لحظه متوقف شده‌اند. گزارش اصلی نیز این محدودیت را به‌درستی اعلام کرده است.

status پایه `done` حفظ شد؛ به‌دلیل نیاز UPD-S1-019/020 به revalidation لغو در چرخهٔ نهایی، تصمیم `re_review_required` باقی می‌ماند.

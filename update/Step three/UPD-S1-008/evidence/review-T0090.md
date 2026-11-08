# بازبینی محتوایی T0090 — ترتیب قطعی پیام‌های مکالمهٔ میزبان

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `08ba01194adc2acc37593a2586eb79104c7dd650` — `V 3.1.81`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-010`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Application/Chat/ChatConversation.cs` — `e7b47a387007811af779686aa2dd368c2eed687e`
- `src/Host.Application/Chat/HostConversationRetentionPolicy.cs` — `14fa01b205a276835a806dba492a1a3fe1d517f0`
- `src/Host.Windows/Chat/WindowsHostConversationStore.cs` — `7c6a1476a033d611806f931c9839ed6218ae3004`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs` — `26c68cb8bf34fedefc5453d746cbe0cbc86d53be`

## یافته‌ها
- ChatConversation پیام‌های user/assistant/system را به ترتیب در حافظه نگه می‌دارد؛ MainWindow مسیر append مرکزی دارد تا پاسخ پس از پیام کاربر درج شود.
- HostConversationRetentionPolicy نگهداری را محدود می‌کند؛ WindowsHostConversationStore رمزگذاری DPAPI و نگهداری اختیاری با رضایت مالک را پیاده می‌کند. این history محلی Windows است، نه Room اندروید.
- Acceptance tests ترتیب role/content، شناسه و timestamp پیام، clear و retention/restore را بررسی می‌کنند. Historic quality run 40181923409 و product run 40181923441 موفق بوده‌اند؛ یک source assertion قدیمی در تاریخچه حفظ و بعداً اصلاح شده است.
- یال‌های T0089→T0090 و T0090→T0091 از نوع contract_or_interface_dependency/validated و T0087→T0090 از نوع sequencing_only/validated هستند.

## علت بازبینی مجدد
- این component مکالمهٔ in-memory میزبان است و persistence اختیاری DPAPI جایگزین Room اندروید نیست. UPD-S1-010 باید ترتیب و persistence همان چت Android را تضمین کند؛ مصرف‌کنندگان پس از اتصال runtime جدید دوباره بررسی شوند.

## یال‌های گراف قدیمی که حفظ شدند
- T0087 → T0090: sequencing_only/validated
- T0089 → T0090: contract_or_interface_dependency/validated
- T0090 → T0091: contract_or_interface_dependency/validated
- هیچ یال، status قدیمی یا تاریخچه‌ای در این بازبینی تغییر نکرد.

## شواهد CI دقیق V3.1.81
- Quality Gate: [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) — passed.
- Rendezvous container build/smoke: [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) — passed.
- HostLifecycle job: [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) — passed; 67 PASS markers, one opt-in real-model SKIP, 85.88% line coverage.
- Windows/Android Product Build jobs were still in progress at review time.
- Historical task-specific runs are attributed to their original code revisions; no physical Android-to-Windows E2E, Room E2E, clean install or in-place upgrade was performed in this review.

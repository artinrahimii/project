# بازبینی T0027 — دستورهای تکرارپذیر README

- commit بررسی: `ebede06d9584d17e8277ccf811770e5fe4b40161` — `V 3.1.69`
- وضعیت پایه و فعلی: `done`؛ تصمیم `re_review_required`؛ وضعیت پایه تغییر نکرده است.
- پیش‌نیازهای `T0026` و `T0020` هر دو در گراف قدیمی `done` هستند.
- فرمان‌های .NET restore/build/publish، Android lint/assembleDebug و اجرای آزمون‌های Python در README با `tests/test_readme_build_commands.py` تطبیق داده شدند.
- `tests/test_reproducible_build_contract.py` نسخهٔ ابزارها، clean checkout، خروجی APK، installer و launch smoke را کنترل می‌کند؛ `.github/workflows/product-build.yml` و `quality.yml` مصرف‌کنندگان واقعی همین دستورها هستند.
- Product build run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) در V3.1.61 سه job Windows/Android/HostLifecycle را موفق تکمیل کرد؛ Quality run دقیق V3.1.69 [42484597069](https://github.com/artinrahimii/project/actions/runs/42484597069) نیز موفق شد.
- این شواهد build/اجرای runner را اثبات می‌کنند، نه نصب فیزیکی یا ارتقای نسخه در سیستم کاربر.
- تصمیم `re_review_required` به‌دلیل تغییرات آتی راهنمای نصب، package، checksum و upgrade در `UPD-S1-021` است. پس از آن وظیفه باید README و تمام مصرف‌کنندگان دوباره بررسی شوند.

# شواهد هم‌ترازی UPD-S1-006

## خط مبنا و تمامیت منابع
- commit آنلاین `main`: `ed3ebe09f8555f247a23a7abc67d7234a06e45d2` — `V 3.1.44`.
- گراف به‌روزرسانی: ۲۴ گره، ۸۰ یال، ۱۳ موج؛ نیازمندی‌های مصوب V1.10 دارای ۲۴ وظیفه است.
- Blob SHAهای منابع دریافت‌شده از همان commit:
  - `docs/requirements.md`: `b08c49dff3126dc2955ee9840a916b4630cd0523`
  - `docs/acceptance.md`: `507fd0ec2d225cd9db15b75af6e76f4425e11356`
  - `step one/V1.0.md`: `25fa14843b10ca972796aa17a23ef26582011a20`
  - `update/Step two/project-graph.md`: `60018d9c9907bbb1f0902160517602e32b7c5c63`
  - `update/Step one/Output/V1.10.json`: `5cea3ec095d0c00e32ed3e067796ff698aedd982`
  - `tests/test_acceptance_contract.py`: `0048876287d60ee8d6229311144989cefd2d4ea1`

## تصمیم و نگاشت
- P2P مستقیم و rendezvous metadata-only: ADR-0002.
- میزبان واحد/تلاش ترتیبی و retry دستی: ADR-0022.
- پاسخ کامل terminal و defer streaming: ADR-0023.
- readiness، opt-in و ظرفیت قابل‌اندازه‌گیری: خروجی UPD-S1-005 و ADR-0012.
- هویت بصری: شیء `visual_identity` در V1.10؛ پنج token، یک منبع برداری و آستانه‌های کنتراست.

## حفظ وظایف قدیمی
- تعداد checkboxهای task در V1.0: ۲۲۹.
- تعداد شناسهٔ یکتا: ۲۲۹؛ شناسهٔ تکراری: صفر.
- SHA-1 فهرست مرتب‌شدهٔ شناسه‌ها با newline: `75c65d66c98bd5bd295617fde5098d7ccdff9059`.
- هیچ `Txxxx` جدیدی ساخته نشده است.

## تفکیک سند از پیاده‌سازی
این شواهد فقط انتقال تصمیم‌های مصوب به سند نیازمندی و پذیرش را ثبت می‌کند. حضور REQ/AC یا تعریف palette/icon به‌تنهایی اثبات اتصال Android chat به runtime، اعمال theme در دو برنامه، build انتشار، آزمون دستگاه یا پذیرش محصول نیست.


## کنترل مقادیر دقیق در جدول acceptance

ردیف AC-VISUAL-01 نام و HEX هر پنج توکن را به‌صورت مستقیم ثبت می‌کند و آزمون `test_update_step_one_alignment.py` تمام این جفت‌ها و آستانه‌های 4.5:1 و 3:1 را از همان ردیف کنترل می‌کند. این فقط انطباق نیازمندی/معیار را اثبات می‌کند، نه اعمال واقعی theme یا صحت screenshotهای Android/Windows.

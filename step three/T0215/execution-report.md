# گزارش اجرای T0215

## هدف
گنجاندن فهرست وابستگی‌ها، متن مجوزها و اعلامیهٔ مجوز مدل نمونه در بستهٔ نصب ویندوز و در بستهٔ ZIP انتشار.

## بررسی یال‌ها
- T0214 → T0215: نوع اصلی blocks حفظ شد؛ طبقه‌بندی contract_or_interface_dependency و validation_status=validated ثبت شد. T0215 قرارداد artifact ویندوز تولیدشده در T0214 (مسیر installer و پوشهٔ windows-installer/licenses) را مصرف می‌کند و خروجی مجوز مدل را به همان قرارداد بسته‌بندی اضافه می‌کند.
- T0208 → T0215: نوع اصلی blocks حفظ شد؛ طبقه‌بندی sequencing_only و validation_status=validated ثبت شد. T0215 گزارش‌های coverage و critical-test تولیدشده در T0208 را ورودی فنی خود نمی‌گیرد.
- دلیل و شواهد هر دو یال در edge_change_log ثبت شده‌اند؛ ترتیب محافظه‌کارانه تغییر نکرد.

## تلاش T0215-A001 — پیاده‌سازی اولیه
- مولد fail-closed برای notice مدل نمونه و manifest JSON اضافه شد.
- متن کامل Apache-2.0 از URL رسمی دریافت و markerها، اندازه و UTF-8 آن اعتبارسنجی می‌شود؛ SHA-256 متن و catalog ثبت می‌شود.
- فقط license با status=verified و identifier=Apache-2.0 پذیرفته می‌شود؛ license نامشخص یا identifier پشتیبانی‌نشده build را متوقف می‌کند.
- classification که در catalog نیامده unknown باقی می‌ماند؛ هیچ ادعای open_source از روی identifier استنتاج نمی‌شود.
- وزن‌های مدل داخل نصب‌کننده یا release bundle قرار نمی‌گیرند؛ notice فقط metadata، checksum مورد انتظار و متن مجوز را بسته‌بندی می‌کند.
- اسکریپت ساخت نصب‌کننده، نصب آزمایشی، Quality Gate و release-assets workflow برای وجود notice، manifest و متن کامل مجوز به‌روزرسانی شدند.
- آزمون محلی در این محیط اجرا نشده است. نتیجهٔ نهایی منوط به Quality Gate و Product Build روی commit پس از این تغییر است.

## اقدام بعدی
بررسی CI همان commit، اجرای آزمون‌های جدید مولد مجوز، ساخت واقعی installer و بررسی وجود سه فایل مجوز در Artifact ویندوز. تا دریافت این شواهد، وضعیت وظیفه in_progress باقی می‌ماند.

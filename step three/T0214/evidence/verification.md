# شواهد و راستی‌آزمایی T0214

## مبنا
- شاخه: main
- commit بررسی‌شده: 143e743f179f7c6dee8a9a85cb3dca4c0a9a7ea6 (V 3.0.995)
- گراف از Git Blob همان commit با شناسه c5ae6d9dbd3ebe958dcf9a20bfefc9b3d16f77ce دریافت شد. اندازهٔ tree: 3894366 بایت؛ طول متن دریافت‌شده: 3764410 نویسه؛ محتوای JSON کامل parse شد. محاسبهٔ مجدد SHA-1 در محیط ابزار ممکن نبود؛ ادعای hash محاسبه‌شدهٔ مستقل ثبت نمی‌شود.

## اجرای واقعی در GitHub Actions
- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42260616384 — موفق؛ 438 آزمون و همهٔ مراحل کنترل کیفیت موفق.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42260616427 — موفق؛ هر سه job موفق.
- Windows publish: بررسی اجرایی وجود فایل WhiteP.Host.Windows.exe و 66 فایل در پوشه را گزارش کرد. Artifact whitep-windows-publish، ID 13524323518، اندازهٔ ZIP برابر 10019202 بایت.
- Windows installer build/install/smoke/uninstall نیز در همان Product Build موفق بود؛ این بخش شواهد جانبی بسته‌بندی است.
- Android debug APK: Artifact ID 13525267634، اندازهٔ 28972476 بایت؛ instrumentation روی emulator و launch واقعی MainActivity موفق.
- HostLifecycle: فرمان دقیق مستندشده از clean checkout موفق؛ 85.7% line coverage، 8849/10325 lines و 67 PASS markers در log پوشش‌دار.

## حدود شواهد
این اجرای CI ساخت و آزمون platformها را ثابت می‌کند؛ ZIP مربوط به whitep-windows-publish در این بررسی جداگانه download/extract نشده است. همچنین اجرای واقعی روی دو رایانهٔ مستقل، اتصال اینترنت عمومی، Rendezvous عمومی و استنتاج با مدل دریافت‌شده را ثابت نمی‌کند.

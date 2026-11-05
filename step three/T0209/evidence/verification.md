# شواهد راستی‌آزمایی T0209

- شاخهٔ هدف: main
- commit مبنا: 5596ba37a03542277e980ba8a8397f310c263be5 (V 3.0.952)
- پیش‌نیاز T0208: گزارش و شواهد CI همان commit بررسی شد؛ Windows، Android و HostLifecycle موفق بوده‌اند.
- یال T0208 به T0209: sequencing_only و validated حفظ شد؛ هیچ یالی تغییر نکرد.

فایل‌های پیاده‌سازی:
- tools/installer/WhiteP.iss
- tools/build_windows_installer.ps1
- tools/generate_dependency_licenses.py
- tests/test_generate_dependency_licenses.py
- .github/workflows/product-build.yml
- docs/windows-installer.md

موارد پذیرش که CI باید اثبات کند:
1. publish واقعی self-contained برای win-x64 موفق شود.
2. نسخه و ناشر executable و installer به‌ترتیب 0.1.0 و Artin Rahimi باشند.
3. تمام بسته‌های NuGet restore شده inventory مجوز داشته باشند و اطلاعات مفقود باعث fail شود.
4. installer واقعی ساخته شود و metadata نسخه/ناشر داشته باشد.
5. نصب بی‌صدا، وجود executable و ThirdPartyNotices، smoke launch و uninstall موفق شوند.
6. artifact با نام whitep-windows-installer در Product Build بارگذاری شود.

در این checkpoint موارد فوق روی runner ویندوز اجرا نشده‌اند؛ وضعیت done یا انتشار Release ثبت نمی‌شود. installer فاقد امضای Authenticode است.

 
## A002 — نتایج CI و ریشه‌یابی مجوز

- Step Three Quality Gate run 41997150945: ۴۱۵ مورد از ۴۱۶ آزمون موفق؛ یک assertion قدیمی تعداد upload-artifactها را ۶ انتظار داشت درحالی‌که تعداد واقعی با artifact installer برابر ۷ بود.
- Product Build run 41997150911: clean WPF build، smoke startup، Inno Setup installation و تست‌های unit مجوز موفق؛ publish self-contained هم موفق. مرحلهٔ inventory به‌دلیل نبود license metadata برای Collections.Pooled 1.0.82 متوقف شد.
- منبع upstream: https://github.com/jtmueller/Collections.Pooled/blob/master/LICENSE.txt؛ GitHub repository آن را MIT معرفی می‌کند. CI اکنون revision کامل upstream را resolve می‌کند و فایل LICENSE را از همان revision دریافت می‌کند؛ markerهای MIT و صاحب حق‌نشر بررسی و SHA-256 در inventory ثبت می‌شود.
- نتیجهٔ اصلاح و CI بعدی هنوز باید تأیید شود.


## A003 — نتایج CI

- Step Three Quality Gate run 42001661879: موفق؛ ۴۱۸ آزمون OK، syntax check، graph validation و evidence validation موفق.
- Rendezvous run 42001661910: build image و non-root HTTP contract موفق.
- HostLifecycle job در Product Build run 42001661942: موفق.
- Windows در همان Product Build: Inno Setup، چهار آزمون مجوز، clean build، publish self-contained، Protobuf، startup smoke، agent-tool tests و DPAPI tests موفق؛ inventory ابتدا Collections.Pooled 1.0.82 را با منبع MIT تأیید کرد و سپس به Common.Logging 3.4.1 رسید و به‌دلیل metadata ناقص fail کرد.

## A004 — تغییرات در انتظار CI

1. برای هر upstream repo، SHA مستقل و کامل Git ذخیره می‌شود.
2. Collections.Pooled 1.0.82 با MIT و Common.Logging/Core 3.4.1 با Apache-2.0 به‌صورت exact-version override ثبت شده‌اند.
3. licenses.json و ThirdPartyNotices.md برای هر فایل upstream، revision و SHA-256 درج می‌کنند.
4. missing-licenseهای باقی‌مانده یکجا نمایش داده می‌شوند؛ هیچ بسته‌ای silently skip نمی‌شود.
5. CI بعدی باید Quality Gate، unit tests، کامل بودن inventory، installer compilation، silent install، installed-app smoke، uninstall و artifact upload را اجرا و موفق کند.

در checkpoint فعلی، موارد ۵ تا ۷ هنوز اجرا/تأیید نشده‌اند؛ T0209 تکمیل‌شده نیست. installer بدون Authenticode است و artifact workflow به‌تنهایی GitHub Release محسوب نمی‌شود.

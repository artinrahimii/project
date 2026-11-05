# گزارش اجرای T0209

## هدف و پیش‌نیاز
وظیفهٔ T0209 تولید installer واقعی ویندوز، تعیین نسخه/ناشر و همراه‌کردن مجوز وابستگی‌هاست.
- T0208 با وضعیت done بررسی شد؛ Quality Gate و jobهای Windows، Android و HostLifecycle روی commit 34eaca4b3a0ba5055705241cca14894b896078d2 موفق بوده‌اند.
- یال T0208 به T0209 از قبل sequencing_only و validated بوده است؛ شواهد CI و گزارش T0208 بازبینی شد. یال و ترتیب محافظه‌کارانه حفظ شد.
- README تأیید می‌کرد build artifact فعلی خودش installer نیست؛ بنابراین بسته‌بندی مستقل از publish واقعی طراحی و پیاده‌سازی شد.

## پیاده‌سازی
- نسخهٔ میزبان و installer: 0.1.0، هم‌راستا با نسخهٔ اولیهٔ Android.
- ناشر در metadata: Artin Rahimi. این مقدار امضای دیجیتال یا هویت مورد اعتماد Windows نیست.
- خروجی self-contained برای win-x64 است تا Desktop Runtime جداگانه لازم نباشد.
- Inno Setup نصب per-user، Start Menu و میان‌بر اختیاری دسکتاپ تولید می‌کند.
- inventory مجوزها از project.assets.json مربوط به restore واقعی تولید می‌شود؛ نبود license expression/URL/file باعث توقف fail-closed می‌شود.
- build script installer را می‌سازد، metadata نسخه/ناشر را می‌سنجد و install، launch با --smoke-test و uninstall را روی runner ویندوز بررسی می‌کند.
- امضای Authenticode انجام نمی‌شود؛ گواهی/کلید محرمانه به مخزن افزوده نمی‌شود.

## فایل‌های تغییر‌یافته
- src/Host.Windows/WhiteP.Host.Windows.csproj
- tools/installer/WhiteP.iss
- tools/build_windows_installer.ps1
- tools/generate_dependency_licenses.py
- tests/test_generate_dependency_licenses.py
- .github/workflows/product-build.yml
- docs/windows-installer.md
- README.md
- .gitignore
- step three/T0209/evidence/verification.md
- step three/project-graph.md

## آزمون‌ها و نتیجه
- python -m unittest discover -s tests -p test_generate_dependency_licenses.py -v: دو آزمون موفق در محیط توسعه.
- python -m py_compile tools/generate_dependency_licenses.py: موفق.
- Windows dotnet publish، Inno Setup compilation، installer install/smoke/uninstall: در زمان ثبت این checkpoint هنوز توسط runner ویندوز اجرا نشده‌اند.
- بنابراین وضعیت T0209 برابر in_progress است؛ هیچ installer نهایی یا Release منتشرشده‌ای تا زمان دریافت artifact واقعی CI ادعا نمی‌شود.

## محدودیت
installer بدون امضای Authenticode است. artifact خروجی CI با انتشار GitHub Release متفاوت است و تا زمان وظایف بعدی آمادهٔ انتشار عمومی محسوب نمی‌شود.

 
## A002 — شکست CI و بررسی منبع مجوز

- Step Three Quality Gate run 41997150945 روی commit f09ddede93058b5a0789f177a00dbefb553448bf تعداد ۴۱۶ آزمون را اجرا کرد؛ ۴۱۵ مورد موفق و آزمون test_product_builds_publish_real_artifacts_and_fail_if_missing به‌دلیل انتظار ۶ artifact در برابر مقدار واقعی ۷ ناموفق شد.
- Product Build run 41997150911: نصب Inno Setup، آزمون‌های license inventory، build پاک WPF و startup smoke موفق شدند؛ سپس publish self-contained نیز موفق شد، اما جمع‌آوری مجوزها روی Collections.Pooled 1.0.82 به‌دلیل نداشتن metadata مجوز در nuspec/پوشهٔ بسته متوقف شد.
- منبع رسمی upstream، مجوز MIT و فایل LICENSE.txt را مشخص می‌کند؛ override فقط برای Collections.Pooled/1.0.82 اضافه شده است. فایل مجوز در زمان build از commit مشخص upstream دریافت می‌شود و SHA-256 و revision در inventory ثبت خواهد شد.
- اصلاح assertionهای artifact و آزمون‌های جدید override تا دریافت اجرای CI بعدی تأییدنشده‌اند.


## A003 — نتایج اجرای CI و اصلاح دوم مجوز

- Step Three Quality Gate run 42001661879 روی commit V 3.0.954 موفق شد: ۴۱۸ آزمون با نتیجهٔ OK؛ بررسی syntax، ساختار graph و شواهد اجرا نیز موفق بود.
- Rendezvous run 42001661910 موفق شد: build image تولیدی و HTTP contract با کاربر غیرریشه پاس شدند.
- HostLifecycle job در Product Build run 42001661942 موفق شد.
- در Windows job، Inno Setup و چهار آزمون license inventory موفق شدند؛ build پاک WPF و publish self-contained برای win-x64 بدون warning/error موفق شدند؛ تولید Protobuf، startup smoke، تست‌های agent-tool و DPAPI نیز موفق شدند.
- منبع upstream مجوز Collections.Pooled از revision 86cb27aa7ef64f52f169d0e2f7d512493ddc1b51 دریافت شد و markerهای MIT آن تأیید شدند. سپس inventory روی Common.Logging 3.4.1 به‌دلیل نبود metadata متوقف شد.

## A004 — اصلاح چند منبع مجوز upstream

- Generator برای هر override فایل revision مستقل می‌خواند؛ SHA کامل ۴۰ رقمی و markerهای محتوای منبع را کنترل می‌کند و SHA-256 فایل را ثبت می‌کند.
- overrideها فقط برای نسخه‌های دقیق Collections.Pooled 1.0.82، Common.Logging 3.4.1 و Common.Logging.Core 3.4.1 تعریف شده‌اند.
- دو بستهٔ Common.Logging به فایل license.txt پروژهٔ upstream و SPDX Apache-2.0 متصل شده‌اند؛ revision و digest منبع مشترک برای هر بسته در manifest درج می‌شود.
- generator تمام بسته‌های فاقد license metadata را در یک خطا جمع می‌کند و همچنان fail-closed باقی می‌ماند.
- آزمون‌های اختصاصی برای مجوزهای MIT و Apache، تطبیق نسخهٔ دقیق، کپی فایل، SHA-256 و تجمیع خطاها اضافه شده‌اند.
- A004 هنوز در GitHub Actions اجرا نشده است؛ بنابراین تعداد آزمون‌های جدید و موفقیت دریافت مجوز Common.Logging را هنوز تأییدشده حساب نمی‌کنیم.
- وضعیت T0209 همچنان in_progress است؛ installer compile، install، smoke، uninstall و upload واقعی artifact هنوز پاس نشده‌اند.


## A004 — شکست آزمون نسخهٔ متفاوت override

- Step Three Quality Gate run 42003164528 روی commit V 3.0.955: ۴۲۰ آزمون اجرا شد؛ فقط test_override_does_not_apply_to_a_different_package_version به خطا رسید و نتیجه FAILED (errors=1) شد.
- Product Build run 42003164759: Windows قبل از build و publish، در آزمون اختصاصی inventory متوقف شد؛ از ۶ آزمون، ۵ آزمون موفق و همان آزمون نسخهٔ متفاوت دارای error بود.
- علت دقیق: در generate، اعتبارسنجی overrideهای استفاده‌نشده قبل از گزارش MissingPackageLicenseError اجرا می‌شد؛ آزمون انتظار داشت بستهٔ 1.0.83 به دلیل نداشتن مجوز verified رد شود، اما ValueError مربوط به override بلااستفاده زودتر صادر می‌شد.
- هیچ installer جدیدی در این run ساخته یا نصب نشد؛ مرحلهٔ packaging اصلاً شروع نشد.

## A005 — اصلاح ترتیب اعتبارسنجی

- ابتدا بسته‌های restoreشدهٔ بدون مجوز verified گزارش و fail می‌شوند.
- تنها پس از نبود package missing، overrideهای بی‌مصرف/نسخه‌های منسوخ بررسی و رد می‌شوند.
- بنابراین نسخهٔ جدید همان fail-closed behavior را حفظ می‌کند و خطای اصلی بستهٔ بدون مجوز را با نوع و پیام قابل‌آزمون گزارش می‌دهد.
- A005 هنوز در CI اجرا نشده است.

# گزارش اجرای T0214

## هدف

اجرای دستورهای build، test، run و package مستندشده در README از checkout تمیز و ثبت خروجی‌های واقعی قابل دریافت.

## بررسی وابستگی‌ها

- یال `T0213 → T0214` از `blocks / unreviewed` به `blocks / sequencing_only / validated` تغییر طبقه‌بندی داده شد. T0214 فایل‌های APK، نصب‌کننده، manifest، provenance یا GitHub Release تولیدشده در T0213 را به‌عنوان ورودی build مصرف نمی‌کند؛ اجرای آن از clone تمیز و بر اساس سورس/دستورهای README انجام می‌شود. ترتیب محافظه‌کارانه و نوع اصلی `blocks` حفظ شدند.
- یال `T0208 → T0214` از `blocks / unreviewed` به `blocks / sequencing_only / validated` تغییر طبقه‌بندی داده شد. T0214 گزارش‌های Cobertura/HTML/JUnit/critical log منتشرشده در T0208 را به‌عنوان ورودی مصرف نمی‌کند؛ تست‌ها و خروجی‌های خودش را از checkout تازه تولید می‌کند. ترتیب و نوع اصلی یال حفظ شدند.
- هر دو تغییر همراه با دلیل و شواهد در `edge_change_log` ثبت شدند.

## شکاف پیش از اجرا

README دستور `dotnet publish ... --output artifacts/windows-host` را مستند کرده بود، اما Product Build آن را در CI اجرا نمی‌کرد. همچنین `dotnet restore WhiteP.sln` در job ویندوز به‌صورت صریح اجرا نمی‌شد. برای اینکه پذیرش بر مبنای دستور مستندشده باشد، این دو مرحله به job ویندوز اضافه شدند.
- خروجی publish با وجود `artifacts/windows-host/WhiteP.Host.Windows.exe` و غیرخالی بودن پوشه بررسی می‌شود.
- خروجی با نام `whitep-windows-publish` به‌عنوان Artifact مستقل نگهداری می‌شود.
- فرمان دقیق `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release` در job مستقل HostLifecycle از checkout تمیز اجرا می‌شود و سپس اجرای پوشش کد نیز حفظ می‌شود.
- قراردادهای آزمون README و reproducible build برای اجرای restore/publish و Artifact جدید به‌روز شدند.

## وضعیت تلاش A001

پیاده‌سازی و اصلاح مستندات/آزمون انجام شد و به مخزن ثبت می‌شود. در لحظهٔ ثبت، نتیجهٔ Quality Gate و Product Build مربوط به این commit هنوز دریافت نشده است؛ بنابراین هیچ فرمان یا artifact این commit را موفق اعلام نمی‌کنم و وضعیت T0214 در `in_progress` باقی می‌ماند.

## اقدام بعدی

اجرای Quality Gate، اجرای کامل سه job در Product Build، بررسی Artifact `whitep-windows-publish` و تأیید وجود فایل اجرایی/فایل‌های منتشرشده. فقط پس از موفقیت همهٔ کنترل‌های مرتبط و بررسی Artifact، T0214 به `done` تغییر می‌کند.

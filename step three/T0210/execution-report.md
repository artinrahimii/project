# گزارش اجرای T0210

## هدف و پیش‌نیازها
- اجرای برنامهٔ نصب‌شده روی Windows پشتیبانی‌شده از محیط تمیز.
- T0208 پیش‌تر done بود.
- T0209 در Product Build run 42007819870 روی commit eb8c7644317dfde643b2526b92e2300483d625ea بستهٔ واقعی ویندوز را ساخت، نصب کرد و artifact آن را بارگذاری کرد.
- یال T0209 -> T0210 وابستگی فنی واقعی است؛ T0210 executable نصب‌شده از installer T0209 را مصرف می‌کند.
- یال T0208 -> T0210 ترتیب اجرایی صرف است؛ تست این وظیفه coverage artifact یا API اختصاصی T0208 را مصرف نمی‌کند. نوع اصلی blocks و ترتیب محافظه‌کارانه حفظ شد.

## اجرای واقعی
- Windows runner با checkout و build پاک استفاده شد.
- installer واقعی به‌صورت silent در یک مسیر نصب موقت و یکتا نصب شد.
- executable نصب‌شده و licenses/ThirdPartyNotices بررسی شدند.
- uninstall registration با DisplayName=White-P، InstallLocation دقیق و Publisher برابر Artin Rahimi تأیید شد.
- executable نصب‌شده با --smoke-test اجرا شد؛ MainWindow واقعی پس از StartupUri بارگذاری و visible شد و برنامه با exit code موفق خارج شد.
- silent uninstall موفق شد؛ پوشهٔ نصب و registry registration متناظر حذف شدند.

## معیارهای پذیرش
- اجرا روی Windows runner پاک: موفق
- executable و notices نصب‌شده: موفق
- Publisher registration: موفق
- MainWindow واقعی در حالت smoke: موفق
- uninstall و پاک‌سازی مسیر/registry: موفق
- شاهد: Product Build run 42007819870، Windows job 127178931985.

## محدودیت
Smoke test اجرای واقعی WPF و visible بودن MainWindow را تأیید می‌کند؛ جایگزین پذیرش دستی همهٔ تعاملات یا آزمون همهٔ نسخه‌ها/سخت‌افزارهای Windows نیست. GitHub Actions artifact ساخته شده، نه GitHub Release عمومی.

## نتیجه
T0210 با شواهد واقعی تکمیل شد و از همان اجرای انتها‌به‌انتهای نصب T0209 استفاده کرد.

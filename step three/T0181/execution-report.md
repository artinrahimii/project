# T0181 — نگهداری محلی اختیاری میزبان با رضایت روشن مالک

- **وضعیت این commit:** `in_progress`؛ منتظر اعتبارسنجی CI دقیق commit هستیم.
- **مخزن و شاخه:** `artinrahimii/project` / `main`
- **commit مبنا:** `cdd9282c4ee01abd6232a97dc7fd45565ecd38bb` — `V 3.0.837`
- **تلاش:** `T0181-A001`
- **زمان ثبت تلاش:** `2026-11-03T12:02:00Z`

## بررسی پیش‌نیازها و یال‌ها

پیش‌نیازهای `T0180` و `T0177` در گراف `done` بودند. خروجی‌ها و گزارش‌های آن‌ها بررسی شدند.

- `T0180 → T0181`: از `unreviewed` به `sequencing_only` اعتبارسنجی شد. پاک‌سازی بافرهای انتقال و decoder در T0180 توسط مسیر ذخیرهٔ محلی مکالمه مصرف نمی‌شود. نوع اصلی `blocks` و ترتیب اجرایی حفظ شد.
- `T0177 → T0181`: از `unreviewed` به `sequencing_only` اعتبارسنجی شد. انتخاب میزبان و بودجهٔ retry اندروید در نگهداری تاریخچهٔ محلی میزبان Windows مصرف نمی‌شوند. نوع اصلی `blocks` و ترتیب اجرایی حفظ شد.

دلیل و شواهد هر دو بازبینی در `edge_change_log` آمده است. سایر یال‌ها دست‌نخورده مانده‌اند.

## پیاده‌سازی واقعی

- پیش‌فرض `ProcessSessionOnly` و بدون ذخیرهٔ پایدار باقی ماند. سیاست ذخیره‌سازی پایدار فقط با رضایت روشن مالک فعال می‌شود.
- گزینهٔ جداگانه در رابط WPF افزوده شد. فعال‌سازی پنجرهٔ تأیید Yes/No دارد و ذخیرهٔ پیام‌های نشست جاری و پیام‌های بعدی، رمزنگاری DPAPI و محدودیت محافظت در برابر کد مخرب هم‌حساب را توضیح می‌دهد.
- تنظیمات میزبان فقط مقدار بولی `PersistHostConversation` را ذخیره می‌کند. تنظیمات قدیمی که این فیلد را ندارند، خاموش می‌مانند.
- تاریخچهٔ میزبان در فایل جداگانه زیر پروفایل محلی Windows و با DPAPI در دامنهٔ `CurrentUser` رمز می‌شود. payload plaintext و ciphertext در پایان عملیات صفر می‌شوند؛ نوشتن از فایل موقت رمز‌شده و جایگزینی اتمی استفاده می‌کند.
- خاموش‌کردن گزینه، تنظیم خاموش را ذخیره و سپس فایل تاریخچه را حذف می‌کند. پاک‌سازی نشست نیز فایل را حذف می‌کند. خطاهای خواندن/رمزگشایی fail-closed هستند و فایل خراب خودکار بازنویسی نمی‌شود.
- تنظیمات Android و Room تغییر نکردند؛ آن بخش در T0184 و ADR-0020 مستقل است.

## فایل‌های واقعی تغییر‌یافته

- `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`
- `src/Host.Application/Chat/ChatConversation.cs`
- `src/Host.Infrastructure/LocalHostSettingsStore.cs`
- `src/Host.Windows/Chat/WindowsHostConversationStore.cs`
- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/App.xaml.cs`
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `tests/test_wpf_entrypoint.py`
- `docs/security.md`

## آزمون و محدودیت

- آزمون Windows HostLifecycle برای رضایت مالک، round-trip رمز‌شدهٔ DPAPI، نبود متن prompt در فایل و رد فایل خراب بدون overwrite افزوده شد.
- آزمون settings برای ذخیرهٔ ترجیح و حفظ پیش‌فرض خاموش در فایل‌های قدیمی به‌روز شد.
- آزمون‌های Python برای پیش‌فرض خاموش، کنترل رضایت UI، ذخیرهٔ رمز‌شده و مرز ذخیره‌سازی Android به‌روز شدند.
- آزمون smoke WPF برای وجود کنترل رضایت در پنجرهٔ واقعی گسترش یافت.

در محیط فعلی checkout محلی و اجرای مستقیم .NET SDK / Windows WPF در دسترس نیست؛ بنابراین هنوز نتیجهٔ محلی برای این آزمون‌ها ثبت نمی‌شود. اجرای workflowهای دقیق SHA پس از push لازم است. تا دریافت شواهد CI، وضعیت وظیفه `in_progress` است.

## محدودیت امنیتی

DPAPI با دامنهٔ `CurrentUser` داده را به حساب ویندوز جاری پیوند می‌دهد، اما در برابر کد مخرب اجراشده با همان حساب، دسترسی مدیریتی/سیستمی، backup سیستم‌عامل، pagefile/swap، crash dump یا screenshots تضمین نمی‌دهد. این وظیفه معیار عملیاتی مستقل `AC-PRIV-01` را تأیید نمی‌کند.

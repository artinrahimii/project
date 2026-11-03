# T0181 — نگهداری محلی اختیاری میزبان با رضایت روشن مالک

وضعیت: **تکمیل‌شده و تأییدشده با CI**

مخزن/شاخه: `artinrahimii/project` / `main`

## نسخه‌ها و تلاش‌ها

- `V 3.0.838` — `0399100e6f2f63fbfa35bc8d178208608a920730`
- `V 3.0.839` — `659aebf46077d9b7cd9aaecc6ff12ed2b1da6a78`
- `V 3.0.840` — `aaedf55c54292f19f28de62cbf26ea73e976e782`

سه تلاش append-only ثبت شده‌اند؛ دو خطای میانی اصلاح شده‌اند.

## پیش‌نیازها و یال‌ها

پیش‌نیازهای `T0180` و `T0177` در گراف `done` بودند. خروجی‌ها و مصرف‌کنندگان واقعی آن‌ها بازبینی شدند.

- `T0180 → T0181`: پاک‌سازی بافرهای انتقال/decoder توسط ذخیره‌ساز تاریخچهٔ محلی Windows مصرف نمی‌شود.
- `T0177 → T0181`: selector و retry budget اندروید در پیاده‌سازی نگهداری مکالمهٔ میزبان Windows مصرف نمی‌شود.

طبقه‌بندی هر دو یال از `unreviewed` به `sequencing_only` تغییر یافت؛ نوع اصلی `type=blocks` و ترتیب محافظه‌کارانه حفظ شدند. دلیل و شواهد در `edge_change_log` ثبت شدند. سایر یال‌ها دست‌نخورده‌اند.

## پیاده‌سازی واقعی

- سیاست پیش‌فرض `ProcessSessionOnly` است؛ ذخیرهٔ پایدار خاموش می‌ماند مگر اینکه مالک آن را فعال و تأیید کند.
- گزینهٔ WPF با تأیید جداگانهٔ Yes/No مالک اضافه شد؛ متن تأیید دربارهٔ پیام‌های نشست جاری و آینده، DPAPI و محدودیت محافظت شفاف است.
- تنظیمات فقط Boolean `PersistHostConversation` را ذخیره می‌کنند. تنظیمات قدیمی فاقد این property به‌طور پیش‌فرض خاموش می‌مانند.
- `WindowsHostConversationStore` تاریخچه را در فایل جداگانهٔ پروفایل Windows و با DPAPI در دامنهٔ `CurrentUser` رمز می‌کند.
- نوشتن از فایل موقت رمز‌شده و جایگزینی اتمی استفاده می‌کند؛ اندازه و تعداد پیام‌ها محدود است؛ بافرهای plaintext/ciphertext در پایان عملیات صفر می‌شوند.
- فایل خراب/ناسازگار fail-closed است و خودکار overwrite نمی‌شود. خاموش‌کردن گزینه و Clear Session حذف فایل را اجرا می‌کنند.
- Room اندروید تغییر نکرد؛ نگهداری محلی Android در T0184 و ADR-0020 مستقل می‌ماند.

## فایل‌های خروجی واقعی

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

## شواهد CI نهایی — exact commit V 3.0.840

- Step Three quality gate — run `41633714717`: ۳۴۳ آزمون Python موفق، `compileall` موفق، graph guard برای ۲۳۰ وظیفه و ۴۲۲ یال با صفر خطا و یک هشدار metadata قدیمی T0001.
- Rendezvous container — run `41633714676`: ساخت image تولیدی و HTTP smoke با کاربر غیر root موفق.
- HostLifecycle — job `125909456334` در run `41633714928`: موفق. خروجی runtime صریحاً ثبت کرد: `PASS: in-memory conversation, explicit retention consent and Windows DPAPI local-history storage.`
- Windows WPF — job `125909456784`: قالب‌بندی C#، ساخت Release با صفر warning و صفر error، startup smoke، محدودیت‌های agent-tool و بررسی DPAPI جاری ویندوز موفق. Artifact میزبان: `13229267898`، ۱۴٬۲۷۴٬۹۰۳ بایت.
- Android — job `125909456733`: KtLint، ساخت APK، JVM unit tests، بررسی منابع Protobuf Java/Kotlin و بارگذاری artifactها موفق. APK artifact `13229473066`، ۱۹٬۵۳۱٬۹۸۲ بایت؛ گزارش آزمون artifact `13228684447`، ۵۱٬۲۲۹ بایت.

## شکست‌ها و اصلاحات

- `V 3.0.838`: یک assertion متنی نادرست از ۳۴۳ آزمون Python شکست خورد (۳۴۲ موفق). assertion در `V 3.0.839` با اعلان واقعی property هماهنگ شد.
- `V 3.0.839`: build ویندوز به‌دلیل نبود `using WhiteP.Host.Windows.Chat;` با CS0246 شکست خورد. import در `V 3.0.840` افزوده شد.
- هر دو شکست در `execution_history` حفظ شده‌اند؛ تمام workflowهای نهایی روی `V 3.0.840` موفق شدند.

## محدودیت امنیتی

DPAPI با دامنهٔ `CurrentUser` فایل را برای حساب جاری محافظت می‌کند، اما در برابر کد مخرب تحت همان حساب، administrator/SYSTEM، backup سیستم‌عامل، pagefile/swap، crash dump یا screenshot تضمین نمی‌دهد. این task به‌تنهایی پذیرش عملیاتی مستقل `AC-PRIV-01` را تأیید نمی‌کند.

وضعیت نهایی T0181 در گراف `done` است.

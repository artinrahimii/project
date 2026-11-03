# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ KtLint در تلاش‌های پی‌درپی خطاهای قالب‌بندی جدید آشکار کرده و اجرای کامل Android هنوز موفق نشده است.
- **آخرین commit بررسی‌شده:** `ed8cca851bae251b8309ced5c2d8f96c22e996bc` — `V 3.0.850`
- **تلاش‌ها:** A001 پیاده‌سازی، A002 شکست lint در main source، A003 شکست lint در test source، A004 اصلاح importهای آزمون، A005 اصلاح زنجیرهٔ builder (CI در انتظار)

## پیش‌نیازها و قرارداد

- T0182 و T0177 در گراف `done` هستند؛ یال‌های ورودی T0183 از قبل `sequencing_only` و `validated` هستند و نوع اصلی `blocks` و ترتیب اجرایی حفظ شده‌اند.
- یال `T0183 → T0184` از نوع `blocks` و طبقه‌بندی `contract_or_interface_dependency` با وضعیت `validated` است. T0184 باید رابط `ProtectedBlobProtector` را برای wrap کردن کلید دیتابیس Room مصرف کند؛ رمزگذاری خود Room در T0183 انجام نشده است.

## پیاده‌سازی موجود

- helper مشترک `ProtectedBlobProtector` و `AndroidKeyStoreProtectedBlobProtector` با AES-256-GCM، کلید non-exportable، nonce تصادفی ۹۶ بیتی، tag ۱۲۸ بیتی، AAD وابسته به alias و محدودیت اندازه ایجاد شده‌اند.
- ذخیره‌ساز هویت و trust از دو alias مجزا استفاده می‌کنند و در رکورد خراب یا کلید مفقود fail-closed هستند.
- پنج آزمون instrumentation برای provider واقعی AndroidKeyStore روی emulator API 36 اضافه شده‌اند.
- اصلاح‌های اخیر فقط قالب‌بندی Kotlin و ترتیب importهای آزمون را تغییر می‌دهند؛ قرارداد رمزنگاری عوض نشده است.

## نتایج CI و شکست‌ها

- `V 3.0.848`، run `41673696991`: KtLint در فایل اصلی Keystore هشت violation گزارش کرد؛ ساخت APK و instrumentation آن revision اجرا نشد.
- `V 3.0.849`، run `41675040018`: KtLint به `ktlintTestSourceSetCheck` رسید و دو فایل آزمون JVM را به دلیل ترتیب importها و خط خالی میان importها رد کرد.
- `V 3.0.850`، run `41675745738`: `ktlintMainSourceSetCheck` دو violation جدید/باقی‌مانده در زنجیرهٔ `KeyGenParameterSpec.Builder` گزارش کرد:
  - line 100: expected newline before `.Builder`
  - line 104: unexpected newline before `.setBlockModes`
  job پیش از APK و آزمون‌ها متوقف شد.
- `V 3.0.850`، run `41675744351`: Step Three quality gate موفق.
- در این run، HostLifecycle، WPF و container باید بر اساس status نهایی همان run گزارش شوند؛ این‌ها به‌تنهایی معیارهای پذیرش T0183 را تکمیل نمی‌کنند.

## اصلاح جاری A005

زنجیرهٔ Builder به قالب مورد انتظار KtLint تغییر یافته است: `KeyGenParameterSpec` در خط مستقل و `.Builder(` در خط بعد قرار می‌گیرد؛ اولین `.setBlockModes` کنار پرانتز بستهٔ سازنده باقی می‌ماند. رفتار رمزنگاری، کلید، nonce، AAD و قرارداد interface دست‌نخورده است.

## محدودیت پذیرش

تا موفقیت KtLint، ساخت APK، آزمون‌های JVM، connected instrumentation روی emulator، بررسی Protobuf و بارگذاری artifactها روی commit اصلاحی، وضعیت T0183 `in_progress` باقی می‌ماند. emulator نیز TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. حفاظت کلید Room و رمزگذاری دیتابیس در T0184 هستند.

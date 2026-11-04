# امنیت و مرز نگهداری محتوای درخواست

## سیاست پیش‌فرض میزبان ویندوز

در نسخهٔ فعلی، پردازشگر میزبان محتوای پیام را فقط تا زمان فعال بودن نشست برنامه در حافظهٔ فرایند جاری نگه می‌دارد. سیاست پیش‌فرض در کد با `HostConversationRetentionPolicy.Default` تعریف شده و دامنهٔ آن `ProcessSessionOnly` است؛ ذخیرهٔ پایدار متن درخواست و پاسخ در پایگاه داده یا فایل مکالمه بخشی از این مسیر نیست.

- `MainWindow` سیاست پیش‌فرض را هنگام ساخت `ChatConversation` به‌صورت صریح تزریق می‌کند.
- `ChatConversation` پیام‌ها را در یک `List<ConversationMessage>` داخل حافظه نگه می‌دارد؛ دکمهٔ پاک‌سازی نشست، همین مجموعه و نمای مکالمه را پاک می‌کند.
- `LocalHostSettingsStore` فقط نشانی موتور محلی، شناسهٔ مدل، مهلت درخواست و مسیرهای اجرایی/مدل را در تنظیمات محلی نگه می‌دارد؛ متن درخواست، متن پاسخ و capsule از فیلدهای سند تنظیمات نیستند.
- آزمون میزبان، دامنهٔ سیاست، غیرفعال بودن ذخیرهٔ پایدار و شروع خالی یک نمونهٔ مستقل از مکالمه را کنترل می‌کند. آزمون منبع پایتون نیز از اضافه‌شدن API ذخیره‌سازی به کلاس مکالمه و ورود فیلدهای محتوا به تنظیمات جلوگیری می‌کند.

این سیاست فقط مرز ذخیره‌سازی تحت کنترل برنامهٔ میزبان را توصیف می‌کند. به معنی پاک‌سازی امن یا صفرسازی حافظه، کنترل pagefile/swap، جلوگیری از crash dump، یا کنترل ابزارهای تشخیصی و فرایندهای خارجی نیست.

## میزبان یک نقطهٔ اعتماد است

میزبان پردازشگر پس از رمزگشایی کانال ارتباطی می‌تواند `InferenceRequest.prompt` و بایت‌های `InferenceRequest.context_capsule` را بخواند. رمزگذاری کانال انتقال، محتوا را از خود میزبان مقصد پنهان نمی‌کند. پیش از ارسال، فقط به میزبانی اعتماد کنید که حاضر هستید محتوا را در اختیارش بگذارید. سیاست نگهداری نشست به‌تنهایی تضمین نمی‌کند که سیستم‌عامل، ابزار خارجی یا یک نسخهٔ تغییریافتهٔ برنامه محتوا را نگه ندارد. جزئیات این مرز در [ADR-0019](decisions/ADR-0019-host-visible-inference-content.md) ثبت شده است.

## ذخیره‌سازی Android جدا از سیاست میزبان است

برنامهٔ Android عمداً سابقهٔ مکالمه را در Room روی دستگاه کاربر ذخیره می‌کند. این رفتار ذخیره‌سازی محلی سمت کاربر است و نباید با ذخیره‌سازی پایدار در میزبان ویندوز یا سرویس rendezvous یکی گرفته شود. [ADR-0020](decisions/ADR-0020-android-client-local-conversation-storage.md) مرز این ذخیره‌سازی محلی را شرح می‌دهد. بنابراین این سند ادعای «هیچ محتوایی در هیچ دستگاهی ذخیره نمی‌شود» مطرح نمی‌کند.

## لاگ تشخیصی ساختاریافته با allowlist ثابت (T0179)

مسیرهای تشخیصیِ افزوده‌شده در میزبان ویندوز و کلاینت Android فقط چهار فیلد ثابت را منتشر می‌کنند:

- `schema`: نسخهٔ قالب رکورد؛
- `event_code`: کد عددی رویداد از enum ثابت؛
- `outcome_code`: کد عددی نتیجه از enum ثابت؛
- `duration_ms`: مدت‌زمان عددی عملیات برحسب میلی‌ثانیه.

در .NET، رویدادها از `WhiteP-SafeDiagnostics` و در Android از tag ثابت `WhiteP.SafeDiagnostics` منتشر می‌شوند. هر دو مسیر حداکثر ۸۰ رکورد در هر فرایند می‌پذیرند. API ثبت رویداد فقط enumهای ثابت و مدت‌زمان عددی می‌گیرد؛ بنابراین فراخوان نمی‌تواند متن آزاد، شناسه یا محتوای درخواست را به آن تزریق کند.

این allowlist عمداً شامل متن درخواست یا پاسخ، capsule، کلید و اسرار رمزنگاری، شناسه‌های درخواست/تلاش/دور/همتا/مدل، نشانی شبکه، قالب پیام دلخواه، آرگومان‌های ساختاریافته یا متن exception نیست. diagnostic fixture آزمون بین‌سکویی نیز فقط سطح عددی و شناسهٔ عددی رویداد را چاپ می‌کند؛ قالب پیام و مقادیر آرگومان‌های کتابخانه را render نمی‌کند.

این کنترل، رفتار logهای تحت کنترل همین دو بخش برنامه را محدود می‌کند؛ اثبات حذف logهای سیستم‌عامل، pagefile/swap، crash dump، diagnosticهای فرایندهای خارجی یا log داخلی کتابخانه‌های ثالث نیست و ادعای ناشناس‌بودن کامل شبکه مطرح نمی‌کند. آزمون‌های منبع Python، آزمون ساختاری EventSource در میزبان و آزمون قالب JSON در Android این قرارداد را محافظت می‌کنند.

## پاک‌سازی payload و بافرهای موقت تحت مالکیت برنامه (T0180)

- میزبان Windows پس از اتمام یا شکست ارسال، آرایهٔ frame ساخته‌شده برای wire را با `CryptographicOperations.ZeroMemory` صفر می‌کند. اگر کپی پیام ورودی پیش از انتقال موفق به صف شکست بخورد، همان کپی پاک می‌شود؛ هنگام disposal نیز پیام‌های باقی‌مانده در صف bounded خوانده و صفر می‌شوند.
- چرخهٔ دریافت میزبان، آرایهٔ request را پس از پایان handler یا در شاخه‌ای که مالکیت به handler منتقل نشده است پاک می‌کند. پاسخ handler نیز در `finally` پاک می‌شود. پیام‌های Protobuf کوتاه‌عمر برای پاسخ وضعیت، رد درخواست یا busy از helper ارسال/پاک‌سازی عبور می‌کنند.
- کلاینت Android آرایهٔ سریال‌شدهٔ درخواست، status و cancel را در `finally` پاک می‌کند؛ frame خروجی پس از کامل‌شدن یا شکست Promise نوشتن پاک می‌شود. decoder هنگام جایگزینی بافر ناقص، خطای frame و بسته‌شدن stream نسخهٔ قدیمی و بافرهای موقت خود را صفر می‌کند. تمام frameهای کامل تحویل‌شده نیز حتی در صورت شکست callback پاک می‌شوند.

این کنترل فقط کپی‌های بایتی تحت مالکیت مستقیم این مسیرهای برنامه را پوشش می‌دهد؛ صفرشدن objectهای immutable پروتوباف/رشته‌ها، کپی‌های احتمالی JIT/GC، حافظهٔ داخلی Netty/libp2p یا سیستم‌عامل، pagefile/swap و crash dump را اثبات نمی‌کند. frame اندروید تا پایان/شکست Promise نوشتن زنده می‌ماند تا بافر در میانهٔ ارسال تغییر نکند. ادعای پاک‌سازی مطلق حافظه یا حذف نسخه‌های خارج از مالکیت برنامه مطرح نمی‌شود.

آزمون‌های Android، صفرشدن backing array قبلی و ناقص decoder را بررسی می‌کنند؛ آزمون‌های Python نیز پاک‌سازی در مرزهای مالکیت صف، request lifecycle و stream controller را محافظت می‌کنند. CI دقیق commit باید پیش از تکمیل T0180 موفق شود.

## نگهداری محلی اختیاری میزبان با رضایت مالک (T0181)

سیاست پیش‌فرض همچنان `ProcessSessionOnly` است و گزینهٔ نگهداری محلی به‌صورت پیش‌فرض خاموش می‌ماند. صاحب رایانه فقط با فعال‌کردن گزینه در رابط میزبان و تأیید جداگانهٔ پنجرهٔ رضایت می‌تواند ذخیرهٔ تاریخچهٔ مکالمه را فعال کند. متن تأیید به‌روشنی توضیح می‌دهد که پیام‌های موجود در نشست و پیام‌های بعدی ذخیره خواهند شد.

- ترجیح ذخیره‌سازی فقط به شکل یک مقدار بولی `PersistHostConversation` در تنظیمات میزبان ثبت می‌شود؛ متن prompt، پاسخ یا capsule در فایل تنظیمات قرار نمی‌گیرد. تنظیمات قدیمی که این فیلد را ندارند، به‌طور پیش‌فرض خاموش باقی می‌مانند.
- در حالت فعال، تاریخچهٔ میزبان در فایل محلی زیر پروفایل کاربر ذخیره می‌شود و payload آن با Windows DPAPI در دامنهٔ `CurrentUser` محافظت می‌شود. فایل موقت فقط دادهٔ رمز‌شده را دریافت می‌کند و نوشتن نهایی به‌شکل اتمی جایگزین می‌شود.
- با خاموش‌کردن گزینه، ترجیح ابتدا ذخیره و سپس فایل تاریخچه حذف می‌شود. دکمهٔ پاک‌سازی نشست نیز فایل محلی را حذف می‌کند. اگر حذف فایل به‌علت خطای دسترسی انجام نشود، رابط موفقیت حذف را ادعا نمی‌کند و اعلام می‌کند که نسخهٔ رمز‌شده ممکن است باقی مانده باشد.
- خواندن فایل خراب، ناسازگار یا غیرقابل‌رمزگشایی با شکست بسته متوقف می‌شود؛ برنامه آن را به‌صورت خودکار بازنویسی نمی‌کند. اندازه و تعداد پیام‌ها نیز محدود شده‌اند و بافرهای plaintext و ciphertext در پایان عملیات صفر می‌شوند.

DPAPI از داده در برابر خواندن مستقیم فایل توسط کاربر یا حساب دیگری که DPAPI آن را نمی‌تواند رمزگشایی کند محافظت می‌کند، اما در برابر کد مخرب در حال اجرا با همان حساب ویندوز، دسترسی مدیریتی/سیستمی، نسخه‌های پشتیبان سیستم‌عامل، pagefile/swap، crash dump یا screenshots تضمین نمی‌دهد. این قابلیت صرفاً تاریخچهٔ میزبان Windows را پوشش می‌دهد و نه ذخیره‌سازی Room در Android.

## نگهداری اسرار و کلیدهای محلی Android با Android Keystore (T0183)

کلید خصوصی Ed25519 هویت libp2p کلاینت و فهرست PeerIdهای مورداعتماد، دو نوع دادهٔ متفاوت و دارای alias جداگانه دارند. هر دو از `AndroidKeyStoreProtectedBlobProtector` در مسیر `src/Client.Android/app/src/main/java/com/whitep/client/security/` استفاده می‌کنند:

- کلید wrapper برای هر purpose با Android Keystore، الگوریتم AES-256 و قابلیت‌های encrypt/decrypt ساخته می‌شود؛ کلید به‌صورت exportable در اختیار برنامه قرار نمی‌گیرد.
- داده با `AES/GCM/NoPadding` رمز می‌شود. envelope نسخه‌دار شامل یک بایت نسخه، nonce تصادفی ۹۶ بیتی و ciphertext همراه tag احراز اصالت ۱۲۸ بیتی است. نسخه و alias اختصاصی purpose نیز در AAD قرار می‌گیرند.
- alias هویت `com.whitep.client.peer_identity.v1` و alias اعتماد همتا `com.whitep.client.peer_trust.v1` مستقل‌اند. SharedPreferences فقط رشتهٔ Base64 مربوط به envelope رمز‌شده را نگه می‌دارد؛ نه کلید خصوصی خام و نه فهرست همتایان به شکل متن روشن.
- نوع entry، الگوریتم AES و غیرقابل‌استخراج بودن کلید بررسی می‌شود. اگر envelope خراب باشد یا کلید موجود برای رکورد قبلی حذف/غیرقابل‌دسترسی شود، خواندن fail-closed است؛ هویت قبلی بی‌صدا rotate نمی‌شود و تصمیم اعتماد نیز خودکار ساخته نمی‌شود.
- اندازهٔ دادهٔ محافظت‌شده محدود است. کپی‌های بایتی plaintext/ciphertext/nonce که این مسیر مستقیماً مالک آن‌هاست تا حد امکان صفر می‌شوند. این کار ادعای پاک‌سازی تمام کپی‌های VM/GC یا حافظهٔ سیستم‌عامل نیست.
- `allowBackup=false` در manifest باقی می‌ماند و این مسیر به مجوزهای حساس اضافی نیاز ندارد.

آزمون instrumentation روی Android API 36 emulator، provider واقعی Android Keystore را برای non-exportability، پایداری بین نمونه‌های store، جداسازی aliasها، حذف کلید و رد envelope دستکاری‌شده اجرا می‌کند. این آزمون، یکپارچگی provider روی emulator را نشان می‌دهد؛ سخت‌افزاری بودن کلید با TEE/StrongBox روی دستگاه فیزیکی را اثبات نمی‌کند. T0183 فقط کلیدهای peer identity و trust را با Keystore محافظت می‌کند؛ رمزگذاری پایگاه Room مستقل است و پیاده‌سازی SQLCipher در T0184 افزوده شده، اما تا موفقیت آزمون دقیق commit نباید آن را قابلیت پذیرفته‌شدهٔ محصول دانست.


## پاک‌سازی فایل‌های موقت یتیم میزبان (T0182)

هنگام ساخت `WindowsHostConversationStore` در آغاز میزبان، فایل‌های موقت هم‌مسیر تاریخچه بررسی می‌شوند. فقط فایل‌هایی کاندید حذف هستند که نام دقیق `.<history-file>.<32-hex-GUID>.tmp` داشته باشند و با نام همان فایل تاریخچه منطبق باشند. فایل‌های تازه‌تر از ۳۰ ثانیه برای جلوگیری از برخورد با فاصلهٔ کوتاه بستن فایل و rename نهایی دست‌نخورده می‌مانند. پیش از حذف، مسیر با `FileShare.None` به‌صورت انحصاری باز می‌شود؛ فایل قفل‌شده، غیرقابل‌دسترسی، reparse point، پوشه، نام نامعتبر و فایل متعلق به ذخیره‌ساز دیگر حذف نمی‌شود. خطای دسترسی یا فهرست‌کردن پوشه مانع راه‌اندازی میزبان نمی‌شود و فایل‌های باقی‌مانده در اسکن بعدی دوباره بررسی خواهند شد.

آزمون runtime، فایل یتیم قدیمی، فایل تازه، فایل قفل‌شده، نام GUID نامعتبر و نام متعلق به ذخیره‌ساز دیگر را پوشش می‌دهد. این سازوکار فقط temporary fileهای همین ذخیره‌ساز میزبان را هدف می‌گیرد؛ پاک‌سازی pagefile، crash dump، نسخهٔ پشتیبان سیستم‌عامل یا فایل‌های سایر اجزا را ادعا نمی‌کند.

## رمزگذاری پایگاه محلی Android با SQLCipher (T0184 — پذیرش runtime موفق در V 3.0.866)

Room اکنون از `EncryptedClientDatabaseFactory` و SQLCipher Community Edition استفاده می‌کند. پیاده‌سازی در commit `0cb7b121ba7d375f5779f0b0b1869b59f9890bfb` (`V 3.0.866`) با KtLint، ساخت APK، آزمون‌های JVM و آزمون‌های instrumentation متصل‌شده به emulator با موفقیت اعتبارسنجی شده است؛ گزارش کامل در [GitHub Actions run 41703869530](https://github.com/artinrahimii/project/actions/runs/41703869530) و artifact `13265621398` ثبت شده است.

- وابستگی‌ها SQLCipher نسخهٔ `4.20.0` و `androidx.sqlite:sqlite:2.7.0` هستند و Room 2 از `SupportOpenHelperFactory` استفاده می‌کند.
- برای هر فایل پایگاه، passphrase تصادفی ۲۵۶ بیتی تولید می‌شود. نسخهٔ خام آن در source، تنظیمات، گزارش یا SharedPreferences نوشته نمی‌شود؛ فقط envelope نسخه‌دار و احرازاصالت‌شدهٔ Android Keystore در preferences خصوصی اپ نگهداری می‌شود.
- alias کلید پایگاه `com.whitep.client.database_key.v1` از aliasهای identity و trust همتا جدا است. اگر فایل رمز‌شده وجود داشته باشد ولی کلید یا envelope بازیابی نشود، factory fail-closed می‌شود و فایل را با کلید تازه جایگزین نمی‌کند.
- مهاجرت فایل SQLite قدیمی با WAL checkpoint، export به فایل موقت رمز‌شده از طریق `sqlcipher_export()`، حفظ `user_version` و `application_id`، integrity check و سپس rename اتمی انجام می‌شود. فایل قدیمی تا پیش از موفقیت export و اعتبارسنجی مقصد جایگزین نمی‌شود.
- `allowBackup=false` در AndroidManifest حفظ شده است. این مسیر رمزگذاری فایل پایگاه در حالت سکون را هدف می‌گیرد؛ صفرسازی plaintext در حافظه، حذف فورنزیک بلوک‌های قدیمی فلش، تضمین TEE/StrongBox روی همهٔ دستگاه‌ها یا حفاظت در برابر دستگاه روت‌شده و فرایند مخرب هم‌حساب را ادعا نمی‌کند.
- چهار آزمون instrumentation اختصاصی T0184 در همان commit موفق شدند: `databaseKeyIsStoredOnlyAsKeystoreProtectedEnvelope`، `freshDatabaseIsEncryptedAndConversationDataSurvivesReopen`، `legacyPlaintextRoomDatabaseIsExportedAndExistingRowsArePreserved` و `missingProtectedKeyFailsClosedAndLeavesEncryptedDatabaseUnchanged`. artifact HTML مجموعاً ۹ تست، صفر شکست و صفر skipped نشان می‌دهد؛ پنج تست دیگر متعلق به T0183 هستند.

## کارهای خارج از دامنهٔ این سیاست
- پذیرش انتها‌به‌انتها و بازرسی مستقل شبکه/ذخیره‌سازی در `AC-PRIV-01` همچنان جداگانه لازم است؛ این وظیفه به‌تنهایی آن معیار عملیاتی نهایی را تأیید نمی‌کند.

## شواهد کد و آزمون

- سیاست: `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`
- نگهداری نشست در حافظه: `src/Host.Application/Chat/ChatConversation.cs`
- اتصال سیاست و رضایت UI به میزبان: `src/Host.Windows/MainWindow.xaml.cs` و `src/Host.Windows/MainWindow.xaml`
- ذخیرهٔ محلی رمز‌شده: `src/Host.Windows/Chat/WindowsHostConversationStore.cs` با `src/Host.Windows/Security/WindowsDpapiSecretProtector.cs`
- تنظیمات میزبان: `src/Host.Infrastructure/LocalHostSettingsStore.cs`
- آزمون runtime میزبان: `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs` (Windows DPAPI round-trip و رد فایل خراب)
- آزمون تنظیمات: `tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs`
- کنترل منبع و مستندات: `tests/test_host_conversation_retention_policy.py` و `tests/test_wpf_entrypoint.py`

این شواهد، سیاست ذخیره‌سازی تحت کنترل برنامه را توصیف می‌کنند؛ به‌تنهایی آزمون مستقل ترافیک شبکه، فایل‌های موقت، logها یا رفتار سیستم‌عامل نیستند.


## حداقل مجوزهای کلاینت Android (T0185 — تکمیل‌شده)

در Manifest منبع برنامه فقط android.permission.INTERNET درخواست می‌شود. این مجوز برای بازکردن سوکت‌های شبکه در کشف HTTPS سرویس rendezvous و ارتباط مستقیم TCP/libp2p لازم است. سطح حفاظت آن normal و از نوع install-time است؛ پنجرهٔ درخواست runtime ندارد.

در APK ادغام‌شده، AndroidX Core یک مورد خاص دیگر نیز قرار می‌دهد: com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION. این مجوز app-scoped با protectionLevel برابر signature تعریف می‌شود تا در نسخه‌های قدیمی‌تر Android، رفتار non-exported برای dynamic receiverها حفظ شود. فقط برنامه‌هایی که با گواهی یکسان امضا شده‌اند می‌توانند آن را دریافت کنند. این مورد را فقط به‌صورت دقیق و محدود می‌پذیریم؛ مجوز سفارشی دیگر یا سطح حفاظتی متفاوت مجاز نیست.

مجوز ACCESS_NETWORK_STATE اضافه نشده است: کد فعلی وضعیت اتصال یا شبکه‌های فعال را از سیستم‌عامل استعلام نمی‌کند. پایگاه Room در فضای خصوصی برنامه و با SQLCipher نگهداری می‌شود؛ دسترسی به فایل‌های مشترک، رسانه، مکان، دوربین، میکروفون یا بلوتوث در قابلیت‌های فعلی استفاده نمی‌شود. کد کلاینت نیز API درخواست مجوز runtime ندارد.

کنترل CI دو سطح را بررسی می‌کند:
- Manifest منبع باید فقط INTERNET را درخواست کند.
- AAPT2 روی APK نهایی باید دقیقاً INTERNET و مجوز app-scoped بالا را گزارش کند؛ سپس خروجی xmltree باید تعریف همان مجوز را دقیقاً یک بار با protectionLevel عددی 0x2 (signature) نشان دهد. parser هر دو قالب prefix کوتاه android: و URI کامل فضای نام Android را می‌خواند. هر مجوز اضافی، مجوز سفارشی دیگر، declaration مفقود/تکراری یا سطح حفاظتی متفاوت باعث شکست می‌شود.

دو شکست واقعی نسخه‌های V 3.0.868 و V 3.0.869 در execution history حفظ شده‌اند. اصلاح A005 هر دو قالب namespace را می‌پذیرد. در product run دقیق V 3.0.871، ممیزی AAPT2 روی APK واقعی، آزمون‌های Android JVM و instrumentation موفق شدند؛ artifact 13279121914 شامل ۹ آزمون، صفر شکست و صفر skipped است. product build، quality gate و آزمون container نسخهٔ V 3.0.872 نیز روی commit 6b507bba948743fe7da5cf3d4d4f9925601f24a7 موفق شدند. وضعیت T0185 در گراف done است.

## رضایت آگاهانه برای اعتماد و میزبانی همتا (T0186)

کلاینت Android پیش از ذخیرهٔ اعتماد به یک PeerId کشف‌شده، یک هشدار مختص همان همتا نمایش می‌دهد. متن هشدار روشن می‌کند که میزبان پردازش پس از رمزگشایی کانال می‌تواند prompt و context capsule را بخواند؛ رمزگذاری انتقال محتوا را از مقصد پنهان نمی‌کند و میزبان ممکن است محتوای دریافتی را بررسی یا نگهداری کند. تأیید این پنجره فقط اعتماد همتا را ذخیره می‌کند و هیچ prompt یا capsuleای ارسال نمی‌کند؛ ارسال تنها پس از اقدام جداگانهٔ کاربر با دکمهٔ Send to trusted hosts انجام می‌شود. اعتماد همتا بعداً قابل لغو است.

برنامهٔ میزبان Windows نیز هنگام فعال‌سازی میزبانی inference همتا، پیش از ساخت یا شروع PeerAnnouncementHostLifecycle، رضایت جداگانهٔ صاحب دستگاه را می‌گیرد. هشدار PeerId دقیق مقصد درخواست‌ها، امکان خواندن prompt و capsule در میزبان پس از رمزگشایی و محدودیت حفاظت رمزگذاری انتقال را توضیح می‌دهد. رد رضایت toggle را خاموش نگه می‌دارد و listener/lease ایجاد نمی‌شود. بعد از پذیرش حریم خصوصی، تأیید هویت PeerId از طریق کانال مورداعتماد همچنان مرحله‌ای جداگانه است.

این پنجره‌ها رضایت آگاهانه در رابط را اعمال می‌کنند، نه تضمین حذف داده از سیستم‌عامل، ابزارهای خارجی یا دستگاه طرف مقابل. آن‌ها جایگزین آزمون مستقل ترافیک شبکه، بررسی logهای ثالث یا پذیرش عملیاتی end-to-end نیستند.

شواهد و آزمون‌های قابل تکرار:
- متن میزبان Windows: src/Shared.Security/PeerInferencePrivacyDisclosure.cs.
- محل نمایش رضایت در Windows: src/Host.Windows/MainWindow.xaml.cs.
- پنجرهٔ اعتماد Android: src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt.
- آزمون Windows: assertions مربوط به disclosure در tests/HostLifecycle.Tests/Program.cs.
- آزمون‌های قرارداد منبع/مستندات: tests/test_peer_inference_privacy_disclosure.py.

منابع:
- AndroidX Core merged manifest: https://android.googlesource.com/platform/prebuilts/sdk/+/dbc89592d1bcb88ed9718f2b8d4f117ebb74547f/current/androidx/manifests/androidx.core_core/AndroidManifest.xml
- Android security guidance for dynamic receivers and signature protection: https://developer.android.com/agents/security/android-intent-security/skill
- Android custom permission protection levels: https://developer.android.com/guide/topics/manifest/permission-element
- Android INTERNET permission: https://developer.android.com/reference/android/Manifest.permission#INTERNET
- AAPT2 command documentation: https://developer.android.com/tools/aapt2

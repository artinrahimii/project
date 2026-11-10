# گزارش اجرای UPD-S1-010 — اتصال چت به چرخهٔ ارسال و دریافت

## وضعیت
**در حال اجرا (in_progress)** — تلاش A001 اصلاح نخست کد و آزمون Room را اعمال می‌کند؛ نتیجهٔ CI دقیق این commit هنوز دریافت نشده است.

## مبنای Git
- مخزن: `artinrahimii/project`
- شاخهٔ مقصد: `main`
- مبنای کد ممیزی‌شده: `cf380af27d462eba973574bac6c2aec467901a28` (`V 3.1.164`)
- پیام commit مبنا: `V 3.1.164`

## شکاف احرازشده
صفحهٔ گفت‌وگوی فعلی فقط پیام کاربر را در Room ثبت می‌کرد و صریحاً اعلام می‌کرد ارسال به میزبان در دسترس نیست. RequestViewModel و runtime واقعی موجود بودند، اما در مسیر گفت‌وگوی عادی فراخوانی نمی‌شدند. برای جزئیات به `evidence/chat-runtime-gap-audit.md` مراجعه شود.

## تغییرات کد این تلاش
- `MainActivity.kt`: URL دایرکتوری و رضایت حریم خصوصی در تنظیمات برنامه نگهداری می‌شوند و به گفت‌وگوی موجود منتقل می‌شوند؛ مسیر ناوبری جدید یا چت بازنویسی نشده است.
- `ConversationDetailScreen.kt`: RequestViewModel مستقل برای همان conversation با Room lifecycle repository ساخته می‌شود؛ دایرکتوری در صورت تنظیم‌شدن کشف می‌شود؛ همتاها در رابط عادی با عنوان عمومی نمایش داده می‌شوند؛ trust و رضایت حریم خصوصی پیش از ارسال لازم‌اند؛ پیام از مسیر `RequestViewModel.sendRequest(prompt)` به runtime واقعی فرستاده می‌شود.
- وضعیت انتخاب مدل، کشف میزبان، اتصال/پردازش، شکست، لغو و retry در همان صفحه نمایش داده می‌شود.
- پاسخ موفق و خطای typed با request/attempt correlation در همان جدول Room مکالمه ذخیره می‌شوند.
- `ConversationMessagesViewModel.kt`: ذخیرهٔ پاسخ assistant و اعلان خطای system با شناسهٔ deterministic و جلوگیری از duplicate اضافه شد.
- `ConversationMessagesViewModelTest.kt`: آزمون Room/Robolectric برای ترتیب پیام کاربر، پاسخ و خطا و idempotency اضافه شد.

## فایل‌های تغییر‌یافته
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationMessagesViewModelTest.kt`
- `update/Step three/UPD-S1-010/execution-report.md`
- `update/Step three/UPD-S1-010/evidence/chat-runtime-gap-audit.md`

## آزمون‌ها
- آزمون جدید `persistsAssistantResponseAndTypedErrorInTheSameConversationExactlyOnce` اضافه شد؛ هنوز اجرا نشده است.
- Android JVM tests، Android build، CI و UI tests روی exact commit این تلاش هنوز اجرا نشده‌اند.
- آزمون فیزیکی Android-to-Windows اجرا نشده است.

## محدودیت‌ها و ادامه
- وضعیت این وظیفه done نیست.
- باید CI دقیق اجرا شود، خطاهای کامپایل/آزمون اصلاح شوند و مصرف‌کنندگان مستقیم و غیرمستقیم request/runtime و navigation دوباره بررسی شوند.
- بعد از آزمون‌های UI و integration مناسب، معیارهای پذیرش تک‌به‌تک ارزیابی خواهند شد.
- آزمون P2P فیزیکی و مسیر real inference Android-to-installed-Windows در UPD-S1-020 همچنان معیار مستقل است.


## تلاش A002 — اصلاح قراردادهای آزمون پس از تغییر مسیر چت

- مبنای A002: `0ec848f0573de18449bf01f1b80e0352ab692620` (V 3.1.165)
- نتیجهٔ A001: quality gate به‌دلیل ۷ assertion قدیمی که اتصال normal chat به runtime را ممنوع می‌کردند شکست خورد؛ Android job نیز در `ktlintMainSourceSetCheck` متوقف شد، بنابراین APK و آزمون‌های Android اجرا نشدند.
- آزمون‌های قرارداد منبع اصلاح شدند تا رفتار مصوب جدید را بسنجند: اتصال ConversationDetailScreen به RequestViewModel، ذخیرهٔ Room، پاسخ/خطای هم‌بسته، consent/trust، حفظ اعتبارسنجی پیام و مسیر مستقیم P2P. تست‌های context capsule همچنان تضمین می‌کنند که پیشنهاد خلاصه فقط پیش‌نویس محلی است و خود capsule ViewModel شبکه ایجاد نمی‌کند.
- ADR-0002 به‌روزرسانی شد تا اتصال منبعی چت به runtime موجود را ثبت کند، اما وضعیت physical Android-to-Windows را همچنان NOT RUN نگه دارد.
- خطوط تازه‌افزوده‌شدهٔ متن UI برای رفع شکست قالب‌بندی Kotlin بازآرایی شدند.
- A002 تا اجرای مجدد quality gate، ktlint، build اندروید و unit testها `partial` است. وضعیت وظیفه `in_progress` باقی می‌ماند.

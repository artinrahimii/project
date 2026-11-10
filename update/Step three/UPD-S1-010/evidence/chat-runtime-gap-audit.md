# ممیزی شکاف اتصال چت به runtime — UPD-S1-010

## مبنای بررسی
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit دقیق بررسی: `cf380af27d462eba973574bac6c2aec467901a28` (`V 3.1.164`)
- زمان ثبت یافته‌ها: `2026-11-10T16:53:33.743Z`

## یافته‌های قطعی پیش از اصلاح
1. `ConversationDetailScreen.kt` فقط `ConversationMessagesViewModel.sendMessage` را صدا می‌زد؛ پیام در Room ذخیره می‌شد و هیچ فراخوانی به `RequestViewModel` یا runtime نداشت.
2. همان صفحه به کاربر می‌گفت ارسال به میزبان در دسترس نیست.
3. `ConversationMessagesViewModel.kt` صراحتاً یک ViewModel محلی بود و فقط نقش `user` را در جدول messages ذخیره می‌کرد؛ مسیر ذخیرهٔ پاسخ میزبان در همان مکالمه وجود نداشت.
4. چرخهٔ شبکه از قبل در `RequestViewModel` و `AndroidInferenceRequestRuntime` موجود بود؛ مسئله نبودن runtime نبود، بلکه جدا ماندن آن از مسیر مکالمه بود.
5. اعتماد همتا از قبل با `AndroidPeerTrustStore` و تأیید صریح کاربر ذخیره می‌شد؛ discovery با `AuthenticatedHostDiscovery` از وضعیت تازه و احراز هویت‌شدهٔ میزبان استفاده می‌کرد.
6. صفحهٔ مستقل New request به‌صورت دستی URL دایرکتوری را می‌گرفت و شناسهٔ Peer را نمایش می‌داد؛ این جزئیات نباید وارد رابط عادی چت شوند.

## اصلاحات واقعی در این تلاش
- URL دایرکتوری و رضایت حریم خصوصی در SharedPreferences برنامه نگهداری می‌شوند؛ صفحهٔ چت IP/Port/PeerId یا Model ID نمی‌خواهد.
- مسیر فعلی صفحهٔ گفت‌وگو حفظ شد؛ برای هر مکالمه یک RequestViewModel مستقل و متصل به Room lifecycle store ایجاد می‌شود.
- با ورود به مکالمه، دایرکتوری ذخیره‌شده کشف می‌شود؛ trust قبلی از مخزن امن همتا خوانده می‌شود؛ همتای جدید فقط پس از تأیید صریح کاربر trusted می‌شود.
- پیش از نخستین ارسال، هشدار حریم خصوصی و رضایت صریح نمایش داده می‌شود.
- دکمهٔ ارسال پیام را ابتدا در Room ثبت می‌کند و سپس `RequestViewModel.sendRequest(prompt)` را فراخوانی می‌کند؛ این overload انتخاب خودکار مدل را اجرا می‌کند و Model ID و سقف خروجی از کاربر خواسته نمی‌شود.
- وضعیت‌های کشف، بررسی مدل، اتصال/پردازش، شکست، لغو و retry به متن کاربرپسند تبدیل شده‌اند.
- پاسخ موفق با نقش `assistant` و خطای typed با نقش `system` در همان conversation ذخیره می‌شوند. شناسهٔ deterministic بر اساس conversation/request/attempt/role، ثبت تکراری ناشی از delivery یا recomposition را مهار می‌کند.
- آزمون Robolectric برای ترتیب پیام user/assistant/system و idempotency پاسخ/خطا اضافه شد.

## محدودیت‌های باز
- این commit هنوز روی CI اعتبارسنجی نشده است؛ build و unit tests باید روی exact HEAD اجرا شوند.
- این تغییر یک اتصال Android UI → موجودیت runtime/gateway را برقرار می‌کند، اما به‌تنهایی آزمون فیزیکی Android به میزبان نصب‌شدهٔ Windows نیست.
- مسیر فیزیکی Android-to-Windows، clean install واقعی و in-place upgrade هنوز شواهد مستقلی لازم دارند.
- آزمون فعلی افزوده‌شده مرز Room persistence را با Robolectric و Room واقعی درون آزمون می‌سنجد؛ mock برای P2P/مدل واقعی به‌عنوان مدرک آن مسیر استفاده نشده است.

## نتیجهٔ ممیزی
شکاف اولیه با کد و آزمون مشخص شد و اصلاح نخست ثبت شده است. تا سبزشدن CI و بازبینی مصرف‌کنندگان/وضعیت‌ها، `UPD-S1-010` در `in_progress` باقی می‌ماند.

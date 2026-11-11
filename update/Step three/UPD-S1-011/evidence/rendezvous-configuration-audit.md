# شواهد ممیزی URL و حریم خصوصی Android

- مبنا: `40cdea13743b35487ca1b6365cb881b181a5ede1` (`V 3.1.200`)
- نیازمندی: `update/Step one/Output/V1.10.json`، `UPD-S1-011`
- کد بررسی‌شده: `MainActivity.kt`, `ConversationDetailScreen.kt`, `RequestViewModel.kt`, `AndroidInferenceRequestRuntime.kt`, `AutomaticModelSelectionPolicy.kt`, `AndroidPeerTrustStore.kt`.

## شواهد

- پیش از اولین ارسال، `pendingPrivacyPrompt` تنظیم می‌شود و تا رضایت صریح درخواست فرستاده نمی‌شود؛ لغو dialog ارسال نمی‌کند.
- مدل با `resolveAutomaticModel` انتخاب می‌شود و سقف خروجی از `resolution.maxOutputTokens` گرفته می‌شود.
- همتا فقط پس از `trustPeerAfterExplicitConfirmation` واجد شرایط است؛ اعتماد قابل revoke است.
- گفت‌وگوی عادی ورودی PeerId، URL، IP، Port، Model ID یا Max Output Tokens ندارد.
- پیش از تغییر، URL ذخیره‌شده با مقدار پیش‌فرض خالی بارگذاری می‌شد و کاربر برای کشف میزبان مجبور به ورود دستی بود.
- مستندات deployment/acceptance endpoint عمومی مستقر را اثبات نمی‌کنند؛ مقدار فرضی درج نشد.

## اصلاح و مرز اعتبار

URL پیکربندی‌شده در زمان build به BuildConfig تزریق می‌شود؛ فقط HTTPS مطلق بدون userinfo/query/fragment پذیرفته است. URL سفارشی در صفحهٔ Advanced host settings نگه داشته می‌شود و override خالی به مقدار بسته‌بندی‌شده برمی‌گردد.

این سند اثبات build، اتصال به endpoint واقعی، discovery عمومی یا اجرای inference نیست. CI exact-head و endpoint واقعی هنوز لازم است.


## نتیجهٔ CI دقیق V 3.1.203

- quality run 43038776230: موفق؛ ۵۰۲ آزمون Python، compileall و دو graph guard.
- product build run 43038776527: موفق؛ Android lint/JVM/build/Keystore instrumentation و Windows WPF/HostLifecycle/installer.
- rendezvous container run 43038776377: موفق؛ فقط build و smoke محلی container.
- هیچ endpoint عمومی در این workflowها deploy یا از شبکهٔ مستقل بررسی نشده است. آدرس نمونه در `docs/deployment/rendezvous-service.md` placeholder است؛ پس این CI شرط endpoint واقعی برای تکمیل UPD-S1-011 را برآورده نمی‌کند.

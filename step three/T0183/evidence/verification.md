# T0183-A002 تا A006 — شواهد lint و instrumentation

## snapshot
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit بررسی‌شده پیش از A006: `c964c25c34c0b0321a369fee8d126ebeb78f686c` (`V 3.0.851`)
- فایل گراف در tree آن commit: Blob `b69a152faf7e1676a998cfd11a776997ef3b7c45`, 3,351,159 بایت UTF-8. محتوای اصلی از Blob API دریافت شد و Git SHA-1 از بایت‌ها کامل تطبیق یافت؛ JSON کامل parse شد.

## تلاش‌ها
- `V 3.0.848`, run `41673696991`: هشت تخلف KtLint در helper اصلی.
- `V 3.0.849`, run `41675040018`: دو تخلف import در تست‌های JVM.
- `V 3.0.850`, run `41675745738`: دو تخلف KtLint در Builder chain.
- `V 3.0.851`, run `41676304428`: KtLint، assembleDebug، JVM unit tests و KVM setup موفق؛ WPF، HostLifecycle، quality gate و container موفق. Android instrumentation step به Gradle پروژه نرسید؛ emulator boot شد، اما action هر خط script را جداگانه اجرا کرد. `cd src/Client.Android` در shell اول باقی نماند؛ فرمان Gradle از repository root اجرا و با خطای نبود build فایل متوقف شد.
- A006 فرمان instrumentation را به `gradle --no-daemon --project-dir src/Client.Android connectedDebugAndroidTest` تبدیل می‌کند تا Gradle root صریح باشد. در A006 هیچ تستی تا اجرای CI موفق شمرده نمی‌شود.

## مرز آزمون
instrumentation باید پنج سناریو را در provider واقعی AndroidKeyStore اجرا کند: هویت پایدار، عدم export کلید، aliasهای مستقل، fail-closed پس از حذف کلید و tamper rejection در identity/trust. emulator شاهد TEE/StrongBox دستگاه فیزیکی نیست.

# T0186 — شواهد اجرا و اصلاح CI

## دریافت/اعتبارسنجی گراف
- commit مورد بررسی: `acb674d5fa46e269edf7c32dfd1828adfbfce79f`، شاخه main، پیام V 3.0.873.
- مسیر: `step three/project-graph.md`.
- Blob از Git tree همان commit: `7c7ce57648e646f0cb97fe04d2521e2b26cf7afd`.
- دریافت Blob کامل بود؛ متن UTF-8 برابر ۳٬۳۰۱٬۷۱۲ نویسه و ۳٬۴۲۹٬۰۳۱ بایت بود. Git Blob SHA-1 محاسبه‌شده منطبق شد. JSON کامل parse شد: ۲۳۰ task، ۴۲۲ edge، بدون شناسهٔ تکراری و بدون edge endpoint نامعتبر.
- شمار وضعیت‌ها در این snapshot: ۱۸۴ done، ۲ deferred_final، ۱ in_progress، ۴۳ not_started.

## نتایج CI برای V 3.0.873
- Quality run: `41809659148`، job `126505436285` — شکست. ۳ assertion: یک تست claims audit با متن قدیمی trust dialog؛ تست جدید با انتظار عبارت انگلیسی در متن فارسی docs؛ و تستی که `HostParticipationTitle` را در harness می‌خواست، درحالی‌که این constant متعلق به کلاس Shared.Security است.
- Product run: `41809659192`.
  - Job `126505436341` Android: شکست در KtLint به‌دلیل supportingText یک‌خطی در MainActivity.kt:284 (newline/argument layout/max line length).
  - Job `126505436649` Windows WPF: موفق.
  - Job `126505436597` HostLifecycle: موفق.
- Rendezvous run `41809659187`: موفق.
- Real local inference run `41809659279`: موفق.

## اصلاح ثبت‌شده در A002
- `MainActivity.kt`: supportingText به بلوک چندخطی مطابق KtLint تبدیل شد؛ متن و منطق رضایت تغییر نکرد.
- `tests/test_claims_audit.py`: assertionها با عنوان و متن جدید هشدار اعتماد و تصریح عدم ارسال خودکار هنگام ذخیرهٔ اعتماد هماهنگ شدند.
- `tests/test_peer_inference_privacy_disclosure.py`: انتظار زبان فارسی برای عبارت `رمزگذاری انتقال` تنظیم شد؛ `HostParticipationTitle` در کلاس disclosure بررسی می‌شود و harness فقط متد/محتوای آن را بررسی می‌کند.

نتیجهٔ CI اصلاح A002 هنوز ثبت نشده است؛ این وظیفه در این مرحله تکمیل‌شده محسوب نمی‌شود.

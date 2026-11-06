# شواهد و راستی‌آزمایی T0215

## commit و گراف مبنا
- شاخه: main
- commit: 78924a6da3eb4aaf96308a80041ce091b3deb250 (V 3.0.996)
- Blob فایل گراف: 21cf5e20a5b66cf62b1d39e882a12b128f2d9456
- اندازهٔ Blob: 3905617 بایت؛ طول متن: 3775654 نویسه.
- محاسبهٔ محلی Git Blob روی بایت‌های UTF-8 کامل با سربرگ Git، SHA برابر 21cf5e20a5b66cf62b1d39e882a12b128f2d9456 تولید کرد؛ با tree entry تطبیق داشت و JSON کامل parse شد.
- Git clone مستقیم از container به‌علت عدم resolve شدن github.com در این محیط ممکن نبود؛ کنترل Blob API و محاسبهٔ Git hash موفق بود.

## CI روی commit 78924a6da3eb4aaf96308a80041ce091b3deb250
- Quality Gate run 42268763411: موفق؛ 446 آزمون، syntax compilation، graph validation و recorded-output/evidence validation موفق؛ Graph Guard با 0 error و 1 warning.
- Product Build run 42268763399: موفق؛ jobهای Windows WPF/installer، Android و HostLifecycle همگی موفق.
- Rendezvous run 42268763422: موفق.
- آزمون‌های مولد مدل مجوز در Windows job موفق شدند؛ از جمله ایجاد notice/manifest و کپی verbatim متن مجوز، رد کردن license نامعتبر، URL غیر HTTPS، checksum نامعتبر، catalog خالی و شناسهٔ تکراری.

## بررسی Artifact نصب‌کننده
- Artifact: https://github.com/artinrahimii/project/actions/runs/42268763399/artifacts/13528547357
- اندازهٔ ZIP: 60985518 بایت؛ 28 entry؛ ZIP integrity check موفق.
- فایل‌های حاضر:
  - windows-installer/licenses/ModelLicenses.md
  - windows-installer/licenses/model-licenses.json
  - windows-installer/licenses/files/WhiteP-Sample-Model-Apache-2.0.txt
  - windows-installer/licenses/ThirdPartyNotices.md
  - windows-installer/licenses/licenses.json
- SHA-256 متن مجوز داخل ZIP و manifest هر دو برابر cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30 هستند.
- مدل: smollm2-135m-instruct-q4-k-m؛ مجوز اعلام‌شده Apache-2.0؛ classification برابر unknown؛ weights_included=false.

## ایراد کشف‌شده در A001
- Git Blob کاتالوگ نمونه: f91c7cb68c151b216260fce14c2f30a8a027a06c، اندازه 711 بایت.
- SHA-256 محتوای UTF-8 با LF: faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580.
- مقدار داخل artifact: 27455158559da383eb9c44ffaf9989c362f6e2353cf318df02622f7069a9def4؛ همین مقدار با نسخهٔ CRLF متن حاصل شد. علت، حساسیت هش به تبدیل line ending است؛ به همین دلیل A001 به‌تنهایی برای تکمیل کافی نیست.

## اصلاح A002
- tools/generate_model_license_notice.py: canonicalize CRLF to LF before JSON parsing/hash; reject bare CR; document hash policy.
- tests/test_generate_model_license_notice.py: add LF/CRLF hash-stability regression test.
- docs/windows-installer.md: document canonical hash policy.
- نتیجهٔ CI و Artifact نسخهٔ اصلاح‌شده هنوز pending است.

## انتشار GitHub
- Draft Release 436740935 / tag android-v0.1.0 به commit قدیمی‌تر ad90a80f98321602dc57a1d3c89113cefa86a0e1 متصل است و دارایی‌های آن مدل‌مجوز تازه را ندارند.
- Product Build روی commit 78924a6 موفق است، ولی workflow release-assets به tag جدید و Android signed-release موفق روی همان SHA نیاز دارد.
- ابزارهای متصل GitHub در این نشست create-tag و workflow_dispatch را ارائه نمی‌کنند. بنابراین Release جدید ایجاد یا Release قدیمی اصلاح نشده است.

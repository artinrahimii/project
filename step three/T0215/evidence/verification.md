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
- Draft Release 436740935 / tag android-v0.1.0 قدیمی است: شیء annotated tag با SHA برابر ad90a80f98321602dc57a1d3c89113cefa86a0e1 به commit مقصد cca243d28197df90900885a35a54a8ca8ae3751e (V 3.0.990) اشاره می‌کند؛ ad90... خودِ commit نیست.
- Product Build روی commit 78924a6 موفق است، ولی workflow release-assets به tag جدید و Android signed-release موفق روی همان SHA نیاز دارد.
- ابزارهای متصل GitHub در این نشست create-tag و workflow_dispatch را ارائه نمی‌کنند. بنابراین Release جدید ایجاد یا Release قدیمی اصلاح نشده است.


## A003 — تأیید روی main / V 3.0.998

- Quality Gate [42277720293](https://github.com/artinrahimii/project/actions/runs/42277720293): موفق؛ ۴۴۷ آزمون Python، compileall، graph validation و validation خروجی/شواهد همگی موفق؛ Graph Guard: ۲۳۰ وظیفه، ۴۲۲ یال، صفر خطا و یک هشدار محفوظ.
- Product Build [42277720362](https://github.com/artinrahimii/project/actions/runs/42277720362): سه job ویندوز، اندروید و HostLifecycle موفق. آزمون مولد مدل مجوز ۸/۸ و آزمون inventory مجوز وابستگی ۷/۷ موفق؛ آزمون LF/CRLF نیز در همین اجرا موفق.
- Artifact نصب‌کننده: ID `13534875582`، اندازهٔ ۶۰٬۹۸۵٬۵۲۷ بایت، ۲۸ ورودی؛ ZIP integrity موفق. پنج فایل notice/مجوز مورد انتظار در آن موجود بود.
- کاتالوگ مدل در commit مبنا: ۷۱۱ بایت؛ Git Blob SHA-1 محاسبه‌شده از بایت‌ها با Blob tree برابر `f91c7cb68c151b216260fce14c2f30a8a027a06c` تطبیق کرد. SHA-256 برابر `faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580` با manifest داخل Artifact یکسان بود.
- متن کامل Apache-2.0 داخل ZIP: ۱۱٬۳۵۸ بایت؛ SHA-256 برابر `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30` و مطابق manifest بود.
- tag قدیمی `android-v0.1.0`: annotated tag object `ad90a80f98321602dc57a1d3c89113cefa86a0e1` → commit `cca243d28197df90900885a35a54a8ca8ae3751e` (`V 3.0.990`). Release draft قدیمی همچنان متصل به این نسخه است، نه V 3.0.998.
- **نتیجه:** اصلاح A002 تأیید شده؛ انتشار GitHub Release تازه هنوز pending است. ابزار اتصال این نشست tag creation یا workflow_dispatch ندارد؛ بنابراین T0215 همچنان in_progress می‌ماند.


## راستی‌آزمایی تکمیلی روی main / V 3.0.999

- Quality Gate [42286282408](https://github.com/artinrahimii/project/actions/runs/42286282408): موفق.
- Product Build [42286282513](https://github.com/artinrahimii/project/actions/runs/42286282513): موفق؛ Windows، Android و HostLifecycle همگی موفق.
- Rendezvous [42286282640](https://github.com/artinrahimii/project/actions/runs/42286282640): موفق.
- Windows installer artifact `13536989328`: 60,985,523 بایت، 28 ورودی ZIP، integrity check موفق. پنج فایل notice/license لازم حاضر بودند.
- SHA-256 کل ZIP: `c21d1692402c5ea23a5a5f626e111744a5ee9dfc6d90ee6beba3c31946e2d5ab`.
- `source_catalog_sha256`: `faba6e09aefced04f8bed354af28e8b8f838ad4e80386030d29d73c4d17ca580`؛ `license_text_sha256`: `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30`؛ هر دو مطابق منبع/manifest بررسی‌شده‌اند.
- Variant اندروید این Product Build با کلید موقت CI ساخته شده است، نه کلید تولیدی محافظت‌شده؛ artifact APK دیباگ است. Release تولیدی همچنان نیازمند اجرای workflow `android-release.yml` در محیط محافظت‌شده است.
- Release draft `436740935` و tag `android-v0.1.0` هنوز به commit قدیمی `cca243d28197df90900885a35a54a8ca8ae3751e` (`V 3.0.990`) اشاره دارند. `android-release.yml` و `release-assets.yml` با push tag `android-v*` یا `workflow_dispatch` اجرا می‌شوند؛ این نشست ابزار ایجاد tag و dispatch ندارد. هیچ tag یا Releaseی تغییر نکرد.
- **نتیجه:** خروجی Windows/CI روی V 3.0.999 معتبر است، اما معیار Release جاری تکمیل نشده و T0215 باز می‌ماند.


## A004 — راستی‌آزمایی دوبارهٔ Draft Release و tag روی main / V 3.0.1000 (2026-11-07)

- commit فعلی main هنگام بررسی: `8665c9bef63ac60f57250cca43ebadef2fb3ca44` با پیام `V 3.0.1000`؛ درخت commit: `e9f9a45b6c2dda93fabf6eec8f72d904c33060d0`.
- GET صفحهٔ Releases در مخزن، Draft Release با شناسهٔ `436740935` و tag برابر `android-v0.1.0` را نشان داد. برخلاف عبارت «بدون artifact» در گزارش قبلی A003، این Draft اکنون چهار asset بارگذاری‌شده دارد: `BUILD-PROVENANCE.txt`، `SHA256SUMS.txt`، `WhiteP-Android-0.1.0.apk` و `WhiteP-Windows-Installer.zip`. این اصلاح فقط دربارهٔ وجود assetهاست و به معنای تأیید محتوای آن‌ها یا تازه‌بودن Release نیست.
- GET مرجع tag و شیء annotated آن را تأیید کرد: `android-v0.1.0` → tag object `ad90a80f98321602dc57a1d3c89113cefa86a0e1` → commit `cca243d28197df90900885a35a54a8ca8ae3751e` با پیام `V 3.0.990`. این commit با main فعلی `V 3.0.1000` متفاوت است.
- فایل‌های source در همان commit هدف tag بررسی شدند: `tools/generate_model_license_notice.py` وجود ندارد (404)، `docs/windows-installer.md` شامل توضیح `ModelLicenses.md` و `model-licenses.json` نیست، `tools/build_windows_installer.ps1` فاقد ادغام مولد مجوز مدل است و `.github/workflows/release-assets.yml` نیز کنترل‌های جدید الزامی فایل‌های مجوز مدل را در آن نسخه ندارد. بنابراین source مربوط به Draft قدیمی، تغییرهای T0215 را در بر نمی‌گیرد؛ وجود چهار asset به‌تنهایی معیار گنجاندن مجوز مدل در بستهٔ انتشار جاری را اثبات نمی‌کند.
- فایل‌های اصلی روی main جدیدترند: مولد مجوز مدل و ادغام installer موجودند و CI/Artifactهای Product Build روی commitهای بعدی در A003 و راستی‌آزمایی V 3.0.999 گزارش شده‌اند. با این حال Draft به آن SHAها متصل نیست و این نشست ابزار ساخت tag جدید یا اجرای `workflow_dispatch` برای `android-release.yml` را ندارد.
- **نتیجه:** A004 مانع قبلی را دقیق‌تر کرد و ادعای نادرست «Draft فاقد asset است» را اصلاح کرد؛ اما T0215 هنوز `in_progress` می‌ماند. این بررسی هیچ build، signed-release، آزمون جدید یا فایل اجرایی نرم‌افزار تولید نکرد؛ T0216 آغاز نمی‌شود.

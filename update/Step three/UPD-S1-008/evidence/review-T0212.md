# بازبینی محتوایی T0212 — امضای production APK

- مبنای کد جاری: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` — `V 3.1.106`
- وضعیت پایه: `done`؛ بدون تغییر.
- UPD مرتبط: `UPD-S1-021`
- تصمیم: `re_review_required`؛ وضعیت بازبینی: `reviewed`.
- دلیل بازبینی مجدد آتی: بستهٔ نهایی UPD-S1-021 باید دوباره به source/build هم‌نسخه و artifact قابل‌تحویل متصل شود.

## کد و آزمون بررسی‌شده
- `.github/workflows/android-release.yml` — Blob جاری `1648eb6cb5ed66a67952da5b7dcdda58863234e9`؛ همین Blob دقیقاً در commit اجرای موفق امضا نیز وجود دارد.
- `tests/test_android_release_signing_contract.py` — Blob `c420c1e24c24b2724322a1e45029311d5c45367a`
- `docs/android-release-signing.md`
- workflow، کلید را از protected Environment می‌گیرد، بیرون checkout در runner-temp نگه می‌دارد، امضا را با fingerprint محافظت‌شده تطبیق می‌دهد و keystore را در پایان حذف می‌کند. Gradle بدون داده‌های امضای لازم fail-closed است.

## اجرای production واقعی
- Android signed release run: https://github.com/artinrahimii/project/actions/runs/42114568183
- source commit: `179efa9fe07c3ba007885e9e3198eba620092617` (`V 3.0.983`)، event برابر `workflow_dispatch`، نتیجه `success`.
- job `127550657183` و هر 15 مرحله موفق شدند؛ بازیابی امن keystore، build امضاشده، بررسی امضا و fingerprint، upload artifact و پاک‌سازی key موفق بودند.
- Artifact `whitep-android-release-signed`: شناسهٔ 13455966209، ZIP برابر 18,596,315 بایت؛ APK داخل آن 22,786,909 بایت و SHA-256 ثبت‌شده `7f845825e714c6b3b7b699b704238e2200fd941382a73c8375293fde06c04b7a`.
- Product Build روی همان source commit با run `42113989702` موفق بود.

## محدودیت
اجرای signed-release واقعی روی V 3.0.983 انجام شده است؛ workflow Blob آن با current HEAD یکسان است، ولی در این نوبت dispatch جدید انجام نشد. هیچ secret یا کلید خصوصی خوانده یا در گزارش درج نشده است.

## نتیجه
امضای واقعی production و تطبیق fingerprint با شواهد CI و artifact اثبات شد. وضعیت پایهٔ `done` حفظ شد؛ بازبینی مجدد پس از بسته‌بندی UPD-S1-021 لازم است.

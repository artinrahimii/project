# بازبینی محتوایی T0213 — checksum و Draft Release

- مبنای بررسی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` — `V 3.1.106`
- وضعیت پایه: `done`؛ بدون تغییر.
- UPD مرتبط: `UPD-S1-021`
- تصمیم: `re_review_required`؛ وضعیت بازبینی: `reviewed`.
- دلیل بازبینی مجدد آتی: UPD-S1-021 باید checksum و مجوزها را به artifactهای خروجی همان source commit متصل کند و workflow tag پس از اصلاح مجدداً اجرا شود.

## کد و آزمون بررسی‌شده
- `.github/workflows/release-assets.yml` — Blob جاری `f8a70d869c88b0c1ab5e4a9e6125af144dca212b`
- `tests/test_release_assets_contract.py` — Blob `fc34b15dadacd55d6d3606a2c84fd79c918f699d`
- `docs/release-assets.md`
- `step three/T0213/evidence/SHA256SUMS-final.txt`
- workflow فقط build موفق Android signed-release و Windows installer مربوط به همان SHA را می‌پذیرد؛ APK، installer ZIP و provenance را SHA-256 می‌کند و `sha256sum --check` را اجرا می‌کند.

## شواهد انتشار
- Tag-triggered workflow run: https://github.com/artinrahimii/project/actions/runs/42253832983
- Release-assets artifact: ID 13521278559، حجم ZIP برابر 79,549,283 بایت، digest برابر `7758e32252dd83e1a1a31f253c0cbb919419ba8cf6f00ff66eedb82522154c53`.
- ZIP مستقل بررسی شد و سه ورودی manifest با `sha256sum --check` همگی OK شدند:
  - Android APK: `8d478da29421ceb1e77974c01fced1a8aeeba1f6991c19d930f45d4a5b11daba`
  - Windows installer ZIP: `1083700789325e7927ccabc627824177e347e2ec335ac388a1ba7d847e2477d4`
  - provenance: `0181b6a9c51cc0e91ba81342e4750f862edcbda2c0f0c5b0f41ce010fe84db64`
- Draft Release ID 436740935 با tag `android-v0.1.0` هنوز موجود است، `draft=true` و هر چهار asset در وضعیت `uploaded` هستند.
- اجرای قبلی در مرحلهٔ نهایی Draft lookup failure شد. اصلاح بعدی Quality Gate run `42255409787` با 437 تست و guardها را گذراند؛ Product Build `42255409532` و Rendezvous `42255409556` نیز موفق شدند.
- Quality Gate دقیق current HEAD، run `42516118975`، قرارداد فعلی workflow را نیز با موفقیت آزمود.

## محدودیت و failure history
Run `42253832983` در مرحلهٔ نهایی lookup/validation Draft با failure پایان یافت، هرچند checksum، upload artifact و وجود Draft/چهار asset مستقل تأیید شده بود. اصلاح بعدی در CI قراردادی آزموده شده، اما اجرای tag جدید پس از اصلاح در این نوبت انجام نشده است. Release عمداً Draft است و عمومی منتشر نشده است.

## نتیجه
checksum و Draft Release واقعی وجود دارند و سه ورودی manifest اعتبارسنجی شده‌اند. وضعیت پایهٔ `done` حفظ شد؛ به‌دلیل اصلاح workflow و نیاز به اجرای tag مجدد، بازبینی پس از UPD-S1-021 لازم است.

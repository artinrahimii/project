# T0183-A001 — شواهد، مبنا و مرز اعتبارسنجی

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit مبنا: `0f1089e1890c24846ac9ebf324609951645dd962` (`V 3.0.843`)
- tree مبنا: `b28fee0dc8bc35793cfd2bf655d174b82eadfc07`
- graph path: `step three/project-graph.md`
- graph blob در همان tree: `738fa82f3855fc7c3ac46f02424b48230f433c1d`؛ اندازهٔ tree برابر 3,339,558 بایت است. متن UTF-8 کامل از Git Blob API دریافت و تعداد بایت‌ها با اندازهٔ tree تطبیق داده شد؛ JSON fenced به‌طور کامل parse شد. فایل دریافت‌شده 3,212,587 نویسه داشت؛ اندازهٔ نویسه با بایت یکی فرض نشده است.
- ساختار graph: 230 task node، 422 edge و 259 edge-change entries.
- وضعیت آغازین: 181 `done`، 47 `not_started`، 2 `deferred_final`. T0183=`not_started`; T0182 و T0177=`done`.
- ورودی‌های T0183: `T0182 → T0183` و `T0177 → T0183` هر دو `type=blocks`, class/state=`unreviewed`. خروجی T0183→T0184 هم `type=blocks`, class/state=`unreviewed`.
- شواهد پیش‌نیازها: `step three/T0182/execution-report.md` و `step three/T0182/evidence/verification.md`; `step three/T0177/execution-report.md` و `step three/T0177/evidence/verification.md`. منطق selector/attempt budget در Android از key-storage مسیر مستقلی دارد؛ Windows orphan-temp cleanup نیز خروجی موردنیاز Android Keystore نیست.
- قرارداد T0184: `docs/decisions/ADR-0020-android-client-local-conversation-storage.md` مشخص می‌کند که Room database هنوز encryption-at-rest ندارد؛ helper جدید برای حفاظت کلید دیتابیس آینده عرضه می‌شود اما رمزنگاری دیتابیس در این task انجام نشده است.
- شواهد موجود پیش از این task: identity و trust stores از AndroidKeyStore AES/GCM استفاده می‌کردند، اما تست‌های JVM آن‌ها protector آزمایشی تزریق می‌کردند؛ بنابراین خود provider واقعی در suite آزموده نمی‌شد.
- آزمون جدید: پنج instrumentation scenario روی Android API 36 emulator؛ شامل ذخیره و بازیابی هویت، بررسی exportability، دو alias مستقل، fail-closed پس از حذف کلید و رد envelope دستکاری‌شدهٔ identity/trust.
- محدودیت: emulator نمی‌تواند hardware backing واقعی TEE/StrongBox را روی دستگاه فیزیکی اثبات کند. گزارش CI باید این مرز را حفظ کند.
- هیچ آزمون جدیدی در این فایل PASS نشده است؛ نتیجه باید بعد از اجرای workflow دقیق commit ثبت شود.

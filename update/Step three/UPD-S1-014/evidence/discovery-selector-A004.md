# شواهد UPD-S1-014 — تلاش A004

- مبنای دقیق: `cc6c8c9f0839137867732bdda9ddef9c3045d2a5` — `V 3.1.185`.
- A003 در Product Build Android، ktlintMainSourceSetCheck را با دو تخلف `standard:chain-method-continuation` در HostCandidateSelector.kt خطوط 65 و 67 شکست داد.
- اصلاح A004: status، announcement و capabilities هر دو candidate به متغیرهای جداگانه استخراج می‌شوند؛ access chainها از comparator حذف شده‌اند.
- فرمول همچنان `activeRequests / maxConcurrentRequests` است و با integer cross-product مقایسه می‌شود؛ tie-break هنوز PeerId است.
- Quality V 3.1.185: PASS، ۴۹۶ Python tests و graph guards.
- HostLifecycle و Rendezvous V 3.1.185: PASS.
- Android JVM/Keystore در A003 اجرا نشدند، چون lint شکست خورد.
- CI دقیق A004 هنوز اجرا نشده است؛ هیچ قبولی ادعا نمی‌شود.

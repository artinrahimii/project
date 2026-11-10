# شواهد UPD-S1-014 — تلاش A002

- مبنای دقیق: `66fe4dc744e045b528bf9bade78e7613da6eb831` — `V 3.1.183`.
- خطای A001: ktlint در `HostCandidateSelector.kt` سه خطای `standard:chain-method-continuation` در خطوط 63، 66 و 74 گزارش کرد.
- اصلاح: candidateهای واجدشرایط ابتدا در یک mutable list جمع می‌شوند، سپس `sortWith` جداگانه با همان comparator اجرا می‌شود؛ عملیات chained بعد از lambda حذف شد.
- منطق مقایسه بدون تغییر است: `activeRequests / maxConcurrentRequests` با cross-product صحیح و PeerId به‌عنوان tie-break.
- آزمون جدید `ranksReadyCandidatesByMeasuredUtilizationAndBreaksTiesByPeerId` و Python contract در A001 اضافه شده‌اند و در A002 حفظ شده‌اند.
- هیچ wire/protobuf/API یا production cardinality تغییر نکرده است.
- A002 exact-head CI هنوز اجرا نشده؛ نتیجهٔ PASS ادعا نمی‌شود.
- Product Build ویندوز A001 هنوز در حال اجرا بود؛ نتیجهٔ نهایی آن در update بعدی گزارش/ثبت می‌شود.

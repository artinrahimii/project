# شواهد UPD-S1-014 — تلاش A003

- مبنای دقیق: `911ef42bdfaf8b32a46169360a710ff21a3b9146` — `V 3.1.184`.
- شکست A002: آزمون `test_selector_keeps_three_host_batching_as_compatibility_only` در line 43 انتظار داشت متن `.sortedWith { first, second ->` در source وجود داشته باشد.
- کد A002 به دلیل ktlint chain-method-continuation، عملیات را در `eligibleCandidates.sortWith { first, second ->` جدا کرده بود؛ بنابراین assertion منسوخ شده بود.
- اصلاح A003: assertion Python به `eligibleCandidates.sortWith { first, second ->` تغییر یافت.
- منطق مقایسهٔ `activeRequests / maxConcurrentRequests`، comparator، PeerId tie-break و JVM test ranking تغییر نکردند.
- Quality A002: ۴۹۶ آزمون، یک شکست source-contract؛ compileall/graph guard پس از آن اجرا نشدند.
- HostLifecycle A002: PASS؛ Rendezvous A002: PASS.
- Product Build Android/Windows در زمان ثبت A003 هنوز نتیجهٔ نهایی نداشت.
- CI دقیق A003 هنوز اجرا نشده و هیچ PASS برای assertion جدید ادعا نمی‌شود.

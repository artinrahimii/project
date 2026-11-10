# شواهد UPD-S1-014 — تلاش A005

- مبنا: `f5268f24296dee058ab26bbeddaeee3cc68b9660` — `V 3.1.186`.
- شکست A004: آزمون `test_selector_keeps_three_host_batching_as_compatibility_only` در ۴۹۶ آزمون Python، یک assertion منسوخ داشت.
- موارد قدیمی: `first.status.activeRequests.toLong()`, `second.announcement.capabilities.maxConcurrentRequests.toLong()`, peer ID chain قدیمی و `.take(maxHosts)`.
- source واقعی: `firstStatus.activeRequests`, `firstCapabilities.maxConcurrentRequests`, `secondStatus.activeRequests`, `secondCapabilities.maxConcurrentRequests`, `firstAnnouncement.ephemeralHostId.compareTo(secondAnnouncement.ephemeralHostId)` و `eligibleCandidates.take(maxHosts)`.
- اصلاح A005 فقط Python source contract را با این source واقعی همگام می‌کند. Kotlin comparator و JVM ranking test تغییر نکرده‌اند.
- Android ktlint V3.1.186: PASS؛ clean Debug APK build در جریان بود.
- HostLifecycle و Rendezvous V3.1.186: PASS.
- Exact CI برای A005 هنوز اجرا نشده؛ no PASS is claimed.

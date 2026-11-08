# شواهد بازبینی UPD-S1-005

مبنای ممیزی: V 3.1.36، SHA d027e72f3968ef7c8c826f889042eb67a8533fa4.

## رضایت و توقف
- NetworkParticipationToggle در startup خاموش است.
- فعال‌سازی به backend متصل و Ready، ظرفیت معتبر، privacy disclosure با پاسخ پیش‌فرض No و تأیید صریح PeerId نیاز دارد.
- PeerAnnouncementHostLifecycle.StopAsync ابتدا _active را صفر می‌کند؛ سپس publisher و transport بسته و handlerها تخلیه می‌شوند.
- ParticipationNetworkListenerLifecycleTests خاموش‌بودن اولیه و توقف/آزادسازی listener را آزمون می‌کند.

## ظرفیت
- WindowsHostHardwareDiscovery RAM آزاد فیزیکی و فضای آزاد درایو سیستم را اندازه می‌گیرد.
- HostReadinessEvaluator حدها را با اندازه‌گیری زنده مقایسه و Unknown/Insufficient را fail-closed می‌کند.
- HostInferenceSession پیش از هر inference readiness را دوباره می‌سنجد.
- HostRequestAdmissionPolicy مشارکت خاموش، میزبان غیر Ready، مدل ناسازگار، ظرفیت Unknown/Insufficient و deadline نامعتبر/منقضی را رد می‌کند.
- HostRequestCoordinator با SemaphoreSlim(1,1) و WaitAsync(0) درخواست دوم را صف نمی‌کند و Busy می‌دهد.
- GPU صرفاً inventory است؛ سنجش compute/VRAM یا سقف CPU/idle/AC power ادعا نمی‌شود.

## تغییر جدید
LocalHostSettingsStore schema 2 مقدارهای requiredModelMemoryGiB و minimumFreeDiskGiB را persist و validate می‌کند. schema 1 با defaults قبلی 4/1 GiB بارگذاری می‌شود و اولین Save آن را به schema 2 ارتقا می‌دهد. آزمون C# migration/round-trip/bounds و آزمون Python wiring اضافه شده‌اند؛ CI جدید pending است.
# بازبینی محتوایی T0019 — UPD-S1-008

## مبنای بررسی
- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit اسناد/منابع بررسی‌شده: `32015598e6ff38f3984efb0bc8c5d1797b99cdef` — `V 3.1.59`.
- کیفیت دقیق همین commit: [42481932091](https://github.com/artinrahimii/project/actions/runs/42481932091) — موفق.
- مرجع نیازمندی: `update/Step one/Output/V1.10.json`.
- UPD-S1 مؤثر: `UPD-S1-002`.

## Task baseline و تصمیم
- شناسه: `T0019`.
- عنوان: مشخص‌کردن direct P2P به‌عنوان مسیر دادهٔ پیش‌فرض و محدودیت سرویس کمکی به discovery/connection coordination.
- وضعیت پایه و نهایی: `done → done`.
- وابستگی‌های legacy: `T0018` و `T0010`؛ هر دو در گراف فعلی `done`.
- تصمیم reconciliation: `unchanged`.
- `status_changed=false`؛ هیچ وضعیت یا یال قدیمی تغییر نکرده است.

## نیازمندی و artifact تصمیم
- `REQ-010`: Direct peer-to-peer transfer is the default data path؛ rendezvous فقط برای discovery/connection setup است و payload را relay نمی‌کند.
- `REQ-011`: هیچ API مرکزی نباید متن درخواست را دریافت، مدل را اجرا یا تاریخچه را نگهداری کند.
- `docs/decisions/ADR-0002-direct-p2p-data-path.md` تصمیم مستقیم P2P، control-plane metadata-only، fail-closed برای direct connection، identity-pinning، منع relay inference payload و مرز مشاهده‌پذیری metadata را مشخص می‌کند.
- ADR به‌صراحت تفاوت decision/source evidence با runtime acceptance را نگه می‌دارد؛ سرویس میزبانی/زیرساخت ممکن است metadata اتصال را ببیند و سیاست آن جداگانه نیازمند بررسی است.
- README تصمیم را لینک می‌کند اما صریحاً می‌گوید انتقال P2P و ادغام آن با UI عادی چت هنوز موجود/تکمیل‌شده نیست.
- `docs/acceptance.md`، `AC-P2P-01` و `AC-PRIV-01` را NOT RUN نگه می‌دارد؛ mock یا loopback به‌جای آزمون فیزیکی پذیرفته نشده است.

## آزمون و مصرف‌کنندگان
- `tests/test_p2p_data_path_decision.py` چهار شرط را بررسی می‌کند: هم‌ترازی تصمیم با REQ-010/011؛ محدودیت rendezvous به control-plane؛ منع relay fallback؛ و حفظ NOT RUN بودن runtime acceptance.
- تست فوق در quality workflow دقیق V3.1.59 اجرا شد. نتیجهٔ کامل: 479 آزمون Python موفق، compileall موفق، Step Three graph guard موفق و Update graph structural/output-evidence guards موفق.
- مصرف‌کنندگان/مرزهای منبع بررسی‌شده:
  - `PeerAnnouncement.cs`: metadata peer با محدودیت اندازه/فیلد و اعتبارسنجی identity/address/capability/expiry.
  - `PeerAnnouncementServiceHost.cs` و `PeerAnnouncementDirectory.cs`: endpointهای announcement/discovery و نگهداری in-memory رکورد metadata جاری؛ endpoint inference/history وجود ندارد.
  - `PeerAnnouncementClient.cs`: client فقط عملیات announce/discovery دارد.
  - `PeerTransportFallbackConnector.cs`: اتصال مستقیم با identity موردانتظار و شکست صریح زمانی که direct candidate موفق نباشد؛ fallback سروری برای payload ایجاد نمی‌کند.
  - `RendezvousPeerDirectoryClient.kt`: کلاینت اندروید برای دریافت metadata peer؛ آن را با انتقال inference payload یکی نمی‌گیرد.
  - `Libp2pInferenceRequestTransport.kt`: مؤلفهٔ transport مستقیم موجود است؛ اما ADR و README می‌گویند هنوز به UI گفت‌وگوی عادی متصل نشده است.
  - `PeerAnnouncementClientAcceptanceTests.cs` و `PeerTransportContractAcceptanceTests.cs`: منبع آزمون loopback/host-contract مرور شد؛ این‌ها جایگزین آزمون فیزیکی Android-to-Windows نیستند.
- ADR-0004 منع relay inference و metadata قابل‌مشاهده؛ ADR-0005 اتصال مستقیم host-side و هویت pin‌شده؛ ADR-0006 leaseهای metadata و محدودیت public reachability را با ADR-0002 همسو نگه می‌دارند.
- وابستگی‌های مستند T0019 به T0018/T0010 برقرارند؛ دامنهٔ task از معماری/مستندات فراتر نمی‌رود و implementation/runtime اضافی را به آن نسبت نمی‌دهد.

## نتیجه و محدودیت
- تصمیم `unchanged`؛ وضعیت قدیمی `done` حفظ شد.
- `AC-P2P-01` همچنان NOT RUN؛ این review اثبات packet path، NAT traversal، reachability عمومی یا Android normal-chat routing نیست.
- هیچ prompt/response یا تاریخچه‌ای از طریق این review به rendezvous فرستاده نشده است؛ داده‌های واقعی مدل/شبکه نیز در این review به کار نرفته‌اند.
- این وظیفه از نظر scope قدیمی همچنان fulfilled است؛ کارهای پیاده‌سازی و E2E در وظایف جداگانه باقی می‌مانند.

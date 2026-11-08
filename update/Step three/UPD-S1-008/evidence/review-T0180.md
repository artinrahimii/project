# بازبینی T0180 — پاک‌سازی payload و بافرهای موقت

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` — `V 3.1.94`
- source code baseline: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-007، UPD-S1-018

## کد و مسیرهای بررسی‌شده

- Android `PeerMessageFrameDecoder` در پایان frame، خطای decode، پایان stream یا clear، نسخه‌های موقت قبلی، input و pending buffer را با `Arrays.fill(...,0)` پاک می‌کند.
- Android transport آرایهٔ serialized request، host-status request و CancelRequest را در finally پاک می‌کند؛ buffer فریم transport پس از write/response/teardown نیز تحت کنترل است.
- Windows `Libp2pPeerMessageProtocol` فریم خروجی را تا زمانی که forwarding multiplexer امن باشد در فهرست bounded نگه می‌دارد؛ در صورت شکست ارسال آزاد و zero می‌کند و بعد از دریافت پیام peer یا قطع ارتباط/Dispose بافرهای نگهداری‌شده و پیام‌های inbound باقی‌مانده را پاک می‌کند.
- Windows request lifecycle برای wireهای ارسال/دریافت‌شده از helperهای SendAndClear و zeroization استفاده می‌کند.

## آزمون و شواهد

- `PeerMessageFrameCodecTest.kt`: framing/decoder و رفتار با bufferهای جزئی و پیام‌های malformed را پوشش می‌دهد.
- `tests/test_sensitive_buffer_cleanup.py`: پاک‌سازی بافرهای outbound/inbound، payload wire و temporary Android buffer را به source وصل می‌کند.
- `tests/test_libp2p_peer_message_buffer_lifetime.py`: نگهداری محدود تا ACK/response و پاک‌سازی پس از response/stream teardown/Dispose را کنترل می‌کند.
- Product Build دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شدند.

## محدودیت و تصمیم

بافر Windows برای جلوگیری از آزادسازی زودهنگام memory مورد استفادهٔ async multiplexer، تا دریافت پیام ترتیبی بعدی یا teardown نگه داشته می‌شود؛ بنابراین «پاک‌سازی فوری در لحظهٔ timeout» از این source به‌تنهایی اثبات نمی‌شود. آزمون‌های موجود عمدتاً structural/contract و test-suite integration هستند و سنجش forensic حافظه در اجرای فیزیکی نیستند. وضعیت پایه `done` حفظ شد و `re_review_required` تا بازاعتبارسنجی پس از UPD-S1-018 باقی ماند.

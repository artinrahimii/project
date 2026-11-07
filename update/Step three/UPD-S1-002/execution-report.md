# گزارش اجرای UPD-S1-002 — تصمیم مسیر انتقال و مرز اعتماد

## خط مبنا و نتیجهٔ این تلاش

- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit مبنای شروع اجرای این وظیفه: `22bd0cf7ebbd7b0b75e85331fdc743845769a76a` — `V 3.1.21`
- commit مبنای کد محصول ممیزی‌شده: `14538c63f940c6da5b44c0b9ad03614a54fba69f` — `V 3.1.20`
- تغییر `V 3.1.21` فقط یک assertion آزمون ممیزی بود؛ کد Android/Windows/Rendezvous بین این دو commit تغییر نکرد.
- نتیجهٔ فعلی: **in_progress**؛ تصمیم معماری و regression test نوشته شده‌اند، اما اجرای آزمون‌های همین تغییر روی commit اختصاصی آن هنوز pending است. تا مشاهدهٔ موفق CI وضعیت `done` ثبت نمی‌شود.
- Attempt: `UPD-S1-002-20261107201128`
- زمان آغاز/ثبت این تلاش: `2026-11-07T20:11:28.347Z`

## تصمیم محصول

1. مسیر اصلی نسخهٔ اولیه، انتقال مستقیم و احرازشدهٔ P2P بین Android و میزبان Windows است.
2. Rendezvous فقط control-plane است: دریافت اعلان metadata و پاسخ capability discovery. متن prompt، context capsule، پاسخ مدل یا تاریخچهٔ گفتگو جزو قرارداد آن نیست.
3. مسیر جایگزین فقط تلاش روی آدرس مستقیم دیگر برای همان PeerId مورد اعتماد است. Windows host-side connector ترتیب TCP/QUIC را پشتیبانی می‌کند؛ Android در وضعیت فعلی فقط آدرس TCP را انتخاب می‌کند و QUIC را در این مسیر مصرف نمی‌کند.
4. اگر مسیرهای مستقیم قابل‌استفاده شکست بخورند، درخواست باید با خطای اتصال متوقف شود. relay یا API مرکزی جایگزین خاموش نیست.
5. relay محتوایی در نسخهٔ فعلی غیرفعال است. هر relay آتی نیازمند تصویب مستقل، افشای فراداده، کانال E2E بین دو endpoint و آزمون واقعی سه‌همتایی خواهد بود.
6. از کتابخانهٔ موجود libp2p برای Noise/TLS و اعتبارسنجی PeerId استفاده می‌شود؛ الگوریتم رمزنگاری اختصاصی ایجاد نشده است.

## ممیزی مسیرهای اجرایی موجود

- سرویس Rendezvous مسیرهای `POST /v1/peers/announcements` و `GET /v1/peers?capability=...` را دارد؛ علاوه بر `/healthz` مسیر مرکزی inference یا history در سرویس دیده نشد.
- قرارداد اعلان فقط `PeerId`، آدرس‌های مستقیم، capabilityها و expiry را دارد. `JsonUnmappedMemberHandling.Disallow` فیلدهای افزوده‌شده مانند prompt/capsule را رد می‌کند.
- دایرکتوری از `Dictionary<string, PeerAnnouncement>` در حافظهٔ فرایند استفاده می‌کند، رکورد جدید را جایگزین رکورد قبلی همان PeerId می‌کند و رکوردهای منقضی را هنگام announce/discovery بعدی حذف می‌کند.
- ناشر Windows به‌طور پیش‌فرض TTL پنج دقیقه و renewal یک دقیقه دارد. توقف ناشر حذف فوری رکورد راه دور نیست؛ رکورد پس از expiry و cleanup بعدی از نتایج discovery خارج می‌شود.
- هر دو client برای URL غیر loopback، HTTPS را لازم دارند. Android فقط GET metadata را می‌فرستد، `Cache-Control: no-store` دارد، cache پیش‌فرض پنج ثانیه و سقف یک دقیقه دارد و از cache منقضی‌شده به‌عنوان fallback استفاده نمی‌کند.
- Windows میزبان TCP/QUIC را عرضه می‌کند و fallback آن روی همان PeerId پین‌شده انجام می‌شود. Android فقط direct TCP را انتخاب می‌کند؛ مسیر QUIC در Android هنوز پیاده/انتخاب نشده است.
- گفت‌وگوی عادی Room هنوز در `ConversationMessagesViewModel` پیام کاربر را محلی ذخیره می‌کند و آن را به میزبان نمی‌فرستد. runtime شبکه در مسیر جداگانهٔ درخواست قرار دارد؛ اتصال این runtime به چت عادی همچنان شکاف محصولی است.
- سرویس directory در کد reviewed ذخیره‌سازی پایدار ندارد؛ اما provider/reverse-proxy/network logs خارج از کد repository هستند و retention/deletion آن‌ها در این وظیفه تأیید نشده است.

## دادهٔ قابل مشاهده و نگهداری

- Rendezvous: PeerId، IP و portهای اعلام‌شده، capabilityها، expiry و metadata عملیاتی اتصال در مرز HTTPS. حداکثر عمر اعلان ۲۴ ساعت است؛ مقدار پیش‌فرض ۵ دقیقه است و هر دقیقه نوسازی می‌شود. فقط آخرین اعلان در حافظهٔ process نگهداری می‌شود. cleanup رکورد منقضی هنگام درخواست بعدی انجام می‌شود، نه توسط timer اختصاصی. عدم ثبت metadata در provider یا proxy قابل ادعا نیست.
- Android: cache فراداده در حافظه، پنج ثانیه به‌صورت پیش‌فرض و حداکثر یک دقیقه؛ expiry واقعی peer ممکن است cache را زودتر منقضی کند.
- Windows: شناسه و آدرس خود میزبان، peerId مورد انتظار، وضعیت اتصال و محتوای درخواست پس از رمزگشایی برای inference؛ payload برای انتقال از کانال امن عبور می‌کند، اما میزبان endpoint پردازش‌کننده است و می‌تواند prompt را بخواند.
- دو endpoint مستقیم: هویت peer، آدرس و پورت شبکه و جریان داده را می‌بینند. encryption انتقالی، محتوا را از خود میزبان پردازش‌کننده پنهان نمی‌کند.
- Relay: در نسخهٔ فعلی وجود ندارد؛ در حالت آتی metadataهای مذکور در ADR-0004 برای relay قابل مشاهده خواهند بود و anonymity تضمین نمی‌شود.
- زیرساخت میزبانی: retention لاگ‌های IP، مسیر، زمان و اتصال به پیکربندی deployment وابسته است؛ در مخزن تعیین نشده و pending است.

## گزینه‌های ردشده

- API مرکزی inference یا history: رد شد، چون محتوای درخواست/پاسخ و احتمالاً تاریخچه را در اختیار واسط قرار می‌دهد.
- relay با رمزنگاری خاتمه‌یافته در سرور: رد شد، چون واسط می‌تواند محتوای برنامه را بخواند.
- دو اتصال رمزگذاری‌شدهٔ مستقل که relay بتواند بین آن‌ها داده را رمزگشایی کند: E2E محسوب نمی‌شود و رد شد.
- fallback خودکار به relay پس از شکست P2P: رد شد؛ شکست اتصال مستقیم باید قابل مشاهده باقی بماند.
- الگوریتم رمزنگاری اختصاصی: رد شد؛ از پیاده‌سازی نگهداری‌شدهٔ libp2p استفاده می‌شود.

## تطبیق با وظایف قدیمی

- T0009: ممیزی منبعی محدود؛ گزارش خودش می‌گوید این ممیزی جای اثبات runtime/network نیست.
- T0019: تصمیم معماری direct P2P حفظ شد؛ آن وظیفه پیاده‌سازی transport را ادعا نکرد.
- T0118: direct TCP/QUIC fallback همتا-محور در سمت Windows مستند و آزمون شده؛ NAT traversal عمومی و QUIC در Android اثبات نشده.
- T0119: سیاست relay آتی و visibility فراداده مستند است؛ relay فعال نیست.
- T0120: service و DTO metadata-only هستند؛ C# loopback HTTP acceptance رفتار route، rejection فیلد اضافه و TTL را آزموده است.
- T0129–T0134: کتابخانهٔ رمزنگاری/هویت/اعتماد/کانال امن موجود است؛ در این وظیفه الگوریتم یا مدل trust جدیدی اضافه نشده و وضعیت‌های قدیمی تغییر نکرده‌اند.
- T0206: آزمون C# فیلدهای prompt/context را رد می‌کند و آزمون Android درخواست directory را bodyless GET بررسی می‌کند؛ packet capture و زیرساخت خارجی را اثبات نمی‌کند.
- T0226 و T0227: در گراف اولیه `not_started` باقی مانده‌اند؛ هیچ وضعیت قدیمی به علت این تصمیم تغییر نکرده است. بررسی نهایی privacy/network هنوز نیازمند evidence مستقل است.

## آزمون‌ها و شواهد

- Regression test تازه: `tests/test_update_transport_trust_boundary.py`؛ source-contract assertions برای routeهای metadata، expiry، client cache، direct peer identity، relay-disabled و شکاف عادی chat.
- آزمون اجرایی موجود: `PeerAnnouncementClientAcceptanceTests` در HostLifecycle شامل اجرای HTTP service واقعی روی loopback، announce/discovery، response field whitelist، عدم وجود inference route، رد prompt/context/capsule، TTL renewal/expiry و اتصال peer رمزگذاری‌شده روی loopback.
- آزمون‌های موجود `test_relay_privacy_boundary.py` و `test_p2p_data_path_decision.py` همچنان حفظ شده‌اند.
- Quality Gate روی commit `V 3.1.21`: **PASS**، run 42409561015؛ اصلاح assertion قدیمی در این commit.
- Rendezvous container روی `V 3.1.21`: **PASS**، run 42409560957.
- Product build روی `V 3.1.21`: هنگام ثبت این گزارش در حال اجرا بوده و هنوز نتیجهٔ نهایی آن ثبت نمی‌شود.
- آزمون regression تازهٔ این وظیفه و اجرای دوبارهٔ HostLifecycle پس از تغییرات، تا اجرای CI روی commit این attempt **pending** هستند.
- آزمون packet-level روی deployment واقعی، Android فیزیکی به Windows فیزیکی و AC-P2P-01 در این وظیفه اجرا نشده‌اند و ادعای موفقیت ندارند.

## فایل‌های تغییر‌یافته

- `docs/decisions/ADR-0002-direct-p2p-data-path.md`
- `docs/decisions/README.md`
- `tests/test_update_transport_trust_boundary.py`
- `update/Step three/UPD-S1-002/execution-report.md`
- `update/Step three/UPD-S1-002/evidence/verification.md`
- `update/Step two/project-graph.md`

## نتیجهٔ معیارهای پذیرش

1. مسیر اصلی و سیاست جایگزین مشخص شد: direct authenticated P2P، same-PeerId direct-address fallback در runtimeهای پشتیبان؛ no relay fallback.
2. مرز واسط مشخص شد: DTO/HTTP control plane فقط metadata دارد و unknown sensitive fields رد می‌شوند؛ infrastructure logging خارج از repository صریحاً pending است.
3. metadata و retention هر جزء در ADR ثبت شد؛ limits واقعی و gapهای deployment تفکیک شدند.
4. T0009، T0019، T0118–T0120، T0206 و T0227 تطبیق داده شدند؛ وضعیت‌های legacy تغییر داده نشدند.

**وضعیت وظیفه:** `in_progress` تا موفقیت CI روی commit این تغییر و ثبت SHA/runهای واقعی. این تصمیم، AC-P2P-01 یا تکمیل محصول را نمی‌بندد.

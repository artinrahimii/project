# T0188 — شواهد مدل تهدید و ناشناس‌بودن

## Snapshot و وابستگی‌ها
- شاخه: `main`; مبنای کار: `0a40e409d3818e5256b0cf65b681018c0e16bbda` (`V 3.0.882`).
- گراف در این snapshot با Blob شناسهٔ `47a579a43a13db99c52c9dee0a5abb2117c5f751` دریافت و با بازفرستادن همان محتوای UTF-8 به GitHub Blob API تطبیق داده شد.
- T0187 و T0177 به‌عنوان پیش‌نیازهای موجود بررسی شدند.
- `T0187 → T0188` و `T0177 → T0188`: هر دو `sequencing_only / validated`؛ یال اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شدند.

## شواهد runtime/source که برای نگارش استفاده شد
- قرارداد PeerAnnouncement: PeerId، ۱ تا ۸ نشانی مستقیم، ۱ تا ۳۲ capability، انقضای حداکثر ۲۴ ساعت و رد `/p2p-circuit`.
- دایرکتوری rendezvous: نگهداری درون‌حافظه‌ای فراداده و سقف ۲٬۰۴۸ همتای فعال؛ endpointها صرفاً اعلان و discovery ارائه می‌کنند.
- کلاینت rendezvous در Android و کلاینت Windows به HTTPS محدودند (به‌جز loopback در آزمون).
- Android transport: مستندات کد مسیر مستقیم TCP با NoiseXX و Mplex را بیان می‌کند و PeerId باید مورد اعتماد کاربر باشد.
- Windows transport و PeerSecurityPolicy احراز PeerId و کانال امن را به stack libp2p واگذار می‌کنند.
- ADR-0002 و ADR-0004 مسیر دادهٔ مستقیم و ممنوعیت relay بی‌صدای نسخهٔ اول را تعریف می‌کنند؛ هیچ ادعای packet capture در این وظیفه مطرح نیست.
- T0177 محدودیت host/round و attempt budget را پیاده کرده است؛ این کنترل منبع است، نه ناشناس‌سازی.
- T0187 آزمون‌های محدود به log و temporary artifact تحت مالکیت برنامه دارد؛ این تست‌ها ممیزی کل دستگاه نیستند.
- ADR-0019 و ADR-0020 مرز دید میزبان و ذخیره‌سازی محلی Android را مستند می‌کنند.

## فایل‌های جدید/تغییریافته
- `docs/security.md`
- `tests/test_threat_model_documentation.py`
- `step three/T0187/execution-report.md`
- `step three/T0187/evidence/verification.md`
- `step three/T0188/execution-report.md`
- `step three/T0188/evidence/verification.md`
- `step three/project-graph.md`

## وضعیت آزمون
- آزمون محلی در این محیط اجرا نشده است؛ ابزار این نوبت دسترسی به checkout محلی/مفسر Python ندارد.
- آزمون مستندات و source-contract افزوده شده و باید در CI اجرا شود.
- CI دقیق head پیش از این تغییرات برای T0187 موفق بود؛ آن نتایج به آزمون جدید T0188 تعمیم داده نمی‌شود.
- معیارهای محصول `AC-P2P-01` و `AC-PRIV-01` طبق acceptance ledger هنوز `NOT RUN` هستند.

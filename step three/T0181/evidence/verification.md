# T0181 — شواهد نهایی و ثبت تلاش‌ها

## Snapshot و اعتبار فایل گراف

- commit مبنا: `aaedf55c54292f19f28de62cbf26ea73e976e782`؛ tree: `373eb61d0d2a034c134c7d084b41b99360a446e9`.
- `step three/project-graph.md` در tree همان commit به Blob `03f4331d08f31c17f22404d0880e675bb0ee8757` با اندازهٔ ۳٬۳۵۳٬۴۴۷ بایت اشاره دارد.
- Blob API کامل را برگرداند؛ متن ۳٬۲۲۶٬۵۰۸ نویسه و ۳٬۳۵۳٬۴۴۷ بایت UTF-8 داشت. SHA-1 Git از `blob <length>\0<content>` روی بایت‌های اصلی محاسبه شد و با Blob ID در tree تطبیق یافت.
- JSON کامل پس از حذف حصار Markdown از نسخهٔ جداگانه parse شد؛ ۲۳۰ node و ۴۲۲ edge. نسخهٔ جدید گراف فقط whitespace قالب‌بندی را حذف می‌کند؛ داده‌ها و سوابق قبلی حفظ شده‌اند.

## پیاده‌سازی و آزمون

- `WindowsHostConversationStore.cs`: ذخیرهٔ تاریخچهٔ محدود، نسخه‌بندی‌شده و رمز‌شده با DPAPI؛ نوشتن اتمی و fail-closed.
- WPF: opt-in با رضایت روشن، default-off، بازیابی/ذخیره، opt-out و حذف تاریخچه.
- سیاست و settings: default session-only و Boolean اختیاری؛ تنظیمات قدیمی default-off.
- HostLifecycle runtime DPAPI test، settings round-trip/legacy، Python source guards، WPF smoke و docs به‌روز شدند.
- CI نهایی دقیق V 3.0.840:
  - Step Three quality run 41633714717: 343 تست Python، compileall و graph guard موفق؛ صفر خطا، یک هشدار metadata T0001.
  - Container run 41633714676: production build و non-root HTTP smoke موفق.
  - HostLifecycle job 125909456334: DPAPI local-history runtime test صریحاً PASS.
  - Windows WPF job 125909456784: formatting، Release build با صفر warning/error، startup smoke و DPAPI checks موفق.
  - Android job 125909456733: KtLint، APK، JVM tests، Protobuf Java/Kotlin verification و artifact upload موفق.
- Artifactها: Windows host `13229267898` (14,274,903 bytes)، Android APK `13229473066` (19,531,982 bytes)، Android test reports `13228684447` (51,229 bytes).

## تاریخچهٔ تلاش‌ها

1. `T0181-A001`, commit `0399100e6f2f63fbfa35bc8d178208608a920730`: یک assertion منبع نادرست در Python quality gate شکست خورد (342/343).
2. `T0181-A002`, commit `659aebf46077d9b7cd9aaecc6ff12ed2b1da6a78`: Python gate/HostLifecycle/Android موفق؛ WPF به علت missing namespace و CS0246 شکست خورد.
3. `T0181-A003`, commit `aaedf55c54292f19f28de62cbf26ea73e976e782`: import افزوده شد؛ تمام workflowهای نهایی موفق.

## یال‌های ورودی

- T0180 → T0181: bufferهای انتقال/decoder مصرف نمی‌شوند؛ sequencing_only.
- T0177 → T0181: selector/retry budget اندروید مصرف نمی‌شود؛ sequencing_only.
- در هر دو یال type=blocks حفظ شد؛ rationale و evidence ثبت شدند.

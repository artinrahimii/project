# UPD-S1-010 — شواهد CI دقیق تلاش A011

## مبنای اجرا
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit دقیق: `8e784ad4596412fd811bfb261f62d12ff0644589` — `V 3.1.174`
- اجرای product workflow: 2026-11-10 17:38:07Z تا 17:47:27Z.

## مسیر محصول بررسی‌شده
- `ConversationDetailScreen.kt`: ارسال از صفحهٔ گفت‌وگوی موجود، رضایت حریم خصوصی، تأیید trust، نمایش وضعیت، لغو و retry.
- `RequestViewModel.kt`: انتخاب خودکار مدل، ساخت درخواست، انتخاب میزبان trusted با probe تازه، dispatch و دریافت پاسخ correlate‌شده.
- `AndroidInferenceRequestRuntime.kt`: runtime تولیدی متصل به identity/trust اندروید، discovery احرازشده و `Libp2pInferenceRequestTransport`.
- `ConversationMessagesViewModel.kt`: ذخیرهٔ پیام کاربر، پاسخ assistant و اعلان خطای system در همان conversation؛ شناسهٔ پاسخ بر اساس conversation/request/attempt/role deterministic است.
- `InferenceRequestLifecycleRepository.kt`: lifecycle درخواست با `conversationId` مستقل می‌شود.
- آزمون‌ها: `ConversationMessagesViewModelTest.kt`، `ConversationWorkflowAcceptanceTest.kt`، `RequestViewModelTest.kt` و قراردادهای Python صفحهٔ چت.

## نتایج CI
1. [Step Three quality gate — 42866959118](https://github.com/artinrahimii/project/actions/runs/42866959118): **موفق**؛ 493 آزمون Python، `compileall` و graph_guard برای گراف Update و Legacy در هر دو حالت ساختار و مسیرهای خروجی/شواهد.
2. [White-P product build — 42866959220](https://github.com/artinrahimii/project/actions/runs/42866959220): **موفق**؛ هر سه job با نتیجهٔ success.
   - Windows WPF: restore/build تمیز، format، publish، startup smoke، تولید Protobuf و installer acceptance.
   - Android: KtLint، debug APK، release variant با کلید موقت CI و حذف آن، JVM tests/coverage، Keystore instrumentation و launch smoke روی emulator میزبانی‌شده.
   - HostLifecycle: build و آزمون integration؛ markerها شامل admission مدل/ظرفیت، Unknown/Insufficient capacity، لغو/deadline، trust/transport و پذیرش دقیقاً یک inference از 64 درخواست هم‌زمان.
3. [Rendezvous container build and smoke — 42866959693](https://github.com/artinrahimii/project/actions/runs/42866959693): **موفق**.

## دامنهٔ اثبات
- CI فقط ساخت و آزمون runnerهای میزبانی‌شده و Android emulator را اثبات می‌کند.
- هیچ گوشی فیزیکی، میزبان Windows متعلق به کاربر، مدل محلی واقعی یا مسیر شبکهٔ فیزیکی Android-to-Windows در این اجرا استفاده نشد.
- clean install و in-place upgrade انجام نشدند.
- prompt، پاسخ، کلید، token یا PeerId حساس در گزارش وارد نشده‌اند.

# T0190 — شواهد گزارش وضعیت بدون محتوای پیام

## Snapshot و اعتبارسنجی مبنا
- Branch: `main`.
- Base commit: `bcec43249466d88015c2807701f9ec873b7caa27` (`V 3.0.890`).
- Graph path: `step three/project-graph.md`.
- Graph Blob SHA: `5e228afd442578fdbe4fea79e5c56cda62db219f`; اندازهٔ ثبت‌شده در GitHub برابر ۳٬۴۹۰٬۳۶۸ بایت بود و متن کامل JSON با موفقیت parse شد.
- Node/edge counts: ۲۳۰ / ۴۲۲.
- T0188 و T0189 در این snapshot `done` هستند.
- ورودی‌های T0190 هر دو `blocks` اصلی خود را حفظ می‌کنند و به `sequencing_only / validated` با rationale/evidence ثبت‌شده تغییر طبقه‌بندی داده‌اند.

## شواهد مسئلهٔ موجود در کد
- `src/Host.Application/HostStatusPresentation.cs`: نسخهٔ قبلی `FormatBackendStatus` مقدار `status.Message` را مستقیماً به خروجی UI الحاق می‌کرد.
- `src/Host.Windows/MainWindow.xaml.cs`: مسیر `BackendCapabilityStatus` برای `HostState.Error` متن `status.Message` را نمایش می‌داد.
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`: نسخهٔ قبلی `networkStatusMessage` و `hostAvailabilityMessage` را مستقیم در ناحیهٔ وضعیت رندر می‌کرد.

## پیاده‌سازی و آزمون‌های تغییر یافته
- Windows presenter اکنون فقط از `HostState` و متن ثابت استفاده می‌کند؛ `HostStatus.Message` در status presenter خوانده نمی‌شود.
- Windows capability error copy اکنون ثابت است و جزئیات آزاد backend را نمایش نمی‌دهد.
- Android presenter جدید `toNetworkInferenceStatusPresentation()` فقط وضعیت شبکه/peer و چرخهٔ inference را به برچسب‌های ثابت تبدیل می‌کند.
- Android UI دیگر `networkStatusMessage` و `hostAvailabilityMessage` را در status panel رندر نمی‌کند.
- آزمون Android یک prompt/status-message sentinel را وارد می‌کند و assert می‌کند که هیچ‌کدام از دو خروجی status آن را بازتاب نمی‌دهند.
- آزمون Windows همان sentinel را در `HostStatus.Message` برای همهٔ stateها قرار می‌دهد و عدم حضور آن در backend/network presentation را assert می‌کند.
- Python source contracts تضمین می‌کنند مسیرهای UI به نمایش متن آزاد بازنگشته‌اند.

## وضعیت آزمون
- آزمون محلی در این محیط اجرا نشده است.
- نتیجهٔ KtLint، Android JVM، Windows HostLifecycle، WPF build و Step Three quality gate برای commit نهایی هنوز pending است.
- تا دریافت CI exact-head، T0190 تکمیل‌شده تلقی نمی‌شود.

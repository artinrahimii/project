# T0179 — لاگ ساختاریافته با allowlist فیلدهای مجاز

- **وضعیت این ثبت:** `in_progress`؛ پیاده‌سازی ایجاد شده و CI دقیق commit هنوز باید بررسی شود.
- **شاخهٔ پایه:** `main`
- **commit پایه:** `d01a68a958da8e02ec869fd6a0ea975d66444fcc` (`V 3.0.815`)
- **تلاش:** `T0179-A001`؛ پیاده‌سازی allowlist عددی، اتصال به مسیرهای transport و افزودن آزمون‌های regression/acceptance.
- **نتیجهٔ آزمون در زمان ثبت:** اجرای محلی .NET/Gradle در این نشست در دسترس نیست؛ هیچ آزمونی به‌عنوان پاس‌شده ثبت نشده است. کنترل‌های CI دقیق commit باید قبل از `done` بررسی شوند.

## بررسی وابستگی‌ها

- `T0178 → T0179`: به `sequencing_only` اعتبارسنجی شد. سیاست `HostConversationRetentionPolicy` و مجموعهٔ حافظه‌ای `ChatConversation` قرارداد logging یا خروجی مصرفی برای این کار تولید نمی‌کنند.
- `T0177 → T0179`: به `sequencing_only` اعتبارسنجی شد. آزمون انتخاب میزبان و سقف تلاش، API یا خروجی مصرفی برای ثبت رویداد تشخیصی ایجاد نمی‌کند.
- در هر دو مورد، `type: blocks` و ترتیب محافظه‌کارانهٔ گراف حفظ شده‌اند. هیچ یالی حذف نشده و ترتیب اجرا تغییر نکرده است. دلیل و شواهد در `edge_change_log` گراف ثبت شده‌اند.

## پیاده‌سازی واقعی

- `src/Shared.Security/SafeDiagnosticLog.cs`: EventSource با دو enum ثابت رویداد/نتیجه، مدت‌زمان عددی، اعتبارسنجی مقادیر و سقف ۸۰ رکورد در هر فرایند.
- `src/Host.Windows/Networking/Libp2pPeerTransport.cs`: ثبت شروع و نتیجهٔ اتصال peer فقط با کدهای ثابت و مدت‌زمان؛ شناسه و address منتشر نمی‌شود.
- `src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt`: رکورد JSON عددی با فیلدهای ثابت `schema`، `event_code`، `outcome_code` و `duration_ms`؛ سقف ۸۰ رکورد.
- `Libp2pInferenceRequestTransport.kt`: ثبت شروع، موفقیت، رد و لغو submit؛ مقدار داخلی `lastSubmissionFailure` و متن exception به log فرستاده نمی‌شود.
- fixture آزمون .NET/Android دیگر category، قالب پیام یا نوع exception را منتشر نمی‌کند و فقط سطح و شناسهٔ عددی رویداد را می‌نویسد.
- آزمون‌های Python، EventSource و Android برای قرارداد allowlist افزوده یا اصلاح شده‌اند.
- `docs/security.md` دامنهٔ واقعی و محدودیت ادعا را تشریح می‌کند.

## محدودیت و ادامه

- پیاده‌سازی به‌تنهایی ثابت نمی‌کند که سیستم‌عامل، pagefile/crash dump، فرایندهای خارجی یا کتابخانه‌های ثالث هیچ داده‌ای ثبت نمی‌کنند.
- آزمون‌های محلی در این نشست اجرا نشده‌اند؛ وضعیت کار تا مشاهدهٔ نتیجهٔ CI دقیق commit، `in_progress` باقی می‌ماند.
- پس از push، گردش‌های Step Three quality gate و White-P product build را برای همان SHA بررسی کن. فقط پس از موفقیت همهٔ آزمون‌های مرتبط، وضعیت را به `done` تغییر بده؛ در غیر این صورت شکست را در تلاش جدید نگه دار.

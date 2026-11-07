# ممیزی خط مبنای CI برای UPD-S1-003

- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit خط مبنا: `28594b1fd1c706069748e63fecc482f3634dd296` — `V 3.1.28`
- workflow quality run: `42412981586`
- workflow product build run: `42412981632`

## نتایج CI روی خط مبنا

- Python quality: **FAIL**؛ ۴۶۴ آزمون اجرا شد و ۸ آزمون قرارداد درخواست/انتخاب/بودجه/retry شکست خورد.
- Android build: **FAIL** در ktlint؛ workflow پیش از build/unit tests متوقف شد.
- Windows WPF build: **PASS**.
- Host lifecycle unit/integration: **PASS**.

## علت‌های مشخص

۱. آزمون‌های قدیمی هنوز `FirstReleaseInferenceLimits` را سه میزبان در هر round و نه میزبان در کل درخواست می‌دانستند، درحالی‌که runtime تولیدی `MAX_ACTIVE_HOSTS_PER_REQUEST=1` را اعمال می‌کرد.
۲. اعتبارسنجی `DistributedRequest` هنوز حد generic nine-host را می‌پذیرفت.
۳. default factory برای `maxTotalHosts` هنوز از ثابت قدیمی سه میزبان استفاده می‌کرد.
۴. تست UI انتظار سه attempt هم‌زمان و متن‌های قدیمی retry را داشت.
۵. ktlint در `AndroidInferenceRequestRuntime.kt` و `DistributedRequest.kt` دو ایراد قالب‌بندی گزارش کرد.

## اصلاحات این اجرا

- اعتبارسنجی domain و default factory با سقف واقعی first release هم‌راستا شد: سه round، یک میزبان در هر round، سه attempt در کل.
- سه‌میزبانه‌بودن generic selector/dispatcher به‌عنوان compatibility صریحاً حفظ می‌شود.
- تست‌های UI retry/cancel از سه درخواست هم‌زمان به یک میزبان و یک fallback ترتیبی تغییر می‌کنند.
- پنج source-contract guard با تفکیک first-release و compatibility بازنویسی شدند.
- دو تخلف ktlint اصلاح شدند.

## محدودیت

این فایل صرفاً گزارش run شکست‌خوردهٔ مبنا و تغییرات کد منبع است. نتیجهٔ CI برای commit اصلاحی تا زمان اجرای workflow پس از push، **در انتظار** است. این گزارش هیچ ادعایی دربارهٔ اجرای دستگاه واقعی یا E2E محصول ندارد.

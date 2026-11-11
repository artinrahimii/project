# شواهد ممیزی تخصیص میزبان و retry

## سیاست
- `MAX_ROUNDS = 3`
- `MAX_ACTIVE_HOSTS_PER_REQUEST = 1`
- `MAX_TOTAL_HOSTS = 3`
- سقف generic سه میزبان فقط برای مسیر compatibility است.

## dispatch و retry
- انتخاب زندهٔ runtime با سقف یک میزبان انجام می‌شود و dispatch بیش از یک میزبان را رد می‌کند.
- budget/progress پیش از ارسال کنترل می‌شوند؛ attempt ID یکتا و request ID منطقی پایدار است.
- retry فقط با اقدام صریح UI انجام می‌شود و هنگام ارسال/انتخاب/لغو فعال مسدود است.
- تنها میزبان ناموفق برای retry بعدی باقی می‌ماند؛ deadline و سقف round/attempt اعمال می‌شود.
- timeout یک attempt را terminal می‌کند؛ retry خودکار پنهان وجود ندارد.
- Windows delivery gate duplicate و ambiguous response send را fail-closed می‌کند.

## آزمون‌های ردیابی‌شده
آزمون‌های Python مربوط به budget و مرز retry، JVM مربوط به selection/dispatch/manual retry/correlation و آزمون‌های .NET مربوط به replay/tamper/busy/concurrency/response delivery از قبل در مخزن وجود دارند. آزمون قراردادی `tests/test_update_s1_015_retry_policy.py` اتصال این منطق به مسیر محصول و آزمون‌های JVM را کنترل می‌کند.

این سند ممیزی source/test است، نه نتیجهٔ CI؛ آزمون‌های exact-head پس از commit لازم‌اند.

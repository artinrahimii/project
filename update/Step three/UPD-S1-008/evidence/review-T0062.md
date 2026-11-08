# بازبینی محتوایی T0062 — مدیریت timeout، خطای فرایند، قطع ارتباط و پاسخ ناقص

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `bc9882a8ad1fcd29b6a2e42e31509a76e926f2f6` — `V 3.1.78`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد.
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Infrastructure/LlamaCppServerBackend.cs` — `8112e58e9a952fa41415acee70bc8ca3baf7e654`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`
- `step three/T0062/execution-report.md` — `b1d8ac42586e32f4f046a0f56d08ffefde8389d4`

## یافته‌های فنی
- backend timeout داخلی را به TimeoutException، قطع transport را به HttpRequestException امن و JSON/response ناقص را به InvalidDataException تبدیل می‌کند؛ caller cancellation از timeout جداست.
- Program.cs regressionهای timeout، اتصال قطع‌شده، malformed/truncated JSON و عدم افشای جزئیات transport را پوشش می‌دهد. process-kill integration جداگانه برای T0062 ادعا نشده و متعلق به T0063 است.
- HostLifecycle regression در run 40078502247 و real local inference در 40078502151 موفق ثبت شده‌اند.

## علت بازبینی مجدد
- UPD-S1-023 ممکن است قرارداد typed error و مصرف‌کنندگان آن را تغییر دهد؛ پس از یکپارچه‌سازی resolver، callerها و error mapping باید دوباره بازبینی شوند.

## شواهد و محدودیت CI
- Exact V3.1.78 Quality: [run 42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) passed.
- Exact V3.1.78 Rendezvous: [run 42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) passed.
- Product build [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) was still in progress during this review.
- Historical live local-model evidence: run 40077575618 (T0061), 40078502151 (T0062), and 40092245411 (T0063/T0064) on older exact implementation commits; these are not V3.1.78 results.
- T0065 is a source-level provider boundary check, not packet capture or runtime egress proof.
- No physical Android-to-Windows E2E or install/upgrade test was performed in this review.

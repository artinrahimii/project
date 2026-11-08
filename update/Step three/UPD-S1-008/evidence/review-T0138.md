# بازبینی محتوایی T0138

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: c4b7076392dba6034db3d22200f8d9ab86419814 — V 3.1.86
- وضعیت پایه: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-007, UPD-S1-016
- تصمیم: unchanged
- review_status: reviewed
- review_commit_sha: c4b7076392dba6034db3d22200f8d9ab86419814

## یافته‌ها
ADR-0019, README, requirements and acceptance documents consistently disclose that InferenceRequest.prompt and context_capsule are readable by the receiving processing host after transport decryption; transport encryption is not described as end-to-end encryption against that endpoint. HostContentVisibility tests check protocol fields, host parsing and aligned docs. Android MainActivity repeats the disclosure beside the prompt field, in the pre-trust dialog and on the Privacy page. This legacy task is specifically the explicit content-visibility disclosure, which is already accurate and tested; the unchanged decision does not claim every separate UPD-S1 consent/onboarding requirement has passed.

## فایل‌های بررسی‌شده
- docs/decisions/ADR-0019-host-visible-inference-content.md
- README.md
- docs/requirements.md
- docs/acceptance.md
- src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt
- tests/test_host_content_visibility.py
- tests/test_claims_audit.py

## وابستگی‌ها
- T0137 -> T0138 sequencing_only/validated
- T0138 -> T0139 contract_or_interface_dependency/validated
- T0138 -> T0141/T0142/T0143/T0144/T0145 sequencing_only/validated
- هیچ وضعیت پایه یا تاریخچه‌ای در گراف اولیه تغییر نکرد.

## CI و شواهد
- V3.1.86 Step Three Quality Gate: https://github.com/artinrahimii/project/actions/runs/42509600997 — موفق.
- V3.1.86 Rendezvous container: https://github.com/artinrahimii/project/actions/runs/42509601113 — موفق.
- V3.1.86 HostLifecycle: https://github.com/artinrahimii/project/actions/runs/42509601072 — موفق؛ Android و Windows jobs در زمان snapshot مبنا هنوز در حال اجرا بودند.
- V3.1.82 full Product Build: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ Android گزارش 226 passed، 1 skipped، 0 failed از 227 tests داشت.

## اقدام بعدی
Reopen if the approved data-visibility boundary changes. Track first-request consent, retention and final transport behavior separately under UPD-S1-007/011/018.

## محدودیت شواهد
این بازبینی اثبات نصب فیزیکی Android/Windows، پاسخ inference واقعی در Room، clean install یا in-place upgrade نیست.

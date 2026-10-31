# T0136 — شواهد و اعتبارسنجی

## منبع و نسخه

- Branch: `main`
- Commit بررسی‌شده: `2b64a70c59c1d02c9d30d6ef510e6952b89cdb14` — `V 3.0.477`
- Tree: `7b0aa45cb6c4e5beec99639cb5a0da2d3e64fbd5`; graph Blob همان tree: `ef4c06e5282570e6437f1ebb39b25ab7f44f70d6`.
- `src/Host.Windows/Requests/SecureRequestIdGenerator.cs`: SHA `b896574bf7a956c6264b566c38b3476ce8ae2e12`, 604 bytes.
- `tests/HostLifecycle.Tests/SecureRequestIdGeneratorTests.cs`: SHA `8f6ac0511d0ea05c2bb119c66b233963a52449c1`, 1237 bytes.
- `tests/HostLifecycle.Tests/Program.cs`: SHA `b037736269d8e2a6e0572eb4c80e406f7d87f77b`, 74038 bytes.

## CI اجراشده

- HostLifecycle unit/integration روی دقیقاً `V 3.0.477`: **PASS**, job 122979781847 — [لاگ job](https://github.com/artinrahimii/project/actions/runs/40734640359/job/122979781847).
- خط صریح لاگ: `PASS: generated 4096 temporary request IDs using a cryptographic RNG; format and sample uniqueness checks passed.`
- Step Three quality gate روی دقیقاً `V 3.0.477`: **PASS** — [run 40734640272](https://github.com/artinrahimii/project/actions/runs/40734640272).

## نتیجهٔ وابستگی‌ها

- `T0135 → T0136` در گراف فعال ماند؛ classification `sequencing_only`, validation `validated`.
- `T0128 → T0136` از گراف فعال حذف شد با classification `sequencing_only`, validation `validated`. خود T0128 همچنان `not_started` است.
- این وظیفه بر پایهٔ کد واقعی CSPRNG و اجرای موفق آزمون در CI تکمیل شد، نه صرف وجود فایل یا فقط تولید مستندات.

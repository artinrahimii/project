# گزارش اجرای UPD-S1-020 — اعتبارسنجی انتها‌به‌انتها روی دستگاه واقعی

## تلاش A001 — ارزیابی پیش‌شرط‌های فیزیکی

- وضعیت: blocked.
- commit مبنای بررسی: cf042d3f7634a1a113280a5f50257c354feb2117 — V 3.1.193.
- پیش‌نیاز UPD-S1-019 انجام نشده است.
- دستگاه فیزیکی Android، میزبان Windows واقعی و مدل محلی با SHA-256 در محیط اجرایی در دسترس نیستند.
- هیچ emulator، fixture، test peer، پاسخ ثابت یا backend مستقل به‌عنوان جایگزین آزمون محصول استفاده نشد.
- مسیر اجباری Android Chat UI → P2P → Windows Host → Real local model → same Room conversation اجرا نشد.
- هیچ تست فیزیکی، inference واقعی یا ذخیره‌سازی پاسخ واقعی در Room اجرا نشده است.

## شرط ادامه

Keep blocked until UPD-S1-019 is done and a physical Android device plus an installed Windows host with an actual locally loaded model (with model revision and SHA-256) are available for coordinated end-to-end testing. Then execute the prescribed Android Chat UI → Android runtime → approved P2P → Windows product → real local backend/model → same Room conversation path and all required failure scenarios.

## شواهد

- update/Step three/UPD-S1-020/evidence/physical-device-access-blocker-A001.md
- GitHub Actions CI V 3.1.191 فقط build/unit/instrumentation runner evidence است و E2E فیزیکی محسوب نمی‌شود.

## تغییر برنامه‌ریزی — انتقال آزمون فیزیکی به مرحلهٔ پایانی

- وضعیت فعلی این وظیفه در گراف از `blocked` به `deferred_final` تغییر کرده است؛ تلاش A001 و گزارش عدم دسترسی به محیط فیزیکی حفظ شده‌اند.
- آزمون Android-to-Windows با مدل واقعی هنوز اجرا نشده است؛ هیچ emulator، fixture یا CI به‌عنوان جایگزین پذیرفته نشده و این وظیفه `done` نیست.
- شرط شروع پذیرش نهایی: UPD-S1-019 تکمیل شده باشد، UPD-S1-011 endpoint عمومی و آزمون شبکهٔ مستقل را بسته باشد، و گوشی Android واقعی، میزبان Windows واقعی و مدل محلی دارای revision و SHA-256 فراهم باشند.
- کارهای آماده‌سازی UPD-S1-021 مانند اسناد، بسته‌های candidate، checksum و دارایی‌های آزمون ارتقا می‌توانند پیشاپیش انجام شوند؛ اما UPD-S1-021 و انتشار تا عبور آزمون فیزیکی و شواهد نصب/ارتقای هر دو پلتفرم نباید بسته شوند.

# بازبینی T0201 — inference واقعی با llama.cpp و GGUF

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70`، `V 3.1.102`
- کد تاریخی اجراشده: `a074815979aff371650c5101c514989f42180968`، `V 3.0.922`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وضعیت بازبینی: `reviewed`
- UPDهای مرتبط: UPD-S1-019، UPD-S1-020

## اجرای واقعی ثبت‌شده

- workflow `.github/workflows/real-inference.yml` روی Windows runner واقعی اجرا شده است: run [41886745964](https://github.com/artinrahimii/project/actions/runs/41886745964)، نتیجه `success`، head `a074815979aff371650c5101c514989f42180968`.
- llama.cpp CPU archive نسخهٔ b11443 با SHA-256 برابر `98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6` تأیید شده است.
- باینری `llama-server.exe` SHA-256 برابر `4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968` داشته است.
- مدل `SmolLM2-135M-Instruct.Q4_K_M.gguf` از revision `a6b7826ed969c3926a98359e39912af137b464cb` دانلود و GGUF magic تأیید شده؛ اندازه 105454144 بایت و SHA-256 برابر `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684` ثبت شده است.
- log اجرا می‌گوید inference واقعی llama.cpp پاسخ تولید کرده، cancellation حین generation مشاهده شده و process متعلق به آزمون cleanly متوقف شده است.

## مرز پذیرش

این شاهد، بارگذاری مدل واقعی و inference در Windows GitHub runner است؛ میزبان Windows نصب‌شدهٔ کاربر، pairing با Android، پاسخ به اپ Android یا persistence در Room را اثبات نمی‌کند. workflow از loopback endpoint استفاده می‌کند.

## تصمیم

وضعیت پایهٔ `done` حفظ شد؛ `re_review_required` به‌دلیل تغییرات احتمالی مدل/backend و الزام UPD-S1-019/020 ثبت می‌شود. آزمون دوباره روی کد جاری و سپس Android-to-Windows فیزیکی همچنان لازم است.

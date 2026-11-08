# بازبینی T0202 — Android فیزیکی به Windows host روی LAN غیرloopback

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70`، `V 3.1.102`
- وضعیت پایه در گراف قدیمی: `deferred_final`؛ بدون تغییر.
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- UPDهای مرتبط: UPD-S1-004، UPD-S1-010، UPD-S1-019، UPD-S1-020

## آزمون و زیرساخت بررسی‌شده

- `AndroidWindowsLanIntegrationTest.kt` برای اجرای دستی host address و non-loopback interface طراحی شده است.
- `Invoke-AndroidWindowsLanAcceptance.ps1` و workflow `.github/workflows/android-windows-lan-acceptance.yml` runner ویندوز self-hosted با label `whitep-lan` و Android فیزیکی متصل به همان RFC1918 LAN را لازم دارند.
- `AndroidInteropPeerServer` bind صریح IPv4 غیرloopback و address assigned-to-host را اعتبارسنجی می‌کند؛ default fixture در CI loopback است.
- گزارش تاریخی T0202 تصریح می‌کند workflow دستی اجرا نشده و هیچ artifact احراز‌شدهٔ LAN وجود ندارد. task عمداً به final external-acceptance queue منتقل شده و status آن `deferred_final` است.
- فهرست اجراهای workflow دستی در زمان بررسی هیچ اجرای موفق فیزیکی ثبت نکرده است.

## مانع واقعی و شرط رفع

مانع، نبود runner `whitep-lan` با دستگاه Android فیزیکی روی همان شبکه است. برای رفع، باید runner ثبت شود، برنامه روی دستگاه نصب شود، workflow دستی در `main` اجرا شود و artifact حاوی شواهد redacted از authenticated request/response و disconnect بررسی شود.

## تصمیم

وضعیت پایهٔ `deferred_final` حفظ شد؛ این کار reviewed است، اما acceptance انجام نشده و task complete محسوب نمی‌شود. تصمیم `re_review_required` به‌منظور بازاعتبارسنجی پس از آماده‌شدن محیط فیزیکی ثبت می‌شود. هیچ ادعای LAN pass وجود ندارد.

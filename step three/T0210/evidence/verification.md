# شواهد راستی‌آزمایی T0210

- commit مبنا: eb8c7644317dfde643b2526b92e2300483d625ea (V 3.0.958).
- Product Build: https://github.com/artinrahimii/project/actions/runs/42007819870
- Windows job: https://github.com/artinrahimii/project/actions/runs/42007819870/job/127178931985
- Installer artifact: https://github.com/artinrahimii/project/actions/runs/42007819870/artifacts/13404924326

## شواهد
- Windows job گام Build and acceptance-test Windows installer را با موفقیت اجرا کرد.
- Log: PASS: installed White-P registration reports publisher 'Artin Rahimi' for the isolated install location.
- Log: PASS: installer compiled, installed, smoke-launched, included notices, and uninstalled.
- tools/build_windows_installer.ps1 فایل WhiteP.Host.Windows.exe نصب‌شده را با --smoke-test اجرا و exit code را اعتبارسنجی می‌کند.
- src/Host.Windows/App.xaml.cs در smoke منتظر MainWindow واقعی، IsLoaded و IsVisible می‌ماند؛ تست tests/test_wpf_entrypoint.py نیز همین قرارداد را کنترل می‌کند.
- پس از uninstall، نبود پوشهٔ نصب و نبود registration متناظر تأیید شدند.

## یال‌های بررسی‌شده
- T0209 -> T0210: hard_dependency / validated؛ executable نصب‌شده از بستهٔ T0209 تأمین می‌شود.
- T0208 -> T0210: sequencing_only / validated؛ artifact پوشش T0208 مصرف نمی‌شود.
- type اصلی blocks برای هر دو یال حفظ شد.

# UPD-S1-022 — گزارش اجرای وظیفه

**وضعیت فعلی:** `in_progress`؛ پیاده‌سازی و آزمون‌های موجود در خط مبنای دقیق تأیید شده‌اند؛ اعتبارسنجی گراف پس از ثبت این گزارش هنوز باید در CI همین commit انجام شود.
**مبنای بررسی:** `main` در commit `0eeb9a4ba0ebabb4e689c8a07442558f80b8c675` (`V 3.1.147`).
**عملیات منبع:** `modify`.
**نتیجهٔ ممیزی محصول:** کد لازم برای هویت پایدار، trust صریح، reconnect و revoke در هر دو پلتفرم موجود بود؛ شکاف پذیرش باقی‌مانده‌ای که نیازمند افزودن پیاده‌سازی موازی باشد مشاهده نشد. هیچ منطق trust یا هویت تکراری ساخته نشد.

## دامنه و تصمیم

سیاست نسخهٔ اول حفظ شد: PeerId ناشناخته بدون اقدام صریح کاربر پذیرفته نمی‌شود؛ اعتماد ذخیره‌شده برای اتصال مجدد به همان هویت استفاده می‌شود؛ هویت خراب یا دستکاری‌شده خودکار بازتولید نمی‌شود؛ و revoke، همان PeerId را از فهرست اعتماد حذف می‌کند. تغییر PeerId یا mismatch بین هویت مورد انتظار، multiaddress و هویت احرازشده موجب رد اتصال می‌شود.

Android هویت کلید libp2p را با کلید محافظت‌شده در Android Keystore نگه می‌دارد و trust را در رکورد جداگانهٔ محافظت‌شده ذخیره می‌کند. Windows هویت میزبان و فهرست trust را با DPAPI کاربر جاری محافظت می‌کند. برای رمزنگاری جدید یا الگوریتم اختصاصی تغییری ایجاد نشد.

## تحقق معیارهای پذیرش

1. **عدم تأیید تکراری برای هر درخواست:** تصمیم trust در Android پایدار است و runtime آن را در درخواست‌های بعدی بررسی می‌کند؛ enrollment دوباره برای PeerId ذخیره‌شده لازم نیست.
2. **عدم اعتماد خودکار به PeerId ناشناخته:** کشف همتا به‌تنهایی trust ایجاد نمی‌کند. UI Android برای هویت جدید، PeerId دقیق را همراه با هشدار قابل‌مشاهده نشان می‌دهد و فقط اقدام صریح «Trust this host» تصمیم را ذخیره می‌کند. Windows نیز برای هویت راه‌دور تأیید مستقل و با پاسخ پیش‌فرض No می‌گیرد.
3. **هویت پایدار، mismatch و revoke:** Android Keystore و Windows DPAPI هویت محلی را پایدار نگه می‌دارند و در برابر رکورد خراب fail-closed هستند. گیت transport هویت مورد انتظار و PeerId موجود در آدرس را تطبیق می‌دهد. revoke در Android و Windows پیاده‌سازی شده و آزمون reconnect پس از revoke رد اتصال را تأیید می‌کند.
4. **سازگاری و آزمون دو پلتفرم:** آزمون‌های Android برای enrollment، ذخیره، revoke و tamper؛ و آزمون‌های Windows/.NET برای هویت DPAPI، trust، mismatch، revoke، حذف آخرین رکورد و رد reconnect موجود و در CI اجرا شده‌اند.
5. **عدم نمایش هویت غیرضروری در گفت‌وگوی معمولی:** شناسهٔ همتا فقط در مسیر اختصاصی انتخاب/اعتماد میزبان و پنجرهٔ تأیید مربوط نمایش داده می‌شود؛ صفحهٔ گفت‌وگوی محلی/Room مسیر مستقل دارد و PeerId در کارت پیام عادی درج نشده است.

## کد و آزمون‌های بررسی‌شده

- Android: `AndroidPeerIdentityStore.kt`, `AndroidPeerTrustStore.kt`, `AndroidInferenceRequestRuntime.kt`, `RequestViewModel.kt`, `MainActivity.kt`.
- Windows و مشترک: `WindowsHostIdentityProvider.cs`, `PeerTrustStore.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Libp2pPeerTransport.cs`.
- تصمیم مشترک: `ADR-0018-explicit-peer-trust-and-identity-verification.md`.
- آزمون‌ها: `AndroidPeerIdentityStoreTest.kt`, `AndroidPeerTrustStoreTest.kt`, `RequestViewModelTest.kt`, `Libp2pInferenceRequestTransportTest.kt`, `WindowsHostIdentityStoreAcceptanceTests.cs`, `PeerTrustStoreAcceptanceTests.cs`, `WindowsPeerTrustStoreAcceptanceTests.cs`, `PeerTransportContractAcceptanceTests.cs`, `test_network_listener_exposure.py` و `test_update_transport_trust_boundary.py`.

## CI روی commit مبنا

- Quality gate [run 42790619781](https://github.com/artinrahimii/project/actions/runs/42790619781): اجرای unittest، compileall و هر دو حالت graph_guard برای گراف قدیمی و گراف به‌روزرسانی موفق.
- Product build [run 42790619479](https://github.com/artinrahimii/project/actions/runs/42790619479): jobهای HostLifecycle، Android و Windows همگی موفق. Android lint، build تمیز debug/release، JVM tests، Keystore instrumentation و بررسی permissionها موفق؛ Windows build/publish/smoke و آزمون DPAPI host identity/peer trust و installer موفق؛ HostLifecycle از خروجی تمیز build و اجرا شد.
- Real local inference [run 42790619433](https://github.com/artinrahimii/project/actions/runs/42790619433): اجرای pinned llama.cpp و مدل GGUF واقعی در job مستقل موفق. این نتیجه آزمون trust را تقویت نمی‌کند و آزمون Android-to-Windows فیزیکی نیست.
- Rendezvous [run 42790619836](https://github.com/artinrahimii/project/actions/runs/42790619836): ساخت container و smoke قرارداد HTTP موفق.

## محدودیت شواهد

CI از runner ویندوز و emulator اندروید استفاده کرده است. دستگاه فیزیکی Android متصل به نصب واقعی Windows Host در اختیار این اجرا نبوده است؛ بنابراین آزمون محصول واقعی Android-to-Windows، تولید پاسخ مدل در همان مسیر و ذخیرهٔ آن در Room ادعا نمی‌شود. این موارد gate مستقل `UPD-S1-020` هستند. clean install و in-place upgrade نیز جزو `UPD-S1-021` هستند و در این وظیفه اجرا نشده‌اند.

## تغییرات این تلاش

در کد محصول تغییری ایجاد نشد؛ implementation موجود، آزمون‌های آن و آخرین commit محصول بررسی شدند. این تلاش فقط گزارش، شواهد ممیزی و وضعیت گراف را ثبت می‌کند. commit محصول مبنای شواهد `0eeb9a4ba0ebabb4e689c8a07442558f80b8c675` است. وضعیت تا دریافت نتیجهٔ CI اعتبارسنجی گراف در `in_progress` باقی می‌ماند.

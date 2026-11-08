# بازبینی T0206 — metadata-only discovery و عدم افشای prompt/capsule به rendezvous

- مبنای main: `168a1758094e88f3d1a9a2d86fde31136cdac916`، `V 3.1.104`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم reconciliation: `unchanged`
- UPDهای مرتبط: UPD-S1-002، UPD-S1-007، UPD-S1-016، UPD-S1-018، UPD-S1-019، UPD-S1-020

## تطبیق Blob SHA منبع

این چهار فایل از source baseline تاریخی `a074815979aff371650c5101c514989f42180968` تا main جاری Blob SHA یکسان دارند:

- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`: `9218f1c05f281434dad3b940f7f925d86b54b91b`
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs`: `557f418edf6c0a2c186bf2ffb2af5aa93d59239f`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`: `2b93c0de23b398643a275e2b4e9cbe06200cce17`
- `tests/test_peer_inference_privacy_disclosure.py`: `7cebe8921e06b9948b0b473a94a954d84a9801cc`

## بررسی رفتار و آزمون‌ها

- DTO معرفی همتا فقط PeerId، آدرس مستقیم، capability و expiry را دارد؛ JSON unmapped memberها را رد می‌کند و اندازهٔ control message محدود است.
- سرویس discovery فقط capability را query می‌گیرد و directory metadata را برمی‌گرداند.
- آزمون Windows، ارسال فیلدهای `prompt`, `contextCapsule` و `capsule` را با HTTP 400 رد می‌کند و بعد از آن directory خالی می‌ماند.
- Android client برای discovery یک GET بدون body می‌فرستد و آدرس‌ها/PeerId/expiry را strict parse می‌کند؛ آزمون‌ها PeerId mismatch، metadata expired، duplicate و URL/route نامعتبر را رد می‌کنند.
- Product Build run [41917207293](https://github.com/artinrahimii/project/actions/runs/41917207293)، Windows/Host Lifecycle و Android jobs، و Quality Gate run 41917207398 در snapshot قبلی موفق بودند.

## تصمیم unchanged

قرارداد production metadata-only در source baseline تا main جاری بدون تغییر باقی مانده است؛ آزمون‌ها نیز ورودی حساس را رد می‌کنند. تغییرات UPD-S1 در زمینهٔ امنیت و privacy نیازمند تغییر قرارداد این task نیستند؛ بنابراین وضعیت پایه `done` و تصمیم `unchanged` حفظ می‌شود. این آزمون‌ها packet capture همهٔ شبکه یا رفتار سرویس ثالث را اثبات نمی‌کنند.

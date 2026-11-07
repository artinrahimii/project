# شواهد ممیزی خط مبنای UPD-S1-001

## 1. commit و روش دریافت

- Repository: artinrahimii/project
- Branch: main
- Commit: afa79587ee403942b5ba2f1e91b3e7ccc2988f08
- Commit message: V 3.1.17
- Commit time: 2026-11-07T18:28:42Z
- Root tree SHA: 44cd148a28d4232b481e92571a5713b741fd3cb4
- روش دریافت: GitHub branch/ref، Git tree recursive، GitHub Contents API برای فایل‌های معمولی و Git Blob API برای فایل ۴ مگابایتی گراف قدیمی.
- tree recursive: truncated=false؛ ۱٬۴۱۱ entry در tree.
- محدودیت: وضعیت worktree محلی و re-hash مستقل روی فایل خام در یک clone محلی در این اجرا در دسترس نبود. برای فایل بزرگ، Blob SHA در Git tree با SHA Blob درخواست‌شده برابر و byte-length محاسبه‌شده با size همان tree برابر شد.

## 2. تمامیت فایل‌های مرجع

اندازه برحسب بایت UTF-8 محاسبه و با اندازهٔ Git tree تطبیق داده شد.

| مسیر | Blob SHA | بایت |
|---|---|---:|
| update/Step two/project-graph.md | 6950a82b46636ebc823cb60aa22a831ec3c47d9f | 242٬945 |
| update/Step one/Output/V1.10.json | 5cea3ec095d0c00e32ed3e067796ff698aedd982 | 93٬346 |
| step three/project-graph.md | 1b51b7452762c85d18b0e2f5acbba6f1624e1f83 | 4٬014٬945 |
| step one/V1.0.md | 25fa14843b10ca972796aa17a23ef26582011a20 | 36٬964 |
| update/Step two/legacy-task-reconciliation.json | 30fe209ed36dde8eb1fda05e67023cebafa9f48c | 81٬337 |
| tools/graph_guard.py | 6558c0d38cfb23d8edec710bed92bdaee6cdfcc6 | 11٬873 |
| tests/test_update_graph_contract.py | f8223d549b481765d669072d8103dfdbd0fa1ad8 | 5٬316 |

برای هر هفت فایل: مسیر در tree موجود بود؛ Blob SHA پاسخ فایل با SHA tree برابر بود؛ اندازهٔ بایتی محاسبه‌شده با اندازهٔ tree برابر بود. JSON فایل V1.10، JSON محصور در Markdown هر دو گراف و JSON reconciliation Parse شدند.

## 3. اندازه و وضعیت گراف‌ها

- Update graph: ۲۴ node، ۸۰ edge، ۱۳ execution wave؛ وضعیت پیش از این تلاش ۲۴ not_started؛ هر ۸۰ edge در آغاز unreviewed.
- Legacy graph: ۲۳۰ node، ۴۲۲ edge؛ ۱۶۸ node دارای نشانگر update impact.
- شناسه‌های T یکتای سند V1.0: ۲۲۹.
- شناسه‌های متأثر یکتا: ۱۶۸؛ همه در graph قدیمی موجودند و همه نشانگر دارند.
- Reconciliation artifact: ۱۶۸ رکورد یکتا؛ هر ۱۶۸ مورد pending و بدون evidence بودند. این ممیزی UPD-S1-008 را تکمیل نمی‌کند و هیچ وضعیت legacy را تغییر نمی‌دهد.

## 4. کدهای بررسی‌شده

| ناحیه | مسیرهای اصلی | Blob SHA |
|---|---|---|
| Android app entry | src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt | 5de14e8fbf52bdb93b6b035729968cba527ec02a |
| Android chat UI | src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt | b730e4d76298c9f48a3341176fd8901b04141e9f |
| Android local message | src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt | 0fe9a5c82a0fb2e1d5205db4c0d4470dbb935c4f |
| Android Room | src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt | 9b128bbeeffba05580cb1699bf7e7714b2236c0b |
| Android network runtime | src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt | dfc92163f4a8d98c9522ccbc6a8012162e1f827d |
| Android request UI logic | src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt | 25f0f018976ab9a2645d0d28e79f9e92eebdac58 |
| Android P2P transport | src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt | b9ce837e596a193315c53905c2ccec815273d8ce |
| Windows host UI and routing | src/Host.Windows/MainWindow.xaml.cs | ee30dba8d4a2dd2cc1aacc0e8a18b981d60158e0 |
| Windows host session | src/Host.Application/HostInferenceSession.cs | 2ccb69e748c5b40e89252925d1da80c1e259fab5 |
| Windows admission/concurrency | src/Host.Windows/Requests/HostRequestCoordinator.cs | a06529ed485c4f48352caf7f87c5f76397948131 |
| Windows request policy | src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs | 04540b9be9a1aa8f89446bb35941931bf7180f59 |
| Shared protocol validation | src/Shared.Contracts/ProtocolMessageValidator.cs | cfe79563e9d1111e4ad73702b62d6ec9d80ea157 |
| Shared trust store | src/Shared.Security/PeerTrustStore.cs | 9f96a034d2d137963597eff5d5bc737f854207d0 |
| Shared replay guard | src/Shared.Security/AuthenticatedRequestReplayGuard.cs | c598011542c7cbf8ea704e7fb8dd2e0301cbbf25 |

## 5. اهم یافته‌ها

1. Conversation detail sends local message to Room only; ViewModel comment explicitly says it does not send content to a processing host.
2. Network inference runtime and libp2p transport exist behind a different New request route.
3. New request route asks for Rendezvous URL, exact Model ID and max output token count; peer discovery and trust approval are explicit.
4. Windows host accepts peer requests through the participation lifecycle, validates protocol/request state, checks admission and delegates to local inference.
5. Host coordinator uses a single-slot semaphore; admission checks active participation, model readiness/match, capacity and deadline.
6. Shared contracts/security modules exist, but presence of source is not physical-device proof.
7. The exact expected shared visual assets, physical product E2E acceptance test/workflow and in-place upgrade test/workflow were not found in the audited tree under their expected/product-specific names.

## 6. CI evidence at base commit

- Quality: https://github.com/artinrahimii/project/actions/runs/42401485764 — success; Python suite, Python compileall and graph guards passed.
- Product build: https://github.com/artinrahimii/project/actions/runs/42401485830 — success; Windows restore/build/publish/WPF startup/installer and lifecycle suite passed; Android lint/build/JVM tests and Android emulator Keystore instrumentation passed.
- Rendezvous: https://github.com/artinrahimii/project/actions/runs/42401485784 — success; container build and HTTP smoke passed.
- Not evidenced by these runs: real local model inference on the user's Windows host, physical Android-to-Windows product E2E, clean install on physical supported platforms, or in-place upgrade.

## 7. Audit boundaries

No product source was modified by UPD-S1-001. No legacy task status, dependency edge, model setting, trust policy or visual asset was changed. No physical device or separate Windows host was available to this repository audit. The new static audit regression test is an audit guard only and must not be represented as a product E2E test.


## 8. Regression audit test run and correction

- First quality run for the new audit test: https://github.com/artinrahimii/project/actions/runs/42408388222 — failed.
- Exactly two newly added assertions failed:
  - expected ASCII-formatted 4,014,945 while the evidence contains the Persian grouping separator in 4٬014٬945;
  - expected an explicit SemaphoreSlim(1, 1) constructor call while the implementation uses the target-typed initializer _inferenceSemaphore = new(1, 1).
- The relevant source findings remain supported by the repository: the field is a one-slot SemaphoreSlim and the evidence uses the byte count returned by the Git tree.
- Both assertions were corrected to match the actual repository text. No product source was changed.
- Four of the six new audit tests passed in this first run. Full rerun is pending; no successful result is claimed yet.

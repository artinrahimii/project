# UPD-S1-018 — شواهد ممیزی A001

- Commit: `a0f50e22e56f7052c8b9157226b0b72713e65bc5` (`V 3.1.187`)
- Tree SHA: `a0f50e22e56f7052c8b9157226b0b72713e65bc5`
- پیش‌نیازهای 007/016/017: done و دارای output/evidence.

| مسیر بررسی‌شده | Blob SHA |
|---|---|
| `src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt` | `f359abbbda3c8f5788f95133c98a1e399797fd24` |
| `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt` | `f7c9b1fb8fa4eb22723a02d745db0013cb9960ac` |
| `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt` | `99274979ad3c1c19b33a5f15cfc500d7f5350ad4` |
| `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt` | `bb6e89c8aec7893c7699e240e90e101c30212195` |
| `src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt` | `f39a0695e6ce2b927b255e0f188dd1ed4b778098` |
| `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt` | `0118137308a872d63dda57bef24261aa680efc42` |
| `src/Shared.Security/SafeDiagnosticLog.cs` | `8d6f926a3d783c004eedaffa5760406eecc281b1` |
| `src/Shared.Security/AuthenticatedRequestReplayGuard.cs` | `c598011542c7cbf8ea704e7fb8dd2e0301cbbf25` |
| `src/Shared.Security/HostIdentityStore.cs` | `a53222442c3efb4bc3a372cf4aaf0190f4a3dc77` |
| `src/Shared.Security/PeerTrustStore.cs` | `184487c99bd522986d00abf500f822d056861ece` |
| `src/Host.Windows/Security/WindowsDpapiSecretProtector.cs` | `b9f6c287fdd495b4d5c312ec706c33ccb4f3f290` |
| `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs` | `fb4acb368d91f87d72fbe4eea2c3cf39d79caf77` |
| `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs` | `84625ce367689bdedb5f042eaab9171d0ba6dd73` |
| `src/Host.Windows/Networking/Libp2pPeerTransport.cs` | `c8ac8f5c9785eac4fbb06ce23c05b7f9dc1b3a20` |
| `docs/security.md` | `1ed547cc60d0d058d5f7aef661076850774db510` |
| `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs` | `78f4886bd3b9c6a628661d1b1425ba03dfc461d6` |
| `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs` | `f54cf4e3ae02d7536bf357dfaf88f1049566be60` |
| `tests/HostLifecycle.Tests/WindowsHostIdentityStoreAcceptanceTests.cs` | `2f96caca8dcb0c0b03cc38b0521f1aa170c53683` |
| `tests/HostLifecycle.Tests/WindowsPeerTrustStoreAcceptanceTests.cs` | `4a2160a7d3b9a4c092a44f2a65242351b7a41e1b` |
| `tests/test_network_payload_logging.py` | `ca5b77605b46c126c79643228940cdc0bfd8bea7` |
| `tests/test_sensitive_artifact_scanning.py` | `fd76c90bf94e17a4b1b34ca8a496d8562b4728e7` |
| `tests/test_sensitive_buffer_cleanup.py` | `ac75f8a41f001fe22d88f122045189237deba8ab` |
| `src/Client.Android/app/src/test/java/com/whitep/client/network/SafeDiagnosticLogTest.kt` | `8a3fa05ab5242625eecf555810f5cc0bbec295f3` |
| `src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerIdentityStoreTest.kt` | `cd8d7ff9c014be5594eb510d37fbeb528f9aaae7` |
| `src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerTrustStoreTest.kt` | `3c2e7a0bfe95ffda4939fdf87c552aed0ebb8317` |
| `src/Client.Android/app/src/test/java/com/whitep/client/network/PeerMessageFrameCodecTest.kt` | `490b65d5281c62ef2a46825a31015bc4feed01c1` |
| `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt` | `138619e4bb4e74aed85773646ab10d54c6b674b2` |

## CI دقیق commit
- Quality [42909899013](https://github.com/actions/runs/42909899013): PASS؛ ۴۹۶ تست Python و graph guards.
- HostLifecycle [130157488206](https://github.com/artinrahimii/project/actions/runs/42909899054/job/130157488206): PASS؛ ۶۸ critical pass markers، یک skip، ۸۶٫۱۴٪ line coverage.
- Rendezvous [42909898965](https://github.com/artinrahimii/project/actions/runs/42909898965): PASS.
- Android [130157487721](https://github.com/artinrahimii/project/actions/runs/42909899054): ktlint، Debug/Release، key cleanup، permissions و JVM/coverage PASS؛ Android Keystore instrumentation هنوز در جریان.
- Windows [130157488253](https://github.com/artinrahimii/project/actions/runs/42909899054/job/130157488253): WPF build/publish/smoke، DPAPI identity/trust و installer acceptance PASS.

Replay guard برنامه process-local است؛ صفرسازی به mutable buffers محدود است. pagefile/swap، crash dump، immutable objects و artifactهای ثالث پوشش داده نشده‌اند. Physical Android↔Windows در این task اجرا نشد.


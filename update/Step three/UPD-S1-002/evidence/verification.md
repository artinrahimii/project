# شواهد ممیزی UPD-S1-002

## مبنای مخزن و یکپارچگی فایل‌ها

- Commit اجرای وظیفه پیش از تغییر: `22bd0cf7ebbd7b0b75e85331fdc743845769a76a` (`V 3.1.21`), branch `main`.
- Commit محصول کد مورد ممیزی: `14538c63f940c6da5b44c0b9ad03614a54fba69f` (`V 3.1.20`); commit V3.1.21 تغییر آزمون ممیزی بود و کد محصول در مسیرهای زیر تغییر نکرد.
- گراف به‌روزرسانی: `update/Step two/project-graph.md`.
- منبع نیازمندی: `update/Step one/Output/V1.10.json`, Blob SHA `5cea3ec095d0c00e32ed3e067796ff698aedd982`.
- Baseline graph V3.1.20 Blob SHA for update graph: `5ae45296aa78006c2d75099f2321f6e45296e026`.
- Large legacy graph was validated earlier in the same run from Git Blob `1b51b7452762c85d18b0e2f5acbba6f1624e1f83`, exactly 4,014,945 UTF-8 bytes as the tree entry. No legacy graph content was modified by this task.

## Source evidence checked at V3.1.20

- `docs/decisions/ADR-0002-direct-p2p-data-path.md`: existing direct P2P policy; prior status wording was stale because it said host transport was merely being introduced.
- `docs/decisions/ADR-0004-encrypted-relay-boundary.md`: relay is disabled; future relay metadata and E2E requirements are explicit.
- `docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md`: host-side same-PeerId ordered TCP/QUIC path; does not certify Android QUIC.
- `docs/decisions/ADR-0006-peer-announcement-leases.md`: five-minute TTL, one-minute refresh, max 24-hour lease, in-memory latest record, cleanup on subsequent announce/discovery.
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs`: PeerId, Addresses, Capabilities, ExpiresAtUtc only; size and field bounds.
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`: only announcement POST and capability GET plus health endpoint; unknown JSON properties disallowed.
- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`: current-record in-memory dictionary and expiry cleanup.
- `src/Host.Windows/Networking/PeerAnnouncementClient.cs`: HTTPS required except loopback tests; only announce/discover.
- `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs`: default five-minute TTL and one-minute renewal.
- `src/Host.Windows/Networking/PeerTransportFallbackConnector.cs`: direct endpoint metadata only; same identity; no prompt/response parameters; explicit unavailable outcome.
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`: metadata GET, HTTPS, no-store, five-second cache default / one-minute maximum, stale cache not used.
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`: direct authenticated TCP path and PeerId/request correlation.
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`: normal Room chat stores local user messages and does not send content to the processing host.
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`: loopback service checks exact metadata contract, prompt/capsule/capsule-field rejection, no inference route, replacement semantics, TTL expiry and direct encrypted peer exchange/cancellation.
- `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`: Windows direct TCP/QUIC fallback and mixed PeerId rejection.
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs` does not constitute a physical Android/Windows device test or third-party deployment logging audit.

## Relevant legacy reports read

- `step three/T0009/execution-report.md`: bounded source-only inventory; runtime privacy acceptance not run.
- `step three/T0019/execution-report.md`: direct P2P architecture decision; no transport proof in that historical task.
- `step three/T0118/execution-report.md`: host-side direct TCP/QUIC candidate fallback; NAT traversal and Android E2E excluded.
- `step three/T0119/execution-report.md`: future relay opaque E2E policy; no relay stream enabled.
- `step three/T0120/execution-report.md`: implemented metadata-only service and loopback API acceptance.
- `step three/T0206/execution-report.md`: C# rejection of prompt/context fields and Android bodyless GET; not packet capture.
- `T0227` has no execution report because its legacy graph status is `not_started`; that status is preserved.

## Current CI observations before this attempt

- Update task regression test + decision changes: pending until the first commit containing these files is built.
- Quality Gate V3.1.21: PASS, run 42409561015.
- Rendezvous container V3.1.21: PASS, run 42409560957.
- Product build V3.1.21: in_progress at the last observation (run 42409560949).
- No local shell/clone was available in this tool session; no claim of local unit-test execution is made.


## بررسی CI پس از commit V 3.1.22

- Quality Gate run 42409868004: **FAIL**؛ 464 tests، 9 assertion failures، 455 passed.
- Rendezvous container run 42409867998: **PASS**؛ image build و HTTP smoke روی commit دقیق.
- Product build run 42409868020: Windows WPF job 128515459519 و HostLifecycle job 128515459678 هر دو **PASS**؛ Android job 128515459792 در آخرین مشاهده **IN PROGRESS** بود و نتیجهٔ نهایی workflow هنوز معلوم نبود.
- نتیجهٔ آزمون‌های commit اصلاحی باید از runهای دقیق همان commit گرفته شود؛ در این مرحله pending است.

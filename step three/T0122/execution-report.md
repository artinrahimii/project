# T0122 execution report — expired and disconnected peers excluded from discovery

- **Task:** remove expired or disconnected hosts from the selectable peer-discovery results.
- **Exact verification baseline:** online `main`, commit [`V 3.0.751`](https://github.com/artinrahimii/project/commit/6f83ec3755485e38ba4576107fefc09d463cf307) (`6f83ec3755485e38ba4576107fefc09d463cf307`).
- **Prerequisites:** T0121 (software lease/host-lifecycle scope) and T0115 are both `done`; the incoming T0121 contract/interface edge and T0115 sequencing-only edge were re-read and preserved. The final-only deployment tasks T0230/T0128 are not prerequisites for this local lease-expiry contract.

## Existing production implementation verified

- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`: capability discovery calls `RemoveExpired(DateTimeOffset.UtcNow)` while holding the directory lock, before constructing the returned peer list. Expired announcements therefore are not returned by the selectable discovery endpoint.
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`: `GET /v1/peers?capability=...` returns only the filtered directory result.
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs` and `PeerAnnouncementLeasePublisher.cs`: peer-session disconnect/failure stops the host lifecycle and lease renewal. Once the final TTL elapses, the directory no longer returns that host.
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`: the parser skips entries whose `expiresAtUtc` is not after the supplied current time; `resolve` checks expiry again at lookup and bounds its in-memory cache by the earliest peer expiry.
- The Android JVM test `RendezvousPeerDirectoryClientTest.excludesExpiredPeersFromTheReturnedDirectory` verifies that an expired peer cannot be resolved by the client.

## Exact-commit verification

- **HostLifecycle unit/integration — PASS:** [product job 124826493348](https://github.com/artinrahimii/project/actions/runs/41299223644/job/124826493348). Its acceptance suite includes `PeriodicLeaseRenewalRefreshesTtlAndStopsAsync`: renewal moves expiry forward; after the publisher is stopped and the last TTL elapses, capability discovery returns no peer. It also includes `PeerHostLifecyclePublishesAndServesTrustedPeerAsync`: disconnecting the peer stops host availability/lease renewal, and after 2.2 seconds the disconnected host is absent from discovery.
- **Android lint/APK/JVM/Protobuf — PASS:** [product job 124826493366](https://github.com/artinrahimii/project/actions/runs/41299223644/job/124826493366), including the Android JVM unit-test step containing the expired-directory test above.
- **Product workflow — PASS:** [run 41299223644](https://github.com/artinrahimii/project/actions/runs/41299223644); Windows build, HostLifecycle tests and Android build/JVM tests all completed successfully on the exact V 3.0.751 commit.
- **Step Three quality gate — PASS:** [run 41299223631](https://github.com/artinrahimii/project/actions/runs/41299223631).
- **Rendezvous container smoke — PASS:** [run 41299223751](https://github.com/artinrahimii/project/actions/runs/41299223751).

## Scope and limitations

- No new production source code was necessary in this T0122 attempt: the required service/client behavior and regression coverage already existed in the repository and were verified on the exact current main commit. This task records their acceptance against T0122; it does not claim new code was authored during this attempt.
- The host/peer lifecycle test uses loopback networking. It proves lease expiry and discovery filtering in the local integration environment, not public reachability from an independent network.
- Real public HTTPS deployment and independent-network advertised TCP/QUIC acceptance remain exclusively in final-only T0230, followed by T0128. No such external test is claimed here.
- Readiness/heartbeat beyond lease expiry is a separate T0123 contract and is not claimed as part of T0122.

**Outcome:** T0122 is complete for its lease-expiry/discovery-filtering acceptance scope. Next implementation task: T0123.

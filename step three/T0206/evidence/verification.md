# T0206 verification evidence

## Source revision and outputs

- Repository: `artinrahimii/project`
- Branch: `main`
- Source commit: `d9dce141a90c67172abd5aa0de064f9a0b943fb5` (`V 3.0.941`)
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt`
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`

## Incoming edge validation

1. `T0205 → T0206` — `sequencing_only`, validated.
2. `T0195 → T0206` — `sequencing_only`, validated.

The two incoming edge `type: blocks` values and conservative order were preserved.

## Privacy boundary verified

- `PeerAnnouncement` has exactly four public data properties: peer ID, direct addresses, capability names and expiry.
- `PeerAnnouncementServiceHost` configures JSON unmapped-member handling to `Disallow`.
- The C# control-plane acceptance sends test-only forbidden fields `prompt`, `contextCapsule` and `capsule`; each receives HTTP 400.
- The next capability-directory query returns HTTP 200 with `[]`, confirming rejected announcements were not stored.
- A separate C# assertion checks the directory response has only `addresses`, `capabilities`, `expiresAtUtc` and `peerId`.
- The Android client test asserts `GET`, `doOutput == false`, only `capability=inference.v1` in the query, and no prompt/capsule tokens in the URL.

## Exact-head CI evidence

- [Product Build run 41917207293](https://github.com/artinrahimii/project/actions/runs/41917207293), exact head `d9dce141a90c67172abd5aa0de064f9a0b943fb5`.
  - Host lifecycle job `126879915099`: success. The log shows all three HTTP 400 rejections, the empty directory response, and the metadata-only PASS line.
  - Android job `126879915387`: success. `gradle --no-daemon testDebugUnitTest` ran `:app:testDebugUnitTest` and ended with `BUILD SUCCESSFUL`.
- [Step Three Quality Gate run 41917207398](https://github.com/artinrahimii/project/actions/runs/41917207398), job `126879914564`: success. Graph guard output: 230 tasks, 422 edges, 0 errors, 1 warning (T0001's unmapped text dependency metadata, preserved).

## Limits

The invalid sensitive-field requests are deliberate test inputs. The evidence proves that this service rejects those fields and does not store them, and that the Android discovery path sends a bodyless metadata-only GET. It is not packet capture and does not establish the behavior of unrelated external services.


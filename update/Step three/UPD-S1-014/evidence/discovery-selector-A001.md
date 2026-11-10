# شواهد UPD-S1-014 — تلاش A001

## خط مبنا

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit: `0392fb1a57a1e105f54fda19c6e2a03fe33e531d` — `V 3.1.182`
- Update graph Blob: `66c4f27d634bdcd97eb832012bf582d75e4bf326`
- Requirements V1.10 source: `update/Step one/Output/V1.10.json`, UPD-S1-014.
- Old graph: 230 nodes / 422 edges; 13 legacy task IDs in this task's affected set: T0116–T0128.

## شواهد source

- `RendezvousPeerDirectoryClient.kt`: `2b93c0de23b398643a275e2b4e9cbe06200cce17`
- `AuthenticatedHostDiscovery.kt`: `ad982ec7d4c576edee3b8976066b281801bd9567`
- `HostCandidateSelector.kt`: `f0f89f8052ab60fe740e77fdc9fb39da7bdb1f8b`
- `PeerAnnouncementClient.cs`: `ad7b242c2f28b8c090ce7bc08c25b7b1da98b2bc`
- `PeerAnnouncementHostLifecycle.cs`: `fb4acb368d91f87d72fbe4eea2c3cf39d79caf77`
- `PeerAnnouncementLeasePublisher.cs`: `aa9286271708a488608a2c41706f8702dc7bccf7`
- `PeerAnnouncementServiceHost.cs`: `9218f1c05f281434dad3b940f7f925d86b54b91b`
- `PeerAnnouncementDirectory.cs`: `4a2a2d18c036b14f82589a976d29fda6263a9d26`
- `PeerAnnouncement.cs`: `557f418edf6c0a2c186bf2ffb2af5aa93d59239f`
- `MainWindow.xaml.cs`: `77f14b493e7621bd9d80fa2279130e002e7eb8f4`
- `HostRequestAdmissionPolicy.cs`: `ccc2c1170111b5a4e015dd252ffadd0af3f2ee87`
- `HostStatus` protobuf exposes identity, state, active request count, observation timestamp, models and protocol version.
- `ADR-0022` fixes first-release to one host per round, up to three sequential attempts; Windows host reports BUSY with one active request and READY only after backend/model/capacity verification.

## Acceptance decisions

- TTL and renewals are already integrated in host lifecycle: default TTL 5 minutes, renewal 1 minute; stopping participation stops renewal and the directory removes the record after expiry.
- Android rejects expired leases, noncanonical/mismatched identities, missing direct TCP endpoints, stale/future HostStatus and non-READY/full/mismatching model states.
- Windows network participation defaults off and requires opt-in/privacy disclosure, a separately trusted peer and a live ready backend/capacity.
- The directory only exposes PeerId, addresses, capabilities and expiry; there are no prompt/response/history endpoints or request-body fields.
- First-release selector probes in stable PeerId order and returns one eligible host. Because the approved host runtime is single-inference and reports BUSY for an active request, any READY candidate has measured activeRequests=0. The first ready host in stable order is already at the minimum measured load for this release. Generic selector candidates now additionally sort by actual active-request utilization before PeerId.
- No latency/throughput values are currently measured; no guessed network score is used.

## A001 code diff

- Candidate ranking changed from PeerId-only to exact measured utilization ratio using integer cross-products, with PeerId tie-break.
- Added a JVM test for 0%, 25% and equal 50% utilization ordering.
- Updated Python source contract to assert the actual ranking algorithm and the new JVM test name.
- No protobuf or wire contract changes; no change to first-release one-host cardinality.

## CI status

- Baseline exact V 3.1.182 Quality/Product/Rendezvous all passed before this A001 diff.
- A001 exact-head CI has not run yet. New ranking test and Kotlin lint/build remain pending; the task must remain `in_progress`.
- No physical Android handset, user-owned Windows host, independent public-network probe or real model E2E was used for this attempt.

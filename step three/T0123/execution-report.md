# T0123 execution report — live authenticated HostStatus heartbeat

- **Base:** online `main`, `V 3.0.752` — `226497cf72ba2e0781fdb91d1968c8d72f8ad9bb`.
- **Graph validation:** `step three/project-graph.md` was read from its exact tree Blob `9748c9621e507aa07d6b0724ac786d7cd3939664`; its UTF-8 byte length and recomputed Git Blob SHA-1 matched before parsing. The graph contained 230 tasks / 421 edges. Incoming dependencies `T0122` (validated contract/interface) and `T0115` (validated sequencing-only) are both `done`.
- **Gap found:** `HostStatusRequest` and `HostStatus` existed in the Protobuf schema and wire fixtures, but no production handler or Android status probe used them. Directory lease expiry is not a heartbeat.
- **Production implementation added:**
  - `HostInferenceSession.GetFreshStatusAsync` obtains a current backend health/readiness snapshot for peer status requests. It reports BUSY or a transition conservatively without waiting behind an active inference operation.
  - `ProtocolMessageValidator.ValidateHostStatusRequest` and `ProtocolMessageParser.ParseHostStatusRequest` validate the bounded control message before dispatch.
  - `PeerAnnouncementHostLifecycle` handles a status request only after checking the authenticated peer identity and only for its own local PeerId. It requests a fresh snapshot, reports UNAVAILABLE on provider failure, and stamps a server-generated `observed_at_unix_ms`.
  - The WPF status provider reads `HostInferenceSession.GetFreshStatusAsync`; it reports READY only when the current backend is healthy and capacity remains verified, BUSY with an active-request count while inference is running, and conservative STARTING/UNAVAILABLE states otherwise. No unverified model capability values are fabricated.
  - Android `probeHostStatus` sends a real HostStatusRequest over a direct TCP stream to an explicitly trusted PeerId. The response is associated with the same authenticated stream and exact PeerId. Invalid state/identity, stale timestamps, excessive future clock skew, unreasonable active-request values and expired route leases are rejected. HostStatus is never cached.
- **Tests added:** a C# HostLifecycle loopback exchange checks that an authenticated peer receives a fresh HostStatus response; an Android JVM transport test checks the HostStatusRequest/HostStatus round-trip over the authenticated libp2p stream.
- **Current state:** implementation and regression tests are authored. Exact-head GitHub Actions have not yet run; no test pass is claimed in this attempt. T0123 remains `in_progress`.
- **Next:** inspect exact-commit Step Three quality, HostLifecycle, Windows WPF and Android KtLint/APK/JVM results. Correct any failures, then record the exact successful evidence before changing T0123 to `done`. T0124 is next; public HTTPS and independent-network acceptance remain final-only T0230/T0128.



## T0123-A001 — first compile failure

- **Exact run:** V 3.0.753, product workflow [41302359549](https://github.com/artinrahimii/project/actions/runs/41302359549).
- **HostLifecycle result:** failed to compile with `CS0136` at `PeerAnnouncementHostLifecycle.cs:288:32`: the status branch redeclared `authenticatedPeerIdentity`, already used by the inference branch in the same receive-loop scope.
- **Android result:** the .NET peer-interoperability fixture build also stopped on the same shared compile error. Kotlin lint, APK compilation and Android JVM tests were skipped; no Android runtime pass is claimed.
- **Correction in A002:** renamed the new status-branch local to `statusAuthenticatedPeerIdentity`. No protocol or security behavior was changed.
- **Task state:** T0123 remains `in_progress`; A001 is recorded as failed, A002 is in progress. No acceptance criterion is yet marked complete.

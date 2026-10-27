# T0120 verification evidence — exact-head CI pending

## Baseline integrity
- Repository: `artinrahimii/project`, branch `main`.
- Base: `V 3.0.360`, `919b02586d8e6c62994e7dc9402b642f3704b663`.
- Graph Blob `3e073274c08216ef3fbd800d468e7e19dc690f52`: full original content was retrieved; Git Blob SHA-1 matched and fenced JSON reconstructed exactly: 229 tasks, 426 edges, 229 execution-order entries.

## Actual outputs
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs`: strict metadata-only schema, direct TCP/QUIC address validation, capability tokens and bounded JSON wire format.
- `src/Host.Windows/Networking/PeerAnnouncementClient.cs`: only announcement/capability-discovery API; HTTPS outside loopback; bounded response reader.
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`: only announcement POST and capability GET routes, strict JSON fields and 16 KiB raw request limit.
- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`: in-memory expiry and replacement rather than announcement history.
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`: actual loopback service integration and negative tests.
- `WhiteP.sln` and HostLifecycle configuration updated to build and run service/tests.

## Dependency evidence
- `T0119 → T0120`: validated contract dependency on ADR-0004's metadata-only privacy boundary.
- `T0115 → T0120`: reviewed as sequencing-only; original `blocks` edge retained.

## Current result
Exact-head CI has not yet confirmed compilation or runtime acceptance. T0120 must remain in progress until the new loopback test, product builds and graph/output/evidence gate pass. No Android-to-Windows end-to-end inference claim is made.

# T0121 verification evidence — pending CI

## Baseline and graph validation
- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `6f8694baee77e106ef83ad1441ee4da54a5bce42` (`V 3.0.364`), tree `19663f3c6f61ddf36da207897cb35da69915cb23`.
- Graph path: `step three/project-graph.md`; blob `417ea870c139d3e858cb93ccfadde241feb4c6bb`; UTF-8 byte count 1,866,696 matched the tree entry size and independently recomputed Git SHA-1 Blob ID. Full fenced JSON parsed successfully.
- Graph counts at baseline: 229 tasks; 426 edges; 119 marked done, 1 in progress, 109 not started, 0 blocked. These are graph status values, not a fresh independent revalidation of every previously done task.

## Source-level changes
- `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs`: immediate metadata announcement, bounded TTL/renewal interval, recurring renewal, safe stop and natural remote expiry.
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`: real loopback HTTP integration check for immediate announce, expiry extension, stop, and expiry after the final lease.
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`: includes the new source in the executable acceptance-test project.
- Dependency evidence: `src/Shared.Contracts/Networking/PeerAnnouncement.cs`, `src/Host.Windows/Networking/PeerAnnouncementClient.cs`, `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`.

## Runtime and CI limitations
- Local .NET SDK is unavailable in this execution environment; no local .NET build/test result is asserted.
- GitHub Actions results must be recorded after the online commit is created.
- The helper is not yet wired into WPF participation because the existing listener is loopback-only and a reachable rendezvous URL / externally reachable advertised address are not configurable yet. Therefore T0121 remains `in_progress`.

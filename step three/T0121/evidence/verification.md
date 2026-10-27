# T0121 verification evidence — exact commit CI passed

## Baseline and graph integrity
- Repository: `artinrahimii/project`; target branch: `main`.
- Implementation commit: `969ad720db1a84c1f633031300ba26e00d48d5fa` (`V 3.0.365`).
- Graph baseline Blob SHA: `417ea870c139d3e858cb93ccfadde241feb4c6bb`; tree size 1,866,696 bytes; recomputed Git SHA-1 Blob ID matched. Full fenced JSON parsed successfully with 229 tasks and 426 edges.
- The current graph records T0121 as `in_progress`; the task is intentionally not counted as done.

## Exact-commit CI evidence
- **Host lifecycle and acceptance tests — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746502
  - Includes the live HTTP test for immediate announce, TTL renewal, stop and natural expiry.
  - Log output: `PASS: metadata-only discovery, periodic TTL renewal/expiry, unknown-field rejection and control-message bounds.`
- **Step Three quality guard — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186284
  - Python tests, Python syntax compilation, graph-structure validation and recorded output/evidence validation.
- **Windows product build — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746273
  - `dotnet format whitespace`, Release build, generated Protobuf validation and WPF startup smoke.
- **Android product build — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746493
  - Kotlin lint, debug APK build, Protobuf round-trip and generated-source checks.
- **Real local inference — PASS:** https://github.com/artinrahimii/project/actions/runs/40455186251
  - Live llama.cpp/GGUF inference, cancellation and owned-process shutdown.
- **Aggregate product run:** https://github.com/artinrahimii/project/actions/runs/40455186298

## Remaining acceptance gap
The lease publisher is not yet wired into the WPF host participation lifecycle because the current app only binds its listener to loopback and lacks configured rendezvous/peer-reachable address settings. End-to-end host announcement/renewal lifecycle therefore remains incomplete; do not mark T0121 done.


## T0121-A003 — cleanup/configuration regression coverage

- **Base:** `071be6ccc613400cd649ce33f8fb478692ab8526` (`V 3.0.366`); `main` was re-read before preparing the change.
- **Changed implementation:** `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs` now disposes the renewal cancellation source from `finally` during stop, including an unexpected renewal-task fault path.
- **Changed acceptance tests:** `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs` validates defaults, null dependencies, TTL lower/upper bounds, and the strict renewal-interval-before-TTL rule.
- **Graph validation:** Blob `491422474a6ccceb3c687b14d052465150334e9e`, 1876796 UTF-8 bytes, 1755078 text characters; computed Git SHA-1 equals the tree Blob ID; fenced JSON parses to 229 nodes and 426 edges.
- **New-commit CI:** pending. No new configuration test is marked passed until the workflow result for the resulting main commit is inspected.
- **Acceptance gap unchanged:** host-UI/lifecycle wiring is not safe yet because reachable rendezvous/address configuration is absent and the current listener binds loopback only.


## T0121-A004 — correct the CI compile failure

- **Detected failure:** the Host lifecycle job for `V 3.0.367` failed while compiling the new configuration tests. Compiler error `CS1503` identified an `HttpClient` passed where `PeerAnnouncementClient` is required at the new test construction call sites. [Failed job logs](https://github.com/artinrahimii/project/actions/runs/40455847517)
- **Correction:** the test now wraps its loopback `HttpClient` in `PeerAnnouncementClient` before exercising `PeerAnnouncementLeasePublisher` constructor validation. The tests do not send a network request during these constructor assertions.
- **Verification:** exact commit `e4bd21fe4bc3a7bad2e0ed45f12843284bb58ee2` has a passing graph/Python quality workflow, but its Host lifecycle job failed as above. The correction's exact-commit CI is pending; no test pass is claimed yet.
- **Task status:** `in_progress`; the host UI/participation lifecycle integration is still not safe to enable without configured rendezvous HTTPS and a verified peer-reachable address.

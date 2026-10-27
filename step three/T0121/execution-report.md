# T0121 execution report — partial; CI verified

- **Task:** implement renewable host-presence announcements with TTL.
- **Implementation commit:** `V 3.0.365` — [`969ad720db1a84c1f633031300ba26e00d48d5fa`](https://api.github.com/repos/artinrahimii/project/commit/969ad720db1a84c1f633031300ba26e00d48d5fa).
- **Baseline graph validation:** `step three/project-graph.md` at the base commit was recovered with Git Blob API after ordinary file-content retrieval returned an empty string. Its blob `417ea870c139d3e858cb93ccfadde241feb4c6bb` matched the original UTF-8 bytes (1,866,696 bytes); the complete fenced JSON parsed as 229 nodes and 426 edges.
- **Implementation:** `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs` publishes an immediate metadata-only announcement, refreshes `ExpiresAtUtc` on a configured interval, bounds TTL/interval using the shared contract, survives expected transient HTTP/IO/time-out failures without retaining request content, and stops renewal cleanly so the rendezvous directory naturally expires the final record.
- **Acceptance coverage:** `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs` exercises the live loopback HTTP rendezvous service and verifies immediate publication, later expiry on renewal, clean stop, and removal from discovery after the final TTL expires. `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj` includes the implementation in the executable acceptance test project.
- **CI verification on exact implementation commit:** 
  - **Host lifecycle/integration PASS**, including the emitted line `PASS: metadata-only discovery, periodic TTL renewal/expiry, unknown-field rejection and control-message bounds.` [Job log](https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746502)
  - **Step Three quality gate PASS** (Python tests, compileall, graph structure, recorded-output/evidence validation). [Workflow](https://github.com/artinrahimii/project/actions/runs/40455186284)
  - **Windows WPF Release build, formatting, Protobuf generation validation and startup smoke PASS.** [Job](https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746273)
  - **Android lint, debug APK, Protobuf round-trip and generated-source validation PASS.** [Job](https://github.com/artinrahimii/project/actions/runs/40455186298/job/122026746493)
  - **Real local llama.cpp/GGUF inference and cancellation integration PASS.** [Workflow](https://github.com/artinrahimii/project/actions/runs/40455186251)
- **Dependency review:** `T0120 → T0121` was validated as `contract_or_interface_dependency` against `PeerAnnouncement.ExpiresAtUtc`, `PeerAnnouncementClient.AnnounceAsync` and `PeerAnnouncementDirectory` expiry/replacement behavior. `T0115 → T0121` was validated as `sequencing_only`; the original `type: blocks` relationship remains unchanged. Both reasons/evidence are append-only in `edge_change_log`; `T0121 → T0122` stays unreviewed.
- **Why it is still in progress:** the publisher is implemented and its integration test is green, but it is not yet wired into the WPF host participation lifecycle. The existing listener binds loopback and the app has no configured reachable rendezvous HTTPS endpoint or verified remotely reachable advertised address. Publishing loopback as a public peer address would be incorrect; no false runtime capability is claimed.
- **Next:** add safe configuration for rendezvous URL and actual peer-reachable addresses, then connect the lease publisher to participation enable/disable and app shutdown; verify renewal/revocation through host lifecycle tests.


## T0121-A003 — lifecycle cleanup and configuration acceptance hardening

- **Base commit:** `071be6ccc613400cd649ce33f8fb478692ab8526` (`V 3.0.366`), tree `b2c65973d130bc5249b131bef3986b7029c729e0`; branch head was checked before editing.
- **Production change:** `PeerAnnouncementLeasePublisher.StopCoreAsync` now disposes its cancellation source in a `finally` block, so the cleanup still occurs if an unexpected renewal-task exception propagates during shutdown.
- **Acceptance-test change:** added explicit checks for the five-minute / one-minute defaults, null client/template rejection, zero TTL, TTL exceeding `PeerAnnouncement.MaximumLifetime`, zero renewal interval, and renewal interval equal to TTL. The existing live loopback HTTP renewal/expiry scenario remains intact.
- **Graph integrity:** read from `step three/project-graph.md` Blob `491422474a6ccceb3c687b14d052465150334e9e` at the exact base commit; UTF-8 length 1,876,796 bytes (1,755,078 JavaScript string characters); independently recomputed Git SHA-1 Blob ID matched; fenced JSON parsed as 229 tasks and 426 edges.
- **Test status for this attempt:** no local build/test is claimed. CI must run against the new `V 3.0.367` commit; the added configuration tests are pending that exact-commit result.
- **Current known CI evidence:** all product checks and the Step Three quality gate passed on `969ad720db1a84c1f633031300ba26e00d48d5fa`; the `V 3.0.366` quality gate passed, while its product-build workflow was still running at the last status sample. These are historical baseline results, not evidence that the new test cases have passed.
- **Status remains `in_progress`:** the reusable publisher is not connected to the host UI/lifecycle. The host presently listens on loopback and lacks a configured rendezvous HTTPS endpoint plus a verified peer-reachable advertised address. Publishing loopback as a public address would misrepresent reachability.
- **Next:** inspect CI for the exact new commit; then build the safe configuration and participation-lifecycle integration before marking T0121 complete. Keep `T0121 → T0122` unreviewed until its dependency is resolved and its technical relationship is examined.


## T0121-A004 — correct the CI compile failure

- **Detected failure:** the Host lifecycle job for `V 3.0.367` failed while compiling the new configuration tests. Compiler error `CS1503` identified an `HttpClient` passed where `PeerAnnouncementClient` is required at the new test construction call sites. [Failed job logs](https://github.com/artinrahimii/project/actions/runs/40455847517)
- **Correction:** the test now wraps its loopback `HttpClient` in `PeerAnnouncementClient` before exercising `PeerAnnouncementLeasePublisher` constructor validation. The tests do not send a network request during these constructor assertions.
- **Verification:** exact commit `e4bd21fe4bc3a7bad2e0ed45f12843284bb58ee2` has a passing graph/Python quality workflow, but its Host lifecycle job failed as above. The correction's exact-commit CI is pending; no test pass is claimed yet.
- **Task status:** `in_progress`; the host UI/participation lifecycle integration is still not safe to enable without configured rendezvous HTTPS and a verified peer-reachable address.


## T0121-A005 — exact-commit CI verification (2026-10-27)

- **Verified base commit:** `V 3.0.368` — [`8a49b48259f693dc2ddc04e846a78fe15bff8227`](https://github.com/artinrahimii/project/commit/8a49b48259f693dc2ddc04e846a78fe15bff8227), confirmed as the online `main` head before recording this result.
- **Graph/content integrity:** the graph was fetched from its recorded Git Blob SHA after the normal contents endpoint returned empty content. The fetched Markdown-wrapped JSON parsed as 229 nodes and 426 edges; its original UTF-8 bytes reproduced the recorded Git Blob SHA-1. No dependency edge was changed in this attempt.
- **Exact-commit CI:** all workflows completed successfully on `8a49b48259f693dc2ddc04e846a78fe15bff8227`:
  - [Step Three quality gate, run 40456065310](https://github.com/artinrahimii/project/actions/runs/40456065310) — Python tests, syntax, graph validation, and output/evidence checks passed.
  - [HostLifecycle tests, job 122029550003](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029550003) — passed, including the corrected T0121 acceptance test suite.
  - [Windows WPF Release build/startup smoke, job 122029549657](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029549657) — passed.
  - [Android lint/APK/Protobuf, job 122029549997](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029549997) — passed.
  - [Real local llama.cpp/GGUF inference, run 40456065350](https://github.com/artinrahimii/project/actions/runs/40456065350) — passed.
- **Failure history preserved:** the V 3.0.367 CS1503 compile failure remains in A003's historical record; A004 corrected the fixture, and the exact V 3.0.368 CI now verifies the correction. No prior failure record was removed.
- **Status remains `in_progress`:** CI proves the current build and automated checks, but it does not prove a live peer advertisement from the WPF host. The publisher is still not wired to the participation toggle/shutdown; the current listener is loopback-only, settings have no validated rendezvous/address/identity configuration, and the project does not yet have a safe verified peer-reachable address to advertise. No external address is invented and no loopback address is published as remotely reachable.
- **Next:** establish safe rendezvous HTTPS and peer-reachable listening-address configuration with a verified peer-identity policy, then wire lease start/stop into actual participation enable/disable and shutdown. Add integration tests for runtime renewal and removal, and keep `T0121 → T0122` unreviewed until its dependency is checked against evidence.
- **Attempt outcome:** partial; no product source code changed in this verification-only attempt.
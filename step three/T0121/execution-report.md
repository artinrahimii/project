# T0121 execution report — code and CI verified; external network acceptance blocked

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

## T0121-A006 — blocked pending peer-reachable transport and host identity

- **Status:** `blocked`. Existing component-level lease renewal and loopback expiry tests do not prove the actual WPF host can publish a reachable inference peer.
- **Repository evidence inspected:** `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs` rejects non-loopback bind addresses; `src/Host.Windows/MainWindow.xaml.cs` explicitly reports that inbound requests remain closed until protocol handling is implemented; `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs` requires the owner to advertise only genuinely reachable addresses; `src/Shared.Contracts/Networking/PeerAnnouncement.cs` validates peer identity/address consistency. No WPF lifecycle wiring or verified rendezvous HTTPS/advertised-address/host-identity configuration is present in the current implementation.
- **Dependency-plan risk:** host identity/private-key storage is T0130, currently downstream of T0128 and the T0121/T0122 chain. Adding `T0130 -> T0121` now would create a cycle; that graph ordering needs an explicit design correction rather than an inferred edge.
- **Tests:** no new tests were run in this blocker-status attempt. Previous loopback TTL renewal/expiry and CI results remain preserved under T0121-A005; they do not establish WPF integration or remote reachability.
- **Unblock condition:** implement an authenticated peer-reachable inbound request path, stable verified host identity, validated HTTPS rendezvous plus verified advertised address, then wire publisher renewal/stop to participation enable/disable and application shutdown with exact-host-lifecycle acceptance tests.
- **No capability overclaim:** no public address is fabricated or advertised, and no Android-to-Windows P2P success is claimed. `AC-P2P-01` remains **NOT RUN**. `T0122` remains `not_started` because `T0121` is a validated contract/interface prerequisite.

## T0130 dependency review after baseline reset

The current V 3.0.435 graph was re-read before T0130 work. Repository transport contracts show that the independent-node acceptance in T0128 consumes no persistent-key-store API; a stable host identity is instead useful input for that eventual test. The T0128 → T0130 edge is retained but reclassified to sequencing_only with source/test evidence, while T0129 → T0130 is validated as contract_or_interface_dependency. T0121 remains blocked: the current WPF path binds to loopback and lacks a configured peer-reachable address and wired announcement lifecycle.


## T0121-A007 — WPF peer-host lifecycle integration (2026-10-28)

- **Base commit:** `0f7dab4f9db6d343cbbc6a54c341083a2b470081` (`V 3.0.446`), branch `main` was checked before editing.
- **Implementation added:** `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs` starts a libp2p listener using the persisted host identity and an explicitly trusted remote PeerId, forms direct TCP/QUIC announcement addresses, publishes an initial metadata-only announcement to the configured HTTPS rendezvous service, renews its TTL, passes received message frames to the supplied request handler, and stops renewal/disconnects on a peer-session failure or explicit disable/shutdown.
- **WPF integration:** the previous loopback-only scaffold toggle now requires a ready local inference backend, a specific bind IP, advertised IP, fixed port, trusted PeerId, HTTPS rendezvous URL, and operator confirmation that the advertised address/port was tested from the remote peer. The UI confirms that it does not independently probe public reachability. Private keys remain in the Windows DPAPI-backed identity store; peer trust is stored with the OS-protected trust store.
- **Request path:** inbound bytes are bounded and validated by `ProtocolMessageParser.ParseInferenceRequest`; requests pass through `HostRequestCoordinator`, the current host/readiness/capacity state, and `HostInferenceSession.GenerateAsync`. Responses are serialized as the existing `InferenceResponse` contract. Invalid wire messages fail closed and stop lease renewal for that session.
- **Acceptance test added:** `PeerAnnouncementHostLifecycleWorksWithTrustedPeerAsync` runs a real loopback HTTP rendezvous service and two separate libp2p peer transports, checks initial announcement metadata, exchanges a binary request/response through the real lifecycle handler, and asserts disconnection stops renewal and the lease naturally expires. It also verifies that an unconfirmed advertised address is rejected before any announcement.
- **Exact-head CI:** pending. No build/test pass is claimed until the product and quality workflows complete on the final PR head.
- **Known remaining acceptance gap:** the integration test is loopback-hosted. Public-network reachability of a deployment's advertised address and the actual hosted rendezvous endpoint cannot be verified from this repository-only run. The code fails closed unless an operator explicitly confirms the address was independently tested, and the UI avoids claiming the app performs that test. Keep T0121 blocked until exact-head CI and independent-network acceptance are recorded.
- **Status:** partial implementation; no dependency edges changed. T0122 remains not_started.


## T0121-A008 — startup/guard correction, final product check failed (2026-10-28)

- **Candidate:** `b352d775b8583655d1e2414e712e0a1c7fa11292` on PR #6, based on the checked `main` commit `0f7dab4f9db6d343cbbc6a54c341083a2b470081` (`V 3.0.446`).
- **Runtime correction:** the host listener now waits for the first authenticated libp2p session before calling `ReceiveMessageAsync`, whose transport contract intentionally rejects a missing session. This prevents the newly published lease from being stopped immediately after startup when no peer has connected yet.
- **Regression guards:** Python checks now assert the active authenticated-host configuration and fail-closed behavior, while retaining the separate guard that the legacy `ParticipationGatedNetworkListener` remains loopback-only.
- **Passed on this exact candidate:** [Step Three quality gate run 40661160984](https://github.com/artinrahimii/project/actions/runs/40661160984) (191 Python tests; graph 229 tasks/424 edges, zero errors and one preserved T0001 warning); [HostLifecycle job 122724238606](https://github.com/artinrahimii/project/actions/runs/40661160981/job/122724238606), including the loopback rendezvous/authenticated-peer test.
- **Failure retained:** the exact candidate's overall [product workflow 40661160981](https://github.com/artinrahimii/project/actions/runs/40661160981) failed its Windows build with CS0104 for ambiguous `HostCapabilities` and CS1061 because `Google.Protobuf` was not imported for `InferenceResponse.ToByteArray()`. These are compile errors, not accepted product behavior; A009 fixes them. No history was erased.

## T0121-A009 — exact-candidate CI verified (2026-10-28)

- **Candidate source commit:** `c06d0ff57e18c2e0ea8f685692553ab8ba089fe6` (`T0121-A009: resolve WPF protobuf and host capability references`), on PR #6 targeting `main`.
- **Implementation:** added the domain `HostCapabilities` alias and imported `Google.Protobuf` for the existing serialization extension. The host lifecycle waits for an authenticated session, keeps the metadata-only lease renewed while active, passes bounded/parsed requests to the existing host coordinator and live inference session, and stops renewal on disconnection or shutdown. The WPF controls require an explicit trusted PeerId, HTTPS rendezvous URL, fixed bind/advertised addresses and port, plus the operator's separate-network reachability confirmation.
- **Step Three quality gate PASS:** [run 40661258738](https://github.com/artinrahimii/project/actions/runs/40661258738) — 191 Python tests passed; graph guard verified 229 tasks and 424 edges, zero errors and one existing T0001 unmapped-dependency warning.
- **HostLifecycle + peer-host acceptance PASS:** [job 122724562643](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724562643) — the exact candidate's lifecycle suite passed, including the new loopback-hosted announcement, authenticated libp2p request/response, disconnect-driven lease stop and expiry test.
- **Windows WPF Release build/startup/DPAPI PASS:** [job 122724563236](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724563236) — formatting, build, generated Protobuf validation, WPF startup smoke, agent-tool policy checks and Windows DPAPI identity/trust acceptance passed.
- **Android APK/lint/Protobuf PASS:** [job 122724562866](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724562866).
- **Real local inference PASS:** [run 40661258746](https://github.com/artinrahimii/project/actions/runs/40661258746) — pinned llama.cpp/GGUF validation and live local-model inference/cancellation passed.
- **Remaining acceptance gap:** no remote peer on an independent network or configured production rendezvous/port-forwarded address was available in this execution. Loopback CI does not prove public reachability, and the UI's checked confirmation is an operator assertion rather than an active probe by the application. T0121 therefore remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**.
- **Dependency graph:** no edges changed; all 424 edges are preserved, including the validated `T0121 → T0122` contract dependency.


## T0121-A010 — main-head revalidation; deployment acceptance remains blocked (2026-10-28)

- **Exact online baseline:** branch `main`, commit [`d1056208073960132aaa04cdb1568cfb29b13284`](https://github.com/artinrahimii/project/commit/d1056208073960132aaa04cdb1568cfb29b13284), message `V 3.0.447`. Branch and tree were re-read immediately before this audit update.
- **Graph integrity:** `step three/project-graph.md` was obtained from Blob `94b2f9396cf2b988c85e2082b8626910acfdd74f` at that exact commit. The full original content is 2,191,744 UTF-8 bytes; independently recomputed Git Blob SHA-1 matched. The fenced JSON parsed as 229 tasks and 424 edges.
- **Relevant edge review:** `T0120 → T0121` remains a validated contract/interface dependency; `T0115 → T0121` remains validated sequencing-only; `T0121 → T0122` remains a validated contract/interface dependency. Original `type: blocks` fields and every edge were preserved.
- **Exact-head CI status inspection:** the following online jobs were completed successfully for the current baseline: [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40662651891/job/122729230638), [HostLifecycle tests](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729230947), [Windows WPF build](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729231366), [Android APK](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729231269), and [live local inference](https://github.com/artinrahimii/project/actions/runs/40662652089/job/122729231944). This audit inspected the existing exact-head CI results; it did not launch new builds or tests.
- **Local execution limitation:** a shallow `git clone` attempt failed because this execution environment could not resolve `github.com`. No local build or test run is claimed.
- **Why the task stays blocked:** there was no deployment-configured production HTTPS rendezvous endpoint and no second peer on an independent network available to this execution. The application's UI confirmation is an operator attestation, not an active external reachability probe. Therefore public reachability of the advertised TCP/QUIC addresses remains **NOT RUN**.
- **State:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No product source code or dependency edge was changed in this audit. The record documents a blocker revalidation, not software implementation progress.
- **Next:** use a configured HTTPS rendezvous deployment and a peer on a different network to verify the advertised TCP/QUIC endpoints; record that evidence before re-evaluating `T0121` or starting `T0122`.


## T0121-A011 — exact-head CI revalidation; external acceptance still blocked (2026-10-28)

- **Exact online baseline:** `main` at [`e1ebd0895083da5558963911b04fbc9231b266cd`](https://github.com/artinrahimii/project/commit/e1ebd0895083da5558963911b04fbc9231b266cd), message `V 3.0.448`.
- **Graph integrity:** the graph was located in the recursive tree for this exact commit and fetched through Git Blob API because the ordinary contents fetch returned zero characters. The original file content was 2,197,870 UTF-8 bytes; its recomputed Git Blob SHA-1 matched `3533bf1eeaf654469f93f1edaf6c6863b9bd9af0`. The full fenced JSON parsed as 229 tasks and 424 edges.
- **Exact-head CI status inspected:** [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40704091906/job/122871401609), [HostLifecycle tests](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871402110), [Windows WPF build](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871401355), and [Android APK build](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871401840) all completed successfully on this exact head. This attempt inspected the existing CI results; it did not launch a new build/test run.
- **Unresolved acceptance:** no configured production HTTPS rendezvous endpoint or second peer on an independent network was available. Public reachability of the advertised TCP/QUIC addresses remains **NOT RUN**; loopback tests do not prove it.
- **State and impact:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No source files or dependency edges changed; this record is evidence synchronization, not software implementation progress.
- **Next:** run deployment-specific reachability acceptance from an independent network and record the observed results before re-evaluating `T0121`.


## T0121-A012 — exact V 3.0.449 CI completed; external acceptance still blocked (2026-10-28)

- **Exact online baseline:** `main` at [`ff6dc21837e52c9e3e8123d409b3b41aa265c4a5`](https://github.com/artinrahimii/project/commit/ff6dc21837e52c9e3e8123d409b3b41aa265c4a5), message `V 3.0.449`.
- **Graph integrity:** the recursive Git tree for this exact commit identified `step three/project-graph.md` as Blob `506e379f69c53aabdb2f5884cb40f6926886ae35`, size 2,203,126 bytes. The ordinary file fetch returned zero characters, so the graph was re-fetched using Git Blob API. Full content length: 2,203,126 UTF-8 bytes and 2,081,221 text characters. Recomputed Git Blob SHA-1 matched the tree entry; fenced JSON parsed completely as 229 tasks / 424 edges.
- **Exact-head CI inspected:** [Step Three quality gate]( https://github.com/artinrahimii/project/actions/runs/40705066591/job/122874848819 ), [HostLifecycle integration]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874849027 ), [Windows WPF build]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874848582 ), and [Android APK build]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874848980 ) all completed successfully on `V 3.0.449`. These are GitHub Actions results inspected during this audit; no additional test run was launched by this audit.
- **Independent acceptance:** NOT RUN. No configured production HTTPS rendezvous endpoint or second peer on an independent network was available. Loopback integration and operator attestation do not prove public reachability of advertised TCP/QUIC endpoints.
- **Outcome:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No product source or dependency edge changed. This is a durable evidence update, not implementation progress.
- **Next:** perform deployment-specific reachability acceptance from a peer on an independent network, then re-evaluate `T0121` before starting `T0122`.


## T0121-A013 — current-head CI partially completed; external acceptance blocked (2026-10-28)

- **Exact online baseline:** `main` at [`d5f7c1d5416fef22cffa21608b173ed7839a6c38`](https://github.com/artinrahimii/project/commit/d5f7c1d5416fef22cffa21608b173ed7839a6c38), message `V 3.0.450`.
- **Graph/blob integrity:** the full graph and the two existing task evidence files were retrieved by their tree-recorded Blob IDs. Recomputed Git Blob SHA-1 values matched. Graph size: 2,209,184 UTF-8 bytes; text length: 2,087,263 characters; fenced JSON parses as 229 tasks and 424 edges.
- **Exact-head results inspected:** [Step Three quality gate — PASS]( https://github.com/artinrahimii/project/actions/runs/40706062117 ), [Android APK build — PASS]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385398 ), [HostLifecycle tests — PASS]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385751 ).
- **Windows WPF:** [job status]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385648 ) is still `in_progress` at `Set up job`; no completed conclusion is available, so this is neither reported as a pass nor a failure. Overall product workflow: [https://github.com/artinrahimii/project/actions/runs/40706063503]( https://github.com/artinrahimii/project/actions/runs/40706063503 ).
- **Independent-network acceptance:** NOT RUN. No configured production HTTPS rendezvous endpoint or second peer on an independent network was available.
- **Outcome:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No product source or dependency edge changed; this update records new CI evidence and is not implementation progress.
- **Next:** finish inspecting the exact-head Windows build and obtain deployment-specific reachability evidence before re-evaluating `T0121`.


## T0121-A015 — exact V 3.0.452 CI revalidation; external acceptance blocked (2026-10-28)

- **Exact online baseline:** `main` at [`07b430e8f8bb6753e20d8421324fdfc652bfaed4`](https://github.com/artinrahimii/project/commit/07b430e8f8bb6753e20d8421324fdfc652bfaed4), message `V 3.0.452`.
- **Graph integrity:** tree entry `step three/project-graph.md` is Blob `c565a7ee388a1af895c5d1031966ea0c3393d76e`, size 2,219,085 bytes. Git Blob API returned the full file; recomputed Git Blob SHA-1 matched. Content is 2,219,085 UTF-8 bytes and 2,097,136 characters; fenced JSON parses completely as 229 nodes and 424 edges.
- **Exact-head CI inspected:** [Step Three quality gate — PASS]( https://github.com/artinrahimii/project/actions/runs/40708989069 ) and [HostLifecycle integration — PASS]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888764070 ).
- **Pending CI:** [Windows WPF]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888764070 ) and [Android APK]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888763816 ) remained `in_progress` without final conclusions at inspection time. Overall product workflow: [https://github.com/artinrahimii/project/actions/runs/40708988954]( https://github.com/artinrahimii/project/actions/runs/40708988954 ).
- **External acceptance:** NOT RUN. No production HTTPS rendezvous endpoint or peer on an independent network was available. Loopback integration and CI do not establish public TCP/QUIC reachability.
- **Outcome:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No source code or dependency edge changed; this is an evidence synchronization attempt, not implementation progress.
- **Next:** obtain the final Windows/Android CI results and perform independent-network deployment acceptance before re-evaluating `T0121`.


## T0121-A017 — exact V 3.0.454 CI passed; external reachability acceptance unavailable (2026-10-28)

- **Exact online baseline:** [`1572ba5150e4bcd09f1264bb9162c54e081aff2d`](https://github.com/artinrahimii/project/commit/1572ba5150e4bcd09f1264bb9162c54e081aff2d), message `V 3.0.454`.
- **Full graph verification:** Git Trees API identified Blob `f76fb050ac68d4d995aed6ad564bf68e811a9052` in the exact commit tree; Git Blob API returned the complete content; the recomputed SHA-1 Git Blob ID matched. The file has 2,228,168 UTF-8 bytes (2,106,179 Unicode code units); fenced JSON parsed as 229 tasks and 424 edges.
- **Step Three quality gate — PASS:** [run 40709904930](https://github.com/artinrahimii/project/actions/runs/40709904930) — automated tests, Python syntax, graph integrity, and evidence/output validation succeeded.
- **Host lifecycle integration — PASS:** [run 40709904932, job 122892022010](https://github.com/artinrahimii/project/actions/runs/40709904932/job/122892022010).
- **Windows WPF — PASS:** [job 122892022375](https://github.com/artinrahimii/project/actions/runs/40709904932/job/122892022375).
- **Android APK — PASS:** [job 122892022486](https://github.com/artinrahimii/project/actions/runs/40709904932/job/122892022486).
- **Independent-network acceptance — NOT RUN:** no configured production HTTPS rendezvous endpoint or second peer on a separate network was available. CI/loopback results do not satisfy the external reachability requirement.
- **Status:** `T0121` remains `blocked`, `T0122` remains `not_started`, and `AC-P2P-01` remains **NOT RUN**. No code or dependency edge was changed. This is evidence synchronization, not implementation progress.
- **Follow-up:** obtain a production rendezvous URL and independent-network peer, verify the advertised TCP/UDP endpoint, and then re-evaluate T0121.


## T0121-A018 — exact V 3.0.455 CI completed successfully; deployment acceptance remains blocked (2026-10-28)

- **Exact online baseline:** [`8c770adbf0cb51153e301fdbec1ad8ddc242da0a`](https://github.com/artinrahimii/project/commit/8c770adbf0cb51153e301fdbec1ad8ddc242da0a), message `V 3.0.455`.
- **Graph integrity:** exact-tree Git Blob `2a02bd71e2bf4fb7c0295906114bae801196789e` fetched in full; 2,233,276 UTF-8 bytes; recomputed Git Blob SHA-1 matched; full fenced JSON parsed as 229 nodes / 424 edges.
- **Step Three quality gate — PASS:** [run 40711411110](https://github.com/artinrahimii/project/actions/runs/40711411110).
- **HostLifecycle integration — PASS:** [job 122897386880](https://github.com/artinrahimii/project/actions/runs/40711410986/job/122897386880).
- **Windows WPF Release/startup/security — PASS:** [job 122897387158](https://github.com/artinrahimii/project/actions/runs/40711410986/job/122897387158).
- **Android APK/lint/Protobuf — PASS:** [job 122897386482](https://github.com/artinrahimii/project/actions/runs/40711410986/job/122897386482).
- **Independent-network acceptance — NOT RUN:** no configured production HTTPS rendezvous endpoint or peer on an independent network was available. Loopback tests and CI do not establish external reachability.
- **Result:** `T0121` remains `blocked`; `T0122` remains `not_started`; `AC-P2P-01` remains **NOT RUN**. No implementation code or dependency edge changed; this records completed CI evidence, not product implementation progress.
- **Next:** deploy/configure the HTTPS rendezvous endpoint and verify advertised TCP/UDP reachability from a separate peer network before re-evaluating T0121.


## T0121-A019 — exact V 3.0.607 current-head revalidation; external reachability still blocked

Revalidated T0121 against the exact online main head V 3.0.607. The full graph Blob SHA-1 matches the exact tree entry and parses as 229 tasks / 417 edges. Current-head GitHub Actions pass: Step Three quality gate; HostLifecycle integration including PeerAnnouncementClientAcceptanceTests; Windows WPF Release/startup/security; Android lint/APK/unit tests. The WPF participation toggle is wired to PeerAnnouncementHostLifecycle and requests HTTPS rendezvous plus operator-confirmed advertised address. No production rendezvous endpoint or independent-network peer is available in this execution context, so real external TCP/UDP reachability remains NOT RUN. No product source changed; T0121 stays blocked and T0122 stays not_started.

- Exact online main head: `ee74f6d0135049bf97ba3d155674892b833b313a` (`V 3.0.607`).
- Graph blob: `f5a5c72a11f5ff08c4d8760b3a12072c1fa0efcd`; 2,953,201 UTF-8 bytes; recomputed Git Blob SHA-1 matched; full fenced JSON parsed as 229 nodes / 417 edges.
- Step Three quality gate PASS: https://github.com/artinrahimii/project/actions/runs/41021718017 (job 123950320641).
- HostLifecycle unit/integration PASS: https://github.com/artinrahimii/project/actions/runs/41021717940 (job 123950320671).
- Windows WPF build/startup/security PASS: https://github.com/artinrahimii/project/actions/runs/41021717940 (job 123950320733).
- Android APK/lint/unit tests PASS: https://github.com/artinrahimii/project/actions/runs/41021717940 (job 123950320438).
- Source review confirms participation-toggle lifecycle wiring, HTTPS rendezvous validation, and an explicit remote-address operator attestation. The application itself does not verify public reachability.
- Independent-network TCP/UDP acceptance: NOT RUN; no configured production rendezvous endpoint or separate peer was available.
- Outcome: T0121 remains blocked; T0122 remains not_started. No product implementation or edge classification changed. This update is evidence synchronization, not implementation progress.
- Follow-up: configure the production endpoint and perform remote reachability verification from a peer on a separate network.


## T0121-A020 — exact V 3.0.741 main-head revalidation; external reachability remains blocked

- **Exact online baseline:** `main` at `1fde4556b751c7d5139b03879f7d7aaf8f548f33` (`V 3.0.741`), reviewed at 2026-11-01T06:17:34.840Z.
- **Graph integrity:** `step three/project-graph.md` was fetched by Git Blob API from the exact commit tree. Blob `8bb83cd65755d0dc3c19093b393fdd50f4247a0d` matched the recomputed Git SHA-1; original content is 3,562,208 UTF-8 bytes (3,438,507 text characters). The complete fenced JSON parsed as 229 task nodes / 418 edges / 22 sections, with no unresolved edge references.
- **Exact-head CI evidence inspected:** Step Three quality gate **PASS** — https://github.com/artinrahimii/project/actions/runs/41279967441; Windows WPF build/startup/security **PASS** — https://github.com/artinrahimii/project/actions/runs/41279967432/job/124768193055; HostLifecycle unit/integration **PASS** — https://github.com/artinrahimii/project/actions/runs/41279967432/job/124768192954.
- **Android job at inspection time:** still `in_progress`; Kotlin lint and APK build had completed successfully, while Android JVM unit tests, generated Protobuf checks and artifact upload had no final conclusion yet. No Android job pass is claimed for this head.
- **Independent-network acceptance:** **NOT RUN**. No production HTTPS rendezvous endpoint or peer on a separate network is available in this execution context. Loopback CI does not prove advertised public TCP/UDP reachability.
- **Dependency review:** `T0121 → T0122` remains validated as `contract_or_interface_dependency` with the existing `ExpiresAtUtc` / renewable-lease evidence. No edge was changed. T0122 must remain not_started until T0121's end-to-end lease lifecycle acceptance is satisfied.
- **Outcome:** no product source or test code changed in this revalidation. T0121 remains `blocked`; the next action remains deployment/configuration of the production HTTPS rendezvous service and independent-network reachability verification.
- **This attempt is evidence synchronization only, not software implementation progress.**

## T0121-A021 — exact V 3.0.742 CI finalization; external acceptance remains blocked

- **Exact online baseline:** `main` at [`64b913dc65921b6fba0f8491883dea5b10fc4f91`](https://github.com/artinrahimii/project/commit/64b913dc65921b6fba0f8491883dea5b10fc4f91), message `V 3.0.742`.
- **Graph integrity:** exact-tree Blob `67b88a4f333bda145ac077b189407d7c3e6f6b44` was fetched via Git Blob API. Its full content is 3,565,300 UTF-8 bytes (3,441,579 text characters); recomputed Git Blob SHA-1 matched, and the complete fenced JSON parsed as 229 task nodes / 418 edges / 22 sections.
- **Exact-head CI inspected:** [Step Three quality gate — PASS](https://github.com/artinrahimii/project/actions/runs/41280266301); [product workflow — PASS](https://github.com/artinrahimii/project/actions/runs/41280266207), including HostLifecycle unit/integration, Windows WPF Release build/startup/security, and Android lint, debug APK, JVM unit tests, generated Protobuf checks and artifact upload. All three product jobs completed successfully on the exact V 3.0.742 commit.
- **Independent-network acceptance:** **NOT RUN**. No deployed production HTTPS rendezvous endpoint or second peer on an independent network was available in this execution context. CI and loopback integration do not prove advertised public TCP/UDP reachability.
- **Dependency review:** `T0121 → T0122` remains validated as `contract_or_interface_dependency`; no edge was changed. `T0122` remains `not_started` until T0121's external lease/reachability acceptance is satisfied.
- **Outcome:** T0121 remains `blocked`; its attempt count is now 21. The Android CI state recorded as pending in A020 is now finalized as passed. No product source or test code changed, and no implementation-progress credit is added. This is evidence synchronization only.
- **Next:** provide/deploy a production HTTPS rendezvous endpoint and run advertised TCP/UDP reachability acceptance from a peer on a separate network before re-evaluating T0121.



## T0121-A022 — exact V 3.0.743 CI passed; independent-network acceptance remains blocked

- **Exact online baseline:** `main` at [`ae2e8cf5fb6874230e8068e160adbe52d9db2dcf`](https://github.com/artinrahimii/project/commit/ae2e8cf5fb6874230e8068e160adbe52d9db2dcf), message `V 3.0.743`.
- **Graph integrity:** retrieved `step three/project-graph.md` from this commit's exact tree Blob `859a17a36bba64ab8211a67a0e95ccc779a950f5`. The complete original content is 3,569,190 UTF-8 bytes (3,445,461 text characters); recomputed Git Blob SHA-1 matched. The complete fenced JSON parsed as 229 task nodes / 418 edges / 22 sections, with no duplicate task IDs or unresolved edge references.
- **Exact-head CI evidence:** [Step Three quality gate — PASS](https://github.com/artinrahimii/project/actions/runs/41280778924); [HostLifecycle unit/integration — PASS](https://github.com/artinrahimii/project/actions/runs/41280778977/job/124772090697); [Windows WPF Release/startup/security — PASS](https://github.com/artinrahimii/project/actions/runs/41280778977/job/124772090858); [Android lint/APK/JVM/generated Protobuf — PASS](https://github.com/artinrahimii/project/actions/runs/41280778977/job/124772090831). All jobs and steps were completed successfully on the exact V 3.0.743 head.
- **Independent-network acceptance:** **NOT RUN**. No production HTTPS rendezvous endpoint or peer on a separate network was available in this execution. CI and loopback tests do not establish public advertised TCP/UDP reachability.
- **Dependency review:** `T0120 → T0121` remains validated as `contract_or_interface_dependency`; `T0115 → T0121` remains validated as `sequencing_only`; `T0121 → T0122` remains validated as `contract_or_interface_dependency`. No edge was changed. `T0122` remains `not_started` until T0121's external lease/reachability acceptance is completed.
- **Outcome:** T0121 remains `blocked`; attempt count is now 22. No product source or test code changed, and no implementation-progress credit is added. This attempt records exact-head CI evidence only.
- **Next:** provide/deploy the production HTTPS rendezvous endpoint and run advertised TCP/UDP reachability acceptance from a peer on an independent network before re-evaluating T0121.


## T0121-A023 — exact V 3.0.744 CI passed; independent-network acceptance remains blocked

- **Exact online baseline:** `main` at [`6783231ca0eaebfb2ae4680abea836d6ca157020`](https://github.com/artinrahimii/project/commit/6783231ca0eaebfb2ae4680abea836d6ca157020), message `V 3.0.744`.
- **Graph integrity before update:** Blob `aeca86edfb7607de97dbc1c785fb78ba65ef01e9` from the exact V 3.0.744 tree was fetched in full and its complete JSON parsed as 229 nodes / 418 edges / 22 sections. The file contains 3,560,003 UTF-8 bytes and 3,436,348 text characters; recomputed Git Blob SHA-1 matched the tree entry.
- **Exact-head CI evidence:** [Step Three quality gate — PASS](https://github.com/artinrahimii/project/actions/runs/41281088547); [HostLifecycle unit/integration — PASS](https://github.com/artinrahimii/project/actions/runs/41281088539/job/124772993825); [Windows WPF Release/startup/security — PASS](https://github.com/artinrahimii/project/actions/runs/41281088539/job/124772993636); [Android lint/APK/JVM/generated Protobuf — PASS](https://github.com/artinrahimii/project/actions/runs/41281088539/job/124772993886). All four jobs completed successfully on the exact V 3.0.744 head.
- **Independent-network acceptance:** **NOT RUN**. No production HTTPS rendezvous endpoint or peer on an independent network was available. CI and loopback tests do not establish public advertised TCP/UDP reachability.
- **Dependency review:** existing validated edges `T0120 → T0121`, `T0115 → T0121`, and `T0121 → T0122` were preserved. No edge was changed. T0122 remains `not_started` until T0121's external lease/reachability acceptance is completed.
- **Outcome:** T0121 remains `blocked`; attempt count is now 23. No product source or test code changed; no implementation-progress credit is added. This attempt records exact-head CI evidence only.
- **Next:** provide/deploy the production HTTPS rendezvous endpoint and run advertised TCP/UDP reachability acceptance from a peer on an independent network before re-evaluating T0121.


## T0121-A024 — exact V 3.0.745 CI passed; independent-network acceptance remains blocked (2026-11-01)

- **Exact online baseline:** [`V 3.0.745`](https://github.com/artinrahimii/project/commit/279795f1d1bcb2cd6fbeff0181ab1e1c2104ddd4) (`279795f1d1bcb2cd6fbeff0181ab1e1c2104ddd4`).
- **Graph integrity:** exact commit tree `aa702e7b3a5c584cbb222df368e188d6d7706ac6` identifies `step three/project-graph.md` as Blob `1ac2bdcc62693eff794a68e57dbb1985c353100b`; the complete Blob was recovered after the ordinary file fetch returned empty. The original content is 3,564,656 UTF-8 bytes (3,440,993 text characters), its recomputed Git Blob SHA-1 matches, and the complete fenced JSON parses as 229 nodes / 418 edges with no duplicate task IDs or unresolved edge references.
- **Exact-head Step Three quality gate — PASS:** [run 41284224821](https://github.com/artinrahimii/project/actions/runs/41284224821); automated tests, Python syntax, graph structure, and recorded evidence/output validation all succeeded.
- **Exact-head HostLifecycle unit/integration — PASS:** [job 124782140148](https://github.com/artinrahimii/project/actions/runs/41284224801/job/124782140148); lifecycle test steps completed successfully.
- **Exact-head Windows WPF — PASS:** [job 124782140161](https://github.com/artinrahimii/project/actions/runs/41284224801/job/124782140161); formatting, host build, Protobuf, WPF startup smoke, agent-tool policy/process/capability checks, DPAPI identity/trust storage, and artifact upload all succeeded.
- **Exact-head Android — PASS:** [job 124782140230](https://github.com/artinrahimii/project/actions/runs/41284224801/job/124782140230); Kotlin lint, APK build, JVM tests, peer interoperability fixture, Protobuf verification, and artifact upload all succeeded.
- **Independent-network acceptance — NOT RUN:** no deployed production HTTPS rendezvous endpoint or peer on an independent network was available. CI and loopback integration do not establish advertised public TCP/UDP reachability.
- **Dependency review:** the validated edges `T0120 → T0121`, `T0115 → T0121`, and `T0121 → T0122` remain unchanged. The first and third are contract/interface dependencies; the second is sequencing-only.
- **Outcome:** `T0121` remains `blocked`; `T0122` remains `not_started`. No product source, tests, or dependency edges changed. This is exact-head CI evidence synchronization, not implementation progress.
- **Next:** deploy/configure a production HTTPS rendezvous endpoint and run advertised TCP/UDP reachability acceptance from a peer on a separate network before re-evaluating `T0121`.


## T0121-A025 — exact V 3.0.746 CI passed; public reachability remains blocked (2026-11-01)

- **Exact online baseline:** [`V 3.0.746`](https://github.com/artinrahimii/project/commit/8407c362c61446dd7dc835ef492c702b75888166) (`8407c362c61446dd7dc835ef492c702b75888166`).
- **Exact-head Step Three quality gate — PASS:** [run 41291144159](https://github.com/artinrahimii/project/actions/runs/41291144159); automated tests, Python syntax, graph integrity, and recorded output/evidence checks succeeded.
- **Exact-head HostLifecycle unit/integration — PASS:** [job 124802499244](https://github.com/artinrahimii/project/actions/runs/41291144174/job/124802499244).
- **Exact-head Windows WPF build/startup/security — PASS:** [job 124802499636](https://github.com/artinrahimii/project/actions/runs/41291144174/job/124802499636); formatting, build, generated Protobuf, startup smoke, policy/process and DPAPI checks completed successfully.
- **Exact-head Android — PASS:** [job 124802499379](https://github.com/artinrahimii/project/actions/runs/41291144174/job/124802499379); Kotlin lint, APK, JVM, peer-interoperability, generated Protobuf and artifact upload steps completed successfully.
- **External acceptance — NOT RUN:** the repository tree contains no deployment descriptor or deployment workflow for a production Rendezvous endpoint, and no independently networked peer/production HTTPS endpoint was available in this execution. CI and loopback tests do not prove advertised public TCP/UDP reachability.
- **Dependency outcome:** T0121 remains `blocked`; T0122 remains `not_started`. No product source or test code changed. This is exact-head CI evidence synchronization, not implementation progress.
- **Next:** provision/deploy a production HTTPS Rendezvous endpoint and test the advertised host address from an independent network before re-evaluating T0121.


## T0121-A026 — add deployable container and production smoke coverage (2026-11-01)

- **Exact online baseline:** [`V 3.0.747`](https://github.com/artinrahimii/project/commit/3016440610e56682cc08a8800b9979bff019c136) (`3016440610e56682cc08a8800b9979bff019c136`).
- **Implementation:** added `src/Rendezvous.Service/Dockerfile` (multi-stage .NET 10 publish, non-root UID/GID 10001, HTTP container port 8080), Dockerfile-specific build-context exclusions, and `docs/deployment/rendezvous-service.md`.
- **Service change:** added `GET /healthz` returning a minimal `{ status: "ok" }` health response.
- **Automated smoke coverage:** added `tests/rendezvous_container_smoke.py` and `.github/workflows/rendezvous-container.yml`. The workflow builds and starts the image, verifies its process is non-root, checks health, publishes a valid metadata announcement, discovers it by capability, and verifies expired/invalid metadata rejection.
- **Operational constraint:** `PeerAnnouncementDirectory` is in-memory and singleton-scoped. Production must run one replica behind HTTPS termination. Multi-replica deployment is unsafe until a shared expiry-aware directory store is implemented. The container itself serves private HTTP; this commit does not create a public HTTPS endpoint.
- **Validation at commit creation:** the container workflow is newly added and has not run on this commit yet. No test/build pass is claimed in advance.
- **External acceptance:** public HTTPS deployment and advertised TCP/UDP/QUIC reachability from an independent peer remain **NOT RUN**. The smoke script uses synthetic metadata only and is not a reachability test.
- **Outcome:** T0121 remains `blocked`; T0122 remains `not_started`. This attempt advances deployment readiness but does not complete the external acceptance criterion.
- **Next:** verify the exact-commit container workflow, deploy a single replica behind a real HTTPS endpoint, configure both clients, and test authenticated reachability from a separate network.


## T0121-A026 — exact V 3.0.748 container smoke result correction (2026-11-01)

- **Workflow:** [Rendezvous container build and smoke test — run 41293258833](https://github.com/artinrahimii/project/actions/runs/41293258833).
- **Passed:** Docker image built successfully; container ran as a non-root UID; `GET /healthz` returned 200; a valid announcement returned 204; capability discovery returned 200; invalid capability returned 400.
- **Failed:** expired announcement returned HTTP 500 instead of 400. Kestrel showed `ArgumentOutOfRangeException` from the `PeerAnnouncement` constructor during automatic minimal-API JSON parameter binding, before the handler's exception handling. Invalid-address assertion was not reached.
- **Correction:** see T0121-A027; JSON deserialization is moved inside `HandleAnnouncement` so invalid metadata is translated to a safe 400 response. This is a real API defect discovered by the new smoke test; the A026 failure is retained.

## T0121-A027 — fix invalid announcement HTTP status handling (2026-11-01)

- **Baseline:** exact `V 3.0.748` commit `d88163b11db85787244957d2837fa2b53cf5e9d8`.
- **Changed source:** `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs` now reads the JSON body inside `HandleAnnouncement` and catches constructor argument failures, malformed JSON and `BadHttpRequestException`, returning the corresponding safe client-error status. Directory-capacity failures remain HTTP 503.
- **Regression coverage:** the existing container smoke test already asserts that expired announcements and addresses without a matching peer ID both return HTTP 400. Exact-commit container build/smoke is pending; no pass is claimed until it completes.
- **External acceptance:** production HTTPS deployment and advertised TCP/UDP/QUIC reachability from an independent peer remain **NOT RUN**.
- **Outcome:** T0121 remains `blocked`; T0122 remains `not_started`. The runtime API error is corrected in source, but external reachability is still a release blocker.

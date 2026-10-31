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

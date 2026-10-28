# T0121 verification evidence — historical CI and exact-candidate integration results

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


## T0121-A005 — exact-commit CI verification (2026-10-27)

- **Exact implementation/test commit:** [`8a49b48259f693dc2ddc04e846a78fe15bff8227` (`V 3.0.368`)](https://github.com/artinrahimii/project/commit/8a49b48259f693dc2ddc04e846a78fe15bff8227).
- **Step Three quality gate:** PASS — [run 40456065310](https://github.com/artinrahimii/project/actions/runs/40456065310); all automated test, Python syntax, graph, and output/evidence steps completed successfully.
- **HostLifecycle acceptance tests:** PASS — [job 122029550003](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029550003), including the corrected T0121 acceptance suite.
- **Windows WPF build/startup checks:** PASS — [job 122029549657](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029549657).
- **Android APK/lint/Protobuf:** PASS — [job 122029549997](https://github.com/artinrahimii/project/actions/runs/40456065291/job/122029549997).
- **Live local inference integration:** PASS — [run 40456065350](https://github.com/artinrahimii/project/actions/runs/40456065350), including the pinned llama.cpp/GGUF and actual local-model inference step.
- **Acceptance boundary:** these results verify the exact V 3.0.368 build and tests, not host network-participation integration. The publisher is still disconnected from the WPF toggle, and no validated public rendezvous endpoint plus peer-reachable address/identity configuration exists. T0121 therefore remains `in_progress`.
- **Graph integrity:** the unchanged T0121 -> T0122 edge remains `unreviewed`; no edge classification was changed in this attempt.
- **Product change:** none in this attempt; this section records observed CI output only.

## T0121-A007 — exact-source and test scope (2026-10-28)

- **Base:** `0f7dab4f9db6d343cbbc6a54c341083a2b470081` (`V 3.0.446`), with the graph Blob resolved from the exact main commit tree.
- **Implementation paths:** `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`, `src/Host.Windows/MainWindow.xaml.cs`, `src/Host.Windows/MainWindow.xaml`.
- **Test path:** `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`; the test project links the lifecycle class from the production source tree in `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- **Graph path:** `step three/project-graph.md`. The original large-graph content was retrieved through the Git Blob API after normal file-content fetch returned zero characters. Before this candidate edit, the source Blob SHA was `0be6e6a1cc26a429bbc1800f175be0efab6b567e`, size was 2,175,654 UTF-8 bytes and the computed Git SHA-1 matched the tree entry; the complete fenced JSON parsed as 229 nodes / 424 edges. The graph update preserves the original JSON formatting and all existing dependency edges.
- **Validation still pending:** exact-PR-head Step Three quality gate, HostLifecycle integration test, Windows WPF build/startup checks and Android product checks. An external peer on a separate network and a deployment-specific HTTPS rendezvous endpoint were not available in this run. No claim of public reachability or AC-P2P-01 success is made.


## T0121-A008/A009 — candidate failures preserved and corrected (2026-10-28)

- **Large graph baseline:** exact main commit `0f7dab4f9db6d343cbbc6a54c341083a2b470081`. Its graph Blob `0be6e6a1cc26a429bbc1800f175be0efab6b567e` was retrieved after ordinary fetch returned an empty string; the complete file was 2,175,654 UTF-8 bytes and its computed Git Blob SHA-1 matched. The original fenced JSON parsed as 229 tasks/424 edges.
- **Updated candidate graph before this verification record:** Blob `62bae2ed10194eea1b6f5ce69633e90db26b9b66`, 2,179,623 UTF-8 bytes (2,057,762 JavaScript string characters); the computed Git Blob SHA-1 matched, JSON parsed completely, and all 229 nodes/424 edges remained present. `T0121` remains blocked and `T0122` not started.
- **A008 failure preserved:** [quality run 40661160984](https://github.com/artinrahimii/project/actions/runs/40661160984) and [HostLifecycle job 122724238606](https://github.com/artinrahimii/project/actions/runs/40661160981/job/122724238606) passed on candidate `b352d775b8583655d1e2414e712e0a1c7fa11292`, but [overall product run 40661160981](https://github.com/artinrahimii/project/actions/runs/40661160981) failed Windows compilation because `HostCapabilities` was ambiguous and `Google.Protobuf` was missing from scope. No failed outcome was removed; A009 repairs those exact errors.
- **A009 exact-candidate source:** `c06d0ff57e18c2e0ea8f685692553ab8ba089fe6`.
- **Quality gate PASS:** [40661258738](https://github.com/artinrahimii/project/actions/runs/40661258738) — 191 Python tests, graph/evidence checks, 229 tasks/424 edges, zero errors and one preserved T0001 warning.
- **Host lifecycle/peer-host integration PASS:** [40661258731 job 122724562643](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724562643).
- **Windows build/startup/DPAPI PASS:** [40661258731 job 122724563236](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724563236).
- **Android build/lint/Protobuf PASS:** [40661258731 job 122724562866](https://github.com/artinrahimii/project/actions/runs/40661258731/job/122724562866).
- **Live local inference PASS:** [40661258746](https://github.com/artinrahimii/project/actions/runs/40661258746).
- **External reachability not run:** this execution has no separate-network remote peer/deployment rendezvous endpoint. No claim is made that the public advertised address works; `AC-P2P-01` remains **NOT RUN**.


## 2026-10-28 — exact online main-head revalidation

- Baseline: `main` at `d1056208073960132aaa04cdb1568cfb29b13284` (`V 3.0.447`).
- Graph integrity: Blob `94b2f9396cf2b988c85e2082b8626910acfdd74f`, 2,191,744 UTF-8 bytes; recomputed Git Blob SHA-1 matched the tree entry. Full fenced JSON parsed as 229 tasks / 424 edges.
- Exact-head CI jobs inspected and confirmed successful: [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40662651891/job/122729230638), [HostLifecycle integration](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729230947), [Windows WPF](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729231366), [Android APK](https://github.com/artinrahimii/project/actions/runs/40662651874/job/122729231269), and [live local inference](https://github.com/artinrahimii/project/actions/runs/40662652089/job/122729231944). These statuses were inspected; no new test run was launched during this audit.
- Independent-network acceptance: **NOT RUN**. No configured production HTTPS rendezvous endpoint or second peer on an independent network was available. Public reachability must not be inferred from loopback CI.
- Local clone: attempted and failed because `github.com` could not be resolved in this environment; no local build/test is claimed.
- Result: T0121 stays blocked, T0122 stays not_started, and AC-P2P-01 stays NOT RUN. No dependency edges were changed.


## 2026-10-28 — exact V 3.0.448 CI revalidation

- Baseline: `main` at `e1ebd0895083da5558963911b04fbc9231b266cd` (`V 3.0.448`).
- Graph read method: Git Trees API -> exact graph Blob `3533bf1eeaf654469f93f1edaf6c6863b9bd9af0` -> Git Blob API. The complete original content was 2,197,870 UTF-8 bytes; recomputed Git Blob SHA-1 matched the tree entry; fenced JSON parsed as 229 tasks and 424 edges.
- Exact-head CI completed successfully: [quality gate](https://github.com/artinrahimii/project/actions/runs/40704091906/job/122871401609), [HostLifecycle integration](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871402110), [Windows host build](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871401355), and [Android APK build](https://github.com/artinrahimii/project/actions/runs/40704092070/job/122871401840).
- Independent-network reachability acceptance remains **NOT RUN** because no deployed HTTPS rendezvous endpoint or external peer was available. T0121 remains blocked; T0122 remains not_started; AC-P2P-01 remains NOT RUN.
- No dependency edge changed, and no product implementation was modified in this verification-only attempt.


## Exact online main V 3.0.449 — post-CI revalidation (2026-10-28)

- Baseline: `main` at `ff6dc21837e52c9e3e8123d409b3b41aa265c4a5` (`V 3.0.449`); commit tree `53335766a2d5157876a5d1cf2a694f33f50254da`.
- Graph tree entry: `step three/project-graph.md`, Blob `506e379f69c53aabdb2f5884cb40f6926886ae35`, 2,203,126 bytes. The ordinary file fetch returned empty content; Git Blob API recovered the full file. Recomputed Git Blob SHA-1 exactly matched the tree entry. Original text is 2,081,221 characters; UTF-8 body is 2,203,126 bytes. Fenced JSON parsed as 229 nodes and 424 edges.
- Exact-head CI results inspected and completed successfully: [quality gate]( https://github.com/artinrahimii/project/actions/runs/40705066591/job/122874848819 ), [HostLifecycle integration]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874849027 ), [Windows WPF build]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874848582 ), [Android APK build]( https://github.com/artinrahimii/project/actions/runs/40705066594/job/122874848980 ). No new local tests/builds were claimed.
- Independent-network acceptance remains **NOT RUN** because no deployed HTTPS rendezvous endpoint or second peer was available. `T0121` stays blocked, `T0122` stays not_started, and `AC-P2P-01` stays **NOT RUN**. No source code or edge was changed.


## Exact online main V 3.0.450 — partial product CI revalidation (2026-10-28)

- Baseline: `main` at `d5f7c1d5416fef22cffa21608b173ed7839a6c38` (`V 3.0.450`); tree `4d54257346062dc4e1d902bac29338b48a60d17b`.
- Graph Blob `ee2097c43cada89d2581c1ac14682b33414b2190`: 2,209,184 UTF-8 bytes, 2,087,263 characters; recomputed Git Blob SHA-1 matched. Full fenced JSON parsed as 229 tasks / 424 edges.
- Exact-head CI: [quality gate]( https://github.com/artinrahimii/project/actions/runs/40706062117 ), [Android APK]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385398 ), and [HostLifecycle tests]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385751 ) completed successfully. [Windows WPF build]( https://github.com/artinrahimii/project/actions/runs/40706063503/job/122878385648 ) remains `in_progress` at `Set up job`; no final outcome was recorded during this check.
- External acceptance remains **NOT RUN** because no production HTTPS rendezvous endpoint or independent-network peer was available. `T0121` remains blocked; `T0122` remains not_started; `AC-P2P-01` remains **NOT RUN**. No source code or dependency edge was changed.


## Exact online main V 3.0.452 — CI snapshot (2026-10-28)

- Baseline: `main` at `07b430e8f8bb6753e20d8421324fdfc652bfaed4` (`V 3.0.452`), tree `5cf93b16eadf62cd72bb7df4c98feb8dfc4156e7`.
- Graph Blob `c565a7ee388a1af895c5d1031966ea0c3393d76e`: 2,219,085 UTF-8 bytes, 2,097,136 text characters; recomputed Git Blob SHA-1 matched the tree entry. Complete fenced JSON parsed as 229 tasks / 424 edges.
- Exact-head [quality gate]( https://github.com/artinrahimii/project/actions/runs/40708989069 ) and [HostLifecycle integration]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888764070 ) completed successfully. [Windows WPF build]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888764070 ) and [Android APK build]( https://github.com/artinrahimii/project/actions/runs/40708988954/job/122888763816 ) remain in progress; no final result was recorded during this check.
- Independent-network acceptance remains **NOT RUN**. T0121 remains blocked, T0122 remains not_started, and AC-P2P-01 remains **NOT RUN**. No product code or dependency edge changed.

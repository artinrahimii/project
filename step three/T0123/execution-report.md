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

## T0123-A002 — HostStatus path compiled; Kotlin style gate failed

- **Exact verification commit:** `V 3.0.754` — `d38558244b9bcff6b2a1c42c1421d086872d3f27`.
- **HostLifecycle:** PASS — the authenticated loopback HostStatusRequest/HostStatus exchange passed within [job 124836380905](https://github.com/artinrahimii/project/actions/runs/41302568558/job/124836380905). The test verified the matching host PeerId, READY status, active request count and server-generated fresh timestamp.
- **Windows WPF:** PASS — build, Protobuf, startup, security and DPAPI checks passed in [job 124836380718](https://github.com/artinrahimii/project/actions/runs/41302568558/job/124836380718).
- **Android .NET fixture:** PASS — shared C# peer fixture compiled in [job 124836380819](https://github.com/artinrahimii/project/actions/runs/41302568558/job/124836380819).
- **Android KtLint:** FAILED — formatting in the new Android transport and test files violated multiline-expression, newline and trailing-comma rules. The APK and Android JVM test stages were skipped; no Android probe test pass is claimed.
- **Other checks:** [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41302568562) and [Rendezvous container smoke](https://github.com/artinrahimii/project/actions/runs/41302568585) passed.
- **Correction in A003:** reflowed the affected Kotlin expressions, conditions and constructors to match existing KtLint style. This change is formatting-only; it does not alter the HostStatus wire format or runtime semantics.
- **Task state:** T0123 remains `in_progress`. A002 is preserved as failed because Android's style gate blocked compilation and runtime testing.

## T0123-A003 — Android Kotlin formatting correction

- **Changed files:** `Libp2pInferenceRequestTransport.kt` and `Libp2pInferenceRequestTransportTest.kt`.
- **Correction:** split multiline expressions after assignment, wrapped multiline boolean conditions with trailing commas, and formatted constructor/when/try expressions in repository style.
- **Verification:** exact V 3.0.755 KtLint, Android APK/JVM, Windows and HostLifecycle CI are pending. No success is claimed for A003 yet.


## T0123-A003 — exact V 3.0.755 CI result

- **Commit:** `d5858762bb9f1e6d672c2d9f861a81a1187dcbea`.
- **Passed:** HostLifecycle authenticated peer-status loopback and regression suite [job 124837268094](https://github.com/artinrahimii/project/actions/runs/41302871481/job/124837268094); Windows WPF build/startup/security job [124837268396](https://github.com/artinrahimii/project/actions/runs/41302871481/job/124837268396); Step Three quality gate [41302871471](https://github.com/artinrahimii/project/actions/runs/41302871471); Rendezvous container build and smoke [41302871554](https://github.com/artinrahimii/project/actions/runs/41302871554).
- **Failed:** Android KtLint test source set rejected the indentation/chain layout in `Libp2pInferenceRequestTransportTest.kt`. The production transport source set passed KtLint. Android APK and JVM tests were skipped.
- **Exact evidence:** [Product workflow 41302871481](https://github.com/artinrahimii/project/actions/runs/41302871481).
- **Task state:** T0123 remains `in_progress`; A003 is recorded as failed because Android runtime validation did not run.

## T0123-A004 — Android JVM test formatting correction

- **Base:** exact online `main` commit `d5858762bb9f1e6d672c2d9f861a81a1187dcbea` (`V 3.0.755`).
- **Changes:** corrected only the remaining KtLint test-file diagnostics: builder chain indentation, `when` branch indentation, and constructor argument/closing-parenthesis indentation. No test assertions, wire behavior or transport logic are changed.
- **Validation:** pending exact-head product workflow. T0123 remains `in_progress` until Android lint, APK and JVM checks pass.


## T0123-A004 — exact V 3.0.756 CI result

- **Commit:** `fdf51f72fa9d389aeed11709db8a870c036fac2f`.
- **Passed:** HostLifecycle regression [job 124838033151](https://github.com/artinrahimii/project/actions/runs/41303131592/job/124838033151); Step Three quality gate [41303131602](https://github.com/artinrahimii/project/actions/runs/41303131602); Rendezvous container build and smoke [41303131595](https://github.com/artinrahimii/project/actions/runs/41303131595). The Windows WPF job passed its build and startup/security checks.
- **Failed:** Android KtLint in the production source set reported indentation and chain-layout errors in `Libp2pInferenceRequestTransport.kt` within the status-probe implementation. The Android JVM test source set passed KtLint. Android APK/JVM steps were skipped.
- **Exact evidence:** [Product workflow 41303131592](https://github.com/artinrahimii/project/actions/runs/41303131592).
- **Task state:** T0123 remains `in_progress`; A004 is recorded as failed because the production Android source gate prevented APK and JVM execution.

## T0123-A005 — Android production transport formatting correction

- **Base:** exact online `main` commit `fdf51f72fa9d389aeed11709db8a870c036fac2f` (`V 3.0.756`).
- **Changes:** corrected only Kotlin continuation indentation and member-chain layout in `probeHostStatus` and the HostStatus receive branch. No runtime or protocol behavior is intentionally changed. The JVM test formatting from A004 is retained.
- **Validation:** pending exact-head product workflow. T0123 remains `in_progress` until Kotlin lint, debug APK build and Android JVM transport acceptance pass together with HostLifecycle and WPF checks.


## T0123-A005 — exact V 3.0.757 CI result

- **Commit:** `9b6b1e9e87bb7e1c0ff6c321dbd81d65565961b9`.
- **Passed:** Android production/test KtLint; Android .NET peer interoperability fixture; HostLifecycle unit/integration suite; Windows WPF build, startup, security and DPAPI checks.
- **Failed:** APK compilation at `Libp2pInferenceRequestTransport.kt:650:22` — `No value passed for parameter 'p2'`. The controller close callback is a two-parameter function `(PeerId, InferencePeerMessageController) -> Unit`, but the exception branch passed only the peer ID. Android JVM tests and generated Protobuf verification were skipped.
- **Exact evidence:** [Product workflow 41303424789](https://github.com/artinrahimii/project/actions/runs/41303424789).
- **Task state:** T0123 remains `in_progress`; A005 is failed because the Android APK did not compile.

## T0123-A006 — peer controller close-callback correction

- **Base:** exact online `main` commit `9b6b1e9e87bb7e1c0ff6c321dbd81d65565961b9` (`V 3.0.757`).
- **Change:** changed the exception path to call `onClosed(peerId, this)`, satisfying the existing two-parameter callback contract and retaining the controller identity needed for stream cleanup.
- **Validation:** pending exact-head Android APK and JVM CI. T0123 remains `in_progress`.


## T0123-A006 — exact V 3.0.758 CI result

- **Commit:** `ee88a6d6d433b1eb5b2916c7b6a3fd4dae29706e`.
- **Passed:** Android KtLint (main/test), .NET peer-interoperability fixture, debug APK build, Android JVM unit tests, generated Protobuf Java/Kotlin verification and APK artifact upload. The Android JVM run included the authenticated live HostStatus request/response round-trip.
- **Passed:** HostLifecycle unit/integration; Windows WPF build, startup, security and DPAPI checks; Step Three quality gate; Rendezvous container build and smoke.
- **Exact evidence:** [Product workflow 41303786641](https://github.com/artinrahimii/project/actions/runs/41303786641), [Step Three quality gate 41303786616](https://github.com/artinrahimii/project/actions/runs/41303786616), [Rendezvous smoke 41303786689](https://github.com/artinrahimii/project/actions/runs/41303786689).
- **Task state:** A006 succeeded. Review of the Protobuf contract then identified that `active_requests` is `uint32`; generated Kotlin exposes it as a signed `Int`, so wire values above `Int.MAX_VALUE` may appear negative. A007 adds a lower bound and an authenticated loopback regression test; T0123 remains `in_progress` pending that exact-head run.

## T0123-A007 — reject signed representation of out-of-range uint32

- **Base:** exact online `main` commit `ee88a6d6d433b1eb5b2916c7b6a3fd4dae29706e` (`V 3.0.758`).
- **Change:** the accepted active-request range is now `0..MAXIMUM_HOST_STATUS_ACTIVE_REQUESTS`, rejecting negative Kotlin representations of invalid unsigned wire values.
- **Regression test:** added an authenticated libp2p loopback test that sends `active_requests = -1` through the Protobuf wire representation and asserts that the Android probe rejects it.
- **Validation:** pending exact-head Android lint, APK and JVM checks plus product/quality regressions.


## T0123-A007 — exact V 3.0.759 verification completed

- **Verified commit:** `450b0bd8ab937e6a9b8b0605521efa1bdde3dca7` (`V 3.0.759`).
- **Android Kotlin:** production and test KtLint passed.
- **Android APK:** `assembleDebug` passed and the `whitep-android-debug-apk` artifact uploaded.
- **Android JVM:** `testDebugUnitTest` passed. The checked-in suite includes `probesFreshHostStatusOverTheAuthenticatedPeerStream` and `rejectsNegativeUnsignedActiveRequestCountOverAuthenticatedPeerStream`; the latter sends the negative Kotlin `Int` representation of an out-of-range Protobuf `uint32` over the authenticated peer stream and verifies rejection.
- **Protobuf:** generated Java/Kotlin source verification passed.
- **Windows/Host:** HostLifecycle unit/integration passed; Windows WPF Release build passed with zero warnings and zero errors, along with startup, agent-tool process/policy/capability and current-user DPAPI identity/trust checks.
- **Other regression gates:** [Step Three quality gate 41304338796](https://github.com/artinrahimii/project/actions/runs/41304338796) and [Rendezvous container build/smoke 41304338927](https://github.com/artinrahimii/project/actions/runs/41304338927) passed. The full [White-P product build 41304338914](https://github.com/artinrahimii/project/actions/runs/41304338914) completed successfully.
- **Acceptance boundary:** this closes the live authenticated heartbeat/readiness and Android fresh-status probe scope. It does not claim public HTTPS or independent-network operation; those remain final-only T0230/T0128.
- **Task state:** T0123 is now `done`; A007 is recorded as succeeded. Next task is T0124, subject to its validated incoming dependency edges.

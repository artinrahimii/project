# T0180-A001 — evidence and exact-base review

- Repository/branch: `artinrahimii/project` / `main`
- Base commit: `1c329bd92f403ee380962dc7a237a3fce19bf84e` (`V 3.0.821`)
- Base tree: `c09c7f2eb063f18fb4407d59d980e4358850eb79`
- Graph Blob at base: `801a1ec718d82436e90f9e6cabaa1e389659a6a3`; exact base-tree entry confirmed and full graph parsed as 230 tasks / 422 edges.
- Source and test files were read at the exact base revision; each fetched Blob SHA matched its recursive-tree entry.
- T0177 and T0179 were `done`; their reports, evidence and relevant implementation interfaces were checked.
- `T0177 → T0180` and `T0179 → T0180` are validated `sequencing_only`; original `blocks` types and execution order remain unchanged. Evidence/reasons are appended to graph.edge_change_log.
- `T0180 → T0181` remains unreviewed.
- CI for the new exact head is pending; no test pass is claimed yet.

Changed code implements zeroization for Windows frames/queue/lifecycle buffers and Android decoder/controller buffers. Android JVM tests inspect retained backing arrays; Python source-contract regression tests cover ownership paths. Limits of the zeroization guarantee are documented in `docs/security.md`.


## T0180-A002 — exact-base review and validation limits

- Base commit: `b14a8c311cd98422c5b15ddb9089d24f3170edc8` (`V 3.0.822`); target branch: `main`.
- The graph's ordinary file-fetch response was empty (0 characters) despite returning Blob `12e8b52f163c3b221df2c731dbb2c911c6257097`. The separate Blob fetch returned 3,825,072 characters / 3,951,827 UTF-8 bytes. A locally implemented Git SHA-1 over `blob 3951827\\0` plus the complete original fenced Markdown bytes produced the same Blob SHA `12e8b52f163c3b221df2c731dbb2c911c6257097`.
- Full JSON parsing succeeded after removing the Markdown fence only in a separate parsing copy. Parsed graph: 230 nodes, 422 edges, 230 execution-order entries, no duplicate task IDs and no unresolved edge references.
- The connected GitHub actions do not expose a read-tree operation. The exact tree SHA used as the base for reconstructing a single safe multi-file commit is retained in the prior A001 record: `c09c7f2eb063f18fb4407d59d980e4358850eb79` (commit `V 3.0.821`). The resulting commit will be compared against the current `main` parent before advancing the ref; any unexpected file difference aborts the ref update.
- Source review confirmed the redundant blank line in the Android JVM test; it was removed. No automated test was executed in this environment. The local clone attempt failed because DNS could not resolve `github.com`; .NET SDK, Gradle and Android SDK are unavailable.
- The combined commit status for the base SHA returned no status checks. No PR-triggered workflow run was returned. This is not treated as a test pass or failure.
- T0180 remains `in_progress`; both validated incoming edges remain `sequencing_only`, with original `type=blocks` and order preserved. The outgoing edge to T0181 remains `unreviewed`.

## T0180-A003 — مستقل‌سازی و اجرای آزمون decoder

- Base revision: b9092888dab7b52a7bbbc22056d8a6f8b4bec57c (V 3.0.823).
- Production source: src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt; fetched Blob SHA: f39a0695e6ce2b927b255e0f188dd1ed4b778098.
- Runtime: Kotlin/JVM compiler 1.9.0, JDK 21.0.12.1.
- Command: kotlinc PeerMessageFrameCodec.kt CodecAcceptanceHarness.kt -include-runtime -d codec-acceptance.jar && java -jar codec-acceptance.jar
- Observed output: PASS: 8/8 codec acceptance scenarios.
- The harness exercised the production decoder/codec source in isolation. It does not compile the Android app or prove Netty integration, Android SDK compatibility, or Windows behavior.
- Additional exact-source audit: 11/11 predicates for Windows frame/request/response cleanup, Android pending/temp/frame cleanup, and presence of the committed Python/JVM regressions were true.
- T0180-A003 closes the Android peer stream when cancellation, interruption or timeout leaves a Netty write promise pending; the existing completion callback then zeros the encoded frame. Exact-head Android integration/CI remains pending.

## T0180-A004 — CI evidence and corrections

- Base commit: fef1135d983ed2f62619fe8e1224301adb887615 (`V 3.0.824`).
- Step Three quality gate: run 41465625302 — success; 338 Python tests passed; compileall passed; graph guard 230 tasks / 422 edges / zero errors / one pre-existing T0001 unmapped-dependency metadata warning.
- Rendezvous container: run 41465625258 — production image build and non-root HTTP contract smoke passed.
- Product build: run 41465625430 — Windows WPF job passed, Windows HostLifecycle job passed, Android job failed at KtLint before APK/JVM/Protobuf steps.
- Failing job: 125345607621. KtLint exact diagnostic: `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt:521:5 Expected a blank line for this declaration`.
- Windows build warning: `Libp2pPeerMessageProtocol.cs(197,44): CS8600` from the nullable contract of ChannelReader.TryRead. The loop now declares `byte[]?` and applies a non-null/length pattern before zeroization.
- A004 corrects both findings and tightens the source regression. It does not mark task done; exact-head CI remains pending.

## T0180-A005/A006 — CI attempt accounting reconciliation

- Tested commit: `d85b2a362cd6e95db38f4b6ed5b7a4d2321377ac` (`V 3.0.825`).
- Step Three quality gate run `41467491212`: 338 Python tests passed and compileall passed; graph_guard failed on `FAILED_ATTEMPT_COUNT_MISMATCH: T0180: summary=1, history=0`.
- Product workflow run `41467491153`: final run attempt 3 concluded success. Android job `125354254445` passed KtLint, APK build, JVM tests, generated Protobuf verification and artifact uploads. Windows job `125354160493` passed formatting/build/smoke/security checks with no nullable warning. HostLifecycle job `125355346288` passed on attempt 3 after attempts 1 and 2 failed on early peer-stream closure.
- Rendezvous container run `41467491150` passed production build and non-root HTTP contract smoke.
- A005 adds the missing `result=failed` row for the failed graph-validation attempt; A006 is the metadata reconciliation attempt. The failed prior HostLifecycle retries remain explicitly preserved, along with the successful retry.
- New graph counters: 6 total attempts / 1 failed. T0180 remains in_progress until the corrected graph passes its own exact-head quality gate and the required product workflows pass.

## T0180-A007/A008 — V 3.0.826 product failure and fixture correction

- Step Three quality gate `41469076873`: success; 338 Python tests; compileall; graph guard 230 tasks/422 edges, zero errors and one T0001 metadata warning.
- Rendezvous container `41469076680`: success.
- Windows WPF job `125357569545`: success.
- Android job `125357569592`: KtLint and APK build passed; 196 JVM tests completed, 1 failure. JUnit artifact `13153164724` shows `sendsCancellationToThreeNativeDotNetLibp2pHostsAndReceivesCancelledResponses` failed because the client observed stream closure before a correlated CANCELLED response. All three .NET fixture peers logged `CANCEL_RECEIVED`, `VALIDATED`, `SENT`, `PASS`; the fixture process then returned and disposed its transport immediately after send.
- Windows HostLifecycle job `125357569172`: `PeerHostLifecyclePublishesAndServesTrustedPeerAsync` failed its first-response correlation assertion at line 293. Assertion previously did not identify mismatched fields.
- A008 changes the one-shot .NET fixture to remain alive until the Android client closes the stream after handling the response, and adds non-sensitive boolean diagnostics to the Windows assertion. No production behavior is changed in A008; exact-head CI remains mandatory.


## T0180-A008 — V 3.0.827 exact-head CI evidence

- Step Three quality gate run 41471350827 / job 125365446419: passed; 338 Python tests, compileall, graph guard for 230 tasks / 422 edges, zero errors and one preserved T0001 metadata warning.
- Rendezvous container run 41471350787 / job 125365442833: passed; production image build and non-root HTTP contract smoke passed.
- Android product job 125365442148: passed; KtLint, debug APK, JVM unit tests, generated Protobuf verification and APK upload succeeded.
- Windows WPF job 125365442688: passed; formatting, Release build, generated Protobuf, startup/security and DPAPI checks succeeded.
- HostLifecycle job 125365442905: failed at PeerHostLifecyclePublishesAndServesTrustedPeerAsync while waiting for HostStatus at line 266 after HostStatusRequest. ReceiveMessageAsync threw IOException with inner ChannelClosedException (libp2p channel aborted); the test did not reach the InferenceRequest exchange.
- Real local inference run 41471350738 / job 125365443683: the shared HostLifecycle acceptance suite failed at the same HostStatus receive before the optional real-model block. The llama.cpp/GGUF inference acceptance was not reached and is not classified as a model-test failure.
- The A008 Android fixture handshake correction is supported by the Android JVM pass on this exact head; Windows HostLifecycle acceptance remains failing.

## T0180-A009 — diagnostic change

- Added a safe, type-only diagnostic wrapper around the HostStatus receive in PeerAnnouncementClientAcceptanceTests. It surfaces host.IsActive, host.LastFailureType, client.IsConnected and the received exception type only.
- No secrets, identifiers, prompts, responses or exception messages are emitted by the added diagnostic.
- This commit has not yet been validated by exact-head CI; no task completion is claimed.

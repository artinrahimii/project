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


## T0180-A010 — type/stage-only and counter diagnostics

- Exact base: `V 3.0.828`, commit `0608663fa99031d43c637158f3c9976f257bd9c6`, branch `main`.
- The exact-base graph Blob was obtained from Git Blobs API and matched the Git Trees entry by SHA-1. Seven modified-file inputs were fetched as Base64 and each recomputed Git Blob SHA-1 matched both the contents response and the same commit's tree entry.
- Windows host diagnostics now expose a fixed receive-loop stage separately from exception type. The acceptance failure message includes only these values.
- Android transport maintains numeric counts for inbound chunks, decoded frames, HostStatus frames, InferenceResponse frames, malformed response frames, unmatched response frames, correlation rejections, accepted responses, peer-closed callbacks and callback failures. No application payload, peer ID, prompt, response text, exception message or key material is recorded.
- CI failures observed on V 3.0.828 remain: HostLifecycle reported InvalidDataException and Android JVM reported 1 failure among 196 tests in the three-peer cancellation round-trip. The new diagnostic counters/stage have not yet been exercised by exact-head CI.
- No task completion is claimed; T0180 remains in_progress pending root-cause correction and full exact-head acceptance.


## T0180-A011 — V 3.0.829 CI failure evidence

- Step Three quality gate run 41478433797 / job 125390039305: 338 Python tests ran; 1 failed. `test_android_does_not_correlate_a_previous_request_response_to_a_new_request` in `tests/test_android_inference_response_correlation_contract.py:49` asserted the previous source spelling and failed after the null-pending path was expanded to increment a diagnostic counter. The Python suite stopped before compileall and graph_guard.
- Windows HostLifecycle job 125390036031: test project compilation failed at `PeerAnnouncementClientAcceptanceTests.cs:279` with CS1039 (unterminated string literal) and CS1003. The new `LastFailureStage` was not exercised.
- Android product job 125390035607: failed while building the Windows .NET interop fixture because of the same C# test-source syntax error; Android lint/JVM acceptance did not run.
- Rendezvous container run 41478431814: passed production image build and non-root HTTP smoke.
- No inference-model success is inferred from an in-progress workflow; the next exact-head CI must verify all required jobs.


## T0180-A013 — V 3.0.830 exact-head evidence

- Step Three quality gate run 41479259509: passed, including Python tests and graph validation for 230 tasks / 422 edges.
- Rendezvous container run 41479259270 / job 125392908935: passed production build and non-root HTTP smoke.
- Windows WPF job 125392910104: passed.
- HostLifecycle job 125392910293: failed after successful compilation. Diagnostic: `hostActive=False`, `hostFailureType=InvalidDataException`, `hostFailureStage=reject_malformed_application_message`; the listener rejected the received application frame as neither HostStatusRequest, InferenceRequest nor CancelRequest. Parser validation categories, frame size and first tag were not yet exposed.
- Android job 125392910595: failed KtLint at `Libp2pInferenceRequestTransport.kt:115:5`; Android APK/JVM/Protobuf tests were not reached.
- Real local inference run 41479259306 / job 125392908304: timed out at `PeerAnnouncementClientAcceptanceTests.cs:340` in the shared HostLifecycle loopback suite, before live model generation was confirmed. Downloading and validating the pinned model/runtime is not proof of inference success.
- A014 adds payload-free parser categories, frame length and first tag, plus the KtLint blank line. No exact-head validation result for those changes exists yet.

## T0180-A014 — diagnostic source changes (CI pending)

- Windows `PeerAnnouncementHostLifecycle` records only parser validation enum/error-code names, received application-frame byte length and first Protobuf tag byte when the receive loop terminates on a malformed message.
- Windows HostLifecycle failure output includes those fields alongside active/failure stage/type and client connectivity. No Protobuf payload, peer identity or prompt/response content is logged.
- Android transport’s counter snapshot was retained; the missing blank line required by KtLint at line 115 was added before the protocol binding declaration.
- No local tests were executed. Exact-head Python quality, C# compilation/HostLifecycle, Android KtLint/APK/JVM/Protobuf, Windows WPF, Rendezvous and real local inference remain required. T0180 stays in_progress.


## T0180-A014 — V 3.0.831 exact-head results

- Step Three quality gate run 41481768427: passed.
- Rendezvous container build and non-root smoke run 41481768488: passed.
- Windows WPF job 125401644497: passed.
- HostLifecycle job 125401643696: failed. The listener rejected an empty application frame; diagnostics reported `hostFailureMessageBytes=0`, `hostFailureFirstTag=-1`, and `reject_malformed_application_message`.
- Android job 125401644795: failed Kotlin compilation with unresolved references to `inboundChunks`, `decodedFrames` and `callbackFailures` inside the top-level controller class.
- Real local inference job 125401638934: peer stream closed in the shared HostLifecycle loopback before the live model generation check. No actual model-generation success is claimed.
- These failures are recorded in the graph execution history as A014; T0180 remains in_progress.

## T0180-A015 — changes prepared for exact-head validation

- The three diagnostic AtomicLong values are passed explicitly from the owning Android transport to the top-level `InferencePeerMessageController`.
- A Python source-contract regression verifies the constructor and binding ownership contract.
- The Windows HostStatus loopback test records only `statusRequestBytes` and `statusRequestFirstTag`, allowing them to be compared against the receiver's existing zero-byte/first-tag diagnostics. The serialized request array is zeroed in a finally block after the send completes.
- No local .NET, Android Gradle or Python tests ran in this session. Exact-head CI is required; T0180 remains incomplete.


## T0180-A015 — verified V 3.0.832 CI

- Android format/lint, Kotlin compile and debug APK build passed; the Android JVM suite finished 196 tests with one cross-runtime cancellation failure (job 125817204860).
- JUnit artifact 13216862899 showed all three .NET fixtures received, validated and sent their cancellation responses. Android saw 3 inbound chunks and 261 decoded response frames, of which 259 were unmatched and 2 accepted.
- Windows WPF checks passed (job 125817205085).
- Windows HostLifecycle failed: serialized sender request was 54 bytes with first tag 10; listener reported an empty frame (0 bytes, first tag -1) (job 125817205025).
- The real local inference workflow failed before model generation because the same HostStatus loopback did not pass (job 125817205365).
- The Step Three quality gate (run 41606404540) and Rendezvous container smoke (run 41606404559) passed.
- No claim of successful live inference or complete T0180 acceptance.

## T0180-A016 — additional metadata for the next diagnostic run

- New Android counters expose only frame byte lengths, numeric first Protobuf tags and empty-frame count.
- The Windows .NET fixture emits a payload-free response length/tag marker and clears temporary serialized response bytes after send.
- The Android test retains this marker in the failure diagnostics.
- The purpose is to determine whether frame expansion/alignment occurs before, during or after the Windows application protocol writer. The root cause remains unknown pending exact-head CI.


## T0180-A016 — V 3.0.833 exact-head CI

- Step Three quality gate run 41610464154: passed.
- Rendezvous container build/smoke run 41610464102: passed.
- Windows WPF build/startup and process/policy/DPAPI checks job 125830768358: passed.
- HostLifecycle job 125830768736: failed with sender HostStatusRequest length 54 / first tag 10 and receiver frame length 0 / first tag -1.
- Real local inference job 125830768050: pinned llama.cpp b11443 and GGUF validated; runtime status check passed; QUIC fallback binary-response assertion failed at PeerTransportContractAcceptanceTests.cs:372. No real generation success.
- Android formatting/lint and debug APK build passed; Android JVM test suite was still in progress at the time of this evidence update.
- The task remains in_progress. No acceptance criterion is declared complete from these failures.


## T0180-A016 — Android CI completion on V 3.0.833

- Android job 125830768838 passed: formatting/lint, Kotlin compile, debug APK and Protobuf generation.
- Android JVM class `Libp2pInferenceRequestTransportTest`: 7 tests, 0 failures (100%).
- The three-native-.NET-host cancellation round-trip test passed.
- JUnit artifact: 13218328799.
- This is an exact-run success and does not establish that the earlier timeout's underlying cause is permanently resolved.
- Windows HostLifecycle empty-frame failure and real-inference QUIC fallback failure remain open; no successful live model generation is claimed.


## T0180-A016 — V 3.0.833 exact-head CI

- Step Three quality gate run 41610464154 passed.
- Rendezvous container build/smoke run 41610464102 passed.
- Windows WPF job 125830768358 passed.
- Android job 125830768838 passed: 7/7 transport JVM tests, formatting/lint, Kotlin compile, debug APK and Protobuf passed.
- HostLifecycle job 125830768736 failed: 54-byte sender HostStatusRequest (tag 10) was received as a zero-byte application message (tag -1); VarInt.Decode then reported an aborted channel.
- Real local inference job 125830768050 verified llama.cpp b11443/GGUF and runtime status, but failed QUIC fallback binary response delivery at PeerTransportContractAcceptanceTests.cs:372; no model response generation is evidenced.

## T0180-A017 — bounded outbound frame retention

- Source evidence: `IWriter.WriteAsync` acknowledges when the immediate channel reader consumes a chunk. Yamux forwards the returned `ReadOnlySequence` asynchronously to the application stream. The previous send implementation zeroized its backing frame immediately after the lower-level acknowledgement, before remote application consumption was assured.
- Successful Windows outbound frames are now retained in a bounded per-stream collection until the next ordered peer message or stream teardown/disposal. Failed writes are removed and zeroized immediately. Retained frames are zeroized on acknowledgement or closure.
- Retention is bounded by configured inbound queue capacity and an aggregate budget of two maximum-sized frames.
- Added a Python source-contract regression. No A017 build/test result is claimed pending exact-head CI.


## T0180-A018 — exact-head V 3.0.836 completion evidence

- Exact head: `main`, commit `be44ff01649a6e36f631e8a286f1bfc9b4fb8820` (`V 3.0.836`).
- Step Three quality run `41612366597`: 341 Python tests passed; syntax and output/evidence checks passed; graph guard reported 230 tasks / 422 edges / 0 errors / 1 existing metadata warning.
- Relevant Python regressions passed: `test_successful_sends_retain_bounded_frame_buffers`, `test_response_and_stream_teardown_release_retained_frames`, and all four `SensitiveBufferCleanupTests`.
- Android JUnit artifact `13219812228`: 196 tests, 0 failures. `PeerMessageFrameCodecTest`: 10 tests / 0 failures, including `zeroesThePreviousPendingBufferWhenAFrameCompletes`, `zeroesPartialFrameWhenStreamFinishesAsTruncatedAndAllowsReuse`, and `explicitClearZeroesAndDiscardsPartialFrame`. `Libp2pInferenceRequestTransportTest`: 7 tests / 0 failures, including three-native-.NET-host cancellation.
- Product run `41612366697`: Windows WPF job `125837178133`, Android job `125837178286`, and HostLifecycle job `125837178370` all passed. The HostLifecycle suite includes authenticated HostStatus, request/replay/cancellation, TCP and QUIC transport checks.
- Rendezvous run `41612366582`: production container build and non-root HTTP contract smoke passed.
- Windows successful outbound frames remain only in a bounded per-stream buffer until an ordered peer message acknowledges consumption or stream teardown/disposal; unsuccessful sends remove and zero the frame immediately. Android temporary/pending/decoded/request buffers are zeroized in their success/error/cancellation/stream-close paths.
- No Protobuf schema or peer-trust contract changed. No local SDK test run was performed in this session; the exact-head CI and JUnit artifact are the execution evidence.
- T0180 status is now `done`; this does not imply completion of the overall project.

# T0168 — response collection verification

- Production entry point: `AndroidInferenceRequestRuntime.awaitRoundResponses`.
- All queued attempts use the stored shared `InferenceRoundTimer.Window`.
- Collector concurrently awaits all attempts and returns per-host response/failure outcomes; first success does not end the collection.
- Existing authenticated PeerId and request/attempt/round/model/deadline correlation validation remains intact.
- Five JVM regression tests cover first response continuation, isolated timeout, unqueued submissions, expired window and caller cancellation.
- V 3.0.702–V 3.0.705 exact CI records are retained in the task execution history; the failures were KtLint layout and brittle source-contract assertions. Windows WPF/HostLifecycle passed in V 3.0.705.
- V 3.0.706 A005 changes only chain indentation and the related Python assertion. Exact-head CI pending.
- Local Gradle execution unavailable. Keep T0168 in_progress until all exact-head gates pass.


## Exact-head V 3.0.706 verification

- Step Three quality: passed (308 Python tests, syntax, graph and evidence checks).
- Windows WPF and HostLifecycle: passed.
- Android interop fixture: passed.
- Android KtLint: failed on trailing spaces at collector lines 84/89/94 and nested `attempt.failure` line wrapping at lines 99/113/117. APK/JVM/Protobuf were skipped.
- A006 removes whitespace-only suffixes and wraps the flagged nested calls. V 3.0.707 exact-head CI pending.

# T0168 — response collection verification

- Runtime integration: `AndroidInferenceRequestRuntime.awaitRoundResponses`.
- All queued attempts share the round window registered at dispatch; the remaining window is passed to the authenticated transport awaiter.
- Collector awaits all outcomes and isolates failures/timeouts per host; first success never cancels other waiters.
- Transport peer authentication and request/attempt/round/model/deadline correlation remain unchanged.
- Five JVM tests cover first-response continuation, timeout isolation, non-queued outcomes, expired deadline and caller cancellation.
- V 3.0.702: Python quality, Windows WPF and HostLifecycle passed; KtLint failed.
- V 3.0.703: 307/308 Python passed; a source assertion was too strict; KtLint still failed; Windows and HostLifecycle passed.
- V 3.0.704: all 308 Python tests passed; KtLint still failed on single-argument data class constructors and map/awaitAll chain layout; HostLifecycle passed.
- A004 applies the exact Kotlin formatting corrections reported by CI. V 3.0.705 exact-head validation pending.
- Local Gradle execution unavailable. T0168 remains in_progress until all exact-head gates pass.

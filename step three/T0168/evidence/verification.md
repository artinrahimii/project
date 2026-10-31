# T0168 — round response collection verification

- Runtime entry point: `AndroidInferenceRequestRuntime.awaitRoundResponses`.
- All queued attempts share the dispatch-created `InferenceRoundTimer.Window`; each uses its remaining duration.
- Collector waits for every queued attempt, preserves later valid responses after the first success, and records timeout/failure per attempt.
- Existing authenticated-peer and request/attempt/round/model/deadline validation remains in the transport path.
- JVM coverage includes late second response, a sibling surviving an independent timeout, non-queued submission outcomes, expired shared window and caller cancellation.
- V 3.0.702 Android KtLint failed on three style rules; V 3.0.703 corrected those style rules but its Python quality suite exposed one brittle assertion requiring same-line `}.awaitAll()`. 307/308 Python tests passed.
- V 3.0.703 HostLifecycle passed; product Android/Windows work was still running at the recorded observation.
- V 3.0.704 updates the assertion only; exact-head gates pending.
- Task remains in_progress until all product and quality gates pass.

# T0168 — response collection evidence

- Runtime API: `AndroidInferenceRequestRuntime.awaitRoundResponses`.
- Collector awaits all dispatched attempts concurrently against the shared per-round timer window.
- First valid response does not cancel other host waiters. Per-host failure/timeout is isolated; non-queued submissions do not start a response wait; caller cancellation propagates.
- Existing authenticated PeerId and request/attempt/round/model/deadline response validation is retained.
- Five JVM tests cover late additional responses, isolated host timeout, non-queued outcomes, expired window and caller cancellation.
- V 3.0.708: 308 Python tests and syntax passed; graph guard failed on attempt counter/history mismatch (6/5 vs 7/6); Windows WPF/HostLifecycle passed; KtLint reported two indentation lines only.
- V 3.0.709 A008 adjusts the last two indentation levels and sets summary counters to 8 attempts / 7 failed, matching its eight execution-history records.
- Exact-head CI pending; local Gradle unavailable. T0168 stays in_progress pending all gates.

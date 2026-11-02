# T0177 — per-round host and per-request attempt budget acceptance

- **Status:** `in_progress`; implementation is committed, exact-head CI is pending.
- **Base revision:** `main`, `75651286a49d7dfbb598e749c7f5f9967ed4d0b8`, message `V 3.0.809`.
- **Attempt:** `T0177-A001` — partial until the exact commit passes the required workflows.
- **Implementation:** added two Android JVM integration tests to `InferenceAttemptDispatcherTest.kt`. The first drives the real selector and dispatcher through three rounds with twelve available peers, asserting no more than three unique peers per round, no peer reuse across ordinary rounds, and the nine-attempt request ceiling. The second uses a four-attempt custom budget and explicitly retries one previously used peer, proving that the retry consumes the remaining budget and blocks a further send.
- **Cross-check:** added `tests/test_android_inference_attempt_budget_acceptance.py` to pin the production ceilings, selector/dispatcher guards, and JVM acceptance scenarios.
- **Production code:** unchanged. Existing production policy centralizes the first-release limits at three rounds, three hosts per round, and nine total host attempts; this task strengthens executable acceptance coverage rather than changing runtime behavior.
- **Dependency review:** `T0160 → T0177` is `contract_or_interface_dependency` because the tested dispatcher creates each protocol payload through T0160's actual payload builder. `T0176 → T0177` is `sequencing_only`: the task's response collector/ViewModel tests are not consumed by the host-budget algorithm. Both original `blocks` edge types and the conservative execution order remain unchanged. Rationale and code/test evidence are recorded in the graph edge log.
- **Tests:** no CI result is claimed in this commit. The Android KtLint/build/JVM suite and Python graph-quality suite must pass on the exact new `main` commit before status can move to `done`.
- **Next action:** inspect exact-head GitHub Actions for `V 3.0.810`; fix any real failures and retain the task as incomplete until all required checks pass.

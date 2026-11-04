# T0194 execution report

- **Task:** هنگام بسته‌شدن اپ اندروید، وضعیت درخواست‌های جاری را بازیابی‌پذیر یا صریحاً منقضی کن.
- **Base:** `main` / `21e31e097a8818c2806f9e8611dc280897ae70e6` (`V 3.0.908`).
- **Attempt:** `T0194-A001`.
- **Status:** `in_progress`; implementation committed for exact-head CI validation. No acceptance success is claimed until Quality, Android lint/build/JVM tests and relevant Room migration/restart tests pass.
- **Dependency review:** both incoming edges were validated as `sequencing_only` while preserving their original `type=blocks` and conservative execution order. T0193 supplies no Windows runtime interface consumed by Android T0194; T0188 supplies privacy/threat-model guardrails rather than an executable artifact. The outgoing edge to T0195 remains `unreviewed`.
- **Finding:** the previous `RequestViewModel` retained request lifecycle state only in memory. The existing `request_states` Room table is conversation-bound and the request screen is independent; its UI displayed persisted conversation rows but did not persist live inference lifecycle transitions.
- **Implementation:** added an independent Room metadata table `client_inference_request_states`, an encrypted Room repository, schema migration 4→5, startup recovery, and lifecycle writes before dispatch / while awaiting / after terminal outcomes. Stale `QUEUED`, `CONTACTING`, and `WAITING` states become `EXPIRED` with fixed error code `APP_CLOSED_BEFORE_RESULT`.
- **Privacy boundary:** only opaque request ID, state enum, timestamps, attempt number and fixed error code are stored. Prompt, context capsule, peer identifiers, payload and response bodies remain in memory and are not part of this table.
- **Remote-work boundary:** `EXPIRED` means the Android client did not record the result before its process/request owner ended. It does not assert that a remote host was stopped or did not finish.
- **Retention:** only the 20 most recent app-level lifecycle rows are retained.
- **Tests added/updated:** Room file-reopen recovery, migration v1→v5, ViewModel startup recovery, expired-status presentation, SQLCipher restart recovery and Python source/privacy contracts. Existing capsule persistence tests were updated to register migration 4→5.
- **Verification boundary:** source/contract structure was inspected, but no local Android SDK/build is available in this execution environment. Exact-head GitHub Actions results are pending; no test success is yet claimed for A001.
- **Next:** inspect exact-head Step Three quality and Android lint/build/JVM/instrumentation results. Fix failures and only then accept T0194.

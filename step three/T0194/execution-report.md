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


## T0194-A001 CI finding and T0194-A002 correction

- Exact-head Quality run [41875864372](https://github.com/artinrahimii/project/actions/runs/41875864372), job `126734953437`, ran **403 tests** and failed one source-contract test: `test_android_request_architecture.test_compose_screen_uses_viewmodel_and_production_peer_gateway` expected the old `viewModel()` no-argument syntax. The production code now correctly injects `RequestViewModelFactory` and `RoomInferenceRequestLifecycleRepository`; this test had not been updated for that intended wiring. The other 402 tests passed.
- Corrected `tests/test_android_request_architecture.py` to assert the Compose `viewModel(` factory call, `RequestViewModelFactory`, and `RoomInferenceRequestLifecycleRepository`.
- This correction is attempt `T0194-A002`. No Android build or runtime success is claimed yet. The product workflow was still running Windows, Android and host lifecycle jobs at inspection. Rerun exact-head CI is required.


## T0194-A002 graph validation correction

- Exact-head Quality run [41876248248](https://github.com/artinrahimii/project/actions/runs/41876248248), job `126736315881`: **403 Python tests passed** (`OK`). The graph guard then reported one error: `FAILED_ATTEMPT_COUNT_MISMATCH: T0194: summary=1, history=0`.
- Cause: A001's CI gate had failed the stale architecture assertion, and the summary incremented `failed_attempt_count`, but its history item still said `partial`.
- Correction: A001 history is now marked `failed`, matching the recorded counter. A002 remains `partial`; this corrects the execution ledger and does not assert Android product acceptance.
- A fresh Quality run is required before T0194 can proceed to acceptance. Product workflow results remain separately pending.

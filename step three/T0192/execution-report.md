# T0192 — Bound internal retries and keep them separate from distributed request retry

- **Status:** `in_progress` pending exact-head CI for the new cross-layer regression guard.
- **Baseline:** `main`, `d6b91bdbaef56806447505be7a66e12765677e1f` (`V 3.0.895`); graph Blob `476afdeca2cc22649c734fa5c76992212a877406`.
- **Decision:** no second automatic inference-retry loop was added. Source inspection found that the required retry boundaries already exist in production code. Adding an inner retry scheduler would multiply attempts and conflict with the Android request-level budget.
- **Android request budget:** `FirstReleaseInferenceLimits` enforces 3 rounds, 3 hosts per round and 9 total host attempts. `DistributedRequestProgress` carries the cumulative round/host count and attempted peer IDs.
- **Single-round dispatch:** `InferenceAttemptDispatcher.dispatchRound` makes one transport submission for each selected host. A round-window timeout becomes `TIMED_OUT`; a failed/rejected submission is recorded and does not replay that host inside the same dispatch. Existing JVM coverage includes `keepsSubmittingOtherHostsAfterTransportRejectsOrThrows`, the three-round/nine-host boundary and the custom four-attempt retry boundary.
- **Distributed request retry:** `RequestViewModel.retryFailedAttempts` is reached only through the explicit Retry action in the Android screen. It preserves the logical request ID and cumulative attempt budget, uses fresh attempt IDs, excludes hosts whose latest attempt succeeded or was cancelled, and disables retry for terminal cancellation or an exhausted budget.
- **Host Busy response:** Windows advertises a fixed one-second `HostBusy.retry_after_unix_ms` hint. Android parses the typed Busy envelope into `HOST_BUSY`; it does not automatically schedule another inference request from that hint.
- **Response delivery:** `HostRequestResponseDeliveryGate` atomically claims one response-path handoff. It deliberately retains the claim after an ambiguous callback failure, because replaying an uncertain send could duplicate the response. Existing HostLifecycle tests cover sequential duplicates, 64-way concurrent contention and ambiguous failure.
- **Connection fallback is a separate layer:** peer announcements cap direct addresses at 8 and reject duplicates. `PeerTransportFallbackConnector` tries distinct addresses for the same pinned peer using secure connection establishment; it does not replay an inference request or response payload.
- **New regression guard:** `tests/test_request_retry_budget_boundary.py` ties these production layers together with five Python contract tests. It protects the shared 3/3/9 budget, one-submit-per-host dispatch (including the bounded round timeout), manual-only distributed retry, Busy hint handling, at-most-once response handoff and the separate eight-address connection fallback.
- **Dependency review:**
  - `T0191 → T0192`: validated as `contract_or_interface_dependency`. Android serializes the round's absolute deadline into `InferenceRequest`; the host admission/coordinator consumes it and T0191 now actively cancels in-flight work at expiry. This is a real request-deadline contract used by the distributed retry flow. Original `type=blocks` retained.
  - `T0188 → T0192`: validated as `sequencing_only`. The threat model supplies normative privacy constraints but no executable retry API/schema consumed by this task. Original `type=blocks` and conservative order retained.
- **No retry-policy expansion:** no new runtime loop, extra retry count, altered wire format, or automatic retry trigger was introduced.
- **Local test limitation:** no local project checkout is available in this execution environment. The new Python guard is pending the exact-head GitHub Actions run; no local pass is claimed.
- **Next action:** inspect the exact-head Step Three quality gate and product jobs. Keep this node `in_progress` unless the new test and relevant Android/Windows acceptance suites pass.


## T0192-A002 — first exact-head CI observation
- Commit `fc53011424092440573a94886a5d404c87c7d064` (`V 3.0.896`) ran the complete Python suite: **396 passed**, including all five tests in `test_request_retry_budget_boundary.py`; `compileall` passed.
- The exact-head graph guard then failed with one metadata error, `DONE_WITHOUT_TEST_FILE_EVIDENCE: T0191`, because its existing HostLifecycle acceptance source was absent from T0191's graph `evidence` array. The source path existed in `actual_output_paths`, and the test itself was green. The unrelated T0001 unmapped dependency remained a warning.
- The Rendezvous workflow on this exact commit passed. Android and Windows product workflows were still running at the time of this inspection; they are not claimed here.
- This is recorded as **partial**, not success or an implementation failure. The evidence list is being corrected and all exact-head jobs will be checked again.

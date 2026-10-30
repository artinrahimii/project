# T0163 execution report

- **Task:** Send one correlated inference attempt to each selected host, using an ephemeral request ID and a distinct attempt ID.
- **Base revision:** `main` commit `75f996b801a6b80b8634c3ca002a18c96753cab5` (`V 3.0.615`).
- **Graph source validation:** The graph was fetched from the exact-tree Blob `c98503844da13fd59cbd18905ecfdcef66656960`; the previous revision's complete graph was 2,984,104 UTF-8 bytes and its Git Blob SHA-1 was verified. This revision's complete fenced JSON parsed as 229 nodes and 417 edges.
- **Predecessors:** T0160 and T0162 are `done`. T0160's payload builder creates the canonical Protobuf `InferenceRequest` with `request_id` and `attempt_id`; T0162's selector returns at most three compatible `HostCandidate` values with an ephemeral host ID.
- **Dependency review:** T0160 → T0163 and T0162 → T0163 are validated `contract_or_interface_dependency` edges, based on the producer/consumer APIs. Both original `type: blocks` values and the execution order are preserved. No unrelated or outgoing unreviewed edge was changed.
- **Implementation:** `InferenceAttemptDispatcher` accepts only an explicitly injected `InferenceRequestTransport`, validates a non-terminal request and at most three distinct/consistent selected host identities, generates a distinct nonblank attempt ID per host, constructs each canonical payload with the shared request ID and per-host attempt ID, validates all payloads before the first transport call, and submits each selected host independently. Rejections and ordinary transport exceptions are recorded per host; coroutine cancellation is rethrown.
- **Tests:** Seven Android JVM tests cover correlation, duplicate hosts, the three-host cap, invalid payload rejection before submission, terminal requests, transport rejection/exception behavior, and duplicate attempt IDs. One repository-level Python source-contract test covers the dispatcher API/boundary.
- **A001 — V 3.0.614:** Step Three quality gate passed (41043613877). HostLifecycle passed. Windows formatting, WPF build, Protobuf, startup, policy, capability, DPAPI and artifact upload all passed. Android KtLint failed; APK/JVM/Protobuf stages were skipped.
- **A002 — V 3.0.615:** Step Three quality gate passed (41043948552) and HostLifecycle passed in product workflow 41043948733. Android KtLint still failed on multiline expression layout and indentation; APK/JVM/Protobuf stages were skipped. Exact findings are retained in `evidence/v3.0.615-ci-failure.md`.
- **A003 — V 3.0.616:** Applies only the remaining KtLint layout/indentation corrections. Exact-head CI is pending; no test result is claimed yet.
- **Local execution limitation:** Android/Gradle tasks were not run locally in this connected GitHub tool session; exact-head GitHub Actions supplies the build/test evidence.
- **Integration boundary:** No concrete Android-to-Windows transport adapter or routable peer-address resolution exists at this revision. The dispatcher invokes a required transport port and does not provide a fake/default transport. Remote delivery, authenticated peer connection, inference execution, and end-to-end Android-to-Windows request/response are therefore not proven.
- **Task status:** `in_progress`, not `done`. Implement and wire authenticated host address resolution and a concrete Android-to-host transport, then verify remote delivery against a real host and exact-head CI.


## A003 exact-head CI failure and A004 correction

- **Verified revision:** `V 3.0.616`, commit `edfd75ce4bdb36a02b37b6fc1bab9e6ba3d7b7bd` on `main`.
- **Step Three quality gate:** PASS — workflow 41044231304.
- **HostLifecycle unit/integration:** PASS — job 124024230799.
- **Android:** KtLint failed only in test-source formatting (job 124024230961). All seven `runBlocking` bodies were under-indented after conversion to multiline expression bodies; KtLint also required different indentation for the body of `distributedRequest` and the dispatcher helper expression on the signature line. The production dispatcher passed KtLint. APK/JVM/Protobuf steps were skipped.
- **A004 correction:** Applied the exact indentation and expression-body formatting requested by the log. No test logic or production behavior changed. Exact-head `V 3.0.617` CI is pending.
- **Task status:** `in_progress`; no Android acceptance is claimed until KtLint, APK build, JVM tests and Protobuf all pass; real transport wiring is still absent.

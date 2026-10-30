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


## A004 exact-head CI outcome and A005 correction

- **Verified revision:** `V 3.0.617`, commit `e4ada9fa25f3e25ddbc437f0ceeb70596a02907a` on `main`.
- **Step Three quality:** FAILED — workflow 41044526810. All 271 Python tests and compileall passed; graph validation reported `FAILED_ATTEMPT_COUNT_MISMATCH: T0163: summary=3, history=2`. The immutable V 3.0.617 graph Blob was independently reread and shows four total attempts, with two history entries marked `failed`; the summary count of three therefore exposed that A001's recorded result had remained `partial` even though its Android KtLint job failed. A005 corrects A001's final attempt result to `failed`, preserving the event history and aligning the summary with all failed attempts.
- **HostLifecycle:** PASS — product workflow 41044526799, job 124025244547.
- **Android:** FAILED at `ktlintTestSourceSetCheck`, job 124025244202. The exact log identified eight trailing-whitespace lines in the new test source (lines 49, 60, 99, 128, 152, 176, 211 and 243). All indentation and expression formatting findings from A003 were resolved. Android APK, JVM tests and Protobuf checks were skipped.
- **A005 correction:** Remove trailing spaces from the Android JVM test source; mark A001's final CI result as failed so `failed_attempt_count` equals the number of failed history records. No behavior changes. Exact-head `V 3.0.618` CI is pending.

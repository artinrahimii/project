# T0163 execution report

- **Task:** Send one correlated inference attempt to each selected host with a shared ephemeral request ID and a distinct attempt ID per host.
- **Base revision:** `main` commit `e94470cf6b8e75e2d5bca7362d9ecd909cde0aaa` (`V 3.0.620`).
- **Graph:** 229 task nodes and 417 edges. T0160 → T0163 and T0162 → T0163 remain validated `contract_or_interface_dependency`; original `type: blocks` values and execution order are unchanged.
- **Implementation:** `InferenceAttemptDispatcher` validates request state, at most three unique/consistent host identities, nonblank unique attempt IDs and all payloads before any transport call. Each host payload shares the distributed request ID but has its own attempt ID. Dispatch uses an explicit injected `InferenceRequestTransport`; no default/mock transport exists.
- **Tests:** Seven logical requirements are represented in the Android JVM suite. The invalid-payload preflight case was originally one test with multiple assertions, then split into four independent test methods so exact CI results will identify which invariant is failing.
- **A001 — V 3.0.614:** Step Three quality, HostLifecycle, Windows WPF/security/DPAPI/artifact passed; Android KtLint failed.
- **A002 — V 3.0.615:** Step Three quality, HostLifecycle and Windows passed; Android KtLint failed.
- **A003 — V 3.0.616:** Step Three quality (271 Python tests), HostLifecycle and Windows passed; Android main-source lint passed but test-source lint failed.
- **A004 — V 3.0.617:** Python tests/compileall passed but graph validation failed on attempt count; Android test-source lint failed on trailing spaces; HostLifecycle and Windows passed.
- **A005 — V 3.0.618:** Step Three quality, KtLint, APK build, Windows and HostLifecycle passed. JVM compilation failed on missing `InferenceRequestPayloadRejection` import.
- **A006 — V 3.0.619:** Step Three quality, KtLint, APK build, Windows and HostLifecycle passed. JVM suite executed 81 tests; 80 passed and one invalid-payload assertion failed.
- **A007 — V 3.0.620:** Step Three quality, KtLint, APK, Windows and HostLifecycle passed. JVM suite again executed 81 tests; 80 passed and the same test method failed, but concise CI logs did not identify which of its assertions failed. See `evidence/v3.0.620-ci-failure.md`.
- **A008 — V 3.0.621:** Splits blank-prompt dispatch validation into four separate JVM tests, sharing the same invalid-prompt dispatch helper. Exact-head CI pending.
- **Local execution limitation:** Android/Gradle was not run locally; GitHub Actions is the evidence source.
- **Integration boundary:** No concrete Android-to-Windows transport adapter or routable authenticated peer-address resolver has been integrated. Remote receipt, peer authentication, inference execution and end-to-end request/response remain unproven.
- **Task status:** `in_progress`, not `done`. Concrete authenticated transport/address resolution and real-host delivery acceptance are still required.

# T0163 execution report

- **Task:** Send one correlated inference attempt to each selected host with a shared ephemeral request ID and a distinct attempt ID per host.
- **Base revision:** `main` commit `5c1a3997810040dcb22b00ac80080d33269122fc` (`V 3.0.619`).
- **Graph:** 229 task nodes and 417 edges. Incoming edges T0160 → T0163 and T0162 → T0163 are validated `contract_or_interface_dependency`; original `type: blocks` values and execution order remain unchanged.
- **Implementation:** `InferenceAttemptDispatcher` validates request state, at most three unique/consistent host identities, nonblank unique per-host attempt IDs and every payload before making any transport call. Each host payload shares the distributed request ID but uses its own attempt ID. Submission uses only an explicitly injected `InferenceRequestTransport`, without a fake/default transport.
- **Tests:** Seven Android JVM tests cover correlation, duplicate hosts, host limit, invalid payload preflight, terminal request rejection, transport rejection/exception handling, and duplicate attempt IDs. A Python source-contract test checks the dispatcher boundary.
- **A001 — V 3.0.614:** Step Three quality, HostLifecycle and Windows WPF/security/DPAPI/artifact passed; Android KtLint failed.
- **A002 — V 3.0.615:** Step Three quality, HostLifecycle and Windows WPF/security/DPAPI/artifact passed; Android KtLint failed.
- **A003 — V 3.0.616:** Step Three quality (271 Python tests), HostLifecycle and Windows WPF/security/artifact passed; Android main-source KtLint passed but test-source KtLint failed.
- **A004 — V 3.0.617:** Python 271 tests/compileall passed, but graph validation failed on attempt count; Android test-source KtLint failed on trailing spaces; HostLifecycle and Windows passed.
- **A005 — V 3.0.618:** Step Three quality, Android KtLint and APK build, Windows WPF/security/DPAPI/artifact, and HostLifecycle passed. Android JVM compilation failed because the test omitted the `InferenceRequestPayloadRejection` import; JVM tests did not run.
- **A006 — V 3.0.619:** Step Three quality, KtLint, debug APK build, Windows WPF/security/DPAPI/artifact and HostLifecycle passed. Android JVM tests executed 81 cases; 80 passed and `rejectsInvalidPayloadBeforeSubmittingAnyHost` failed with an AssertionError reported at line 141. The CI log did not expose the individual assertion's expected/actual values. Protobuf verification and APK artifact upload were skipped. Details are in `evidence/v3.0.619-ci-failure.md`.
- **A007 — V 3.0.620:** Splits the invalid-payload test into separate assertions for result type, rejection category, payload rejection and zero transport calls, each with diagnostic messages. Exact-head CI is pending.
- **Local execution limitation:** Android/Gradle was not run locally in this connected GitHub tool session; CI is the evidence source.
- **Integration boundary:** No concrete Android-to-Windows transport adapter or routable authenticated peer-address resolver has been integrated. The dispatcher invokes the transport port only; remote receipt, peer authentication, inference execution and end-to-end delivery are not proven.
- **Task status:** `in_progress`, not `done`. Concrete authenticated transport/address-resolution and real-host delivery acceptance remain required.

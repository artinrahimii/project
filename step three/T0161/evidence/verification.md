# T0161 verification evidence

## Baseline and graph integrity

- **Branch/commit:** `main` at `V 3.0.599`, `8e6f4f1ccd37283526eecccd29059eb63c5e54e2`.
- **Graph path:** `step three/project-graph.md`.
- **Graph source:** Git Blob API using the Blob SHA obtained from the exact `V 3.0.599` commit tree: `180f1f8bbd54e635df48ff248d07482df47b1bdd`.
- **Graph completeness:** 2,909,937 UTF-8 bytes (matching the tree entry), 2,787,359 JavaScript string code units, complete JSON parse after removing the actual Markdown fence, and Git SHA-1 Blob recomputation matched the tree Blob ID.
- **Graph structure:** 229 task nodes and 417 edges; no truncation was reported by the recursive tree response.
- **T0160 prerequisite:** Status `done`; T0160 implementation/test paths exist and the V 3.0.599 product workflow and quality gate both passed.

## Contract review

- `InferenceRequestPayloadBuilder.build(...)` accepts `requestId` and `deadlineUnixMs`.
- `protocol/whitep/v1/messages.proto` defines the corresponding `request_id` and `deadline_unix_ms` fields.
- T0161 models the same request-scoped ID and deadline, together with maxRounds and lifecycle status.
- The edge is therefore validated as `contract_or_interface_dependency`; it is not classified as a hard compile-time dependency.
- T0161 → T0162 remains unreviewed until that next task is selected.

## New implementation

- Production: `src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt`.
- Tests: `src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt`.
- JVM tests use JUnit only; the model has no Android framework dependency.
- CI evidence for this implementation is pending on the implementation commit. No test is marked passed before execution.


## A001 CI failure and A002 correction

- Exact-head `V 3.0.600` Android KtLint failed only on import order in `DistributedRequestTest.kt:3:1`: the `java.util.UUID` import was before `org.junit`. No compiler or JVM result was obtained because subsequent Android workflow steps were skipped.
- The production `DistributedRequest.kt` passed the KtLint source check in that run.
- A002 changed only import order in the unit test; the exact-head V 3.0.601 result is recorded below.


## A003 — exact-head V 3.0.601 verification

- **Commit:** `9f6d1920165d2a1c1d897626886f70f3cd39e2f4` on `main`.
- **Android formatting:** `gradle --no-daemon ktlintCheck` passed.
- **APK build:** `gradle --no-daemon assembleDebug` passed.
- **JVM tests:** `gradle --no-daemon testDebugUnitTest` passed. This includes all six tests in `DistributedRequestTest`: preservation of request ID/deadline/maxRounds/status, unique UUID IDs and QUEUED default, and rejection of blank ID, nonpositive deadline and nonpositive maxRounds, plus the declared lifecycle status set.
- **Generated Protobuf:** Java and Kotlin generated source checks passed.
- **Android artifact:** `whitep-android-debug-apk.zip`, 11,968,325 bytes, artifact 12961600224.
- **Windows WPF/security:** formatting, build, generated Protobuf, startup smoke, process/policy/capability checks, DPAPI host identity and peer-trust checks passed.
- **Windows artifact:** `whitep-windows-host.zip`, 14,255,001 bytes, artifact 12960594708.
- **HostLifecycle:** Unit/integration checks passed.
- **Quality:** 260 Python tests passed; compileall and graph/evidence guards passed with 0 errors and 1 preserved T0001 metadata warning.
- **CI:** [Product build 41019550415](https://github.com/artinrahimii/project/actions/runs/41019550415) · [Quality gate 41019550381](https://github.com/artinrahimii/project/actions/runs/41019550381).
- **Result:** T0161 acceptance criteria are satisfied. The model carries a transient request-scoped ID, absolute Unix-millisecond deadline, positive maxRounds and lifecycle status. No live transport behavior is claimed.


## A004 — graph guard finding and additional contract test

- V 3.0.602 quality: 260 Python tests and compileall passed, but graph validation failed with `DONE_WITHOUT_TEST_FILE_EVIDENCE`. The guard requires a path starting with `tests/` or `test/`; the actual Kotlin JVM tests live under `src/Client.Android/app/src/test/`.
- Added `tests/test_android_distributed_request_contract.py` with four Python source-contract checks for the production model fields/validation, UUID factory, lifecycle enum and all six real JVM test methods.
- The Android JVM tests remain the execution proof. The Python file is supplementary graph-guard contract evidence only.
- T0161 is reopened as `in_progress` until exact-head V 3.0.603 CI confirms the evidence requirement and the product checks remain successful.


## A005 — supplemental source-contract test failure and correction

- V 3.0.603 quality automated tests ran 264 tests: 263 passed, one failed in the new Python test because a normal regex dot did not span a newline within a Kotlin documentation comment.
- The failing check is source-level only; it does not change the six Android JVM tests or the production model.
- A006 will use a direct substring assertion for the privacy statement and rerun the exact-head checks.


## A006 — exact-head V 3.0.604 final verification

- **Graph and source revision:** V 3.0.604, `79449e5f0a4c81a2082bff29f3d637a16742e695`; current graph was retrieved from the exact commit tree and its Git SHA-1 Blob ID matched the tree entry.
- **Python quality:** 264 tests passed; compileall passed; graph guard validated 229 tasks and 417 edges with zero errors and one preserved T0001 metadata warning. The output/evidence guard also passed.
- **Android:** KtLint, debug APK build and all JVM tests passed; generated Protobuf Java and Kotlin outputs were both verified. Artifact: `whitep-android-debug-apk.zip`, 11,968,326 bytes, ID 12961343214.
- **Windows:** WPF build, Protobuf, startup smoke, agent-tool process/policy/capability checks, DPAPI host identity and peer-trust persistence/tamper checks passed. Artifact: `whitep-windows-host.zip`, 14,254,988 bytes, ID 12960509884.
- **HostLifecycle:** Unit/integration job passed.
- **Workflows:** [Product 41020640994](https://github.com/artinrahimii/project/actions/runs/41020640994) and [Quality 41020640807](https://github.com/artinrahimii/project/actions/runs/41020640807) completed successfully.
- **Outcome:** T0161 acceptance criteria satisfied; no live peer transport capability is implied.


## A007 — recorded-output evidence schema finding

- V 3.0.605 structure validation passed: 229 tasks, 417 edges, zero errors and one preserved T0001 metadata warning.
- The repo-root evidence check failed because four Actions URLs were placed in `T0161.evidence`, which graph_guard resolves as local repository paths.
- Those URLs remain in the `tests_passed` records and narrative reports; the evidence list is now restricted to file paths that exist in the exact V 3.0.605 repository tree.
- T0161 is reopened pending exact-head V 3.0.606 graph and evidence validation.

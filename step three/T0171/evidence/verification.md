# T0171 — verification evidence

## Reviewed base

- Branch: `main`
- Base commit: `166ac6aa0aee8f9c8ea08bac32b2e1f35f6e9b7d` (`V 3.0.723`)
- The large `step three/project-graph.md` was read from its exact base-commit Blob; its UTF-8 byte count and computed Git Blob ID matched the tree entry before task selection.

## Dependency evidence

- T0170: `DistributedRequestProgress`, `AndroidInferenceRequestRuntime.dispatchProgress`, and runtime dispatch produce the request-scoped progress contract extended here.
- T0160: the generated `InferenceRequest.request_id` and payload-builder contract provide the canonical request identity matched against `DistributedRequest.requestId`.
- The two incoming T0171 edges were inspected against those real interfaces, reclassified to `contract_or_interface_dependency`, and recorded in the graph edge-change log. Their `blocks` type and ordering remain unchanged.

## Implemented behavior

- Every successful runtime dispatch records the ordered ephemeral peer IDs for that request.
- All previously attempted peers are excluded from next-round selection by default, including hosts seen in earlier-than-immediately-previous rounds.
- Retry exceptions are explicit per-selection allowlists and are rejected if they name an unrecorded host.
- Incomplete multi-round history fails closed; the legacy counter-only overload is limited to first-round compatibility.
- Kotlin/JVM regression tests and the existing runtime integration test were updated. Exact-head CI is still pending; no test pass is claimed here.

## Pending exact-head checks

- Android KtLint, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin validation.
- Step Three Python tests, syntax, graph, output-path and evidence checks.
- Windows WPF build/startup/security checks and HostLifecycle integration tests.


## A001 — V 3.0.724 exact-head CI finding

Step Three quality workflow 41230294822 passed; HostLifecycle job 124622910470 passed; Windows WPF job 124622910280 passed all format/build/Protobuf/startup/process/policy/capability/DPAPI/artifact steps. Android job 124622910620 failed at `ktlintTestSourceSetCheck`. The exact violations were seven formatting findings in `HostCandidateSelectorTest.kt`: five `DistributedRequestProgress` argument layouts and two multiline-expression layout findings. The APK, JVM tests and generated Protobuf checks were skipped. This is recorded as a failed verification, not as a Kotlin test failure or product pass.

## A002 — formatting correction

The five progress arguments were expanded to KtLint-compliant multiline constructor calls, and the two multiline expressions were moved to the required continuation layout. The fix is awaiting a new exact-head CI run.


## A003 — V 3.0.725 exact-head CI findings

Step Three quality workflow 41230521477 passed. HostLifecycle job 124623603658 passed. Windows WPF job 124623603615 passed C# formatting, Release build, generated Protobuf, startup, process restrictions, policy rejection, backend/model capability gating, DPAPI identity/trust and artifact upload. Android job 124623603397 failed at `ktlintTestSourceSetCheck` with seven indentation findings: candidate-list items/closing delimiter at lines 385–389 and the nested `attemptedEphemeralHostIds` argument/constructor close at lines 440–441. APK, JVM and generated-source checks were skipped.

## A004 — continuation-indentation correction

The five multi-round candidate entries now use the indentation required by KtLint, and the nested `attemptedEphemeralHostIds` argument and progress-constructor close are aligned with the constructor. No production source or runtime behavior changed. Exact-head CI remains pending.


## A004 — V 3.0.726 exact-head CI result

Step Three quality workflow 41230929453 passed. HostLifecycle job 124624805496 passed. Android job 124624805452 failed at `ktlintTestSourceSetCheck` with one finding: `HostCandidateSelectorTest.kt:389:1 Unexpected indentation (16) (should be 20)`. The Android APK, JVM suite and generated Java/Kotlin Protobuf checks were skipped. The Windows WPF job was still running at the time of this record and is not marked passed here.

## A005 — one-line formatting correction

Moved the closing delimiter of the multi-round candidate `listOf` from 16 to 20 spaces, matching the continuation indentation reported by KtLint. No production code or behavior changed. Exact-head CI remains pending.

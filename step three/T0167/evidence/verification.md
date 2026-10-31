# T0167 — multiple-host response rendering evidence

## Prerequisite and edge evidence

- T0166 and T0160 are done.
- T0166 → T0167 is a validated `contract_or_interface_dependency`: the renderer consumes the per-attempt response state and Compose card interface.
- T0160 → T0167 is a validated `sequencing_only` edge: the response renderer does not call or consume the request payload builder. The original blocks ordering remains.

## Production rendering evidence

- `HostResponseCards.kt` iterates the response list and renders one Material 3 Card per element.
- Each card reads its own host, attempt, model, outcome, output and error fields.
- Each subtree is now keyed by `(requestId, attemptId)`, keeping same-host retries distinct in Compose identity.
- No response string joining or aggregate output is used.

## Automated regression evidence

- New test: `tests/test_android_multi_host_response_rendering_contract.py`.
- It checks the per-response loop, the stable request/attempt key, separate Card/output structure and absence of concatenation.
- Existing JVM test `keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard` asserts two entries, distinct output strings and attempt IDs; V 3.0.698 exact-head Android JVM CI passed.

## Scope boundary

This task covers response presentation only. The request screen still identifies Android-to-host request submission as unavailable; it does not prove a complete live multi-host inference round.


## Exact-head V 3.0.700 acceptance — passed

- Step Three quality passed: [41204091924](https://github.com/artinrahimii/project/actions/runs/41204091924), including the dedicated no-auto-merge renderer regression.
- Android KtLint, Debug APK, JVM tests, generated Protobuf Java/Kotlin checks and artifact uploads passed: [job 124545531910](https://github.com/artinrahimii/project/actions/runs/41204092083/job/124545531910).
- Android APK artifact [13044190781](https://github.com/artinrahimii/project/actions/runs/41204092083/artifacts/13044190781): 19,441,958 bytes. JVM reports artifact [13043651529](https://github.com/artinrahimii/project/actions/runs/41204092083/artifacts/13043651529): 44,447 bytes.
- Windows WPF build/security/DPAPI/artifact passed: [job 124545532034](https://github.com/artinrahimii/project/actions/runs/41204092083/job/124545532034); Windows artifact [13044465085](https://github.com/artinrahimii/project/actions/runs/41204092083/artifacts/13044465085): 14,255,799 bytes. HostLifecycle passed: [job 124545532019](https://github.com/artinrahimii/project/actions/runs/41204092083/job/124545532019).
- Full V 3.0.700 product workflow succeeded. T0167 is `done`.
- Scope boundary: response presentation only; complete live peer inference remains unclaimed.

## T0168 incoming dependency review

- T0167 → T0168 validated as `sequencing_only`: the response-round receive runtime does not consume the Compose cards.
- T0160 → T0168 validated as `contract_or_interface_dependency`: the round's remaining wait budget is capped by the absolute DistributedRequest deadline.
- The existing `blocks` edges are preserved and both classifications/evidence are recorded in `edge_change_log`.

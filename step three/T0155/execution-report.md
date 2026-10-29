# T0155 execution report

- **Base revision:** `76245201996af722cacc95ffd77c27d9e3c2942a` on `main` (V 3.0.564).
- **Task:** Implement a replaceable local context-summary provider and validate generated capsules against the current v1 schema and configured UTF-8 byte limit.
- **Prerequisites:** T0154 passed exact-head CI on V 3.0.564. T0150 is done with exact-head evidence for the local conversation/message persistence workflow.
- **Dependency review:** T0154 → T0155 is a contract/interface dependency because the summarizer uses the configured capsule size policy and v1 codec. T0150 → T0155 is sequencing-only: the provider accepts an independent message-value model and does not depend on the older screen/send/Room workflow directly.
- **Implementation:** Added an injectable suspend provider interface, a conservative extractive default that preserves existing user-entered fields and uses explicit field markers for facts/decisions/constraints/open items, and a validation boundary that round-trips output through the existing codec. Invalid settings, unsupported schema, provider errors, empty initial history, and summaries exceeding the selected limit return rejected results rather than accepted output.
- **Privacy boundary:** The default provider is local and extractive, adds no network dependency, and does not copy unmarked transcript lines into the capsule's facts/decisions/constraints. This does not certify a future replacement provider unless it follows the same local-data policy.
- **Tests added:** Five Kotlin tests cover extraction, schema rejection, invalid size configuration, oversized output and preserving an existing capsule if no usable history remains. Python source-contract checks cover replaceability and codec enforcement.
- **Local validation:** The connected GitHub tools do not provide local Gradle/Python execution in this run. No local command is claimed; exact-head CI is pending.
- **Status:** `in_progress`, attempt `T0155-A001` partial. Do not mark this task done until exact-head quality and product checks pass.


## Exact-head V 3.0.565 CI result — attempt A001

- Step Three quality [run 40909822726](https://github.com/artinrahimii/project/actions/runs/40909822726) passed automated Python tests, Python syntax, graph structure and recorded-output/evidence validation.
- Product [run 40909822388](https://github.com/artinrahimii/project/actions/runs/40909822388): Android `ktlintMainSourceSetCheck` failed seven formatting findings in this source; APK build and Android JVM tests were skipped. HostLifecycle passed. Windows WPF was still running when this report was updated.
- A001 is a failed implementation attempt for the Android gate; it is not accepted. A002 changes only formatting and must pass a new exact-head CI run before task completion.


## Exact-head V 3.0.566 CI result — attempt A002

- Step Three quality [run 40910278299](https://github.com/artinrahimii/project/actions/runs/40910278299) passed Python tests, syntax, graph structure and output/evidence checks.
- Product [run 40910278198](https://github.com/artinrahimii/project/actions/runs/40910278198): Android `ktlintMainSourceSetCheck` failed four chain-continuation findings at the field-value expression on line 175. APK build and JVM tests were skipped. HostLifecycle passed; Windows WPF was still running at report time.
- A002 is preserved as a failed attempt. A003 splits the string normalization sequence into independent statements and awaits new exact-head CI.

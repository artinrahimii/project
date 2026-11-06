# T0148 execution report

- **Task:** Restore conversation UI state after rotation and process recreation using ViewModel and saved state.
- **State:** `done` — accepted after exact-head CI on `V 3.0.539`.
- **Accepted implementation commit:** [`2b0c8a8231e3a258021322e6be2f24f6c168fce8`](https://github.com/artinrahimii/project/commit/2b0c8a8231e3a258021322e6be2f24f6c168fce8).
- **Quality CI:** [run 40872603020](https://github.com/artinrahimii/project/actions/runs/40872603020).
- **Product CI:** [run 40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021).

## Dependency review

- **T0147 → T0148:** `contract_or_interface_dependency`, validated. The conversation detail screen consumes and displays T0147's Room-backed request-state rows; saved-state changes were implemented without removing that contract.
- **T0138 → T0148:** `sequencing_only`, validated. The host-visibility/privacy disclosure does not provide an API consumed by local draft restoration. Both original `blocks` edges are retained.
- The rationale/evidence is in the graph's append-only `edge_change_log`. T0148 → T0149 and other unrelated edges were not changed.

## Implementation

- Replaced Compose's non-restorable local `remember` draft with ViewModel-owned `StateFlow` backed by Android `SavedStateHandle`.
- Added `ViewModelProvider.Factory.create(modelClass, extras)` and `extras.createSavedStateHandle()` to receive activity/navigation saved-state data.
- Kept input validation, the Room message persistence path, request-status observation, and `sendMessage(String)` contract unchanged.
- Added `draftIsRestoredAfterViewModelRecreation` to the Robolectric/JVM tests and source-contract assertions for the saved-state factory and screen wiring.

## Exact-head verification

- **Step Three quality** [40872603020](https://github.com/artinrahimii/project/actions/runs/40872603020): 229 Python tests passed; Python syntax, graph structure, and recorded-output/evidence validation passed. Graph guard reported 229 tasks / 417 edges, zero errors, and one pre-existing `T0001 UNMAPPED_DEPENDENCY` warning preserved as metadata.
- **Android product job** [40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021): `ktlintCheck`, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin verification, and debug APK upload passed. The JVM suite includes the saved-draft recreation regression. Uploaded APK artifact: **11,875,221 bytes**.
- **Windows WPF job** [40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021): C# formatting, Release build, generated Protobuf, WPF startup smoke, agent-tool safety/security checks and artifact upload passed. Artifact archive: **14,254,981 bytes**.
- **HostLifecycle job** [40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021) passed.

## Preserved failed attempts and boundary

- A001 / `V 3.0.537`: Android ktlint rejected five factory-formatting findings; APK and JVM tests did not run.
- A002 / `V 3.0.538`: Android ktlint rejected two remaining whitespace findings; APK and JVM tests did not run.
- A003 / `V 3.0.539`: corrected the remaining signature layout; the exact-head workflows then passed. A004 records that observed acceptance and is appended to the graph history.

Acceptance means the unsent draft is restored when Android saved-state restoration is available. It does **not** mean the draft is durably persisted after a normal process restart without saved-state restoration, nor that Android-to-Windows P2P transport or live request transitions are implemented. Durable message/request-state recovery across normal app close/reopen is T0149's scope.

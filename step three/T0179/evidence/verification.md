# T0179 verification evidence

## Exact graph snapshot and output verification

- Repository: `artinrahimii/project`, branch `main`.
- Implementation commit: `0b023a4201b28feb9748c17ec4698bb5528b7d52` (`V 3.0.820`).
- Graph path at the implementation commit: `step three/project-graph.md`.
- Graph Blob: `4b15f304bebac4c6b52012483e45b359f6b30830`.
- Original graph bytes: 3940829 UTF-8 bytes; text length: 3814095 Unicode characters.
- Recomputed Git Blob SHA-1 using the original full bytes matched the tree Blob.
- Full JSON parsed; 230 unique task nodes and 422 edges.
- All expected T0179 source, test, security-document and evidence paths were present in the exact V 3.0.820 tree.

## Dependency review retained

- `T0177 → T0179` and `T0178 → T0179`: edge type `blocks` retained, classification validated as `sequencing_only`, original execution order retained.
- `T0179 → T0180` and `T0177 → T0180`: remain `unreviewed`; no dependent work started.

## Exact-head CI — A005 / V 3.0.820

- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41448964242): success. Python tests, compileall and graph guard passed; graph guard reported 230 tasks, 422 edges, zero graph errors and one existing T0001 metadata warning.
- [White-P product build](https://github.com/artinrahimii/project/actions/runs/41448964240): success; all jobs completed successfully.
  - Android job `125289084756`: success.
    - Kotlin formatting/lint: success.
    - Kotlin compilation and debug APK build: success.
    - JVM unit tests via `testDebugUnitTest`: success.
    - Generated Protobuf Java/Kotlin source verification: success.
    - Android debug APK upload: success.
    - JVM unit-test reports upload: success.
  - Windows WPF job `125289083955`: success; C# formatting, build, generated Protobuf, WPF startup smoke, process restrictions, policy rejection, capability gate and DPAPI identity/trust checks passed.
  - HostLifecycle job `125289084220`: success. Its log explicitly states: `PASS: safe diagnostic EventSource emitted only the three allowlisted numeric fields.`
- [Rendezvous container build and smoke test](https://github.com/artinrahimii/project/actions/runs/41448964125): success.
- Uploaded artifacts:
  - `whitep-android-debug-apk`: ID `13144066959`, 19,529,756 bytes.
  - `whitep-android-unit-test-reports`: ID `13143233051`, 51,034 bytes.
  - `whitep-windows-host`: ID `13143761971`, 14,267,967 bytes.

## Acceptance

T0179's implementation outputs exist in the exact main tree, all relevant exact-head automated checks succeeded, Android APK and test-report artifacts were uploaded, and HostLifecycle exercised the allowlist EventSource. T0179 is therefore marked `done` on V 3.0.820. Four earlier failed attempts remain in the execution history; A005 is the successful fifth attempt.

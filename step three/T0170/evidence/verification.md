# T0170 — verification evidence

## Revisions

- Initial base: `6338ed92c0c7508a9b07b3ee0eafd3a1abaff20a` (`V 3.0.716`), branch `main`.
- A001: `17077b464d0946d28789d0a4f8378f80bee09845` (`V 3.0.717`).
- A002: `63be6aa51b229ecf4f9d49565bee04e9c0a8a4d8` (`V 3.0.718`).
- A003: `62d844488bf9d81c00683963943c251c3de8f3de` (`V 3.0.719`).
- A004: `ce0e6ebdfefbff004a9be0f754e55d653cffaee4` (`V 3.0.720`).
- A005: `e708e8da220078cf783d1610286d793b8ff8f77c` (`V 3.0.721`).
- A006: version/commit pending; parent expected to be A005 after a fresh head check.

## Dependency and source evidence

- `step one/V1.0.md` requires first-release round/host ceilings but does not choose numeric values. The adopted policy (3 rounds, 3 hosts per round, 9 total attempts) is explicitly documented.
- T0169 supplies deterministic three-host selection and the completed response/attempt contract; T0170 adds request-budget progress to that selector API.
- T0160 supplies the canonical generated `InferenceRequest` and payload-builder contract consumed by selection and dispatch.
- No Protobuf wire-schema change was made.

## Exact-head CI evidence

### A001 — V 3.0.717

- Step Three quality: failed, run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757), one Python source-contract failure among 308 tests.
- Android KtLint: failed, product run [41216834811](https://github.com/artinrahimii/project/actions/runs/41216834811); APK/JVM/Protobuf skipped.
- Windows WPF and HostLifecycle passed.

### A002 — V 3.0.718

- Step Three quality: passed, run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620).
- Android KtLint: failed, product run [41217032694](https://github.com/artinrahimii/project/actions/runs/41217032694); helper signature/body placement. APK/JVM/Protobuf skipped.
- HostLifecycle and Windows WPF passed.

### A003 — V 3.0.719

- Step Three quality: passed, run [41217272330](https://github.com/artinrahimii/project/actions/runs/41217272330).
- Android KtLint: failed, product run [41217272243](https://github.com/artinrahimii/project/actions/runs/41217272243); helper argument indentation. APK/JVM/Protobuf skipped.
- HostLifecycle and Windows WPF passed.

### A004 — V 3.0.720

- Step Three quality: passed, run [41217532369](https://github.com/artinrahimii/project/actions/runs/41217532369).
- Android KtLint: failed, product run [41217532383](https://github.com/artinrahimii/project/actions/runs/41217532383); five production-source formatting findings. APK/JVM/Protobuf skipped.
- Windows WPF and HostLifecycle passed.

### A005 — V 3.0.721

- Step Three quality: passed, run [41217933594](https://github.com/artinrahimii/project/actions/runs/41217933594).
- Android KtLint: failed, product run [41217933725](https://github.com/artinrahimii/project/actions/runs/41217933725), at `DistributedRequest.kt:78`. KtLint required the `minOf` call arguments and close parenthesis to be split onto separate lines; the remaining expression also exceeded the line-length limit.
- Windows WPF and HostLifecycle passed.
- Because KtLint failed, `assembleDebug`, `testDebugUnitTest` and generated Protobuf verification were skipped.

### A006 — pending

- Changed only the multiline formatting of the `DistributedRequest.create` default argument; numeric policy and API behavior are unchanged.
- Exact-head quality and product workflows are pending.
- Android KtLint, `assembleDebug`, `testDebugUnitTest`, generated Protobuf, Windows WPF and HostLifecycle are not confirmed for A006.
- Local Gradle/build execution was not run in this connected GitHub session.
- No task-wide acceptance or product-release completion is claimed.

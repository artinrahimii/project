# T0170 — verification evidence

## Revisions

- Initial base: `6338ed92c0c7508a9b07b3ee0eafd3a1abaff20a` (`V 3.0.716`), branch `main`.
- A001: `17077b464d0946d28789d0a4f8378f80bee09845` (`V 3.0.717`).
- A002: `63be6aa51b229ecf4f9d49565bee04e9c0a8a4d8` (`V 3.0.718`).
- A003: `62d844488bf9d81c00683963943c251c3de8f3de` (`V 3.0.719`).
- A004: exact-head commit pending.

## Dependency and source evidence

- `step one/V1.0.md` asks to cap first-release rounds and attempted hosts but does not prescribe numeric values. The chosen policy (3 rounds, 3 hosts per round, 9 total attempts) is explicit in the implementation and task report.
- T0169 supplies the three-host selection and completed response/attempt contract; its selector now also consumes request progress.
- T0160 supplies the canonical generated `InferenceRequest` / payload-builder contract consumed by the selector and dispatcher.
- T0170 does not change `protocol/whitep/v1/messages.proto` or the on-wire payload schema.

## Exact-head CI evidence

### A001 — V 3.0.717

- Step Three quality: failed, run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757), one Python string-contract failure among 308 tests.
- Android KtLint: failed, product run [41216834811](https://github.com/artinrahimii/project/actions/runs/41216834811); APK/JVM/Protobuf steps skipped.
- Windows WPF and HostLifecycle passed.

### A002 — V 3.0.718

- Step Three quality: passed, run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620).
- Android KtLint: failed, product run [41217032694](https://github.com/artinrahimii/project/actions/runs/41217032694); helper expression placement failed. APK/JVM/Protobuf steps skipped.
- HostLifecycle passed. Windows WPF passed after the earlier report check.

### A003 — V 3.0.719

- Step Three quality: passed, run [41217272330](https://github.com/artinrahimii/project/actions/runs/41217272330).
- Android KtLint: failed, product run [41217272243](https://github.com/artinrahimii/project/actions/runs/41217272243); the helper constructor arguments/closing parenthesis had excess indentation. APK/JVM/Protobuf steps skipped.
- HostLifecycle passed. Windows WPF was still in progress at last inspection.

### A004 — pending

- KtLint argument indentation has been adjusted in the helper.
- Exact-head Step Three quality gate and product workflow have not yet run.
- Android KtLint, assembleDebug, testDebugUnitTest, generated Protobuf, Windows WPF and HostLifecycle are not yet confirmed for A004.
- Local Gradle/build execution was not run in this connected GitHub session.
- No task-wide acceptance or product-release completion is claimed at this stage.

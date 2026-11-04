# T0207 verification evidence

## Incoming edges

1. `T0206 → T0207`: `sequencing_only`, validated; original `blocks` type/order retained.
2. `T0195 → T0207`: `sequencing_only`, validated; original `blocks` type/order retained.

## Clean-build implementation evidence

Workflow: `.github/workflows/product-build.yml`.
- Windows: explicit solution clean then non-incremental Release build.
- Android: .NET fixture clean/non-incremental build; Gradle clean; lint and APK assemble from clean outputs.
- HostLifecycle: explicit project clean followed by full lifecycle suite.

## First exact implementation CI — pass

Run `41921424399`, commit `ccaddf87dac832c838c9c270d20fc16e6ccf9e28`:
- Android job `126894611985`: **28/28 passed**, including JVM unit tests, Keystore emulator instrumentation, generated Java/Kotlin Protobuf source verification and APK upload.
- Windows job `126894612727`: **16/16 passed**.
- HostLifecycle job `126894612520`: **9/9 passed**.
- Quality Gate `41921424419` and Rendezvous `41921424425`: passed.

## Later exact-head rerun — external Maven throttling

Run `41922371821`, commit `56c307679e79b72177eb48666b9f59ce0d541e47`:
- Windows and HostLifecycle remained fully green (16/16 and 9/9).
- The Android clean fixture, Gradle clean, lint, APK and permission steps passed.
- `testDebugUnitTest`: 191 tests completed, 5 failed during Robolectric setup; 186 passed.
- Downloaded JUnit report artifact `13368600158` shows all five failures caused by HTTP 429 from repo1.maven.org for `org.robolectric:android-all-instrumented:15-robolectric-13954326-i7`, not by assertion failures in the affected tests.

## Correction under verification

- `src/Client.Android/app/build.gradle.kts` now configures `robolectric.dependency.repo.url` and `robolectric.dependency.repo.id` for all Android JVM test tasks.
- Default runtime repository: `https://maven-central.storage-download.googleapis.com/maven2/`.
- Override: Gradle property `robolectricDependencyRepoUrl` or environment variable `ROBOLECTRIC_DEPENDENCY_REPO_URL`.
- `tests/test_android_libp2p_transport_contract.py` asserts the mirror configuration remains attached to `unitTests.all`.

No success is claimed for the mirror correction yet. T0207 remains `in_progress` pending a new exact-head CI pass.

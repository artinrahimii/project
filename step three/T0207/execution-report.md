# T0207 execution report

- Status: **in_progress**
- Attempts: 2 (one clean-build implementation attempt; one rate-limit resilience correction)
- Implementation baseline: `ccaddf87dac832c838c9c270d20fc16e6ccf9e28` (`V 3.0.943`)
- Current tracking revision: `56c307679e79b72177eb48666b9f59ce0d541e47` (`V 3.0.944`)
- Current correction: `T0207-A002`, CI pending

## Incoming edge review

- `T0206 → T0207`: **validated / sequencing_only**. T0207 rebuilds from a fresh checkout and consumes no T0206-specific API, schema, fixture, or generated output. Original `blocks` edge and order retained.
- `T0195 → T0207`: **validated / sequencing_only**. T0207 consumes no task-specific output from T0195. Original `blocks` edge and order retained.

## Clean-build workflow implementation

The online workflow `.github/workflows/product-build.yml` now explicitly cleans outputs before compiling on fresh GitHub-hosted runners:

- Windows: `dotnet clean WhiteP.sln`, then a non-incremental Release build; WPF startup and Windows acceptance checks follow.
- Android: clean the .NET interop fixture and rebuild it non-incrementally, run Gradle `clean`, lint and build the debug APK with build cache disabled; execute JVM and emulator tests and verify generated sources.
- HostLifecycle: `dotnet clean` followed by the full lifecycle suite.

## Verified CI results

**Implementation commit V 3.0.943 — full success**

Product Build run `41921424399` completed successfully:
- Android job `126894611985`: **28/28 steps passed**, including clean fixture build, Gradle clean, APK build/permission checks, all JVM tests, Android Keystore instrumentation, generated Protobuf source checks and APK artifact upload.
- Windows WPF job `126894612727`: **16/16 passed**.
- HostLifecycle job `126894612520`: **9/9 passed**.
- Step Three Quality Gate `41921424419`: passed.
- Rendezvous container run `41921424425`: passed.

**Follow-up exact-head verification V 3.0.944 — one external dependency failure**

Product Build run `41922371821`:
- Windows WPF: **16/16 passed**.
- HostLifecycle: **9/9 passed**.
- Android: clean .NET fixture build, Gradle clean, KtLint, clean APK assembly and permission validation passed; `testDebugUnitTest` then ran **191 tests, 5 failed**.
- JUnit report artifact `13368600158` identifies the same cause for all five failures: Robolectric `MavenArtifactFetcher` received HTTP **429 Too Many Requests** from `https://repo1.maven.org/maven2` while fetching `org.robolectric:android-all-instrumented:15-robolectric-13954326-i7`. The failures occurred during Robolectric sandbox setup before the five affected test bodies; **186 other tests passed**.
- Affected class setup reports: `ConversationContextCapsuleSummarizerTest`, `ConversationContextCapsuleTest`, `ClientDatabaseMigrationTest`, `ClientDatabaseTest`, and `ConversationContextCapsulePersistenceTest`.

## Resilience correction T0207-A002

Robolectric's documented runtime properties allow an independent Maven repository URL and ID. The Android test configuration now uses a Google-hosted Maven Central mirror by default and allows override through `-ProbolectricDependencyRepoUrl` or `ROBOLECTRIC_DEPENDENCY_REPO_URL`. A Python quality-gate contract test guards this configuration.

This correction has not yet run on CI. **T0207 remains in_progress** until the next exact-head Quality Gate and full Product Build pass.

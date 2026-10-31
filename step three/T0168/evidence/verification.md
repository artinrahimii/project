# T0168 — round response collection verification

- Runtime API: `AndroidInferenceRequestRuntime.awaitRoundResponses`; it binds each queued attempt to the shared window stored during dispatch.
- Collector: concurrently awaits all queued attempts and returns independent response/failure outcomes.
- Existing transport authentication and request/attempt/round/model/deadline correlation validation is unchanged.
- JVM regression coverage includes first-success continuation, sibling survival after timeout, non-queued submission outcomes, expired-window handling and caller cancellation.
- Python source-contract tests verify runtime integration and JVM test presence.
- V 3.0.702 Step Three quality passed; Windows WPF and HostLifecycle passed.
- V 3.0.702 Android KtLint failed with three formatting findings; APK/JVM/Protobuf did not run.
- A002 applies only those style corrections. V 3.0.703 exact-head CI pending. Local Gradle was unavailable.
- T0168 remains in_progress until exact-head Android and quality gates pass.

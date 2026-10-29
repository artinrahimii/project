# T0154 execution report

- **Task:** Make the capsule size limit configurable in settings and enforce it in the domain layer.
- **Status:** `done`; implementation and regression tests passed exact-head CI on V 3.0.564.
- **Baseline:** online `main` at [V 3.0.561](https://github.com/artinrahimii/project/commit/bc14edc67014d8456ff9a4a7a845698cc767c3ad).
- **Attempts:** `T0154-A001`, `T0154-A002`, and `T0154-A003`.
- **Scope:** Android device-local setting and domain/Room-save enforcement. The immutable protocol ceiling remains 4,096 serialized UTF-8 bytes. This task does not change the capsule wire schema, add host transport, or claim at-rest encryption.

## Dependency review

- **T0153 → T0154:** validated as `contract_or_interface_dependency`. T0154 extends the capsule form and ViewModel introduced by T0153 and passes the selected limit to the existing repository/codec save contract. The original `blocks` edge is retained.
- **T0150 → T0154:** validated as `sequencing_only`. T0150's accepted tests cover local conversation/message creation and history restoration, but do not consume a size setting or define the domain limit policy. The original `blocks` edge is retained.
- Both reviews were appended to `edge_change_log`; neither edge was removed.

## Implementation

- Added `SharedPreferencesConversationContextCapsuleSizeLimitStore` to persist a device-wide limit, defaulting to 4,096 bytes.
- Added a domain policy for limits from 512 to 4,096 bytes in 256-byte steps. The codec rejects invalid limit values; corrupted persisted values are normalized before use.
- The default `ConversationContextCapsule.encode()` still enforces the 4,096-byte protocol maximum. An explicit configured limit can only lower that ceiling. The Room repository accepts a selected limit while retaining a backward-compatible default for existing callers.
- Wired the preference into the capsule UI as a slider and into ViewModel/Room save. Lowering the setting does not delete existing capsules or make valid stored capsules unreadable; the stricter threshold applies to later saves.
- Added domain, SharedPreferences, ViewModel persistence/enforcement, and Python source-contract tests. No new library dependency was added.

## Verification state

No local Android build or test command was run in this connector execution. The attempt is intentionally `partial` until exact-head CI runs Kotlin lint, APK build, Android JVM tests, Python/graph/evidence checks, Windows WPF and HostLifecycle gates. CI failures must be corrected and recorded; do not mark this task done until the relevant gates pass.


## First exact-head CI result — V 3.0.562

- Quality [run 40903639160](https://github.com/artinrahimii/project/actions/runs/40903639160) completed successfully. Automated Python tests, Python syntax, graph structure, and recorded-output/evidence checks passed.
- Product [run 40903639332](https://github.com/artinrahimii/project/actions/runs/40903639332) completed with an Android lint failure and HostLifecycle and Windows WPF jobs successful. Android `ktlintMainSourceSetCheck` reported three instances of “A multiline expression should start on a new line” in `ConversationContextCapsuleViewModel.kt` at lines 146, 152, and 158. APK build and Android JVM tests did not run because lint failed.
- **T0154-A001 is recorded as failed for the product lint gate**, not accepted. **T0154-A002** moves the three `_uiState.value.copy(...)` expressions onto continuation lines; behavior is unchanged. Exact-head CI remains pending.


## Second exact-head CI result — V 3.0.563

- Quality [run 40904263570](https://github.com/artinrahimii/project/actions/runs/40904263570) passed the Python tests, syntax, graph structure and recorded-output/evidence checks.
- Product [run 40904263586](https://github.com/artinrahimii/project/actions/runs/40904263586): the main Kotlin source lint passed, then test-source lint failed at `ConversationContextCapsuleViewModelTest.kt:138` with “Expected newline before '.'” in the `.errorMessage!!.contains(...)` assertion. APK and Android JVM tests were skipped. HostLifecycle passed; Windows WPF was still in progress at last inspection.
- **T0154-A002 is recorded as failed for test-source lint.** **T0154-A003** binds the error message to a local variable before asserting, avoiding the flagged chain formatting. The behavior is unchanged; exact-head CI is pending.


## Final exact-head CI result — V 3.0.564

- Exact tested main commit: `76245201996af722cacc95ffd77c27d9e3c2942a`.
- Step Three quality run [40904612498](https://github.com/artinrahimii/project/actions/runs/40904612498) passed Python tests, syntax, graph structure and recorded-output/evidence checks.
- Product run [40904612247](https://github.com/artinrahimii/project/actions/runs/40904612247) passed Android Kotlin lint, debug APK build, Android JVM tests, generated Protobuf validation and APK artifact upload; Windows WPF and HostLifecycle jobs also passed.
- A003 is accepted as succeeded. Earlier A001 and A002 lint failures remain preserved in the graph history.

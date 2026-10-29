# T0154 execution report

- **Task:** Make the capsule size limit configurable in settings and enforce it in the domain layer.
- **Status:** `in_progress`; implementation and tests are committed with exact-head CI pending.
- **Baseline:** online `main` at [V 3.0.561](https://github.com/artinrahimii/project/commit/bc14edc67014d8456ff9a4a7a845698cc767c3ad).
- **Attempt:** `T0154-A001`.
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

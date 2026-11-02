# T0178 verification evidence

## Base revision inspected
- Branch: `main`
- Base commit: `08431f8eabb2a6010be6cd3f8cd149ec8928acbe` (`V 3.0.812`)
- `ChatConversation` stores messages in an in-memory `List<ConversationMessage>`; the WPF window clears it when the user clears the session.
- `LocalHostSettingsStore` persists configuration fields (local endpoint, model ID, timeout and local paths), not request/response content.
- Host-side peer identity and trust stores persist cryptographic identity/trust material, not prompt/response text.
- Android Room message persistence is a distinct client-side behavior documented in ADR-0020.
- Search of the inspected host runtime files found no file-writing, append-log, or serialization-to-disk API in `ChatConversation`, `MainWindow`, `HostInferenceSession`, host network listener, or the local inference adapter's request path. The local backend serializes request bytes in memory for the configured localhost HTTP engine.

## Dependency review
- `T0177 → T0178`: validated as `sequencing_only`; no artifact/contract from the prior Android attempt-budget tests is consumed by host conversation retention. The edge's `type=blocks` and execution order remain unchanged.
- Evidence: `step three/T0177/execution-report.md`, `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`, `tests/test_android_inference_attempt_budget_acceptance.py`, `src/Host.Application/Chat/ChatConversation.cs`, `src/Host.Windows/MainWindow.xaml.cs`.

## New evidence created
- Immutable default policy: `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`.
- Runtime test: `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`.
- Source regression test: `tests/test_host_conversation_retention_policy.py`.
- Scoped privacy/security explanation: `docs/security.md`.

## Execution boundary
- Status while awaiting exact-head CI: `partial`.
- HostLifecycle, Android, Python quality, graph/evidence and container checks have **not** yet been run against this new commit. This evidence file does not claim they passed.
- This task does not prove operating-system memory zeroization, paging/crash-dump behavior, external diagnostic handling, complete log redaction (T0179), temporary-buffer cleanup (T0180), or end-to-end operational privacy acceptance (`AC-PRIV-01`).

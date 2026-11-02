# T0178 verification evidence

## Base source review
- Branch: `main`; base `08431f8eabb2a6010be6cd3f8cd149ec8928acbe`.
- Host conversation messages are held by the in-memory `List<ConversationMessage>`.
- `MainWindow` clears the in-memory conversation when the user clears the session.
- `LocalHostSettingsStore` persists model/backend configuration only; it has no prompt, response, or capsule fields.
- Host peer identity/trust persistence is separate cryptographic metadata.
- Android Room message persistence is a distinct client-side behavior recorded in ADR-0020.
- Inspected host runtime sources showed no durable file/log/JSON serialization path for the conversation; local inference request serialization occurs in memory for the local engine request.

## Dependency validation
- `T0177 → T0178` is validated as `sequencing_only`; the existing `blocks` relation and order are preserved.
- Evidence: T0177 report, host candidate/attempt-dispatch tests, existing `ChatConversation` and `MainWindow` source.

## Implemented output
- `HostConversationRetentionPolicy.Default`: process-session-only scope and no persistent prompt/response storage.
- `ChatConversation`: accepts and exposes the policy.
- `MainWindow`: explicitly wires the default policy.
- HostLifecycle and Python tests cover policy use and guard against reintroducing persistence APIs/settings content.
- `docs/security.md` scopes the claim and lists independent limitations.

## Attempts and test evidence
- A001 commit `1bd292f8daddc2466177aa48844b84c7a8603db3`: Step Three quality gate run [41439579310](https://github.com/artinrahimii/project/actions/runs/41439579310) failed one existing WPF source assertion that expected the old parameterless initializer. The other 331 Python tests did not report failure; this does not replace Windows/Android build evidence.
- A002 updates `tests/test_wpf_entrypoint.py` to the explicit default-policy initializer. Exact-head CI is pending; no pass is claimed yet.

## Explicit non-claims
- No proof of OS pagefile/swap, crash-dump or memory-zeroization behavior.
- No full log-redaction proof (T0179), temporary-buffer cleanup proof (T0180), or independent network/storage trace for `AC-PRIV-01`.
- Android Room persistence remains local client storage and is not disabled by this host-side task.

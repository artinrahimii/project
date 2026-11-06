# T0130 verification evidence — attempt A001

- **Baseline commit:** 4a0715ef5465d9d94a5a4fa7d639c1f111eaeba1 (V 3.0.435), branch main.
- **Graph source:** step three/project-graph.md, Blob b9ba4d28eb0e26afd57812d70e8b76a543e7432d, fetched from the baseline tree. The response contains 2,111,024 UTF-8 bytes (1,989,126 Unicode characters); the original fenced JSON parses completely and its Git Blob SHA-1 matches the tree entry.
- **Implementation files:** src/Shared.Security/HostIdentityStore.cs, src/Host.Windows/Security/WindowsDpapiSecretProtector.cs, src/Host.Windows/Security/WindowsHostIdentityProvider.cs, portable and Windows-only acceptance tests, the Python source guard, the Windows workflow invocation, and ADR-0017.
- **Pre-publish defect checks:** corrected the two non-constant relational-pattern compile errors in the candidate store, added using Google.Protobuf for the Windows test's PrivateKey.ToByteArray() extension, and avoided a redundant ProtectedData package reference in the WPF app.
- **Tests:** not yet run locally in this tool environment. Exact-commit GitHub Actions runs are pending after push. Do not claim DPAPI verification until Windows CI emits the explicit pass line and the quality/product jobs succeed.
- **Failure policy:** malformed, unreadable or unprotectable existing identity must fail closed and must not be replaced. Concurrent first-run callers must reload the identity that wins the atomic no-overwrite operation.
- **Security boundary:** the portable test protector is reversible and test-only; it provides no confidentiality. DPAPI CurrentUser does not protect against malicious code already running as the same user. AC-P2P-01 remains NOT RUN.


## Exact V 3.0.436 result — 2026-10-27

- Step Three quality gate: PASS ([run 40577737262](https://github.com/artinrahimii/project/actions/runs/40577737262)).
- Windows WPF build/startup plus DPAPI at-rest, stability and tamper acceptance: PASS (run 40577737280; Windows job 122448561129). The explicit log line states: “PASS: Windows current-user DPAPI preserves the host PeerId, protects key bytes at rest and rejects tampered storage.”
- Android APK/lint/Protobuf: PASS (run 40577737280).
- Real local inference integration: PASS ([run 40577737171](https://github.com/artinrahimii/project/actions/runs/40577737171)).
- HostLifecycle: FAIL (run 40577737280, job 122448561285): concurrent first-run callers on separate store instances returned different PeerIds. The feature is not accepted; implementation corrected with path-scoped named-mutex synchronization in A002.

## Attempt A002

The named-mutex repair is now committed for rerun. Its tests are pending; this record must be updated from exact commit CI before T0130 can be marked done.


## Attempt A002 — exact V 3.0.437 results

- **Graph integrity:** the graph content in this commit lineage remains structurally parseable; the next ledger commit records A002's code commit SHA.
- **Step Three quality gate:** PASS ([run 40578402636](https://github.com/artinrahimii/project/actions/runs/40578402636)).
- **HostLifecycle suite:** PASS (product run 40578402808, job 122450934049). Explicit result: “PASS: host identity store serializes concurrent creation, preserves PeerId on reload and fails closed on invalid or unreadable files.”
- **Windows product/DPAPI job:** PASS (product run 40578402808, job 122450934039); Windows WPF build/startup and the dedicated DPAPI stability, at-rest privacy and tamper-rejection test succeeded.
- **Android job:** PASS (product run 40578402808, job 122450933657); APK, Kotlin lint and Protobuf checks succeeded.
- **A001 failure:** preserved in execution history; the cross-instance race was repaired by a named mutex scoped to the normalized identity-file path and the failing test passed on A002.
- **Boundary:** the successful tests establish the T0130 local host identity/store behavior, not production inbound reachability, peer enrollment, Android-to-Windows E2E or an independent cryptographic audit. T0121 remains blocked; AC-P2P-01 remains NOT RUN.

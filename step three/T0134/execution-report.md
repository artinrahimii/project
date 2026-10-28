# T0134 execution report

- **Task:** T0134 — ردکردن هویت/کلید نامعتبر، همتای ناشناخته و نسخهٔ ناسازگار
- **State:** `in_progress` — partial acceptance coverage passed; one secure-channel acceptance gap remains.
- **Reviewed base:** [V 3.0.458](https://github.com/artinrahimii/project/commit/c49edacbc89498ce35a3ba906b463ce305c5136a)
- **Implementation/test commit:** [V 3.0.459](https://github.com/artinrahimii/project/commit/234b79b9248f38a037febba07c55a265b2e0fc31)

## Work completed

Reviewed the implementation paths already present in the application: `PeerSecurityPolicy` rejects malformed/non-normalized PeerIds; `PeerTrustStore` rejects peers not explicitly trusted and fails closed for corrupt storage; `HostIdentityStore` rejects unreadable/corrupt identities and unsupported persisted identity-envelope versions; `Libp2pPeerTransport` checks the authenticated peer identity. The HostLifecycle test runner also exercises malformed and unsupported **application protocol** versions.

Added two explicit negative assertions to `tests/HostLifecycle.Tests/HostIdentityStoreAcceptanceTests.cs`:
1. An unsupported persisted identity-envelope version must be rejected and left unchanged.
2. Malformed private-key protobuf bytes must be rejected and left unchanged instead of being overwritten with a fresh identity.

The tests stay in the repository's existing integrated HostLifecycle test project rather than creating a parallel `tests/security/` project.

## Verified test results

- [HostLifecycle integration job, run 40717445528 / job 122918865305](https://github.com/artinrahimii/project/actions/runs/40717445528/job/122918865305): **passed** on commit `234b79b9248f38a037febba07c55a265b2e0fc31`. Log line at 15:35:52Z reports the identity store rejects corrupt envelopes, unsupported identity versions and malformed private keys. The same job also reports PASS for malformed PeerId validation and fail-closed unknown/corrupt peer trust.
- [Step Three quality gate, run 40717445979](https://github.com/artinrahimii/project/actions/runs/40717445979): **passed** on the same commit.

No local build was run in this connector-only environment. At the time of this update, the Windows WPF, Android APK, and real local-inference workflow jobs had not all completed; this report does not claim those runs succeeded.

## Why the task remains in progress

The code delegates secure-channel cryptography and peer authentication to the maintained libp2p Noise/TLS stack; it does not implement its own crypto negotiation. The verified tests cover invalid persisted key material, malformed PeerIds, unknown peers, and application-protocol version incompatibility. They do **not yet** directly prove that an incompatible libp2p Noise/TLS secure-channel protocol/version or invalid secure-channel certificate is rejected in a negative integration test. Therefore T0134 is intentionally not marked `done`.

## Dependency review

- `T0133 → T0134`: validated `contract_or_interface_dependency`; T0134 consumes the actual identity, peer-trust and authenticated-transport contracts.
- `T0128 → T0134`: validated `sequencing_only` and removed from active edges. The independent-network E2E run produces no artifact consumed by the deterministic security rejection tests. This does not claim that T0128 has passed; it remains `not_started`.

## Next action

Add a real negative integration case at the libp2p secure-channel boundary for unsupported secure-channel negotiation/certificate validation (without implementing custom cryptography), run it in CI, and only then decide whether T0134 satisfies all of its intended acceptance scope.


## Latest attempt: T0134-A003

- **Code/test commit:** [V 3.0.461](https://github.com/artinrahimii/project/commit/a9ab827ab2d2a767825085e7185eba655fafbe29), on `main`.
- **Changed file:** `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`.
- **Change:** added `VerifySecureChannelRejectsUnexpectedPeerIdentityAsync`. It creates a syntactically valid but different libp2p PeerId, explicitly trusts that ID on the dialer, rewrites the actual listener address to advertise the different ID, and attempts a real local libp2p connection. The intended negative assertion is that the actual remote identity is rejected and no connected/remote identity is published to the dialer.
- **Test result:** pending. The local environment did not run .NET tests. The GitHub combined-status lookup for the exact code commit returned no status entries, which does **not** establish CI success or failure.
- **State remains** `in_progress` until the exact-commit HostLifecycle job verifies this case and the logs confirm it was exercised.
- **Security scope:** this case targets authenticated remote-identity/certificate mismatch at the libp2p transport boundary. It does not by itself prove rejection of every unsupported Noise/TLS protocol-version negotiation.
- **Next action:** inspect the exact commit's GitHub Actions run and test log. If the job fails, diagnose/fix and re-run; if it passes, review whether all T0134 acceptance scope is covered before changing the node to `done`.


## Latest attempt: T0134-A004

- **Code/test commit:** [V 3.0.465](https://github.com/artinrahimii/project/commit/e0512ece82f900e2a3f4586a6c286454eac994a2), on `main`.
- **Changed file:** `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`.
- **Additional negative test:** `VerifyRejectsPlaintextOnlyPeerAsync` creates a real local libp2p listener configured to offer plaintext only, then attempts to connect with the application's secure-only transport to the listener's actual PeerId. The intended acceptance result is that protocol negotiation fails and the client never reports a connected/authenticated peer. This tests fail-closed behavior when no mutually supported secure channel exists; it is not a direct test of every unsupported Noise/TLS protocol version.
- **Prior negative test retained:** `VerifySecureChannelRejectsUnexpectedPeerIdentityAsync` attempts a connection where the dial address advertises a valid but incorrect remote PeerId.
- **Test result:** pending. No local .NET execution was possible because this environment has no `dotnet` executable. The GitHub combined commit-status query returned no entries; the available workflow-run retrieval only exposes pull-request-triggered runs, so this connector cannot verify a push-triggered run or its logs. Empty status data is not treated as a pass or failure.
- **State remains** `in_progress` until the exact-commit HostLifecycle job verifies the added cases. No test has been marked passed from code presence alone.
- **Next action:** retrieve the GitHub Actions check run for commit `e0512ece82f900e2a3f4586a6c286454eac994a2`, verify both security-negative cases executed and passed, then decide whether the remaining unsupported secure-channel version requirement needs a separate test before completion.


## CI failure diagnosis — T0134-A005 and T0134-A006

The exact push-triggered workflow runs are now available through GitHub's Actions REST endpoint (the earlier connector helper did not expose push runs):

- [V 3.0.465 product-build run 40722598672](https://github.com/artinrahimii/project/actions/runs/40722598672), HostLifecycle job 122937219297: **failed**.
- [V 3.0.468 product-build run 40723019009](https://github.com/artinrahimii/project/actions/runs/40723019009), HostLifecycle job 122938717256: **failed again**. The Android APK and Windows WPF jobs passed.
- [V 3.0.468 Step Three quality gate 40723018973](https://github.com/artinrahimii/project/actions/runs/40723018973): **passed**.

Both HostLifecycle logs terminate in `VerifySecureChannelRejectsUnexpectedPeerIdentityAsync`: the libp2p dial throws `Nethermind.Libp2p.Core.Exceptions.PeerConnectionException`, but the test catch filter did not include that wrapper exception. The exception escaped the test runner before its negative assertion and before `VerifyRejectsPlaintextOnlyPeerAsync` could run. This proves the test suite failed; it does not prove a plaintext connection was accepted or that the intended rejection assertion passed.

## Corrective attempt — T0134-A007

The catch filter in `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` now includes `PeerConnectionException`, the failure type actually emitted by the maintained libp2p stack. The test still asserts that the failed handshake leaves the client disconnected and `RemotePeerIdentity` null. No .NET test has yet run against this corrective revision; its result is **pending CI**, not passed.

T0134 remains `in_progress`. After CI, verify that both negative tests execute and pass. Separately determine whether the acceptance requirement still needs a direct negative test for an unsupported Noise/TLS secure-channel protocol version; this fix does not close that possible coverage gap.

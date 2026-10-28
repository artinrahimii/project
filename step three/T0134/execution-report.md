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


## CI verification — T0134-A008

The exact push-triggered CI for [V 3.0.469](https://github.com/artinrahimii/project/commit/782be7fbd75edba6160f9e93f5c8323976764df6) completed successfully:

- [White-P product build run 40726185754](https://github.com/artinrahimii/project/actions/runs/40726185754): HostLifecycle job 122949971717 passed. Its log explicitly prints PASS for both `VerifySecureChannelRejectsUnexpectedPeerIdentityAsync` and `VerifyRejectsPlaintextOnlyPeerAsync`; Android APK formatting/build/Protobuf checks and Windows WPF build/smoke/security checks also passed.
- [Real local inference integration run 40726185709](https://github.com/artinrahimii/project/actions/runs/40726185709): passed, including the real local llama.cpp/GGUF response check.
- [Step Three quality gate run 40726185483](https://github.com/artinrahimii/project/actions/runs/40726185483): passed.

This confirms the A007 exception-filter correction and the identity-mismatch/plaintext-only negative cases. It does not cover an explicitly unsupported libp2p secure-channel protocol ID/version.

## New acceptance case — T0134-A009 (pending CI)

Added `VerifyRejectsUnsupportedSecureChannelProtocolVersionAsync` to the integrated HostLifecycle acceptance suite. It starts a real local libp2p listener whose stream stack advertises only `/tls/999.0.0`, while the production secure client offers its normal supported Noise/TLS protocols. The expected behavior is negotiation failure with neither a connected state nor an authenticated remote identity. The test uses the maintained libp2p stack and a test-only protocol-ID fixture; it adds no custom cryptography and changes no production protocol policy.

The test has not yet run on this revision. T0134 remains `in_progress` until the exact commit's HostLifecycle logs confirm this new assertion and the prior negative cases all passed. This specifically tests unsupported libp2p secure-channel protocol ID/version negotiation; it does not independently test every possible TLS wire-level protocol-version failure.


## T0134-A009 source review correction

During review of the new `/tls/999.0.0` test fixture, a string-interpolation typo was found in the constructed listen address: the draft emitted a literal dollar sign before the address. It was corrected in [V 3.0.471](https://github.com/artinrahimii/project/commit/406baaec426819380fe6e7588b297df3ad6f39de) to construct `{tcpAddress}/p2p/{serverPeerIdentity}` correctly when the listener address does not already contain a peer ID. The A009 test has not been treated as passing; the CI outcome for the superseded V 3.0.470 revision must be read separately from the corrected revision.

## Corrective attempt — T0134-A010 (pending CI)

- **Change:** fixed the test-only multiaddress interpolation in `VerifyRejectsUnsupportedSecureChannelProtocolVersionAsync`; its server still advertises only `/tls/999.0.0`, and its client must reject negotiation without publishing connection state.
- **Verification:** not yet available for the corrective revision. T0134 remains `in_progress`; the exact commit's HostLifecycle job must print the unsupported-version PASS line and still pass the identity-mismatch and plaintext-only cases.


## Final acceptance evidence — T0134-A010

The corrected source revision [V 3.0.471](https://github.com/artinrahimii/project/commit/406baaec426819380fe6e7588b297df3ad6f39de) was verified by exact-commit GitHub Actions:

- [White-P product build run 40727866470](https://github.com/artinrahimii/project/actions/runs/40727866470), HostLifecycle job 122955900543: **passed**. The logs explicitly report PASS for the unexpected authenticated remote PeerId case, plaintext-only peer rejection without downgrade, and unsupported `/tls/999.0.0` secure-channel protocol ID/version negotiation.
- In the same run, Android job 122955899965 and Windows WPF job 122955900557 passed. [Real local inference integration run 40727866362](https://github.com/artinrahimii/project/actions/runs/40727866362) passed, as did the [Step Three quality gate run 40727866237](https://github.com/artinrahimii/project/actions/runs/40727866237).

The V 3.0.470 PASS line for the new version-ID case is deliberately **not** used as acceptance evidence because source review found the initial address interpolation could generate a malformed multiaddress. V 3.0.471 fixed the address, and the actual HostLifecycle run for that corrected revision independently emitted the expected PASS line. Earlier failed A005/A006 attempts remain in history, with the uncaught `PeerConnectionException` cause and corrective evidence preserved.

**Final state:** T0134 is now `done`. Evidence covers malformed persisted key/identity material, unknown/untrusted peers, authenticated remote identity mismatch, plaintext-only downgrade rejection, application-protocol version checks, and unsupported libp2p secure-channel protocol ID/version negotiation. It does not claim exhaustive coverage of every TLS wire-level configuration.


## Graph quality-gate follow-up — V 3.0.472 / V 3.0.473

The Step Three quality gate for [V 3.0.472](https://github.com/artinrahimii/project/commit/e17296c1c9a220525b7b66eee92255a117008a22), run [40728705601](https://github.com/artinrahimii/project/actions/runs/40728705601), found two graph-ledger metadata errors: `T0134` was marked done with an empty node-level `evidence` array, and that array did not directly reference a test source file. The graph guard’s 191 Python tests and syntax check passed; graph validation then failed with exactly those two errors. This did not invalidate the already-passing HostLifecycle test run, but it meant the evidence ledger was incomplete.

The V 3.0.473 correction adds explicit node-level evidence links to the existing identity-store, peer-security, peer-trust and transport test sources plus the T0134 report/evidence files. No task status or dependency edge was changed in this correction. The exact V 3.0.473 quality-gate result must be checked before claiming the ledger is clean.

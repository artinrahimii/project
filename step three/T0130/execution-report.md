# T0130 execution report — in progress

- **Task:** create a stable host key identity and persist its private key in secure OS-backed storage.
- **Attempt:** T0130-A001.
- **Baseline:** V 3.0.435 — 4a0715ef5465d9d94a5a4fa7d639c1f111eaeba1.
- **Dependency review:** T0129 → T0130 was validated as contract_or_interface_dependency: T0129 provides the shared security project and maintained libp2p identity/PeerId boundary. T0128 → T0130 was validated as sequencing_only: the independent-node E2E acceptance does not consume key-store output; stable host identity can precede that test. Both graph edges remain present with the original type=blocks.
- **Implementation:** added HostIdentityStore with a bounded/versioned envelope, an OS-backed protector interface, atomic no-overwrite first-run persistence, and fail-closed handling of corrupt/unreadable identity data; added a Windows DPAPI CurrentUser adapter and provider under LocalApplicationData; added portable and Windows-only acceptance tests, a Python source guard, and ADR-0017.
- **CI regression correction:** prior candidate CI exposed non-constant relational-pattern compile errors, and the Windows test did not import the Google.Protobuf extension namespace required for PrivateKey.ToByteArray(). These are corrected before this reimplementation. The redundant ProtectedData package reference is not added to the WPF app project; the test project references it for the platform adapter compile.
- **Verification:** local repository/SDK build execution is unavailable in this connected tool environment. Exact-commit GitHub Actions must be inspected before T0130 can be marked done. No test is claimed passed in this report.
- **Boundary:** this identity provider is not yet wired into WPF participation or a remotely reachable listener. T0121 stays blocked because inbound request handling remains fail-closed and no verified peer-reachable address/rendezvous configuration is wired. AC-P2P-01 remains NOT RUN.
- **Next:** inspect all workflows for the resulting commit; repair any failures, then update T0130 status only after portable tests, the Windows DPAPI acceptance test and product/quality gates have actually passed.


## T0130-A001 — exact V 3.0.436 CI outcome

- Step Three quality gate passed: [run 40577737262](https://github.com/artinrahimii/project/actions/runs/40577737262).
- Windows WPF build/startup and the explicit Windows DPAPI host-identity acceptance passed; Android APK/lint/Protobuf also passed in [product run 40577737280](https://github.com/artinrahimii/project/actions/runs/40577737280).
- Real local llama.cpp/GGUF inference passed: [run 40577737171](https://github.com/artinrahimii/project/actions/runs/40577737171).
- **Failure requiring repair:** the HostLifecycle job [122448561285](https://github.com/artinrahimii/project/actions/runs/40577737280) failed because concurrent first-run calls using separate HostIdentityStore instances returned different PeerIds. The object-level lock was insufficient for cross-instance coordination. T0130 remains in progress.

## T0130-A002 — concurrency repair

- Added a path-scoped named mutex around LoadOrCreate so multiple store instances and cooperating processes serialize first-run creation and subsequent loading of the winning identity. Timeout and abandoned-mutex behavior are handled explicitly; private-key serialization, DPAPI protection, atomic write and fail-closed checks remain.
- The fix is committed for exact-commit CI rerun. No result is claimed for A002 until the workflows complete.

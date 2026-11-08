# Release limitations and acceptance status

**Audit snapshot:** `main` at `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`), reviewed 2026-11-09.  
**Status:** Not approved for release. A successful CI build is not a substitute for product acceptance.

## Verified in current-head CI

- Quality Gate passed: 480 Python tests, Python compile checks and both graph guards.
- Product Build passed Android lint/build, JVM and Keystore instrumentation, Windows WPF build/publish/startup smoke, HostLifecycle tests, and Windows installer build/acceptance.
- Rendezvous container build and HTTP-contract smoke passed.
- These results belong to the exact audited commit. They are hosted CI results, not proof of installation or operation on the owner's physical devices.

## Product and release limitations

1. **Ordinary Android conversation is not connected to the host request runtime.** The current conversation UI persists the user message to Room and explicitly says sending to a processing host is not available. A separate request runtime exists, but ordinary chat does not yet receive a real model response.
2. **Physical Android-to-Windows inference acceptance has not run on this commit.** The LAN workflow requires a self-hosted Windows runner labelled `whitep-lan` and a physical Android device. A separate Windows local-inference test cannot substitute for this path.
3. **Live model inference on the target Windows host is not established by the current-head push workflows.** The Product Build proves that the host builds and its smoke test starts. The `real-inference.yml` workflow is separate and must run against the intended source/model before a live-model claim.
4. **Model catalogue lifecycle is partial.** Catalogue validation, bounded HTTPS download, SHA-256/GGUF verification and update of an existing model are implemented. Selecting a catalogue item stores a preference; it does not install or activate a model. Removing the active model disconnects it but leaves its file on disk. The catalogue does not bundle model weights.
5. **Rendezvous is metadata-only.** The service stores peer identity, direct addresses, capability tags and lease expiry in process memory; it is not an inference relay. Container build/smoke is not proof of public HTTPS reachability, external-provider log retention or a real peer-to-peer session.
6. **First-release request policy is one active host at a time, with up to three sequential attempts.** It is not the old three-host-per-round behavior. A one-shot correlated response is in scope; streaming is deferred.
7. **Windows installer signing:** the installer is currently unsigned for Authenticode. Publisher text is descriptive metadata, not a Windows-trusted signature.
8. **Android production release signing:** the current Product Build's disposable test keystore validates signing configuration only. Production distribution requires the protected `android-release` environment and pinned certificate to be provisioned and the signed-release workflow to pass for the exact release commit.
9. **Release bundle:** the release-assets workflow must run for the same tagged source commit as the successful production-signed Android artifact and Windows product build. A historic draft or checksum artifact from another commit is not a current release.
10. **Upgrade preservation is unverified.** No target-device in-place upgrade was performed on this snapshot. No before/after snapshot proves preservation of Android Room IDs/order, Android Keystore/trust state, Windows settings, model path/checksum, host identity, protected secrets, peer trust or opt-in state.

## Acceptance gates still required

- Run physical Android-to-Windows acceptance on the intended LAN and installed applications, with a loaded local model whose revision and SHA-256 are recorded.
- Connect ordinary Android chat to the request runtime and demonstrate a real host response shown and persisted in the same Room conversation.
- Complete trust, replay, tampering, disconnect, busy, timeout, cancel and retry acceptance on the real product path.
- Complete independent clean install and in-place upgrade on both platforms, including redacted before/after data and identity snapshots.
- Produce the current-source production-signed Android package, Windows distribution bundle, SHA-256 manifest and provenance; resolve Windows Authenticode signing policy.
- Re-run the complete `docs/acceptance.md` matrix and retain redacted evidence for every required gate.

Until these gates are closed, this repository snapshot is a CI-built development candidate, not a fully accepted release.

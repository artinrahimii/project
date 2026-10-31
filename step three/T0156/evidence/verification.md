# T0156 verification evidence

- Base commit: `0287bb8181dbf123b72d52f49abf4e5bb6acd7ca`, branch `main`.
- Graph source: `step three/project-graph.md`, Git blob `eed15f437409830127933956b7a255837b60270f`; original bytes: 2763712 bytes; decoded text: 2641463 characters. The Git object id was recomputed from the Git blob header and all original bytes and matched the tree entry.
- Dependency contract: `T0155 → T0156` is validated as `contract_or_interface_dependency` because the task consumes the accepted summarizer request/result and size-limit interface.
- Local history contract: `T0150 → T0156` is validated as `contract_or_interface_dependency` because this task reads the persisted `MessageDao.observeForConversation` stream covered by the existing conversation workflow acceptance test. The original `blocks` edges remain in place.
- Added automated coverage: `ConversationContextCapsuleViewModelTest.suggestsCapsuleFromOnlyThisConversationsLocalHistoryAndRequiresSave` and `tests/test_android_context_capsule_history_update_contract.py`.
- This file records implementation scope only; it does not assert that tests have already run. Record actual GitHub Actions results after the implementation commit.

## Failed first exact-head attempt and corrective retest

- Exact tested implementation head: `2dfca52b9c53cdb5343c2f3f2f6262567d5f816d` (V 3.0.570).
- 249 Python tests, Python compilation and structural graph validation passed. The repository-backed evidence guard failed due to a false positive: `"todo"` is a supported field label parsed by the completed T0155 implementation, not a TODO placeholder.
- Android ktlint failed due to one import-order issue and two extra blank lines; APK/JVM stages were not run. HostLifecycle passed; Windows WPF status was still pending at the time of update.
- Corrective commit includes a regression test for quoted labels; actual new workflow results will be recorded after they complete. T0156 remains in progress until all required gates pass.

## V 3.0.571 corrective outcome and A003

- Exact tested commit: `d7bea35a156598676f83f8f5263253a6eafcc5e8`.
- Step Three quality passed 250 Python tests, compilation, graph structure and repository-backed evidence checks.
- Android lint failed on one needless blank line at `ConversationContextCapsuleViewModelTest.kt:112`; APK and JVM stages were not run. HostLifecycle passed; Windows WPF was still running when reviewed.
- A003 removes the final reported blank line. T0156 remains in progress pending the new exact-head workflow.

## V 3.0.572 exact-head outcome and A004

- Step Three quality passed 250 Python tests, compileall, graph structure and recorded-output/evidence checks.
- Android lint and debug APK build passed, but Android JVM suite completed 52 tests with one failure in the new local-history/manual-save test. Generated Protobuf and APK upload were skipped.
- Windows WPF build and all configured startup/security checks passed; HostLifecycle passed.
- A004 reads an existing persisted capsule directly when the ViewModel StateFlow has not yet observed it. This is a corrective attempt, not an acceptance claim.

## V 3.0.573 result and A005

- Step Three quality passed 250 Python tests, compilation and graph/repository-backed evidence validation.
- Android ktlint rejected the diagnostic assertion formatting before any APK/JVM tests ran; HostLifecycle passed and Windows WPF was still running.
- A005 applies the formatting correction. T0156 remains in progress pending exact-head test results.


## Exact-head V 3.0.574 acceptance evidence

- Tested commit: `bdfd3fb0128b3dde351be0e5b6a69aaaacbdd36e` (`V 3.0.574`) on `main`.
- Graph provenance: `step three/project-graph.md`, blob `1005df8792e7b9eff2a1a99e6ce81c7380f4ae57`; exact blob content was retrieved, parsed and SHA-1 matched. Original size: 2,785,258 UTF-8 bytes and 2,662,983 decoded characters.
- Step Three quality run [40916608649](https://github.com/artinrahimii/project/actions/runs/40916608649): pass; 250 Python tests, compileall, graph and repository-backed evidence checks; 229 tasks / 417 edges, zero errors and one preserved T0001 warning.
- Product run [40916608612](https://github.com/artinrahimii/project/actions/runs/40916608612): pass; Android lint, APK build, JVM tests, Protobuf verification and APK upload; Windows WPF build/startup/security/DPAPI checks and HostLifecycle tests.
- The selected-conversation/manual-save acceptance is implemented in `ConversationContextCapsuleViewModelTest.suggestsCapsuleFromOnlyThisConversationsLocalHistoryAndRequiresSave`. The exact-head Android JVM test task completed successfully. Transport remains unimplemented; no claim about an unbuilt network payload is made.
- The existing `blocks` edges remain. Next task T0157 requires review of its two incoming edges before implementation.

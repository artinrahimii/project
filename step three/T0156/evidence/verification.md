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

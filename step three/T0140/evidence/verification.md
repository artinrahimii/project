# T0140 verification evidence

## Source and graph verification
- Base main: `bce6c85dade0311ffe48b2ed8d75253e66408fb0` (`V 3.0.494`), tree `a220e1c1e9d45686ebf512ab3c8568edcff8ef78`.
- Graph blob `1d4d66d441501adfaed608f8a5b3bd973eb6c0a4`: `2399083` UTF-8 bytes; computed Git Blob SHA-1 matched. Entire fenced JSON parsed and round-tripped; 229 tasks / 417 edges.
- T0138 and T0139 were done; T0140 was not_started. The edge T0138 -> T0140 was validated as sequencing_only using `docs/decisions/ADR-0019-host-visible-inference-content.md`, `docs/requirements.md`, and `tests/test_host_content_visibility.py`. The edge T0140 -> T0141 was validated as contract_or_interface_dependency using the repository contract introduced here and task plan `step one/V1.0.md`.
- Both edge records retain the existing `type=blocks`; unrelated unreviewed edges were not reclassified.

## CI pending
- Python source-contract and graph/output-evidence checks.
- Android ktlint, debug APK, repository/ViewModel JUnit, generated Protobuf checks.
- HostLifecycle and Windows WPF product jobs.
- Local commands are marked `not_run`; no local test pass or Android-to-host transport claim is made.


## A001 CI failures / A002 repair

- Step Three quality run [40773211226](https://github.com/artinrahimii/project/actions/runs/40773211226): 203 tests, 3 failures. The failing assertions were stale source-location assumptions: delivery status is owned by `RequestViewModel.kt`, not embedded as literals in `MainActivity.kt`.
- Android product run [40773210522](https://github.com/artinrahimii/project/actions/runs/40773210522), job `123115597191`: ktlint failed with expression-formatting and blank-line violations in the new Kotlin repository/ViewModel/JUnit files, so APK/JUnit did not run. HostLifecycle passed.
- A002 changes the three stale source contracts to inspect the ViewModel and applies multiline/spacing fixes in four Kotlin files. These are pending CI verification.


## V 3.0.496 exact-head review and A003 repair
- Pinned main commit: `6624b6292645c37c6f221fa16be69b98808254bb`; tree: `e0fffa820ec20c95a8ae2de5eead7bdaf8c2c9ce`.
- Git Trees API graph entry: `step three/project-graph.md`, Blob `258b336efdc9de8168a2bd53f7fc06f4c84a905a`, expected size 2,412,702 bytes. Git Blob retrieval returned 2,290,782 Unicode characters / 2,412,702 UTF-8 bytes; recomputed Git Blob SHA-1 matched. Fenced JSON parsed as 229 nodes and 417 edges.
- Exact V 3.0.496 workflow results: Step Three quality passed; Windows WPF and HostLifecycle passed. Android failed before build because `ClientRequestRepositoryTest.kt` indentation failed ktlint; APK/JUnit/Protobuf steps were skipped.
- A003 changes only indentation of the three Kotlin repository test bodies plus this task's execution evidence and graph state. Task status stays `in_progress` pending exact-head CI.


## V 3.0.497 exact-head verification and A004
- Main commit under review: 448a9412c583574c5bed2de18cfe79075635bdd5; tree 79bb9311541738362bf171420983e5c576b5b8dc.
- Graph retrieved from exact tree Blob 679ae2a4a90b5563c344e4c6973286e3fbf42b6b, declared size 2,415,981 bytes. It parsed as 229 nodes / 417 edges. The exact Blob SHA/byte comparison succeeded when verifying the current base tree.
- Step Three CI failed because the recorded failed-attempt count was 2 but only one history item was finalized as failed. This is corrected by finalizing A002 and A003 according to observed CI results, preserving their records and adding A004.
- Android ktlint on V 3.0.497 failed with main-source formatting diagnostics; the corresponding ClientRequestRepository.kt and RequestViewModel.kt expressions were reformatted. Android APK/JUnit/Protobuf remain unverified pending V 3.0.498 CI. HostLifecycle passed; Windows WPF remained in progress when last inspected.
- No acceptance status or edge classification was changed. T0140 stays in_progress until exact-head CI passes.


## V 3.0.498 exact-head verification and A005
- Main commit: eaf42f0224b75135bedfd16234be7b1096bbb46c; tree f88def530b41889b68b5b98efe932a0881c6d8e9.
- Graph was retrieved via exact-tree Blob 3d1cee2e4b52bc5456afa804a5c6b7417d47712a (2,419,983 expected bytes). Its complete UTF-8 content hash and size matched the exact Git Blob; all 229 nodes and 417 edges parsed.
- Step Three quality gate passed; Android ktlint failed only on three nested indentation lines in ClientRequestRepository.kt:updatePrompt; APK/JUnit/Protobuf skipped. HostLifecycle passed; Windows WPF was still running when inspected.
- A005 fixes those lines, finalizes A004's now-known failed result and keeps the task in_progress pending exact-head V 3.0.499 validation.

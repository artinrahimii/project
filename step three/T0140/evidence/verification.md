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

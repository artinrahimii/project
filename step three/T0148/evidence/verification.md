# T0148 evidence ledger

- **Accepted implementation:** [`V 3.0.539`](https://github.com/artinrahimii/project/commit/2b0c8a8231e3a258021322e6be2f24f6c168fce8).
- **Quality CI:** [run 40872603020](https://github.com/artinrahimii/project/actions/runs/40872603020) passed 229 Python tests, syntax, graph structure and repository-backed evidence checks (229 tasks/417 edges; zero errors; one existing T0001 metadata warning preserved).
- **Android product job:** [run 40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021) passed ktlint, APK build, JVM unit tests including `draftIsRestoredAfterViewModelRecreation`, generated Protobuf verification, and APK artifact upload (11,875,221 bytes).
- **Windows/host jobs:** [product run 40872603021](https://github.com/artinrahimii/project/actions/runs/40872603021) passed Windows WPF formatting/build/Protobuf/startup/security checks and artifact upload (14,254,981 bytes), plus HostLifecycle unit/integration tests.
- **Dependency validation:** T0147 → T0148 = `contract_or_interface_dependency`; T0138 → T0148 = `sequencing_only`. Reasons/evidence recorded in `edge_change_log`; original `blocks` edges retained.
- **Attempt history:** A001 and A002 failures remain preserved; A003 is the formatting correction; A004 is the successful exact-head verification.
- **Acceptance boundary:** ViewModel/SavedStateHandle restores an unsent draft from Android saved state. This does not replace Room persistence across normal app close/reopen (T0149), and does not prove P2P delivery or live request transitions.

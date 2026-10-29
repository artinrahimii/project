# T0144 evidence ledger

- Implementation: [`V 3.0.517`](https://github.com/artinrahimii/project/commit/12b76f6f617c982b1353e8e6ec7cb7f5370bb6b3).
- Initial exact-head CI: Python quality test failed due to stale single-line source-contract expectation; Android ktlint failed on formatting/import ordering. Links:
  - https://github.com/artinrahimii/project/actions/runs/40849730503/job/123365907777
  - https://github.com/artinrahimii/project/actions/runs/40849730591/job/123365908350
- Host lifecycle integration job passed on V 3.0.517: https://github.com/artinrahimii/project/actions/runs/40849730591/job/123365908429 (not Android evidence).
- Attempt A002 corrects the observed source-contract and ktlint failures. Its build/test result remains pending until CI completes on the corresponding commit.
- Do not mark T0144 complete unless the exact-head quality gate, Android lint/build, and Android JVM tests pass.

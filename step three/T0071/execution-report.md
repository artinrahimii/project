# T0071 execution report

- Status: **in progress; implementation is committed, acceptance gate requires a metadata correction and a clean rerun**.
- Starting commit: `175dd6ff5541459a8b0c5c60004781c0adf3e9b2` on `main`; graph blob at start: `a0bcf8d0cf78625de04b791b0fd2e05b0b584c79` (1,146,321 bytes; parsed JSON: 229 tasks, 426 edges).
- Dependency review:
  - `T0070 -> T0071`: retained `type=blocks`; validated as a contract/interface dependency. The view reads real `ModelCatalogEntry` records parsed by the bounded local-file loader; no static model rows are introduced.
  - `T0067 -> T0071`: retained `type=blocks`; validated as a contract/interface dependency. Backend IDs shown in the details come from entries already validated against the host's supported-backend registry.
- Implementation:
  - Added a selected-model details panel in the Windows WPF host.
  - Selecting a loaded catalog entry shows model name/ID, format, declared size in bytes/MiB/GiB, source URL, license status, license identifier, license terms URL, supported backend IDs, checksum and limitations.
  - Unverified/review-required license metadata explicitly warns not to assume permission to use, modify or redistribute the model. Even a catalog's `verified` status is not represented as independent legal verification.
  - No network fetch, model download, installation or fabricated catalog entries were added.
- CI on implementation commit `175dd6ff5541459a8b0c5c60004781c0adf3e9b2`:
  - Python suite: **146 tests passed**, including the new UI regression tests.
  - Graph guard without repository evidence validation: 0 errors, one pre-existing T0001 warning.
  - Graph guard with `--repo-root`: failed because three GitHub Actions URLs had been placed in T0070's local-file `evidence` field. This was a report-format error, not a failed application test. The URLs remain in test records and this report; the file-path-only evidence field is being corrected in the follow-up.
  - Windows product build was still running when this report was updated; do not claim it passed until confirmed.
- Next: re-run the Step Three gate and inspect the Windows product build after the metadata correction; only then mark T0071 done.

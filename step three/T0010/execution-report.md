# T0010 — execution report

## Attempt T0010-A001 — clean-checkout build/test reproducibility

- Base commit: `1f3fded138a12802a7dd5a631c38fdc149835a91` (`V 3.0.47`).
- Result: **succeeded for the defined build/test-and-artifact scope**.
- Dependency review: inspected `T0009 → T0010`. The edge remains present with `type: blocks` and `dependency_classification: sequencing_only`; it is an execution-order relation, not a technical prerequisite. Updated evidence for affected outgoing T0010 edges is recorded in the graph change log because earlier references to a missing-project snapshot had become stale.
- Implementation: the README now lists the exact Windows build and host-lifecycle commands, Android `assembleDebug` command, Python quality commands, and required toolchain baselines. The product workflow uploads the Windows WPF build output and debug APK with seven-day retention; `if-no-files-found: error` prevents missing outputs from being silently accepted. `tests/test_reproducible_build_contract.py` enforces consistency between documentation, workflow commands, tool versions and artifact paths.
- Exact-commit quality evidence: [Step Three quality gate run 39677788361](${qualityUrl}) passed for V 3.0.47. The full suite ran 45 tests and ended with `OK`; Python syntax, graph structure, and recorded-output/evidence checks also passed.
- Clean-checkout product build evidence: [White-P product build run 39677788376](${productUrl}):
  - Windows WPF build step passed; Windows artifact upload passed.
  - Host lifecycle unit/integration test job completed successfully.
  - Android `assembleDebug` step passed; Android APK upload passed.
  - Both uploaded artifacts are visible in the run: [Windows host artifact](${winArtifactUrl}) (88,918 bytes) and [Android debug APK](${apkArtifactUrl}) (10,577,636 bytes), each retained until 2026-10-28.
  - **Qualification:** a subsequent check confirmed that this same workflow completed successfully at 2026-10-21T21:20:46Z, with all three jobs succeeding. The successful Windows and Android build outputs were uploaded as artifacts.
- Scope limit: repeatable builds and artifacts do not prove runtime launch on target hardware, local model inference, direct P2P request/response, encryption, or absence of persistent conversation history.
- Next: T0011 is already recorded as done. Review the dependency edge and current source evidence before starting the next uncompleted task; keep all runtime gates NOT RUN until their real tests exist.

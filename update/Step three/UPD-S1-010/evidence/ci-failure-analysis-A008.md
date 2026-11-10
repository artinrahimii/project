# Android test-source KtLint correction — UPD-S1-010 A008

## Exact baseline and CI evidence
- Repository: `artinrahimii/project`
- Branch: `main`
- Baseline: `8c540bdf774692ece0ba6f24e1f826fe738e011c` (`V 3.1.171`)
- Android product workflow run: <https://github.com/artinrahimii/project/actions/runs/42865142546>
- Quality gate run: <https://github.com/artinrahimii/project/actions/runs/42865138755>

## A007 results
- Python quality gate passed on the exact baseline.
- HostLifecycle acceptance passed.
- Rendezvous container build/smoke passed.
- Android lint failed in the new `ConversationWorkflowAcceptanceTest.kt`: import ordering at line 3 and chain-method-continuation at lines 94 and 101. Android APK build and JVM test steps were skipped.
- Windows WPF build/publish/startup smoke had passed; installer acceptance was still running when this evidence was captured.

## Corrective changes
- Reordered the newly added test imports according to KtLint's lexicographic grouping.
- Replaced two multiline chained DAO assertions with named intermediate query results and compact Flow collection expressions, removing the exact chain-continuation violations without changing test behavior.
- No runtime source, persistence schema, transport contract, or product UI behavior was changed.

## Validation state
- A008 exact-head Android lint, APK build and JVM test workflow: pending.
- No physical Android device or user-owned Windows host was used. Physical Android-to-Windows inference and in-place upgrade were not performed.

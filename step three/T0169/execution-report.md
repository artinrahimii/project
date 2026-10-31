# T0169 — execution report

## Task
If the completed round response window contains no valid response, select the next batch of up to three different compatible hosts.

## Dependency review
- `T0168 → T0169`: retained `type=blocks`; validated as `contract_or_interface_dependency`. The response collector returns its result only after all queued attempts settle against their shared round window. A non-empty validated response list suppresses another selection; an empty list allows the next batch.
- `T0160 → T0169`: retained `type=blocks`; validated as `contract_or_interface_dependency`. The selector continues to consume the canonical `InferenceRequest` payload contract and preserve model, protocol, prompt and capsule compatibility filtering.
- Both rationales and concrete evidence paths are recorded in `edge_change_log`. Conservative execution order is unchanged. The outgoing `T0169 → T0170` edge remains `unreviewed`.

## Implementation
- Extended `HostCandidateSelector.select` with an optional set of excluded ephemeral host IDs.
- Added `selectNextRound`, which only selects a new batch after a completed response collection has no valid responses.
- The next selection excludes every host attempted in the previous round, including rejected, failed and timed-out submissions.
- A valid response suppresses another round. Empty, blank or duplicate previous-round host identity data fails closed.
- Existing deterministic ordering, compatibility checks, duplicate-candidate handling and the three-host per-round cap remain in force.

## Automated acceptance coverage
- Added JVM regressions for selecting the next three hosts after a no-response round, suppressing retry when a valid response exists, and failing closed when previous-round identities are missing or ambiguous.
- Extended the repository Python source-contract test to require the production API and the new JVM regressions.

## Verification and status
- Source and test changes are staged in this commit on `main`.
- Exact-head GitHub Actions quality, Android KtLint/APK/JVM/Protobuf, Windows WPF and HostLifecycle results are pending. No test is reported as passed before those workflows complete.
- A local Gradle build was not run because this execution environment cannot resolve `github.com` for a repository clone; exact-head GitHub Actions is the available build/test path.
- Status: `in_progress` pending exact-head verification.

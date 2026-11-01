# T0125 execution report — three distinct peer identities

- **Task:** guarantee a selected batch contains no more than three hosts and no repeated ephemeral PeerId; reject invalid/duplicate host IDs before any request is submitted.
- **Prerequisites:** T0124 is `done` on V 3.0.765; T0115 is `done`. The incoming `T0124 → T0125` edge was reviewed and validated as a `contract_or_interface_dependency`: T0124 returns `List<HostCandidate>`, and T0125 consumes that candidate contract to preserve unique ephemeral identities. The `T0115 → T0125` edge remains validated `sequencing_only`.
- **Existing production code reviewed:**
  - `HostCandidateSelector.select` groups by `announcement.ephemeralHostId`, discards ambiguous duplicate-identity groups, sorts deterministically and caps selection at `MAX_HOSTS_PER_ROUND = 3`.
  - `HostCandidateSelector.selectReachable` applies the same uniqueness boundary to live-probed announcements before selecting at most three ready/capable peers.
  - `InferenceAttemptDispatcher.dispatchRound` validates each candidate's announcement/capability/status PeerId equality, rejects duplicate host IDs with `DUPLICATE_HOST_IDENTITY`, and rejects batches above three before submitting any payload.
- **Existing regression evidence:** `HostCandidateSelectorTest.selectsAtMostThreeDistinctCompatibleReadyHostsInStableOrder`, `excludesAmbiguousDuplicatePeerIdentities`, `selectReachableRejectsDuplicatePeerIdsBeforeProbingAndHonorsExclusions`, `selectNextThreeDistinctHostsWhenThePreviousRoundHasNoValidResponse`, and `InferenceAttemptDispatcherTest.rejectsDuplicateHostIdentityBeforeSubmitting` cover the deterministic selection, duplicate filtering, across-round exclusion and no-send dispatch guard.
- **New regression in A001:** `selectReachableSelectsThreeDistinctHostsInStablePeerIdOrder` supplies four valid announcements in unsorted order, exercises the live-selector status-probe seam and asserts three unique peers are probed and returned in stable order.
- **Scope boundary:** the tests validate candidate identity and dispatch invariants once validated `HostAnnouncement` snapshots are supplied. They do not claim that the current Rendezvous HTTP endpoint publishes full model/capability records or that the request UI is already wired to this candidate source.

## T0125-A001 — focused live selection regression

- **Implementation:** existing production uniqueness and dispatch guards were reused; a dedicated three-peer live-selector test was added rather than duplicating production logic.
- **Changed files:** `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`, this report and `step three/project-graph.md`.
- **Validation:** exact-head KtLint, debug APK, Android JVM tests and Step Three recorded-output gate are pending. T0125 remains `in_progress`.

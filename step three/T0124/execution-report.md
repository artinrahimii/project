# T0124 execution report — live reachability-aware host selection

- **Task:** implement host selection based on peer reachability, runtime readiness, remaining concurrency and model compatibility.
- **Base:** exact online `main` commit `c84f3d1e84985353d4301fdfb759d9784f769bd6` (`V 3.0.761`).
- **Graph validation:** the graph was read from its exact tree Blob `a5519e25a316276f2fbf50af6375a1fac7726178`. The complete UTF-8 source was 3,651,115 bytes; recomputed Git Blob SHA-1 matched the tree Blob ID; fenced JSON parsed into 230 unique task nodes and 421 edges with no dangling references.
- **Prerequisites:** `T0123` is `done`, with exact V 3.0.759 evidence for authenticated live HostStatus requests, freshness/identity validation, Android JVM round-trip and negative uint32 active-count rejection. `T0115` is `done` with accepted listener lifecycle tests for disconnect recovery, invalid frames, sequential clients and shutdown. No prerequisite edge was changed.
- **Incoming edges:** `T0123 → T0124` remains a validated `contract_or_interface_dependency` because the selector consumes fresh HostStatus snapshots; `T0115 → T0124` remains validated `sequencing_only` because T0124 does not consume the legacy TCP frame reader/listener API. Both original `blocks` edge labels are preserved.
- **Existing implementation reused:** `T0162` already completed deterministic selection over supplied HostAnnouncement/HostStatus snapshots, including model and capacity checks. That implementation does not perform a network probe and explicitly does not claim reachability. T0124 therefore adds the missing live-probe stage rather than duplicating or replacing T0162.
- **Implementation location:** the graph lists generic Shared.Networking/WPF networking paths, but the actual consumer is the Android app: its existing HostCandidateSelector and AndroidInferenceRequestRuntime use the Android libp2p transport and explicit peer-trust store. The code is therefore added at the actual Android application boundary rather than moved to the WPF host.

## Changes in A001

- `HostCandidateSelector.selectReachable` now examines announcements in stable PeerId order, removes ambiguous duplicate identities, respects excluded IDs, and avoids network probes for announcements that cannot satisfy the request's static protocol/model/prompt/capsule/concurrency requirements.
- It obtains a fresh `HostStatus` through an injected suspend probe. Probe failures (including untrusted or unreachable peers) fail closed; coroutine cancellation is rethrown. Only a matching peer ID, positive observation timestamp, READY state, available concurrency and runtime model/output compatibility can be selected.
- `AndroidInferenceRequestRuntime.selectReachableHosts` binds the selector to the real `Libp2pInferenceRequestTransport.probeHostStatus`. That transport enforces explicit trust, exact authenticated PeerId, an unexpired directory lease, direct route constraints and a bounded fresh timestamp. Cached status is never used as proof of reachability.
- Android JVM regression tests cover unreachable probes, non-READY status, exhausted capacity, model mismatch in live status, stable selection order, duplicate identities, explicit exclusions and cancellation propagation.
- Probing is sequential because the current transport permits only one pending HostStatus probe per peer; stable order makes candidate selection deterministic.

## Validation

- Source-level diff review completed against the exact V 3.0.761 tree; changes are restricted to the selector, Android runtime composition root, selector JVM tests, this report and the graph.
- Exact-head KtLint, APK build, JVM tests and product regression workflows are pending. No acceptance test is yet claimed as passed for A001.
- **Task state:** `in_progress` until those exact-head workflows pass.


## T0124-A001 — exact V 3.0.762 quality-gate result

- **Commit:** `910ef405225cede0292303d369a02dfd776023cc`.
- **Failed gate:** [Step Three quality gate 41305388502](https://github.com/artinrahimii/project/actions/runs/41305388502) ran 312 Python tests. One existing source-contract assertion failed in `tests/test_android_host_candidate_selector_contract.py:33` because the refactor renamed the local `capabilities` reference to `maximumConcurrentRequests`, so the exact expected source expression `status.activeRequests >= capabilities.maxConcurrentRequests` was not present. The capacity decision remained semantically equivalent; this is a source-contract compatibility failure, not evidence of a runtime capacity defect.
- **Product workflow at follow-up time:** Android KtLint and HostLifecycle/Windows jobs were still running; no outcome is inferred for those unfinished jobs here.
- **Correction in A002:** restore the established `capabilities` local and exact expression without changing the condition's meaning.
- **Task state:** T0124 remains `in_progress`; A001 is retained as failed.

## T0124-A002 — preserve the established source contract

- **Base:** exact online `main` commit `910ef405225cede0292303d369a02dfd776023cc` (`V 3.0.762`).
- **Change:** use `candidate.announcement.capabilities` as the local `capabilities` and preserve `status.activeRequests >= capabilities.maxConcurrentRequests`. The lower-bound check for negative active counts and all live-probe logic remain unchanged.
- **Validation:** pending exact-head Step Three and product workflows.


## T0124-A002 — exact V 3.0.763 CI result

- **Commit:** `2ad34f147a0517736f62893a52b0083f45ebb469`.
- **Passed:** [Step Three quality gate 41305547585](https://github.com/artinrahimii/project/actions/runs/41305547585), [Rendezvous container smoke 41305547544](https://github.com/artinrahimii/project/actions/runs/41305547544), HostLifecycle and Android .NET peer interoperability fixture within [product workflow 41305547591](https://github.com/artinrahimii/project/actions/runs/41305547591).
- **Failed:** Android KtLint main source set reported three dot/newline violations at `HostCandidateSelector.kt:251` in the prompt/capsule size conversions. APK, Android JVM and generated Protobuf checks were skipped. The Windows WPF job had passed build/startup and early policy checks but was still in its final checks when the result was recorded.
- **Correction in A003:** use intermediate values for UTF-8 prompt bytes and capsule bytes, matching the style already used in the same file. No behavior is changed.
- **Task state:** T0124 remains `in_progress`; A002 is retained as failed.

## T0124-A003 — Kotlin size-chain formatting correction

- **Base:** exact online `main` commit `2ad34f147a0517736f62893a52b0083f45ebb469` (`V 3.0.763`).
- **Change:** replaced the two KtLint-rejected chained size conversions with explicit prompt byte-array and capsule size intermediates. The comparisons and accepted byte limits remain unchanged.
- **Validation:** exact-head CI is pending; no APK/JVM pass is claimed for A003 yet.

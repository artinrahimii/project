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

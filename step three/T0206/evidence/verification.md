# T0206 verification evidence

## Incoming edge validation

1. `T0205 → T0206` — `sequencing_only`, validated. The cancellation acceptance and metadata-directory privacy checks are distinct paths.
2. `T0195 → T0206` — `sequencing_only`, validated. No T0195-specific output/API is consumed by the rendezvous contract checks.
Original `blocks` edge values and conservative sequence remain unchanged.

## Privacy boundary under test

- The shared `PeerAnnouncement` model contains exactly four public data properties: peer ID, direct addresses, capability names and expiry.
- The rendezvous Android client issues a `GET /v1/peers?capability=inference.v1`; the test now asserts GET, `doOutput == false`, and the exact capability-only query.
- The C# test examines the actual directory response JSON and asserts its property set is exactly `addresses`, `capabilities`, `expiresAtUtc`, and `peerId`.
- The C# control-plane test submits unknown `prompt`, `contextCapsule`, and `capsule` announcement properties. Each must receive HTTP 400; a subsequent directory query must remain empty.

These assertions verify both client request shape and server acceptance/storage behavior. They do not claim packet capture or observation of traffic outside the test HTTP boundary.

## Execution state

Implementation and edge validation are recorded in attempt `T0206-A001`. The HostLifecycle suite, Android JVM suite and Quality Gate must pass on the exact committed head before T0206 can be marked done.

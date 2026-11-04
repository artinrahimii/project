# T0206 execution report

- Status: **in_progress**
- Attempt: `T0206-A001`
- Base commit: `a3e50e03f105e54ad01a0d97d04b9436ebcb6dbd`
- CI verification: pending

## Incoming edge review

- `T0205 → T0206`: **validated / sequencing_only**. T0205 verifies Windows host-side cancellation. T0206 verifies the separate rendezvous privacy boundary; no T0205-specific API/output is consumed.
- `T0195 → T0206`: **validated / sequencing_only**. T0195 concerns installer/download and host request recovery contracts; it produces no rendezvous metadata privacy interface consumed here.
Original `blocks` edges and conservative order are retained.

## Implemented test changes

- C# `PeerAnnouncementClientAcceptanceTests` checks the discovery response has exactly the metadata fields `peerId`, `addresses`, `capabilities`, and `expiresAtUtc`, with no prompt/capsule fields.
- C# control-plane acceptance now attempts to include `prompt`, `contextCapsule`, and `capsule` in an announcement. The service must reject each unknown field with HTTP 400 and must not store any rejected entry.
- Android `RendezvousPeerDirectoryClientTest` asserts directory lookup uses a bodyless `GET`, with the exact query `capability=inference.v1`, and no prompt/capsule fields in the URL.

The service's shared `PeerAnnouncement` contract and JSON options are already metadata-only and fail closed on unmapped fields; the changes add executable evidence at the wire boundaries.

## Verification

The code and edge review are committed only when the exact source Blob has been re-read and verified. Runtime CI is pending; T0206 remains `in_progress`.

# T0134 verification evidence

## Exact revision and graph integrity

- Base head reviewed: `c49edacbc89498ce35a3ba906b463ce305c5136a` (`V 3.0.458`).
- Implementation/test commit: `234b79b9248f38a037febba07c55a265b2e0fc31` (`V 3.0.459`).
- Reviewed graph Blob at base: `4e7891ec268b973e7aaba74c270043427ed7c599`, 2,259,671 bytes. Retrieved via exact commit tree and Blob API; Git SHA-1 matched and fenced JSON parsed fully (229 nodes, 422 active edges).
- Quality gate passed for the implementation commit: https://github.com/artinrahimii/project/actions/runs/40717445979
- HostLifecycle job passed for the implementation commit: https://github.com/artinrahimii/project/actions/runs/40717445528/job/122918865305

## Observed negative-test output

The HostLifecycle job log includes:
- `PASS: shared peer identity policy rejects malformed input and delegates PeerId parsing to libp2p.`
- `PASS: peer trust is explicit, persisted, canonical-PeerId-bound and fail-closed for unknown or corrupt identities.`
- `PASS: host identity store serializes concurrent creation, preserves PeerId on reload and rejects corrupt envelopes, unsupported identity versions and malformed private keys.`

The runner's `Program.cs` also has test assertions that missing/malformed and unsupported application protocol versions are rejected. This is **application protocol** validation, not a direct negative integration test of Noise/TLS secure-channel version negotiation.

## Graph edges

- `T0133 → T0134` is now `contract_or_interface_dependency`, `validated`.
- `T0128 → T0134` is `sequencing_only`, `validated`, and removed from active edges. Reason and evidence remain append-only in `edge_change_log`.
- `T0128` remains `not_started`; no two-machine/independent-network test was run or claimed.

## Outstanding acceptance gap

A direct failing-handshake test for unsupported libp2p Noise/TLS secure-channel negotiation or invalid secure-channel certificate has not yet been run. No custom crypto implementation was added. T0134 remains `in_progress` pending that evidence.

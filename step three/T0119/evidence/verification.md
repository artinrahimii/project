# T0119 verification evidence — pending exact-head quality gate

## Baseline integrity
- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `V 3.0.358`, `fd86ea667ff5e13dc230d982f76b2b2a55a26993`.
- Graph Blob `1c95ae7879e578edf1df37c325df215ea9d50954`: 1,841,895 original UTF-8 bytes and 1,720,180 returned text characters. Git Blob SHA-1 matched; full fenced JSON parsed/reconstructed exactly (229 tasks, 426 edges).

## Decision and source-contract guard
- `docs/decisions/ADR-0004-encrypted-relay-boundary.md`: first-release no-relay decision, future end-to-end opaque-byte path, and explicit metadata exposure.
- `tests/test_relay_privacy_boundary.py`: guards that relay remains disabled in the host transport, exhausted direct candidates fail explicitly, and any future relay needs end-to-end encryption and documented metadata exposure.
- `docs/decisions/README.md`: registers the new ADR and corrects the ADR-0002 evidence statement to distinguish host-side TCP/QUIC integration from still-unimplemented Android-to-Windows app-level routing.
- `T0119 → T0120`: validated as `contract_or_interface_dependency`; peer announcements must consume the metadata-only privacy boundary.

## Scope boundary
This task attempt provides architecture/contract design only, not a working relay stream. The upstream relay README notes a stream-bridging gap. No relay runtime acceptance is claimed. Direct TCP/QUIC exhaustion remains an explicit connection failure.

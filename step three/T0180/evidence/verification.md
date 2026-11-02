# T0180-A001 — evidence and exact-base review

- Repository/branch: `artinrahimii/project` / `main`
- Base commit: `1c329bd92f403ee380962dc7a237a3fce19bf84e` (`V 3.0.821`)
- Base tree: `c09c7f2eb063f18fb4407d59d980e4358850eb79`
- Graph Blob at base: `801a1ec718d82436e90f9e6cabaa1e389659a6a3`; exact base-tree entry confirmed and full graph parsed as 230 tasks / 422 edges.
- Source and test files were read at the exact base revision; each fetched Blob SHA matched its recursive-tree entry.
- T0177 and T0179 were `done`; their reports, evidence and relevant implementation interfaces were checked.
- `T0177 → T0180` and `T0179 → T0180` are validated `sequencing_only`; original `blocks` types and execution order remain unchanged. Evidence/reasons are appended to graph.edge_change_log.
- `T0180 → T0181` remains unreviewed.
- CI for the new exact head is pending; no test pass is claimed yet.

Changed code implements zeroization for Windows frames/queue/lifecycle buffers and Android decoder/controller buffers. Android JVM tests inspect retained backing arrays; Python source-contract regression tests cover ownership paths. Limits of the zeroization guarantee are documented in `docs/security.md`.

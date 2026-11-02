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


## T0180-A002 — exact-base review and validation limits

- Base commit: `b14a8c311cd98422c5b15ddb9089d24f3170edc8` (`V 3.0.822`); target branch: `main`.
- The graph's ordinary file-fetch response was empty (0 characters) despite returning Blob `12e8b52f163c3b221df2c731dbb2c911c6257097`. The separate Blob fetch returned 3,825,072 characters / 3,951,827 UTF-8 bytes. A locally implemented Git SHA-1 over `blob 3951827\\0` plus the complete original fenced Markdown bytes produced the same Blob SHA `12e8b52f163c3b221df2c731dbb2c911c6257097`.
- Full JSON parsing succeeded after removing the Markdown fence only in a separate parsing copy. Parsed graph: 230 nodes, 422 edges, 230 execution-order entries, no duplicate task IDs and no unresolved edge references.
- The connected GitHub actions do not expose a read-tree operation. The exact tree SHA used as the base for reconstructing a single safe multi-file commit is retained in the prior A001 record: `c09c7f2eb063f18fb4407d59d980e4358850eb79` (commit `V 3.0.821`). The resulting commit will be compared against the current `main` parent before advancing the ref; any unexpected file difference aborts the ref update.
- Source review confirmed the redundant blank line in the Android JVM test; it was removed. No automated test was executed in this environment. The local clone attempt failed because DNS could not resolve `github.com`; .NET SDK, Gradle and Android SDK are unavailable.
- The combined commit status for the base SHA returned no status checks. No PR-triggered workflow run was returned. This is not treated as a test pass or failure.
- T0180 remains `in_progress`; both validated incoming edges remain `sequencing_only`, with original `type=blocks` and order preserved. The outgoing edge to T0181 remains `unreviewed`.

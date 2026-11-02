# T0176 evidence

## Exact revision / graph validation

- Repository/branch: `artinrahimii/project`, `main`.
- Accepted test revision: `V 3.0.808`, `062794153557c686538150b2f4af0abfccba9778`.
- Base graph at this revision: `39f36bd0690f560de504109f6e9eb4b52f15a1a8`; 3,892,548 UTF-8 bytes.
- Git blob SHA-1 over `blob 3892548\0<original UTF-8 bytes>` exactly matched the tree blob ID.
- Full fenced JSON parsed and round-tripped; graph contained 230 nodes and 422 edges.

## Dependency validation

- `T0175 → T0176`: `contract_or_interface_dependency`, `validated`, type `blocks` retained.
- `T0160 → T0176`: `contract_or_interface_dependency`, `validated`, type `blocks` retained.
- Reasons and source/test evidence are recorded in `edge_change_log`.

## Actual acceptance coverage

- Zero responses: three queued attempts yield three timeout failures.
- One response: one response and two timeout failures.
- Two responses: concurrent collector regression verifies both responses and their dispatch order.
- Three responses: explicit collector regression verifies three responses in dispatch order.
- Duplicate response: a second correlated response for the same request/attempt is rejected, preserving the first card.
- Late completion and late arrival: separate correlation validator regressions reject both cases.
- Timeout: deterministic exception-based timeout in the collector test; existing shared-round deadline behavior remains covered.
- Cancellation: existing ViewModel cancellation test verifies all selected queued attempts are cancelled and that a cancelled request is not retryable.

## Exact-head CI — V 3.0.808

- [Product build](https://github.com/artinrahimii/project/actions/runs/41431021173): success. Android KtLint, debug APK build, full Android JVM test task, Protobuf generated source verification, Windows WPF and HostLifecycle jobs passed.
- Android unit report artifact: `13133179900`, 49,921 bytes.
- Android APK artifact: `13134591087`, 19,524,950 bytes.
- Windows host artifact: `13133473445`, 14,266,183 bytes.
- [Step Three quality](https://github.com/artinrahimii/project/actions/runs/41431021155): 324 Python tests passed, `compileall` passed, graph structure and recorded-output/evidence validation passed. Graph guard reported 230 nodes / 422 edges / zero errors and retained one existing T0001 metadata warning.
- [Rendezvous container](https://github.com/artinrahimii/project/actions/runs/41431021244): production container build and non-root HTTP contract smoke passed.

## Failed attempt history retained

- A001 (`V 3.0.807`) failed Android KtLint and path-only graph evidence validation. It is preserved as a failed attempt in the graph; its 324 Python tests, compileall, structural graph check, Windows job and HostLifecycle results remain recorded.
- A002 (`V 3.0.808`) corrected those exact failures and passed all required exact-head CI checks.

# T0176 evidence

## Exact revision and graph integrity

- Repository/branch: `artinrahimii/project`, `main`.
- A001 commit: `201cf977f09c6c0f01ec0ea284eed3d25b92ab39` (`V 3.0.807`).
- Graph path: `step three/project-graph.md`.
- A001 graph blob: `b88883e230c2d4b5f87cd9f3d459ab7d2765a59a`.
- Received graph content: 3,887,936 UTF-8 bytes; SHA-1 over `blob 3887936\0<original bytes>` matched the exact blob ID.
- Full fenced JSON parsed and round-trip serialization preserved the JSON payload; 230 nodes and 422 edges were present.

## Reviewed incoming edges

- `T0175 → T0176`: `contract_or_interface_dependency`, `validated`; original `blocks` retained.
- `T0160 → T0176`: `contract_or_interface_dependency`, `validated`; original `blocks` retained.
- Each review has a rationale and source/test evidence in `edge_change_log`.

## Coverage added or made explicit

- Zero responses: three queued attempts produce explicit timeout failures.
- One response: one response plus two per-attempt timeout failures.
- Two responses: existing concurrent collector test asserts two correlated responses in dispatch order.
- Three responses: explicit all-success collector test asserts three results in dispatch order.
- Duplicate response: ViewModel refuses a second response for the same request/attempt and retains the first card.
- Late responses: validator tests separately reject late completion and late arrival.
- Timeout determinism: removes a 20 ms wall-clock race in favour of explicit `TimeoutException`.
- Cancellation: existing ViewModel cancellation regression is retained.
- A002 further verifies exactly three cards and the expected successful/failed card counts for each cardinality.

## A001 observed CI result — V 3.0.807

- [Step Three quality run](https://github.com/artinrahimii/project/actions/runs/41430423260): 324 Python tests passed, compileall passed and graph structure validation passed; repository-backed evidence validation failed because six T0175 Actions URLs had been placed in a path-only evidence list.
- [Android product run](https://github.com/artinrahimii/project/actions/runs/41430423224): KtLint failed on the new Kotlin formatting; APK/JVM were skipped.
- [Windows job](https://github.com/artinrahimii/project/actions/runs/41430423224/job/125227305983): passed.
- [HostLifecycle job](https://github.com/artinrahimii/project/actions/runs/41430423224/job/125227306211): passed.

The six Actions URLs remain preserved in T0175 execution/test records; only the path-only array is corrected in A002.

## A002 status

Awaiting exact-head `V 3.0.808` GitHub Actions. No passing result is claimed for A002 yet.

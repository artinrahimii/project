# T0109 verification evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Implementation commit tested by CI: `b82ce66b585c93057730225826896097845ee74f` (`V 3.0.335`); tree `b8621c58dfa85efb9dda0faeefdeea1b7d3568c2`.
- Graph path: `step three/project-graph.md`; tree-derived Blob ID `004608bd3f0633ae4b1dd66ecad0a9b36f1d8ace`.
- The exact Git Blob API content included its Markdown fence and measured **1,715,325 UTF-8 bytes** and **1,593,593 Unicode characters**. Recomputed Git Blob SHA-1 exactly matched the Blob ID. The complete fenced JSON parsed and contains 229 tasks, 426 edges, and 22 sections.

## Dependency edge validation

- `T0108 -> T0109`: type `blocks` preserved; classification `hard_dependency`; validation `validated`. Evidence names the listener implementation/API, listener acceptance test, and T0108 report/evidence.
- `T0107 -> T0109`: type `blocks` preserved; classification `sequencing_only`; validation `validated`. T0107's coordinator fault tests expose no task-specific API/data consumed by listener lifecycle handling.
- `T0109 -> T0110` remains `unreviewed`; no premature change was made.

## Failed attempts preserved; third attempt passed

- **Attempt 01:** lifecycle acceptance and quality gate passed; Windows smoke failed with exit code `-532462766`. Log: https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685895265.
- **Attempt 02:** quality gate failed three source-contract assertions for old direct `Shutdown(0/1/3)` literals, and Windows smoke remained failed. Quality gate: https://github.com/artinrahimii/project/actions/runs/40356065948. Product build: https://github.com/artinrahimii/project/actions/runs/40356065945.
- **Attempt 03:** passed after final `Close()` was dispatched after the original Closing event unwound, and contract tests were updated.

## Exact-head verification

All items below correspond to implementation commit `b82ce66b585c93057730225826896097845ee74f`.

- Step Three graph/Python quality gate: **PASS** — https://github.com/artinrahimii/project/actions/runs/40357312350.
- HostLifecycle tests, including `ParticipationNetworkListenerLifecycleTests`: **PASS** — https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693129382.
- Windows Release build, Protobuf generated-source validation, startup smoke, Windows tool policy checks and artifacts: **PASS** — https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693130018.
- Android lint, debug APK, Protobuf tests, generated-source checks and APK artifact: **PASS** — https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693129952.
- Full Windows/Android product workflow: **PASS** — https://github.com/artinrahimii/project/actions/runs/40357312422.
- No local .NET execution is claimed; these results came from the cited CI jobs.

## Scope boundary

The implemented listener lifecycle is loopback-only. Accepted connections still close before payload bytes are read. T0110 must add a bounded Protobuf parser before request processing; P2P request/response transfer and encryption remain unimplemented/unverified.

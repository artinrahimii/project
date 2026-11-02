# T0179 verification evidence

## Graph and dependency integrity

- Initial graph Blob at V3.0.815: `8a230baabd5cad87156b0bd94e4118f28eb2eb7d`; 3,920,856 UTF-8 bytes and 3,794,175 characters; Git Blob SHA-1 was independently recomputed and matched.
- Graph contains 230 unique tasks and 422 edges.
- `T0177 → T0179` and `T0178 → T0179` retain `blocks`, validated as `sequencing_only`; no edge removed and original ordering preserved.

## A001 — V 3.0.816
- Commit `583dccc5decebccb78d6599c082e12e516bb80c9`.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41446302498): success; 333 Python tests, compileall, graph guard (0 errors, one existing T0001 warning).
- [White-P product build](https://github.com/artinrahimii/project/actions/runs/41446302777): Windows and HostLifecycle passed; Android ktlint failed at SafeDiagnosticLog.kt lines 35 and 43; APK/JVM skipped.
- HostLifecycle log confirmed the EventSource emitted only the three allowlisted numeric values.
- [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41446302729) and [Real local inference](https://github.com/artinrahimii/project/actions/runs/41446302931) passed.

## A002 — V 3.0.817
- Commit `4bfb38ab6238b3c4b8168352715704241c739b82`.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41447155706): 333 Python tests and graph guard passed.
- [White-P product build](https://github.com/artinrahimii/project/actions/runs/41447156073): HostLifecycle passed; Android ktlint failed at enum constructor formatting in SafeDiagnosticLog.kt lines 7 and 12; APK/JVM skipped.
- [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41447155821): passed.

## A003 — V 3.0.818
- Commit `4b686da26a6d61e73adc8daa13d1d0ac742f9e11`.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41447625728): 333 Python tests and graph guard passed.
- [White-P product build](https://github.com/artinrahimii/project/actions/runs/41447625945): Windows WPF and HostLifecycle passed; Android ktlint then failed at Libp2pInferenceRequestTransport.kt:132:9 (blank line before local declaration). APK/JVM skipped.
- [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41447625716): passed.

## A004 — V 3.0.819
- Commit `998c84ec83e3b8706b20e664f7d8570e6cac14b5`.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41448213572): 333 Python tests, compileall and graph guard passed (230 tasks/422 edges, zero errors, one existing T0001 warning).
- [White-P product build](https://github.com/artinrahimii/project/actions/runs/41448213571): Windows WPF and HostLifecycle passed; Android ktlint passed but compileDebugKotlin failed with unresolved PascalCase enum names in Libp2pInferenceRequestTransport.kt:129, 130, 136, 137, 256, 257, 263 and 264. APK/JVM skipped.
- [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41448213662): passed.

## A005 correction
- Android transport now uses the declared Kotlin enum entries `INFERENCE_SUBMISSION`, `STARTED`, `REJECTED`, `SUCCEEDED` and `CANCELLED`.
- The Python source-contract regression test now expects those actual enum identifiers.
- The record schema and runtime behavior are unchanged. Exact-head A005 CI has not run yet.

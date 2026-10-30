# T0161 verification evidence

## Baseline and graph integrity

- **Branch/commit:** \`main\` at \`V 3.0.599\`, \`8e6f4f1ccd37283526eecccd29059eb63c5e54e2\`.
- **Graph path:** \`step three/project-graph.md\`.
- **Graph source:** Git Blob API using the Blob SHA obtained from the exact \`V 3.0.599\` commit tree: \`180f1f8bbd54e635df48ff248d07482df47b1bdd\`.
- **Graph completeness:** 2,909,937 UTF-8 bytes (matching the tree entry), 2,787,359 JavaScript string code units, complete JSON parse after removing the actual Markdown fence, and Git SHA-1 Blob recomputation matched the tree Blob ID.
- **Graph structure:** 229 task nodes and 417 edges; no truncation was reported by the recursive tree response.
- **T0160 prerequisite:** Status \`done\`; T0160 implementation/test paths exist and the V 3.0.599 product workflow and quality gate both passed.

## Contract review

- \`InferenceRequestPayloadBuilder.build(...)\` accepts \`requestId\` and \`deadlineUnixMs\`.
- \`protocol/whitep/v1/messages.proto\` defines the corresponding \`request_id\` and \`deadline_unix_ms\` fields.
- T0161 models the same request-scoped ID and deadline, together with maxRounds and lifecycle status.
- The edge is therefore validated as \`contract_or_interface_dependency\`; it is not classified as a hard compile-time dependency.
- T0161 → T0162 remains unreviewed until that next task is selected.

## New implementation

- Production: \`src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt\`.
- Tests: \`src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt\`.
- JVM tests use JUnit only; the model has no Android framework dependency.
- CI evidence for this implementation is pending on the implementation commit. No test is marked passed before execution.


## A001 CI failure and A002 correction

- Exact-head `V 3.0.600` Android KtLint failed only on import order in `DistributedRequestTest.kt:3:1`: the `java.util.UUID` import was before `org.junit`. No compiler or JVM result was obtained because subsequent Android workflow steps were skipped.
- The production `DistributedRequest.kt` passed the KtLint source check in that run.
- A002 changes only import order in the unit test; exact-head `V 3.0.601` CI must validate it.

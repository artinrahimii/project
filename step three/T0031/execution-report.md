# T0031 — Execution report

- **Task:** Define the versioned Protobuf `InferenceRequest` message.
- **Starting main HEAD:** `eb529f48a65c595a201e14070d2dd689b3dc51ae` (`V 3.0.86`).
- **Implementation:** Added optional `bytes context_capsule = 7` to `protocol/whitep/v1/messages.proto`. Existing field numbers 1–6 are unchanged. Added a regression test for request/attempt IDs, prompt, deadline, output-token limit and capsule.
- **Attempt 1 / commit `ceb839afb8c95f116ce274653300c1d237e4c527`:** A source inspection found the test's regex patterns were over-escaped. Exact-commit CI runs were still in progress when the defect was caught; this attempt is not accepted as passing.
- **Attempt 2:** Corrected the regular-expression escapes in `tests/test_proto_contract.py`. The corrected commit's quality and product-build workflows must be checked before task completion.
- **Scope limit:** This defines the wire field only. It does not implement runtime capsule-size enforcement or P2P transport.
- **CI observation on `75e68b719f26165f552bfc06e2e4d6bbe7b3e478` (`V 3.0.88`):** 73 Python tests passed and Python syntax validation passed. The quality gate still failed because the graph summary counted one failed attempt while neither history entry had `result: failed`; the separate product-build workflow passed. This is a graph bookkeeping defect, not evidence that the product build or the complete quality gate passed.
- **Correction in this commit:** Mark attempt `T0031-A001` failed because its acceptance test was over-escaped, and record the exact CI evidence for attempt `T0031-A002`. T0031 remains in progress until the quality gate on the corrected graph commit passes.
- **State:** In progress; 2 attempts recorded, 1 failed attempt, graph-quality correction committed for revalidation.
- **Next action:** Inspect the new commit's quality and product-build workflows. Only mark T0031 done if the graph gate and acceptance evidence pass.

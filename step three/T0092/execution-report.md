# T0092 — execution report

## Attempt T0092-2026-10-25-01 — trace inference failures to the UI error path

- Base commit reviewed: `7621bfd45a7808934c681e23f2e225200ded9281` (`V 3.0.287`).
- Dependency review:
  - `T0091 -> T0092`: validated as `contract_or_interface_dependency`. T0092 uses the same `GenerateButton_Click` exception path whose `_clearSessionRequested` guard is required to prevent a late failure from repopulating a cleared conversation. The edge relation `blocks` remains unchanged.
  - `T0087 -> T0092`: validated as `sequencing_only`. Catalog license classification is independent of the inference-result/error-presenter contract; preserve the existing conservative order without asserting a technical prerequisite.
- Source inspection: `GenerateButton_Click` appends an Assistant-role message only after `await generationTask` returns. Cancellation appends a System-role cancellation message; other failures append a System-role message through `HostErrorPresentation.ToUserMessage`. The presenter maps exceptions to stable actionable messages without displaying raw exception details.
- Existing executable backend tests already assert that malformed JSON, transport disconnection and request timeout raise typed exceptions with stable, non-sensitive messages; the backend does not return these failures as generated text.
- Implementation added in this attempt: `tests/test_wpf_entrypoint.py::test_inference_failures_are_system_messages_not_assistant_responses`. It scopes assertions to `GenerateButton_Click` and checks that the Assistant success append follows the awaited result while cancellation/generic exception paths append System messages and never Assistant responses.
- Validation: base commit `V 3.0.287` quality and product workflows passed ([quality](https://github.com/artinrahimii/project/actions/runs/40231676597), [product](https://github.com/artinrahimii/project/actions/runs/40231676645)); those runs do not contain the new test. New-test result: **not yet run**.
- Outcome: `in_progress`; no false claim of test acceptance. No production source was changed because the inspected implementation already had the safe error/success separation; this attempt adds missing regression coverage.
- Next: run the complete quality gate and product build on the commit containing this test. Only mark T0092 done if both succeed and the new regression test passes.

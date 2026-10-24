# T0061 execution report

## Exact baseline and graph integrity

- Repository/branch: `artinrahimii/project` / `main`.
- Baseline commit: `9b164aab3fae39752a733732a3cb781dc223981f` (`V 3.0.186`); the implementation changes in this report are committed to the online `main` branch.
- Graph: `step three/project-graph.md`, obtained from the Git Blob API using the Blob SHA from the recursive tree of the exact baseline commit.
- Integrity checks passed before editing: the tree entry named `step three/project-graph.md` resolved to Blob `e5efddbf6bc7eac73892132f09bdd938ab7145c4`; UTF-8 byte length was 1160412, matching the tree's recorded size; SHA-1 of Git's exact `blob <size>\\0<content>` object preimage matched the Blob SHA; the outer Markdown JSON fence and complete JSON parsed successfully.
- Parsed graph counts: 229 unique task IDs and 426 edges; there were no unresolved edge references. Before this attempt, 59 tasks were `done`, one was `in_progress`, and 169 were `not_started`.

## Dependency review for T0061

- `T0060 → T0061`: reclassified from `unreviewed` to `contract_or_interface_dependency`. The backend's `GenerateAsync` checks its health when not already available, and `HostInferenceSession` gates readiness on backend health. Evidence is recorded on the edge; the original `type=blocks` is unchanged.
- `T0056 → T0061`: reclassified from `unreviewed` to `sequencing_only`. The host session consumes the capacity/readiness result before allowing inference, but the HTTP adapter itself does not use the capacity evaluator as an interface. `type=blocks` is retained as conservative order.
- `T0061 → T0062` and other unrelated edges remain untouched and unreviewed.

## Attempt T0061-A001 — implementation and exact-commit verification

- Fixed `src/Host.Infrastructure/LlamaCppServerBackend.cs`: under web-default JSON naming, the token limit was serialized as `maxTokens`; the request property is now explicitly serialized as the OpenAI-compatible `max_tokens`.
- Strengthened `tests/HostLifecycle.Tests/Program.cs` to validate the exact serialized request sent through a recording HTTP handler: POST to `v1/chat/completions`, JSON content type, configured model ID, one user message, exact prompt, `max_tokens=64`, and absence of the non-standard `maxTokens` field. The existing assertion still checks response mapping from `choices[0].message.content`.
- Exact code commit: [`9b164aab3fae39752a733732a3cb781dc223981f` — `V 3.0.186`](https://github.com/artinrahimii/project/commit/9b164aab3fae39752a733732a3cb781dc223981f).
- [Step Three quality gate, run 40073802629](https://github.com/artinrahimii/project/actions/runs/40073802629) passed on the exact code commit.
- In [product workflow run 40073802621](https://github.com/artinrahimii/project/actions/runs/40073802621), the host lifecycle test job passed and the Windows formatting, Release build, generated Protobuf validation and WPF startup smoke job passed.
- At the time of this report, the overall product workflow had not yet completed its Android job; no Android success is claimed.
- No live `llama-server.exe` plus real GGUF model was launched. The deterministic HTTP handler proves serialization and response-field mapping, but it does not prove a real model can generate a response. Therefore `T0061` remains `in_progress`, and the task is not counted as complete.

## Current status and next action

- Task status: `in_progress`; attempt result: `partial`. Two incoming edges were reviewed and recorded in `edge_change_log`; the outgoing `T0061 → T0062` edge remains unreviewed.
- Next: run the opt-in real-local-model integration path using a compatible local server and license-acceptable GGUF model, record the actual generated response, and only then revisit completion of T0061.


## Attempt T0061-A002 — executable opt-in live-model verification path

- Baseline commit: `00f4edb0fef5b4aebbd563403f7f09f13d5c8c3f` (`V 3.0.187`); target branch remains `main`.
- Added an opt-in path to `tests/HostLifecycle.Tests/Program.cs`. When explicitly enabled, the harness starts the real `llama-server.exe` through the existing shell-free process launcher, waits up to two minutes for the local health endpoint, sends an inference request to the configured model, and requires a non-empty response. It rejects the known recording-handler fixture string as evidence of live inference.
- The opt-in path requires `WHITEP_RUN_REAL_INFERENCE_TEST=1`, `WHITEP_LLAMA_SERVER_EXE`, `WHITEP_LLAMA_MODEL_PATH`, `WHITEP_LLAMA_SERVER_URL`, and `WHITEP_LLAMA_MODEL_ID`. It defaults the prompt to a short request; no model is downloaded and no external provider is contacted by this test path.
- This attempt adds an executable verification path but does **not** claim that a live model was run in this environment. The normal CI run skips this opt-in section unless its explicit environment flag is configured. T0061 therefore remains `in_progress` until a real server process and license-acceptable GGUF model produce a response under this test.
- Baseline CI at `V 3.0.187`: Step Three quality gate run 40074032000 and product build run 40074031978 both report success for the baseline commit. They do not test the new change; results for the new commit must be read from its own workflow runs.

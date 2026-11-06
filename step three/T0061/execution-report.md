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


## Attempt T0061-A002 — CI failure found after commit

- The opt-in test path was added in commit `941e2535357c4aaefde5ece4c1572fe24ca893e4` (`V 3.0.188`).
- Exact-commit workflow run [40076984165](https://github.com/artinrahimii/project/actions/runs/40076984165) exposed a compile failure in the host lifecycle test project: `CS0246`, `LocalInferenceProcessLauncher` was not found because its namespace was not imported. The real-model path was not executed.
- This is recorded as a failed attempt, not a passing test. The follow-up adds the missing namespace import; the fix remains unverified until the next exact-commit workflow run completes.


## Verification follow-up — T0061-A003

- Verification commit: `3d579d9185f3aa7a34a41d6f500a136841b7f88d` (`V 3.0.189`).
- Exact-commit [White-P product build run 40077091311](https://github.com/artinrahimii/project/actions/runs/40077091311) completed successfully. The host lifecycle harness, Windows WPF Release build/startup smoke, and Android debug APK/tests all passed.
- Exact-commit [Step Three quality gate run 40077091319](https://github.com/artinrahimii/project/actions/runs/40077091319) passed automated tests, Python compilation, graph structure, and recorded-output/evidence validation.
- This verifies the namespace-import repair and normal product checks. It does not perform real-model inference because the live test configuration is opt-in; T0061 remains `in_progress`.

## Attempt T0061-A004 — pinned live-model CI verification path

- Started: `2026-10-24T08:19:02.145Z`.
- Added `.github/workflows/real-inference.yml`, a dedicated Windows job that runs the existing opt-in acceptance path against a real local process rather than the recording-handler fixture.
- The job pins the official llama.cpp b11443 Windows CPU archive and verifies its published SHA-256 before use. It pins the GGUF model file to revision `a6b7826ed969c3926a98359e39912af137b464cb` from `QuantFactory/SmolLM2-135M-Instruct-GGUF`; the model page labels the repository Apache-2.0. The workflow validates the expected file-size range and GGUF magic, logs its SHA-256, starts the server on loopback, and requires a non-empty generated response.
- Sources: https://github.com/ggml-org/llama.cpp/releases/tag/b11443 and https://huggingface.co/QuantFactory/SmolLM2-135M-Instruct-GGUF
- No live response is claimed at commit creation. The new workflow must finish successfully and its step log must show the live-inference PASS line before T0061 can be marked done. No cloud inference provider is used.


## Attempt T0061-A004 — verified live-model result

- Verification commit: [`7f25b649c7684194b6ec33340e9e4e41a67e3375` — `V 3.0.190`](https://github.com/artinrahimii/project/commit/7f25b649c7684194b6ec33340e9e4e41a67e3375).
- Real inference workflow [run 40077575618](https://github.com/artinrahimii/project/actions/runs/40077575618) completed with `success`. Its Windows job `120824173177` passed each step: downloading and validating the pinned llama.cpp binary, downloading and validating the pinned GGUF, and the live local-model acceptance test.
- The official llama.cpp b11443 Windows CPU archive SHA-256 matched: `98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6`.
- Model file: `SmolLM2-135M-Instruct.Q4_K_M.gguf`, pinned repository revision `a6b7826ed969c3926a98359e39912af137b464cb`; observed size `105454144` bytes and SHA-256 `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684`. The workflow verified the GGUF magic and constrained the endpoint to loopback.
- Actual job log: `PASS: real local llama.cpp process started, health check passed, and the configured GGUF model returned a non-empty generated response.` This is live local inference from the production backend, not the recording-handler fixture.
- Exact-commit [Step Three quality gate run 40077575900](https://github.com/artinrahimii/project/actions/runs/40077575900) passed graph structure, syntax, automated tests, and evidence/output checks.
- Exact-commit [White-P product build run 40077575695](https://github.com/artinrahimii/project/actions/runs/40077575695) passed Windows WPF Release build/startup smoke, host lifecycle tests, Android debug APK, Kotlin lint, Protobuf round-trip tests, and generated-source validation.
- Result: T0061 meets its graph criterion for routing a request through the local inference backend and receiving real model output, so its graph status is now `done`. The earlier failed compile attempt A002 remains in history. This evidence does not claim the complete Android-to-P2P product is finished.


## Graph-evidence validation correction — V 3.0.191

- The live inference acceptance result remains successful: real local llama.cpp process started, health check passed, and the pinned GGUF model returned a non-empty response in workflow [run 40077575618](https://github.com/artinrahimii/project/actions/runs/40077575618).
- The subsequent Step Three quality gate [run 40077809529](https://github.com/artinrahimii/project/actions/runs/40077809529) failed only at the recorded-output/evidence validation step. The failure was caused by remote URLs placed in the task's top-level `evidence` array; `graph_guard.py` treats values with slashes in that array as repository paths and requires the files to exist locally.
- The graph correction now stores only existing source/test/workflow/report files in `nodes[T0061].evidence`. The external run/release/model URLs remain in this report and the relevant execution history, where they serve as navigable links rather than repo-file evidence.
- This preserves the successful live-model history, does not erase the historical A002 compile failure, and avoids treating CI URLs themselves as local evidence files.
- Workflow run [White-P product build 40077809527](https://github.com/artinrahimii/project/actions/runs/40077809527) on V 3.0.191 showed the host lifecycle suite and Windows WPF Release build/startup smoke passed; Android lint/build/tests and artifact upload steps also passed before the final job result was rechecked.

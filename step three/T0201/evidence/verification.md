# T0201 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- Real local inference workflow run 41886745964 succeeded on exact commit a074815979aff371650c5101c514989f42180968.
- The workflow sets WHITEP_RUN_REAL_INFERENCE_TEST=1 and supplies the pinned llama.cpp executable, GGUF model, loopback URL and model identifier.
- PASS: real local llama.cpp inference succeeded; active generation cancellation was observed and the owned process stopped cleanly.

## Source/test paths

- `.github/workflows/real-inference.yml`
- `tests/HostLifecycle.Tests/Program.cs`
- `src/Host.Windows/Inference/LlamaCppServerBackend.cs`
- `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886745964
- https://github.com/artinrahimii/project/actions/runs/41886745964/job/126773133022

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.

# T0074 execution report

- Status: **done**; exact-head online CI passed.
- Implementation head validated: `b11d9fed82edf482b9fc5f452a339cd6fb7bb80d` (`V 3.0.228`) on `main`.
- Dependency review:
  - `T0073 -> T0074`: retained `type=blocks`, validated as `contract_or_interface_dependency`; the sample catalog conforms to the tested catalog-v1 metadata contract.
  - `T0067 -> T0074`: retained `type=blocks`, validated as `contract_or_interface_dependency`; the sample declares `llama.cpp` and the runtime acceptance test checks the actual host supported-backend registry.
- Actual output: `src/Host.Windows/Models/Catalog/sample-model-catalog.json`, a metadata-only entry for SmolLM2-135M-Instruct Q4_K_M.
  - Source: https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/resolve/main/SmolLM2-135M-Instruct-Q4_K_M.gguf
  - Upstream-reported size: `105454432` bytes.
  - Upstream-reported SHA-256: `2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d`.
  - Declared license: `Apache-2.0`; metadata URL: https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/blob/main/README.md.
- Runtime acceptance test: `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` loads the checked-in catalog using `ModelCatalogFileLoader`, the host's real backend registry, and explicit source/license host allowlists.
- Exact-head CI evidence on `b11d9fed82edf482b9fc5f452a339cd6fb7bb80d`:
  - [Step Three quality gate run 40123058807](https://github.com/artinrahimii/project/actions/runs/40123058807) — success.
  - [Host lifecycle tests, Windows WPF Release build, and Android APK build run 40123058861](https://github.com/artinrahimii/project/actions/runs/40123058861) — all jobs successful.
  - [Real local llama.cpp process and GGUF response run 40123058748](https://github.com/artinrahimii/project/actions/runs/40123058748) — success.
- Scope limitation: catalog metadata was validated; the model weights were not downloaded or bundled. The license declaration has not been independently legally verified.
- Next task: `T0075`, after reviewing its incoming dependency edges from `T0074` and `T0067`.

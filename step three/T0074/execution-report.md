# T0074 execution report

- Status: **in progress; exact-head CI is pending**.
- Starting commit: `3ee8fffe1624c17addfb9e8f07744d4c6a7e18d1` on `main`.
- Dependency review:
  - `T0073 -> T0074`: retained `type=blocks`, validated as `contract_or_interface_dependency` because the sample must conform to the tested catalog-v1 metadata contract.
  - `T0067 -> T0074`: retained `type=blocks`, validated as `contract_or_interface_dependency`; the sample declares `llama.cpp` and the runtime acceptance test validates it against the host's supported-backend registry.
- Added `src/Host.Windows/Models/Catalog/sample-model-catalog.json`, a metadata-only catalog containing one real GGUF model entry: SmolLM2-135M-Instruct Q4_K_M.
  - Source file: https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/resolve/main/SmolLM2-135M-Instruct-Q4_K_M.gguf
  - Size: `105454432` bytes.
  - SHA-256: `2e8040ceae7815abe0dcb3540b9995eaa1fa0d2ca9e797d0a635ae4433c68c2d`.
  - License identifier: `Apache-2.0`; license metadata URL: https://huggingface.co/bartowski/SmolLM2-135M-Instruct-GGUF/blob/main/README.md.
- Upstream cross-check: the quantized model card declares Apache-2.0 and identifies the GGUF file. The file's Hugging Face Xet pointer reports `105454432` bytes and the SHA-256 above; the upstream model card also declares Apache-2.0: https://huggingface.co/HuggingFaceTB/SmolLM2-135M-Instruct.
- Added a C# acceptance check to load the actual checked-in catalog via `ModelCatalogFileLoader`, using the host backend registry and an explicit `huggingface.co` source/license allowlist. The test compares its parsed size/checksum/backend/license fields with the reviewed metadata.
- Safety and product scope: this manifest does not include or download the 105 MB model weights. The source host must be explicitly trusted in the UI. The UI still says the license declaration has not been independently legally verified; this report is not a legal opinion. The upstream model card describes the model as primarily English and warns that generated content can be inaccurate.
- Validation in this execution: catalog JSON parse, schema/parser/factory source presence checks, and patch-shape checks passed. The executable C# test, graph guard and Windows WPF build have **not yet been verified on the new implementation commit**; T0074 remains `in_progress`.
- Follow-up: inspect exact-head Step Three quality, HostLifecycle acceptance and Windows build. Mark done only after those jobs pass.

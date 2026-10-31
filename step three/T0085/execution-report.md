# T0085 — Execution report

## Target and commits

- Repository: \`artinrahimii/project\`; branch: \`main\`.
- Base used for implementation: \`38521356b602b999b8dae365d8540358b06a6544\` (\`V 3.0.255\`).
- Implementation commit: \`e47f0e1db66e539f75a77b82ee0cc70ea8dc197d\` (\`V 3.0.256\`): https://github.com/artinrahimii/project/commit/e47f0e1db66e539f75a77b82ee0cc70ea8dc197d
- Compile correction commit: \`19f462500726f5f64b851745904a6128369fe5f1\` (\`V 3.0.257\`): https://github.com/artinrahimii/project/commit/19f462500726f5f64b851745904a6128369fe5f1

## Implemented behavior

- Added \`VerifiedModelInstaller.DownloadAndUpdateAsync\`. The new model is streamed into a unique staging file beside the installed file, then checked against the catalog's declared size, SHA-256 and GGUF magic/version. The current destination is checked for deletion/replacement during the download; only after validation does the implementation use same-volume \`File.Replace\` with a unique backup file.
- The original install method still refuses to overwrite an existing file. The update path separately requires an existing regular model file. Failed download/checksum/format checks and detected concurrent destination changes leave the existing destination untouched; the staged file is cleaned up.
- Added and registered acceptance cases for successful replacement, checksum failure retaining the prior model byte-for-byte, refusing an absent destination before networking, and refusing to overwrite a destination changed by another writer during download.
- Integrated this capability in the Windows catalog UI. The user must select a validated model, explicitly trust its source hostname and confirm the update. The UI disconnects the host, stops only an engine process owned by this app, reports progress from actual downloaded bytes, supports cancellation, and persists the new model path/ID only after success. A locked destination may still cause replacement to be refused; external engines need restarting to load the new file.

## Validation results

- Step Three graph quality gate **passed** on correction commit \`19f462500726f5f64b851745904a6128369fe5f1\`: https://github.com/artinrahimii/project/actions/runs/40172132207
- HostLifecycle acceptance suite **passed** on the corrected commit. The log shows the aggregate model installer acceptance line; the runner invokes the new update methods and assertions before printing it: https://github.com/artinrahimii/project/actions/runs/40172132118/jobs/121105014399
- Windows WPF Release build, C# format check, generated Protobuf source check and application startup smoke test **passed** on the corrected commit: https://github.com/artinrahimii/project/actions/runs/40172132118/jobs/121105014394
- Real local llama.cpp/GGUF inference **passed** on implementation commit \`e47f0e1db66e539f75a77b82ee0cc70ea8dc197d\`; the only subsequent code change was the missing namespace import: https://github.com/artinrahimii/project/actions/runs/40171951202
- The first implementation attempt's Windows build failure was recorded and corrected: the missing \`using WhiteP.Host.Windows.Models;\` is now present. The failed attempt remains in the execution history; \`failed_attempt_count\` is 1.
- The broader product-build workflow's Android APK job was still in progress in its final validation/cleanup phase when this report was written, with no failing step reported. Android was not modified and is not part of T0085's acceptance surface.

## Dependency review

- \`T0084 -> T0085\`: validated as a contract/interface dependency. Updating an active model needs the serialized disconnect lifecycle before file replacement.
- \`T0075 -> T0085\`: validated as a contract/interface dependency. The update consumes a \`ModelCatalogEntry\` from the validated offline-aware catalog and relies on its source URL, size, checksum and format metadata. Both edges retain their existing \`type=blocks\`; no edge was removed or relaxed.

## Outcome

**T0085 is complete** under its current graph acceptance rule: real update code is connected to the Windows UI, the original install semantics remain safe, the focused regression tests passed, and the corrected Windows host built and started successfully in CI. This does not claim that an external server hot-reloads the model or that every platform-wide CI job had completed at report time.

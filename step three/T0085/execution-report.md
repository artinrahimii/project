# T0085 — Execution report

## Implementation commit

- Base commit: \`38521356b602b999b8dae365d8540358b06a6544\` (\`V 3.0.255\`), branch \`main\`.
- Initial implementation commit: \`e47f0e1db66e539f75a77b82ee0cc70ea8dc197d\` (\`V 3.0.256\`): https://github.com/artinrahimii/project/commit/e47f0e1db66e539f75a77b82ee0cc70ea8dc197d
- Precondition review: T0084 and T0075 are both marked done with evidence. Reviewed \`T0075 -> T0085\` against the actual catalog-to-installer flow; the selected \`ModelCatalogEntry\` comes from the offline-aware validated catalog and supplies source URL, declared size, SHA-256 and GGUF metadata.

## Implemented behavior

- Added \`VerifiedModelInstaller.DownloadAndUpdateAsync\`. The new payload is streamed into a unique same-directory staging file, checked against declared size and SHA-256, and validated for GGUF magic/version before the target file is touched.
- Before promotion, the updater checks that the destination still exists and that its size/last-write timestamp have not changed during download. It then calls same-volume \`File.Replace\` with a unique backup path, so a failed transfer/checksum/format/target-change check leaves the installed model untouched. The old \`DownloadAndInstallAsync\` path continues to refuse overwriting an existing destination.
- Added tests for successful atomic replacement, checksum-failure preservation, missing target rejection before networking, and refusing to overwrite a destination changed by another writer during download.
- Connected the update API to the Windows catalog UI with explicit confirmation, a selected-entry requirement, exact trusted-host checking, streamed byte/speed progress, cancellation, disconnect of the host and stopping only the app-owned engine, and settings persistence after successful replacement. The UI explains that an external engine may need restarting and a locked file can cause replacement to be refused.

## CI results and correction

The first CI run for \`V 3.0.256\` was mixed:
- Step Three graph quality gate passed: https://github.com/artinrahimii/project/actions/runs/40171951129
- HostLifecycle suite passed, including the aggregate model installer acceptance tests: https://github.com/artinrahimii/project/actions/runs/40171951154/jobs/121104457271
- Real local llama.cpp/GGUF inference passed: https://github.com/artinrahimii/project/actions/runs/40171951202
- Windows WPF Release build **failed** because \`MainWindow.xaml.cs\` was missing \`using WhiteP.Host.Windows.Models;\`, leaving \`ModelStreamingDownloader\`, \`VerifiedModelInstaller\` and \`ModelDownloadProgress\` unresolved. Failure log: https://github.com/artinrahimii/project/actions/runs/40171951154/jobs/121104457161

The missing namespace import has now been added in the follow-up change. This report deliberately does not mark T0085 complete until the exact post-fix commit has a successful Windows Release build and the remaining checks have been observed.

## Remaining limitation

If an external process keeps the target file locked, atomic replacement can fail; the prior file is not removed beforehand. A successful file update does not hot-reload an external inference server; restart the engine/service before further inference.

## Next validation

Check the new versioned commit's Windows WPF Release build and startup smoke test, HostLifecycle unit/integration tests, Step Three quality gate, Android build/tests, and applicable real-inference evidence. Only then set task status to done.

# T0075 execution report

- Target branch: `main`; implementation based on `8a9226ca4a8013ce63f6b9cbb607eed2a7cea635` (`V 3.0.229`).
- Dependency review:
  - `T0074 -> T0075`: retained edge type `blocks`; reviewed as an implementation/data dependency because the offline store reuses the catalog loader and parser produced by T0074. Evidence: `ModelCatalogFileLoader.Load` performs strict UTF-8, size-limit, JSON, URL allowlist, backend and license validation before cache use.
  - `T0067 -> T0075`: retained edge type `blocks`; reviewed as a contract/interface dependency because the UI supplies `LocalInferenceBackendFactory.SupportedBackendIds` to the catalog loader. Evidence: current `MainWindow.LoadCatalogButton_Click` and `LocalInferenceBackendFactory` use that registry.
- Implementation: `src/Host.Windows/Models/Catalog/OfflineModelCatalogStore.cs` validates a selected catalog before atomically refreshing a local cache. If the selected file cannot be loaded, it revalidates the cached bytes with the current host allowlists/backend registry and reports whether fallback occurred. Invalid cache is never displayed as valid.
- UI: catalog status explicitly distinguishes a normal load, offline fallback, and failure when no valid cache is available.
- Acceptance tests: added checks for first successful load/cache creation, fallback after the selected file disappears, and rejection of invalid cache. CI evidence is pending until the new commit's workflows finish.
- Cache location: under the current user's LocalApplicationData/WhiteP/model-catalog-cache.json. The cache contains public catalog metadata only; no model weights or secrets.
- Limitations: no local test runner was available through this connector. Do not mark this task done until exact-head CI succeeds.

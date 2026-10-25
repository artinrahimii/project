# T0082 execution report

- Task: `T0082` — persist the selected catalog model ID in local settings.
- Target branch: `main`; implementation base: `e328d8880dc217736711ce7f08496704a55992f4` (`V 3.0.248`).
- Dependency review:
  - `T0081 -> T0082`: retained `type=blocks`; validated as `sequencing_only`. The selection preference does not call the installer or consume its installed-file result; the graph-required order remains.
  - `T0075 -> T0082`: retained `type=blocks`; validated as `contract_or_interface_dependency`. The UI obtains entries from `OfflineModelCatalogStore.Load` and persists the selected `ModelCatalogEntry.Id`.
  - `T0082 -> T0083` remains unreviewed; no downstream edge was changed.
- Implementation: added optional `ActiveCatalogModelId` to local settings and its JSON document. Existing schema-version-1 settings without the field remain readable. The WPF catalog restores a matching saved selection and persists user selection through the existing atomic settings store; connecting the backend preserves the selection. The UI clarifies that selecting a catalog entry does not download, install, or activate it in the inference backend.
- Tests added: persistent round-trip without overwriting the server `ModelId`; legacy settings compatibility; rejection of oversized/control-character IDs.
- Current status: `in_progress`; exact-head CI/build/test results are pending.

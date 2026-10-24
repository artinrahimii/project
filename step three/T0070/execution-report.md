# T0070 execution report

- Status: partial; exact-head CI pending.
- Scope: read metadata from a selected local catalog JSON file and bind UI rows to parsed entries; no static model rows, downloads, or installation.
- Dependency review:
  - `T0069 -> T0070`: retained `type=blocks`; validated as `contract_or_interface_dependency`. The parser's `ModelCatalog`/`ModelCatalogEntry` contract is consumed by the file loader and WPF list.
  - `T0067 -> T0070`: retained `type=blocks`; validated as `contract_or_interface_dependency`. The loader consumes a read-only supported-backend registry from the host factory rather than duplicating the backend identifier.
- Implementation:
  - Added a bounded local-file loader with strict UTF-8 decoding and parser validation for schema, URL hosts, backend compatibility and license state.
  - Added Browse/Load controls, explicit trusted source-host and optional license-host lists, and a ListView bound to `catalog.Models`.
  - No model entries are fabricated or bundled. The user selects a local JSON catalog and explicitly trusts hostnames. Loading does not fetch URLs or download/install weights.
  - Added C# runtime tests for file loading and oversized-file rejection, plus Python contract tests.
- Exact-head quality gate and product build are pending for this implementation commit. Do not mark T0070 complete until they pass.

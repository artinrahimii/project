# T0069 execution report

- Attempt: T0069-A001
- Result: partial; exact-head CI pending
- Scope: parse version-1 model metadata and reject malformed metadata, unknown backend IDs and unsafe/unapproved URLs.
- Dependency review:
  - `T0068 -> T0069`: retained `type=blocks`, classified as validated `contract_or_interface_dependency`. The parser directly follows the schema version, field shape and metadata constraints. Evidence: `model-catalog-v1.schema.json`, schema tests, and REQ-007.
  - `T0067 -> T0069`: retained `type=blocks`, classified as validated `sequencing_only`. The parser receives supported backend IDs from trusted caller configuration and does not instantiate the backend factory. Source evidence: `LocalInferenceBackendFactory.cs`, `IInferenceBackend.cs`, and `LlamaCppServerBackend.cs`.
- Implementation: added `ModelCatalogParser.cs` and immutable metadata records. The parser caps JSON input at 2 MiB and 10,000 models; rejects malformed/version-mismatched JSON, duplicate/unknown properties, duplicate model IDs, invalid IDs/names/sizes, non-SHA256 or malformed checksums, unsupported formats/backends, invalid host configuration, unsafe HTTPS URLs, unapproved source/license hosts, and invalid verified-license claims.
- Fail-closed URL policy: caller must supply non-empty trusted DNS host allowlists plus currently supported backend IDs. URLs must use HTTPS, an allowlisted DNS host, the default port, no credentials, no fragment, no control characters, and no IP-literal/loopback host. The parser performs no URL fetch or network request.
- Missing license: absent license metadata is normalized to `review_required`; this is a review state, not redistribution permission.
- Tests: added runtime C# acceptance tests to `HostLifecycle.Tests` and Python source-contract checks. Exact-head CI is pending.
- Scope limitation: no real model rows, downloader, installation, or catalog UI are added by this task.


## T0069-A002 — exact-head CI failure

- Step Three quality gate passed: [run 40106803595](https://github.com/artinrahimii/project/actions/runs/40106803595).
- Windows WPF host build and Android APK jobs passed, but the HostLifecycle.Tests job failed to compile because the duplicate-root-property JSON fixture had an unescaped quote in its C# string literal ([job log](https://github.com/artinrahimii/project/actions/runs/40106803542/job/120910070094)). The real-inference workflow also failed and is a separate unresolved issue.
- T0069 remains in progress. A passing parser acceptance suite is not claimed.

## T0069-A003 — compile-error correction

- Escaped the `models` property value correctly in the duplicate-root-property fixture so the test source compiles and can exercise duplicate-property rejection.
- Exact-head CI for this correction is pending; do not mark the task complete until the runtime suite passes.

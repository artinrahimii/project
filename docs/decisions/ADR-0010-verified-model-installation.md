# ADR-0010: Validated model metadata and integrity-checked installation

- **Status:** Accepted for the current implementation and executable acceptance suite.
- **Date:** 2026-10-27
- **Related task:** T0007 (technical-decision traceability).
- **Related requirements:** REQ-007 (model metadata/license transparency) and REQ-008 (integrity-checked model installation).
- **Scope:** Windows host model-catalog ingestion, streamed download, first installation, and model update.

## Context

Model metadata controls the model source URL, expected size, checksum, file format, backend compatibility and license disclosure. A download must not publish an incomplete, corrupted, malformed, or unapproved-source file as a usable model, and a failed update must not destroy the existing installation.

## Decision

1. **Parse catalog data strictly and within limits.** The v1 parser enforces a 2 MiB catalog limit, a maximum model-record count, stable identifiers, positive declared sizes, SHA-256 checksum format, supported format/backend IDs, unique IDs/properties, and validated license status. Source and license URLs must satisfy trusted application-provided HTTPS host allowlists; the catalog itself cannot supply or expand those allowlists.
2. **Stream rather than buffer model files.** `ModelStreamingDownloader` writes through fixed-size chunks and reports observed progress. It rejects mismatched declared `Content-Length`, oversized or truncated payloads, and unsafe initial, redirect, or effective URLs. Each redirect is revalidated and the redirect count is bounded. Partial output is removed on failure/cancellation.
3. **Verify before promotion.** `VerifiedModelInstaller` validates the expected SHA-256 metadata before network access, computes SHA-256 over the downloaded staging file, compares the digest to the catalog checksum using `CryptographicOperations.FixedTimeEquals`, and validates the supported GGUF header/version before publishing a first installation.
4. **Keep staging and destination on the same volume.** The installer stages in the destination directory. A new file is moved to its final path only after verification. Updates require an existing destination and atomically replace it only after the replacement passes checks; failed or cancelled updates preserve the previous file and remove staging/backup artifacts where safe. Concurrent destination changes fail closed.
5. **Fail closed on capacity and validation failures.** Insufficient/invalid free-space measurements, malformed checksum metadata, unsupported file formats, checksum mismatch, malformed GGUF header/version, HTTP failure and cancellation must not produce a falsely complete final model or replace a prior working model.
6. **Treat catalog metadata as a trust input, not signed provenance.** The implementation verifies the downloaded file against the SHA-256 value in parsed catalog metadata; this is an integrity check relative to that metadata. It is not a detached signature or independent proof of publisher identity. If both the catalog metadata and downloaded bytes can be maliciously replaced, this SHA-256 comparison alone cannot establish authenticity. Trusted catalog source/configuration and provenance remain separate responsibilities.

## Implementation and verification links

- Requirements: `docs/requirements.md` (REQ-007 and REQ-008).
- Catalog schema/parser and bounded file loader: `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json`, `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`, and `src/Host.Windows/Models/Catalog/ModelCatalogFileLoader.cs`.
- Offline cache boundary: `src/Host.Windows/Models/Catalog/OfflineModelCatalogStore.cs`.
- Streaming and allowlisted transport: `src/Host.Windows/Models/ModelStreamingDownloader.cs`.
- SHA-256, GGUF validation and staged/atomic activation: `src/Host.Windows/Models/VerifiedModelInstaller.cs`.
- Executable tests: `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs`, `tests/HostLifecycle.Tests/ModelStreamingDownloaderAcceptanceTests.cs`, `tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs`, `tests/HostLifecycle.Tests/IncompleteModelDownloadAcceptanceTests.cs`, `tests/HostLifecycle.Tests/ModelDownloadCancellationAcceptanceTests.cs`, and `tests/HostLifecycle.Tests/UpdateModelSafetyAcceptanceTests.cs`.
- Harness/project and schema checks: `tests/HostLifecycle.Tests/Program.cs`, `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`, `tests/test_model_catalog_schema.py`, and `tests/test_model_catalog_parser.py`.
- CI entrypoint: `.github/workflows/product-build.yml`.

## Evidence boundary

The acceptance suite uses controlled HTTP handlers and generated test bytes for deterministic failure/integrity cases. Passing those tests establishes the exercised contracts, not the independent trustworthiness of a real catalog publisher, a release-time checksum provenance chain, or the general safety/legality of every catalog entry. License status remains explicit metadata and must not be inferred from a successful checksum.

# ADR-0012: Validated, user-local settings with fail-closed persistence

- **Status:** Accepted for the current implementation and executable regression suite.
- **Date:** 2026-10-27
- **Related requirement:** REQ-014 in `docs/requirements.md` (bounds and input validation).
- **Scope:** Windows host endpoint/model preferences and local settings persistence.

## Context

The WPF host persists local inference endpoint settings, selected model identifiers, timeout values and optional local file paths. These values affect which endpoint the app connects to and which local artifacts it selects. Invalid or corrupt persisted settings must not silently change connection behavior or be overwritten as if they were valid.

## Decision

1. **Keep settings user-local and schema-versioned.** By default, settings live under the current user's LocalApplicationData directory at `WhiteP/settings.json`. The JSON document carries an explicit schema version; an unsupported/future version is rejected instead of guessed or silently migrated.
2. **Validate before using or saving.** The inference endpoint must be an absolute HTTP(S) loopback URL without user-info credentials, query or fragment. Model identifiers are bounded and reject control characters; request timeouts are constrained to 1–3600 seconds; optional paths are normalized to full paths and invalid values are rejected.
3. **Fail closed when existing settings are unreadable or invalid.** A missing settings file may use documented defaults. Invalid JSON, an unsupported schema or semantically invalid values are reported as an error. The WPF startup boundary contains these errors and displays a safe generic message instead of exposing exception details or proceeding as though invalid settings were accepted.
4. **Do not destroy a user's existing settings during a failed read/validation.** Saving validates first, writes a unique temporary file with exclusive creation, flushes it to disk, then promotes it using a same-directory move. Temporary output is cleaned up in a `finally` path. Failure to validate a loaded file does not rewrite that file.
5. **Preserve unrelated settings and compatible optional fields.** Persisting the active catalog selection must not overwrite the separately configured server-model ID, timeout, or other settings. Existing schema-version-1 documents without the optional active-catalog ID load with no catalog selection.
6. **Do not treat the sample environment file as runtime configuration.** `.env.example` documents non-secret defaults but is explicitly not auto-loaded. Local environment, secret, signing and machine-specific files remain excluded by the repository ignore policy.

## Implementation and verification links

- Requirements: `docs/requirements.md` (REQ-014).
- Schema-versioned validation and persistence: `src/Host.Infrastructure/LocalHostSettingsStore.cs`.
- WPF settings load/save and safe startup error mapping: `src/Host.Windows/MainWindow.xaml.cs`.
- C# persistence, compatibility and fail-closed tests: `tests/HostLifecycle.Tests/LocalHostSettingsPersistenceTests.cs` and `tests/HostLifecycle.Tests/Program.cs`.
- Local configuration and error-disclosure checks: `tests/test_local_config_safety.py`.
- Non-secret environment example and ignore policy: `.env.example` and `.gitignore`.
- CI verification: `.github/workflows/product-build.yml` and `tests/test_decision_traceability.py`.

## Evidence boundary

The existing tests cover schema-version checks, loopback endpoint validation, timeout/model-ID bounds, settings round-trip, optional-field compatibility, invalid-file preservation, safe startup error handling and repository secret-file hygiene. This is not an OS permission/ACL audit or a claim that every machine's LocalApplicationData directory has independently verified permissions. No remote settings service or cloud synchronization is implied.

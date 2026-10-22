# ADR-0003: Local inference backend integration

- **Status:** Accepted for the first backend integration; live-model acceptance remains pending.
- **Date:** 2026-10-22
- **Related tasks:** T0018, T0020
- **Requirements:** REQ-003 and REQ-004 in `docs/requirements.md`
- **Scope:** Select the local inference adapter boundary and first runnable backend protocol, without claiming that a model is bundled or has been exercised on a target machine.

## Context

The Windows host needs to call inference running on the host computer, without sending the prompt to a central inference API. Keeping the UI coupled to a specific model runtime would make later backend changes harder to test and maintain.

## Decision

1. Expose inference through `IInferenceBackend` and manage readiness, busy/error transitions, and request lifecycle through `HostInferenceSession`.
2. Use a separately installed `llama.cpp` server as the first real local backend, accessed through HTTP:
   - `GET /health` to verify server readiness.
   - `POST /v1/chat/completions` using the configured model ID and prompt.
3. Let the Windows UI configure the local server base URL and model ID, initiate the health check, submit a prompt, and display the returned generated text.
4. Keep request deadlines/cancellation and a per-backend-instance inference gate in the adapter. Do not log prompt/response contents in the adapter.
5. Do not claim that the server/model is bundled, that hardware suitability is automatically detected, or that a supported model has been verified on every target machine.

## Rationale

- The backend interface separates UI/lifecycle management from the HTTP protocol implementation.
- The first adapter uses the local `llama.cpp` server's OpenAI-compatible HTTP interface and can be tested with a controlled `HttpMessageHandler` without pretending the fixture is real inference.
- A separately installed server avoids silently downloading large model weights as part of application startup.

## Code and test traceability

- Interface, lifecycle, and session: `src/Host.Windows/HostLifecycle.cs`.
- HTTP backend implementation: `src/Host.Windows/LlamaCppServerBackend.cs`.
- Host UI and request actions: `src/Host.Windows/MainWindow.xaml` and `src/Host.Windows/MainWindow.xaml.cs`.
- Controlled endpoint/lifecycle tests: `tests/HostLifecycle.Tests/Program.cs`.
- Build and lifecycle CI: `.github/workflows/product-build.yml`.
- Requirement and release gates: `docs/requirements.md` (REQ-003/REQ-004), `docs/acceptance.md` (AC-LOCAL-01 and AC-LOCAL-02).
- Decision index: `docs/decisions/README.md`.

## Consequences and limits

- The HTTP adapter, host UI integration, and controlled endpoint tests exist in the repository.
- Controlled tests use a test handler/fixture; they do not prove generation by a live model.
- `AC-LOCAL-01` remains **NOT RUN** until a user-installed server and supported model return a real generated response on the target host. `AC-LOCAL-02` remains **NOT RUN** until overlapping live requests demonstrate the required single-inference policy.

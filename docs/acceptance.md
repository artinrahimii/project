# Acceptance and evidence policy

This file defines the first executable acceptance gate for Step Three. It is a project-quality gate, not a claim that the Windows host, Android client, or P2P inference product already exists.

## Task completion gate

A graph task may be marked done only when all of the following are true:

1. Its real output files are listed in actual_output_paths and exist in the repository.
2. Its evidence field points to reviewable evidence.
3. At least one relevant test/check is recorded in execution_summary.tests_passed.
4. execution_history contains a succeeded attempt with the real command/test result; summary attempt counters agree with the history.
5. All graph edges still reference valid task IDs, and a validated edge has a written rationale and evidence.

Run the graph guard locally with Python 3.11 or later:

    python -m unittest discover -s tests -v
    python tools/graph_guard.py "step three/project-graph.md"
    python tools/graph_guard.py "step three/project-graph.md" --repo-root .
    
Warnings such as a free-text prerequisite that does not resolve to a task ID remain visible and are not silently converted into task dependencies. Use --strict when warnings must fail a validation run. Passing guard tests do not prove that application features work; each feature still needs its own implementation-specific acceptance tests. Final Windows, Android, and P2P requirements will be detailed with their dedicated task.

## Task T0001 scope

The deliverable for T0001 is the graph guard, its automated tests, and this acceptance policy. The guard uses only the Python standard library. It checks graph structure and rejects completion claims missing outputs, evidence, passing tests, or a successful recorded attempt.


## Task T0003 scope — completion requires code/output, tests, and acceptance criteria

For every node marked `done`, the graph guard now also requires a non-empty `acceptance` criterion and reviewable evidence that names a test source file under `tests/` or `test/`. The existing output/evidence existence checks, recorded passing-test check, and successful-attempt check remain in force. This is a metadata/evidence gate: it does not prove feature correctness by itself; implementation-specific tests and a successful CI run are still required.


## Task T0004 scope — reject known placeholder implementations

When repository-backed validation is enabled with `--repo-root`, each non-test source-code file listed in a task's `actual_output_paths` is inspected (the guard's own implementation and test-source files are excluded to avoid self-matching the detector patterns) for known placeholder markers before a `done` claim can pass. The current denylist covers TODO markers, `NotImplementedException`, `NotImplementedError`, and explicit mock/placeholder/simulated-response implementation labels. Regression tests verify that a placeholder implementation is rejected and ordinary executable source without those markers is accepted.

This is a conservative static check, not proof that arbitrary code is correct or free of every possible stub. Human review and feature-specific runtime tests remain required; passing this gate alone does not prove the Windows/Android/P2P product is complete.


## Task T0005 — reproducible build paths (verified for the recorded baseline)

An earlier version of this section described a superseded repository snapshot in which the application projects were absent. That statement is stale. The current repository has a WPF project at `src/Host.Windows/`, an Android Gradle app at `src/Client.Android/`, and the workflow `.github/workflows/product-build.yml` builds those platform projects and runs the host lifecycle harness.

For baseline commit `aa1bc7e77bc5bbce80e9d7f987de75c1bdc71c36` (`V 3.0.42`), [White-P product build run 39671083885](https://github.com/artinrahimii/project/actions/runs/39671083885) and [Step Three quality gate run 39671083734](https://github.com/artinrahimii/project/actions/runs/39671083734) both completed successfully. These results apply to that exact baseline revision; changes in later commits must be validated by their own workflow runs.

These CI results demonstrate that the configured Windows and Android build entrypoints and lifecycle test run on the recorded baseline. They do not prove product-level local inference, a real-device launch, hardware discovery, P2P transport, encrypted transmission, or no persistent conversation storage. Keep those separate acceptance gates unpassed until the corresponding runtime evidence exists.


## T0013 — Final Windows, Android, and P2P acceptance matrix

**Gate status:** defined; not yet passed by the product and not accepted for release. The repository contains a Windows WPF host, an Android Gradle app, versioned shared Protobuf contracts, a host lifecycle harness, and a Windows UI connected to a local llama.cpp HTTP backend adapter. CI builds both platform projects and runs the lifecycle harness. Live generation against an installed model on the target host remains unverified. Hardware discovery, Android-to-host/P2P transport, cryptographic transport, and product-level request/network/storage test harnesses remain unimplemented. Compilation and controlled tests do not pass the end-to-end release gates.

### Release decision rule

A release candidate is accepted only when every applicable gate below has a linked test result or inspection record from the actual implementation. A missing project, skipped test, mocked-only end-to-end path, unavailable device, or unrecorded result is **not passed**. Mark each gate `PASS`, `FAIL`, or `NOT RUN`; `NOT RUN` blocks release acceptance. Preserve logs and toolchain versions without recording prompts, model responses, credentials, or other sensitive payloads.

### Gate matrix

| Gate | Required real acceptance evidence | Required verification | Failure condition |
|---|---|---|---|
| AC-WIN-01 — Windows build and launch | Clean-checkout build log, produced executable, OS/.NET versions, and launch smoke-test result | Build the actual Windows host project on a supported Windows runner; launch it and verify the initial UI opens without an unhandled exception | Missing project/artifact, failed build, failed launch, or absent log |
| AC-WIN-02 — Host lifecycle | Test results for Stopped, Starting, Ready, Busy, and Error states | Exercise real lifecycle transitions and verify displayed states derive from runtime state | State is hard-coded, unreachable, or does not match runtime |
| AC-AND-01 — Android build and UI | Clean Gradle build log, APK artifact, SDK/JDK versions, and automated UI/instrumentation result | Build the actual Android project and open the app in an emulator or device; verify request entry and visible state/error UI | Missing APK, failed build, or UI test not run |
| AC-HOST-01 — Hardware and capacity | Test logs for adequate/insufficient RAM and storage, unsupported backend, and unknown capability | Derive readiness from actual OS/backend observations; unknown or insufficient capacity must not report Ready | Fabricated capability or unsupported host reports Ready |
| AC-LOCAL-01 — Local inference | Integration log from a real local backend and supported test model; backend health and generated result | Send a request to the backend on the same host and verify a backend-generated response; separately audit outbound inference traffic | Mock-only response, cloud inference request, or no runtime evidence |
| AC-LOCAL-02 — Single active inference | Concurrency test report for overlapping requests | Start two requests concurrently and prove inference execution never overlaps; verify the defined busy/wait policy | Two concurrent executions or undefined outcome |
| AC-MODEL-01 — Model metadata and install integrity | Catalog validation and controlled local-fixture install report | Validate license/source/checksum/backend metadata; test streaming writes, checksum rejection, cancellation, and atomic activation | Invalid metadata silently accepted or corrupt/incomplete file activated |
| AC-P2P-01 — Direct request/response | Multi-process or multi-device transport integration result with endpoint/packet inspection | Transfer a request and response directly between Android client and host; verify the payload path does not traverse a rendezvous relay | UI-only test, same-process simulation, or relay carrying inference content |
| AC-PRIV-01 — No central prompt/response handling | Endpoint review plus transport/network trace and storage inspection | Verify no central inference/history endpoint receives prompt or response content and rendezvous messages contain discovery/connection metadata only | Prompt/response appears at a central endpoint, relay, or persistent conversation store |
| AC-ID-01 — Temporary request identity | Protocol tests and rendezvous storage/payload inspection | Verify request identifiers are temporary, not a stable account identity, and not retained by rendezvous infrastructure | Persistent user identifier required for inference routing or request ID persisted centrally |
| AC-PROTO-01 — Shared protocol | Source Protobuf contract, reproducible generation log, C#/Kotlin round-trip fixture results | Generate both platform bindings from the checked-in source; test compatible and incompatible protocol versions | Generated sources drift, round-trip mismatch, or incompatible version accepted |
| AC-INPUT-01 — Bounds and validation | Boundary-test report | Test below-limit, at-limit, over-limit, missing-field, invalid-value, and incompatible-version cases before inference starts | Invalid input reaches inference or boundaries are untested |
| AC-FAIL-01 — Deadline and failure states | Test results for deadline, cancellation, disconnect, malformed response, and backend failure | Confirm each condition yields a distinct typed failure and never a success-shaped model response | Failure appears as successful output or cancellation is ignored |
| AC-STATUS-01 — Busy and unsupported model | Shared protocol fixture tests and Android integration/UI results | Serialize/deserialize busy and unsupported-model outcomes; verify the client displays each as a non-success state | Outcome is ambiguous, lost, or rendered as success |
| AC-TOOLS-01 — Agent-tool safety | Tool-policy and process-boundary test report | Reject unknown tools/arguments; require confirmation for side effects; prove arbitrary system commands are disabled by default | Unapproved or arbitrary command execution is possible |
| AC-CI-01 — Reproducible release build | Successful clean-checkout CI logs for Windows, Android, and shared contracts, with pinned toolchain versions | Restore dependencies, build actual projects, run their tests/lint, and verify failures fail the workflow | Only Python/graph checks run, or a product build/test is skipped or failing |
| AC-CLAIMS-01 — Claims and scope | Documentation contract-test output and claim-to-evidence audit | Ensure privacy/security/capability claims link to repeatable evidence and out-of-scope items are not represented as delivered | Unsupported local-only/P2P/encryption/no-storage claim or scope misrepresentation |

### Requirement-to-gate traceability

| Requirement | Acceptance gate(s) |
|---|---|
| REQ-001 | AC-WIN-01, AC-WIN-02 |
| REQ-002 | AC-AND-01 |
| REQ-003 | AC-LOCAL-01 |
| REQ-004 | AC-LOCAL-01 |
| REQ-005 | AC-LOCAL-02 |
| REQ-006 | AC-HOST-01 |
| REQ-007 | AC-MODEL-01 |
| REQ-008 | AC-MODEL-01 |
| REQ-009 | AC-P2P-01, AC-STATUS-01 |
| REQ-010 | AC-P2P-01 |
| REQ-011 | AC-PRIV-01 |
| REQ-012 | AC-ID-01 |
| REQ-013 | AC-PROTO-01 |
| REQ-014 | AC-INPUT-01 |
| REQ-015 | AC-FAIL-01 |
| REQ-016 | AC-STATUS-01 |
| REQ-017 | AC-TOOLS-01 |
| REQ-018 | AC-CI-01 |
| REQ-019 | AC-CLAIMS-01 |
| REQ-020 | AC-CLAIMS-01 |

The matrix groups related requirements for operational use but does not replace the detailed acceptance text in `docs/requirements.md`. Before release, each listed requirement must pass its own acceptance condition as well as the corresponding gate.

### Evidence record template

For each gate, record: gate ID, `PASS/FAIL/NOT RUN`, commit SHA under test, command or test identifier, runner/device and OS/toolchain versions, artifact/log URL, observed result, and reviewer/date. Store evidence references in the task report and graph; do not copy prompt text, generated responses, access tokens, or private model data into logs.

### Current result

All AC-* gates are **NOT RUN for the product** in the strict release-acceptance sense unless their complete evidence requirement is linked. The current Python quality workflow can validate this matrix's document contract, but it cannot establish that the Windows host, Android client, local inference, or direct P2P transport works. Current CI proves platform compilation and the host lifecycle harness only for the recorded commit; it does not establish real local inference, Android-to-host transport, direct P2P, encrypted transmission, absence of central prompt handling, or no persistent conversation storage.


## T0006 — executable host lifecycle coverage

The lifecycle source used by the Windows host is independent of WPF and can be tested on cross-platform CI. The harness checks allowed state transitions, rejects an invalid transition, and verifies startup reports an error when the local inference backend is not configured.

Run with .NET 10: `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release`.

The host must not display `Ready` while the backend is unavailable. A new session starts with `UnconfiguredInferenceBackend` until the user connects to a server; the connected `LlamaCppServerBackend` performs real HTTP health and generation requests. The controlled tests do not establish generation with an installed production model, hardware capability detection, Android-to-host networking, P2P transport, or physical-device behavior. Those product gates remain NOT RUN until verified with corresponding runtime evidence.


## T0007 — executable lifecycle decision trace

The host lifecycle decision “Ready is allowed only when the configured inference backend reports available” is reflected in `src/Host.Domain/HostLifecycle.cs` and `src/Host.Application/HostInferenceSession.cs` and covered by the executable harness `tests/HostLifecycle.Tests/Program.cs`. The harness checks both sides of the decision: an unconfigured backend must yield Error, while an available test backend may yield Ready. The available backend is a test double used only to exercise the branch; it is not a production inference backend.

The test result for this revision is pending the GitHub Actions run. This single traced decision does not complete the broader T0007 requirement or establish real inference readiness.


## T0007-A002 — backend startup failure containment

HostStartup.Start now converts an exception thrown while checking the inference backend into a stable Error status rather than allowing the exception to escape the startup path. The user-facing message deliberately excludes raw exception details. The executable lifecycle harness adds a throwing-backend regression case and checks both the Error state and absence of leaked exception text.

Run in CI with .NET 10: `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release`. The harness covers lifecycle/error handling and controlled local-backend endpoint contracts; it does not establish live generation against an installed model on the target machine. The product-build workflow for commit `1d4760ae30c340144a95f8c29a9cabd724295b04` passed on 2026-10-22, including this harness.


## T0007-A004 — unavailable-reason getter failure

The lifecycle harness also exercises a backend that reports `IsAvailable == false` but throws while reading `UnavailableReason`. The expected behavior is a stable `Error` status with a generic startup-check failure message; raw exception details must not reach the user-facing status. This validates startup error handling and does not replace the separate live-model inference acceptance test.

Result for this revision: pending the GitHub Actions run. The case uses a test double and does not establish production backend availability.


## T0007-A005 — empty unavailable-reason fallback

The lifecycle harness verifies that an unavailable backend returning a blank or whitespace-only `UnavailableReason` cannot leave startup without a usable status message. The expected result is `Error` with a stable generic startup-check failure message. This exercises the existing `ArgumentException.ThrowIfNullOrWhiteSpace` guard and fallback path; it is not evidence of a production inference backend.


### T0007-A007 — deterministic unavailable-backend fallback

When a backend reports unavailable, HostStartup now trims a non-empty reason and uses the explicit message `Local inference backend is unavailable. Host is not ready.` when the reason is null, empty, or whitespace. This avoids treating malformed-but-nonthrowing backend metadata as an exception and makes the UI state deterministic. The executable regression harness asserts the exact fallback string.

- Verification target: `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release`
- CI evidence for commit `aa1bc7e77bc5bbce80e9d7f987de75c1bdc71c36` (`V 3.0.42`): [Step Three quality gate run 39671083734](https://github.com/artinrahimii/project/actions/runs/39671083734) and [White-P product build run 39671083885](https://github.com/artinrahimii/project/actions/runs/39671083885) both passed.
- Scope limit: `LlamaCppServerBackend` now provides a real HTTP adapter for a separately installed llama.cpp server and the Windows UI calls it; live generation with a supported model on the target host is still unverified. Hardware discovery and P2P transport are not implemented.


## T0008 — repeatable audit of capability and privacy claims

**Audit scope and result:** Reviewed the checked-in Windows and Android UI source, host lifecycle/backend source, `README.md`, `docs/requirements.md`, and the release acceptance matrix on `main` at `aa1bc7e77bc5bbce80e9d7f987de75c1bdc71c36`. The source review found the following actual limitations:

- The Windows UI now exposes connection, health-check, prompt, and generated-output controls backed by `HostInferenceSession` and `LlamaCppServerBackend`. An unconfigured session correctly reports unavailable; CI verifies build and controlled tests, while a live model run on the target machine remains unverified.
- The Android request button only updates local UI status. It does not transmit the request, and the UI explicitly says that transport is not implemented.
- The code and documents describe local inference, P2P, privacy, and no-central-history as goals/requirements, not as proven delivered capabilities. The UI labels were changed to identify the Windows host and Android client as previews and to disclose the missing transport.
- No encrypted transport or product-level storage/network audit has been implemented. This source/document review therefore **does not prove** that a future runtime will keep prompt/response data local, encrypted, or unpersisted.

A repeatable documentation/source contract check is in `tests/test_claims_audit.py`. Run it with `python -m unittest discover -s tests -p "test_claims_audit.py" -v`; the full Python quality workflow also executes it. The source-level checks verify honest capability labels and explicit limitations only. AC-LOCAL-01, AC-P2P-01, AC-PRIV-01, and the encryption/storage portions of release acceptance remain **NOT RUN** until real runtime/traffic/storage evidence exists.


## T0009 — guard against central inference/history endpoint declarations

Requirement REQ-011 in `docs/requirements.md` prohibits a central API from receiving inference text, generating model responses, or retaining conversation history. The tracked `src/` inventory at V 3.0.44 includes only the Windows host shell and Android client; no central server/API project or central endpoint handler is present. The Android request UI explicitly states that requests are not sent because host transport is not implemented.

The new guard in `tests/test_central_inference_boundary.py` scans first-party source files outside `src/Host.Windows/` for common POST/PUT/PATCH route-handler declarations with paths indicating inference, prompts, chat, completion, responses, conversations, or history. The intended local Windows host is excluded because local inference/request endpoints may legitimately live there; this check also does not prohibit future direct peer-to-peer transport routes.

This is a preventive source-level check, not proof of runtime routing. REQ-011 remains unaccepted until endpoint review, actual transport/network traces, and storage inspection show that any optional rendezvous component handles discovery/connection metadata only and never receives prompt/response content. The current source snapshot has no central prompt service, but Android-to-host transport is not implemented yet.


## T0010 — clean-checkout build/test reproducibility and artifacts

The build workflow and README now share explicit commands for the Windows WPF host, Android debug APK, host lifecycle tests, and Python graph/quality checks. Toolchain policy is checked against `global.json`, JDK 17, Gradle 8.13, Android platform 36 and build-tools 36.0.0.

The product workflow now uploads the Windows host build output and Android debug APK as separate GitHub Actions artifacts with seven-day retention. `if-no-files-found: error` makes a missing output fail the workflow instead of appearing successful. The new `tests/test_reproducible_build_contract.py` keeps the README, toolchain declarations, workflow commands, output paths and quality gate aligned.

For commit `1f3fded138a12802a7dd5a631c38fdc149835a91` (`V 3.0.47`), the [Step Three quality gate](${qualityUrl}) passed 45 Python tests and all syntax/graph/evidence checks. In [product-build run 39677788376](${productUrl}), the Windows build/upload steps passed, the Android debug APK build/upload steps passed, and the host lifecycle test job completed successfully. The run contains `whitep-windows-host` (88,918 bytes) and `whitep-android-debug-apk` (10,577,636 bytes); both expire on 2026-10-28. A subsequent check confirmed that the same product workflow completed with conclusion `success` at 2026-10-21T21:20:46Z; all jobs completed without failure.

A successful build/artifact upload demonstrates repeatable build outputs; it does not prove runtime local inference, Android-to-host connectivity, P2P transport, encryption or absence of persistent history.


## T0017 — shared Protobuf wire contract

The versioned source of truth is `protocol/whitep/v1/messages.proto`. The Windows project generates C# messages during .NET build; the Android module generates Java-lite messages from the same repository-level source during Gradle build. The contract includes capabilities/status, inference request and response, cancellation, and typed protocol errors.

Acceptance requires the quality workflow and actual Windows/Android build jobs to pass for the commit. Message definitions and generated classes do not establish that transport, P2P routing, local inference, encryption, or no-persistence behavior exists; those remain separate acceptance gates.

Verification history for T0017: on commit `f3783ee3af38c2d2086777950e97778273190573` (`V 3.0.50`), the quality gate passed and the Windows/lifecycle jobs passed, but the Android build failed during Kotlin DSL configuration because the `proto`/`srcDir` extension references were unresolved. The follow-up adds the explicit Protobuf Gradle extension import; final acceptance is pending the exact follow-up commit's CI results.


## T0018 — local inference backend client

The Windows host contains a concrete `LlamaCppServerBackend` HTTP client for a locally running llama.cpp server: `GET /health` for readiness and `POST /v1/chat/completions` for generation. The test harness verifies request/response mapping, health-state changes, prompt validation and the endpoint paths using a controlled HTTP handler. This verifies the client contract, not actual model inference. Release acceptance still requires a separate real-server/model integration run, confirmation that the backend listens only on the intended local interface, and UI lifecycle integration. No prompt or generated response should be logged.

## T0019 — selected P2P data-path decision

[ADR-0002 — Direct P2P data path](decisions/ADR-0002-direct-p2p-data-path.md) records the first-release architecture: inference requests and responses travel directly between peers; rendezvous/coordination is limited to discovery and connection setup and must not relay inference payloads. This is an architecture decision, not an implementation claim. The source-contract test checks traceability and policy wording; **AC-P2P-01 remains NOT RUN** until real multi-process or multi-device transport evidence and payload-path inspection pass.

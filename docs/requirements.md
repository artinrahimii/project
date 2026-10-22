# White-P — Testable Product Requirements

**Document status:** Requirements baseline for the planned first release; implementation is not implied.
**Scope source:** `README.md` and `step one/V1.0.md`.
**Interpretation:** A requirement is accepted only when its stated acceptance condition is demonstrated by a reviewable test or inspection. A planned task reference is traceability, not evidence of implementation.

## Product boundary

The first release is intended to comprise a Windows host, an Android client, shared versioned message contracts, a real local inference backend, and direct peer-to-peer request/response transfer. A central service, if eventually used for rendezvous, must not process inference text or retain conversation history. These are requirements to implement and verify, not claims about the current repository.

## Functional requirements

### REQ-001 — Windows host application
- **Requirement:** Provide a runnable Windows host application that can report its lifecycle state.
- **Acceptance:** A clean CI build produces the host executable; a launch smoke test reaches the initial UI without an unhandled exception and the UI exposes Stopped/Starting/Ready/Busy/Error states from runtime state.
- **Verification:** Windows build job plus a host lifecycle test; record OS/runtime versions.
- **Trace:** T0014, T0015, T0021, T0022, T0041, T0043, T0044, T0046.

### REQ-002 — Android client application
- **Requirement:** Provide an installable Android client with a request-entry screen and visible request state.
- **Acceptance:** A clean CI build produces an APK; an automated UI or instrumentation test opens the main screen and verifies request entry and a visible state/error area.
- **Verification:** Gradle build and Android unit/UI test.
- **Trace:** T0014, T0016, T0021, T0023, T0029.

### REQ-003 — Local inference only
- **Requirement:** The host sends inference input only to a backend running on the same host for the first release; no cloud inference provider is used in the normal inference path.
- **Acceptance:** An integration test with a local backend returns its generated output, and a network-observation or dependency-injection test demonstrates that the inference path makes no request to a cloud inference endpoint.
- **Verification:** Local-backend integration test and outbound-network audit; a mocked test alone is not sufficient for the end-to-end claim.
- **Trace:** T0018, T0057–T0067.

### REQ-004 — Real local inference backend
- **Requirement:** Support a real local inference backend through a backend-independent engine interface, with llama.cpp server as the first planned backend.
- **Acceptance:** With a supported backend and an available test model, a health check succeeds and an inference request returns backend-generated text; missing backend and backend failure produce explicit errors.
- **Verification:** Engine unit tests and an opt-in integration test with a licensed small model.
- **Trace:** T0018, T0057–T0067.

### REQ-005 — One active inference per host
- **Requirement:** The host accepts at most one active inference request at a time.
- **Acceptance:** A concurrency test starts two requests together and proves that no two inference executions overlap; the second request receives a defined busy response or waits according to the documented policy.
- **Verification:** Deterministic coordinator concurrency test.
- **Trace:** T0098–T0105.

### REQ-006 — Hardware and capacity awareness
- **Requirement:** The host reports available OS, memory, storage, and backend/GPU capability only from actual system/backend observations.
- **Acceptance:** Unit/integration tests cover sufficient resources, insufficient RAM/storage, unsupported backend, and unknown capability; unknown or insufficient capacity must never be reported as Ready.
- **Verification:** Platform service tests and host-state tests.
- **Trace:** T0049–T0056.

### REQ-007 — Model metadata and license transparency
- **Requirement:** Each listed model has a stable catalog identifier, format, size, checksum, backend compatibility, source, and license status.
- **Acceptance:** Valid catalog entries pass schema validation; missing license information is labelled as needing review; malformed metadata, checksum format, or unsafe URL is rejected.
- **Verification:** Catalog schema/parser tests and review of the bundled catalog sources.
- **Trace:** T0068–T0075, T0087.

### REQ-008 — Integrity-checked model installation
- **Requirement:** Download and install model files without holding the complete file in memory, and do not activate a file before integrity checks succeed.
- **Acceptance:** Tests demonstrate streaming writes, checksum mismatch rejection, cancellation without a falsely complete final file, and atomic activation only after validation.
- **Verification:** Downloader/install service tests using controlled local test fixtures; document the fixture limits.
- **Trace:** T0076–T0086.

### REQ-009 — Android-to-host request/response
- **Requirement:** The Android client can submit a request to a selected reachable host and display the response or a typed failure.
- **Acceptance:** A host/client integration test transfers a request and response over the selected transport; timeout, disconnect, busy host, and unsupported model are distinguishable from a successful response.
- **Verification:** Cross-component integration tests; UI-only tests do not satisfy transport acceptance.
- **Trace:** T0031–T0040, T0106–T0125, T0140–T0165.

### REQ-010 — Direct P2P data path
- **Requirement:** Direct peer-to-peer transfer is the default data path. Any optional rendezvous service may assist discovery/connection setup only and must not relay inference content as the normal path.
- **Acceptance:** A transport integration test confirms peer-to-peer data transfer; a test or network trace confirms that request/response payloads do not pass through the rendezvous component.
- **Verification:** Multi-process or multi-device transport test and documented packet/endpoint inspection.
- **Trace:** T0019, T0126–T0139, T0140–T0165.

### REQ-011 — No central inference API or conversation history
- **Requirement:** No central API receives request text, generates model output, or stores conversation history.
- **Acceptance:** Repository inspection finds no central inference/history endpoint, and a transport test verifies that any rendezvous messages contain discovery/connection metadata only, not prompt or response content.
- **Verification:** Endpoint review plus network-level integration test; source-code search alone is insufficient.
- **Trace:** T0009, T0019, T0106–T0165.

### REQ-012 — Temporary request identity
- **Requirement:** Each request uses a temporary request identifier/hash and does not require a permanent user identifier for inference routing.
- **Acceptance:** Protocol tests verify that request identifiers are unique within the defined collision assumptions, are not reused as a stable account identity, and are not persisted by any rendezvous component.
- **Verification:** Identifier-generation tests and rendezvous payload/storage inspection.
- **Trace:** T0031–T0035, T0106–T0125, T0126–T0139.

### REQ-013 — Versioned cross-platform protocol
- **Requirement:** Shared messages are defined in source Protobuf contracts, generated for both platforms, and carry a protocol version.
- **Acceptance:** Code generation runs from the checked-in source contract; C# and Kotlin round-trip fixtures agree; incompatible protocol versions are rejected with a typed error.
- **Verification:** Reproducible code-generation step and shared fixture compatibility tests.
- **Trace:** T0017, T0031–T0040.

### REQ-014 — Bounds and input validation
- **Requirement:** The transport and message parser enforce documented request, capsule, and response size limits and reject malformed or unsupported messages.
- **Acceptance:** Boundary tests cover just-below-limit, at-limit, over-limit, missing fields, invalid values, and incompatible versions; invalid data is rejected before inference starts.
- **Initial protocol ceilings:** serialized inference request ≤ 16,384 bytes; prompt ≤ 8,192 UTF-8 bytes; context capsule ≤ 4,096 bytes; serialized inference response ≤ 2,097,152 bytes; generated response text ≤ 1,048,576 UTF-8 bytes. Host-announcement/control messages are limited to 16,384 raw bytes before Protobuf parsing. The separately invoked local llama.cpp HTTP adapter also bounds its JSON request body to 65,536 bytes and response body to 2,097,152 bytes.
- **Enforcement:** Typed request/response sizes are validated with the shared policy; the local adapter checks prompt bytes and serialized request body before sending and streams the HTTP response through a bounded reader before JSON parsing. A future peer transport must check raw frame length before Protobuf parsing and enforce the smaller of local and negotiated capsule ceilings.
- **Verification:** Contract parser, shared C# host harness, Android shared-wire tests, and local-adapter tests for oversize payload rejection.
- **Trace:** T0039, T0040, T0130–T0139.

### REQ-015 — Deadline, cancellation, and failure states
- **Requirement:** Requests support deadlines and cancellation; backend, transport, and validation errors remain distinguishable from successful model output.
- **Acceptance:** Tests cover expired deadline, cancellation during generation, disconnect, malformed response, and backend failure; none may be rendered as a successful response.
- **Verification:** Engine/coordinator/transport unit and integration tests.
- **Trace:** T0032, T0034, T0062, T0063, T0091, T0100–T0125.

### REQ-016 — Busy and unsupported-model handling
- **Requirement:** The client and host represent a busy host and an unsupported model as explicit protocol outcomes.
- **Acceptance:** Contract tests serialize/deserialize both outcomes and an integration test verifies the Android UI displays each as a non-success state.
- **Verification:** Shared protocol fixture test and client integration/UI test.
- **Trace:** T0034, T0098–T0105, T0140–T0165.

### REQ-017 — Safe agent-tool boundary
- **Requirement:** Agent tools are disabled unless the model/backend supports them, only allowlisted tools may run, and operations with side effects require user confirmation.
- **Acceptance:** Tests reject unknown tools and invalid arguments, require confirmation for side-effect operations, and prove arbitrary system commands are not enabled by default.
- **Verification:** Tool policy unit tests and process-boundary tests.
- **Trace:** T0093–T0097.

### REQ-018 — Reproducible builds and quality gates
- **Requirement:** Build and test entrypoints for each implemented platform are documented, versioned, and run in CI from a clean checkout.
- **Acceptance:** A clean CI run restores dependencies and builds the actual Windows and Android projects plus shared contracts; automated tests and lint fail the workflow when they fail. Python graph checks alone do not satisfy this requirement.
- **Verification:** GitHub Actions logs for the .NET and Android jobs, with toolchain versions pinned.
- **Trace:** T0010, T0024–T0030.

### REQ-019 — Honest capability and privacy claims
- **Requirement:** UI and documentation distinguish implemented, unavailable, and unverified capabilities; claims such as local-only, P2P, encrypted, or no-storage are not presented as proven without matching evidence.
- **Acceptance:** Contract tests prevent unsupported completion/privacy claims in project documentation, and each security/privacy claim links to a repeatable test or audit result.
- **Verification:** Documentation contract tests plus feature-specific network/security tests; text checks alone do not prove runtime privacy.
- **Trace:** T0001–T0009, T0011–T0013.

### REQ-020 — Explicit first-release boundary
- **Requirement:** The first release prioritizes Windows hosting, Android requesting, one inference per host, local model execution, and direct P2P exchange; full model marketplace and broad unrestricted agent execution are out of scope.
- **Acceptance:** The release checklist maps each in-scope capability to a passing test and lists out-of-scope features separately; no out-of-scope feature is described as delivered without its own approved scope change.
- **Verification:** Release acceptance review and traceability audit.
- **Trace:** T0011–T0020 and the relevant implementation tasks referenced above.

## Implementation status

All requirements above are a testable specification baseline. They are not marked implemented merely because their acceptance criteria are written. The repository must supply the platform code, automated tests, build logs, and transport/backend evidence before any implementation requirement is accepted.

# ADR-0015: Fail-closed participation listener lifecycle

- **Status:** Accepted for the existing loopback-only WPF listener lifecycle and recovery boundary.
- **Date:** 2026-10-27
- **Related task:** T0007 (technical-decision traceability).
- **Related requirements:** REQ-009, REQ-010, REQ-011, REQ-019 and REQ-020 in `docs/requirements.md`.
- **Scope:** Safe start/stop/shutdown of the current Windows host's participation listener. This ADR does not claim that peer-to-peer request routing or Android-to-Windows inference is implemented.

## Context

The WPF host has an explicit network-participation toggle, a serialized listener lifecycle, bounded request framing and an asynchronous shutdown path. The current application constructs that lifecycle with `IPAddress.Loopback`; the listener must not be presented as a remotely reachable peer. Until the host request handler and cross-platform route are integrated, accepting a frame must not be represented as successful inference service.

## Decision

1. **Remain loopback-only by default.** The WPF composition uses `new(IPAddress.Loopback, 0)`; the current listener rejects non-loopback bind addresses. No public address, reachable rendezvous configuration, or peer presence is inferred from a local socket.
2. **Start disabled and serialize transitions.** Participation starts inactive. Lifecycle transitions use a semaphore so activation, deactivation and disposal cannot overlap in an unsafe order.
3. **Stop fail-closed.** Deactivation first marks participation inactive and then stops/releases the listener. Failed activation resets the active flag, requests listener cleanup and surfaces a safe user-facing error without raw exception details.
4. **Await shutdown cleanup.** WPF close is deferred until the participation lifecycle's asynchronous disposal finishes. Repeated or post-disposal activation is rejected; the shutdown path does not advertise successful cleanup when an error is reported.
5. **Do not imply request processing.** The current request-frame listener has no inference callback wired by the WPF composition. The UI states that inbound requests are closed until protocol handling is implemented. The production P2P route remains a separate acceptance requirement.

## Implementation and verification links

- WPF participation toggle and explicit loopback construction: `src/Host.Windows/MainWindow.xaml`, `src/Host.Windows/MainWindow.xaml.cs`.
- Serialized start/stop/disposal: `src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs`.
- Loopback restriction, bounded frames and fail-closed dispatch: `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`.
- Safe activation rollback/error presentation: `src/Host.Application/ParticipationActivationRecovery.cs`.
- Executable lifecycle coverage: `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`, `tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs`, and `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`.
- Source regression guard: `tests/test_network_listener_exposure.py`.
- Decision/source/test register and traceability test: `docs/decisions/README.md`, `tests/test_decision_traceability.py`.
- Windows/Android product build and host acceptance entry points: `.github/workflows/product-build.yml`.

## Evidence boundary and remaining work

The linked tests exercise host-side listener lifecycle, loopback frame handling, activation rollback and resource release. Source-level checks protect the WPF composition and user-facing capability boundary. These checks do not prove public reachability, production peer announcement, Android-to-Windows request/response routing, or complete inference delivery.

`T0121` remains `in_progress` until a safe HTTPS rendezvous configuration, a verified peer-reachable address/identity policy, and real participation/shutdown wiring are implemented and tested. `AC-P2P-01` remains **NOT RUN**. Do not advertise loopback as remotely reachable.

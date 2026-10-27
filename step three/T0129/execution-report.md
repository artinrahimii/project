# T0129 execution report — in progress

- **Task:** use a maintained cryptography library and do not implement application-specific cryptographic algorithms.
- **Attempt:** T0129-A001.
- **Baseline:** `V 3.0.433` — `b525337ea862863adc2ea67584c186be700fe787`.
- **Dependency review:** the incoming edge `T0128 → T0129` was reviewed against the existing libp2p transport and its tests. T0128 is the eventual multi-node end-to-end acceptance test; it produces no crypto-library or identity-parser artifact consumed by T0129. The secure libp2p stack and host-side authenticated transport already exist, so centralizing its use is a sequencing-only step that should precede the end-to-end test. The original `type=blocks` edge remains; its classification and evidence are recorded in the graph.
- **Implemented in this attempt:** created `src/Shared.Security/WhiteP.Shared.Security.csproj` and `PeerSecurityPolicy.cs`; refactored `Libp2pPeerTransport` to use the shared fail-closed identity input/parser policy; added a runnable C# acceptance test and a Python regression guard; pinned the libp2p stack to `1.1.0`; added ADR-0016 and registered it.
- **Test boundary:** no local repo checkout is available in this environment, so local .NET execution is not claimed. Exact-commit Step Three quality and Windows/Android product workflows are pending; do not mark T0129 done until both are inspected.
- **Security boundary:** this change does not implement cryptography. The libp2p library owns Noise/TLS and the cryptographic handshake; the new shared module normalizes input and calls the library's PeerId parser. It does not implement stable host key storage or peer enrollment. `AC-P2P-01` remains **NOT RUN**.
- **Follow-up:** inspect exact-commit CI. Preserve any failure and repair it before completion. Continue T0121 only after the host has a real authenticated inbound request path, stable identity/configuration, and verified peer-reachable address.

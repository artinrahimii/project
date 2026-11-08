# UPD-S1-008 — audit of all dependency edges

- Source graph: `update/Step two/project-graph.md`, exact source HEAD `ae7725fd30f98a75968edb78082fe0d64ce409cd`.
- Authoritative prerequisite source: `update/Step one/Output/V1.10.json`.
- Update graph inventory: 24 nodes, 80 directed edges, 13 execution waves.
- Before this review: 10/80 edges were validated and 70/80 were unreviewed.
- Every one of the 70 remaining edges was checked against the corresponding target task's explicit `prerequisites` array in V1.10. No edge was absent from the approved prerequisite list.
- Producer and consumer code/test/documentation evidence was checked for every edge. All evidence paths resolve in the exact source HEAD Git tree.
- No edge was deleted, no `blocks` type was reinterpreted as a hard dependency by itself, and the execution order and 13 waves were preserved.
- Classification after this review (all 80 edges): 19 `hard_dependency`, 34 `contract_or_interface_dependency`, 27 `sequencing_only`, 0 `unreviewed`, 0 `disputed`.
- For 70 edges reviewed in this attempt: 19 hard, 31 contract/interface, 20 sequence-only. The other 10 prevalidated edges remain unchanged (7 sequence-only, 3 contract/interface).

## Edge-by-edge decisions

| # | Dependency | Classification | Validation | Evidence |
|---:|---|---|---|---|
| 3 | UPD-S1-001 → UPD-S1-004 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-001/evidence/baseline-audit.md`; `docs/decisions/ADR-0023-one-shot-inference-v1.md` |
| 4 | UPD-S1-002 → UPD-S1-004 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-002/evidence/verification.md`; `docs/decisions/ADR-0023-one-shot-inference-v1.md` |
| 10 | UPD-S1-002 → UPD-S1-007 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-002/evidence/verification.md`; `docs/decisions/ADR-0018-explicit-peer-trust-and-identity-verification.md` |
| 11 | UPD-S1-005 → UPD-S1-007 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-005/evidence/resource-policy-review.md`; `docs/decisions/ADR-0018-explicit-peer-trust-and-identity-verification.md` |
| 12 | UPD-S1-006 → UPD-S1-008 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `tests/test_update_graph_contract.py` |
| 13 | UPD-S1-007 → UPD-S1-008 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-007/evidence/security-control-matrix.md`; `tests/test_update_graph_contract.py` |
| 14 | UPD-S1-006 → UPD-S1-009 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationWorkflowAcceptanceTest.kt` |
| 15 | UPD-S1-008 → UPD-S1-009 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-008/evidence/reconciliation-baseline-audit.md`; `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationWorkflowAcceptanceTest.kt` |
| 16 | UPD-S1-024 → UPD-S1-009 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationWorkflowAcceptanceTest.kt` |
| 17 | UPD-S1-009 → UPD-S1-010 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`; `tests/test_android_conversation_detail_contract.py` |
| 18 | UPD-S1-017 → UPD-S1-010 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `tests/test_android_conversation_detail_contract.py` |
| 19 | UPD-S1-022 → UPD-S1-010 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt`; `tests/test_android_conversation_detail_contract.py` |
| 20 | UPD-S1-023 → UPD-S1-010 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`; `tests/test_android_conversation_detail_contract.py` |
| 21 | UPD-S1-024 → UPD-S1-010 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/test_android_conversation_detail_contract.py` |
| 22 | UPD-S1-006 → UPD-S1-011 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `tests/test_android_conversation_detail_contract.py` |
| 23 | UPD-S1-007 → UPD-S1-011 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-007/evidence/security-control-matrix.md`; `tests/test_android_conversation_detail_contract.py` |
| 24 | UPD-S1-014 → UPD-S1-011 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`; `tests/test_android_conversation_detail_contract.py` |
| 25 | UPD-S1-023 → UPD-S1-011 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`; `tests/test_android_conversation_detail_contract.py` |
| 26 | UPD-S1-024 → UPD-S1-011 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/test_android_conversation_detail_contract.py` |
| 27 | UPD-S1-006 → UPD-S1-012 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs` |
| 28 | UPD-S1-008 → UPD-S1-012 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-008/evidence/reconciliation-baseline-audit.md`; `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs` |
| 29 | UPD-S1-024 → UPD-S1-012 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs` |
| 30 | UPD-S1-005 → UPD-S1-013 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-005/evidence/resource-policy-review.md`; `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs` |
| 31 | UPD-S1-012 → UPD-S1-013 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`; `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs` |
| 32 | UPD-S1-024 → UPD-S1-013 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs` |
| 33 | UPD-S1-002 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-002/evidence/verification.md`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 34 | UPD-S1-003 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-003/evidence/decision-matrix.md`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 35 | UPD-S1-013 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 36 | UPD-S1-017 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 37 | UPD-S1-022 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 38 | UPD-S1-023 → UPD-S1-014 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 39 | UPD-S1-024 → UPD-S1-014 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` |
| 40 | UPD-S1-003 → UPD-S1-015 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-003/evidence/decision-matrix.md`; `tests/test_android_inference_attempt_budget_acceptance.py` |
| 41 | UPD-S1-014 → UPD-S1-015 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`; `tests/test_android_inference_attempt_budget_acceptance.py` |
| 42 | UPD-S1-016 → UPD-S1-015 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`; `tests/test_android_inference_attempt_budget_acceptance.py` |
| 43 | UPD-S1-017 → UPD-S1-015 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `tests/test_android_inference_attempt_budget_acceptance.py` |
| 44 | UPD-S1-024 → UPD-S1-015 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/test_android_inference_attempt_budget_acceptance.py` |
| 45 | UPD-S1-002 → UPD-S1-016 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-002/evidence/verification.md`; `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` |
| 46 | UPD-S1-007 → UPD-S1-016 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-007/evidence/security-control-matrix.md`; `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` |
| 47 | UPD-S1-008 → UPD-S1-016 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-008/evidence/reconciliation-baseline-audit.md`; `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` |
| 48 | UPD-S1-017 → UPD-S1-016 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` |
| 49 | UPD-S1-022 → UPD-S1-016 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt`; `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` |
| 53 | UPD-S1-007 → UPD-S1-018 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-007/evidence/security-control-matrix.md`; `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs` |
| 54 | UPD-S1-016 → UPD-S1-018 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`; `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs` |
| 55 | UPD-S1-017 → UPD-S1-018 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs` |
| 56 | UPD-S1-009 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 57 | UPD-S1-010 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 58 | UPD-S1-011 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 59 | UPD-S1-012 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`; `tests/HostLifecycle.Tests/Program.cs` |
| 60 | UPD-S1-013 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs`; `tests/HostLifecycle.Tests/Program.cs` |
| 61 | UPD-S1-014 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 62 | UPD-S1-015 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 63 | UPD-S1-016 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 64 | UPD-S1-017 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `protocol/whitep/v1/messages.proto`; `tests/HostLifecycle.Tests/Program.cs` |
| 65 | UPD-S1-018 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt`; `tests/HostLifecycle.Tests/Program.cs` |
| 66 | UPD-S1-023 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs`; `tests/HostLifecycle.Tests/Program.cs` |
| 67 | UPD-S1-024 → UPD-S1-019 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/HostLifecycle.Tests/Program.cs` |
| 68 | UPD-S1-019 → UPD-S1-020 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `.github/workflows/product-build.yml`; `src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidWindowsLanIntegrationTest.kt` |
| 69 | UPD-S1-020 → UPD-S1-021 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `.github/workflows/android-windows-lan-acceptance.yml`; `docs/release-assets.md` |
| 70 | UPD-S1-024 → UPD-S1-021 | `hard_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `docs/release-assets.md` |
| 71 | UPD-S1-001 → UPD-S1-022 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-001/evidence/baseline-audit.md`; `tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs` |
| 72 | UPD-S1-007 → UPD-S1-022 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-007/evidence/security-control-matrix.md`; `tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs` |
| 73 | UPD-S1-024 → UPD-S1-022 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs` |
| 74 | UPD-S1-001 → UPD-S1-023 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-001/evidence/baseline-audit.md`; `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` |
| 75 | UPD-S1-005 → UPD-S1-023 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-005/evidence/resource-policy-review.md`; `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` |
| 76 | UPD-S1-006 → UPD-S1-023 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` |
| 77 | UPD-S1-024 → UPD-S1-023 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `src/Host.Windows/App.xaml`; `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` |
| 78 | UPD-S1-001 → UPD-S1-024 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-001/evidence/baseline-audit.md`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt` |
| 79 | UPD-S1-006 → UPD-S1-024 | `contract_or_interface_dependency` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-006/evidence/step-one-alignment.md`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt` |
| 80 | UPD-S1-008 → UPD-S1-024 | `sequencing_only` | `validated` | `update/Step one/Output/V1.10.json`; `update/Step three/UPD-S1-008/evidence/reconciliation-baseline-audit.md`; `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt` |

## Classification policy applied

- `hard_dependency`: the producer output is a required artifact/gate for the consumer's acceptance or execution; the consumer cannot pass without it.
- `contract_or_interface_dependency`: the producer's decision or output establishes the protocol, trust, capability, request, or host policy consumed by the target.
- `sequencing_only`: the approved graph requires this order, but no direct runtime interface or hard artifact is transferred. The `blocks` edge is preserved without calling it a technical block.
- `disputed` was not used: all 70 edges appeared in approved V1.10 prerequisite declarations and had a defensible classification supported by current source/test evidence. Where the technical link was not direct, classification was sequencing-only.

## Boundary and remaining UPD-S1-008 work

The 168 reconciliation rows now have reviewed decisions, evidence and real review commit SHAs; their legacy statuses are unchanged. This edge audit closes the edge-classification part of the work but does not by itself close cross-graph synchronization. The Step Three `update_impact` markers and legacy graph summary still need to be synchronized with the reconciliation ledger, the legacy contract tests must be updated to enforce the completed state, and both graph guards must pass on the exact resulting commit. Therefore `UPD-S1-008` remains `in_progress` in this commit.

Physical Android-to-Windows inference, final release signing/tag workflow and clean install/in-place upgrade remain independent product acceptance gates and are not claimed by this dependency audit.

# Technology decision register

This register is the entry point for technology choices and their source/test traceability. A decision marked **accepted** records the selected design; it does not imply that every dependent runtime feature has been delivered. The detailed acceptance matrix in `docs/acceptance.md` is authoritative for runtime gates.

| Decision | Scope | Source/configuration links | Test and verification links | Current evidence boundary |
|---|---|---|---|---|
| [ADR-0001 — Platform build baselines](ADR-0001-platform-build-baselines.md) | Windows WPF/.NET and Android Kotlin/Gradle build inputs | `global.json`; `src/Host.Windows/WhiteP.Host.Windows.csproj`; `src/Client.Android/app/build.gradle.kts`; `.github/workflows/product-build.yml` | `tests/test_product_build_contract.py`; `tests/test_readme_contract.py`; `tests/test_reproducible_build_contract.py` | CI builds prove the configured projects compile and publish artifacts on the referenced run, not physical-device compatibility or product runtime acceptance. |
| [ADR-0002 — Direct P2P data path](ADR-0002-direct-p2p-data-path.md) | Direct request/response is the intended data path; rendezvous is control-plane only | `docs/requirements.md` (REQ-010/REQ-011); `README.md`; Android client UI `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt` | `tests/test_p2p_data_path_decision.py`; `docs/acceptance.md` (AC-P2P-01) | No direct P2P transport is implemented. The Android UI discloses that transport is not implemented. AC-P2P-01 is **NOT RUN**. |
| [ADR-0003 — Local inference backend](ADR-0003-local-inference-backend.md) | Backend-independent host lifecycle and first llama.cpp HTTP adapter | `src/Host.Domain/HostLifecycle.cs`; `src/Host.Application/HostInferenceSession.cs`; `src/Host.Infrastructure/LlamaCppServerBackend.cs`; `src/Host.Windows/MainWindow.xaml`; `src/Host.Windows/MainWindow.xaml.cs` | `tests/HostLifecycle.Tests/Program.cs`; `.github/workflows/product-build.yml`; `docs/acceptance.md` (AC-LOCAL-01/AC-LOCAL-02) | Controlled endpoint tests and build CI passed on recorded runs; live inference against a supported model remains unverified. AC-LOCAL-01 and AC-LOCAL-02 are **NOT RUN**. |

## Traceability rules

1. Every decision must link to the configuration or source where it is reflected, or explicitly say that the code is not implemented yet.
2. Every decision must link to a relevant executable/source-contract test or build workflow. A static wording test is not a runtime transport/model test.
3. A successful build, a controlled test handler, or this register does not close a product runtime gate. Only evidence meeting the named acceptance gate can do that.
4. New decision records should be added to this register in the same commit that adds or updates their source/test links.

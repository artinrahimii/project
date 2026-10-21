# ADR-0001: Platform build baselines

- **Status:** Accepted for the current first-release build baseline; subject to change when supported-device requirements or CI evidence require it.
- **Date:** 2026-10-21
- **Related task:** T0014
- **Scope:** Pin the initial Windows and Android build toolchain inputs so CI can build the actual application skeletons reproducibly.

## Decision

### Windows host

- Use .NET SDK `10.0.100` as the selected SDK in `global.json`, with `rollForward: latestFeature` and prereleases disabled.
- Target `net10.0-windows` and use WPF in `src/Host.Windows/WhiteP.Host.Windows.csproj`.
- Build the project in GitHub Actions using the Windows runner and `dotnet build ... --configuration Release`.

### Android client

- Use JDK 17 and Gradle 8.13 in CI.
- Set `compileSdk = 36`, `targetSdk = 36`, and `minSdk = 26` in `src/Client.Android/app/build.gradle.kts`.
- Install Android platform 36, build-tools 36.0.0, and platform-tools explicitly in the product-build workflow before invoking `gradle --no-daemon assembleDebug`.

## Rationale

- The selected values are already represented in the checked-in project configuration and are consumed by the CI build entrypoints.
- Explicit SDK levels and toolchain versions make configuration drift easier to detect and reduce dependence on whatever happens to be preinstalled on a runner.
- The current workflow produces a Windows build and a debug APK; this is a development baseline, not a signed release pipeline.

## Consequences and limitations

- `minSdk = 26` defines the Android API floor for this initial build. It does not establish a tested device-compatibility matrix.
- `targetSdk = 36` and `compileSdk = 36` must be reviewed when platform policy, dependencies, or release requirements change.
- `global.json` permits latest-feature roll-forward within the selected SDK feature band; it does not pin every compiler/runtime patch byte-for-byte.
- A successful compile does not prove that the app launches on a physical device, performs local inference, or transfers data peer-to-peer.
- No claim is made here that the product meets its runtime, privacy, cryptographic, or release acceptance gates.

## Verification

- Configuration contract tests: `tests/test_product_build_contract.py`.
- README contract tests: `tests/test_readme_contract.py`.
- CI build entrypoint: `.github/workflows/product-build.yml`.
- Review the Windows host, Android debug APK, and lifecycle-test jobs in the GitHub Actions run for the commit under review. The acceptance matrix in `docs/acceptance.md` remains authoritative for runtime/release gates.

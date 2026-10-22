# T0028 — execution report

## Dependency review
- `T0027 → T0028`: validated as `sequencing_only`; the README documents workflow commands but is not consumed by the .NET workflow. The original `type: blocks` and edge direction are preserved.
- `T0020 → T0028`: validated as `contract_or_interface_dependency`; `global.json`, ADR-0001, `WhiteP.sln`, and the Windows project define the baseline that the workflow builds.
- Evidence for both reviews is recorded in `step three/project-graph.md`.

## Attempt T0028-A001 — verify existing .NET CI (succeeded)
- Inspected `.github/workflows/product-build.yml`: the `windows-host` job sets up .NET 10, runs `dotnet format whitespace WhiteP.sln --verify-no-changes`, builds `WhiteP.sln` in Release, and uploads the resulting host artifact.
- Inspected `host-lifecycle-tests`: it runs `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release`.
- Exact-commit evidence: [product build run 39743813489](https://github.com/artinrahimii/project/actions/runs/39743813489). Both the Windows host job and host lifecycle test job completed successfully.
- No duplicate workflow or source changes were needed: a real CI implementation already existed and was verified. The graph's old expected-output path (`src/Shared.Contracts/ + tests/contracts/`) did not match this task's title and has been corrected to the actual workflow/project/test paths.
- Scope limit: a successful build/lifecycle harness does not prove the production inference backend or complete product runtime.

## Result
- Status: `done` based on the checked-in workflow, real job configuration, and exact-commit passing CI evidence.
- The commit SHA for this tracker/report update is resolved from online `main` after commit creation.


## Post-commit tracker-validation correction — V 3.0.85
- Quality run [39744728550](https://github.com/artinrahimii/project/actions/runs/39744728550) failed at recorded-output/evidence validation because the graph's evidence field contained an Actions URL rather than a repository file path. The path-only evidence field has been corrected; the workflow URL remains in this report and test-result records.
- The original .NET workflow and test jobs were green on exact commit 7169cb200f3b28d12184247e2272388825690c8b. This correction does not claim to re-run the build locally.

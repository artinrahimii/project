# T0207 execution report

- Status: **in_progress**
- Attempt: `T0207-A001`
- Base commit: `86c09ff5cd222c0101a7f2b43ea038ebb569e796` (`V 3.0.942`)
- Implementation commit: pending CI trigger
- CI verification: pending; no clean-build result is claimed yet

## Incoming edge review

- `T0206 → T0207`: **validated / sequencing_only**. T0207 rebuilds from a fresh checkout and consumes no API, schema, fixture, or generated output from the rendezvous privacy tests in T0206. Keep the original `blocks` edge and conservative order.
- `T0195 → T0207`: **validated / sequencing_only**. T0207 builds the repository independently; it consumes no task-specific output from the failure-path tests in T0195. Keep the original `blocks` edge and conservative order.

## Workflow changes

Updated `.github/workflows/product-build.yml` so the fresh GitHub-hosted runners explicitly clear build outputs before compiling:

- Windows runner: `dotnet clean WhiteP.sln --configuration Release`, then `dotnet build ... --no-incremental`.
- Android runner: clean the .NET interoperability fixture, build it without incremental reuse, run Gradle `clean`, and build the debug APK with the Gradle build cache disabled for that task.
- HostLifecycle runner: `dotnet clean` before running the full lifecycle suite.

## Verification

The prior Product Build on the base commit is only baseline evidence; it does not verify the new clean steps. This task remains **in_progress** until the workflow triggered by this implementation commit completes successfully on its exact head.


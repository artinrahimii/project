# T0043 verification evidence

- Production state model: `src/Host.Domain/HostLifecycle.cs`.
- Production project: `src/Host.Domain/WhiteP.Host.Domain.csproj`.
- Executable regression harness: `tests/HostLifecycle.Tests/Program.cs`.
- Test project references the real production projects: `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.
- Assertions cover initial state, legal transitions through Stopped/Starting/Ready/Busy/Error, illegal transitions, status-message trimming and blank-message rejection.
- Exact-commit CI is required before accepting this task. No result is claimed until the GitHub Actions runs for the resulting commit finish successfully.

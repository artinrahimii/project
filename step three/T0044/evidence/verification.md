# T0044 verification evidence

- Production source: `src/Host.Domain/HostLifecycle.cs`
- Regression harness: `tests/HostLifecycle.Tests/Program.cs`
- Test project: `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`
- Coverage added: all allowed transitions and all invalid transitions across Stopped, Starting, Ready, Busy, and Error; invalid transitions must leave the current state unchanged.
- CI result for this exact revision: pending. Do not mark T0044 complete until exact-commit workflows pass.

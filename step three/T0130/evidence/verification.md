# T0130 verification evidence — attempt A001

- **Baseline commit:** 4a0715ef5465d9d94a5a4fa7d639c1f111eaeba1 (V 3.0.435), branch main.
- **Graph source:** step three/project-graph.md, Blob b9ba4d28eb0e26afd57812d70e8b76a543e7432d, fetched from the baseline tree. The response contains 2,111,024 UTF-8 bytes (1,989,126 Unicode characters); the original fenced JSON parses completely and its Git Blob SHA-1 matches the tree entry.
- **Implementation files:** src/Shared.Security/HostIdentityStore.cs, src/Host.Windows/Security/WindowsDpapiSecretProtector.cs, src/Host.Windows/Security/WindowsHostIdentityProvider.cs, portable and Windows-only acceptance tests, the Python source guard, the Windows workflow invocation, and ADR-0017.
- **Pre-publish defect checks:** corrected the two non-constant relational-pattern compile errors in the candidate store, added using Google.Protobuf for the Windows test's PrivateKey.ToByteArray() extension, and avoided a redundant ProtectedData package reference in the WPF app.
- **Tests:** not yet run locally in this tool environment. Exact-commit GitHub Actions runs are pending after push. Do not claim DPAPI verification until Windows CI emits the explicit pass line and the quality/product jobs succeed.
- **Failure policy:** malformed, unreadable or unprotectable existing identity must fail closed and must not be replaced. Concurrent first-run callers must reload the identity that wins the atomic no-overwrite operation.
- **Security boundary:** the portable test protector is reversible and test-only; it provides no confidentiality. DPAPI CurrentUser does not protect against malicious code already running as the same user. AC-P2P-01 remains NOT RUN.

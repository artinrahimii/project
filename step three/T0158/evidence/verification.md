# T0158 verification evidence

- Base commit: \`e4293fb9f5fce5bbb3aa346383ef41cc1055cf90\` (\`V 3.0.579\`), branch \`main\`.
- Graph source: \`step three/project-graph.md\`, Blob \`8a5041a47a5870815090982dfe049b0dce40c105\`; complete original UTF-8 content was byte-counted and its Git Blob SHA-1 recomputed before parsing; 229 nodes / 417 edges.
- Incoming \`T0157 → T0158\`: validated as \`contract_or_interface_dependency\` from the actual conflict-review/save contract. Its \`blocks\` edge remains intact.
- Incoming \`T0150 → T0158\`: validated as \`contract_or_interface_dependency\` from the local conversation key, Room relationship, conversation workflow and local-ID versus request-ID requirement. Its \`blocks\` edge remains intact.
- Outgoing \`T0158 → T0159\` remains unreviewed.
- Local history rows contain only source, changed field names, timestamp, schema version and byte count. A foreign key to the stored capsule supports cascade deletion; the capsule write and history insert run in the same Room transaction.
- The network \`InferenceRequest\` Protobuf is unchanged; it contains temporary request/attempt IDs and bounded capsule bytes but no permanent conversation ID or full history.
- The previous revision's Step Three quality gate, Android build/lint/JVM/Protobuf checks, Windows host build and HostLifecycle tests all passed on \`V 3.0.579\`. The new T0158 exact-head CI has not yet run at report creation.
- No local Android/Gradle/Python run is claimed.

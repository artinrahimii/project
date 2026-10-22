# Shared contracts project boundary

This directory establishes the planned source boundary for platform-neutral .NET contract types.

- The canonical wire schema remains `protocol/whitep/v1/messages.proto`. Do not copy the schema into this directory or hand-maintain a second version.
- The Windows host and Android client are separate runnable projects under `src/Host.Windows/` and `src/Client.Android/`.
- The project/solution files and buildable .NET project are owned by task T0022. Generated contract code and cross-platform code-generation wiring are owned by the corresponding generation task (T0036).
- This file records the directory boundary for T0021; it is not a substitute for the shared project, generated classes, or cross-language compatibility tests.

The existing schema and source contract checks remain under `protocol/whitep/v1/messages.proto` and `tests/test_proto_contract.py` until the graph's later generation/build tasks integrate the shared project.

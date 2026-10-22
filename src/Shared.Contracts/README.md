# Shared contracts project boundary

This directory contains the buildable `WhiteP.Shared.Contracts.csproj` library targeting `net10.0`, included in the root `WhiteP.sln` alongside the Windows WPF host. It is the platform-neutral .NET contract boundary.

- The canonical wire schema remains `protocol/whitep/v1/messages.proto`. Do not copy the schema into this directory or hand-maintain a second version.
- The Windows host and Android client are separate runnable projects under `src/Host.Windows/` and `src/Client.Android/`.
- The library project, host project reference, and root solution are delivered by task T0022. Generated contract code and cross-platform code-generation wiring are owned by the corresponding generation task (T0036).
- This file documents the project boundary; it is not a substitute for generated classes or cross-language compatibility tests.

The existing schema and source contract checks remain under `protocol/whitep/v1/messages.proto` and `tests/test_proto_contract.py` until the graph's later generation/build tasks integrate the shared project.

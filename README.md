# gto-diagnostics

Open-source diagnostic and datalogging software for Mk1 Mitsubishi GTO / 3000GT / Dodge Stealth vehicles using the 1990-1993 OBD1-style diagnostic system.

The initial target is Linux-first development in C#/.NET, engine ECU read-only diagnostics, raw traffic capture, replay/simulation support, and a UI layer once the core protocol and serial foundations are stable.

## Project structure

```text
gto-diagnostics/
  docs/
  src/
    GtoDiagnostics.App/
    GtoDiagnostics.Core/
    GtoDiagnostics.Protocol/
    GtoDiagnostics.Serial/
    GtoDiagnostics.Simulator/
  tests/
    GtoDiagnostics.Core.Tests/
    GtoDiagnostics.Protocol.Tests/
    GtoDiagnostics.E2E.Tests/
```

## Current status

This is the first scaffold. It includes:

- module and sensor definition models
- protocol hex helpers and raw JSONL capture writer
- serial device discovery for common Linux USB serial device paths
- hardware-free scripted byte transport for simulator/replay testing
- small executable test projects with no external test package dependency

## Build

```bash
DOTNET_CLI_HOME=/tmp dotnet build GtoDiagnostics.sln -m:1
```

## Run hardware-free checks

```bash
DOTNET_CLI_HOME=/tmp dotnet run --project tests/GtoDiagnostics.Core.Tests/GtoDiagnostics.Core.Tests.csproj
DOTNET_CLI_HOME=/tmp dotnet run --project tests/GtoDiagnostics.Protocol.Tests/GtoDiagnostics.Protocol.Tests.csproj
DOTNET_CLI_HOME=/tmp dotnet run --project tests/GtoDiagnostics.E2E.Tests/GtoDiagnostics.E2E.Tests.csproj
```

## CLI starter

```bash
DOTNET_CLI_HOME=/tmp dotnet run --project src/GtoDiagnostics.App/GtoDiagnostics.App.csproj -- ports
DOTNET_CLI_HOME=/tmp dotnet run --project src/GtoDiagnostics.App/GtoDiagnostics.App.csproj -- simulate-capture captures/example.jsonl
```

# Development Setup

Requirements:

- Linux desktop as the primary development environment
- .NET SDK
- VS Code with the recommended C# extensions

Useful commands:

```bash
DOTNET_CLI_HOME=/tmp dotnet build GtoDiagnostics.sln -m:1
DOTNET_CLI_HOME=/tmp dotnet run --project src/GtoDiagnostics.App/GtoDiagnostics.App.csproj
```

On Linux, USB serial access may require membership of the `dialout` group depending on the distribution and adapter.

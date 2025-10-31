# Package Publishing

Quick guide for publishing .NET projects in this codebase to OutSystems-compatible packages.

## Quick Start

### Publish a Project

```cmd
publish.cmd MathRound
```

Or use PowerShell directly:

```powershell
.\generate_upload_package.ps1 -ProjectName MathRound
```

This creates `MathRound/bin/Release/MathRound.zip` (Release build, linux-x64 runtime).

### With Custom Output Name

```cmd
publish.cmd MathRound Release MathRound-v1.0.0.zip
```

### Debug Build

```cmd
publish.cmd MathRound Debug
```

## PowerShell Options

For more control, use the PowerShell script with parameters:

```powershell
# Custom output name
.\generate_upload_package.ps1 -ProjectName MathRound -OutputName MyPackage.zip

# Debug configuration
.\generate_upload_package.ps1 -ProjectName MathRound -Configuration Debug

# Self-contained (includes .NET runtime)
.\generate_upload_package.ps1 -ProjectName MathRound -SelfContained $true

# Combine options
.\generate_upload_package.ps1 `
    -ProjectName MathRound `
    -Configuration Release `
    -OutputName MathRound-v1.0.0.zip
```

## Parameters

| Parameter         | Required | Default             | Description                                     |
| ----------------- | -------- | ------------------- | ----------------------------------------------- |
| `ProjectName`     | ✅ Yes   | -                   | Project folder name (e.g., `MathRound`)         |
| `Configuration`   | No       | `Release`           | Build configuration (`Release` or `Debug`)      |
| `OutputName`      | No       | `{ProjectName}.zip` | Output zip file name                            |
| `SelfContained`   | No       | `$false`            | Include .NET runtime in package                 |
| `TargetFramework` | No       | Auto-detected       | Target framework (auto-detected from `.csproj`) |

**Note:** Runtime is always `linux-x64` for OutSystems compatibility.

## Adding New Projects

The script automatically works with any .NET project in the codebase:

```cmd
publish.cmd YourNewProject
```

## Troubleshooting

### Execution Policy Error

If you get a PowerShell execution policy error:

```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

### Project Not Found

Ensure you're running from the repository root and the project name matches the folder name exactly.

## Prerequisites

- .NET SDK
- PowerShell (built into Windows)

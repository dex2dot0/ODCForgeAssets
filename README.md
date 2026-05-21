# Forge Assets Library

A collection of different projects that expose Forge Assets to OutSystems.

## Issues

If you find any issues with the libraries, please report them in the [Issues](https://github.com/dex2dot0/ODCForgeAssets/issues) section.

## Feature Requests

If you have any feature requests, please add them to the [Issues](https://github.com/dex2dot0/ODCForgeAssets/issues) section.

## Contributing

If you would like to contribute to the project, please fork the repository and create a pull request.

## Code Standards

- All code should be written in C# and be compatible with .NET 8.0.
- Code comments are strongly encouraged.
- All code should include tests as appropriate.
- All projects should include a README.md file with information about the project.

## Included Tooling

### Kiota OpenAPI to OutSystems Generator

`KiotaOutSystems.Generator`: a dev-time generator that can emit OutSystems wrapper source from either a local OpenAPI file or a remote spec URL. With this generator, you can:

- read an OpenAPI document (local or remote)
- bind to Kiota client metadata
- emit OutSystems wrapper source (structures, server actions, manifest)

See [KiotaOutSystems.Generator/README.md](KiotaOutSystems.Generator/README.md) for more details.

#### Why?

- Save on AOs for OpenAPI backed APIs
- Publish external logic as your API integration layer with versioning
- Maintain API integrations in code and back with source control

### Project Icons

OutSystems external libraries need a logo to publish successfully (to Forge). In this repo, the supported pattern is:

- store the PNG under `resources/`
- embed it via `<EmbeddedResource Include="resources\\...png" />`
- reference it from the single `OSInterface` using `IconResourceName`

Use the repo command from the repository root, for example:

```cmd
icon.cmd UUIDGenerator --icon UUIDGenerator\resources\uuid_generator.png
```

Or use PowerShell directly:

```powershell
.\setup-project-icon.ps1 -ProjectName UUIDGenerator -IconPath .\UUIDGenerator\resources\uuid_generator.png
```

To use the a generic icon

```cmd
icon.cmd UUIDGenerator --placeholder
```

```powershell
.\setup-project-icon.ps1 -ProjectName UUIDGenerator -ScaffoldPlaceholder
```

For Kiota generator-backed projects, the command updates the stable generator config so the icon survives regeneration.

### Packaging

A convenience script is provided to package the projects for deployment to OutSystems. See [PACKAGING.md](PACKAGING.md) for more details.

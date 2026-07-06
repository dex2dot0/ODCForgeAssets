# KiotaOutSystems.Generator

A developer tool that reads an OpenAPI document, binds to Kiota client metadata, and emits OutSystems wrapper source files (structures, server actions, manifest).

## Pre-requisites

- .NET SDK installed so you can run `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet run`
- PowerShell or `cmd` available from the repository root
- an OpenAPI spec, either:
  - a local `.json`, `.yaml`, or `.yml` file, or
  - an HTTP/HTTPS URL that returns the spec document

## Getting Started

Use this workflow when you want to create a new OutSystems external-library project backed by an OpenAPI API spec and generate the wrapper code yourself.

1. Scaffold the project from the repository root.

```powershell
$projectName = "MyApiProject"
.\setup-kiota-project.ps1 -ProjectName $projectName
```

By default, the script:

- creates the class library project
- adds it to `ForgeAssets.sln`
- removes the default `Class1.cs`
- adds the required package references
- creates the `Client/`, `Generated/`, and `Specs/` folders
- scaffolds a starter `outsystems-generator.json`

Useful options:

```powershell
.\setup-kiota-project.ps1 `
  -ProjectName "MyApiProject" `
  -InputName "My API" `
  -ClientClassName "MyApiClient" `
  -GeneratedClassName "MyApiActionsGenerated" `
  -GeneratedInterfaceName "IMyApiActionsGenerated"
```

Use `-SkipSolutionAdd` if you want to scaffold the project without adding it to `ForgeAssets.sln`.

2. Put your OpenAPI description in the project.

- If you already have a local spec file, copy it under `$projectName/Specs/`.
- If you will generate directly from a remote URL later, you can skip the local file copy, but you will still usually want a checked-in spec for repeatability.

3. Generate the Kiota client.

If this repo does not already have the Kiota local tool restored, run:

```powershell
dotnet tool restore
```

For a local spec file:

```powershell
dotnet tool run kiota generate `
  -l CSharp `
  -c MyApiClient `
  -n "$projectName.Client" `
  -d ".\$projectName\Specs\openapi.yml" `
  -o ".\$projectName\Client"
```

For a remote spec URL:

```powershell
dotnet tool run kiota generate `
  -l CSharp `
  -c MyApiClient `
  -n "$projectName.Client" `
  -d "https://example.com/openapi.json" `
  -o ".\$projectName\Client"
```

This produces the Kiota client code plus the `kiota-lock.json` file that the OutSystems generator can reuse.

The scaffold script already creates a starter `outsystems-generator.json`. Add action, structure, field, icon, or endpoint targeting overrides later as needed. For a full sample, see `JsonPlaceholderKiota/outsystems-generator.json`.

4. Run the OutSystems generator.

For a local spec file:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config ".\$projectName\outsystems-generator.json" `
  --spec ".\$projectName\Specs\openapi.yml" `
  --output ".\$projectName\Generated" `
  --kiota-lock ".\$projectName\Client\kiota-lock.json"
```

For a remote spec URL:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config ".\$projectName\outsystems-generator.json" `
  --spec "https://example.com/openapi.json" `
  --output ".\$projectName\Generated" `
  --kiota-lock ".\$projectName\Client\kiota-lock.json"
```

The generator writes:

- `GeneratedStructures.g.cs`
- `GeneratedActions.g.cs`
- `generation-manifest.json`

5. Add an icon before packaging/publishing.

For a placeholder:

```cmd
icon.cmd MyApiProject --placeholder
```

For an existing PNG:

```cmd
icon.cmd MyApiProject --icon path\to\icon.png
```

For generator-backed projects, this updates `outsystems-generator.json` so `IconResourceName` survives regeneration.

6. Review the generated surface and regenerate as needed.

- If your assembly should expose only the generated OutSystems interface, do not add a second handwritten `OSInterface`.
- If you do want a handwritten facade instead, run the generator with `--emit-interface false`.
- Re-run Kiota when the source API changes.
- Re-run this generator whenever the spec, config, or icon metadata changes.

## Inputs

The generator accepts either:

- a local OpenAPI spec file path
- an HTTP/HTTPS URL that returns an OpenAPI document

For Kiota binding, pass a `kiota-lock.json` file so the generator can reuse the generated client namespace and client class name.

You can also pass a generator config file with `--config` to centralize:

- interface/class/namespace naming
- `emitInterface` behavior
- endpoint/method targeting
- action name/description overrides
- structure and field name/description overrides
- icon metadata for generated `OSInterface` output

Generator config example:

```json
{
	"namespace": "JsonPlaceholderKiota.Generated",
	"className": "JsonPlaceholderPostsGenerated",
	"interfaceName": "IJsonPlaceholderPostsGenerated",
	"inputName": "JSONPlaceholder",
	"targets": [
		{
			"path": "/posts*",
			"methods": ["GET", "POST", "PATCH", "DELETE"]
		}
	],
	"icon": {
		"fileName": "ProjectIcon.png",
		"resourceName": "JsonPlaceholderKiota.resources.ProjectIcon.png"
	}
}
```

For a fuller example, see [JsonPlaceholderKiota/outsystems-generator.json](JsonPlaceholderKiota/outsystems-generator.json).

## Target specific endpoints

Use `targets` when you want to generate only part of a spec instead of the full API surface.

- If `targets` is omitted, the generator keeps the current all-endpoints behavior.
- `path` supports exact matches such as `"/users"` or a trailing `*` prefix wildcard such as `"/pet*"`.
- `methods` is optional. If omitted, all HTTP methods for the matched path are considered.
- Multiple targets are grouped together as an OR.

Example:

```json
{
	"targets": [
		{
			"path": "/pet*",
			"methods": ["GET", "POST", "PUT", "DELETE"]
		}
	]
}
```

The generator keeps the selected actions plus any OutSystems request/response structures they still need. For the Petstore example, targeting `"/pet*"` excludes `/store/*` and `/user/*` actions while retaining supporting models such as `Pet`, `Category`, and `Tag`.

## Runtime request options

Generated actions now take a shared `RequestOptions` input as their first parameter. This keeps the external library stateless while letting OutSystems provide runtime connection details such as the API base URL and authentication values.

The generated `RequestOptions` structure always includes:

- `BaseUrl` to override the default URL derived from the OpenAPI document

It also includes auth fields derived from the security schemes used by the generated operations, for example:

- `ApiKey` for a single API-key scheme
- `Username` and `Password` for HTTP basic auth
- `AccessToken` for bearer, OAuth2, or OpenID Connect token-based auth

Example generated action shape:

```csharp
RealtimeWeather(RequestOptions requestOptions, string q, string lang = "", bool includeLang = false)
```

If the operation requires authentication and the required values are not present in `RequestOptions`, the generated action throws an `InvalidOperationException` before making the downstream request.

## OutSystems interface emission

OutSystems packages can expose only one interface decorated with `OSInterface` per assembly. By default, the generator emits that interface and the corresponding `OSAction` members.

If you already maintain a handwritten OutSystems facade in the same project and only want generated helper/runtime code, pass:

```powershell
--emit-interface false
```

That mode suppresses the generated `OSInterface` and generated interface block, leaving only the generated class, mappers, structures, and manifest.

## Optional query parameters in OutSystems

OutSystems simple input types default to values like `0` and `""`, so optional query parameters cannot reliably depend on `null` alone to mean "do not send this filter".

For optional query parameters, the generator emits a companion Boolean flag. For example, an optional `userId` filter becomes:

```csharp
ListPosts(int userId = 0, bool includeUserId = false, string title = "", bool includeTitle = false)
```

Only parameters whose `include...` flag is `true` are sent to the downstream API. This avoids accidental empty/default filters such as `?title=&userId=0`.

## Request/response structures

The generator emits shared OutSystems structures for component schemas and operation-specific request structures for JSON bodies when needed. For example, a `PATCH /posts/{id}` body becomes a generated `UpdatePostRequest` structure so request contracts can evolve independently from response contracts.

## Validation

The generator validates a few OutSystems-specific constraints before writing files:

- duplicate generated names after normalization
- emitting an `OSInterface` into a project that already contains another handwritten `OSInterface`
- unsupported schema shapes that cannot be mapped cleanly into the SDK surface

## Example

Generate wrapper files from a local spec:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config "./JsonPlaceholderKiota/outsystems-generator.json" `
  --spec "./JsonPlaceholderKiota/Specs/posts-api.yml" `
  --output "./JsonPlaceholderKiota/Generated" `
  --kiota-lock "./JsonPlaceholderKiota/Client/kiota-lock.json"
```

Generate wrapper files from a remote spec:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config "./JsonPlaceholderKiota/outsystems-generator.json" `
  --spec "https://example.invalid/posts-api.yml" `
  --output "./JsonPlaceholderKiota/Generated" `
  --kiota-lock "./JsonPlaceholderKiota/Client/kiota-lock.json"
```

## Output

The generator currently emits:

- `GeneratedStructures.g.cs`
- `GeneratedActions.g.cs`
- `generation-manifest.json`

## Icons for generated projects

Generated projects should keep icon metadata in the generator config rather than patching generated code directly. The repo helper command updates that config for you.

Example config fragment:

```json
{
	"icon": {
		"fileName": "ProjectIcon.png",
		"resourceName": "JsonPlaceholderKiota.resources.ProjectIcon.png"
	}
}
```

When present, the generator emits `IconResourceName` on the generated `OSInterface`.

## Testing

Run `dotnet test KiotaOutSystems.Generator.Tests/KiotaOutSystems.Generator.Tests.csproj`.

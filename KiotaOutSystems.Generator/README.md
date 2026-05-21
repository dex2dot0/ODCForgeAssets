# KiotaOutSystems.Generator

A developer tool that reads an OpenAPI document, binds to Kiota client metadata, and emits OutSystems wrapper source files (structures, server actions, manifest).

## Inputs

The generator accepts either:

- a local OpenAPI spec file path
- an HTTP/HTTPS URL that returns an OpenAPI document

For Kiota binding, pass a `kiota-lock.json` file so the generator can reuse the generated client namespace and client class name.

You can also pass a generator config file with `--config` to centralize:

- interface/class/namespace naming
- `emitInterface` behavior
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
	"icon": {
		"fileName": "ProjectIcon.png",
		"resourceName": "JsonPlaceholderKiota.resources.ProjectIcon.png"
	}
}
```

For a fuller example, see [JsonPlaceholderKiota/outsystems-generator.json](JsonPlaceholderKiota/outsystems-generator.json).

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

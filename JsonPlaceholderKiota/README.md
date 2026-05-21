# JsonPlaceholderKiota

A sample OutSystems external library that wraps a Kiota-generated client for the JSONPlaceholder `/posts` API.

## Regenerating the Kiota client

Run the generator workflow from the repository root:

```powershell
dotnet tool restore
dotnet tool run kiota generate -l CSharp -c PostsClient -n JsonPlaceholderKiota.Client -d "./JsonPlaceholderKiota/Specs/posts-api.yml" -o "./JsonPlaceholderKiota/Client"
```

## Regenerating the OutSystems wrappers

Generate the wrapper source from the local spec file:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config "./JsonPlaceholderKiota/outsystems-generator.json" `
  --spec "./JsonPlaceholderKiota/Specs/posts-api.yml" `
  --output "./JsonPlaceholderKiota/Generated" `
  --kiota-lock "./JsonPlaceholderKiota/Client/kiota-lock.json"
```

Generate the same wrapper source from a remote spec URL:

```powershell
dotnet run --project KiotaOutSystems.Generator -- `
  --config "./JsonPlaceholderKiota/outsystems-generator.json" `
  --spec "https://example.invalid/posts-api.yml" `
  --output "./JsonPlaceholderKiota/Generated" `
  --kiota-lock "./JsonPlaceholderKiota/Client/kiota-lock.json"
```

If your assembly already has a handwritten OutSystems `OSInterface`, add `--emit-interface false` so the generator does not create a second one.

For optional query filters such as `ListPosts`, the generated OutSystems action now includes companion Boolean flags like `includeUserId` and `includeTitle`. In OutSystems, set those flags to `True` only when you actually want that filter sent to the API.

The generator also emits operation-specific request structures such as `CreatePostRequest` and `UpdatePostRequest`, which are better matches for OutSystems contracts than reusing response DTOs for every body payload.

## Icon Setup

Because this project is generator-backed, use the repo helper to manage the icon so the `IconResourceName` metadata lives in `outsystems-generator.json` and survives regeneration:

```cmd
icon.cmd JsonPlaceholderKiota --placeholder
```

Or wire an existing PNG:

```cmd
icon.cmd JsonPlaceholderKiota --icon path\to\icon.png
```

## Testing

To run the sample library tests, run `dotnet test JsonPlaceholderKiota.Tests/JsonPlaceholderKiota.Tests.csproj`.

## Packaging

To package the sample library for OutSystems, run `publish.cmd JsonPlaceholderKiota`.

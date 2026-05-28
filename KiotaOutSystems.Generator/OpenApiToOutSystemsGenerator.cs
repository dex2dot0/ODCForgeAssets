using System.Text.Json;

namespace KiotaOutSystems.Generator;

internal static class OpenApiToOutSystemsGenerator
{
    public static GenerationResult Generate(GeneratorOptions options, LoadedSpec loadedSpec)
    {
        var config = GeneratorConfigLoader.Load(options.ConfigPath);
        var resolvedOptions = ResolveOptions(options, config);
        var kiotaMetadata = KiotaMetadataLoader.Load(resolvedOptions);
        var generationNamespace = resolvedOptions.Namespace ?? "Generated.OutSystems";
        var className = resolvedOptions.ClassName ?? "GeneratedApiActions";
        var interfaceName = resolvedOptions.InterfaceName ?? $"I{className}";
        var inputName = resolvedOptions.InputName ?? loadedSpec.Document.Info?.Title ?? "Generated API";
        var emitInterface = resolvedOptions.EmitInterface ?? true;
        var iconResourceName = config.Icon?.ResourceName;
        var normalizedDocument = OpenApiDocumentNormalizer.Normalize(loadedSpec, kiotaMetadata, config);

        ValidateGenerationTarget(resolvedOptions.OutputDirectory, emitInterface);
        ValidateNoNormalizedNameCollisions(normalizedDocument.Schemas, normalizedDocument.Operations);

        Directory.CreateDirectory(resolvedOptions.OutputDirectory);

        var generatedFiles = new List<string>
        {
            WriteFile(
                resolvedOptions.OutputDirectory,
                "GeneratedStructures.g.cs",
                OutSystemsCodeRenderer.RenderStructures(generationNamespace, normalizedDocument.Schemas)),
            WriteFile(
                resolvedOptions.OutputDirectory,
                "GeneratedActions.g.cs",
                OutSystemsCodeRenderer.RenderActions(
                    generationNamespace,
                    className,
                    interfaceName,
                    inputName,
                    normalizedDocument.Schemas,
                    normalizedDocument.Operations,
                    kiotaMetadata,
                    emitInterface,
                    iconResourceName))
        };

        var manifest = new
        {
            inputName,
            source = loadedSpec.ResolvedSource,
            specHash = loadedSpec.ContentHash,
            specificationVersion = loadedSpec.SpecificationVersion,
            kiotaMetadata.ClientNamespace,
            kiotaMetadata.ClientClassName,
            kiotaMetadata.KiotaVersion,
            emitInterface,
            iconResourceName,
            structures = normalizedDocument.Schemas.Select(schema => schema.Name).ToArray(),
            operations = normalizedDocument.Operations.Select(operation => operation.Name).ToArray()
        };

        generatedFiles.Add(WriteFile(
            resolvedOptions.OutputDirectory,
            "generation-manifest.json",
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true })));

        return new GenerationResult(generatedFiles, normalizedDocument.Operations.Count, normalizedDocument.Schemas.Count);
    }

    private static string WriteFile(string outputDirectory, string fileName, string content)
    {
        var filePath = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static GeneratorOptions ResolveOptions(GeneratorOptions options, EffectiveGeneratorConfig config)
    {
        return new GeneratorOptions
        {
            SpecSource = options.SpecSource,
            OutputDirectory = options.OutputDirectory,
            Namespace = options.Namespace ?? config.Namespace,
            ClientNamespace = options.ClientNamespace ?? config.ClientNamespace,
            ClientClassName = options.ClientClassName ?? config.ClientClassName,
            InterfaceName = options.InterfaceName ?? config.InterfaceName,
            ClassName = options.ClassName ?? config.ClassName,
            KiotaLockPath = options.KiotaLockPath,
            InputName = options.InputName ?? config.InputName,
            EmitInterface = options.EmitInterface ?? config.EmitInterface ?? true,
            ConfigPath = options.ConfigPath
        };
    }

    private static void ValidateGenerationTarget(string outputDirectory, bool emitInterface)
    {
        if (!emitInterface)
        {
            return;
        }

        var projectRoot = FindProjectRoot(outputDirectory);
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            return;
        }

        var csFiles = Directory.EnumerateFiles(projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.StartsWith(Path.GetFullPath(outputDirectory), StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var csFile in csFiles)
        {
            var content = File.ReadAllText(csFile);
            if (content.Contains("[OSInterface", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"An existing OSInterface was found in '{csFile}'. OutSystems allows only one OSInterface per assembly. Re-run with --emit-interface false or remove the handwritten interface.");
            }
        }
    }

    private static string? FindProjectRoot(string outputDirectory)
    {
        var current = Directory.GetParent(Path.GetFullPath(outputDirectory));
        while (current is not null)
        {
            if (Directory.EnumerateFiles(current.FullName, "*.csproj", SearchOption.TopDirectoryOnly).Any() ||
                Directory.EnumerateFiles(current.FullName, "*.sln", SearchOption.TopDirectoryOnly).Any())
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static void ValidateNoNormalizedNameCollisions(IReadOnlyList<SchemaDefinition> schemas, IReadOnlyList<OperationDefinition> operations)
    {
        ValidateUniqueNames("structure", schemas.Select(schema => schema.Name));
        ValidateUniqueNames("operation", operations.Select(operation => operation.Name));

        foreach (var schema in schemas)
        {
            ValidateUniqueNames($"field in {schema.Name}", schema.Properties.Select(property => property.Name));
        }
    }

    private static void ValidateUniqueNames(string kind, IEnumerable<string> names)
    {
        var duplicate = names
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"A duplicate generated {kind} name was found: '{duplicate.Key}'. Add a config override to disambiguate the generated names.");
        }
    }
}

internal sealed record GenerationResult(IReadOnlyList<string> Files, int OperationCount, int StructureCount);

internal static class KiotaMetadataLoader
{
    public static KiotaMetadata Load(GeneratorOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.KiotaLockPath))
        {
            if (string.IsNullOrWhiteSpace(options.ClientNamespace) || string.IsNullOrWhiteSpace(options.ClientClassName))
            {
                throw new InvalidOperationException("Provide either --kiota-lock or both --client-namespace and --client-class.");
            }

            return new KiotaMetadata(options.ClientNamespace, options.ClientClassName, null, null, null);
        }

        var lockPath = Path.GetFullPath(options.KiotaLockPath);
        using var document = JsonDocument.Parse(File.ReadAllText(lockPath));
        var root = document.RootElement;

        var clientNamespace = options.ClientNamespace ?? root.GetProperty("clientNamespaceName").GetString();
        var clientClassName = options.ClientClassName ?? root.GetProperty("clientClassName").GetString();
        var descriptionLocation = root.TryGetProperty("descriptionLocation", out var descriptionLocationValue)
            ? descriptionLocationValue.GetString()
            : null;
        var kiotaVersion = root.TryGetProperty("kiotaVersion", out var kiotaVersionValue)
            ? kiotaVersionValue.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(clientNamespace) || string.IsNullOrWhiteSpace(clientClassName))
        {
            throw new InvalidOperationException("The Kiota lock file did not contain a client namespace or client class name.");
        }

        return new KiotaMetadata(clientNamespace, clientClassName, descriptionLocation, kiotaVersion, Path.GetDirectoryName(lockPath));
    }
}

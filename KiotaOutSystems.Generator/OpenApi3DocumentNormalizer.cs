using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;

namespace KiotaOutSystems.Generator;

internal static class OpenApi3DocumentNormalizer
{
    public static NormalizedOpenApiDocument Normalize(OpenApiDocument document, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        var schemas = BuildSchemas(document, kiotaMetadata, config);
        var operations = BuildOperations(document, schemas, config, kiotaMetadata);
        var runtimeContext = BuildRuntimeContext(document, schemas, operations);
        var compatibleDocument = ApplyOutSystemsCompatibility(schemas, operations, runtimeContext);
        return config.Targets.Count > 0
            ? PruneUnreachableSchemas(compatibleDocument)
            : compatibleDocument;
    }

    private static IReadOnlyList<SchemaDefinition> BuildSchemas(OpenApiDocument document, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        var schemas = new List<SchemaDefinition>();
        IDictionary<string, IOpenApiSchema> rawComponentSchemas = document.Components?.Schemas ?? new Dictionary<string, IOpenApiSchema>();
        var schemaTypeNamesByReference = new Dictionary<IOpenApiSchema, string>(ReferenceEqualityComparer.Instance);
        var schemaTypeNamesByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var componentSchemaNamesByReference = new Dictionary<IOpenApiSchema, string>(ReferenceEqualityComparer.Instance);
        var usedStructureNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var kiotaTypeHintsBySchemaId = BuildKiotaTypeHints(document, kiotaMetadata);
        var modelTypeLookup = BuildKiotaModelTypeLookup(kiotaMetadata);

        var componentSchemas = new List<(string SchemaName, OpenApiSchema ConcreteSchema, string DefaultStructureName, string StructureName, string Description, string? KiotaModelType, bool IsRequestShape)>();
        foreach (var (schemaName, schema) in rawComponentSchemas)
        {
            var concreteSchema = ResolveConcreteSchema(schema, $"component schema '{schemaName}'");
            if (string.IsNullOrWhiteSpace(concreteSchema.Id))
            {
                concreteSchema.Id = schemaName;
            }

            if (HasSchemaType(concreteSchema, JsonSchemaType.Array) && concreteSchema.Items is not null)
            {
                RegisterNestedInlineObjectSchema(
                    schemaName,
                    "Item",
                    concreteSchema.Items,
                    componentSchemas,
                    schemaTypeNamesByReference,
                    schemaTypeNamesByName,
                    usedStructureNames);
            }

            if (!HasSchemaType(concreteSchema, JsonSchemaType.Object))
            {
                continue;
            }

            var defaultStructureName = OpenApiNamingPolicy.CreateStructureName(schemaName);
            var structureOverride = FindStructureOverride(config, schemaName, defaultStructureName);
            var structureName = AllocateUniqueName(structureOverride?.Name ?? defaultStructureName, usedStructureNames);
            schemaTypeNamesByReference[schema] = structureName;
            schemaTypeNamesByReference[concreteSchema] = structureName;
            schemaTypeNamesByName[schemaName] = structureName;
            if (!string.IsNullOrWhiteSpace(concreteSchema.Title))
            {
                schemaTypeNamesByName[concreteSchema.Title] = structureName;
            }

            componentSchemaNamesByReference[schema] = schemaName;
            componentSchemaNamesByReference[concreteSchema] = schemaName;
            componentSchemas.Add((
                schemaName,
                concreteSchema,
                defaultStructureName,
                structureName,
                structureOverride?.Description ?? $"Represents the {schemaName} schema from the source OpenAPI document.",
                ResolveKiotaModelType(schemaName, concreteSchema.Title, kiotaMetadata, kiotaTypeHintsBySchemaId, modelTypeLookup),
                false));
        }

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                if (operation.RequestBody is null || !TryGetSupportedRequestMediaType(operation.RequestBody, out var mediaType, out _))
                {
                    continue;
                }

                var requestSchema = mediaType.Schema;
                if (requestSchema is null)
                {
                    continue;
                }

                var requestSchemaId = TryGetReferenceId(requestSchema);
                var componentSchema = ResolveConcreteSchema(requestSchema, $"request body schema for '{operation.OperationId ?? path}'");
                if (!HasSchemaType(componentSchema, JsonSchemaType.Object))
                {
                    continue;
                }

                var resourcePropertyName = OpenApiNamingPolicy.CreateStructureName(path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "resource");
                var defaultOperationName = CreateOperationName(operationType, path, resourcePropertyName, operation.OperationId);
                var actionOverride = FindActionOverride(config, operationType, path, defaultOperationName);
                var operationName = actionOverride?.Name ?? defaultOperationName;

                var syntheticSchemaName = $"{operationName}Request";
                if (componentSchemas.Any(schema => string.Equals(schema.SchemaName, syntheticSchemaName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                requestSchemaId ??= syntheticSchemaName;
                componentSchema.Title = requestSchemaId;

                var structureOverride = FindStructureOverride(config, syntheticSchemaName, syntheticSchemaName);
                var structureName = AllocateUniqueName(structureOverride?.Name ?? syntheticSchemaName, usedStructureNames);
                schemaTypeNamesByReference[requestSchema] = structureName;
                schemaTypeNamesByReference[componentSchema] = structureName;
                schemaTypeNamesByName[syntheticSchemaName] = structureName;
                schemaTypeNamesByName[requestSchemaId] = structureName;
                componentSchemas.Add((
                    syntheticSchemaName,
                    componentSchema,
                    syntheticSchemaName,
                    structureName,
                    structureOverride?.Description ?? NormalizeDescription($"Represents the request body for {operationName}."),
                    OpenApiSchemaLoweringPass.IsProxySchema(componentSchema)
                        ? null
                        : TryGetReferenceId(requestSchema) is not null
                            ? $"{kiotaMetadata.ClientNamespace}.Models.{OpenApiNamingPolicy.CreateKiotaModelTypeName(requestSchemaId)}"
                            : CreateKiotaRequestBodyTypeName(kiotaMetadata, path, operationType),
                    true));
            }
        }

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                if (!TryGetPrimaryResponseSchema(operation.Responses, out var responseSchema))
                {
                    continue;
                }

                var concreteResponseSchema = ResolveConcreteSchema(responseSchema, $"response schema for '{operation.OperationId ?? path}'");
                var responseItemSchema = HasSchemaType(concreteResponseSchema, JsonSchemaType.Array) ? concreteResponseSchema.Items : concreteResponseSchema;
                if (responseItemSchema is null)
                {
                    continue;
                }

                var concreteResponseItemSchema = ResolveConcreteSchema(responseItemSchema, $"response item schema for '{operation.OperationId ?? path}'");
                if (!IsObjectLikeSchema(concreteResponseItemSchema) || TryGetReferenceId(responseItemSchema) is not null)
                {
                    continue;
                }

                var resourcePropertyName = OpenApiNamingPolicy.CreateStructureName(path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "resource");
                var defaultOperationName = CreateOperationName(operationType, path, resourcePropertyName, operation.OperationId);
                var actionOverride = FindActionOverride(config, operationType, path, defaultOperationName);
                var operationName = actionOverride?.Name ?? defaultOperationName;
                var syntheticSchemaName = HasSchemaType(concreteResponseSchema, JsonSchemaType.Array)
                    ? $"{operationName}Item"
                    : $"{operationName}Response";

                if (schemaTypeNamesByReference.ContainsKey(responseItemSchema) || schemaTypeNamesByReference.ContainsKey(concreteResponseItemSchema))
                {
                    continue;
                }

                concreteResponseItemSchema.Title = syntheticSchemaName;

                var structureOverride = FindStructureOverride(config, syntheticSchemaName, syntheticSchemaName);
                var structureName = AllocateUniqueName(structureOverride?.Name ?? syntheticSchemaName, usedStructureNames);
                schemaTypeNamesByReference[responseItemSchema] = structureName;
                schemaTypeNamesByReference[concreteResponseItemSchema] = structureName;
                schemaTypeNamesByName[syntheticSchemaName] = structureName;
                componentSchemas.Add((
                    syntheticSchemaName,
                    concreteResponseItemSchema,
                    syntheticSchemaName,
                    structureName,
                    structureOverride?.Description ?? NormalizeDescription(
                        HasSchemaType(concreteResponseSchema, JsonSchemaType.Array)
                            ? $"Represents an item in the response body for {operationName}."
                            : $"Represents the response body for {operationName}."),
                    CreateKiotaResponseTypeName(kiotaMetadata, path),
                    false));
            }
        }

        for (var index = 0; index < componentSchemas.Count; index++)
        {
            var parentSchema = componentSchemas[index];
            if (HasSchemaType(parentSchema.ConcreteSchema, JsonSchemaType.Array) && parentSchema.ConcreteSchema.Items is not null)
            {
                RegisterNestedInlineObjectSchema(
                    parentSchema.SchemaName,
                    "Item",
                    parentSchema.ConcreteSchema.Items,
                    componentSchemas,
                    schemaTypeNamesByReference,
                    schemaTypeNamesByName,
                    usedStructureNames);
            }

            foreach (var property in parentSchema.ConcreteSchema.Properties ?? new Dictionary<string, IOpenApiSchema>())
            {
                RegisterNestedInlineObjectSchema(
                    parentSchema.SchemaName,
                    property.Key,
                    property.Value,
                    componentSchemas,
                    schemaTypeNamesByReference,
                    schemaTypeNamesByName,
                    usedStructureNames);
            }
        }

        var kiotaModelTypeToStructureName = componentSchemas
            .Where(componentSchema => !string.IsNullOrWhiteSpace(componentSchema.KiotaModelType))
            .GroupBy(componentSchema => NormalizeTypeName(componentSchema.KiotaModelType!), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().StructureName, StringComparer.Ordinal);

        foreach (var componentSchema in componentSchemas)
        {
            var requiredProperties = componentSchema.ConcreteSchema.Required is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(componentSchema.ConcreteSchema.Required, StringComparer.OrdinalIgnoreCase);
            var properties = new List<PropertyDefinition>();
            foreach (var property in componentSchema.ConcreteSchema.Properties ?? new Dictionary<string, IOpenApiSchema>())
            {
                try
                {
                    properties.Add(CreatePropertyDefinition(
                        componentSchema.SchemaName,
                        componentSchema.DefaultStructureName,
                        property.Key,
                        property.Value,
                        requiredProperties,
                        config,
                        schemaTypeNamesByName,
                        schemaTypeNamesByReference,
                        rawComponentSchemas,
                        componentSchema.KiotaModelType,
                        kiotaMetadata,
                        kiotaModelTypeToStructureName));
                }
                catch (NotSupportedException exception)
                {
                    DebugSchemaSkip(
                        componentSchema.SchemaName,
                        $"Failed to map property '{property.Key}' on schema '{componentSchema.SchemaName}'. {exception.Message}");
                }
            }

            properties = EnsureUniquePropertyNames(properties);

            schemas.Add(new SchemaDefinition(
                componentSchema.StructureName,
                componentSchema.DefaultStructureName,
                componentSchema.SchemaName,
                componentSchema.KiotaModelType,
                componentSchema.Description,
                componentSchema.IsRequestShape,
                properties));
        }

        return BreakRecursiveStructCycles(schemas);
    }

    private static IReadOnlyDictionary<string, string> BuildKiotaTypeHints(OpenApiDocument document, KiotaMetadata kiotaMetadata)
    {
        var hints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                var operationKey = operation.OperationId ?? "operation";
                if (operation.RequestBody is not null && TryGetSupportedRequestMediaType(operation.RequestBody, out var requestMediaType, out _))
                {
                    var expectedRequestSchemaId = OpenApiNamingPolicy.CreateStructureName($"{operationKey}_request");
                    var requestSchemaId = TryGetReferenceId(requestMediaType.Schema);
                    if (string.Equals(requestSchemaId, expectedRequestSchemaId, StringComparison.OrdinalIgnoreCase))
                    {
                        hints[requestSchemaId] = CreateKiotaRequestBodyTypeName(kiotaMetadata, path, operationType);
                    }
                }

                foreach (var (statusCode, response) in operation.Responses)
                {
                    if (response.Content is null || !response.Content.TryGetValue("application/json", out var responseMediaType) || responseMediaType.Schema is null)
                    {
                        continue;
                    }

                    var expectedResponseSchemaId = OpenApiNamingPolicy.CreateStructureName($"{operationKey}_{statusCode}_response");
                    var responseSchemaId = TryGetReferenceId(responseMediaType.Schema);
                    if (string.Equals(responseSchemaId, expectedResponseSchemaId, StringComparison.OrdinalIgnoreCase))
                    {
                        hints[responseSchemaId] = CreateKiotaResponseTypeName(kiotaMetadata, path);
                    }
                }
            }
        }

        return hints;
    }

    private static string ResolveKiotaModelType(
        string schemaName,
        string? schemaTitle,
        KiotaMetadata kiotaMetadata,
        IReadOnlyDictionary<string, string> kiotaTypeHintsBySchemaId,
        IReadOnlyDictionary<string, string> modelTypeLookup)
    {
        if (kiotaTypeHintsBySchemaId.TryGetValue(schemaName, out var hintedType))
        {
            return hintedType;
        }

        if (TryResolveLookupModelType(schemaName, schemaTitle, modelTypeLookup, out var discoveredType))
        {
            return discoveredType;
        }

        return $"{kiotaMetadata.ClientNamespace}.Models.{OpenApiNamingPolicy.CreateKiotaModelTypeName(schemaName)}";
    }

    private static IReadOnlyDictionary<string, string> BuildKiotaModelTypeLookup(KiotaMetadata kiotaMetadata)
    {
        if (string.IsNullOrWhiteSpace(kiotaMetadata.ClientRootPath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var modelsDirectory = Path.Combine(kiotaMetadata.ClientRootPath, "Models");
        if (!Directory.Exists(modelsDirectory))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in Directory.EnumerateFiles(modelsDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(modelsDirectory, filePath);
            var relativeWithoutExtension = Path.ChangeExtension(relativePath, null)!;
            var directoryName = Path.GetDirectoryName(relativePath);
            var className = Path.GetFileNameWithoutExtension(relativePath);
            var namespaceSuffix = string.IsNullOrWhiteSpace(directoryName)
                ? string.Empty
                : "." + string.Join(".", directoryName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Where(segment => !string.IsNullOrWhiteSpace(segment)));
            var fullType = $"{kiotaMetadata.ClientNamespace}.Models{namespaceSuffix}.{className}";

            lookup[NormalizeModelLookupKey(relativeWithoutExtension)] = fullType;
            lookup[NormalizeModelLookupKey(className)] = fullType;
        }

        return lookup;
    }

    private static bool TryResolveLookupModelType(
        string schemaName,
        string? schemaTitle,
        IReadOnlyDictionary<string, string> modelTypeLookup,
        out string modelType)
    {
        foreach (var candidate in new[] { schemaName, schemaTitle })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (modelTypeLookup.TryGetValue(NormalizeModelLookupKey(candidate), out modelType!))
            {
                return true;
            }
        }

        modelType = default!;
        return false;
    }

    private static string NormalizeModelLookupKey(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_');
        }

        var normalized = builder.ToString();
        while (normalized.Contains("__", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("__", "_", StringComparison.Ordinal);
        }

        return normalized.Trim('_');
    }

    private static IReadOnlyList<SchemaDefinition> BreakRecursiveStructCycles(IReadOnlyList<SchemaDefinition> schemas)
    {
        if (schemas.Count == 0)
        {
            return schemas;
        }

        var schemaNames = schemas.Select(schema => schema.Name).ToHashSet(StringComparer.Ordinal);
        var adjacency = schemas.ToDictionary(
            schema => schema.Name,
            schema => GetSchemaDependencies(schema, schemaNames).ToList(),
            StringComparer.Ordinal);
        var stronglyConnectedNames = FindStronglyConnectedSchemaNames(adjacency);
        if (stronglyConnectedNames.Count == 0)
        {
            return schemas;
        }

        return schemas
            .Select(schema => schema with
            {
                Properties = schema.Properties
                    .Select(property => RewriteRecursiveProperty(property, schema.Name, stronglyConnectedNames))
                    .ToList()
            })
            .ToList();
    }

    private static IEnumerable<string> GetSchemaDependencies(SchemaDefinition schema, IReadOnlySet<string> schemaNames)
    {
        foreach (var property in schema.Properties)
        {
            if (TryGetReferencedSchemaName(property.CSharpType, schemaNames) is { } dependency)
            {
                yield return dependency;
            }
        }
    }

    private static HashSet<string> FindStronglyConnectedSchemaNames(IReadOnlyDictionary<string, List<string>> adjacency)
    {
        var index = 0;
        var stack = new Stack<string>();
        var onStack = new HashSet<string>(StringComparer.Ordinal);
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);
        var lowLinks = new Dictionary<string, int>(StringComparer.Ordinal);
        var recursiveNames = new HashSet<string>(StringComparer.Ordinal);

        void StrongConnect(string node)
        {
            indexes[node] = index;
            lowLinks[node] = index;
            index++;
            stack.Push(node);
            onStack.Add(node);

            foreach (var neighbor in adjacency[node])
            {
                if (!indexes.ContainsKey(neighbor))
                {
                    StrongConnect(neighbor);
                    lowLinks[node] = Math.Min(lowLinks[node], lowLinks[neighbor]);
                }
                else if (onStack.Contains(neighbor))
                {
                    lowLinks[node] = Math.Min(lowLinks[node], indexes[neighbor]);
                }
            }

            if (lowLinks[node] != indexes[node])
            {
                return;
            }

            var component = new List<string>();
            string currentNode;
            do
            {
                currentNode = stack.Pop();
                onStack.Remove(currentNode);
                component.Add(currentNode);
            } while (!string.Equals(currentNode, node, StringComparison.Ordinal));

            if (component.Count > 1 || adjacency[node].Contains(node, StringComparer.Ordinal))
            {
                foreach (var name in component)
                {
                    recursiveNames.Add(name);
                }
            }
        }

        foreach (var node in adjacency.Keys)
        {
            if (!indexes.ContainsKey(node))
            {
                StrongConnect(node);
            }
        }

        return recursiveNames;
    }

    private static PropertyDefinition RewriteRecursiveProperty(PropertyDefinition property, string ownerSchemaName, IReadOnlySet<string> recursiveSchemaNames)
    {
        if (!recursiveSchemaNames.Contains(ownerSchemaName))
        {
            return property;
        }

        if (recursiveSchemaNames.Contains(property.CSharpType))
        {
            return property with
            {
                CSharpType = "string",
                IsNullable = true,
                Description = AppendRecursiveReferenceNote(property.Description)
            };
        }

        if (TryGetListElementType(property.CSharpType, out var elementType) &&
            elementType is not null &&
            recursiveSchemaNames.Contains(elementType))
        {
            return property with
            {
                CSharpType = "List<string>",
                IsNullable = false,
                Description = AppendRecursiveReferenceNote(property.Description)
            };
        }

        return property;
    }

    private static string? TryGetReferencedSchemaName(string cSharpType, IReadOnlySet<string> schemaNames)
    {
        if (schemaNames.Contains(cSharpType))
        {
            return cSharpType;
        }

        return TryGetListElementType(cSharpType, out var elementType) && elementType is not null && schemaNames.Contains(elementType)
            ? elementType
            : null;
    }

    private static bool TryGetListElementType(string cSharpType, out string? elementType)
    {
        const string prefix = "List<";
        if (cSharpType.StartsWith(prefix, StringComparison.Ordinal) && cSharpType.EndsWith('>'))
        {
            elementType = cSharpType[prefix.Length..^1];
            return true;
        }

        elementType = null;
        return false;
    }

    private static string AppendRecursiveReferenceNote(string description)
    {
        const string note = " Recursive reference lowered to a string identifier to keep the emitted OutSystems struct graph CLR-safe.";
        return description.Contains(note, StringComparison.Ordinal) ? description : description + note;
    }

    private static string ResolveModelSchemaType(
        string schemaName,
        IReadOnlyDictionary<string, string> schemaModelTypes)
    {
        return schemaModelTypes.TryGetValue(schemaName, out var modelType)
            ? modelType
            : schemaName;
    }

    private static string ResolveModelSchemaType(
        string schemaName,
        string? kiotaModelType,
        IReadOnlyList<SchemaDefinition> schemas)
    {
        if (TryGetListElementType(schemaName, out var elementType) &&
            elementType is not null &&
            schemas.FirstOrDefault(schema => string.Equals(schema.Name, elementType, StringComparison.OrdinalIgnoreCase)) is { KiotaModelType: { } elementKiotaType })
        {
            return $"List<{elementKiotaType}>";
        }

        return string.IsNullOrWhiteSpace(kiotaModelType) ? schemaName : kiotaModelType;
    }

    private static IReadOnlyList<OperationDefinition> BuildOperations(
        OpenApiDocument document,
        IReadOnlyList<SchemaDefinition> schemas,
        EffectiveGeneratorConfig config,
        KiotaMetadata kiotaMetadata)
    {
        IDictionary<string, IOpenApiSchema> rawComponentSchemas = document.Components?.Schemas ?? new Dictionary<string, IOpenApiSchema>();
        var schemaNames = schemas
            .GroupBy(schema => schema.OriginalSchemaName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.OrdinalIgnoreCase);
        var schemaModelTypes = schemas.ToDictionary(
            schema => schema.Name,
            schema => ResolveModelSchemaType(schema.Name, schema.KiotaModelType, schemas),
            StringComparer.OrdinalIgnoreCase);
        var schemaKiotaSupport = schemas
            .GroupBy(schema => schema.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Any(schema => !string.IsNullOrWhiteSpace(schema.KiotaModelType)), StringComparer.OrdinalIgnoreCase);
        var operations = new List<OperationDefinition>();
        var usedOperationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                try
                {
                    if (operation.RequestBody is not null && !HasSupportedRequestMediaType(operation.RequestBody))
                    {
                        DebugOperationSkip(path, operationType, operation.OperationId, "unsupported request body media type");
                        continue;
                    }

                    var parameters = (pathItem.Parameters ?? [])
                        .Concat(operation.Parameters ?? [])
                        .Select(parameter => ResolveParameter(parameter, schemaNames, rawComponentSchemas))
                        .ToList();
                    parameters = AllocateUniqueParameterNames(parameters);

                    var resourcePropertyName = OpenApiNamingPolicy.CreateStructureName(path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "resource");
                    var defaultOperationName = CreateOperationName(operationType, path, resourcePropertyName, operation.OperationId);
                    var actionOverride = FindActionOverride(config, operationType, path, defaultOperationName);
                    var operationName = AllocateUniqueName(actionOverride?.Name ?? defaultOperationName, usedOperationNames);
                    var requestBody = operation.RequestBody is null
                        ? null
                        : ResolveBody(operation.RequestBody, schemaNames, schemaModelTypes, path, ToHttpMethod(operationType), operationName, rawComponentSchemas, kiotaMetadata);
                var response = ResolveResponse(path, ToHttpMethod(operationType), operation.Responses, schemaNames, rawComponentSchemas, kiotaMetadata);

                    if ((requestBody is not null && schemaKiotaSupport.TryGetValue(requestBody.SchemaName, out var requestBodySupported) && !requestBodySupported) ||
                        (response?.SchemaName is not null &&
                         !string.Equals(response.Kind, "array", StringComparison.OrdinalIgnoreCase) &&
                         schemaKiotaSupport.TryGetValue(response.SchemaName, out var responseSupported) &&
                         !responseSupported))
                    {
                        DebugOperationSkip(
                            path,
                            operationType,
                            operation.OperationId,
                            $"unsupported Kiota binding (request: {requestBody?.SchemaName ?? "<none>"}, response: {response?.SchemaName ?? "<none>"})");
                        continue;
                    }

                    var operationDescription = actionOverride?.Description
                        ?? NormalizeDescription(operation.Summary ?? operation.Description ?? $"{operationType} {path}");

                    operations.Add(new OperationDefinition(
                        Name: operationName,
                        DefaultName: defaultOperationName,
                        Description: operationDescription,
                        HttpMethod: operationType.ToString().ToUpperInvariant(),
                        Path: path,
                        ResourcePropertyName: resourcePropertyName,
                        Parameters: parameters,
                        RequestBody: requestBody,
                        Response: response,
                        SecurityRequirements: ResolveSecurityRequirements(document, operation)));
                }
                catch (NotSupportedException exception)
                {
                    DebugOperationSkip(path, operationType, operation.OperationId, $"not supported by current normalizer: {exception.Message}");
                    continue;
                }
            }
        }

        return operations;
    }

    private static RuntimeContextDefinition BuildRuntimeContext(
        OpenApiDocument document,
        IReadOnlyList<SchemaDefinition> schemas,
        IReadOnlyList<OperationDefinition> operations)
    {
        var defaultBaseUrl = ResolveDefaultBaseUrl(document);
        var requestOptionsTypeName = AllocateUniqueName("RequestOptions", schemas.Select(schema => schema.Name));
        var usedSecuritySchemeIds = operations
            .SelectMany(operation => operation.SecurityRequirements)
            .SelectMany(requirement => requirement.SchemeIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var securitySchemes = ResolveSecuritySchemes(document)
            .Where(scheme => usedSecuritySchemeIds.Contains(scheme.Id))
            .ToArray();

        return new RuntimeContextDefinition(defaultBaseUrl, requestOptionsTypeName, securitySchemes);
    }

    private static string? ResolveDefaultBaseUrl(OpenApiDocument document)
    {
        return document.Servers?
            .Select(server => server?.Url?.Trim())
            .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));
    }

    private static IReadOnlyList<SecurityRequirementDefinition> ResolveSecurityRequirements(OpenApiDocument document, OpenApiOperation operation)
    {
        var requirements = operation.Security is not null
            ? operation.Security
            : document.Security;

        if (requirements is null || requirements.Count == 0)
        {
            return [];
        }

        var result = new List<SecurityRequirementDefinition>();
        foreach (var requirement in requirements)
        {
            var schemeIds = requirement.Keys
                .Select(ResolveSecuritySchemeId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (schemeIds.Length > 0)
            {
                result.Add(new SecurityRequirementDefinition(schemeIds));
            }
        }

        return result;
    }

    private static IReadOnlyList<SecuritySchemeDefinition> ResolveSecuritySchemes(OpenApiDocument document)
    {
        var rawSchemes = document.Components?.SecuritySchemes;
        if (rawSchemes is null || rawSchemes.Count == 0)
        {
            return [];
        }

        var schemes = new List<SecuritySchemeDefinition>();
        foreach (var (schemeId, scheme) in rawSchemes)
        {
            var resolvedScheme = ResolveSecurityScheme(scheme);
            if (resolvedScheme?.Type is null || !IsSupportedSecuritySchemeType(resolvedScheme.Type.Value))
            {
                continue;
            }

            schemes.Add(new SecuritySchemeDefinition(
                schemeId,
                resolvedScheme.Type.Value.ToString(),
                resolvedScheme.In?.ToString(),
                resolvedScheme.Name,
                resolvedScheme.Scheme,
                resolvedScheme.Description));
        }

        return schemes;
    }

    private static OpenApiSecurityScheme? ResolveSecurityScheme(IOpenApiSecurityScheme? scheme)
    {
        return scheme switch
        {
            null => null,
            OpenApiSecuritySchemeReference reference when reference.Target is OpenApiSecurityScheme target => target,
            OpenApiSecurityScheme concreteScheme => concreteScheme,
            _ => null
        };
    }

    private static string? ResolveSecuritySchemeId(IOpenApiSecurityScheme scheme)
    {
        return scheme switch
        {
            OpenApiSecuritySchemeReference reference when !string.IsNullOrWhiteSpace(reference.Reference?.Id) => reference.Reference!.Id,
            OpenApiSecuritySchemeReference reference when !string.IsNullOrWhiteSpace(reference.Name) => reference.Name,
            OpenApiSecurityScheme concreteScheme when !string.IsNullOrWhiteSpace(concreteScheme.Name) => concreteScheme.Name,
            _ => null
        };
    }

    private static bool IsSupportedSecuritySchemeType(SecuritySchemeType type)
    {
        return type == SecuritySchemeType.ApiKey ||
               type == SecuritySchemeType.Http ||
               type == SecuritySchemeType.OAuth2 ||
               type == SecuritySchemeType.OpenIdConnect;
    }

    private static string AllocateUniqueName(string baseName, IEnumerable<string> existingNames)
    {
        var usedNames = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        if (usedNames.Add(baseName))
        {
            return baseName;
        }

        var suffix = 2;
        while (!usedNames.Add($"{baseName}{suffix}"))
        {
            suffix++;
        }

        return $"{baseName}{suffix}";
    }

    private static ParameterDefinition ResolveParameter(
        IOpenApiParameter parameter,
        IReadOnlyDictionary<string, string> schemaNames,
        IDictionary<string, IOpenApiSchema> componentSchemas)
    {
        var parameterSchema = parameter.Schema ?? throw new NotSupportedException($"Parameter '{parameter.Name}' does not declare a schema.");
        parameterSchema = TryResolvePreferredParameterSchema(parameterSchema) ?? parameterSchema;
        var type = ResolveParameterType(parameter, parameterSchema, schemaNames, componentSchemas);
        var isNullable = !parameter.Required || IsNullableSchema(parameterSchema);
        var queryProperty = parameter.In == ParameterLocation.Query ? OpenApiNamingPolicy.CreateQueryPropertyName(parameter.Name) : null;
        var requiresPresenceFlag = parameter.In == ParameterLocation.Query && !parameter.Required;
        var presenceFlagName = requiresPresenceFlag ? OpenApiNamingPolicy.CreatePresenceFlagName(OpenApiNamingPolicy.SanitizeIdentifier(parameter.Name)) : null;

        return new ParameterDefinition(
            Name: OpenApiNamingPolicy.CreateParameterName(OpenApiNamingPolicy.SanitizeIdentifier(parameter.Name)),
            DefaultName: OpenApiNamingPolicy.CreateParameterName(OpenApiNamingPolicy.SanitizeIdentifier(parameter.Name)),
            OriginalName: parameter.Name,
            Location: parameter.In?.ToString().ToLowerInvariant() ?? "query",
            CSharpType: type,
            IsNullable: isNullable,
            Description: NormalizeParameterDescription(parameter),
            QueryPropertyName: queryProperty,
            RequiresPresenceFlag: requiresPresenceFlag,
            PresenceFlagName: presenceFlagName);
    }

    private static List<ParameterDefinition> AllocateUniqueParameterNames(IReadOnlyList<ParameterDefinition> parameters)
    {
        var usedParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedPresenceFlagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedParameters = new List<ParameterDefinition>(parameters.Count);

        foreach (var parameter in parameters)
        {
            var uniqueName = AllocateUniqueName(parameter.Name, usedParameterNames);
            var uniquePresenceFlagName = parameter.PresenceFlagName is null
                ? null
                : AllocateUniqueName(parameter.PresenceFlagName, usedPresenceFlagNames);

            normalizedParameters.Add(parameter with
            {
                Name = uniqueName,
                PresenceFlagName = uniquePresenceFlagName
            });
        }

        return normalizedParameters;
    }

    private static string ResolveParameterType(
        IOpenApiParameter parameter,
        IOpenApiSchema parameterSchema,
        IReadOnlyDictionary<string, string> schemaNames,
        IDictionary<string, IOpenApiSchema> componentSchemas)
    {
        if (HasSchemaType(parameterSchema, JsonSchemaType.String) &&
            string.Equals(parameterSchema.Format, "date", StringComparison.OrdinalIgnoreCase))
        {
            return "DateTime";
        }

        try
        {
            return MapSchemaType(parameterSchema, schemaNames, "parameter", componentSchemas: componentSchemas);
        }
        catch (NotSupportedException) when (parameter.In == ParameterLocation.Query && TryResolveQueryParameterFallbackType(parameterSchema, schemaNames, componentSchemas) is not null)
        {
            // Stripe exposes some query parameters as OpenAPI objects/range helpers, while
            // Kiota still binds them as plain query string values on the request builder.
            return TryResolveQueryParameterFallbackType(parameterSchema, schemaNames, componentSchemas)!;
        }
    }

    private static BodyDefinition ResolveBody(
        IOpenApiRequestBody requestBody,
        IReadOnlyDictionary<string, string> schemaNames,
        IReadOnlyDictionary<string, string> schemaModelTypes,
        string path,
        HttpMethod operationType,
        string operationName,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        KiotaMetadata kiotaMetadata)
    {
        if (!TryGetSupportedRequestMediaType(requestBody, out var mediaType, out var mediaTypeName))
        {
            throw new NotSupportedException("Only JSON, form, text, and binary request bodies are supported in v1.");
        }

        var schema = mediaType.Schema;
        if (IsBinaryRequestBody(mediaTypeName, schema))
        {
            return new BodyDefinition("byte[]", "Stream", "binary", mediaTypeName);
        }

        if (IsTextRequestBody(mediaTypeName, schema))
        {
            return new BodyDefinition("string", "string", "text", mediaTypeName);
        }

        if (schema is null &&
            TryResolveExistingClientRequestBodyType(kiotaMetadata, path, operationType) is { } untypedBodyType)
        {
            return CreateFallbackRequestBodyDefinition(untypedBodyType, mediaTypeName);
        }

        string schemaName;
        try
        {
            schemaName = MapSchemaType(schema, schemaNames, "request body", componentSchemas: componentSchemas);
        }
        catch (NotSupportedException) when (TryResolveExistingClientRequestBodyType(kiotaMetadata, path, operationType) is { } fallbackBodyType)
        {
            return CreateFallbackRequestBodyDefinition(fallbackBodyType, mediaTypeName);
        }

        var modelSchemaName = requestBody.Content is not null && requestBody.Content.ContainsKey("multipart/form-data")
            ? "Microsoft.Kiota.Abstractions.MultipartBody"
            : ResolveModelSchemaType(schemaName, schemaModelTypes);
        if (HasSchemaType(schema, JsonSchemaType.Array))
        {
            return new BodyDefinition(schemaName, modelSchemaName, "json", mediaTypeName);
        }

        return new BodyDefinition(schemaName, modelSchemaName, "json", mediaTypeName);
    }

    private static ResponseDefinition? ResolveResponse(
        string path,
        HttpMethod operationType,
        OpenApiResponses responses,
        IReadOnlyDictionary<string, string> schemaNames,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        KiotaMetadata kiotaMetadata)
    {
        foreach (var statusCode in new[] { "200", "201", "202", "204", "default" })
        {
            if (!responses.TryGetValue(statusCode, out var response))
            {
                continue;
            }

            if (response.Content is null || response.Content.Count == 0)
            {
                return new ResponseDefinition("void", null);
            }

            if (response.Content.TryGetValue("application/json", out var mediaType) && mediaType.Schema is not null)
            {
                var schema = mediaType.Schema;
                if (HasSchemaType(schema, JsonSchemaType.Array))
                {
                    return new ResponseDefinition("array", MapSchemaType(schema.Items, schemaNames, "array response", componentSchemas: componentSchemas), "application/json");
                }

                return new ResponseDefinition("object", MapSchemaType(schema, schemaNames, "response", componentSchemas: componentSchemas), "application/json");
            }

            if (TryResolveExistingClientResponseKind(kiotaMetadata, path, operationType) is { } existingResponseKind)
            {
                return existingResponseKind;
            }

            foreach (var (mediaTypeName, alternateMediaType) in response.Content)
            {
                if (IsBinaryResponseBody(mediaTypeName, alternateMediaType.Schema))
                {
                    return new ResponseDefinition("binary", "byte[]", mediaTypeName);
                }

                if (IsTextResponseBody(mediaTypeName, alternateMediaType.Schema))
                {
                    return new ResponseDefinition("string", "string", mediaTypeName);
                }
            }

            if (response.Content.FirstOrDefault().Value?.Schema is { } fallbackSchema)
            {
                if (HasSchemaType(fallbackSchema, JsonSchemaType.Array))
                {
                    return new ResponseDefinition("array", MapSchemaType(fallbackSchema.Items, schemaNames, "array response", componentSchemas: componentSchemas));
                }

                return new ResponseDefinition("object", MapSchemaType(fallbackSchema, schemaNames, "response", componentSchemas: componentSchemas));
            }

            return new ResponseDefinition("void", null);
        }

        return null;
    }

    private static PropertyDefinition CreatePropertyDefinition(
        string sourceSchemaName,
        string defaultStructureName,
        string propertyName,
        IOpenApiSchema schema,
        IReadOnlySet<string> requiredProperties,
        EffectiveGeneratorConfig config,
        IReadOnlyDictionary<string, string> schemaTypeNamesByName,
        IReadOnlyDictionary<IOpenApiSchema, string> schemaTypeNamesByReference,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        string? parentKiotaModelType,
        KiotaMetadata kiotaMetadata,
        IReadOnlyDictionary<string, string> kiotaModelTypeToStructureName)
    {
        var defaultPropertyName = OpenApiNamingPolicy.CreatePropertyName(propertyName, defaultStructureName);
        var propertyOverride = FindStructureFieldOverride(config, sourceSchemaName, defaultStructureName, propertyName, defaultPropertyName);
        string cSharpType;
        try
        {
            cSharpType = MapSchemaType(
                schema,
                schemaTypeNamesByName,
                $"property '{propertyName}' on schema '{sourceSchemaName}'",
                schemaTypeNamesByReference,
                componentSchemas);
        }
        catch (NotSupportedException exception)
        {
            if (TryResolvePropertyTypeFromKiotaModel(
                    parentKiotaModelType,
                    defaultPropertyName,
                    kiotaMetadata,
                    kiotaModelTypeToStructureName) is { } kiotaPropertyType)
            {
                cSharpType = kiotaPropertyType;
            }
            else if (IsProxyOptionProperty(propertyName))
            {
                cSharpType = "string";
            }
            else if (TryResolveLoosePropertyFallbackType(schema) is { } loosePropertyType)
            {
                cSharpType = loosePropertyType;
            }
            else
            {
                throw new NotSupportedException(
                    $"Failed to map property '{propertyName}' on schema '{sourceSchemaName}'. {exception.Message}",
                    exception);
            }
        }
        var isNullable = !requiredProperties.Contains(propertyName) || IsNullableSchema(schema);

        return new PropertyDefinition(
            Name: propertyOverride?.Name ?? defaultPropertyName,
            DefaultName: defaultPropertyName,
            OriginalName: propertyName,
            CSharpType: cSharpType,
            IsNullable: isNullable,
            Description: propertyOverride?.Description ?? NormalizeDescription(schema.Description ?? $"Maps the '{propertyName}' field."));
    }

    private static bool IsProxyOptionProperty(string propertyName)
    {
        return Regex.IsMatch(propertyName, @"^option_\d+$", RegexOptions.CultureInvariant);
    }

    private static string CreateOperationName(HttpMethod operationType, string path, string resourcePropertyName, string? operationId)
    {
        if (!string.IsNullOrWhiteSpace(operationId))
        {
            return OpenApiNamingPolicy.CreateStructureName(operationId);
        }

        var singularResourceName = resourcePropertyName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? resourcePropertyName[..^1]
            : resourcePropertyName;
        var hasPathParameter = path.Contains('{');

        return operationType switch
        {
            _ when operationType == HttpMethod.Get && !hasPathParameter => $"List{resourcePropertyName}",
            _ when operationType == HttpMethod.Post && !hasPathParameter => $"Create{singularResourceName}",
            _ when operationType == HttpMethod.Get && hasPathParameter => $"Get{singularResourceName}ById",
            _ when operationType == HttpMethod.Put && hasPathParameter => $"Update{singularResourceName}",
            _ when operationType == HttpMethod.Patch && hasPathParameter => $"Update{singularResourceName}",
            _ when operationType == HttpMethod.Delete && hasPathParameter => $"Delete{singularResourceName}",
            _ => throw new NotSupportedException($"Cannot derive a stable action name for {operationType} {path}.")
        };
    }

    private static string ResolveSchemaName(IOpenApiSchema schema, IReadOnlyDictionary<string, string> schemaNames, string context)
    {
        if (schema is OpenApiSchemaReference reference && reference.Target is not null)
        {
            return ResolveSchemaName(reference.Target, schemaNames, context);
        }

        var referenceId = TryGetReferenceId(schema);
        if (string.IsNullOrWhiteSpace(referenceId))
        {
            throw new NotSupportedException($"Inline schemas are not supported for {context} in v1.");
        }

        if (!schemaNames.TryGetValue(referenceId, out var schemaName))
        {
            throw new NotSupportedException($"Referenced schema '{referenceId}' for {context} was not found in components.");
        }

        return schemaName;
    }

    private static string MapSchemaType(
        IOpenApiSchema? schema,
        IReadOnlyDictionary<string, string> schemaNames,
        string context,
        IReadOnlyDictionary<IOpenApiSchema, string>? schemaTypeNamesByReference = null,
        IDictionary<string, IOpenApiSchema>? componentSchemas = null)
    {
        if (schema is null)
        {
            throw new NotSupportedException($"A schema is required for {context}.");
        }

        if (schemaTypeNamesByReference is not null)
        {
            if (schemaTypeNamesByReference.TryGetValue(schema, out var structureName))
            {
                return structureName;
            }

            var concreteSchema = ResolveConcreteSchema(schema, context);
            if (!ReferenceEquals(concreteSchema, schema) &&
                schemaTypeNamesByReference.TryGetValue(concreteSchema, out var concreteStructureName))
            {
                return concreteStructureName;
            }
        }

        if (TryResolveSingleNonNullCandidate(schema) is { } nonNullCandidate)
        {
            return MapSchemaType(nonNullCandidate, schemaNames, context, schemaTypeNamesByReference, componentSchemas);
        }

        if (TryGetReferenceId(schema) is { } referenceId)
        {
            if (schemaNames.TryGetValue(referenceId, out var schemaName))
            {
                return schemaName;
            }

            if (componentSchemas is not null && componentSchemas.TryGetValue(referenceId, out var referencedSchema))
            {
                var concreteReferencedSchema = ResolveConcreteSchema(referencedSchema, context);
                if (schemaTypeNamesByReference is not null &&
                    schemaTypeNamesByReference.TryGetValue(concreteReferencedSchema, out var concreteStructureName))
                {
                    return concreteStructureName;
                }

                if (!ReferenceEquals(concreteReferencedSchema, schema) &&
                    !string.Equals(TryGetReferenceId(concreteReferencedSchema), referenceId, StringComparison.OrdinalIgnoreCase))
                {
                    return MapSchemaType(concreteReferencedSchema, schemaNames, context, schemaTypeNamesByReference, componentSchemas);
                }

                if (HasSchemaType(concreteReferencedSchema, JsonSchemaType.Integer))
                {
                    return "int";
                }

                if (HasSchemaType(concreteReferencedSchema, JsonSchemaType.Number))
                {
                    return "decimal";
                }

                if (HasSchemaType(concreteReferencedSchema, JsonSchemaType.Boolean))
                {
                    return "bool";
                }

                if (HasSchemaType(concreteReferencedSchema, JsonSchemaType.String))
                {
                    return string.Equals(concreteReferencedSchema.Format, "date-time", StringComparison.OrdinalIgnoreCase)
                        ? "DateTime"
                        : "string";
                }

                if (HasSchemaType(concreteReferencedSchema, JsonSchemaType.Array) && concreteReferencedSchema.Items is not null)
                {
                    return $"List<{MapSchemaType(concreteReferencedSchema.Items, schemaNames, context, schemaTypeNamesByReference, componentSchemas)}>";
                }
            }

            return ResolveSchemaName(schema, schemaNames, context);
        }

        if (schema is OpenApiSchemaReference reference && reference.Target is not null)
        {
            return MapSchemaType(reference.Target, schemaNames, context, schemaTypeNamesByReference, componentSchemas);
        }

        if (schema is OpenApiSchema titledSchema &&
            !string.IsNullOrWhiteSpace(titledSchema.Title) &&
            schemaNames.TryGetValue(titledSchema.Title, out var schemaNameFromTitle))
        {
            return schemaNameFromTitle;
        }

        if (IsObjectLikeSchema(schema))
        {
            if (schemaTypeNamesByReference is not null && schemaTypeNamesByReference.TryGetValue(schema, out var objectStructureName))
            {
                return objectStructureName;
            }

            var concreteSchema = ResolveConcreteSchema(schema, context);
            if (schemaTypeNamesByReference is not null &&
                !ReferenceEquals(concreteSchema, schema) &&
                schemaTypeNamesByReference.TryGetValue(concreteSchema, out var concreteObjectStructureName))
            {
                return concreteObjectStructureName;
            }
        }

        if (HasSchemaType(schema, JsonSchemaType.Integer))
        {
            return "int";
        }

        if (HasSchemaType(schema, JsonSchemaType.Number))
        {
            return "decimal";
        }

        if (HasSchemaType(schema, JsonSchemaType.Boolean))
        {
            return "bool";
        }

        if (HasSchemaType(schema, JsonSchemaType.String))
        {
            return string.Equals(schema.Format, "date-time", StringComparison.OrdinalIgnoreCase) ? "DateTime" : "string";
        }

        if (HasSchemaType(schema, JsonSchemaType.Array) && schema.Items is not null)
        {
            return $"List<{MapSchemaType(schema.Items, schemaNames, context, schemaTypeNamesByReference, componentSchemas)}>";
        }

        throw new NotSupportedException(
            $"Schema type '{schema.Type}' is not supported for {context}. " +
            $"Schema runtime type: '{schema.GetType().Name}', referenceId: '{TryGetReferenceId(schema) ?? "<none>"}', " +
            $"title: '{(schema is OpenApiSchema detailedSchema ? detailedSchema.Title ?? "<none>" : "<n/a>")}', " +
            $"anyOfCount: {schema.AnyOf?.Count ?? 0}, oneOfCount: {schema.OneOf?.Count ?? 0}, allOfCount: {schema.AllOf?.Count ?? 0}.");
    }

    private static bool IsObjectLikeSchema(IOpenApiSchema schema)
    {
        if (HasSchemaType(schema, JsonSchemaType.Object))
        {
            return true;
        }

        if (schema is not OpenApiSchema openApiSchema)
        {
            return false;
        }

        if ((openApiSchema.Properties?.Count ?? 0) > 0 ||
            openApiSchema.AdditionalProperties is not null)
        {
            return true;
        }

        return false;
    }

    private static bool IsNullableSchema(IOpenApiSchema schema)
    {
        return HasSchemaType(schema, JsonSchemaType.Null) ||
               ((schema.AnyOf?.Count ?? 0) > 0 && schema.AnyOf!.Any(candidate => HasSchemaType(candidate, JsonSchemaType.Null))) ||
               ((schema.OneOf?.Count ?? 0) > 0 && schema.OneOf!.Any(candidate => HasSchemaType(candidate, JsonSchemaType.Null)));
    }

    private static NameDescriptionOverride? FindActionOverride(EffectiveGeneratorConfig config, HttpMethod operationType, string path, string defaultName)
    {
        return FindOverride(config.ActionOverrides, $"{operationType.Method.ToUpperInvariant()} {path}", defaultName);
    }

    private static NameDescriptionOverride? FindStructureOverride(EffectiveGeneratorConfig config, string originalSchemaName, string defaultName)
    {
        return FindOverride(config.StructureOverrides, originalSchemaName, defaultName);
    }

    private static NameDescriptionOverride? FindStructureFieldOverride(
        EffectiveGeneratorConfig config,
        string originalSchemaName,
        string defaultStructureName,
        string originalFieldName,
        string defaultFieldName)
    {
        if (config.StructureFieldOverrides.TryGetValue(originalSchemaName, out var byOriginalSchema))
        {
            var match = FindOverride(byOriginalSchema, originalFieldName, defaultFieldName);
            if (match is not null)
            {
                return match;
            }
        }

        if (config.StructureFieldOverrides.TryGetValue(defaultStructureName, out var byDefaultStructure))
        {
            return FindOverride(byDefaultStructure, originalFieldName, defaultFieldName);
        }

        return null;
    }

    private static NameDescriptionOverride? FindOverride(
        IReadOnlyDictionary<string, NameDescriptionOverride> source,
        string primaryKey,
        string defaultName)
    {
        if (source.TryGetValue(primaryKey, out var primary))
        {
            return primary;
        }

        return source.TryGetValue(defaultName, out var fallback) ? fallback : null;
    }

    private static string NormalizeDescription(string description)
    {
        return string.Join(" ", description
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim()))
            .Trim();
    }

    private static string NormalizeParameterDescription(IOpenApiParameter parameter)
    {
        var rawDescription = parameter.Description;
        if (!string.IsNullOrWhiteSpace(rawDescription))
        {
            var normalized = NormalizeDescription(rawDescription);
            if (normalized.StartsWith("key:", StringComparison.OrdinalIgnoreCase))
            {
                var keyValue = normalized["key:".Length..].Trim();
                return $"The {keyValue} path parameter.";
            }

            return normalized;
        }

        return parameter.In switch
        {
            ParameterLocation.Path => $"The {parameter.Name} path parameter.",
            ParameterLocation.Query => $"The {parameter.Name} query parameter.",
            _ => $"{parameter.Name} {parameter.In} parameter."
        };
    }

    private static string ToPascalCase(string value)
    {
        var parts = value
            .Split(['-', '_', ' ', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]);
        return string.Concat(parts);
    }

    private static string ToCamelCase(string value)
    {
        var pascal = ToPascalCase(value);
        return string.IsNullOrEmpty(pascal)
            ? value
            : char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    private static string ToKiotaModelTypeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var sanitized = value
            .Replace('-', '_')
            .Replace('.', '_')
            .Replace(' ', '_');

        return char.ToUpperInvariant(sanitized[0]) + sanitized[1..];
    }

    private static string SanitizeIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }

    private static bool HasSchemaType(IOpenApiSchema? schema, JsonSchemaType expectedType)
    {
        return schema?.Type is JsonSchemaType actualType && (actualType & expectedType) == expectedType;
    }

    private static string? TryGetReferenceId(IOpenApiSchema? schema)
    {
        return schema switch
        {
            null => null,
            OpenApiSchemaReference reference when !string.IsNullOrWhiteSpace(reference.Id) => reference.Id,
            _ when !string.IsNullOrWhiteSpace(schema.Id) => schema.Id,
            _ => null
        };
    }

    private static OpenApiSchema ResolveConcreteSchema(IOpenApiSchema? schema, string context)
    {
        if (schema is null)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = $"Missing schema placeholder for {context}.",
                AdditionalPropertiesAllowed = true
            };
        }

        if (TryResolveSingleNonNullCandidate(schema) is { } nonNullCandidate)
        {
            return ResolveConcreteSchema(nonNullCandidate, context);
        }

        return schema switch
        {
            OpenApiSchemaReference reference when reference.RecursiveTarget is OpenApiSchema recursiveTarget => recursiveTarget,
            OpenApiSchemaReference reference when reference.Target is OpenApiSchema target => target,
            OpenApiSchema concreteSchema => concreteSchema,
            _ => throw new NotSupportedException($"Schema '{context}' could not be resolved to a concrete OpenAPI schema.")
        };
    }

    private static IOpenApiSchema? TryResolveSingleNonNullCandidate(IOpenApiSchema? schema)
    {
        var candidates = EnumerateComposedCandidates(schema)
            .Where(candidate => !HasSchemaType(candidate, JsonSchemaType.Null))
            .ToList();

        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static IEnumerable<IOpenApiSchema> EnumerateComposedCandidates(IOpenApiSchema? schema)
    {
        if (schema is null)
        {
            return [];
        }

        if ((schema.AnyOf?.Count ?? 0) > 0)
        {
            return schema.AnyOf!;
        }

        if ((schema.OneOf?.Count ?? 0) > 0)
        {
            return schema.OneOf!;
        }

        return [];
    }

    private static bool HasSupportedRequestMediaType(IOpenApiRequestBody requestBody)
    {
        return TryGetSupportedRequestMediaType(requestBody, out _, out _);
    }

    private static bool TryGetSupportedRequestMediaType(IOpenApiRequestBody requestBody, out IOpenApiMediaType mediaType, out string mediaTypeName)
    {
        mediaType = default!;
        mediaTypeName = string.Empty;
        if (requestBody.Content is null)
        {
            return false;
        }

        foreach (var supportedMediaType in new[]
                 {
                     "application/json",
                     "application/x-www-form-urlencoded",
                     "multipart/form-data",
                     "text/plain",
                     "text/x-markdown",
                     "application/octet-stream",
                     "application/pdf",
                     "image/png",
                     "image/gif",
                     "image/jpeg",
                     "image/bmp"
                 })
        {
            if (requestBody.Content.TryGetValue(supportedMediaType, out mediaType))
            {
                mediaTypeName = supportedMediaType;
                return true;
            }
        }

        foreach (var (candidateMediaTypeName, candidateMediaType) in requestBody.Content)
        {
            if (IsBinaryRequestBody(candidateMediaTypeName, candidateMediaType.Schema) ||
                IsTextRequestBody(candidateMediaTypeName, candidateMediaType.Schema))
            {
                mediaType = candidateMediaType;
                mediaTypeName = candidateMediaTypeName;
                return true;
            }
        }

        return false;
    }

    private static bool IsTextRequestBody(string mediaTypeName, IOpenApiSchema? schema)
    {
        return IsTextMediaType(mediaTypeName) && (schema is null || HasSchemaType(schema, JsonSchemaType.String));
    }

    private static BodyDefinition CreateFallbackRequestBodyDefinition(string bodyTypeName, string mediaTypeName)
    {
        if (bodyTypeName.Contains("Stream", StringComparison.Ordinal))
        {
            return new BodyDefinition("byte[]", "Stream", "binary", mediaTypeName);
        }

        if (bodyTypeName.Contains("string", StringComparison.Ordinal))
        {
            return new BodyDefinition("string", "string", "text", mediaTypeName);
        }

        return new BodyDefinition("string", bodyTypeName, "rawJson", mediaTypeName);
    }

    private static bool IsBinaryRequestBody(string mediaTypeName, IOpenApiSchema? schema)
    {
        return IsBinaryMediaType(mediaTypeName) ||
               (schema is not null &&
                HasSchemaType(schema, JsonSchemaType.String) &&
                string.Equals(schema.Format, "binary", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTextResponseBody(string mediaTypeName, IOpenApiSchema? schema)
    {
        return IsTextRequestBody(mediaTypeName, schema);
    }

    private static bool IsBinaryResponseBody(string mediaTypeName, IOpenApiSchema? schema)
    {
        return IsBinaryRequestBody(mediaTypeName, schema);
    }

    private static bool IsTextMediaType(string mediaTypeName)
    {
        return mediaTypeName.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBinaryMediaType(string mediaTypeName)
    {
        return mediaTypeName.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(mediaTypeName, "application/octet-stream", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(mediaTypeName, "application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetPrimaryResponseSchema(OpenApiResponses responses, out IOpenApiSchema schema)
    {
        schema = default!;
        foreach (var statusCode in new[] { "200", "201", "202", "204", "default" })
        {
            if (!responses.TryGetValue(statusCode, out var response) ||
                response.Content is null ||
                !response.Content.TryGetValue("application/json", out var mediaType) ||
                mediaType.Schema is null)
            {
                continue;
            }

            schema = mediaType.Schema;
            return true;
        }

        return false;
    }

    private static IOpenApiSchema? TryResolvePreferredParameterSchema(IOpenApiSchema schema)
    {
        var candidates = EnumerateComposedCandidates(schema)
            .Where(candidate => !HasSchemaType(candidate, JsonSchemaType.Null))
            .ToList();
        if (candidates.Count < 2)
        {
            return null;
        }

        var primitiveCandidate = candidates.FirstOrDefault(candidate =>
            HasSchemaType(candidate, JsonSchemaType.Integer) ||
            HasSchemaType(candidate, JsonSchemaType.Number) ||
            HasSchemaType(candidate, JsonSchemaType.String) ||
            HasSchemaType(candidate, JsonSchemaType.Boolean));

        return primitiveCandidate;
    }

    private static string? TryResolveQueryParameterFallbackType(
        IOpenApiSchema schema,
        IReadOnlyDictionary<string, string> schemaNames,
        IDictionary<string, IOpenApiSchema> componentSchemas)
    {
        if (HasSchemaType(schema, JsonSchemaType.String))
        {
            return "string";
        }

        if (schema is OpenApiSchemaReference reference && reference.Target is not null)
        {
            return TryResolveQueryParameterFallbackType(reference.Target, schemaNames, componentSchemas);
        }

        if (TryResolvePreferredParameterSchema(schema) is { } preferredSchema)
        {
            return TryResolveQueryParameterFallbackType(preferredSchema, schemaNames, componentSchemas);
        }

        if (TryGetReferenceId(schema) is { Length: > 0 } referenceId && schemaNames.ContainsKey(referenceId))
        {
            return null;
        }

        if (HasSchemaType(schema, JsonSchemaType.Array))
        {
            return schema.Items is not null && TryResolveQueryParameterFallbackType(schema.Items, schemaNames, componentSchemas) is not null
                ? "List<string>"
                : null;
        }

        if (HasSchemaType(schema, JsonSchemaType.Object))
        {
            return "string";
        }

        var concreteSchema = ResolveConcreteSchema(schema, "query parameter");
        if (ReferenceEquals(concreteSchema, schema))
        {
            return null;
        }

        if (TryGetReferenceId(schema) is { } referencedSchemaId &&
            componentSchemas.TryGetValue(referencedSchemaId, out var referencedSchema))
        {
            return TryResolveQueryParameterFallbackType(referencedSchema, schemaNames, componentSchemas);
        }

        return TryResolveQueryParameterFallbackType(concreteSchema, schemaNames, componentSchemas);
    }

    private static string CreateKiotaRequestBodyTypeName(KiotaMetadata kiotaMetadata, string path, HttpMethod operationType)
    {
        var methodName = ToKiotaMethodName(operationType);
        return TryResolveExistingClientType(kiotaMetadata, path, $"{methodName}RequestBody")
            ?? $"{CreateKiotaOperationNamespace(kiotaMetadata, path)}.{CreateKiotaOperationTypeStem(path)}{methodName}RequestBody";
    }

    private static string CreateKiotaResponseTypeName(KiotaMetadata kiotaMetadata, string path)
    {
        var operationNamespace = CreateKiotaOperationNamespace(kiotaMetadata, path);
        var operationStem = CreateKiotaOperationTypeStem(path);
        return PathEndsWithParameter(path)
            ? $"{operationNamespace}.{CreateKiotaRequestBuilderTypeName(path)}.{operationStem}Response"
            : $"{operationNamespace}.{operationStem}Response";
    }

    private static string? TryResolveExistingClientType(KiotaMetadata kiotaMetadata, string path, string fileSuffix)
    {
        if (TryGetExistingClientDirectoryPath(kiotaMetadata, path) is not { } directoryPath)
        {
            return null;
        }

        var matches = Directory.EnumerateFiles(directoryPath, $"*{fileSuffix}.cs", SearchOption.TopDirectoryOnly).ToList();
        if (matches.Count != 1)
        {
            return null;
        }

        var relativeWithoutExtension = Path.ChangeExtension(Path.GetRelativePath(kiotaMetadata.ClientRootPath, matches[0]), null)!;
        return $"{kiotaMetadata.ClientNamespace}.{relativeWithoutExtension.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.')}";
    }

    private static ResponseDefinition? TryResolveExistingClientResponseKind(KiotaMetadata kiotaMetadata, string path, HttpMethod operationType)
    {
        if (TryGetExistingRequestBuilderFilePath(kiotaMetadata, path) is not { } requestBuilderPath ||
            !File.Exists(requestBuilderPath))
        {
            return null;
        }

        var methodName = ToKiotaMethodName(operationType);
        var source = File.ReadAllText(requestBuilderPath);
        var match = Regex.Match(
            source,
            $@"public\s+async\s+Task(?:<(?<type>[^>]+)>)?\s+{Regex.Escape(methodName)}Async\s*\(",
            RegexOptions.CultureInvariant);

        if (!match.Success)
        {
            return null;
        }

        var returnType = match.Groups["type"].Success ? match.Groups["type"].Value : string.Empty;
        if (string.IsNullOrWhiteSpace(returnType))
        {
            return new ResponseDefinition("void", null);
        }

        if (returnType.Contains("Stream", StringComparison.Ordinal))
        {
            return new ResponseDefinition("binary", "byte[]");
        }

        if (returnType.Contains("string", StringComparison.Ordinal))
        {
            return new ResponseDefinition("string", "string");
        }

        return null;
    }

    private static string? TryResolveExistingClientRequestBodyType(KiotaMetadata kiotaMetadata, string path, HttpMethod operationType)
    {
        if (TryGetExistingRequestBuilderFilePath(kiotaMetadata, path) is not { } requestBuilderPath ||
            !File.Exists(requestBuilderPath))
        {
            return null;
        }

        var methodName = ToKiotaMethodName(operationType);
        var source = File.ReadAllText(requestBuilderPath);
        var match = Regex.Match(
            source,
            $@"public\s+async\s+Task(?:<[^>]+>)?\s+{Regex.Escape(methodName)}Async\s*\((?<type>[^\s,]+)\s+body(?:,|\))",
            RegexOptions.CultureInvariant);

        return match.Success ? match.Groups["type"].Value : null;
    }

    private static string? TryResolvePropertyTypeFromKiotaModel(
        string? parentKiotaModelType,
        string propertyName,
        KiotaMetadata kiotaMetadata,
        IReadOnlyDictionary<string, string> kiotaModelTypeToStructureName)
    {
        if (string.IsNullOrWhiteSpace(parentKiotaModelType) ||
            string.IsNullOrWhiteSpace(kiotaMetadata.ClientNamespace) ||
            string.IsNullOrWhiteSpace(kiotaMetadata.ClientRootPath))
        {
            return null;
        }

        var normalizedParentType = NormalizeTypeName(parentKiotaModelType);
        if (!normalizedParentType.StartsWith($"{kiotaMetadata.ClientNamespace}.", StringComparison.Ordinal))
        {
            return null;
        }

        var relativeTypePath = normalizedParentType[(kiotaMetadata.ClientNamespace.Length + 1)..]
            .Replace('.', Path.DirectorySeparatorChar);
        var modelFilePath = Path.Combine(kiotaMetadata.ClientRootPath!, $"{relativeTypePath}.cs");
        if (!File.Exists(modelFilePath))
        {
            return null;
        }

        var source = File.ReadAllText(modelFilePath);
        var match = Regex.Match(
            source,
            $@"^\s*public\s+(?<type>.+?)\s+{Regex.Escape(propertyName)}\s*\{{",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return null;
        }

        return MapKiotaPropertyTypeToGeneratedType(match.Groups["type"].Value.Trim(), kiotaModelTypeToStructureName);
    }

    private static string? MapKiotaPropertyTypeToGeneratedType(
        string kiotaPropertyType,
        IReadOnlyDictionary<string, string> kiotaModelTypeToStructureName)
    {
        var normalizedType = NormalizeTypeName(kiotaPropertyType).TrimEnd('?');
        if (string.Equals(normalizedType, "UntypedNode", StringComparison.Ordinal))
        {
            return "string";
        }

        if (normalizedType.StartsWith("List<", StringComparison.Ordinal) && normalizedType.EndsWith(">", StringComparison.Ordinal))
        {
            var innerType = normalizedType["List<".Length..^1];
            var mappedInnerType = MapKiotaPropertyTypeToGeneratedType(innerType, kiotaModelTypeToStructureName);
            return mappedInnerType is null ? null : $"List<{mappedInnerType}>";
        }

        return normalizedType switch
        {
            "string" => "string",
            "bool" => "bool",
            "int" => "int",
            "long" => "long",
            "double" => "double",
            "float" => "float",
            "decimal" => "decimal",
            "DateTimeOffset" => "DateTime",
            "DateTime" => "DateTime",
            "Date" => "Date",
            _ when kiotaModelTypeToStructureName.TryGetValue(normalizedType, out var structureName) => structureName,
            _ => null
        };
    }

    private static string? TryResolveLoosePropertyFallbackType(IOpenApiSchema schema)
    {
        var concreteSchema = TryResolveSingleNonNullCandidate(schema) ?? schema;
        concreteSchema = ResolveConcreteSchema(concreteSchema, "property fallback");

        if ((concreteSchema.Enum?.Count ?? 0) > 0)
        {
            return "string";
        }

        if (HasSchemaType(concreteSchema, JsonSchemaType.Null))
        {
            return "string";
        }

        if (IsFreeFormObjectSchema(concreteSchema))
        {
            return "string";
        }

        if (HasSchemaType(concreteSchema, JsonSchemaType.Array) &&
            concreteSchema.Items is not null)
        {
            if (IsUntypedSchema(concreteSchema.Items))
            {
                return "List<string>";
            }

            if (TryResolveLoosePropertyFallbackType(concreteSchema.Items) is { } itemType)
            {
                return $"List<{itemType}>";
            }

            return "List<string>";
        }

        if (IsUntypedSchema(concreteSchema))
        {
            return "string";
        }

        return null;
    }

    private static bool IsFreeFormObjectSchema(IOpenApiSchema schema)
    {
        return schema is OpenApiSchema openApiSchema &&
               HasSchemaType(schema, JsonSchemaType.Object) &&
               (openApiSchema.Properties?.Count ?? 0) == 0 &&
               openApiSchema.AdditionalProperties is null &&
               openApiSchema.AdditionalPropertiesAllowed;
    }

    private static bool IsUntypedSchema(IOpenApiSchema schema)
    {
        return schema is OpenApiSchema openApiSchema &&
               openApiSchema.Type == 0 &&
               (openApiSchema.Properties?.Count ?? 0) == 0 &&
               openApiSchema.AdditionalProperties is null &&
               (openApiSchema.AnyOf?.Count ?? 0) == 0 &&
               (openApiSchema.OneOf?.Count ?? 0) == 0 &&
               (openApiSchema.AllOf?.Count ?? 0) == 0;
    }

    private static string NormalizeTypeName(string value)
    {
        return value.Replace("global::", string.Empty, StringComparison.Ordinal).Trim();
    }

    private static string? TryGetExistingRequestBuilderFilePath(KiotaMetadata kiotaMetadata, string path)
    {
        if (TryGetExistingClientDirectoryPath(kiotaMetadata, path) is not { } directoryPath)
        {
            return null;
        }

        var requestBuilderTypeName = CreateKiotaRequestBuilderTypeName(path);
        var requestBuilderPath = Path.Combine(directoryPath, $"{requestBuilderTypeName}.cs");
        if (File.Exists(requestBuilderPath))
        {
            return requestBuilderPath;
        }

        if (PathEndsWithParameter(path))
        {
            var matches = Directory.EnumerateFiles(directoryPath, "With*ItemRequestBuilder.cs", SearchOption.TopDirectoryOnly).ToList();
            if (matches.Count == 1)
            {
                return matches[0];
            }
        }

        return null;
    }

    private static string? TryGetExistingClientDirectoryPath(KiotaMetadata kiotaMetadata, string path)
    {
        if (string.IsNullOrWhiteSpace(kiotaMetadata.ClientRootPath))
        {
            return null;
        }

        var directorySegments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Select(segment =>
            segment.StartsWith('{') && segment.EndsWith('}')
                ? "Item"
                : CreateKiotaDirectorySegmentName(segment));
        var directoryPath = Path.Combine(new[] { kiotaMetadata.ClientRootPath }.Concat(directorySegments).ToArray());
        return Directory.Exists(directoryPath) ? directoryPath : null;
    }

    private static string ToKiotaMethodName(HttpMethod operationType)
    {
        var method = operationType.Method.ToLowerInvariant();
        return char.ToUpperInvariant(method[0]) + method[1..];
    }

    private static HttpMethod ToHttpMethod(object operationType)
    {
        return new HttpMethod(operationType.ToString() ?? "GET");
    }

    private static string CreateKiotaDirectorySegmentName(string segment)
    {
        return string.Equals(segment, "void", StringComparison.OrdinalIgnoreCase)
            ? "VoidNamespace"
            : CreateKiotaPathSegmentName(segment);
    }

    private static string CreateKiotaOperationNamespace(KiotaMetadata kiotaMetadata, string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var namespaceSegments = segments.Select(segment => segment.StartsWith('{') && segment.EndsWith('}')
            ? "Item"
            : CreateKiotaPathSegmentName(segment));

        return $"{kiotaMetadata.ClientNamespace}.{string.Join(".", namespaceSegments)}";
    }

    private static string CreateKiotaOperationTypeStem(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return "Root";
        }

        var lastSegment = segments[^1];
        if (lastSegment.StartsWith('{') && lastSegment.EndsWith('}'))
        {
            return $"With{OpenApiNamingPolicy.CreateStructureName(lastSegment[1..^1])}";
        }

        return CreateKiotaPathSegmentName(lastSegment);
    }

    private static string CreateKiotaRequestBuilderTypeName(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return "RootRequestBuilder";
        }

        var lastSegment = segments[^1];
        if (lastSegment.StartsWith('{') && lastSegment.EndsWith('}'))
        {
            return $"With{OpenApiNamingPolicy.CreateStructureName(lastSegment[1..^1])}ItemRequestBuilder";
        }

        return $"{CreateKiotaPathSegmentName(lastSegment)}RequestBuilder";
    }

    private static string CreateKiotaPathSegmentName(string segment)
    {
        var kiotaName = OpenApiNamingPolicy.CreateKiotaPathSegmentName(segment);
        return IsVersionSegment(segment)
            ? kiotaName.Replace("_", string.Empty, StringComparison.Ordinal)
            : kiotaName;
    }

    private static bool IsVersionSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment) || segment.Length < 2 || char.ToLowerInvariant(segment[0]) != 'v')
        {
            return false;
        }

        return segment[1..].All(character => char.IsDigit(character) || character == '.' || character == '_');
    }

    private static bool PathEndsWithParameter(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        var lastSegment = segments[^1];
        return lastSegment.StartsWith('{') && lastSegment.EndsWith('}');
    }

    private static List<PropertyDefinition> EnsureUniquePropertyNames(IEnumerable<PropertyDefinition> properties)
    {
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<PropertyDefinition>();
        foreach (var property in properties)
        {
            var uniqueName = AllocateUniqueName(property.Name, usedNames);
            result.Add(uniqueName == property.Name ? property : property with { Name = uniqueName });
        }

        return result;
    }

    private static string AllocateUniqueName(string preferredName, ISet<string> usedNames)
    {
        var candidate = OpenApiNamingPolicy.ConstrainOutSystemsName(preferredName);
        var suffix = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = OpenApiNamingPolicy.ConstrainOutSystemsName($"{preferredName}{suffix++}");
        }

        return candidate;
    }

    private static NormalizedOpenApiDocument ApplyOutSystemsCompatibility(
        IReadOnlyList<SchemaDefinition> schemas,
        IReadOnlyList<OperationDefinition> operations,
        RuntimeContextDefinition runtimeContext)
    {
        var emptySchemaNames = schemas
            .Where(schema => schema.Properties.Count == 0)
            .Select(schema => schema.Name)
            .ToHashSet(StringComparer.Ordinal);

        var compatibleSchemas = schemas
            .Where(schema => !emptySchemaNames.Contains(schema.Name))
            .Select(schema => schema with
            {
                Properties = schema.Properties
                    .Select(property => property with { CSharpType = RewriteOutSystemsType(property.CSharpType, emptySchemaNames) })
                    .ToList()
            })
            .ToList();

        var compatibleOperations = operations
            .Select(operation => operation with
            {
                RequestBody = RewriteOutSystemsRequestBody(operation.RequestBody, emptySchemaNames),
                Response = RewriteOutSystemsResponse(operation.Response, emptySchemaNames)
            })
            .ToList();

        return new NormalizedOpenApiDocument(compatibleSchemas, compatibleOperations, runtimeContext);
    }

    private static NormalizedOpenApiDocument PruneUnreachableSchemas(NormalizedOpenApiDocument document)
    {
        var schemaLookup = document.Schemas.ToDictionary(schema => schema.Name, StringComparer.OrdinalIgnoreCase);
        var reachableSchemaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var operation in document.Operations)
        {
            foreach (var parameter in operation.Parameters)
            {
                VisitSchemaReferences(parameter.CSharpType, schemaLookup, reachableSchemaNames);
            }

            if (operation.RequestBody?.SchemaName is { Length: > 0 } requestBodySchemaName)
            {
                VisitSchemaReferences(requestBodySchemaName, schemaLookup, reachableSchemaNames);
            }

            if (operation.Response?.SchemaName is { Length: > 0 } responseSchemaName)
            {
                VisitSchemaReferences(responseSchemaName, schemaLookup, reachableSchemaNames);
            }
        }

        var reachableSchemas = document.Schemas
            .Where(schema => reachableSchemaNames.Contains(schema.Name))
            .ToList();

        return document with { Schemas = reachableSchemas };
    }

    private static void VisitSchemaReferences(
        string typeName,
        IReadOnlyDictionary<string, SchemaDefinition> schemaLookup,
        HashSet<string> reachableSchemaNames)
    {
        foreach (Match match in Regex.Matches(typeName, @"[A-Za-z_][A-Za-z0-9_]*"))
        {
            var candidate = match.Value;
            if (!schemaLookup.TryGetValue(candidate, out var schema) || !reachableSchemaNames.Add(schema.Name))
            {
                continue;
            }

            foreach (var property in schema.Properties)
            {
                VisitSchemaReferences(property.CSharpType, schemaLookup, reachableSchemaNames);
            }
        }
    }

    private static BodyDefinition? RewriteOutSystemsRequestBody(BodyDefinition? body, IReadOnlySet<string> emptySchemaNames)
    {
        if (body is null)
        {
            return null;
        }

        return emptySchemaNames.Contains(body.SchemaName)
            ? body with { SchemaName = "string", Kind = "rawJson" }
            : body with { SchemaName = RewriteOutSystemsType(body.SchemaName, emptySchemaNames) };
    }

    private static ResponseDefinition? RewriteOutSystemsResponse(ResponseDefinition? response, IReadOnlySet<string> emptySchemaNames)
    {
        if (response is null || response.SchemaName is null)
        {
            return response;
        }

        if (string.Equals(response.Kind, "array", StringComparison.Ordinal))
        {
            var rewrittenSchemaName = RewriteOutSystemsType(response.SchemaName, emptySchemaNames);
            return rewrittenSchemaName.StartsWith("List<", StringComparison.Ordinal)
                ? response with { SchemaName = "string" }
                : response with { SchemaName = rewrittenSchemaName };
        }

        return emptySchemaNames.Contains(response.SchemaName)
            ? response with { Kind = "rawJson", SchemaName = "string" }
            : response with { SchemaName = RewriteOutSystemsType(response.SchemaName, emptySchemaNames) };
    }

    private static string RewriteOutSystemsType(string cSharpType, IReadOnlySet<string> emptySchemaNames)
    {
        if (emptySchemaNames.Contains(cSharpType))
        {
            return "string";
        }

        if (IsNestedListType(cSharpType))
        {
            return "List<string>";
        }

        if (cSharpType.StartsWith("List<", StringComparison.Ordinal) &&
            cSharpType.EndsWith(">", StringComparison.Ordinal))
        {
            var innerType = cSharpType["List<".Length..^1];
            var rewrittenInner = RewriteOutSystemsType(innerType, emptySchemaNames);
            return string.Equals(innerType, rewrittenInner, StringComparison.Ordinal) ? cSharpType : $"List<{rewrittenInner}>";
        }

        return cSharpType;
    }

    private static bool IsNestedListType(string cSharpType)
    {
        return cSharpType.StartsWith("List<", StringComparison.Ordinal) &&
               cSharpType.EndsWith(">", StringComparison.Ordinal) &&
               cSharpType["List<".Length..^1].StartsWith("List<", StringComparison.Ordinal);
    }

    private static void DebugSchemaSkip(string schemaName, string reason)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ODCFORGE_DEBUG_SCHEMA_SKIPS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        Console.WriteLine($"SKIP SCHEMA {schemaName}: {reason}");
    }

    private static void RegisterNestedInlineObjectSchema(
        string parentSchemaName,
        string propertyName,
        IOpenApiSchema schema,
        IList<(string SchemaName, OpenApiSchema ConcreteSchema, string DefaultStructureName, string StructureName, string Description, string? KiotaModelType, bool IsRequestShape)> componentSchemas,
        IDictionary<IOpenApiSchema, string> schemaTypeNamesByReference,
        IDictionary<string, string> schemaTypeNamesByName,
        ISet<string> usedStructureNames)
    {
        if (HasSchemaType(schema, JsonSchemaType.Array) && schema.Items is not null)
        {
            RegisterNestedInlineObjectSchema($"{parentSchemaName}_{propertyName}", $"{propertyName}Item", schema.Items, componentSchemas, schemaTypeNamesByReference, schemaTypeNamesByName, usedStructureNames);
            return;
        }

        var resolvedSchema = TryResolveSingleNonNullCandidate(schema) ?? schema;
        var concreteSchema = ResolveConcreteSchema(resolvedSchema, $"property '{propertyName}' on schema '{parentSchemaName}'");
        if (!IsObjectLikeSchema(concreteSchema) ||
            TryGetReferenceId(resolvedSchema) is not null ||
            TryGetReferenceId(concreteSchema) is not null ||
            schemaTypeNamesByReference.ContainsKey(resolvedSchema) ||
            schemaTypeNamesByReference.ContainsKey(concreteSchema))
        {
            return;
        }

        var syntheticSchemaName = $"{parentSchemaName}_{propertyName}";
        if (string.IsNullOrWhiteSpace(concreteSchema.Title))
        {
            concreteSchema.Title = syntheticSchemaName;
        }

        var defaultStructureName = OpenApiNamingPolicy.CreateStructureName(concreteSchema.Title);
        var structureName = AllocateUniqueName(defaultStructureName, usedStructureNames);
        schemaTypeNamesByReference[resolvedSchema] = structureName;
        schemaTypeNamesByReference[concreteSchema] = structureName;
        schemaTypeNamesByName[syntheticSchemaName] = structureName;
        schemaTypeNamesByName[concreteSchema.Title] = structureName;
        componentSchemas.Add((
            syntheticSchemaName,
            concreteSchema,
            defaultStructureName,
            structureName,
            NormalizeDescription($"Represents the {propertyName} field on {parentSchemaName}."),
            null,
            false));
    }

    private static void DebugOperationSkip(string path, HttpMethod operationType, string? operationId, string reason)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ODCFORGE_DEBUG_OPERATION_SKIPS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        Console.Error.WriteLine($"SKIP {operationType} {path} ({operationId ?? "<no operationId>"}): {reason}");
    }
}

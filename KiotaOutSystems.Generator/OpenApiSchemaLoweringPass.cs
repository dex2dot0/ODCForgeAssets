using Microsoft.OpenApi;
namespace KiotaOutSystems.Generator;

internal static class OpenApiSchemaLoweringPass
{
    private const string ProxyExtensionName = "x-odcforge-proxy";

    public static OpenApiDocument Lower(OpenApiDocument document)
    {
        var loweredDocument = new OpenApiDocument(document);
        loweredDocument.Components ??= new OpenApiComponents();
        loweredDocument.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        var context = new LoweringContext(loweredDocument);
        LowerComponentSchemas(context);
        LowerPathSchemas(context);
        loweredDocument.SetReferenceHostDocument();
        return loweredDocument;
    }

    public static bool IsProxySchema(IOpenApiSchema schema)
    {
        return schema.Extensions is not null &&
               schema.Extensions.TryGetValue(ProxyExtensionName, out var extension) &&
               extension is OpenApiMarkerExtension;
    }

    private static void LowerComponentSchemas(LoweringContext context)
    {
        var componentSchemas = context.Document.Components?.Schemas;
        if (componentSchemas is null)
        {
            return;
        }

        foreach (var schemaName in componentSchemas.Keys.ToList())
        {
            if (componentSchemas[schemaName] is OpenApiSchema componentSchema &&
                string.IsNullOrWhiteSpace(componentSchema.Id))
            {
                componentSchema.Id = schemaName;
            }

            context.AssignSchemaName(componentSchemas[schemaName], schemaName);
            context.AssignSchemaName(ResolveSchema(componentSchemas[schemaName]), schemaName);

            context.BeginProcessing(schemaName);
            try
            {
                componentSchemas[schemaName] = LowerSchema(
                    context,
                    componentSchemas[schemaName],
                    schemaName,
                    schemaName,
                    preserveComponentName: true);
            }
            finally
            {
                context.EndProcessing(schemaName);
            }
        }
    }

    private static void LowerPathSchemas(LoweringContext context)
    {
        foreach (var (_, pathItem) in context.Document.Paths)
        {
            LowerParameterCollection(context, pathItem.Parameters, "pathItem");

            foreach (var (_, operation) in pathItem.Operations ?? [])
            {
                LowerParameterCollection(context, operation.Parameters, operation.OperationId ?? "operation");
                LowerRequestBody(context, operation.RequestBody, operation.OperationId ?? "operation");
                LowerResponses(context, operation.Responses, operation.OperationId ?? "operation");
            }
        }
    }

    private static void LowerParameterCollection(LoweringContext context, IList<IOpenApiParameter>? parameters, string scopeName)
    {
        if (parameters is null)
        {
            return;
        }

        for (var index = 0; index < parameters.Count; index++)
        {
            if (parameters[index] is not OpenApiParameter parameter || parameter.Schema is null)
            {
                continue;
            }

            parameter.Schema = LowerParameterSchema(context, parameter.Schema, $"{scopeName}_{parameter.Name}", $"{scopeName}_{parameter.Name}");
        }
    }

    private static IOpenApiSchema LowerParameterSchema(
        LoweringContext context,
        IOpenApiSchema schema,
        string suggestedName,
        string contextName)
    {
        if (TryResolveSingleNonNullCandidate(schema) is { } nonNullCandidate)
        {
            return LowerParameterSchema(context, nonNullCandidate, suggestedName, contextName);
        }

        var concreteSchema = ResolveSchema(schema);
        if (context.IsActivelyLoweringSchema(concreteSchema))
        {
            return CreateActiveSchemaBreak(context, schema, concreteSchema);
        }

        var activeReferenceId = TryGetTraversalSchemaName(context, schema, concreteSchema);
        if (activeReferenceId is not null && context.IsActivelyLowering(activeReferenceId))
        {
            return CreateSchemaReference(context.Document, activeReferenceId);
        }

        var tracksActiveReference = activeReferenceId is not null;
        context.BeginActiveLoweringSchema(concreteSchema);
        if (tracksActiveReference)
        {
            context.BeginActiveLowering(activeReferenceId!);
        }

        try
        {
        if (HasSchemaType(concreteSchema, JsonSchemaType.Array) && concreteSchema.Items is not null)
        {
            concreteSchema.Items = LowerNestedSchema(context, concreteSchema.Items, $"{suggestedName}_item", $"{contextName}_item", lowerAsParameterSchema: true);
        }

        if ((concreteSchema.Properties?.Count ?? 0) > 0)
        {
            var properties = concreteSchema.Properties!;
            foreach (var propertyName in properties.Keys.ToList())
            {
                properties[propertyName] = LowerSchema(
                    context,
                    properties[propertyName],
                    $"{suggestedName}_{propertyName}",
                    $"{contextName}_{propertyName}");
            }
        }

        return concreteSchema;
        }
        finally
        {
            context.EndActiveLoweringSchema(concreteSchema);
            if (tracksActiveReference)
            {
                context.EndActiveLowering(activeReferenceId!);
            }
        }
    }

    private static void LowerRequestBody(LoweringContext context, IOpenApiRequestBody? requestBody, string scopeName)
    {
        if (requestBody?.Content is null)
        {
            return;
        }

        foreach (var mediaType in requestBody.Content.Values.OfType<OpenApiMediaType>())
        {
            if (mediaType.Schema is null)
            {
                continue;
            }

            mediaType.Schema = LowerSchema(context, mediaType.Schema, $"{scopeName}_request", $"{scopeName}_request");
        }
    }

    private static void LowerResponses(LoweringContext context, OpenApiResponses? responses, string scopeName)
    {
        if (responses is null)
        {
            return;
        }

        foreach (var (statusCode, response) in responses)
        {
            if (response.Content is null)
            {
                continue;
            }

            foreach (var mediaType in response.Content.Values.OfType<OpenApiMediaType>())
            {
                if (mediaType.Schema is null)
                {
                    continue;
                }

                mediaType.Schema = LowerSchema(context, mediaType.Schema, $"{scopeName}_{statusCode}_response", $"{scopeName}_{statusCode}_response");
            }
        }
    }

    private static IOpenApiSchema LowerSchema(
        LoweringContext context,
        IOpenApiSchema schema,
        string suggestedName,
        string contextName,
        bool preserveComponentName = false)
    {
        var concreteSchema = ResolveSchema(schema);
        if (context.IsActivelyLoweringSchema(concreteSchema))
        {
            return CreateActiveSchemaBreak(context, schema, concreteSchema);
        }

        var activeReferenceId = TryGetTraversalSchemaName(context, schema, concreteSchema);
        if (activeReferenceId is not null && context.IsActivelyLowering(activeReferenceId))
        {
            return CreateSchemaReference(context.Document, activeReferenceId);
        }

        var tracksActiveReference = activeReferenceId is not null;
        context.BeginActiveLoweringSchema(concreteSchema);
        if (tracksActiveReference)
        {
            context.BeginActiveLowering(activeReferenceId!);
        }

        try
        {

        if (TryCreateProxySchema(context, schema, suggestedName, contextName, preserveComponentName) is { } proxySchema)
        {
            return proxySchema;
        }

        if (HasSchemaType(concreteSchema, JsonSchemaType.Array) && concreteSchema.Items is not null)
        {
            concreteSchema.Items = LowerNestedSchema(context, concreteSchema.Items, $"{suggestedName}_item", $"{contextName}_item");
        }

        if ((concreteSchema.AllOf?.Count ?? 0) > 0)
        {
            var mergedSchema = MergeAllOfSchema(context, concreteSchema, suggestedName, contextName);
            if (mergedSchema is not null)
            {
                return mergedSchema;
            }
        }

        if ((concreteSchema.Properties?.Count ?? 0) > 0)
        {
            var properties = concreteSchema.Properties!;
            foreach (var propertyName in properties.Keys.ToList())
            {
                properties[propertyName] = LowerNestedSchema(
                    context,
                    properties[propertyName],
                    $"{suggestedName}_{propertyName}",
                    $"{contextName}_{propertyName}");
            }
        }

        return concreteSchema;
        }
        finally
        {
            context.EndActiveLoweringSchema(concreteSchema);
            if (tracksActiveReference)
            {
                context.EndActiveLowering(activeReferenceId!);
            }
        }
    }

    private static OpenApiSchema? MergeAllOfSchema(LoweringContext context, OpenApiSchema schema, string suggestedName, string contextName)
    {
        var objectCandidates = schema.AllOf!
            .Select(ResolveSchema)
            .ToList();

        if (objectCandidates.Count == 0 || objectCandidates.Any(candidate => !HasSchemaType(candidate, JsonSchemaType.Object)))
        {
            return null;
        }

        var mergedSchema = CreateSchemaCopy(schema);
        mergedSchema.AllOf = new List<IOpenApiSchema>();
        mergedSchema.Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.OrdinalIgnoreCase);
        mergedSchema.Required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in objectCandidates)
        {
            foreach (var property in candidate.Properties ?? new Dictionary<string, IOpenApiSchema>())
            {
                mergedSchema.Properties[property.Key] = LowerSchema(
                    context,
                    property.Value,
                    $"{suggestedName}_{property.Key}",
                    $"{contextName}_{property.Key}");
            }

            if (candidate.Required is not null)
            {
                foreach (var requiredProperty in candidate.Required)
                {
                    if (!mergedSchema.Required.Contains(requiredProperty, StringComparer.OrdinalIgnoreCase))
                    {
                        mergedSchema.Required.Add(requiredProperty);
                    }
                }
            }
        }

        return mergedSchema;
    }

    private static IOpenApiSchema? TryCreateProxySchema(
        LoweringContext context,
        IOpenApiSchema originalSchema,
        string suggestedName,
        string contextName,
        bool preserveComponentName)
    {
        var candidates = EnumerateComposedCandidates(originalSchema)
            .Where(candidate => !HasSchemaType(candidate, JsonSchemaType.Null))
            .ToList();

        if (candidates.Count < 2)
        {
            return null;
        }

        var componentName = preserveComponentName
            ? suggestedName
            : context.GetOrCreateSyntheticSchemaName(suggestedName);

        if (!preserveComponentName && context.IsProcessing(componentName))
        {
            return CreateSchemaReference(context.Document, componentName);
        }

        var componentSchemas = context.Document.Components?.Schemas;
        if (componentSchemas is not null &&
            componentSchemas.TryGetValue(componentName, out var existingSchema) &&
            IsProxySchema(existingSchema))
        {
            return preserveComponentName
                ? ResolveSchema(existingSchema)
                : CreateSchemaReference(context.Document, componentName);
        }

        var proxySchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.OrdinalIgnoreCase),
            Required = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            Extensions = new Dictionary<string, IOpenApiExtension>(StringComparer.OrdinalIgnoreCase),
            Description = ResolveSchema(originalSchema).Description,
            Title = OpenApiNamingPolicy.CreateInternalStructureName(componentName)
        };
        proxySchema.Extensions[ProxyExtensionName] = new OpenApiMarkerExtension();
        proxySchema.Properties["object_type"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Proxy discriminator-like value."
        };

        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            var branchSourceName = GetBranchSourceName(candidate, index);
            var loweredBranchSchema = LowerProxyBranchSchema(context, candidate, $"{componentName}_{branchSourceName}", $"{contextName}_{branchSourceName}");
            proxySchema.Properties[branchSourceName] = loweredBranchSchema;
        }

        componentSchemas ??= context.Document.Components?.Schemas
            ?? throw new InvalidOperationException("The lowered document is missing its component schema collection.");
        componentSchemas[componentName] = proxySchema;

        return preserveComponentName
            ? proxySchema
            : CreateSchemaReference(context.Document, componentName);
    }

    private static IOpenApiSchema EnsureComponentSchema(LoweringContext context, IOpenApiSchema schema, string suggestedName, string contextName)
    {
        var componentSchemas = context.Document.Components?.Schemas
            ?? throw new InvalidOperationException("The lowered document is missing its component schema collection.");

        if (TryGetReferenceId(schema) is { Length: > 0 } referenceId)
        {
            return CreateSchemaReference(context.Document, referenceId);
        }

        var resolvedSchema = ResolveSchema(schema);
        if (TryGetReferenceId(resolvedSchema) is { Length: > 0 } resolvedReferenceId)
        {
            return CreateSchemaReference(context.Document, resolvedReferenceId);
        }

        if ((context.TryGetAssignedSchemaName(schema, out var assignedSchemaName) ||
             context.TryGetAssignedSchemaName(resolvedSchema, out assignedSchemaName)) &&
            componentSchemas.ContainsKey(assignedSchemaName))
        {
            return CreateSchemaReference(context.Document, assignedSchemaName);
        }

        var syntheticName = context.GetOrCreateSyntheticSchemaName(suggestedName);
        if (context.IsProcessing(syntheticName))
        {
            return CreateSchemaReference(context.Document, syntheticName);
        }

        if (!componentSchemas.ContainsKey(syntheticName))
        {
            context.AssignSchemaName(schema, syntheticName);
            context.AssignSchemaName(resolvedSchema, syntheticName);
            context.BeginProcessing(syntheticName);
            componentSchemas[syntheticName] = new OpenApiSchema
            {
                Id = syntheticName,
                Title = syntheticName
            };

            try
            {
                var loweredSchema = LowerSchema(context, schema, syntheticName, contextName, preserveComponentName: true);
                componentSchemas[syntheticName] = loweredSchema;
            }
            finally
            {
                context.EndProcessing(syntheticName);
            }
        }

        return CreateSchemaReference(context.Document, syntheticName);
    }

    private static IOpenApiSchema LowerProxyBranchSchema(LoweringContext context, IOpenApiSchema schema, string suggestedName, string contextName)
    {
        var resolvedSchema = ResolveSchema(schema);
        if (HasSchemaType(resolvedSchema, JsonSchemaType.Object) || OpenApiSchemaLoweringPass.IsProxySchema(resolvedSchema))
        {
            return EnsureComponentSchema(context, schema, suggestedName, contextName);
        }

        return LowerSchema(context, schema, suggestedName, contextName);
    }

    private static IOpenApiSchema CreateSchemaReference(OpenApiDocument document, string schemaId)
    {
        return new OpenApiSchema
        {
            Id = schemaId,
            Title = schemaId
        };
    }

    private static string GetBranchSourceName(IOpenApiSchema schema, int index)
    {
        return TryGetReferenceId(schema)
            ?? TryGetReferenceId(ResolveSchema(schema))
            ?? (schema is OpenApiSchema concreteSchema && !string.IsNullOrWhiteSpace(concreteSchema.Title) ? concreteSchema.Title : $"option_{index + 1}");
    }

    private static IEnumerable<IOpenApiSchema> EnumerateComposedCandidates(IOpenApiSchema schema)
    {
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

    private static IOpenApiSchema? TryResolveSingleNonNullCandidate(IOpenApiSchema schema)
    {
        var candidates = EnumerateComposedCandidates(schema)
            .Where(candidate => !HasSchemaType(candidate, JsonSchemaType.Null))
            .ToList();

        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static OpenApiSchema ResolveSchema(IOpenApiSchema schema)
    {
        return schema switch
        {
            OpenApiSchemaReference reference when reference.RecursiveTarget is OpenApiSchema recursiveTarget => recursiveTarget,
            OpenApiSchemaReference reference when reference.Target is OpenApiSchema target => target,
            OpenApiSchema concreteSchema => concreteSchema,
            _ => CreateSchemaCopy(schema)
        };
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

    private static string? TryGetTraversalSchemaName(LoweringContext context, IOpenApiSchema schema, IOpenApiSchema? resolvedSchema = null)
    {
        resolvedSchema ??= ResolveSchema(schema);

        return TryGetReferenceId(schema)
            ?? TryGetReferenceId(resolvedSchema)
            ?? (context.TryGetAssignedSchemaName(schema, out var schemaName) ? schemaName : null)
            ?? (context.TryGetAssignedSchemaName(resolvedSchema, out var resolvedSchemaName) ? resolvedSchemaName : null);
    }

    private static IOpenApiSchema CreateActiveSchemaBreak(LoweringContext context, IOpenApiSchema schema, IOpenApiSchema resolvedSchema)
    {
        if (TryGetTraversalSchemaName(context, schema, resolvedSchema) is { Length: > 0 } schemaName)
        {
            return CreateSchemaReference(context.Document, schemaName);
        }

        return CreateSchemaCopy(resolvedSchema);
    }

    private static IOpenApiSchema LowerNestedSchema(
        LoweringContext context,
        IOpenApiSchema schema,
        string suggestedName,
        string contextName,
        bool lowerAsParameterSchema = false)
    {
        if (TryGetReferenceId(schema) is { Length: > 0 } schemaReferenceId)
        {
            return CreateSchemaReference(context.Document, schemaReferenceId);
        }

        return lowerAsParameterSchema
            ? LowerParameterSchema(context, schema, suggestedName, contextName)
            : LowerSchema(context, schema, suggestedName, contextName);
    }

    private static bool HasSchemaType(IOpenApiSchema? schema, JsonSchemaType expectedType)
    {
        return schema?.Type is JsonSchemaType actualType && (actualType & expectedType) == expectedType;
    }

    private static OpenApiSchema CreateSchemaCopy(IOpenApiSchema schema)
    {
        return new OpenApiSchema
        {
            Type = schema.Type,
            Format = schema.Format,
            Description = schema.Description,
            Title = schema.Title,
            Properties = schema.Properties is null
                ? null
                : new Dictionary<string, IOpenApiSchema>(schema.Properties, StringComparer.OrdinalIgnoreCase),
            Required = schema.Required is null
                ? null
                : new HashSet<string>(schema.Required, StringComparer.OrdinalIgnoreCase),
            Items = schema.Items,
            AnyOf = schema.AnyOf is null ? null : new List<IOpenApiSchema>(schema.AnyOf),
            OneOf = schema.OneOf is null ? null : new List<IOpenApiSchema>(schema.OneOf),
            AllOf = schema.AllOf is null ? null : new List<IOpenApiSchema>(schema.AllOf),
            AdditionalProperties = schema.AdditionalProperties,
            AdditionalPropertiesAllowed = schema.AdditionalPropertiesAllowed
        };
    }

    private sealed class LoweringContext(OpenApiDocument document)
    {
        private readonly HashSet<string> _syntheticNames = new(document.Components?.Schemas?.Keys ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _processingNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activeLoweringReferences = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<IOpenApiSchema> _activeLoweringSchemas = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IOpenApiSchema, string> _assignedSchemaNames = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

        public OpenApiDocument Document { get; } = document;

        public string GetOrCreateSyntheticSchemaName(string suggestedName)
        {
            var baseName = OpenApiNamingPolicy.CreateInternalStructureName(suggestedName);
            var candidate = baseName;
            var suffix = 1;
            while (!_syntheticNames.Add(candidate))
            {
                candidate = $"{baseName}{suffix++}";
            }

            return candidate;
        }

        public void BeginProcessing(string schemaName)
        {
            _processingNames.Add(schemaName);
        }

        public void EndProcessing(string schemaName)
        {
            _processingNames.Remove(schemaName);
        }

        public bool IsProcessing(string schemaName)
        {
            return _processingNames.Contains(schemaName);
        }

        public void BeginActiveLowering(string schemaName)
        {
            _activeLoweringReferences.Add(schemaName);
        }

        public void EndActiveLowering(string schemaName)
        {
            _activeLoweringReferences.Remove(schemaName);
        }

        public bool IsActivelyLowering(string schemaName)
        {
            return _activeLoweringReferences.Contains(schemaName);
        }

        public void BeginActiveLoweringSchema(IOpenApiSchema schema)
        {
            _activeLoweringSchemas.Add(schema);
        }

        public void EndActiveLoweringSchema(IOpenApiSchema schema)
        {
            _activeLoweringSchemas.Remove(schema);
        }

        public bool IsActivelyLoweringSchema(IOpenApiSchema schema)
        {
            return _activeLoweringSchemas.Contains(schema);
        }

        public void AssignSchemaName(IOpenApiSchema schema, string schemaName)
        {
            _assignedSchemaNames[schema] = schemaName;
        }

        public bool TryGetAssignedSchemaName(IOpenApiSchema schema, out string schemaName)
        {
            return _assignedSchemaNames.TryGetValue(schema, out schemaName!);
        }
    }

    private sealed class OpenApiMarkerExtension : IOpenApiExtension
    {
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteValue(true);
        }
    }
}

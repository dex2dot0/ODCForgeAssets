using Microsoft.OpenApi;

namespace KiotaOutSystems.Generator;

internal static class OpenApiOperationFilterPass
{
    public static OpenApiDocument Apply(OpenApiDocument document, EffectiveGeneratorConfig config)
    {
        if (config.Targets.Count == 0)
        {
            return document;
        }

        var filteredDocument = new OpenApiDocument(document);
        FilterPaths(filteredDocument, config.Targets);
        PruneSchemas(filteredDocument);
        return filteredDocument;
    }

    private static void FilterPaths(OpenApiDocument document, IReadOnlyList<EffectiveTargetConfig> targets)
    {
        foreach (var path in document.Paths.Keys.ToList())
        {
            var pathItem = document.Paths[path];
            if (pathItem.Operations is null)
            {
                document.Paths.Remove(path);
                continue;
            }

            foreach (var operationType in pathItem.Operations.Keys.ToList())
            {
                var method = operationType.ToString().ToUpperInvariant();
                if (!targets.Any(target => target.Matches(path, method)))
                {
                    pathItem.Operations.Remove(operationType);
                }
            }

            if (pathItem.Operations.Count == 0)
            {
                document.Paths.Remove(path);
            }
        }
    }

    private static void PruneSchemas(OpenApiDocument document)
    {
        var componentSchemas = document.Components?.Schemas;
        if (componentSchemas is null || componentSchemas.Count == 0)
        {
            return;
        }

        var reachableSchemaIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitedSchemas = new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance);

        foreach (var (_, pathItem) in document.Paths)
        {
            VisitParameterCollection(pathItem.Parameters, componentSchemas, reachableSchemaIds, visitedSchemas);

            foreach (var (_, operation) in pathItem.Operations ?? [])
            {
                VisitParameterCollection(operation.Parameters, componentSchemas, reachableSchemaIds, visitedSchemas);
                VisitRequestBody(operation.RequestBody, componentSchemas, reachableSchemaIds, visitedSchemas);
                VisitResponses(operation.Responses, componentSchemas, reachableSchemaIds, visitedSchemas);
            }
        }

        foreach (var schemaName in componentSchemas.Keys.ToList())
        {
            if (!reachableSchemaIds.Contains(schemaName))
            {
                componentSchemas.Remove(schemaName);
            }
        }
    }

    private static void VisitParameterCollection(
        IList<IOpenApiParameter>? parameters,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        HashSet<string> reachableSchemaIds,
        HashSet<IOpenApiSchema> visitedSchemas)
    {
        if (parameters is null)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            if (parameter.Schema is not null)
            {
                VisitSchema(parameter.Schema, componentSchemas, reachableSchemaIds, visitedSchemas);
            }
        }
    }

    private static void VisitRequestBody(
        IOpenApiRequestBody? requestBody,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        HashSet<string> reachableSchemaIds,
        HashSet<IOpenApiSchema> visitedSchemas)
    {
        if (requestBody?.Content is null)
        {
            return;
        }

        foreach (var mediaType in requestBody.Content.Values)
        {
            if (mediaType.Schema is not null)
            {
                VisitSchema(mediaType.Schema, componentSchemas, reachableSchemaIds, visitedSchemas);
            }
        }
    }

    private static void VisitResponses(
        OpenApiResponses? responses,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        HashSet<string> reachableSchemaIds,
        HashSet<IOpenApiSchema> visitedSchemas)
    {
        if (responses is null)
        {
            return;
        }

        foreach (var (_, response) in responses)
        {
            if (response.Content is null)
            {
                continue;
            }

            foreach (var mediaType in response.Content.Values)
            {
                if (mediaType.Schema is not null)
                {
                    VisitSchema(mediaType.Schema, componentSchemas, reachableSchemaIds, visitedSchemas);
                }
            }
        }
    }

    private static void VisitSchema(
        IOpenApiSchema schema,
        IDictionary<string, IOpenApiSchema> componentSchemas,
        HashSet<string> reachableSchemaIds,
        HashSet<IOpenApiSchema> visitedSchemas)
    {
        if (TryGetReferenceId(schema) is { Length: > 0 } schemaId)
        {
            if (!reachableSchemaIds.Add(schemaId))
            {
                return;
            }

            if (componentSchemas.TryGetValue(schemaId, out var componentSchema))
            {
                VisitSchema(componentSchema, componentSchemas, reachableSchemaIds, visitedSchemas);
            }
        }

        var resolvedSchema = ResolveSchema(schema);
        if (!visitedSchemas.Add(resolvedSchema))
        {
            return;
        }

        if (TryGetReferenceId(resolvedSchema) is { Length: > 0 } resolvedSchemaId &&
            reachableSchemaIds.Add(resolvedSchemaId) &&
            componentSchemas.TryGetValue(resolvedSchemaId, out var resolvedComponentSchema))
        {
            VisitSchema(resolvedComponentSchema, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        if (resolvedSchema.Items is not null)
        {
            VisitSchema(resolvedSchema.Items, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        foreach (var property in resolvedSchema.Properties?.Values ?? [])
        {
            VisitSchema(property, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        if (resolvedSchema.AdditionalProperties is not null)
        {
            VisitSchema(resolvedSchema.AdditionalProperties, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        foreach (var candidate in resolvedSchema.AllOf ?? [])
        {
            VisitSchema(candidate, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        foreach (var candidate in resolvedSchema.AnyOf ?? [])
        {
            VisitSchema(candidate, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        foreach (var candidate in resolvedSchema.OneOf ?? [])
        {
            VisitSchema(candidate, componentSchemas, reachableSchemaIds, visitedSchemas);
        }

        if (resolvedSchema.Not is not null)
        {
            VisitSchema(resolvedSchema.Not, componentSchemas, reachableSchemaIds, visitedSchemas);
        }
    }

    private static OpenApiSchema ResolveSchema(IOpenApiSchema schema)
    {
        return schema switch
        {
            OpenApiSchemaReference reference when reference.RecursiveTarget is OpenApiSchema recursiveTarget => recursiveTarget,
            OpenApiSchemaReference reference when reference.Target is OpenApiSchema target => target,
            OpenApiSchema concreteSchema => concreteSchema,
            _ => throw new NotSupportedException($"Unsupported schema runtime type '{schema.GetType().Name}'.")
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
}

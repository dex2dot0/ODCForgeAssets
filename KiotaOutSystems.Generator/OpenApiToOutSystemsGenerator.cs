using System.Text;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace KiotaOutSystems.Generator;

internal static class OpenApiToOutSystemsGenerator
{
    public static GenerationResult Generate(GeneratorOptions options, LoadedSpec loadedSpec)
    {
        var config = GeneratorConfigLoader.Load(options.ConfigPath);
        var resolvedOptions = ResolveOptions(options, config);
        var kiotaMetadata = global::KiotaOutSystems.Generator.KiotaMetadataLoader.Load(resolvedOptions);
        var generationNamespace = resolvedOptions.Namespace ?? "Generated.OutSystems";
        var className = resolvedOptions.ClassName ?? "GeneratedApiActions";
        var interfaceName = resolvedOptions.InterfaceName ?? $"I{className}";
        var inputName = resolvedOptions.InputName ?? loadedSpec.Document.Info?.Title ?? "Generated API";
        var emitInterface = resolvedOptions.EmitInterface ?? true;
        var iconResourceName = config.Icon?.ResourceName;

        var schemas = BuildSchemas(loadedSpec.Document, kiotaMetadata, config);
        var operations = BuildOperations(loadedSpec.Document, schemas, config);

        ValidateGenerationTarget(resolvedOptions.OutputDirectory, emitInterface);
        ValidateNoNormalizedNameCollisions(schemas, operations);

        Directory.CreateDirectory(resolvedOptions.OutputDirectory);

        var generatedFiles = new List<string>
        {
            WriteFile(resolvedOptions.OutputDirectory, "GeneratedStructures.g.cs", RenderStructures(generationNamespace, schemas)),
            WriteFile(resolvedOptions.OutputDirectory, "GeneratedActions.g.cs", RenderActions(generationNamespace, className, interfaceName, inputName, schemas, operations, kiotaMetadata, emitInterface, iconResourceName))
        };

        var manifest = new
        {
            inputName,
            source = loadedSpec.ResolvedSource,
            specHash = loadedSpec.ContentHash,
            kiotaMetadata.ClientNamespace,
            kiotaMetadata.ClientClassName,
            kiotaMetadata.KiotaVersion,
            emitInterface,
            iconResourceName,
            structures = schemas.Select(schema => schema.Name).ToArray(),
            operations = operations.Select(operation => operation.Name).ToArray()
        };

        generatedFiles.Add(WriteFile(
            resolvedOptions.OutputDirectory,
            "generation-manifest.json",
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true })));

        return new GenerationResult(generatedFiles, operations.Count, schemas.Count);
    }

    private static IReadOnlyList<SchemaDefinition> BuildSchemas(OpenApiDocument document, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        var schemas = new List<SchemaDefinition>();

        foreach (var (schemaName, schema) in document.Components.Schemas)
        {
            if (schema.Type != "object")
            {
                throw new NotSupportedException($"Schema '{schemaName}' is not supported because only object component schemas are supported in v1.");
            }

            var defaultStructureName = ToPascalCase(schemaName);
            var structureOverride = FindStructureOverride(config, schemaName, defaultStructureName);
            var structureName = structureOverride?.Name ?? defaultStructureName;
            var properties = schema.Properties
                .Select(property => CreatePropertyDefinition(schemaName, defaultStructureName, property.Key, property.Value, schema.Required, config))
                .ToList();

            schemas.Add(new SchemaDefinition(
                structureName,
                defaultStructureName,
                schemaName,
                $"{kiotaMetadata.ClientNamespace}.Models.{defaultStructureName}",
                structureOverride?.Description ?? $"Represents the {schemaName} schema from the source OpenAPI document.",
                false,
                properties));
        }

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                if (operation.RequestBody is null || !operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
                {
                    continue;
                }

                var requestSchema = mediaType.Schema;
                if (requestSchema.Reference is null || !document.Components.Schemas.TryGetValue(requestSchema.Reference.Id, out var componentSchema))
                {
                    throw new NotSupportedException("Only referenced component object schemas are supported for JSON request bodies in v1.");
                }

                var resourcePropertyName = ToPascalCase(path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "resource");
                var defaultOperationName = CreateOperationName(operationType, path, resourcePropertyName, operation.OperationId);
                var actionOverride = FindActionOverride(config, operationType, path, defaultOperationName);
                var operationName = actionOverride?.Name ?? defaultOperationName;

                var syntheticSchemaName = $"{operationName}Request";
                if (schemas.Any(schema => string.Equals(schema.Name, syntheticSchemaName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var structureOverride = FindStructureOverride(config, syntheticSchemaName, syntheticSchemaName);
                var structureName = structureOverride?.Name ?? syntheticSchemaName;
                var properties = componentSchema.Properties
                    .Select(property => CreatePropertyDefinition(syntheticSchemaName, syntheticSchemaName, property.Key, property.Value, componentSchema.Required, config))
                    .ToList();

                schemas.Add(new SchemaDefinition(
                    structureName,
                    syntheticSchemaName,
                    syntheticSchemaName,
                    $"{kiotaMetadata.ClientNamespace}.Models.{ToPascalCase(requestSchema.Reference.Id)}",
                    structureOverride?.Description ?? NormalizeDescription($"Represents the request body for {operationName}."),
                    true,
                    properties));
            }
        }

        return schemas;
    }

    private static IReadOnlyList<OperationDefinition> BuildOperations(OpenApiDocument document, IReadOnlyList<SchemaDefinition> schemas, EffectiveGeneratorConfig config)
    {
        var schemaNames = schemas.ToDictionary(schema => schema.OriginalSchemaName, schema => schema.Name, StringComparer.OrdinalIgnoreCase);
        var operations = new List<OperationDefinition>();

        foreach (var (path, pathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in pathItem.Operations)
            {
                var parameters = pathItem.Parameters
                    .Concat(operation.Parameters)
                    .Select(parameter => ResolveParameter(parameter, schemaNames))
                    .ToList();

                var resourcePropertyName = ToPascalCase(path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "resource");
                var defaultOperationName = CreateOperationName(operationType, path, resourcePropertyName, operation.OperationId);
                var actionOverride = FindActionOverride(config, operationType, path, defaultOperationName);
                var operationName = actionOverride?.Name ?? defaultOperationName;
                var requestBody = operation.RequestBody is null ? null : ResolveBody(operation.RequestBody, schemaNames, operationName);
                var response = ResolveResponse(operation.Responses, schemaNames);
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
                    Response: response));
            }
        }

        return operations;
    }

    private static ParameterDefinition ResolveParameter(OpenApiParameter parameter, IReadOnlyDictionary<string, string> schemaNames)
    {
        var type = MapSchemaType(parameter.Schema, schemaNames, "parameter");
        var isNullable = !parameter.Required || IsNullableSchema(parameter.Schema);
        var queryProperty = parameter.In == ParameterLocation.Query ? ToPascalCase(parameter.Name) : null;
        var requiresPresenceFlag = parameter.In == ParameterLocation.Query && !parameter.Required;
        var presenceFlagName = requiresPresenceFlag ? $"include{ToPascalCase(SanitizeIdentifier(parameter.Name))}" : null;

        return new ParameterDefinition(
            Name: ToCamelCase(SanitizeIdentifier(parameter.Name)),
            DefaultName: ToCamelCase(SanitizeIdentifier(parameter.Name)),
            OriginalName: parameter.Name,
            Location: parameter.In?.ToString().ToLowerInvariant() ?? "query",
            CSharpType: type,
            IsNullable: isNullable,
            Description: NormalizeParameterDescription(parameter),
            QueryPropertyName: queryProperty,
            RequiresPresenceFlag: requiresPresenceFlag,
            PresenceFlagName: presenceFlagName);
    }

    private static BodyDefinition ResolveBody(OpenApiRequestBody requestBody, IReadOnlyDictionary<string, string> schemaNames, string operationName)
    {
        var schema = requestBody.Content.TryGetValue("application/json", out var mediaType)
            ? mediaType.Schema
            : throw new NotSupportedException("Only application/json request bodies are supported in v1.");

        var schemaName = ResolveSchemaName(schema, schemaNames, "request body");
        return new BodyDefinition($"{operationName}Request", schemaName);
    }

    private static ResponseDefinition? ResolveResponse(OpenApiResponses responses, IReadOnlyDictionary<string, string> schemaNames)
    {
        foreach (var statusCode in new[] { "200", "201", "202", "204", "default" })
        {
            if (!responses.TryGetValue(statusCode, out var response))
            {
                continue;
            }

            if (!response.Content.TryGetValue("application/json", out var mediaType))
            {
                return new ResponseDefinition("void", null);
            }

            var schema = mediaType.Schema;
            if (schema.Type == "array")
            {
                return new ResponseDefinition("array", ResolveSchemaName(schema.Items, schemaNames, "array response"));
            }

            return new ResponseDefinition("object", ResolveSchemaName(schema, schemaNames, "response"));
        }

        return null;
    }

    private static PropertyDefinition CreatePropertyDefinition(
        string sourceSchemaName,
        string defaultStructureName,
        string propertyName,
        OpenApiSchema schema,
        ISet<string> requiredProperties,
        EffectiveGeneratorConfig config)
    {
        var defaultPropertyName = ToPascalCase(propertyName);
        var propertyOverride = FindStructureFieldOverride(config, sourceSchemaName, defaultStructureName, propertyName, defaultPropertyName);
        var cSharpType = MapSchemaType(schema, new Dictionary<string, string>(), $"property '{propertyName}'");
        var isNullable = !requiredProperties.Contains(propertyName) || IsNullableSchema(schema);

        return new PropertyDefinition(
            Name: propertyOverride?.Name ?? defaultPropertyName,
            DefaultName: defaultPropertyName,
            OriginalName: propertyName,
            CSharpType: cSharpType,
            IsNullable: isNullable,
            Description: propertyOverride?.Description ?? NormalizeDescription(schema.Description ?? $"Maps the '{propertyName}' field."));
    }

    private static string RenderStructures(string generationNamespace, IReadOnlyList<SchemaDefinition> schemas)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using OutSystems.ExternalLibraries.SDK;");
        builder.AppendLine();
        builder.AppendLine($"namespace {generationNamespace};");
        builder.AppendLine();

        foreach (var schema in schemas)
        {
            builder.Append($"[OSStructure(Description = \"{EscapeString(schema.Description)}\"");
            if (!string.Equals(schema.Name, schema.DefaultName, StringComparison.Ordinal))
            {
                builder.Append($", OriginalName = \"{EscapeString(schema.DefaultName)}\"");
            }
            builder.AppendLine(")]");
            builder.AppendLine($"public struct {schema.Name}");
            builder.AppendLine("{");

            foreach (var property in schema.Properties)
            {
                builder.Append($"    [OSStructureField(Description = \"{EscapeString(property.Description)}\"");
                if (!string.Equals(property.Name, property.DefaultName, StringComparison.Ordinal))
                {
                    builder.Append($", OriginalName = \"{EscapeString(property.DefaultName)}\"");
                }
                builder.AppendLine(")]");
                builder.AppendLine($"    public {FormatType(property.CSharpType, property.IsNullable)} {property.Name} {{ get; set; }}");
                builder.AppendLine();
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static string RenderActions(
        string generationNamespace,
        string className,
        string interfaceName,
        string inputName,
        IReadOnlyList<SchemaDefinition> schemas,
        IReadOnlyList<OperationDefinition> operations,
        KiotaMetadata kiotaMetadata,
        bool emitInterface,
        string? iconResourceName)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using Microsoft.Kiota.Abstractions.Authentication;");
        builder.AppendLine("using Microsoft.Kiota.Http.HttpClientLibrary;");
        builder.AppendLine("using Microsoft.Kiota.Serialization.Json;");
        builder.AppendLine("using OutSystems.ExternalLibraries.SDK;");
        builder.AppendLine();
        builder.AppendLine($"namespace {generationNamespace};");
        builder.AppendLine();
        builder.AppendLine("internal static class GeneratedModelMapper");
        builder.AppendLine("{");

        foreach (var schema in schemas)
        {
            if (!schema.IsRequestShape)
            {
                builder.AppendLine($"    public static {schema.Name} ToStructure({schema.KiotaModelType}? model)");
                builder.AppendLine("    {");
                builder.AppendLine("        if (model is null)");
                builder.AppendLine("        {");
                builder.AppendLine("            return default;");
                builder.AppendLine("        }");
                builder.AppendLine();
                builder.AppendLine($"        return new {schema.Name}");
                builder.AppendLine("        {");
                foreach (var property in schema.Properties)
                {
                    builder.AppendLine($"            {property.Name} = model.{property.Name},");
                }
                builder.AppendLine("        };");
                builder.AppendLine("    }");
                builder.AppendLine();
            }

            builder.AppendLine($"    public static {schema.KiotaModelType} ToModel({schema.Name} structure)");
            builder.AppendLine("    {");
            builder.AppendLine($"        return new {schema.KiotaModelType}");
            builder.AppendLine("        {");
            foreach (var property in schema.Properties)
            {
                builder.AppendLine($"            {property.Name} = structure.{property.Name},");
            }
            builder.AppendLine("        };");
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        builder.AppendLine();
        var implementedInterfaceClause = emitInterface ? $" : {interfaceName}" : string.Empty;
        builder.AppendLine($"public class {className}{implementedInterfaceClause}");
        builder.AppendLine("{");
        builder.AppendLine($"    private readonly {kiotaMetadata.ClientNamespace}.{kiotaMetadata.ClientClassName} _client;");
        builder.AppendLine();
        builder.AppendLine($"    public {className}()");
        builder.AppendLine("    {");
        builder.AppendLine("        var requestAdapter = new HttpClientRequestAdapter(");
        builder.AppendLine("            new AnonymousAuthenticationProvider(),");
        builder.AppendLine("            new JsonParseNodeFactory(),");
        builder.AppendLine("            new JsonSerializationWriterFactory(),");
        builder.AppendLine("            new HttpClient(),");
        builder.AppendLine("            null);");
        builder.AppendLine();
        builder.AppendLine($"        _client = new {kiotaMetadata.ClientNamespace}.{kiotaMetadata.ClientClassName}(requestAdapter);");
        builder.AppendLine("    }");
        builder.AppendLine();

        foreach (var operation in operations)
        {
            builder.AppendLine(RenderClassMethod(operation));
            builder.AppendLine();
        }

        builder.AppendLine("}");
        builder.AppendLine();
        if (emitInterface)
        {
            builder.Append($"[OSInterface(Description = \"Generated OutSystems wrapper for {EscapeString(inputName)}.\", Name = \"{EscapeString(className)}\"");
            if (!string.IsNullOrWhiteSpace(iconResourceName))
            {
                builder.Append($", IconResourceName = \"{EscapeString(iconResourceName)}\"");
            }
            builder.AppendLine(")]");
            builder.AppendLine($"public interface {interfaceName}");
            builder.AppendLine("{");

            foreach (var operation in operations)
            {
                builder.AppendLine(RenderInterfaceMethod(operation));
                builder.AppendLine();
            }

            builder.AppendLine("}");
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static string RenderClassMethod(OperationDefinition operation)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"    public {GetReturnType(operation)} {operation.Name}({RenderMethodParameters(operation)})");
        builder.AppendLine("    {");
        builder.AppendLine($"        var requestBuilder = _client.{operation.ResourcePropertyName};");

        var pathParameter = operation.Parameters.FirstOrDefault(parameter => parameter.Location == "path");
        if (pathParameter is not null)
        {
            builder.AppendLine($"        var itemRequestBuilder = requestBuilder[{pathParameter.Name}];");
        }

        switch (operation.HttpMethod)
        {
            case "GET":
                if (operation.Response?.Kind == "array")
                {
                    builder.AppendLine("        var response = requestBuilder.GetAsync(config =>");
                    builder.AppendLine("        {");
                    foreach (var parameter in operation.Parameters.Where(parameter => parameter.Location == "query"))
                    {
                        if (parameter.RequiresPresenceFlag)
                        {
                            builder.AppendLine($"            if ({parameter.PresenceFlagName})");
                            builder.AppendLine("            {");
                            builder.AppendLine($"                config.QueryParameters.{parameter.QueryPropertyName} = {parameter.Name};");
                            builder.AppendLine("            }");
                        }
                        else
                        {
                            builder.AppendLine($"            config.QueryParameters.{parameter.QueryPropertyName} = {parameter.Name};");
                        }
                    }
                    builder.AppendLine("        }).GetAwaiter().GetResult();");
                    builder.AppendLine();
                    builder.AppendLine($"        return response?.Select(GeneratedModelMapper.ToStructure).ToList() ?? new List<{operation.Response.SchemaName}>();");
                }
                else
                {
                    builder.AppendLine("        var response = itemRequestBuilder.GetAsync().GetAwaiter().GetResult();");
                    builder.AppendLine("        return GeneratedModelMapper.ToStructure(response);");
                }
                break;
            case "POST":
                builder.AppendLine($"        var response = requestBuilder.PostAsync(GeneratedModelMapper.ToModel({operation.RequestBody!.SchemaName.ToCamelCaseInvariant()})).GetAwaiter().GetResult();");
                builder.AppendLine("        return GeneratedModelMapper.ToStructure(response);");
                break;
            case "PATCH":
                builder.AppendLine($"        var response = itemRequestBuilder.PatchAsync(GeneratedModelMapper.ToModel({operation.RequestBody!.SchemaName.ToCamelCaseInvariant()})).GetAwaiter().GetResult();");
                builder.AppendLine("        return GeneratedModelMapper.ToStructure(response);");
                break;
            case "DELETE":
                builder.AppendLine("        using var responseStream = itemRequestBuilder.DeleteAsync().GetAwaiter().GetResult();");
                break;
            default:
                throw new NotSupportedException($"Operation '{operation.Name}' uses unsupported HTTP method '{operation.HttpMethod}'.");
        }

        builder.AppendLine("    }");
        return builder.ToString().TrimEnd();
    }

    private static string RenderInterfaceMethod(OperationDefinition operation)
    {
        var builder = new StringBuilder();
        builder.Append($"    [OSAction(Description = \"{EscapeString(operation.Description)}\"");
        if (!string.Equals(operation.Name, operation.DefaultName, StringComparison.Ordinal))
        {
            builder.Append($", OriginalName = \"{EscapeString(operation.DefaultName)}\"");
        }
        builder.AppendLine($"{RenderReturnMetadata(operation)})]");
        builder.AppendLine($"    {GetReturnType(operation)} {operation.Name}({RenderMethodParameters(operation, includeAttributes: true)});");
        return builder.ToString().TrimEnd();
    }

    private static string RenderReturnMetadata(OperationDefinition operation)
    {
        if (operation.Response is null || operation.Response.Kind == "void")
        {
            return string.Empty;
        }

        return $", ReturnDescription = \"The result returned by the API.\", ReturnName = \"{EscapeString(GetReturnName(operation))}\"";
    }

    private static string GetReturnName(OperationDefinition operation)
    {
        return operation.Response?.Kind switch
        {
            "array" => "Items",
            "object" => "Item",
            _ => "Result"
        };
    }

    private static string GetReturnType(OperationDefinition operation)
    {
        if (operation.Response is null || operation.Response.Kind == "void")
        {
            return "void";
        }

        return operation.Response.Kind == "array"
            ? $"List<{operation.Response.SchemaName}>"
            : operation.Response.SchemaName!;
    }

    private static string RenderMethodParameters(OperationDefinition operation, bool includeAttributes = false)
    {
        var parameters = new List<string>();

        foreach (var parameter in operation.Parameters)
        {
            var attribute = includeAttributes
                ? BuildParameterAttribute(parameter.Description, !string.Equals(parameter.Name, parameter.DefaultName, StringComparison.Ordinal) ? parameter.DefaultName : null) + " "
                : string.Empty;
            var type = GetPublicParameterType(parameter);
            var defaultValue = GetParameterDefaultValue(parameter);
            parameters.Add($"{attribute}{type} {parameter.Name}{defaultValue}");

            if (parameter.RequiresPresenceFlag)
            {
                var includeAttribute = includeAttributes
                    ? BuildParameterAttribute($"Set to true to send the {parameter.OriginalName} query parameter to the API.", null) + " "
                    : string.Empty;
                parameters.Add($"{includeAttribute}bool {parameter.PresenceFlagName} = false");
            }
        }

        if (operation.RequestBody is not null)
        {
            var bodyName = operation.RequestBody.SchemaName.ToCamelCaseInvariant();
            var attribute = includeAttributes
                ? BuildParameterAttribute("The JSON request body payload.", null) + " "
                : string.Empty;
            parameters.Add($"{attribute}{operation.RequestBody.SchemaName} {bodyName}");
        }

        return string.Join(", ", parameters);
    }

    private static string CreateOperationName(OperationType operationType, string path, string resourcePropertyName, string? operationId)
    {
        if (!string.IsNullOrWhiteSpace(operationId))
        {
            return ToPascalCase(operationId);
        }

        var singularResourceName = resourcePropertyName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? resourcePropertyName[..^1]
            : resourcePropertyName;
        var hasPathParameter = path.Contains('{');

        return operationType switch
        {
            OperationType.Get when !hasPathParameter => $"List{resourcePropertyName}",
            OperationType.Post when !hasPathParameter => $"Create{singularResourceName}",
            OperationType.Get when hasPathParameter => $"Get{singularResourceName}ById",
            OperationType.Patch when hasPathParameter => $"Update{singularResourceName}",
            OperationType.Delete when hasPathParameter => $"Delete{singularResourceName}",
            _ => throw new NotSupportedException($"Cannot derive a stable action name for {operationType} {path}.")
        };
    }

    private static string ResolveSchemaName(OpenApiSchema schema, IReadOnlyDictionary<string, string> schemaNames, string context)
    {
        if (schema.Reference is null)
        {
            throw new NotSupportedException($"Inline schemas are not supported for {context} in v1.");
        }

        var referenceId = schema.Reference.Id;
        if (!schemaNames.TryGetValue(referenceId, out var schemaName))
        {
            throw new NotSupportedException($"Referenced schema '{referenceId}' for {context} was not found in components.");
        }

        return schemaName;
    }

    private static string MapSchemaType(OpenApiSchema schema, IReadOnlyDictionary<string, string> schemaNames, string context)
    {
        if (schema.Reference is not null)
        {
            return ResolveSchemaName(schema, schemaNames, context);
        }

        return schema.Type switch
        {
            "integer" => "int",
            "number" => "decimal",
            "boolean" => "bool",
            "string" when string.Equals(schema.Format, "date-time", StringComparison.OrdinalIgnoreCase) => "DateTime",
            "string" => "string",
            "array" when schema.Items is not null => $"List<{MapSchemaType(schema.Items, schemaNames, context)}>",
            _ => throw new NotSupportedException($"Schema type '{schema.Type}' is not supported for {context}.")
        };
    }

    private static bool IsNullableSchema(OpenApiSchema schema)
    {
        return schema.Nullable || (schema.AnyOf.Count > 0 && schema.AnyOf.Any(candidate => candidate.Type == "null"));
    }

    private static string WriteFile(string outputDirectory, string fileName, string content)
    {
        var filePath = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
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

    private static string SanitizeIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }

    private static string FormatType(string cSharpType, bool isNullable)
    {
        if (!isNullable || cSharpType.StartsWith("List<", StringComparison.Ordinal))
        {
            return cSharpType;
        }

        return $"{cSharpType}?";
    }

    private static string GetPublicParameterType(ParameterDefinition parameter)
    {
        if (parameter.RequiresPresenceFlag)
        {
            return parameter.CSharpType;
        }

        return FormatType(parameter.CSharpType, parameter.IsNullable);
    }

    private static string GetParameterDefaultValue(ParameterDefinition parameter)
    {
        if (parameter.RequiresPresenceFlag)
        {
            return parameter.CSharpType switch
            {
                "string" => " = \"\"",
                "bool" => " = false",
                "int" => " = 0",
                "decimal" => " = 0",
                "DateTime" => " = default",
                _ => string.Empty
            };
        }

        return parameter.IsNullable ? " = null" : string.Empty;
    }

    private static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string BuildParameterAttribute(string description, string? originalName)
    {
        var builder = new StringBuilder();
        builder.Append($"[OSParameter(Description = \"{EscapeString(description)}\"");
        if (!string.IsNullOrWhiteSpace(originalName))
        {
            builder.Append($", OriginalName = \"{EscapeString(originalName)}\"");
        }
        builder.Append(")]");
        return builder.ToString();
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

    private static NameDescriptionOverride? FindActionOverride(EffectiveGeneratorConfig config, OperationType operationType, string path, string defaultName)
    {
        return FindOverride(config.ActionOverrides, $"{operationType.ToString().ToUpperInvariant()} {path}", defaultName);
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

    private static string NormalizeParameterDescription(OpenApiParameter parameter)
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

    private static string ToCamelCaseInvariant(this string value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
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

            return new KiotaMetadata(options.ClientNamespace, options.ClientClassName, null, null);
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

        return new KiotaMetadata(clientNamespace, clientClassName, descriptionLocation, kiotaVersion);
    }
}

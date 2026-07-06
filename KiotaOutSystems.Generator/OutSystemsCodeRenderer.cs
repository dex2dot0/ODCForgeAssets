using System.Text;
using System.Text.RegularExpressions;

namespace KiotaOutSystems.Generator;

internal static class OutSystemsCodeRenderer
{
    private static readonly Dictionary<string, string?> RequestBuilderSourceCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, bool> EnumTypeCache = new(StringComparer.Ordinal);

    public static string RenderStructures(
        string generationNamespace,
        IReadOnlyList<SchemaDefinition> schemas,
        RuntimeContextDefinition runtimeContext)
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
            if (!string.Equals(schema.Name, schema.OriginalSchemaName, StringComparison.Ordinal))
            {
                builder.Append($", OriginalName = \"{EscapeString(schema.OriginalSchemaName)}\"");
            }
            builder.AppendLine(")]");
            builder.AppendLine($"public struct {schema.Name}");
            builder.AppendLine("{");

            foreach (var property in schema.Properties)
            {
                builder.Append($"    [OSStructureField(Description = \"{EscapeString(property.Description)}\"");
                if (!string.Equals(property.Name, property.OriginalName, StringComparison.Ordinal))
                {
                    builder.Append($", OriginalName = \"{EscapeString(property.OriginalName)}\"");
                }
                builder.AppendLine(")]");
                builder.AppendLine($"    public {FormatType(property.CSharpType, property.IsNullable)} {property.Name} {{ get; set; }}");
                builder.AppendLine();
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        builder.Append(RenderRequestOptionsStructure(runtimeContext));

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    public static string RenderActions(
        string generationNamespace,
        string className,
        string interfaceName,
        string inputName,
        IReadOnlyList<SchemaDefinition> schemas,
        IReadOnlyList<OperationDefinition> operations,
        RuntimeContextDefinition runtimeContext,
        KiotaMetadata kiotaMetadata,
        bool emitInterface,
        string? iconResourceName)
    {
        var builder = new StringBuilder();
        var generatedStructureNames = schemas
            .Select(schema => schema.Name)
            .ToHashSet(StringComparer.Ordinal);
        generatedStructureNames.Add(runtimeContext.RequestOptionsTypeName);
        var schemaMap = schemas.ToDictionary(schema => schema.Name, StringComparer.Ordinal);
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using Microsoft.Kiota.Abstractions;");
        builder.AppendLine("using Microsoft.Kiota.Abstractions.Authentication;");
        builder.AppendLine("using Microsoft.Kiota.Abstractions.Serialization;");
        builder.AppendLine("using Microsoft.Kiota.Http.HttpClientLibrary;");
        builder.AppendLine("using Microsoft.Kiota.Serialization.Json;");
        builder.AppendLine("using OutSystems.ExternalLibraries.SDK;");
        builder.AppendLine("using System.Reflection;");
        builder.AppendLine("using System.Runtime.Serialization;");
        builder.AppendLine("using System.Text;");
        builder.AppendLine("using System.Text.Json;");
        builder.AppendLine("using System.Text.Json.Serialization;");
        builder.AppendLine();
        builder.AppendLine($"namespace {generationNamespace};");
        builder.AppendLine();
        builder.AppendLine("internal static class GeneratedModelMapper");
        builder.AppendLine("{");
        builder.AppendLine("    private static readonly JsonSerializerOptions SerializerOptions = new()");
        builder.AppendLine("    {");
        builder.AppendLine("        PropertyNameCaseInsensitive = true,");
        builder.AppendLine("        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,");
        builder.AppendLine("        Converters = { new JsonStringEnumConverter(), new JsonStringOrRawJsonConverter() }");
        builder.AppendLine("    };");
        builder.AppendLine();
        builder.AppendLine("    private sealed class JsonStringOrRawJsonConverter : JsonConverter<string>");
        builder.AppendLine("    {");
        builder.AppendLine("        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (reader.TokenType == JsonTokenType.Null)");
        builder.AppendLine("            {");
        builder.AppendLine("                return null;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            if (reader.TokenType == JsonTokenType.String)");
        builder.AppendLine("            {");
        builder.AppendLine("                return reader.GetString();");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            using var document = JsonDocument.ParseValue(ref reader);");
        builder.AppendLine("            return document.RootElement.ValueKind switch");
        builder.AppendLine("            {");
        builder.AppendLine("                JsonValueKind.Object or JsonValueKind.Array => document.RootElement.GetRawText(),");
        builder.AppendLine("                JsonValueKind.Null or JsonValueKind.Undefined => null,");
        builder.AppendLine("                _ => document.RootElement.ToString()");
        builder.AppendLine("            };");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)");
        builder.AppendLine("        {");
        builder.AppendLine("            writer.WriteStringValue(value);");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TDestination? Convert<TDestination>(object? source)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (source is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return default;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var json = JsonSerializer.Serialize(source, SerializerOptions);");
        builder.AppendLine("        return JsonSerializer.Deserialize<TDestination>(json, SerializerOptions);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static byte[] ReadAllBytes(Stream? source)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (source is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return Array.Empty<byte>();");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        using var buffer = new MemoryStream();");
        builder.AppendLine("        source.CopyTo(buffer);");
        builder.AppendLine("        return buffer.ToArray();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TDestination ParseJson<TDestination>(string source, ParsableFactory<TDestination> factory) where TDestination : IParsable");
        builder.AppendLine("    {");
        builder.AppendLine("        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(source) ? \"{}\" : source));");
        builder.AppendLine("        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(\"application/json\", stream).GetAwaiter().GetResult();");
        builder.AppendLine("        return parseNode.GetObjectValue(factory)!;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TEnum? ParseEnum<TEnum>(string? source) where TEnum : struct, Enum");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(source))");
        builder.AppendLine("        {");
        builder.AppendLine("            return null;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        foreach (var value in Enum.GetValues<TEnum>())");
        builder.AppendLine("        {");
        builder.AppendLine("            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();");
        builder.AppendLine("            var enumMember = member?.GetCustomAttribute<EnumMemberAttribute>();");
        builder.AppendLine("            if (string.Equals(enumMember?.Value, source, StringComparison.OrdinalIgnoreCase))");
        builder.AppendLine("            {");
        builder.AppendLine("                return value;");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var normalized = NormalizeEnumToken(source);");
        builder.AppendLine("        return Enum.TryParse<TEnum>(normalized, true, out var parsed) ? parsed : null;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TEnum[] ParseEnumArray<TEnum>(IEnumerable<string>? source) where TEnum : struct, Enum");
        builder.AppendLine("    {");
        builder.AppendLine("        if (source is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return Array.Empty<TEnum>();");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return source");
        builder.AppendLine("            .Select(ParseEnum<TEnum>)");
        builder.AppendLine("            .Where(item => item.HasValue)");
        builder.AppendLine("            .Select(item => item!.Value)");
        builder.AppendLine("            .ToArray();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static Microsoft.Kiota.Abstractions.Date ToKiotaDate(DateTime source)");
        builder.AppendLine("    {");
        builder.AppendLine("        return new Microsoft.Kiota.Abstractions.Date(source.Year, source.Month, source.Day);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static string SerializeObject(object? source)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (source is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return string.Empty;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (source is IAdditionalDataHolder additionalDataHolder && HasNoDeclaredPayload(source.GetType()))");
        builder.AppendLine("        {");
        builder.AppendLine("            return JsonSerializer.Serialize(additionalDataHolder.AdditionalData, SerializerOptions);");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return JsonSerializer.Serialize(source, SerializerOptions);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static bool HasNoDeclaredPayload(Type type)");
        builder.AppendLine("    {");
        builder.AppendLine("        return !type.GetProperties(BindingFlags.Instance | BindingFlags.Public)");
        builder.AppendLine("            .Any(property => !string.Equals(property.Name, nameof(IAdditionalDataHolder.AdditionalData), StringComparison.Ordinal));");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string NormalizeEnumToken(string source)");
        builder.AppendLine("    {");
        builder.AppendLine("        var builder = new StringBuilder(source.Length);");
        builder.AppendLine("        var capitalizeNext = true;");
        builder.AppendLine("        foreach (var character in source)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (!char.IsLetterOrDigit(character))");
        builder.AppendLine("            {");
        builder.AppendLine("                capitalizeNext = true;");
        builder.AppendLine("                continue;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);");
        builder.AppendLine("            capitalizeNext = false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return builder.Length == 0 ? source : builder.ToString();");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("}");
        builder.AppendLine();
        builder.Append(RenderAuthenticationProvider(runtimeContext));
        builder.AppendLine();
        var implementedInterfaceClause = emitInterface ? $" : {interfaceName}" : string.Empty;
        builder.AppendLine($"public class {className}{implementedInterfaceClause}");
        builder.AppendLine("{");
        builder.AppendLine($"    public {className}()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.Append(RenderRequestConfigurationHelpers(runtimeContext, kiotaMetadata));
        builder.AppendLine();

        foreach (var operation in operations)
        {
                builder.AppendLine(RenderClassMethod(operation, generatedStructureNames, schemaMap, runtimeContext, kiotaMetadata));
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
                builder.AppendLine(RenderInterfaceMethod(operation, runtimeContext));
                builder.AppendLine();
            }

            builder.AppendLine("}");
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static string RenderClassMethod(
        OperationDefinition operation,
        IReadOnlySet<string> generatedStructureNames,
        IReadOnlyDictionary<string, SchemaDefinition> schemaMap,
        RuntimeContextDefinition runtimeContext,
        KiotaMetadata kiotaMetadata)
    {
        var builder = new StringBuilder();
        var returnsArray = string.Equals(operation.Response?.Kind, "array", StringComparison.Ordinal);
        var returnsVoid = operation.Response is null || string.Equals(operation.Response.Kind, "void", StringComparison.Ordinal);
        var requestBuilderExpression = BuildRequestBuilderExpression(operation, kiotaMetadata, "client");
        var preferredMethodName = ResolvePreferredRequestMethodName(operation, kiotaMetadata);
        var getRequestArguments = RenderGetRequestArguments(operation, generatedStructureNames, schemaMap, kiotaMetadata);
        builder.AppendLine($"    public {GetReturnType(operation)} {operation.Name}({RenderMethodParameters(operation, runtimeContext)})");
        builder.AppendLine("    {");
        builder.AppendLine($"        var activeSecuritySchemes = ResolveActiveSecuritySchemes(requestOptions{RenderSecurityRequirementArguments(operation)});");
        builder.AppendLine("        var client = CreateClient(requestOptions, activeSecuritySchemes);");
        builder.AppendLine($"        var requestBuilder = {requestBuilderExpression};");

        switch (operation.HttpMethod)
        {
            case "GET":
                if (returnsVoid)
                {
                    builder.AppendLine($"        requestBuilder.{preferredMethodName}({getRequestArguments}).GetAwaiter().GetResult();");
                }
                else if (operation.Response?.Kind == "array")
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({getRequestArguments}).GetAwaiter().GetResult();");
                    builder.AppendLine();
                    AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                }
                else
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({getRequestArguments}).GetAwaiter().GetResult();");
                    AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                }
                break;
            case "POST":
                if (returnsVoid)
                {
                    if (operation.RequestBody is not null)
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                    }
                    else
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                    }
                    break;
                }

                if (operation.RequestBody is not null)
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                }
                else
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                }

                if (returnsArray)
                {
                    AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    break;
                }

                AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                break;
            case "PATCH":
                if (returnsVoid)
                {
                    if (operation.RequestBody is not null)
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                    }
                    else
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                    }
                    break;
                }

                if (operation.RequestBody is not null)
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                }
                else
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                }

                if (returnsArray)
                {
                    AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    break;
                }

                AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                break;
            case "PUT":
                if (returnsVoid)
                {
                    if (operation.RequestBody is not null)
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                    }
                    else
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                    }
                    break;
                }

                if (operation.RequestBody is not null)
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                }
                else
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                }

                if (returnsArray)
                {
                    AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    break;
                }

                AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                break;
            case "DELETE":
                if (operation.RequestBody is not null)
                {
                    if (returnsVoid)
                    {
                        builder.AppendLine($"        requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                        break;
                    }

                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}({RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap)}).GetAwaiter().GetResult();");
                    if (returnsArray)
                    {
                        AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    }
                    else
                    {
                        AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    }
                }
                else if (returnsVoid)
                {
                    builder.AppendLine($"        requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                }
                else
                {
                    builder.AppendLine($"        var response = requestBuilder.{preferredMethodName}().GetAwaiter().GetResult();");
                    if (returnsArray)
                    {
                        AppendArrayResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    }
                    else
                    {
                        AppendResponseReturn(builder, "response", operation.Response, generatedStructureNames, 8);
                    }
                }
                break;
            default:
                throw new NotSupportedException($"Operation '{operation.Name}' uses unsupported HTTP method '{operation.HttpMethod}'.");
        }

        builder.AppendLine("    }");
        return builder.ToString().TrimEnd();
    }

    private static string RenderInterfaceMethod(OperationDefinition operation, RuntimeContextDefinition runtimeContext)
    {
        var builder = new StringBuilder();
        builder.Append($"    [OSAction(Description = \"{EscapeString(operation.Description)}\"");
        if (!string.Equals(operation.Name, operation.DefaultName, StringComparison.Ordinal))
        {
            builder.Append($", OriginalName = \"{EscapeString(operation.DefaultName)}\"");
        }
        builder.AppendLine($"{RenderReturnMetadata(operation)})]");
        builder.AppendLine($"    {GetReturnType(operation)} {operation.Name}({RenderMethodParameters(operation, runtimeContext, includeAttributes: true)});");
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
            "binary" => "Data",
            "rawJson" => "Json",
            "string" => "Text",
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

        if (operation.Response.Kind == "binary")
        {
            return "byte[]";
        }

        if (operation.Response.Kind == "rawJson")
        {
            return "string";
        }

        return operation.Response.Kind == "array"
            ? $"List<{operation.Response.SchemaName}>"
            : operation.Response.SchemaName!;
    }

    private static string RenderMethodParameters(
        OperationDefinition operation,
        RuntimeContextDefinition runtimeContext,
        bool includeAttributes = false)
    {
        var requiredParameters = new List<string>();
        var optionalParameters = new List<string>();

        var requestOptionsAttribute = includeAttributes
            ? BuildParameterAttribute("Runtime request configuration including base URL and authentication.", null) + " "
            : string.Empty;
        requiredParameters.Add($"{requestOptionsAttribute}{runtimeContext.RequestOptionsTypeName} requestOptions");

        foreach (var parameter in operation.Parameters)
        {
            var attribute = includeAttributes
                ? BuildParameterAttribute(parameter.Description, !string.Equals(parameter.Name, parameter.DefaultName, StringComparison.Ordinal) ? parameter.DefaultName : null) + " "
                : string.Empty;
            var type = GetPublicParameterType(parameter);
            var defaultValue = GetParameterDefaultValue(parameter);
            var parameterDeclaration = $"{attribute}{type} {parameter.Name}{defaultValue}";
            var isOptional = parameter.RequiresPresenceFlag || parameter.IsNullable;
            (isOptional ? optionalParameters : requiredParameters).Add(parameterDeclaration);

            if (parameter.RequiresPresenceFlag)
            {
                var includeAttribute = includeAttributes
                    ? BuildParameterAttribute($"Set to true to send the {parameter.OriginalName} query parameter to the API.", null) + " "
                    : string.Empty;
                optionalParameters.Add($"{includeAttribute}bool {parameter.PresenceFlagName} = false");
            }
        }

        if (operation.RequestBody is not null)
        {
            var bodyName = "requestBody";
            var attribute = includeAttributes
                ? BuildParameterAttribute(GetRequestBodyDescription(operation.RequestBody), null) + " "
                : string.Empty;
            requiredParameters.Add($"{attribute}{operation.RequestBody.SchemaName} {bodyName}");
        }

        return string.Join(", ", requiredParameters.Concat(optionalParameters));
    }

    private static string RenderSecurityRequirementArguments(OperationDefinition operation)
    {
        if (operation.SecurityRequirements.Count == 0)
        {
            return string.Empty;
        }

        var renderedRequirements = operation.SecurityRequirements
            .Select(requirement => $"new[] {{ {string.Join(", ", requirement.SchemeIds.Select(id => $"\"{EscapeString(id)}\""))} }}");

        return ", " + string.Join(", ", renderedRequirements);
    }

    private static string BuildRequestBuilderExpression(OperationDefinition operation, KiotaMetadata kiotaMetadata, string clientVariableName)
    {
        var pathParameters = operation.Parameters
            .Where(parameter => parameter.Location == "path")
            .ToDictionary(parameter => parameter.OriginalName, parameter => parameter, StringComparer.OrdinalIgnoreCase);
        var literalSegments = new List<string>();

        var builder = clientVariableName;
        foreach (var segment in operation.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.StartsWith('{') && segment.EndsWith('}'))
            {
                var originalName = segment[1..^1];
                if (!pathParameters.TryGetValue(originalName, out var parameter))
                {
                    throw new InvalidOperationException($"No path parameter binding was found for '{originalName}' on '{operation.Path}'.");
                }

                var currentPath = literalSegments.Count == 0 ? "/" : "/" + string.Join("/", literalSegments);
                var parameterValue = ResolvePathIndexerValue(parameter, currentPath, kiotaMetadata);
                builder += $"[{parameterValue}]";
            }
            else
            {
                builder += $".{CreateKiotaRequestBuilderMemberName(segment)}";
                literalSegments.Add(segment);
            }
        }

        return builder;
    }

    private static string RenderGetRequestArguments(
        OperationDefinition operation,
        IReadOnlySet<string> generatedStructureNames,
        IReadOnlyDictionary<string, SchemaDefinition> schemaMap,
        KiotaMetadata kiotaMetadata)
    {
        var parts = new List<string>();
        if (operation.RequestBody is not null)
        {
            parts.Add(RenderBodyArgument(operation.RequestBody, generatedStructureNames, schemaMap));
        }

        if (operation.Parameters.Any(parameter => parameter.Location == "query"))
        {
            var configBuilder = new StringBuilder();
            configBuilder.AppendLine("config =>");
            configBuilder.AppendLine("        {");
            foreach (var parameter in operation.Parameters.Where(parameter => parameter.Location == "query"))
            {
                if (parameter.RequiresPresenceFlag)
                {
                    configBuilder.AppendLine($"            if ({parameter.PresenceFlagName})");
                    configBuilder.AppendLine("            {");
                    configBuilder.AppendLine($"                {RenderQueryParameterAssignment(operation, parameter, kiotaMetadata)}");
                    configBuilder.AppendLine("            }");
                }
                else
                {
                    configBuilder.AppendLine($"            {RenderQueryParameterAssignment(operation, parameter, kiotaMetadata)}");
                }
            }

            configBuilder.Append("        }");
            parts.Add(configBuilder.ToString());
        }

        return string.Join(", ", parts);
    }

    private static string RenderQueryParameterValue(ParameterDefinition parameter)
    {
        if (parameter.CSharpType.StartsWith("List<", StringComparison.Ordinal) &&
            TryGetListElementType(parameter.CSharpType, out var elementType) &&
            elementType is not null)
        {
            if (string.Equals(elementType, "int", StringComparison.Ordinal))
            {
                return $"({parameter.Name} ?? new List<int>()).Select(item => (int?)item).ToArray()";
            }

            return $"({parameter.Name} ?? new List<{elementType}>()).ToArray()";
        }

        return parameter.Name;
    }

    private static string RenderQueryParameterAssignment(
        OperationDefinition operation,
        ParameterDefinition parameter,
        KiotaMetadata kiotaMetadata)
    {
        if (TryResolvePreferredQueryParameterBinding(operation, parameter, kiotaMetadata, out var propertyName, out var propertyType))
        {
            return $"config.QueryParameters.{propertyName} = {RenderTypedQueryParameterValue(parameter, propertyType!, kiotaMetadata)};";
        }

        if (TryGetQueryParameterPropertyType(operation, parameter, kiotaMetadata, out var directPropertyType))
        {
            return $"config.QueryParameters.{parameter.QueryPropertyName} = {RenderTypedQueryParameterValue(parameter, directPropertyType!, kiotaMetadata)};";
        }

        return $"config.QueryParameters.{parameter.QueryPropertyName} = {RenderQueryParameterValue(parameter)};";
    }

    private static bool TryResolvePreferredQueryParameterBinding(
        OperationDefinition operation,
        ParameterDefinition parameter,
        KiotaMetadata kiotaMetadata,
        out string? propertyName,
        out string? propertyType)
    {
        propertyName = null;
        propertyType = null;

        if (string.IsNullOrWhiteSpace(parameter.QueryPropertyName) ||
            TryGetRequestBuilderSource(kiotaMetadata, operation.Path) is not { } requestBuilderSource)
        {
            return false;
        }

        var obsoleteMatch = Regex.Match(
            requestBuilderSource,
            $@"\[Obsolete\(""[^""]*use\s+(?<replacement>\w+)\s+instead\.?""\)\][\s\S]{{0,600}}?public\s+(?<type>[^\r\n]+?)\s+{Regex.Escape(parameter.QueryPropertyName)}\s*\{{",
            RegexOptions.CultureInvariant);
        if (!obsoleteMatch.Success)
        {
            return false;
        }

        var replacementName = obsoleteMatch.Groups["replacement"].Value;
        var replacementMatch = Regex.Match(
            requestBuilderSource,
            $@"public\s+(?<type>[^\r\n]+?)\s+{Regex.Escape(replacementName)}\s*\{{",
            RegexOptions.CultureInvariant);
        if (!replacementMatch.Success)
        {
            return false;
        }

        var replacementType = NormalizeGeneratedTypeName(replacementMatch.Groups["type"].Value);
        if (!CanRenderQueryReplacementValue(parameter, replacementType, kiotaMetadata))
        {
            return false;
        }

        propertyName = replacementName;
        propertyType = replacementType;
        return true;
    }

    private static bool CanRenderQueryReplacementValue(ParameterDefinition parameter, string propertyType, KiotaMetadata kiotaMetadata)
    {
        var normalizedType = propertyType.TrimEnd('?');
        if (IsPrimitiveLikeType(normalizedType))
        {
            return true;
        }

        if (normalizedType.EndsWith("[]", StringComparison.Ordinal) &&
            parameter.CSharpType.StartsWith("List<string>", StringComparison.Ordinal) &&
            IsKiotaEnumType(kiotaMetadata, normalizedType[..^2]))
        {
            return true;
        }

        return string.Equals(parameter.CSharpType, "string", StringComparison.Ordinal) &&
               IsKiotaEnumType(kiotaMetadata, normalizedType);
    }

    private static bool TryGetQueryParameterPropertyType(
        OperationDefinition operation,
        ParameterDefinition parameter,
        KiotaMetadata kiotaMetadata,
        out string? propertyType)
    {
        propertyType = null;
        if (string.IsNullOrWhiteSpace(parameter.QueryPropertyName) ||
            TryGetRequestBuilderSource(kiotaMetadata, operation.Path) is not { } requestBuilderSource)
        {
            return false;
        }

        var propertyMatch = Regex.Match(
            requestBuilderSource,
            $@"public\s+(?<type>[^\r\n]+?)\s+{Regex.Escape(parameter.QueryPropertyName)}\s*\{{",
            RegexOptions.CultureInvariant);
        if (!propertyMatch.Success)
        {
            return false;
        }

        propertyType = NormalizeGeneratedTypeName(propertyMatch.Groups["type"].Value);
        return true;
    }

    private static string RenderTypedQueryParameterValue(ParameterDefinition parameter, string propertyType, KiotaMetadata kiotaMetadata)
    {
        var normalizedType = propertyType.TrimEnd('?');
        if (string.Equals(normalizedType, "Date", StringComparison.Ordinal) &&
            string.Equals(parameter.CSharpType, "DateTime", StringComparison.Ordinal))
        {
            return $"GeneratedModelMapper.ToKiotaDate({parameter.Name})";
        }

        if (IsPrimitiveLikeType(normalizedType))
        {
            return RenderQueryParameterValue(parameter);
        }

        if (normalizedType.EndsWith("[]", StringComparison.Ordinal) &&
            parameter.CSharpType.StartsWith("List<string>", StringComparison.Ordinal) &&
            IsKiotaEnumType(kiotaMetadata, normalizedType[..^2]))
        {
            return $"GeneratedModelMapper.ParseEnumArray<{normalizedType[..^2]}>({parameter.Name})";
        }

        if (string.Equals(parameter.CSharpType, "string", StringComparison.Ordinal) &&
            IsKiotaEnumType(kiotaMetadata, normalizedType))
        {
            return $"GeneratedModelMapper.ParseEnum<{normalizedType}>({parameter.Name})";
        }

        return RenderQueryParameterValue(parameter);
    }

    private static bool IsPrimitiveLikeType(string value)
    {
        return value switch
        {
            "string" or "int" or "long" or "double" or "float" or "decimal" or "bool" or "Date" or "DateTime" => true,
            _ => false
        };
    }

    private static bool IsKiotaEnumType(KiotaMetadata kiotaMetadata, string typeName)
    {
        if (EnumTypeCache.TryGetValue(typeName, out var cached))
        {
            return cached;
        }

        if (string.IsNullOrWhiteSpace(kiotaMetadata.ClientNamespace) ||
            string.IsNullOrWhiteSpace(kiotaMetadata.ClientRootPath))
        {
            EnumTypeCache[typeName] = false;
            return false;
        }

        var normalizedType = NormalizeGeneratedTypeName(typeName);
        if (!normalizedType.StartsWith($"{kiotaMetadata.ClientNamespace}.", StringComparison.Ordinal))
        {
            EnumTypeCache[normalizedType] = false;
            return false;
        }

        var relativePath = normalizedType[(kiotaMetadata.ClientNamespace.Length + 1)..]
            .Replace('.', Path.DirectorySeparatorChar);
        var filePath = Path.Combine(kiotaMetadata.ClientRootPath!, $"{relativePath}.cs");
        var isEnum = File.Exists(filePath) &&
                     Regex.IsMatch(File.ReadAllText(filePath), @"\benum\s+" + Regex.Escape(Path.GetFileNameWithoutExtension(filePath)) + @"\b", RegexOptions.CultureInvariant);
        EnumTypeCache[normalizedType] = isEnum;
        return isEnum;
    }

    private static bool HasDeclaredPayload(string typeName, KiotaMetadata kiotaMetadata)
    {
        return TryGetTypeSourcePath(typeName, kiotaMetadata, out var filePath) &&
               Regex.IsMatch(
                   File.ReadAllText(filePath!),
                   @"public\s+(?!IDictionary<string,\s*object>\s+AdditionalData\b)(?<type>[^\r\n]+?)\s+(?<name>\w+)\s*\{",
                   RegexOptions.CultureInvariant);
    }

    private static bool TryGetTypeSourcePath(string typeName, KiotaMetadata kiotaMetadata, out string? filePath)
    {
        filePath = null;
        if (string.IsNullOrWhiteSpace(kiotaMetadata.ClientNamespace) ||
            string.IsNullOrWhiteSpace(kiotaMetadata.ClientRootPath))
        {
            return false;
        }

        var normalizedType = NormalizeGeneratedTypeName(typeName);
        if (!normalizedType.StartsWith($"{kiotaMetadata.ClientNamespace}.", StringComparison.Ordinal))
        {
            return false;
        }

        var relativePath = normalizedType[(kiotaMetadata.ClientNamespace.Length + 1)..]
            .Replace('.', Path.DirectorySeparatorChar);
        var candidate = Path.Combine(kiotaMetadata.ClientRootPath!, $"{relativePath}.cs");
        if (!File.Exists(candidate))
        {
            return false;
        }

        filePath = candidate;
        return true;
    }

    private static string ResolvePreferredRequestMethodName(OperationDefinition operation, KiotaMetadata kiotaMetadata)
    {
        var defaultMethodName = $"{operation.HttpMethod[..1].ToUpperInvariant()}{operation.HttpMethod[1..].ToLowerInvariant()}Async";
        if (TryGetRequestBuilderSource(kiotaMetadata, operation.Path) is not { } requestBuilderSource)
        {
            return defaultMethodName;
        }

        var preferredMatch = Regex.Match(
            requestBuilderSource,
            $@"public\s+async\s+Task<(?<returnType>.+?)>\s+(?<name>{Regex.Escape(operation.HttpMethod[..1].ToUpperInvariant() + operation.HttpMethod[1..].ToLowerInvariant())}As\w+Async)\s*\(",
            RegexOptions.CultureInvariant);
        if (!preferredMatch.Success)
        {
            return defaultMethodName;
        }

        var returnType = NormalizeGeneratedTypeName(preferredMatch.Groups["returnType"].Value).TrimEnd('?');
        return HasDeclaredPayload(returnType, kiotaMetadata) ? preferredMatch.Groups["name"].Value : defaultMethodName;
    }

    private static string? TryGetRequestBuilderSource(KiotaMetadata kiotaMetadata, string path)
    {
        var requestBuilderFilePath = TryGetExistingRequestBuilderFilePath(kiotaMetadata, path);
        if (string.IsNullOrWhiteSpace(requestBuilderFilePath) || !File.Exists(requestBuilderFilePath))
        {
            return null;
        }

        if (!RequestBuilderSourceCache.TryGetValue(requestBuilderFilePath, out var source))
        {
            source = File.ReadAllText(requestBuilderFilePath);
            RequestBuilderSourceCache[requestBuilderFilePath] = source;
        }

        return source;
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
        var directoryPath = Path.Combine(new[] { kiotaMetadata.ClientRootPath! }.Concat(directorySegments).ToArray());
        return Directory.Exists(directoryPath) ? directoryPath : null;
    }

    private static string NormalizeGeneratedTypeName(string value)
    {
        return value.Replace("global::", string.Empty, StringComparison.Ordinal).Trim();
    }

    private static string CreateKiotaDirectorySegmentName(string segment)
    {
        return string.Equals(segment, "void", StringComparison.OrdinalIgnoreCase)
            ? "VoidNamespace"
            : CreateKiotaPathSegmentName(segment);
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

    private static string ResolvePathIndexerValue(ParameterDefinition parameter, string currentPath, KiotaMetadata kiotaMetadata)
    {
        if (string.Equals(parameter.CSharpType, "string", StringComparison.Ordinal))
        {
            return parameter.Name;
        }

        var typedKeyword = parameter.CSharpType switch
        {
            "int" or "long" or "double" or "float" or "decimal" or "bool" => parameter.CSharpType,
            _ => null
        };
        if (typedKeyword is not null &&
            TryGetRequestBuilderSource(kiotaMetadata, currentPath) is { } requestBuilderSource &&
            Regex.IsMatch(requestBuilderSource, $@"this\[\s*{Regex.Escape(typedKeyword)}\s+position\s*\]", RegexOptions.CultureInvariant))
        {
            return parameter.Name;
        }

        return $"{parameter.Name}.ToString()";
    }

    private static string RenderBodyArgument(
        BodyDefinition requestBody,
        IReadOnlySet<string> generatedStructureNames,
        IReadOnlyDictionary<string, SchemaDefinition> schemaMap)
    {
        const string bodyName = "requestBody";
        if (requestBody.Kind == "binary")
        {
            return $"new MemoryStream({bodyName} ?? Array.Empty<byte>())";
        }

        if (requestBody.Kind == "text")
        {
            return $"{bodyName} ?? string.Empty";
        }

        if (requestBody.Kind == "rawJson")
        {
            return $"GeneratedModelMapper.ParseJson<{requestBody.ModelSchemaName}>({bodyName} ?? \"{{}}\", {requestBody.ModelSchemaName}.CreateFromDiscriminatorValue)";
        }

        if (string.Equals(requestBody.SchemaName, requestBody.ModelSchemaName, StringComparison.Ordinal))
        {
            if (TryGetListElementType(requestBody.SchemaName, out var elementType) &&
                elementType is not null &&
                generatedStructureNames.Contains(elementType) &&
                schemaMap.TryGetValue(elementType, out var elementSchema) &&
                !string.IsNullOrWhiteSpace(elementSchema.KiotaModelType))
            {
                return $"{bodyName}.Select(item => GeneratedModelMapper.Convert<{elementSchema.KiotaModelType}>(item)!).ToList()";
            }

            return bodyName;
        }

        return $"GeneratedModelMapper.Convert<{requestBody.ModelSchemaName}>({bodyName})!";
    }

    private static void AppendArrayResponseReturn(
        StringBuilder builder,
        string responseVariableName,
        ResponseDefinition? response,
        IReadOnlySet<string> generatedStructureNames,
        int indentSize)
    {
        var indent = new string(' ', indentSize);
        if (response?.SchemaName is null)
        {
            builder.AppendLine($"{indent}return new List<object>();");
            return;
        }

        if (generatedStructureNames.Contains(response.SchemaName))
        {
            builder.AppendLine($"{indent}return {responseVariableName}?.Select(item => GeneratedModelMapper.Convert<{response.SchemaName}>(item)!).ToList() ?? new List<{response.SchemaName}>();");
            return;
        }

        builder.AppendLine($"{indent}return GeneratedModelMapper.Convert<List<{response.SchemaName}>>({responseVariableName}) ?? new List<{response.SchemaName}>();");
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
        if ((parameter.RequiresPresenceFlag || parameter.IsNullable) &&
            parameter.CSharpType.StartsWith("List<", StringComparison.Ordinal))
        {
            return $"{parameter.CSharpType}?";
        }

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
                _ when parameter.CSharpType.StartsWith("List<", StringComparison.Ordinal) => " = null",
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

    private static string ToCamelCaseInvariant(this string value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string CreateKiotaRequestBuilderMemberName(string segment)
    {
        return segment.ToLowerInvariant() switch
        {
            "delete" => "DeletePath",
            "post" => "PostPath",
            "void" => "Void",
            _ => CreateKiotaPathSegmentName(segment)
        };
    }

    private static void AppendResponseReturn(
        StringBuilder builder,
        string responseVariableName,
        ResponseDefinition? response,
        IReadOnlySet<string> generatedStructureNames,
        int indentSize)
    {
        var indent = new string(' ', indentSize);
        if (response is null || response.Kind == "void")
        {
            return;
        }

        if (response.Kind == "binary")
        {
            builder.AppendLine($"{indent}return GeneratedModelMapper.ReadAllBytes({responseVariableName});");
            return;
        }

        if (response.Kind == "string")
        {
            builder.AppendLine($"{indent}return {responseVariableName} ?? string.Empty;");
            return;
        }

        if (response.Kind == "rawJson")
        {
            builder.AppendLine($"{indent}return GeneratedModelMapper.SerializeObject({responseVariableName});");
            return;
        }

        if (response.SchemaName is not null && generatedStructureNames.Contains(response.SchemaName))
        {
            builder.AppendLine($"{indent}return GeneratedModelMapper.Convert<{response.SchemaName}>({responseVariableName})!;");
            return;
        }

        builder.AppendLine($"{indent}return {responseVariableName} ?? {GetDefaultValueExpression(response.SchemaName)};");
    }

    private static string GetDefaultValueExpression(string? typeName)
    {
        return typeName switch
        {
            "string" => "string.Empty",
            "byte[]" => "Array.Empty<byte>()",
            "bool" => "false",
            "int" => "0",
            "decimal" => "0m",
            "DateTime" => "default",
            null => "default!",
            _ => "default!"
        };
    }

    private static string GetRequestBodyDescription(BodyDefinition requestBody)
    {
        return requestBody.Kind switch
        {
            "binary" => "The binary request body payload.",
            "text" => "The text request body payload.",
            "rawJson" => "The raw JSON request body payload.",
            _ => "The JSON request body payload."
        };
    }

    private static string RenderRequestOptionsStructure(RuntimeContextDefinition runtimeContext)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[OSStructure(Description = \"Runtime request configuration including base URL and authentication.\")]");
        builder.AppendLine($"public struct {runtimeContext.RequestOptionsTypeName}");
        builder.AppendLine("{");
        builder.AppendLine("    [OSStructureField(Description = \"Overrides the default API base URL when provided.\")]");
        builder.AppendLine("    public string? BaseUrl { get; set; }");
        builder.AppendLine();

        foreach (var scheme in runtimeContext.SecuritySchemes)
        {
            foreach (var field in GetSecuritySchemeFields(scheme, runtimeContext.SecuritySchemes))
            {
                builder.AppendLine($"    [OSStructureField(Description = \"{EscapeString(field.Description)}\")]");
                builder.AppendLine($"    public string? {field.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }
        }

        builder.AppendLine("}");
        builder.AppendLine();
        return builder.ToString();
    }

    private static string RenderAuthenticationProvider(RuntimeContextDefinition runtimeContext)
    {
        var builder = new StringBuilder();
        builder.AppendLine("internal sealed class GeneratedAccessTokenProvider : IAccessTokenProvider");
        builder.AppendLine("{");
        builder.AppendLine("    private readonly string? _accessToken;");
        builder.AppendLine();
        builder.AppendLine("    public GeneratedAccessTokenProvider(string? accessToken)");
        builder.AppendLine("    {");
        builder.AppendLine("        _accessToken = accessToken;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public AllowedHostsValidator AllowedHostsValidator { get; } = new();");
        builder.AppendLine();
        builder.AppendLine("    public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)");
        builder.AppendLine("    {");
        builder.AppendLine("        return Task.FromResult(_accessToken ?? string.Empty);");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("internal sealed class GeneratedAuthenticationProvider : IAuthenticationProvider");
        builder.AppendLine("{");
        builder.AppendLine($"    private readonly {runtimeContext.RequestOptionsTypeName} _requestOptions;");
        builder.AppendLine("    private readonly HashSet<string> _activeSchemeIds;");
        builder.AppendLine();
        builder.AppendLine($"    public GeneratedAuthenticationProvider({runtimeContext.RequestOptionsTypeName} requestOptions, IEnumerable<string> activeSchemeIds)");
        builder.AppendLine("    {");
        builder.AppendLine("        _requestOptions = requestOptions;");
        builder.AppendLine("        _activeSchemeIds = new HashSet<string>(activeSchemeIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)");
        builder.AppendLine("    {");
        builder.AppendLine("        foreach (var schemeId in _activeSchemeIds)");
        builder.AppendLine("        {");
        builder.AppendLine("            switch (schemeId)");
        builder.AppendLine("            {");

        foreach (var scheme in runtimeContext.SecuritySchemes)
        {
            builder.AppendLine($"                case \"{EscapeString(scheme.Id)}\":");
            foreach (var line in RenderSecuritySchemeApplication(scheme, runtimeContext))
            {
                builder.AppendLine($"                    {line}");
            }
            builder.AppendLine("                    break;");
        }

        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString().TrimEnd();
    }

    private static string RenderRequestConfigurationHelpers(RuntimeContextDefinition runtimeContext, KiotaMetadata kiotaMetadata)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"    private static {kiotaMetadata.ClientNamespace}.{kiotaMetadata.ClientClassName} CreateClient({runtimeContext.RequestOptionsTypeName} requestOptions, IEnumerable<string> activeSecuritySchemes)");
        builder.AppendLine("    {");
        builder.AppendLine("        var requestAdapter = new HttpClientRequestAdapter(");
        builder.AppendLine("            new GeneratedAuthenticationProvider(requestOptions, activeSecuritySchemes),");
        builder.AppendLine("            new JsonParseNodeFactory(),");
        builder.AppendLine("            new JsonSerializationWriterFactory(),");
        builder.AppendLine("            new HttpClient(),");
        builder.AppendLine("            null);");
        builder.AppendLine();
        builder.AppendLine($"        var resolvedBaseUrl = string.IsNullOrWhiteSpace(requestOptions.BaseUrl) ? {(runtimeContext.DefaultBaseUrl is null ? "string.Empty" : $"\"{EscapeString(runtimeContext.DefaultBaseUrl)}\"")} : requestOptions.BaseUrl!;");
        builder.AppendLine("        if (!string.IsNullOrWhiteSpace(resolvedBaseUrl))");
        builder.AppendLine("        {");
        builder.AppendLine("            requestAdapter.BaseUrl = resolvedBaseUrl;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine($"        return new {kiotaMetadata.ClientNamespace}.{kiotaMetadata.ClientClassName}(requestAdapter);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine($"    private static string[] ResolveActiveSecuritySchemes({runtimeContext.RequestOptionsTypeName} requestOptions, params string[][] securityRequirementOptions)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (securityRequirementOptions is null || securityRequirementOptions.Length == 0)");
        builder.AppendLine("        {");
        builder.AppendLine("            return Array.Empty<string>();");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        foreach (var requirement in securityRequirementOptions)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (requirement.All(schemeId => IsSecuritySchemeConfigured(requestOptions, schemeId)))");
        builder.AppendLine("            {");
        builder.AppendLine("                return requirement;");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        throw new InvalidOperationException(\"The supplied request options do not satisfy the authentication requirements for this operation.\");");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine($"    private static bool IsSecuritySchemeConfigured({runtimeContext.RequestOptionsTypeName} requestOptions, string schemeId)");
        builder.AppendLine("    {");
        builder.AppendLine("        return schemeId switch");
        builder.AppendLine("        {");
        foreach (var scheme in runtimeContext.SecuritySchemes)
        {
            builder.AppendLine($"            \"{EscapeString(scheme.Id)}\" => {RenderSecuritySchemeConfiguredExpression(scheme, runtimeContext.SecuritySchemes)},");
        }
        builder.AppendLine("            _ => false");
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        return builder.ToString().TrimEnd();
    }

    private static IEnumerable<string> RenderSecuritySchemeApplication(
        SecuritySchemeDefinition scheme,
        RuntimeContextDefinition runtimeContext)
    {
        var fields = GetSecuritySchemeFields(scheme, runtimeContext.SecuritySchemes);
        if (string.Equals(scheme.Type, "ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            var field = fields.Single();
            return scheme.Location?.ToLowerInvariant() switch
            {
                "header" =>
                [
                    $"await new ApiKeyAuthenticationProvider(_requestOptions.{field.PropertyName}!, \"{EscapeString(scheme.ParameterName ?? scheme.Id)}\", ApiKeyAuthenticationProvider.KeyLocation.Header, {RenderAllowedHostsArgument(runtimeContext)}).AuthenticateRequestAsync(request, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);"
                ],
                "cookie" => [$"request.Headers.Add(\"Cookie\", \"{EscapeString(scheme.ParameterName ?? scheme.Id)}=\" + _requestOptions.{field.PropertyName}!);"],
                _ =>
                [
                    $"await new ApiKeyAuthenticationProvider(_requestOptions.{field.PropertyName}!, \"{EscapeString(scheme.ParameterName ?? scheme.Id)}\", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter, {RenderAllowedHostsArgument(runtimeContext)}).AuthenticateRequestAsync(request, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);"
                ]
            };
        }

        if (string.Equals(scheme.Type, "Http", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(scheme.Scheme, "basic", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                $"await new BasicAuthenticationProvider(_requestOptions.{fields[0].PropertyName}!, _requestOptions.{fields[1].PropertyName}!).AuthenticateRequestAsync(request, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);"
            ];
        }

        var tokenField = fields.Single();
        return
        [
            $"await new BaseBearerTokenAuthenticationProvider(new GeneratedAccessTokenProvider(_requestOptions.{tokenField.PropertyName}!)).AuthenticateRequestAsync(request, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);"
        ];
    }

    private static string RenderAllowedHostsArgument(RuntimeContextDefinition runtimeContext)
    {
        if (string.IsNullOrWhiteSpace(runtimeContext.DefaultBaseUrl) ||
            !Uri.TryCreate(runtimeContext.DefaultBaseUrl, UriKind.Absolute, out var defaultUri) ||
            string.IsNullOrWhiteSpace(defaultUri.Host))
        {
            return "Array.Empty<string>()";
        }

        return $"new[] {{ \"{EscapeString(defaultUri.Host)}\" }}";
    }

    private static string RenderSecuritySchemeConfiguredExpression(
        SecuritySchemeDefinition scheme,
        IReadOnlyList<SecuritySchemeDefinition> allSchemes)
    {
        var fields = GetSecuritySchemeFields(scheme, allSchemes);
        return fields.Count switch
        {
            1 => $"!string.IsNullOrWhiteSpace(requestOptions.{fields[0].PropertyName})",
            _ => string.Join(" && ", fields.Select(field => $"!string.IsNullOrWhiteSpace(requestOptions.{field.PropertyName})"))
        };
    }

    private static IReadOnlyList<(string PropertyName, string Description)> GetSecuritySchemeFields(
        SecuritySchemeDefinition scheme,
        IReadOnlyList<SecuritySchemeDefinition> allSchemes)
    {
        var multipleSchemesOfType = allSchemes.Count(candidate =>
            string.Equals(candidate.Type, scheme.Type, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.Scheme, scheme.Scheme, StringComparison.OrdinalIgnoreCase)) > 1;
        var prefix = multipleSchemesOfType ? SanitizeIdentifier(scheme.Id) : string.Empty;

        if (string.Equals(scheme.Type, "ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            return [(string.IsNullOrWhiteSpace(prefix) ? "ApiKey" : $"{prefix}ApiKey", $"API key for security scheme '{scheme.Id}'.")];
        }

        if (string.Equals(scheme.Type, "Http", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(scheme.Scheme, "basic", StringComparison.OrdinalIgnoreCase))
        {
            var usernameName = string.IsNullOrWhiteSpace(prefix) ? "Username" : $"{prefix}Username";
            var passwordName = string.IsNullOrWhiteSpace(prefix) ? "Password" : $"{prefix}Password";
            return
            [
                (usernameName, $"Username for security scheme '{scheme.Id}'."),
                (passwordName, $"Password for security scheme '{scheme.Id}'.")
            ];
        }

        return [(string.IsNullOrWhiteSpace(prefix) ? "AccessToken" : $"{prefix}AccessToken", $"Access token for security scheme '{scheme.Id}'.")];
    }

    private static string SanitizeIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length);
        var capitalizeNext = true;
        foreach (var character in value)
        {
            if (!char.IsLetterOrDigit(character))
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return builder.Length == 0 ? "Auth" : builder.ToString();
    }
}

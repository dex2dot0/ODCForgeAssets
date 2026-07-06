namespace KiotaOutSystems.Generator;

internal sealed record KiotaMetadata(string ClientNamespace, string ClientClassName, string? DescriptionLocation, string? KiotaVersion, string? ClientRootPath);

internal sealed record NormalizedOpenApiDocument(
    IReadOnlyList<SchemaDefinition> Schemas,
    IReadOnlyList<OperationDefinition> Operations,
    RuntimeContextDefinition RuntimeContext);

internal sealed record SchemaDefinition(
    string Name,
    string DefaultName,
    string OriginalSchemaName,
    string? KiotaModelType,
    string Description,
    bool IsRequestShape,
    IReadOnlyList<PropertyDefinition> Properties);

internal sealed record PropertyDefinition(
    string Name,
    string DefaultName,
    string OriginalName,
    string CSharpType,
    bool IsNullable,
    string Description);

internal sealed record OperationDefinition(
    string Name,
    string DefaultName,
    string Description,
    string HttpMethod,
    string Path,
    string ResourcePropertyName,
    IReadOnlyList<ParameterDefinition> Parameters,
    BodyDefinition? RequestBody,
    ResponseDefinition? Response,
    IReadOnlyList<SecurityRequirementDefinition> SecurityRequirements);

internal sealed record ParameterDefinition(
    string Name,
    string DefaultName,
    string OriginalName,
    string Location,
    string CSharpType,
    bool IsNullable,
    string Description,
    string? QueryPropertyName = null,
    bool RequiresPresenceFlag = false,
    string? PresenceFlagName = null);

internal sealed record BodyDefinition(
    string SchemaName,
    string ModelSchemaName,
    string Kind = "json",
    string? MediaType = null);

internal sealed record ResponseDefinition(
    string Kind,
    string? SchemaName,
    string? MediaType = null);

internal sealed record RuntimeContextDefinition(
    string? DefaultBaseUrl,
    string RequestOptionsTypeName,
    IReadOnlyList<SecuritySchemeDefinition> SecuritySchemes);

internal sealed record SecuritySchemeDefinition(
    string Id,
    string Type,
    string? Location,
    string? ParameterName,
    string? Scheme,
    string? Description);

internal sealed record SecurityRequirementDefinition(
    IReadOnlyList<string> SchemeIds);

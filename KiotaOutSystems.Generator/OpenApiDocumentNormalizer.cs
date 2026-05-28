namespace KiotaOutSystems.Generator;

internal static class OpenApiDocumentNormalizer
{
    public static NormalizedOpenApiDocument Normalize(LoadedSpec loadedSpec, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        var loweredDocument = OpenApiSchemaLoweringPass.Lower(loadedSpec.Document);
        var filteredDocument = OpenApiOperationFilterPass.Apply(loweredDocument, config);
        return loadedSpec.SpecificationVersion.StartsWith("2.", StringComparison.Ordinal)
            ? OpenApi2DocumentNormalizer.Normalize(filteredDocument, kiotaMetadata, config)
            : OpenApi3DocumentNormalizer.Normalize(filteredDocument, kiotaMetadata, config);
    }
}

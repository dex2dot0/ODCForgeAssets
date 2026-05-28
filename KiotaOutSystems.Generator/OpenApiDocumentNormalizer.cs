namespace KiotaOutSystems.Generator;

internal static class OpenApiDocumentNormalizer
{
    public static NormalizedOpenApiDocument Normalize(LoadedSpec loadedSpec, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        var loweredDocument = OpenApiSchemaLoweringPass.Lower(loadedSpec.Document);
        return loadedSpec.SpecificationVersion.StartsWith("2.", StringComparison.Ordinal)
            ? OpenApi2DocumentNormalizer.Normalize(loweredDocument, kiotaMetadata, config)
            : OpenApi3DocumentNormalizer.Normalize(loweredDocument, kiotaMetadata, config);
    }
}

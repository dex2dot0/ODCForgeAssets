using Microsoft.OpenApi;

namespace KiotaOutSystems.Generator;

internal static class OpenApi2DocumentNormalizer
{
    public static NormalizedOpenApiDocument Normalize(OpenApiDocument document, KiotaMetadata kiotaMetadata, EffectiveGeneratorConfig config)
    {
        // OpenAPI.NET already projects most Swagger 2.0 constructs into the shared object model.
        // Keep the 2.x entry point separate so Swagger-only quirks can diverge cleanly as needed.
        return OpenApi3DocumentNormalizer.Normalize(document, kiotaMetadata, config);
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace KiotaOutSystems.Generator;

internal static class SpecSourceLoader
{
    public static async Task<LoadedSpec> LoadAsync(string specSource, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(specSource, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return Parse(content, specSource, $"sha256:{ComputeHash(content)}");
        }

        var fullPath = Path.GetFullPath(specSource);
        var contentFromFile = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
        return Parse(contentFromFile, fullPath, $"sha256:{ComputeHash(contentFromFile)}");
    }

    private static LoadedSpec Parse(string content, string resolvedSource, string contentHash)
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        var result = OpenApiDocument.Load(memoryStream, settings: settings);
        if (result.Diagnostic?.Errors.Count > 0)
        {
            var errors = string.Join(Environment.NewLine, result.Diagnostic.Errors.Select(error => error.Message));
            throw new InvalidOperationException($"Failed to parse the OpenAPI document:{Environment.NewLine}{errors}");
        }

        if (result.Document is null)
        {
            throw new InvalidOperationException("Failed to parse the OpenAPI document because no document was returned by the OpenAPI reader.");
        }

        return new LoadedSpec(result.Document, resolvedSource, contentHash, DetectSpecificationVersion(content));
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private static string DetectSpecificationVersion(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.TryGetProperty("swagger", out var swaggerVersion))
            {
                return swaggerVersion.GetString() ?? "2.0";
            }

            if (root.TryGetProperty("openapi", out var openApiVersion))
            {
                return openApiVersion.GetString() ?? "3.0.0";
            }
        }
        catch (JsonException)
        {
        }

        var swaggerMatch = Regex.Match(content, @"(?m)^\s*swagger\s*:\s*[""']?(?<version>[^""'\r\n#]+)");
        if (swaggerMatch.Success)
        {
            return swaggerMatch.Groups["version"].Value.Trim();
        }

        var openApiMatch = Regex.Match(content, @"(?m)^\s*openapi\s*:\s*[""']?(?<version>[^""'\r\n#]+)");
        if (openApiMatch.Success)
        {
            return openApiMatch.Groups["version"].Value.Trim();
        }

        return "3.0.0";
    }
}

internal sealed record LoadedSpec(OpenApiDocument Document, string ResolvedSource, string ContentHash, string SpecificationVersion);

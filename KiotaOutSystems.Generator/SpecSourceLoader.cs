using System.Security.Cryptography;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

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
        var document = new OpenApiStreamReader().Read(memoryStream, out var diagnostic);
        if (diagnostic?.Errors.Count > 0)
        {
            var errors = string.Join(Environment.NewLine, diagnostic.Errors.Select(error => error.Message));
            throw new InvalidOperationException($"Failed to parse the OpenAPI document:{Environment.NewLine}{errors}");
        }

        return new LoadedSpec(document, resolvedSource, contentHash);
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}

internal sealed record LoadedSpec(OpenApiDocument Document, string ResolvedSource, string ContentHash);

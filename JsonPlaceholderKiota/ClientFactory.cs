using JsonPlaceholderKiota.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;

namespace JsonPlaceholderKiota;

internal interface IPostsClientFactory
{
    PostsClient CreateClient();
}

internal sealed class PostsClientFactory : IPostsClientFactory
{
    private readonly HttpClient? _httpClient;

    public PostsClientFactory(HttpClient? httpClient = null)
    {
        _httpClient = httpClient;
    }

    public PostsClient CreateClient()
    {
        var requestAdapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            _httpClient ?? new HttpClient(),
            null);

        return new PostsClient(requestAdapter);
    }
}

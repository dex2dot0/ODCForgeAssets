#nullable enable
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using OutSystems.ExternalLibraries.SDK;

namespace JsonPlaceholderKiota.Generated;

internal static class GeneratedModelMapper
{
    public static Post ToStructure(JsonPlaceholderKiota.Client.Models.Post? model)
    {
        if (model is null)
        {
            return default;
        }

        return new Post
        {
            UserId = model.UserId,
            Id = model.Id,
            Title = model.Title,
            Body = model.Body,
        };
    }

    public static JsonPlaceholderKiota.Client.Models.Post ToModel(Post structure)
    {
        return new JsonPlaceholderKiota.Client.Models.Post
        {
            UserId = structure.UserId,
            Id = structure.Id,
            Title = structure.Title,
            Body = structure.Body,
        };
    }

    public static JsonPlaceholderKiota.Client.Models.Post ToModel(CreatePostRequest structure)
    {
        return new JsonPlaceholderKiota.Client.Models.Post
        {
            UserId = structure.UserId,
            Id = structure.Id,
            Title = structure.Title,
            Body = structure.Body,
        };
    }

    public static JsonPlaceholderKiota.Client.Models.Post ToModel(UpdatePostRequest structure)
    {
        return new JsonPlaceholderKiota.Client.Models.Post
        {
            UserId = structure.UserId,
            Id = structure.Id,
            Title = structure.Title,
            Body = structure.Body,
        };
    }

}

public class JsonPlaceholderPostsGenerated : IJsonPlaceholderPostsGenerated
{
    private readonly JsonPlaceholderKiota.Client.PostsClient _client;

    public JsonPlaceholderPostsGenerated()
    {
        var requestAdapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            new HttpClient(),
            null);

        _client = new JsonPlaceholderKiota.Client.PostsClient(requestAdapter);
    }

    public List<Post> ListPosts(int userId = 0, bool includeUserId = false, string title = "", bool includeTitle = false)
    {
        var requestBuilder = _client.Posts;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeUserId)
            {
                config.QueryParameters.UserId = userId;
            }
            if (includeTitle)
            {
                config.QueryParameters.Title = title;
            }
        }).GetAwaiter().GetResult();

        return response?.Select(GeneratedModelMapper.ToStructure).ToList() ?? new List<Post>();
    }

    public Post CreatePost(CreatePostRequest createPostRequest)
    {
        var requestBuilder = _client.Posts;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.ToModel(createPostRequest)).GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public Post GetPostById(int postId)
    {
        var requestBuilder = _client.Posts;
        var itemRequestBuilder = requestBuilder[postId];
        var response = itemRequestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public Post UpdatePost(int postId, UpdatePostRequest updatePostRequest)
    {
        var requestBuilder = _client.Posts;
        var itemRequestBuilder = requestBuilder[postId];
        var response = itemRequestBuilder.PatchAsync(GeneratedModelMapper.ToModel(updatePostRequest)).GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public void DeletePost(int postId)
    {
        var requestBuilder = _client.Posts;
        var itemRequestBuilder = requestBuilder[postId];
        using var responseStream = itemRequestBuilder.DeleteAsync().GetAwaiter().GetResult();
    }

}

[OSInterface(Description = "Generated OutSystems wrapper for JSONPlaceholder.", Name = "JsonPlaceholderPostsGenerated", IconResourceName = "JsonPlaceholderKiota.resources.ProjectIcon.png")]
public interface IJsonPlaceholderPostsGenerated
{
    [OSAction(Description = "Lists posts with optional user ID and title filters.", ReturnDescription = "The result returned by the API.", ReturnName = "Items")]
    List<Post> ListPosts([OSParameter(Description = "Filter results by user ID")] int userId = 0, [OSParameter(Description = "Set to true to send the userId query parameter to the API.")] bool includeUserId = false, [OSParameter(Description = "Filter results by title")] string title = "", [OSParameter(Description = "Set to true to send the title query parameter to the API.")] bool includeTitle = false);

    [OSAction(Description = "Creates a post in the downstream API.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Post CreatePost([OSParameter(Description = "The JSON request body payload.")] CreatePostRequest createPostRequest);

    [OSAction(Description = "Gets a post by its identifier.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Post GetPostById([OSParameter(Description = "The id of post path parameter.")] int postId);

    [OSAction(Description = "Updates a post by its identifier.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Post UpdatePost([OSParameter(Description = "The id of post path parameter.")] int postId, [OSParameter(Description = "The JSON request body payload.")] UpdatePostRequest updatePostRequest);

    [OSAction(Description = "Deletes a post by its identifier.")]
    void DeletePost([OSParameter(Description = "The id of post path parameter.")] int postId);

}

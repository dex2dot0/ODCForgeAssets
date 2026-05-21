namespace JsonPlaceholderKiota;

/// <summary>
/// Exposes JSONPlaceholder post operations through OutSystems server actions.
/// </summary>
public class JsonPlaceholderPosts
{
    private readonly IPostsClientFactory _clientFactory;

    public JsonPlaceholderPosts() : this(new PostsClientFactory())
    {
    }

    internal JsonPlaceholderPosts(IPostsClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public List<PostItem> ListPosts(int? userId = null, string? title = null)
    {
        var client = _clientFactory.CreateClient();
        var response = client.Posts.GetAsync(config =>
        {
            config.QueryParameters.UserId = userId;
            config.QueryParameters.Title = title;
        }).GetAwaiter().GetResult();

        return response?.Select(PostItemMapper.ToStructure).ToList() ?? new List<PostItem>();
    }

    public PostItem GetPostById(int postId)
    {
        var client = _clientFactory.CreateClient();
        var response = client.Posts[postId].GetAsync().GetAwaiter().GetResult();

        return PostItemMapper.ToStructure(response);
    }

    public PostItem CreatePost(PostItem post)
    {
        var client = _clientFactory.CreateClient();
        var response = client.Posts.PostAsync(PostItemMapper.ToKiotaModel(post)).GetAwaiter().GetResult();

        return PostItemMapper.ToStructure(response);
    }

    public PostItem UpdatePost(int postId, PostItem post)
    {
        var client = _clientFactory.CreateClient();
        var response = client.Posts[postId].PatchAsync(PostItemMapper.ToKiotaModel(post)).GetAwaiter().GetResult();

        return PostItemMapper.ToStructure(response);
    }

    public void DeletePost(int postId)
    {
        var client = _clientFactory.CreateClient();
        using var responseStream = client.Posts[postId].DeleteAsync().GetAwaiter().GetResult();
    }
}

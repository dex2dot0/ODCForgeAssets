using JsonPlaceholderKiota.Client.Models;

namespace JsonPlaceholderKiota;

internal static class PostItemMapper
{
    public static PostItem ToStructure(Post? model)
    {
        if (model is null)
        {
            return default;
        }

        return new PostItem
        {
            Body = model.Body,
            Id = model.Id,
            Title = model.Title,
            UserId = model.UserId
        };
    }

    public static Post ToKiotaModel(PostItem structure)
    {
        return new Post
        {
            Body = structure.Body,
            Id = structure.Id,
            Title = structure.Title,
            UserId = structure.UserId
        };
    }
}

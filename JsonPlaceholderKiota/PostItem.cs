using OutSystems.ExternalLibraries.SDK;

namespace JsonPlaceholderKiota;

[OSStructure(Description = "Represents a JSONPlaceholder post.")]
public struct PostItem
{
    [OSStructureField(Description = "The post body text.")]
    public string? Body { get; set; }

    [OSStructureField(Description = "The unique identifier of the post.")]
    public int? Id { get; set; }

    [OSStructureField(Description = "The post title.")]
    public string? Title { get; set; }

    [OSStructureField(Description = "The identifier of the user who owns the post.")]
    public int? UserId { get; set; }
}

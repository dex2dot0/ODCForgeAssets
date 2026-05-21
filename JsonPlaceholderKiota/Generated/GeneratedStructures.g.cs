#nullable enable
using OutSystems.ExternalLibraries.SDK;

namespace JsonPlaceholderKiota.Generated;

[OSStructure(Description = "Represents the post schema from the source OpenAPI document.")]
public struct Post
{
    [OSStructureField(Description = "Maps the 'userId' field.")]
    public int? UserId { get; set; }

    [OSStructureField(Description = "Maps the 'id' field.")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'title' field.")]
    public string? Title { get; set; }

    [OSStructureField(Description = "Maps the 'body' field.")]
    public string? Body { get; set; }

}

[OSStructure(Description = "Payload used to create a post.")]
public struct CreatePostRequest
{
    [OSStructureField(Description = "Maps the 'userId' field.")]
    public int? UserId { get; set; }

    [OSStructureField(Description = "Maps the 'id' field.")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'title' field.")]
    public string? Title { get; set; }

    [OSStructureField(Description = "Maps the 'body' field.")]
    public string? Body { get; set; }

}

[OSStructure(Description = "Payload used to update a post.")]
public struct UpdatePostRequest
{
    [OSStructureField(Description = "Maps the 'userId' field.")]
    public int? UserId { get; set; }

    [OSStructureField(Description = "Maps the 'id' field.")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'title' field.")]
    public string? Title { get; set; }

    [OSStructureField(Description = "Maps the 'body' field.")]
    public string? Body { get; set; }

}

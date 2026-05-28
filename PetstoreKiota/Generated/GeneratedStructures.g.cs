#nullable enable
using OutSystems.ExternalLibraries.SDK;

namespace PetstoreKiota.Generated;

[OSStructure(Description = "Represents the Category schema from the source OpenAPI document.")]
public struct Category
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'name' field.", OriginalName = "name")]
    public string? Name { get; set; }

}

[OSStructure(Description = "Represents the Tag schema from the source OpenAPI document.")]
public struct Tag
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'name' field.", OriginalName = "name")]
    public string? Name { get; set; }

}

[OSStructure(Description = "Represents the Pet schema from the source OpenAPI document.")]
public struct Pet
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'category' field.", OriginalName = "category")]
    public Category? Category { get; set; }

    [OSStructureField(Description = "Maps the 'name' field.", OriginalName = "name")]
    public string Name { get; set; }

    [OSStructureField(Description = "Maps the 'photoUrls' field.", OriginalName = "photoUrls")]
    public List<string> PhotoUrls { get; set; }

    [OSStructureField(Description = "Maps the 'tags' field.", OriginalName = "tags")]
    public List<Tag> Tags { get; set; }

    [OSStructureField(Description = "pet status in the store", OriginalName = "status")]
    public string? Status { get; set; }

}

[OSStructure(Description = "Represents the ApiResponse schema from the source OpenAPI document.")]
public struct ApiResponse
{
    [OSStructureField(Description = "Maps the 'code' field.", OriginalName = "code")]
    public int? Code { get; set; }

    [OSStructureField(Description = "Maps the 'type' field.", OriginalName = "type")]
    public string? Type { get; set; }

    [OSStructureField(Description = "Maps the 'message' field.", OriginalName = "message")]
    public string? Message { get; set; }

}

[OSStructure(Description = "Represents the request body for UpdatePetWithForm.")]
public struct UpdatePetWithFormRequest
{
    [OSStructureField(Description = "Updated name of the pet", OriginalName = "name")]
    public string? Name { get; set; }

    [OSStructureField(Description = "Updated status of the pet", OriginalName = "status")]
    public string? Status { get; set; }

}

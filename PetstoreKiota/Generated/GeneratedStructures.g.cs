#nullable enable
using OutSystems.ExternalLibraries.SDK;

namespace PetstoreKiota.Generated;

[OSStructure(Description = "Represents the Order schema from the source OpenAPI document.")]
public struct Order
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'petId' field.", OriginalName = "petId")]
    public int? PetId { get; set; }

    [OSStructureField(Description = "Maps the 'quantity' field.", OriginalName = "quantity")]
    public int? Quantity { get; set; }

    [OSStructureField(Description = "Maps the 'shipDate' field.", OriginalName = "shipDate")]
    public DateTime? ShipDate { get; set; }

    [OSStructureField(Description = "Order Status", OriginalName = "status")]
    public string? Status { get; set; }

    [OSStructureField(Description = "Maps the 'complete' field.", OriginalName = "complete")]
    public bool? Complete { get; set; }

}

[OSStructure(Description = "Represents the Category schema from the source OpenAPI document.")]
public struct Category
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'name' field.", OriginalName = "name")]
    public string? Name { get; set; }

}

[OSStructure(Description = "Represents the User schema from the source OpenAPI document.")]
public struct User
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'username' field.", OriginalName = "username")]
    public string? Username { get; set; }

    [OSStructureField(Description = "Maps the 'firstName' field.", OriginalName = "firstName")]
    public string? FirstName { get; set; }

    [OSStructureField(Description = "Maps the 'lastName' field.", OriginalName = "lastName")]
    public string? LastName { get; set; }

    [OSStructureField(Description = "Maps the 'email' field.", OriginalName = "email")]
    public string? Email { get; set; }

    [OSStructureField(Description = "Maps the 'password' field.", OriginalName = "password")]
    public string? Password { get; set; }

    [OSStructureField(Description = "Maps the 'phone' field.", OriginalName = "phone")]
    public string? Phone { get; set; }

    [OSStructureField(Description = "User Status", OriginalName = "userStatus")]
    public int? UserStatus { get; set; }

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

[OSStructure(Description = "Represents the request body for AddPet.")]
public struct AddPetRequest
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

[OSStructure(Description = "Represents the request body for UpdatePet.")]
public struct UpdatePetRequest
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

[OSStructure(Description = "Represents the request body for PlaceOrder.")]
public struct PlaceOrderRequest
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'petId' field.", OriginalName = "petId")]
    public int? PetId { get; set; }

    [OSStructureField(Description = "Maps the 'quantity' field.", OriginalName = "quantity")]
    public int? Quantity { get; set; }

    [OSStructureField(Description = "Maps the 'shipDate' field.", OriginalName = "shipDate")]
    public DateTime? ShipDate { get; set; }

    [OSStructureField(Description = "Order Status", OriginalName = "status")]
    public string? Status { get; set; }

    [OSStructureField(Description = "Maps the 'complete' field.", OriginalName = "complete")]
    public bool? Complete { get; set; }

}

[OSStructure(Description = "Represents the request body for CreateUser.")]
public struct CreateUserRequest
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'username' field.", OriginalName = "username")]
    public string? Username { get; set; }

    [OSStructureField(Description = "Maps the 'firstName' field.", OriginalName = "firstName")]
    public string? FirstName { get; set; }

    [OSStructureField(Description = "Maps the 'lastName' field.", OriginalName = "lastName")]
    public string? LastName { get; set; }

    [OSStructureField(Description = "Maps the 'email' field.", OriginalName = "email")]
    public string? Email { get; set; }

    [OSStructureField(Description = "Maps the 'password' field.", OriginalName = "password")]
    public string? Password { get; set; }

    [OSStructureField(Description = "Maps the 'phone' field.", OriginalName = "phone")]
    public string? Phone { get; set; }

    [OSStructureField(Description = "User Status", OriginalName = "userStatus")]
    public int? UserStatus { get; set; }

}

[OSStructure(Description = "Represents the request body for UpdateUser.")]
public struct UpdateUserRequest
{
    [OSStructureField(Description = "Maps the 'id' field.", OriginalName = "id")]
    public int? Id { get; set; }

    [OSStructureField(Description = "Maps the 'username' field.", OriginalName = "username")]
    public string? Username { get; set; }

    [OSStructureField(Description = "Maps the 'firstName' field.", OriginalName = "firstName")]
    public string? FirstName { get; set; }

    [OSStructureField(Description = "Maps the 'lastName' field.", OriginalName = "lastName")]
    public string? LastName { get; set; }

    [OSStructureField(Description = "Maps the 'email' field.", OriginalName = "email")]
    public string? Email { get; set; }

    [OSStructureField(Description = "Maps the 'password' field.", OriginalName = "password")]
    public string? Password { get; set; }

    [OSStructureField(Description = "Maps the 'phone' field.", OriginalName = "phone")]
    public string? Phone { get; set; }

    [OSStructureField(Description = "User Status", OriginalName = "userStatus")]
    public int? UserStatus { get; set; }

}

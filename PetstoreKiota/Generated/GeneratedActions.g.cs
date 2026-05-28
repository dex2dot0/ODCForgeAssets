#nullable enable
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using OutSystems.ExternalLibraries.SDK;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetstoreKiota.Generated;

internal static class GeneratedModelMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static TDestination? Convert<TDestination>(object? source)
    {
        if (source is null)
        {
            return default;
        }

        var json = JsonSerializer.Serialize(source, SerializerOptions);
        return JsonSerializer.Deserialize<TDestination>(json, SerializerOptions);
    }

    public static Order ToStructure(PetstoreKiota.Client.Models.Order? model)
    {
        return Convert<Order>(model);
    }

    public static PetstoreKiota.Client.Models.Order ToModel(Order structure)
    {
        return Convert<PetstoreKiota.Client.Models.Order>(structure)!;
    }

    public static Category ToStructure(PetstoreKiota.Client.Models.Category? model)
    {
        return Convert<Category>(model);
    }

    public static PetstoreKiota.Client.Models.Category ToModel(Category structure)
    {
        return Convert<PetstoreKiota.Client.Models.Category>(structure)!;
    }

    public static User ToStructure(PetstoreKiota.Client.Models.User? model)
    {
        return Convert<User>(model);
    }

    public static PetstoreKiota.Client.Models.User ToModel(User structure)
    {
        return Convert<PetstoreKiota.Client.Models.User>(structure)!;
    }

    public static Tag ToStructure(PetstoreKiota.Client.Models.Tag? model)
    {
        return Convert<Tag>(model);
    }

    public static PetstoreKiota.Client.Models.Tag ToModel(Tag structure)
    {
        return Convert<PetstoreKiota.Client.Models.Tag>(structure)!;
    }

    public static Pet ToStructure(PetstoreKiota.Client.Models.Pet? model)
    {
        return Convert<Pet>(model);
    }

    public static PetstoreKiota.Client.Models.Pet ToModel(Pet structure)
    {
        return Convert<PetstoreKiota.Client.Models.Pet>(structure)!;
    }

    public static ApiResponse ToStructure(PetstoreKiota.Client.Models.ApiResponse? model)
    {
        return Convert<ApiResponse>(model);
    }

    public static PetstoreKiota.Client.Models.ApiResponse ToModel(ApiResponse structure)
    {
        return Convert<PetstoreKiota.Client.Models.ApiResponse>(structure)!;
    }

    public static PetstoreKiota.Client.Models.Pet ToModel(AddPetRequest structure)
    {
        return Convert<PetstoreKiota.Client.Models.Pet>(structure)!;
    }

    public static PetstoreKiota.Client.Models.Pet ToModel(UpdatePetRequest structure)
    {
        return Convert<PetstoreKiota.Client.Models.Pet>(structure)!;
    }

    public static PetstoreKiota.Client.Models.Order ToModel(PlaceOrderRequest structure)
    {
        return Convert<PetstoreKiota.Client.Models.Order>(structure)!;
    }

    public static PetstoreKiota.Client.Models.User ToModel(CreateUserRequest structure)
    {
        return Convert<PetstoreKiota.Client.Models.User>(structure)!;
    }

    public static PetstoreKiota.Client.Models.User ToModel(UpdateUserRequest structure)
    {
        return Convert<PetstoreKiota.Client.Models.User>(structure)!;
    }

}

public class PetstoreActionsGenerated : IPetstoreActionsGenerated
{
    private readonly PetstoreKiota.Client.PetstoreClient _client;

    public PetstoreActionsGenerated()
    {
        var requestAdapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            new HttpClient(),
            null);

        _client = new PetstoreKiota.Client.PetstoreClient(requestAdapter);
    }

    public void AddPet(AddPetRequest requestBody)
    {
        var requestBuilder = _client.Pet;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.ToModel(requestBody)).GetAwaiter().GetResult();
    }

    public void UpdatePet(UpdatePetRequest requestBody)
    {
        var requestBuilder = _client.Pet;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.ToModel(requestBody)).GetAwaiter().GetResult();
    }

    public List<Pet> FindPetsByStatus(List<string> status)
    {
        var requestBuilder = _client.Pet.FindByStatus;
        var response = requestBuilder.GetAsync(config =>
        {
            config.QueryParameters.Status = status.ToArray();
        }).GetAwaiter().GetResult();

        return response?.Select(GeneratedModelMapper.ToStructure).ToList() ?? new List<Pet>();
    }

    public List<Pet> FindPetsByTags(List<string> tags)
    {
        var requestBuilder = _client.Pet.FindByTags;
        var response = requestBuilder.GetAsync(config =>
        {
            config.QueryParameters.Tags = tags.ToArray();
        }).GetAwaiter().GetResult();

        return response?.Select(GeneratedModelMapper.ToStructure).ToList() ?? new List<Pet>();
    }

    public Pet GetPetById(int petId)
    {
        var requestBuilder = _client.Pet[petId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public void DeletePet(int petId, string? apiKey = null)
    {
        var requestBuilder = _client.Pet[petId];
        using var responseStream = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
    }

    public Order PlaceOrder(PlaceOrderRequest requestBody)
    {
        var requestBuilder = _client.Store.Order;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.ToModel(requestBody)).GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public Order GetOrderById(int orderId)
    {
        var requestBuilder = _client.Store.Order[orderId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public void DeleteOrder(int orderId)
    {
        var requestBuilder = _client.Store.Order[orderId];
        using var responseStream = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
    }

    public void CreateUser(CreateUserRequest requestBody)
    {
        var requestBuilder = _client.User;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.ToModel(requestBody)).GetAwaiter().GetResult();
    }

    public void CreateUsersWithArrayInput(List<User> requestBody)
    {
        var requestBuilder = _client.User.CreateWithArray;
        var response = requestBuilder.PostAsync(requestBody.Select(GeneratedModelMapper.ToModel).ToList()).GetAwaiter().GetResult();
    }

    public void CreateUsersWithListInput(List<User> requestBody)
    {
        var requestBuilder = _client.User.CreateWithList;
        var response = requestBuilder.PostAsync(requestBody.Select(GeneratedModelMapper.ToModel).ToList()).GetAwaiter().GetResult();
    }

    public string LoginUser(string username, string password)
    {
        var requestBuilder = _client.User.Login;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return response ?? string.Empty;
    }

    public void LogoutUser()
    {
        var requestBuilder = _client.User.Logout;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
    }

    public User GetUserByName(string username)
    {
        var requestBuilder = _client.User[username];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ToStructure(response);
    }

    public void UpdateUser(string username, UpdateUserRequest requestBody)
    {
        var requestBuilder = _client.User[username];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.ToModel(requestBody)).GetAwaiter().GetResult();
    }

    public void DeleteUser(string username)
    {
        var requestBuilder = _client.User[username];
        using var responseStream = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
    }

}

[OSInterface(Description = "Generated OutSystems wrapper for Swagger Petstore.", Name = "PetstoreActionsGenerated")]
public interface IPetstoreActionsGenerated
{
    [OSAction(Description = "Add a new pet to the store")]
    void AddPet([OSParameter(Description = "The JSON request body payload.")] AddPetRequest requestBody);

    [OSAction(Description = "Update an existing pet")]
    void UpdatePet([OSParameter(Description = "The JSON request body payload.")] UpdatePetRequest requestBody);

    [OSAction(Description = "Finds Pets by status", ReturnDescription = "The result returned by the API.", ReturnName = "Items")]
    List<Pet> FindPetsByStatus([OSParameter(Description = "Status values that need to be considered for filter")] List<string> status);

    [OSAction(Description = "Finds Pets by tags", ReturnDescription = "The result returned by the API.", ReturnName = "Items")]
    List<Pet> FindPetsByTags([OSParameter(Description = "Tags to filter by")] List<string> tags);

    [OSAction(Description = "Find pet by ID", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Pet GetPetById([OSParameter(Description = "ID of pet to return")] int petId);

    [OSAction(Description = "Deletes a pet")]
    void DeletePet([OSParameter(Description = "Pet id to delete")] int petId, [OSParameter(Description = "api_key Header parameter.")] string? apiKey = null);

    [OSAction(Description = "Place an order for a pet", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Order PlaceOrder([OSParameter(Description = "The JSON request body payload.")] PlaceOrderRequest requestBody);

    [OSAction(Description = "Find purchase order by ID", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Order GetOrderById([OSParameter(Description = "ID of pet that needs to be fetched")] int orderId);

    [OSAction(Description = "Delete purchase order by ID")]
    void DeleteOrder([OSParameter(Description = "ID of the order that needs to be deleted")] int orderId);

    [OSAction(Description = "Create user")]
    void CreateUser([OSParameter(Description = "The JSON request body payload.")] CreateUserRequest requestBody);

    [OSAction(Description = "Creates list of users with given input array")]
    void CreateUsersWithArrayInput([OSParameter(Description = "The JSON request body payload.")] List<User> requestBody);

    [OSAction(Description = "Creates list of users with given input array")]
    void CreateUsersWithListInput([OSParameter(Description = "The JSON request body payload.")] List<User> requestBody);

    [OSAction(Description = "Logs user into the system", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    string LoginUser([OSParameter(Description = "The user name for login")] string username, [OSParameter(Description = "The password for login in clear text")] string password);

    [OSAction(Description = "Logs out current logged in user session")]
    void LogoutUser();

    [OSAction(Description = "Get user by user name", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    User GetUserByName([OSParameter(Description = "The name that needs to be fetched. Use user1 for testing.")] string username);

    [OSAction(Description = "Updated user")]
    void UpdateUser([OSParameter(Description = "name that need to be updated")] string username, [OSParameter(Description = "The JSON request body payload.")] UpdateUserRequest requestBody);

    [OSAction(Description = "Delete user")]
    void DeleteUser([OSParameter(Description = "The name that needs to be deleted")] string username);

}

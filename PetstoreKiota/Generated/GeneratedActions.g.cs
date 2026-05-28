#nullable enable
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using OutSystems.ExternalLibraries.SDK;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetstoreKiota.Generated;

internal static class GeneratedModelMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(), new JsonStringOrRawJsonConverter() }
    };

    private sealed class JsonStringOrRawJsonConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.Object or JsonValueKind.Array => document.RootElement.GetRawText(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => document.RootElement.ToString()
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public static TDestination? Convert<TDestination>(object? source)
    {
        if (source is null)
        {
            return default;
        }

        var json = JsonSerializer.Serialize(source, SerializerOptions);
        return JsonSerializer.Deserialize<TDestination>(json, SerializerOptions);
    }

    public static byte[] ReadAllBytes(Stream? source)
    {
        if (source is null)
        {
            return Array.Empty<byte>();
        }

        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        return buffer.ToArray();
    }

    public static TDestination ParseJson<TDestination>(string source, ParsableFactory<TDestination> factory) where TDestination : IParsable
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(source) ? "{}" : source));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync("application/json", stream).GetAwaiter().GetResult();
        return parseNode.GetObjectValue(factory)!;
    }

    public static TEnum? ParseEnum<TEnum>(string? source) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            var enumMember = member?.GetCustomAttribute<EnumMemberAttribute>();
            if (string.Equals(enumMember?.Value, source, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        var normalized = NormalizeEnumToken(source);
        return Enum.TryParse<TEnum>(normalized, true, out var parsed) ? parsed : null;
    }

    public static TEnum[] ParseEnumArray<TEnum>(IEnumerable<string>? source) where TEnum : struct, Enum
    {
        if (source is null)
        {
            return Array.Empty<TEnum>();
        }

        return source
            .Select(ParseEnum<TEnum>)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToArray();
    }

    public static Microsoft.Kiota.Abstractions.Date ToKiotaDate(DateTime source)
    {
        return new Microsoft.Kiota.Abstractions.Date(source.Year, source.Month, source.Day);
    }

    public static string SerializeObject(object? source)
    {
        if (source is null)
        {
            return string.Empty;
        }

        if (source is IAdditionalDataHolder additionalDataHolder && HasNoDeclaredPayload(source.GetType()))
        {
            return JsonSerializer.Serialize(additionalDataHolder.AdditionalData, SerializerOptions);
        }

        return JsonSerializer.Serialize(source, SerializerOptions);
    }

    private static bool HasNoDeclaredPayload(Type type)
    {
        return !type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Any(property => !string.Equals(property.Name, nameof(IAdditionalDataHolder.AdditionalData), StringComparison.Ordinal));
    }

    private static string NormalizeEnumToken(string source)
    {
        var builder = new StringBuilder(source.Length);
        var capitalizeNext = true;
        foreach (var character in source)
        {
            if (!char.IsLetterOrDigit(character))
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return builder.Length == 0 ? source : builder.ToString();
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

    public void AddPet(Pet requestBody)
    {
        var requestBuilder = _client.Pet;
        requestBuilder.PostAsync(GeneratedModelMapper.Convert<PetstoreKiota.Client.Models.Pet>(requestBody)!).GetAwaiter().GetResult();
    }

    public void UpdatePet(Pet requestBody)
    {
        var requestBuilder = _client.Pet;
        requestBuilder.PutAsync(GeneratedModelMapper.Convert<PetstoreKiota.Client.Models.Pet>(requestBody)!).GetAwaiter().GetResult();
    }

    public List<Pet> FindPetsByStatus(List<string> status)
    {
        var requestBuilder = _client.Pet.FindByStatus;
        var response = requestBuilder.GetAsync(config =>
        {
            config.QueryParameters.StatusAsGetStatusQueryParameterType = GeneratedModelMapper.ParseEnumArray<PetstoreKiota.Client.Pet.FindByStatus.GetStatusQueryParameterType>(status);
        }).GetAwaiter().GetResult();

        return response?.Select(item => GeneratedModelMapper.Convert<Pet>(item)!).ToList() ?? new List<Pet>();
    }

    public List<Pet> FindPetsByTags(List<string> tags)
    {
        var requestBuilder = _client.Pet.FindByTags;
        var response = requestBuilder.GetAsync(config =>
        {
            config.QueryParameters.Tags = (tags ?? new List<string>()).ToArray();
        }).GetAwaiter().GetResult();

        return response?.Select(item => GeneratedModelMapper.Convert<Pet>(item)!).ToList() ?? new List<Pet>();
    }

    public Pet GetPetById(int petId)
    {
        var requestBuilder = _client.Pet[petId.ToString()];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Pet>(response)!;
    }

    public void UpdatePetWithForm(int petId, UpdatePetWithFormRequest requestBody)
    {
        var requestBuilder = _client.Pet[petId.ToString()];
        requestBuilder.PostAsync(GeneratedModelMapper.Convert<PetstoreKiota.Client.Pet.Item.WithPetPostRequestBody>(requestBody)!).GetAwaiter().GetResult();
    }

    public void DeletePet(int petId, string? apiKey = null)
    {
        var requestBuilder = _client.Pet[petId.ToString()];
        requestBuilder.DeleteAsync().GetAwaiter().GetResult();
    }

    public ApiResponse UploadFile(int petId, byte[] requestBody)
    {
        var requestBuilder = _client.Pet[petId.ToString()].UploadImage;
        var response = requestBuilder.PostAsync(new MemoryStream(requestBody ?? Array.Empty<byte>())).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ApiResponse>(response)!;
    }

}

[OSInterface(Description = "Generated OutSystems wrapper for Swagger Petstore.", Name = "PetstoreActionsGenerated")]
public interface IPetstoreActionsGenerated
{
    [OSAction(Description = "Add a new pet to the store")]
    void AddPet([OSParameter(Description = "The JSON request body payload.")] Pet requestBody);

    [OSAction(Description = "Update an existing pet")]
    void UpdatePet([OSParameter(Description = "The JSON request body payload.")] Pet requestBody);

    [OSAction(Description = "Finds Pets by status", ReturnDescription = "The result returned by the API.", ReturnName = "Items")]
    List<Pet> FindPetsByStatus([OSParameter(Description = "Status values that need to be considered for filter")] List<string> status);

    [OSAction(Description = "Finds Pets by tags", ReturnDescription = "The result returned by the API.", ReturnName = "Items")]
    List<Pet> FindPetsByTags([OSParameter(Description = "Tags to filter by")] List<string> tags);

    [OSAction(Description = "Find pet by ID", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Pet GetPetById([OSParameter(Description = "ID of pet to return")] int petId);

    [OSAction(Description = "Updates a pet in the store with form data")]
    void UpdatePetWithForm([OSParameter(Description = "ID of pet that needs to be updated")] int petId, [OSParameter(Description = "The JSON request body payload.")] UpdatePetWithFormRequest requestBody);

    [OSAction(Description = "Deletes a pet")]
    void DeletePet([OSParameter(Description = "Pet id to delete")] int petId, [OSParameter(Description = "api_key Header parameter.")] string? apiKey = null);

    [OSAction(Description = "uploads an image", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ApiResponse UploadFile([OSParameter(Description = "ID of pet to update")] int petId, [OSParameter(Description = "The binary request body payload.")] byte[] requestBody);

}

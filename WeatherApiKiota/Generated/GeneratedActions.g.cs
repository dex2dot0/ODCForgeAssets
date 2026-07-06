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

namespace WeatherApiKiota.Generated;

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

internal sealed class GeneratedAccessTokenProvider : IAccessTokenProvider
{
    private readonly string? _accessToken;

    public GeneratedAccessTokenProvider(string? accessToken)
    {
        _accessToken = accessToken;
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new();

    public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_accessToken ?? string.Empty);
    }
}

internal sealed class GeneratedAuthenticationProvider : IAuthenticationProvider
{
    private readonly RequestOptions _requestOptions;
    private readonly HashSet<string> _activeSchemeIds;

    public GeneratedAuthenticationProvider(RequestOptions requestOptions, IEnumerable<string> activeSchemeIds)
    {
        _requestOptions = requestOptions;
        _activeSchemeIds = new HashSet<string>(activeSchemeIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        foreach (var schemeId in _activeSchemeIds)
        {
            switch (schemeId)
            {
                case "ApiKeyAuth":
                    await new ApiKeyAuthenticationProvider(_requestOptions.ApiKey!, "key", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter, new[] { "api.weatherapi.com" }).AuthenticateRequestAsync(request, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        return;
    }
}
public class WeatherApiActionsGenerated : IWeatherApiActionsGenerated
{
    public WeatherApiActionsGenerated()
    {
    }

    private static WeatherApiKiota.Client.WeatherApiClient CreateClient(RequestOptions requestOptions, IEnumerable<string> activeSecuritySchemes)
    {
        var requestAdapter = new HttpClientRequestAdapter(
            new GeneratedAuthenticationProvider(requestOptions, activeSecuritySchemes),
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            new HttpClient(),
            null);

        var resolvedBaseUrl = string.IsNullOrWhiteSpace(requestOptions.BaseUrl) ? "https://api.weatherapi.com/v1" : requestOptions.BaseUrl!;
        if (!string.IsNullOrWhiteSpace(resolvedBaseUrl))
        {
            requestAdapter.BaseUrl = resolvedBaseUrl;
        }

        return new WeatherApiKiota.Client.WeatherApiClient(requestAdapter);
    }

    private static string[] ResolveActiveSecuritySchemes(RequestOptions requestOptions, params string[][] securityRequirementOptions)
    {
        if (securityRequirementOptions is null || securityRequirementOptions.Length == 0)
        {
            return Array.Empty<string>();
        }

        foreach (var requirement in securityRequirementOptions)
        {
            if (requirement.All(schemeId => IsSecuritySchemeConfigured(requestOptions, schemeId)))
            {
                return requirement;
            }
        }

        throw new InvalidOperationException("The supplied request options do not satisfy the authentication requirements for this operation.");
    }

    private static bool IsSecuritySchemeConfigured(RequestOptions requestOptions, string schemeId)
    {
        return schemeId switch
        {
            "ApiKeyAuth" => !string.IsNullOrWhiteSpace(requestOptions.ApiKey),
            _ => false
        };
    }
    public RealtimeWeatherResponse RealtimeWeather(RequestOptions requestOptions, string q, string lang = "", bool includeLang = false)
    {
        var activeSecuritySchemes = ResolveActiveSecuritySchemes(requestOptions, new[] { "ApiKeyAuth" });
        var client = CreateClient(requestOptions, activeSecuritySchemes);
        var requestBuilder = client.CurrentJson;
        var response = requestBuilder.GetAsCurrentGetResponseAsync(config =>
        {
            config.QueryParameters.Q = q;
            if (includeLang)
            {
                config.QueryParameters.Lang = lang;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<RealtimeWeatherResponse>(response)!;
    }

}

[OSInterface(Description = "Generated OutSystems wrapper for Weather API.", Name = "WeatherApiActionsGenerated", IconResourceName = "WeatherApiKiota.resources.ProjectIcon.png")]
public interface IWeatherApiActionsGenerated
{
    [OSAction(Description = "Realtime API", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    RealtimeWeatherResponse RealtimeWeather([OSParameter(Description = "Runtime request configuration including base URL and authentication.")] RequestOptions requestOptions, [OSParameter(Description = "Pass US Zipcode, UK Postcode, Canada Postalcode, IP address, Latitude/Longitude (decimal degree) or city name. Visit [request parameter section](https://www.weatherapi.com/docs/#intro-request) to learn more.")] string q, [OSParameter(Description = "Returns 'condition:text' field in API in the desired language.<br /> Visit [request parameter section](https://www.weatherapi.com/docs/#intro-request) to check 'lang-code'.")] string lang = "", [OSParameter(Description = "Set to true to send the lang query parameter to the API.")] bool includeLang = false);

}

#nullable enable
using OutSystems.ExternalLibraries.SDK;

namespace WeatherApiKiota.Generated;

[OSStructure(Description = "Represents the location schema from the source OpenAPI document.", OriginalName = "location")]
public struct Location
{
    [OSStructureField(Description = "Maps the 'name' field.", OriginalName = "name")]
    public string? Name { get; set; }

    [OSStructureField(Description = "Maps the 'region' field.", OriginalName = "region")]
    public string? Region { get; set; }

    [OSStructureField(Description = "Maps the 'country' field.", OriginalName = "country")]
    public string? Country { get; set; }

    [OSStructureField(Description = "Maps the 'lat' field.", OriginalName = "lat")]
    public decimal? Lat { get; set; }

    [OSStructureField(Description = "Maps the 'lon' field.", OriginalName = "lon")]
    public decimal? Lon { get; set; }

    [OSStructureField(Description = "Maps the 'tz_id' field.", OriginalName = "tz_id")]
    public string? TzId { get; set; }

    [OSStructureField(Description = "Maps the 'localtime_epoch' field.", OriginalName = "localtime_epoch")]
    public int? LocaltimeEpoch { get; set; }

    [OSStructureField(Description = "Maps the 'localtime' field.", OriginalName = "localtime")]
    public string? Localtime { get; set; }

}

[OSStructure(Description = "Represents the current schema from the source OpenAPI document.", OriginalName = "current")]
public struct Current
{
    [OSStructureField(Description = "Maps the 'last_updated_epoch' field.", OriginalName = "last_updated_epoch")]
    public int? LastUpdatedEpoch { get; set; }

    [OSStructureField(Description = "Maps the 'last_updated' field.", OriginalName = "last_updated")]
    public string? LastUpdated { get; set; }

    [OSStructureField(Description = "Maps the 'temp_c' field.", OriginalName = "temp_c")]
    public decimal? TempC { get; set; }

    [OSStructureField(Description = "Maps the 'temp_f' field.", OriginalName = "temp_f")]
    public decimal? TempF { get; set; }

    [OSStructureField(Description = "Maps the 'is_day' field.", OriginalName = "is_day")]
    public int? IsDay { get; set; }

    [OSStructureField(Description = "Maps the 'condition' field.", OriginalName = "condition")]
    public CurrentCondition? Condition { get; set; }

    [OSStructureField(Description = "Maps the 'wind_mph' field.", OriginalName = "wind_mph")]
    public decimal? WindMph { get; set; }

    [OSStructureField(Description = "Maps the 'wind_kph' field.", OriginalName = "wind_kph")]
    public decimal? WindKph { get; set; }

    [OSStructureField(Description = "Maps the 'wind_degree' field.", OriginalName = "wind_degree")]
    public decimal? WindDegree { get; set; }

    [OSStructureField(Description = "Maps the 'wind_dir' field.", OriginalName = "wind_dir")]
    public string? WindDir { get; set; }

    [OSStructureField(Description = "Maps the 'pressure_mb' field.", OriginalName = "pressure_mb")]
    public decimal? PressureMb { get; set; }

    [OSStructureField(Description = "Maps the 'pressure_in' field.", OriginalName = "pressure_in")]
    public decimal? PressureIn { get; set; }

    [OSStructureField(Description = "Maps the 'precip_mm' field.", OriginalName = "precip_mm")]
    public decimal? PrecipMm { get; set; }

    [OSStructureField(Description = "Maps the 'precip_in' field.", OriginalName = "precip_in")]
    public decimal? PrecipIn { get; set; }

    [OSStructureField(Description = "Maps the 'humidity' field.", OriginalName = "humidity")]
    public decimal? Humidity { get; set; }

    [OSStructureField(Description = "Maps the 'cloud' field.", OriginalName = "cloud")]
    public decimal? Cloud { get; set; }

    [OSStructureField(Description = "Maps the 'feelslike_c' field.", OriginalName = "feelslike_c")]
    public decimal? FeelslikeC { get; set; }

    [OSStructureField(Description = "Maps the 'feelslike_f' field.", OriginalName = "feelslike_f")]
    public decimal? FeelslikeF { get; set; }

    [OSStructureField(Description = "Maps the 'vis_km' field.", OriginalName = "vis_km")]
    public decimal? VisKm { get; set; }

    [OSStructureField(Description = "Maps the 'vis_miles' field.", OriginalName = "vis_miles")]
    public decimal? VisMiles { get; set; }

    [OSStructureField(Description = "Maps the 'uv' field.", OriginalName = "uv")]
    public decimal? Uv { get; set; }

    [OSStructureField(Description = "Maps the 'gust_mph' field.", OriginalName = "gust_mph")]
    public decimal? GustMph { get; set; }

    [OSStructureField(Description = "Maps the 'gust_kph' field.", OriginalName = "gust_kph")]
    public decimal? GustKph { get; set; }

    [OSStructureField(Description = "Maps the 'air_quality' field.", OriginalName = "air_quality")]
    public CurrentAirQuality? AirQuality { get; set; }

}

[OSStructure(Description = "Represents the response body for RealtimeWeather.")]
public struct RealtimeWeatherResponse
{
    [OSStructureField(Description = "Maps the 'location' field.", OriginalName = "location")]
    public Location? Location { get; set; }

    [OSStructureField(Description = "Maps the 'current' field.", OriginalName = "current")]
    public Current? Current { get; set; }

}

[OSStructure(Description = "Represents the condition field on current.", OriginalName = "current_condition")]
public struct CurrentCondition
{
    [OSStructureField(Description = "Maps the 'text' field.", OriginalName = "text")]
    public string? Text { get; set; }

    [OSStructureField(Description = "Maps the 'icon' field.", OriginalName = "icon")]
    public string? Icon { get; set; }

    [OSStructureField(Description = "Maps the 'code' field.", OriginalName = "code")]
    public int? Code { get; set; }

}

[OSStructure(Description = "Represents the air_quality field on current.", OriginalName = "current_air_quality")]
public struct CurrentAirQuality
{
    [OSStructureField(Description = "Maps the 'co' field.", OriginalName = "co")]
    public decimal? Co { get; set; }

    [OSStructureField(Description = "Maps the 'no2' field.", OriginalName = "no2")]
    public decimal? No2 { get; set; }

    [OSStructureField(Description = "Maps the 'o3' field.", OriginalName = "o3")]
    public decimal? O3 { get; set; }

    [OSStructureField(Description = "Maps the 'so2' field.", OriginalName = "so2")]
    public decimal? So2 { get; set; }

    [OSStructureField(Description = "Maps the 'pm2_5' field.", OriginalName = "pm2_5")]
    public decimal? Pm25 { get; set; }

    [OSStructureField(Description = "Maps the 'pm10' field.", OriginalName = "pm10")]
    public decimal? Pm10 { get; set; }

    [OSStructureField(Description = "Maps the 'us-epa-index' field.", OriginalName = "us-epa-index")]
    public int? UsEpaIndex { get; set; }

    [OSStructureField(Description = "Maps the 'gb-defra-index' field.", OriginalName = "gb-defra-index")]
    public int? GbDefraIndex { get; set; }

}

[OSStructure(Description = "Runtime request configuration including base URL and authentication.")]
public struct RequestOptions
{
    [OSStructureField(Description = "Overrides the default API base URL when provided.")]
    public string? BaseUrl { get; set; }

    [OSStructureField(Description = "API key for security scheme 'ApiKeyAuth'.")]
    public string? ApiKey { get; set; }

}

using System.Text.Json;

namespace KiotaOutSystems.Generator;

internal static class GeneratorConfigLoader
{
    public static EffectiveGeneratorConfig Load(string? configPath)
    {
        if (string.IsNullOrWhiteSpace(configPath))
        {
            return new EffectiveGeneratorConfig();
        }

        var fullPath = Path.GetFullPath(configPath);
        var json = File.ReadAllText(fullPath);
        var config = JsonSerializer.Deserialize<GeneratorConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new GeneratorConfig();

        return new EffectiveGeneratorConfig
        {
            Namespace = config.Namespace,
            ClientNamespace = config.ClientNamespace,
            ClientClassName = config.ClientClassName,
            InterfaceName = config.InterfaceName,
            ClassName = config.ClassName,
            InputName = config.InputName,
            EmitInterface = config.EmitInterface,
            Icon = config.Icon,
            ActionOverrides = Normalize(config.ActionOverrides),
            StructureOverrides = Normalize(config.StructureOverrides),
            StructureFieldOverrides = NormalizeNested(config.StructureFieldOverrides)
        };
    }

    private static IReadOnlyDictionary<string, NameDescriptionOverride> Normalize(Dictionary<string, NameDescriptionOverride>? source)
    {
        return source is null
            ? new Dictionary<string, NameDescriptionOverride>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, NameDescriptionOverride>(source, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, NameDescriptionOverride>> NormalizeNested(
        Dictionary<string, Dictionary<string, NameDescriptionOverride>>? source)
    {
        if (source is null)
        {
            return new Dictionary<string, IReadOnlyDictionary<string, NameDescriptionOverride>>(StringComparer.OrdinalIgnoreCase);
        }

        return source.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyDictionary<string, NameDescriptionOverride>)new Dictionary<string, NameDescriptionOverride>(pair.Value, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }
}

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
            Targets = NormalizeTargets(config.Targets),
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

    private static IReadOnlyList<EffectiveTargetConfig> NormalizeTargets(List<TargetConfig>? source)
    {
        if (source is null || source.Count == 0)
        {
            return [];
        }

        return source.Select((target, index) =>
        {
            var rawPath = target.Path?.Trim();
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                throw new InvalidOperationException($"Target at index {index} must declare a non-empty path.");
            }

            if (rawPath.IndexOf('*') >= 0 && !rawPath.EndsWith('*'))
            {
                throw new InvalidOperationException(
                    $"Target path '{rawPath}' is invalid. Only a trailing '*' prefix wildcard is supported.");
            }

            var isPrefixMatch = rawPath.EndsWith('*');
            var normalizedPath = isPrefixMatch ? rawPath[..^1] : rawPath;
            var normalizedMethods = target.Methods is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(
                    target.Methods
                        .Where(method => !string.IsNullOrWhiteSpace(method))
                        .Select(method => method.Trim().ToUpperInvariant()),
                    StringComparer.OrdinalIgnoreCase);

            return new EffectiveTargetConfig
            {
                PathPattern = normalizedPath,
                IsPrefixMatch = isPrefixMatch,
                Methods = normalizedMethods
            };
        }).ToArray();
    }
}

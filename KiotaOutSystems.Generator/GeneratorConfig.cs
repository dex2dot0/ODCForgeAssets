using System.Text.Json.Serialization;

namespace KiotaOutSystems.Generator;

internal sealed class GeneratorConfig
{
    public string? Namespace { get; init; }

    public string? ClientNamespace { get; init; }

    public string? ClientClassName { get; init; }

    public string? InterfaceName { get; init; }

    public string? ClassName { get; init; }

    public string? InputName { get; init; }

    public bool? EmitInterface { get; init; }

    public IconConfig? Icon { get; init; }

    public Dictionary<string, NameDescriptionOverride>? ActionOverrides { get; init; }

    public Dictionary<string, NameDescriptionOverride>? StructureOverrides { get; init; }

    public Dictionary<string, Dictionary<string, NameDescriptionOverride>>? StructureFieldOverrides { get; init; }
}

internal sealed class NameDescriptionOverride
{
    public string? Name { get; init; }

    public string? Description { get; init; }
}

internal sealed class IconConfig
{
    public string? FileName { get; init; }

    public string? ResourceName { get; init; }
}

internal sealed class EffectiveGeneratorConfig
{
    public string? Namespace { get; init; }

    public string? ClientNamespace { get; init; }

    public string? ClientClassName { get; init; }

    public string? InterfaceName { get; init; }

    public string? ClassName { get; init; }

    public string? InputName { get; init; }

    public bool? EmitInterface { get; init; }

    public IconConfig? Icon { get; init; }

    public IReadOnlyDictionary<string, NameDescriptionOverride> ActionOverrides { get; init; } = new Dictionary<string, NameDescriptionOverride>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, NameDescriptionOverride> StructureOverrides { get; init; } = new Dictionary<string, NameDescriptionOverride>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, NameDescriptionOverride>> StructureFieldOverrides { get; init; } =
        new Dictionary<string, IReadOnlyDictionary<string, NameDescriptionOverride>>(StringComparer.OrdinalIgnoreCase);
}

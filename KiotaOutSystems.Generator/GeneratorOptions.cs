namespace KiotaOutSystems.Generator;

internal sealed class GeneratorOptions
{
    public required string SpecSource { get; init; }

    public required string OutputDirectory { get; init; }

    public string? Namespace { get; init; }

    public string? ClientNamespace { get; init; }

    public string? ClientClassName { get; init; }

    public string? InterfaceName { get; init; }

    public string? ClassName { get; init; }

    public string? KiotaLockPath { get; init; }

    public string? InputName { get; init; }

    public bool? EmitInterface { get; init; }

    public string? ConfigPath { get; init; }
}

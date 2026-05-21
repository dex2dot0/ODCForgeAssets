using KiotaOutSystems.Generator;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
{
    PrintHelp();
    return;
}

var options = ParseArguments(args);
var loadedSpec = await SpecSourceLoader.LoadAsync(options.SpecSource, CancellationToken.None).ConfigureAwait(false);
var result = OpenApiToOutSystemsGenerator.Generate(options, loadedSpec);

Console.WriteLine($"Generated {result.Files.Count} files for {result.OperationCount} operations and {result.StructureCount} structures.");
foreach (var file in result.Files)
{
    Console.WriteLine($" - {file}");
}

return;

static GeneratorOptions ParseArguments(string[] args)
{
    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var index = 0; index < args.Length; index++)
    {
        var current = args[index];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unexpected argument '{current}'.");
        }

        if (index == args.Length - 1 || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Missing value for argument '{current}'.");
        }

        values[current] = args[++index];
    }

    if (!values.TryGetValue("--spec", out var specSource))
    {
        throw new InvalidOperationException("Missing required argument --spec.");
    }

    if (!values.TryGetValue("--output", out var outputDirectory))
    {
        throw new InvalidOperationException("Missing required argument --output.");
    }

    values.TryGetValue("--namespace", out var generationNamespace);
    values.TryGetValue("--client-namespace", out var clientNamespace);
    values.TryGetValue("--client-class", out var clientClassName);
    values.TryGetValue("--interface-name", out var interfaceName);
    values.TryGetValue("--class-name", out var className);
    values.TryGetValue("--kiota-lock", out var kiotaLockPath);
    values.TryGetValue("--input-name", out var inputName);
    values.TryGetValue("--config", out var configPath);
    var emitInterface = ParseBoolean(values, "--emit-interface");

    return new GeneratorOptions
    {
        SpecSource = specSource,
        OutputDirectory = outputDirectory,
        Namespace = generationNamespace,
        ClientNamespace = clientNamespace,
        ClientClassName = clientClassName,
        InterfaceName = interfaceName,
        ClassName = className,
        KiotaLockPath = kiotaLockPath,
        InputName = inputName,
        EmitInterface = emitInterface,
        ConfigPath = configPath
    };
}

static bool? ParseBoolean(IReadOnlyDictionary<string, string> values, string key)
{
    if (!values.TryGetValue(key, out var rawValue))
    {
        return null;
    }

    if (bool.TryParse(rawValue, out var parsedValue))
    {
        return parsedValue;
    }

    throw new InvalidOperationException($"Argument '{key}' must be either 'true' or 'false'.");
}

static void PrintHelp()
{
    Console.WriteLine("""
KiotaOutSystems.Generator

Required arguments:
  --spec <path-or-url>      OpenAPI source file path or HTTP/HTTPS URL.
  --output <directory>      Output directory for generated files.

Optional arguments:
  --config <path>           Optional generator config file with naming/description overrides.
  --namespace <namespace>   Namespace for generated wrapper code.
  --client-namespace <ns>   Kiota client namespace (optional when --kiota-lock is provided).
  --client-class <name>     Kiota client class name (optional when --kiota-lock is provided).
  --kiota-lock <path>       Path to kiota-lock.json to bind generated wrappers to a Kiota client.
  --interface-name <name>   Interface name for generated OutSystems actions.
  --class-name <name>       Class name for generated OutSystems actions.
  --input-name <name>       Friendly API/library name used in generated descriptions.
  --emit-interface <bool>   Emit the OutSystems OSInterface and generated interface. Default: true.
""");
}

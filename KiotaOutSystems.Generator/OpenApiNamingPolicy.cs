using System.Security.Cryptography;
using System.Text;

namespace KiotaOutSystems.Generator;

internal static class OpenApiNamingPolicy
{
    public const int OutSystemsMaxNameLength = 50;

    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
        "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while"
    };

    public static string CreateStructureName(string sourceName)
    {
        return EnsureSafeIdentifier(ToPascalCase(sourceName), maxLength: OutSystemsMaxNameLength);
    }

    public static string CreateInternalStructureName(string sourceName)
    {
        return EnsureSafeIdentifier(ToPascalCase(sourceName));
    }

    public static string CreatePropertyName(string sourceName, string? enclosingTypeName = null)
    {
        return EnsureSafeIdentifier(ToPascalCase(sourceName), enclosingTypeName, OutSystemsMaxNameLength);
    }

    public static string CreateParameterName(string sourceName)
    {
        var pascalName = ToPascalCase(sourceName);
        if (string.IsNullOrEmpty(pascalName))
        {
            return "_";
        }

        var camelName = char.ToLowerInvariant(pascalName[0]) + pascalName[1..];
        return EnsureSafeIdentifier(camelName, maxLength: OutSystemsMaxNameLength);
    }

    public static string CreatePresenceFlagName(string sourceName)
    {
        return EnsureSafeIdentifier($"include{ToPascalCase(sourceName)}", maxLength: OutSystemsMaxNameLength);
    }

    public static string CreateQueryPropertyName(string sourceName)
    {
        return EnsureSafeIdentifier(ToPascalCase(sourceName));
    }

    public static string ConstrainOutSystemsName(string candidate)
    {
        return EnsureSafeIdentifier(candidate, maxLength: OutSystemsMaxNameLength);
    }

    public static string CreateKiotaModelTypeName(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return sourceName;
        }

        return EnsureSafeIdentifier(ToPascalCase(sourceName));
    }

    public static string CreateKiotaPathSegmentName(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return sourceName;
        }

        if (sourceName.Contains('_', StringComparison.Ordinal) &&
            !sourceName.Contains('-', StringComparison.Ordinal) &&
            !sourceName.Contains('.', StringComparison.Ordinal) &&
            !sourceName.Contains('/', StringComparison.Ordinal) &&
            !sourceName.Contains(' ', StringComparison.Ordinal) &&
            !sourceName.Contains(':', StringComparison.Ordinal))
        {
            return EnsureSafeIdentifier(char.ToUpperInvariant(sourceName[0]) + sourceName[1..]);
        }

        return CreateKiotaModelTypeName(sourceName);
    }

    public static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "_";
        }

        var parts = value
            .Split(['-', '_', ' ', '.', '/', ':'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Length == 0 ? string.Empty : char.ToUpperInvariant(part[0]) + part[1..]);

        var combined = string.Concat(parts);
        return string.IsNullOrEmpty(combined) ? "_" : combined;
    }

    public static string SanitizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "_";
        }

        var builder = new System.Text.StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return EnsureSafeIdentifier(builder.ToString());
    }

    private static string EnsureSafeIdentifier(string candidate, string? enclosingTypeName = null, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "Value";
        }

        var builder = new StringBuilder(candidate.Length + 8);
        foreach (var character in candidate)
        {
            builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        }

        var sanitized = builder.ToString().Trim('_');
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Value";
        }

        if (!char.IsLetter(sanitized[0]))
        {
            sanitized = $"Value{sanitized}";
        }

        if (CSharpKeywords.Contains(sanitized))
        {
            sanitized += "Value";
        }

        if (!string.IsNullOrWhiteSpace(enclosingTypeName) &&
            string.Equals(sanitized, enclosingTypeName, StringComparison.Ordinal))
        {
            sanitized += "Value";
        }

        if (maxLength is { } boundedLength && sanitized.Length > boundedLength)
        {
            sanitized = ShortenIdentifier(sanitized, boundedLength);
        }

        return sanitized;
    }

    private static string ShortenIdentifier(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        const int hashLength = 8;
        var hash = ComputeStableHash(value)[..hashLength];
        var prefixLength = Math.Max(1, maxLength - hashLength - 1);
        return $"{value[..prefixLength]}_{hash}";
    }

    private static string ComputeStableHash(string value)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

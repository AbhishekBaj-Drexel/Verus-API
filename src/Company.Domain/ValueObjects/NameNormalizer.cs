namespace Company.Domain.ValueObjects;

using System.Text.RegularExpressions;

public static partial class NameNormalizer
{
    public static IReadOnlyList<string> Normalize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var lowercase = name.ToLowerInvariant();
        var withoutPunctuation = PunctuationRegex().Replace(lowercase, " ");

        return withoutPunctuation
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    [GeneratedRegex("[^\\p{L}\\p{N}\\s]+")]
    private static partial Regex PunctuationRegex();
}

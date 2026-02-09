namespace Company.Application.Validation;

using global::Company.Domain.ValueObjects;

public sealed class TokenBasedCompanyRelevanceEvaluator : ICompanyRelevanceEvaluator
{
    private const double TokenMatchWeight = 0.7;
    private const double CompoundAbbreviationMatchWeight = 0.7;
    private const double FirstTokenMatchWeight = 0.2;
    private const double FullNameHostContainsWeight = 0.1;
    private const double RelevantThreshold = 0.7;

    private static readonly HashSet<string> NonMeaningfulTokens = new(StringComparer.Ordinal)
    {
        "www",
        "com",
        "net",
        "org",
        "co",
        "io",
        "ai",
        "inc",
        "llc",
        "ltd",
        "corp",
        "company",
    };

    public Task<RelevanceEvaluation> EvaluateAsync(string companyName, Uri websiteUrl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name is required.", nameof(companyName));
        }

        ArgumentNullException.ThrowIfNull(websiteUrl);

        var allCompanyTokens = NameNormalizer.Normalize(companyName);
        var meaningfulCompanyTokens = allCompanyTokens.Where(IsMeaningfulToken).ToArray();

        var domain = DomainParser.GetRegistrableDomain(websiteUrl);
        var domainTokens = domain
            .Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsMeaningfulToken)
            .ToHashSet(StringComparer.Ordinal);

        var reasons = new List<string>();
        var score = 0d;

        var matchedToken = meaningfulCompanyTokens.FirstOrDefault(domainTokens.Contains);
        if (!string.IsNullOrWhiteSpace(matchedToken))
        {
            score += TokenMatchWeight;
            reasons.Add($"Token match: {matchedToken}");
        }

        var compoundAbbreviationMatch = FindCompoundAbbreviationMatch(domainTokens, meaningfulCompanyTokens);
        if (!string.IsNullOrWhiteSpace(compoundAbbreviationMatch))
        {
            score += CompoundAbbreviationMatchWeight;
            reasons.Add($"Compound abbreviation match: {compoundAbbreviationMatch}");
        }

        if (meaningfulCompanyTokens.Length > 0 && domainTokens.Contains(meaningfulCompanyTokens[0]))
        {
            score += FirstTokenMatchWeight;
            reasons.Add("Prefix match: first company token appears in domain.");
        }

        var normalizedCompanyName = string.Concat(allCompanyTokens);
        var normalizedHost = NormalizeAlphaNumeric(websiteUrl.Host);

        if (!string.IsNullOrWhiteSpace(normalizedCompanyName) &&
            normalizedHost.Contains(normalizedCompanyName, StringComparison.Ordinal))
        {
            score += FullNameHostContainsWeight;
            reasons.Add("Full normalized company name appears in website host.");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("No meaningful token overlap between company name and website domain.");
        }

        score = Math.Min(1d, score);

        return Task.FromResult(new RelevanceEvaluation(
            IsRelevant: score >= RelevantThreshold,
            Score: score,
            Reasons: reasons));
    }

    private static bool IsMeaningfulToken(string token)
    {
        return token.Length >= 3 && !NonMeaningfulTokens.Contains(token);
    }

    private static string? FindCompoundAbbreviationMatch(
        IReadOnlyCollection<string> domainTokens,
        IReadOnlyList<string> companyTokens)
    {
        if (companyTokens.Count < 2)
        {
            return null;
        }

        var firstToken = companyTokens[0];
        if (firstToken.Length < 3)
        {
            return null;
        }

        foreach (var domainToken in domainTokens)
        {
            if (!domainToken.StartsWith(firstToken, StringComparison.Ordinal))
            {
                continue;
            }

            var remainder = domainToken[firstToken.Length..];
            if (remainder.Length < 2)
            {
                continue;
            }

            var matchesAnyFollowingToken = companyTokens
                .Skip(1)
                .Any(token => token.StartsWith(remainder, StringComparison.Ordinal));

            if (matchesAnyFollowingToken)
            {
                return domainToken;
            }
        }

        return null;
    }

    private static string NormalizeAlphaNumeric(string input)
    {
        return new string(input
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }
}

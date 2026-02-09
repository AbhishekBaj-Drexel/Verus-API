namespace Company.UnitTests.Application;

using Company.Application.Validation;

public sealed class TokenBasedCompanyRelevanceEvaluatorTests
{
    private readonly TokenBasedCompanyRelevanceEvaluator _evaluator = new();

    [Fact]
    public async Task EvaluateAsync_WhenNameMatchesDomainToken_ReturnsRelevant()
    {
        var result = await _evaluator.EvaluateAsync(
            "Example",
            new Uri("https://example.com"),
            CancellationToken.None);

        Assert.True(result.IsRelevant);
        Assert.True(result.Score >= 0.7);
        Assert.Contains(result.Reasons, reason => reason.Contains("Token match", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoTokenMatch_ReturnsNotRelevant()
    {
        var result = await _evaluator.EvaluateAsync(
            "Blue Ocean",
            new Uri("https://example.com"),
            CancellationToken.None);

        Assert.False(result.IsRelevant);
        Assert.True(result.Score < 0.7);
        Assert.Contains(result.Reasons, reason => reason.Contains("No meaningful token overlap", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EvaluateAsync_WithPunctuationAndLegalSuffixes_IgnoresNoiseAndFindsRelevantToken()
    {
        var result = await _evaluator.EvaluateAsync(
            "Example, Inc. LLC",
            new Uri("https://example.com"),
            CancellationToken.None);

        Assert.True(result.IsRelevant);
        Assert.Contains(result.Reasons, reason => reason.Contains("Token match: example", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EvaluateAsync_WithCompoundAbbreviationDomain_ReturnsRelevant()
    {
        var result = await _evaluator.EvaluateAsync(
            "First American",
            new Uri("https://www.firstam.com"),
            CancellationToken.None);

        Assert.True(result.IsRelevant);
        Assert.True(result.Score >= 0.7);
        Assert.Contains(result.Reasons, reason => reason.Contains("Compound abbreviation match", StringComparison.Ordinal));
    }
}
